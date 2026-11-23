using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The night tremor of the Salão de Tages (20, <see cref="TagesTremorRules"/>) on the lake's driver: at night, every 150 s of the UTC clock,
    /// 5 s of warning ("stand by a lamp") and the hall shakes; every burning lamp the hero is not holding (8 m) loses 90 s of light, written to
    /// its time mark only then (the hall's sigh then blows out the ones shaken past their light). Entering the hall starts listening from the
    /// current tremor, so nothing is replayed. No damage, never takes the controls.
    /// </summary>
    public sealed partial class LimiarLakeMechanicsDriver
    {
        private readonly Vector2[] _tremorHeroes = new Vector2[1];
        private long _tremorSeen = long.MinValue;
        private long _tremorWarned = long.MinValue;
        private bool _tremorLineSaid;

        /// <summary>One frame in the hall (after the lamps' marks are read): warn, then shake when the clock passes a tremor at night.</summary>
        private void TickTremor(Vector2 local, long now)
        {
            long index = TagesTremorRules.TremorIndex(now);
            if (_tremorSeen == long.MinValue)
            {
                _tremorSeen = index;
            }

            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null && clock.Phase == TimeOfDayPhase.Night;
            if (!night)
            {
                _tremorSeen = index;
                return;
            }

            if (TagesTremorRules.PhaseAt(now) == TagesTremorPhase.Warning && _tremorWarned != index && TagesDarkRules.BurningCount(_lit, now) > 0)
            {
                _tremorWarned = index;
                SayTremor(TagesTremorTexts.Warning);
            }

            if (!TagesTremorRules.Due(_tremorSeen, now, true))
            {
                return;
            }

            _tremorSeen = index;
            Shake(local, index * TagesTremorRules.CycleSeconds);
        }

        private void Shake(Vector2 local, long tremorAt)
        {
            _tremorHeroes[0] = local;
            TagesLamp[] lamps = OverworldLayouts.TagesLamps;
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            int shaken = 0;
            bool held = false;
            for (int i = 0; i < lamps.Length; i++)
            {
                if (!TagesDarkRules.Burning(_lit[i], tremorAt))
                {
                    continue;
                }

                bool steadied = _stats.IsAlive && TagesTremorRules.Steadied(lamps[i].At, _tremorHeroes);
                long after = TagesTremorRules.Shaken(_lit[i], tremorAt, steadied);
                if (after == _lit[i])
                {
                    held |= steadied;
                    continue;
                }

                shaken++;
                _lit[i] = after;
                _litLocal[i] = after;
                if (saved)
                {
                    progress.SetTimeMark(_litKeys[i], after);
                }
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            if (shaken > 0)
            {
                SayText(TagesTremorTexts.Format(localization, TagesTremorTexts.Shook, shaken));
            }
            else if (!held)
            {
                SayTremor(TagesTremorTexts.Quiet);
            }

            if (held)
            {
                SayTremor(TagesTremorTexts.Held, shaken > 0 ? -0.5f : 0f);
                if (!_tremorLineSaid)
                {
                    _tremorLineSaid = true;
                    CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
                    SayTremor(TagesTremorTexts.LineFor(hero), 0.7f);
                }
            }
        }

        private void LeaveTremor()
        {
            _tremorSeen = long.MinValue;
            _tremorLineSaid = false;
        }

        private void SayTremor(string key, float drop = 0f)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), TagesTremorTexts.Get(localization, key));
        }
    }
}
