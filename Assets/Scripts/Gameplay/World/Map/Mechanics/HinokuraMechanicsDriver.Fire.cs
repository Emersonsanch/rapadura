using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// 70 · two more fires on the caldera's heat meter (<see cref="CalderaFireRules"/>): at night (the game clock) the lava lake creeps 8 m up its
    /// shore, and the living Glutão de Brasa is a heat source of his own (24 m), weaker with each forge lit in Hinokura. Said once per night on the
    /// shore and once per fight; the cousin's line the first time the Glutão's heat is felt is one mark in the save, on that event.
    /// </summary>
    public sealed partial class HinokuraMechanicsDriver
    {
        private bool _saidLavaRise;
        private bool _saidGluttonHeat;

        private static bool CalderaNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        /// <summary>The distance to the lava's edge the meter uses tonight (says it once when the night's lake is felt).</summary>
        private float CalderaLakeDistance(Vector2 local)
        {
            float raw = RioFireRules.DistanceToFire(OverworldLayouts.CaldeiraLake, local);
            bool night = CalderaNight();
            float distance = CalderaFireRules.LakeDistance(raw, night);
            if (!night)
            {
                _saidLavaRise = false;
            }
            else if (!_saidLavaRise && distance < HinokuraHeatRules.HeatRange)
            {
                _saidLavaRise = true;
                SayFire(PeaksSteppeRound2Texts.Get(Localization(), PeaksSteppeRound2Texts.LavaRises));
            }

            return distance;
        }

        /// <summary>Points a second the living Glutão gives the hero now (0 away from his arena or when he is not up).</summary>
        private float GluttonHeat(Vector2 local)
        {
            bool nearArena = (local - OverworldLayouts.GlutaoArena).sqrMagnitude < GluttonHintRange * GluttonHintRange;
            EnemyController glutton = nearArena ? FindGlutton() : null;
            if (glutton == null)
            {
                _saidGluttonHeat = false;
                return 0f;
            }

            Vector3 at = glutton.transform.position;
            float distance = Vector2.Distance(local + _origin, new Vector2(at.x, at.z));
            int lit = HinokuraForgeRules.LitCount(_litAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            float gain = CalderaFireRules.GluttonGain(distance, lit);
            if (gain > 0f && !_saidGluttonHeat)
            {
                _saidGluttonHeat = true;
                LocalizationManager localization = Localization();
                SayFire(PeaksSteppeRound2Texts.Get(localization, PeaksSteppeRound2Texts.GluttonHeat));
                if (lit > 0)
                {
                    SayFire(PeaksSteppeRound2Texts.Format(localization, PeaksSteppeRound2Texts.GluttonHeatForges, lit), 0.7f);
                }

                if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(CalderaFireRules.FirstGluttonHeatKey) <= 0L)
                {
                    progress.SetTimeMark(CalderaFireRules.FirstGluttonHeatKey, 1L);
                    SayFire(PeaksSteppeRound2Texts.Get(localization,
                        PeaksSteppeRound2Texts.LineFor(PeaksSteppeRound2Texts.GluttonLinePrefix, MapRequirementHeroState.Capture(null).Hero)), 1.4f);
                }
            }

            return gain;
        }

        private static LocalizationManager Localization()
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return localization;
        }

        private void SayFire(string text, float lift = 0f) =>
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + lift), text);
    }
}
