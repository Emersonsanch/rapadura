using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The abilities of the map bosses of the pits of the Limiar Profundo and of the bridge (Docs/Mapas100.md 84, 85, 87, 88, 89;
    /// <see cref="RegionBossSignature"/> 95–99): what each shows when it warns and what it does when the warning runs out. Rules in
    /// RegionBossRules.DeepBosses.cs.
    /// </summary>
    public sealed partial class RegionBossAbilities
    {
        // The king's copy, the lioness' roars and which way her stripes run, the dragon's breaths and the lamp he left lit.
        private Vector2 _smokeCopy;
        private int _lionessRoars;
        private bool _lionessNorth;
        private int _lampBreaths;
        private Vector2 _lamp;

        /// <summary>Re-armed from the pool: every count starts over.</summary>
        private void ResetDeepMapBosses()
        {
            _lionessRoars = 0;
            _lampBreaths = 0;
        }

        private static bool IsDeepMapBossSignature(RegionBossSignature signature) =>
            signature >= RegionBossSignature.SmokeCopy && signature <= RegionBossSignature.NoveloWave;

        /// <summary>The warning of one of the five: what it marks on the ground, and the line it calls out (left as it is when it has no line of its own).</summary>
        private void WarnDeepMapBoss(RegionBossSignature signature, ref string warning)
        {
            if (!IsDeepMapBossSignature(signature))
            {
                return;
            }

            Vector2 origin = Flat(_signatureOrigin);
            Vector2 mark = Flat(_signatureImpact);
            switch (signature)
            {
                case RegionBossSignature.SmokeCopy:
                {
                    // The copy rises on the hero's spot; halfway back to the king is the line to get past.
                    _smokeCopy = RegionBossRules.SmokeCopySpot(origin, mark);
                    Vector3 copy = new Vector3(_smokeCopy.x, _signatureImpact.y, _smokeCopy.y);
                    Vector2 half = (origin + _smokeCopy) * 0.5f;
                    DungeonShockwave.Spawn(copy, null, _controller.gameObject, 0f, 0f);
                    DungeonShockwave.Spawn(new Vector3(half.x, _signatureOrigin.y, half.y), null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(copy + Vector3.up * 1.2f, Text(RegionBossTexts.SmokeCopyKey));
                    break;
                }

                case RegionBossSignature.BurningStripes:
                {
                    // The hero's stripe lights along its length; the dark stripes either side are labelled.
                    _lionessNorth = RegionBossRules.LionessStripesRunNorth(_lionessRoars++);
                    warning = RegionBossTexts.LionessStripesKey(_lionessNorth);
                    Vector2 along = _lionessNorth ? Vector2.up : Vector2.right;
                    Vector2 across = _lionessNorth ? Vector2.right : Vector2.up;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 hot = mark + along * (RegionBossRules.LionessStripe * 1.5f * i);
                        DungeonShockwave.Spawn(new Vector3(hot.x, _signatureImpact.y, hot.y), null, _controller.gameObject, 0f, 0f);
                        Vector2 dark = mark + across * (RegionBossRules.LionessStripe * i);
                        DungeonFx.Say(new Vector3(dark.x, _signatureImpact.y + 1.2f, dark.y), Text(RegionBossTexts.LionessDarkKey));
                    }

                    break;
                }

                case RegionBossSignature.TailSweep:
                {
                    // The side the tail will sweep: halfway out toward where the hero stood.
                    Vector2 side = mark - origin;
                    Vector2 toward = side.sqrMagnitude > 0.01f ? side.normalized : Vector2.up;
                    Vector2 swept = origin + toward * (RegionBossRules.TailReach * 0.5f);
                    DungeonShockwave.Spawn(new Vector3(swept.x, _signatureOrigin.y, swept.y), null, _controller.gameObject, 0f, 0f);
                    break;
                }

                case RegionBossSignature.LampHaven:
                {
                    // The one lamp he cannot put out: the only light when the dark falls.
                    _lamp = RegionBossRules.LampSpot(_lampBreaths++, origin, mark);
                    Vector3 lamp = new Vector3(_lamp.x, _signatureOrigin.y, _lamp.y);
                    DungeonShockwave.Spawn(lamp, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(lamp + Vector3.up * 1.2f, Text(RegionBossTexts.LampKey));
                    break;
                }

                case RegionBossSignature.NoveloWave:
                {
                    // The wave coming out of the Novelo, and the Guardian's shelter behind him.
                    Vector2 wave = origin + RegionBossRules.NoveloWaveFrom * (RegionBossRules.NoveloReach * 0.6f);
                    Vector2 shelter = origin - RegionBossRules.NoveloWaveFrom * (RegionBossRules.ShelterDepth * 0.5f);
                    Vector3 lee = new Vector3(shelter.x, _signatureOrigin.y, shelter.y);
                    DungeonShockwave.Spawn(new Vector3(wave.x, _signatureOrigin.y, wave.y), null, _controller.gameObject, 0f, 0f);
                    DungeonShockwave.Spawn(lee, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(lee + Vector3.up * 1.2f, Text(RegionBossTexts.ShelterKey));
                    break;
                }
            }
        }

        /// <summary>When the warning of one of the five runs out: false for every other signature.</summary>
        private bool ResolveDeepMapBoss(Vector2 heroFlat, ICombatTarget victim)
        {
            RegionBossSignature signature = _spawn.Signature;
            if (!IsDeepMapBossSignature(signature))
            {
                return false;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.SmokeCopy:
                    DungeonShockwave.Spawn(new Vector3(_smokeCopy.x, _signatureImpact.y, _smokeCopy.y), null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SmokeCopyHits(origin, _smokeCopy, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SmokeCopyDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.BurningStripes:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.LionessStripeBurns(_lionessNorth, origin, Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.LionessDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TailSweep:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.TailSweepHits(Flat(transform.position), Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.TailDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.LampHaven:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.DarkBreathHits(_lamp, origin, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.LampDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.NoveloWave:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.NoveloWaveHits(Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.NoveloDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;
            }

            return true;
        }
    }
}
