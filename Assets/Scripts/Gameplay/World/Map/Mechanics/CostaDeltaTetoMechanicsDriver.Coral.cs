using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Reacender o coral of Kirta (28, <see cref="KirtaCoralRules"/>): the five dimmed corals round the atoll are grey pillars with a spark
    /// caught in them; [E] beside one frees the spark and the coral is lit for good (a time mark in the save, written only then). A lit coral
    /// is a coral-pink pillar with its mending ring, brighter at night: inside it, out of combat, it gives life back. The cousin says a line
    /// the first time a coral lights, Kirta one when all five are lit. No damage, never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const float CoralViewRange = 140f;
        private const float CoralTickSeconds = 0.5f;
        private const float CoralSayCooldown = 40f;
        private const int CoralKeys = 500;

        private static readonly Color CoralDimColor = new Color(0.6f, 0.58f, 0.56f, 0.9f);
        private static readonly Color CoralLitColor = new Color(0.91f, 0.51f, 0.42f, 0.9f);
        private static readonly Color CoralNightColor = new Color(1f, 0.66f, 0.55f, 1f);
        private static readonly Color CoralSparkColor = new Color(0.55f, 0.95f, 0.95f, 1f);

        private readonly HeroCombatClock _coralCombat = new HeroCombatClock();
        private LineRenderer[] _coralLines;
        private bool[] _coralLit;
        private bool _inCorals;
        private bool _coralMendSaid;
        private float _coralTick;
        private float _nextCoralSay;

        /// <summary>
        /// The corals hear the hero's fights (out of combat they mend) while the driver is on; the Ninki Nanka's own glow (Ninki partial) and the
        /// freed Pescador de Luzes (FisherWisps partial) are heard from the same moment.
        /// </summary>
        private void OnEnable()
        {
            _coralCombat.Attach();
            HookDragonGaze();
            HookFisherWisps();
        }

        private void TickCorals(Vector2 local)
        {
            TickMotherCoral(local);
            KirtaCoral[] corals = OverworldLayouts.KirtaCorals;
            int near = KirtaCoralRules.Nearest(corals, local, CoralViewRange);
            if (near < 0)
            {
                LeaveCorals();
                return;
            }

            WorldMapProgress progress = Progress();
            if (!_inCorals || _coralLit == null)
            {
                _inCorals = true;
                _coralLit ??= new bool[corals.Length];
                for (int i = 0; i < corals.Length; i++)
                {
                    _coralLit[i] = progress != null ? KirtaCoralRules.IsLit(progress.GetTimeMark(KirtaCoralRules.MarkKey(corals[i].Id))) : _coralLit[i];
                }
            }

            _coralCombat.Track(_player.gameObject);
            ShowCorals(local, corals);
            TickCoralSpark(local, corals, progress);
            TickCoralMend(local, corals);
        }

        private void TickCoralSpark(Vector2 local, KirtaCoral[] corals, WorldMapProgress progress)
        {
            int coral = KirtaCoralRules.Nearest(corals, local, KirtaCoralRules.SparkReach);
            if (coral < 0 || _controller.Input == null)
            {
                return;
            }

            KirtaCoralCheck check = KirtaCoralRules.Check(_coralLit[coral], Vector2.Distance(local, corals[coral].Position), _stats.IsAlive);
            if (check != KirtaCoralCheck.Relight)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayCoral(KirtaCoralTexts.Prompt);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            bool first = KirtaCoralRules.LitCount(_coralLit) == 0;
            _coralLit[coral] = true;
            progress?.SetTimeMark(KirtaCoralRules.MarkKey(corals[coral].Id), Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            _nextHint = Time.unscaledTime + HintCooldown;
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(string.Format(KirtaCoralTexts.Get(localization, KirtaCoralTexts.Relit), KirtaCoralRules.LitCount(_coralLit), KirtaCoralRules.CoralCount));
            if (first)
            {
                SayCoral(KirtaCoralTexts.LineFor(MapRequirementHeroState.Capture(null).Hero), -0.5f);
            }

            if (KirtaCoralRules.AllLit(_coralLit) && (progress == null || progress.GetTimeMark(KirtaCoralRules.AllThankedKey) <= 0L))
            {
                progress?.SetTimeMark(KirtaCoralRules.AllThankedKey, 1L);
                SayCoral(KirtaCoralTexts.AllLit, -1f);
            }
        }

        private void TickCoralMend(Vector2 local, KirtaCoral[] corals)
        {
            bool inRing = KirtaCoralRules.MendingCoral(corals, _coralLit, local) >= 0;
            if (!inRing)
            {
                _coralTick = 0f;
                _coralMendSaid = false;
                return;
            }

            bool hurt = _stats.CurrentHealth < _stats.MaxHealth;
            if (!KirtaCoralRules.Mends(true, _stats.IsAlive, _coralCombat.SecondsSinceCombat))
            {
                _coralTick = 0f;
                if (hurt && _stats.IsAlive && Time.unscaledTime >= _nextCoralSay)
                {
                    _nextCoralSay = Time.unscaledTime + CoralSayCooldown;
                    SayCoral(KirtaCoralTexts.Waits);
                }

                return;
            }

            _coralTick += Time.deltaTime;
            if (_coralTick < CoralTickSeconds)
            {
                return;
            }

            float seconds = _coralTick;
            _coralTick = 0f;
            if (!hurt)
            {
                return;
            }

            if (!_coralMendSaid)
            {
                _coralMendSaid = true;
                SayCoral(KirtaCoralTexts.Mends);
            }

            float heal = KirtaCoralRules.MendOver(_stats.CurrentHealth, _stats.MaxHealth, seconds);
            if (heal > 0f)
            {
                _stats.Heal(heal);
            }
        }

        private void ShowCorals(Vector2 local, KirtaCoral[] corals)
        {
            _coralLines ??= new LineRenderer[corals.Length * 2];
            bool night = IsNight();
            for (int i = 0; i < corals.Length; i++)
            {
                bool near = (corals[i].Position - local).sqrMagnitude <= CoralViewRange * CoralViewRange;
                if (!near)
                {
                    Off(_coralLines[i * 2]);
                    Off(_coralLines[i * 2 + 1]);
                    HideRing(CoralKeys + i);
                    continue;
                }

                Vector2 world = _costaOrigin + corals[i].Position;
                bool lit = _coralLit[i];
                Color body = lit ? (night ? CoralNightColor : CoralLitColor) : CoralDimColor;
                _coralLines[i * 2] = Pillar(_coralLines[i * 2], "Cs_KirtaCoral", world, lit && night ? 1.3f : 1f, 3.2f, body);
                if (lit)
                {
                    Off(_coralLines[i * 2 + 1]);
                    DrawRing(CoralKeys + i, _costaOrigin, corals[i].Position, KirtaCoralRules.MendRadius, body, true, true);
                }
                else
                {
                    // The caught spark: a thin bright stroke inside the grey branches.
                    _coralLines[i * 2 + 1] = Pillar(_coralLines[i * 2 + 1], "Cs_KirtaCoralSpark", world, 0.3f, 1.8f, CoralSparkColor);
                    DrawRing(CoralKeys + i, _costaOrigin, corals[i].Position, KirtaCoralRules.SparkReach, CoralSparkColor, true, true);
                }
            }
        }

        private void LeaveCorals()
        {
            if (!_inCorals)
            {
                return;
            }

            _inCorals = false;
            _coralTick = 0f;
            _coralMendSaid = false;
            Off(_coralLines);
            for (int i = 0; i < KirtaCoralRules.CoralCount; i++)
            {
                HideRing(CoralKeys + i);
            }
        }

        private void SayCoral(string key, float drop = 0f)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), KirtaCoralTexts.Get(localization, key));
        }
    }
}
