using System;
using System.Collections.Generic;
using Rapadura.Core.EventBus;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Watches every skill cast performed by its owning caster (via the existing
    /// <see cref="SkillCastEvent"/> published by <see cref="SkillManager.TryCast"/>) and, when the
    /// casts match a <see cref="ComboDefinition"/>'s ordered sequence within its time window,
    /// grants that combo's bonus. Entirely data-driven: no per-combo special-casing lives here,
    /// so designers can add new combos purely as new <see cref="ComboDefinition"/> assets.
    /// Attach alongside <see cref="SkillManager"/> (and optionally <see cref="BuffController"/>
    /// for the DamageMultiplier/CostReduction bonuses).
    /// </summary>
    public class ComboTracker : MonoBehaviour
    {
        [SerializeField] private SkillManager _skillManager;
        [SerializeField] private BuffController _buffController;
        [SerializeField] private ComboDefinition[] _combos = Array.Empty<ComboDefinition>();

        private class ComboProgress
        {
            public int Step;
            public float LastCastTime;
        }

        private readonly Dictionary<ComboDefinition, ComboProgress> _progress = new Dictionary<ComboDefinition, ComboProgress>();

        public IReadOnlyList<ComboDefinition> Combos => _combos;

        private void Awake()
        {
            if (_skillManager == null)
            {
                _skillManager = GetComponent<SkillManager>();
            }

            if (_buffController == null)
            {
                _buffController = GetComponent<BuffController>();
            }
        }

        private void Start()
        {
            EventBus.Subscribe<SkillCastEvent>(HandleSkillCast);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SkillCastEvent>(HandleSkillCast);
        }

        /// <summary>
        /// Processes one skill cast against every tracked combo. Public (rather than only a
        /// private EventBus callback) so tests can drive it deterministically without depending
        /// on <see cref="Time.time"/>/EventBus plumbing.
        /// </summary>
        public void HandleSkillCast(SkillCastEvent evt)
        {
            if (_skillManager != null && evt.Caster != _skillManager.gameObject)
            {
                return;
            }

            float now = Time.time;

            foreach (ComboDefinition combo in _combos)
            {
                if (combo == null || combo.Sequence.Length == 0)
                {
                    continue;
                }

                ProcessCombo(combo, evt.Skill, now);
            }
        }

        private void ProcessCombo(ComboDefinition combo, SkillDefinition castSkill, float now)
        {
            if (!_progress.TryGetValue(combo, out ComboProgress progress))
            {
                progress = new ComboProgress();
                _progress[combo] = progress;
            }

            SkillDefinition expected = combo.Sequence[progress.Step];
            bool withinWindow = progress.Step == 0 || (now - progress.LastCastTime) <= combo.WindowSeconds;

            if (castSkill == expected && withinWindow)
            {
                AdvanceProgress(combo, progress, now);
                return;
            }

            // The sequence broke. If this cast happens to be the combo's first skill, it starts a fresh attempt.
            progress.Step = 0;

            if (castSkill == combo.Sequence[0])
            {
                AdvanceProgress(combo, progress, now);
            }
        }

        private void AdvanceProgress(ComboDefinition combo, ComboProgress progress, float now)
        {
            progress.Step++;
            progress.LastCastTime = now;

            if (progress.Step >= combo.Sequence.Length)
            {
                progress.Step = 0;
                CompleteCombo(combo);
            }
        }

        /// <summary>Resets in-progress tracking for every combo (e.g. on respawn/scene change).</summary>
        public void ResetProgress()
        {
            _progress.Clear();
        }

        private void CompleteCombo(ComboDefinition combo)
        {
            ApplyBonus(combo);
            GameObject caster = _skillManager != null ? _skillManager.gameObject : gameObject;
            EventBus.Publish(new ComboCompletedEvent(caster, combo));
        }

        private void ApplyBonus(ComboDefinition combo)
        {
            switch (combo.BonusType)
            {
                case ComboBonusType.DamageMultiplier:
                    // Multiplies real skill damage, so a plain percent-additive modifier works.
                    ApplyTemporaryBuff(StatType.AttackDamage, ModifierApplication.PercentAdditive, combo);
                    break;

                case ComboBonusType.CostReduction:
                    // SkillManager reads ResourceCostReduction the same way it reads CooldownReduction:
                    // as a flat 0..1 fraction against a base value of 0, so this must be Flat, not PercentAdditive.
                    ApplyTemporaryBuff(StatType.ResourceCostReduction, ModifierApplication.Flat, combo);
                    break;

                case ComboBonusType.UnlockSkill:
                    if (combo.UnlockSkill != null && _skillManager != null)
                    {
                        _skillManager.LearnSkill(combo.UnlockSkill, casterLevel: 1, force: true);
                    }
                    break;
            }
        }

        private void ApplyTemporaryBuff(StatType stat, ModifierApplication application, ComboDefinition combo)
        {
            if (_buffController == null)
            {
                return;
            }

            _buffController.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Combo: " + combo.DisplayName,
                affectedStat = stat,
                application = application,
                value = combo.BonusValue,
                durationSeconds = combo.BonusDurationSeconds,
                isDebuff = false
            }, sourceKey: "combo_" + combo.ComboId);
        }
    }
}
