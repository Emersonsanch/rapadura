using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The abilities of the six more map bosses of Regions 10–16 (Docs/Mapas100.md 57, 63, 67, 74, 86, 97; <see cref="RegionBossSignature"/> 60–65):
    /// what each shows when it warns and what it does when the warning runs out. Rules in RegionBossRules.UpperMapBosses.cs.
    /// </summary>
    public sealed partial class RegionBossAbilities
    {
        // The king's gusts, the aurora's dances and the route she lit, Guayota's eruptions, the sail's swings and the side the boom goes.
        private int _aysGusts;
        private int _aysGust;
        private int _auroraDances;
        private int _auroraDance;
        private int _eruptions;
        private int _eruption;
        private int _boomSwings;
        private bool _boomStarboard;

        /// <summary>Re-armed from the pool: every count starts over.</summary>
        private void ResetUpperMapBosses()
        {
            _aysGusts = 0;
            _auroraDances = 0;
            _eruptions = 0;
            _boomSwings = 0;
        }

        private static bool IsUpperMapBossSignature(RegionBossSignature signature) =>
            signature >= RegionBossSignature.GorgeGust && signature <= RegionBossSignature.BoomSwing;

        /// <summary>The warning of one of the six: what it marks on the ground, and the line it calls out (left as it is when it has no line of its own).</summary>
        private void WarnUpperMapBoss(RegionBossSignature signature, ref string warning)
        {
            if (!IsUpperMapBossSignature(signature))
            {
                return;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.GorgeGust:
                {
                    _aysGust = _aysGusts++;
                    warning = RegionBossTexts.AysGustKey(_aysGust);
                    Vector2 downwind = origin + RegionBossRules.AysGustDirection(_aysGust) * (RegionBossRules.AysGustLength * 0.5f);
                    DungeonShockwave.Spawn(new Vector3(downwind.x, _signatureOrigin.y, downwind.y), null, _controller.gameObject, 0f, 0f);
                    break;
                }

                case RegionBossSignature.AuroraRoute:
                {
                    // The route she lights: three marks along it, the last one labelled.
                    _auroraDance = RegionBossRules.AuroraDanceOf(_auroraDances++);
                    Vector2 along = RegionBossRules.AuroraRouteDirection(_auroraDance);
                    for (int i = 1; i <= 3; i++)
                    {
                        Vector2 spot = origin + along * (RegionBossRules.AuroraReach * i / 3.5f);
                        Vector3 at = new Vector3(spot.x, _signatureOrigin.y, spot.y);
                        DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                        if (i == 3)
                        {
                            DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.AuroraRouteKey));
                        }
                    }

                    break;
                }

                case RegionBossSignature.CraterSpit:
                {
                    // The hot squares round the hero's spot: where the lava will fall this time.
                    _eruption = _eruptions++;
                    Vector2 mark = Flat(_signatureImpact);
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            Vector2 spot = mark + new Vector2(dx, dz) * RegionBossRules.CraterSquare;
                            if (RegionBossRules.CraterSpitHits(_eruption, origin, spot))
                            {
                                DungeonShockwave.Spawn(new Vector3(spot.x, _signatureImpact.y, spot.y), null, _controller.gameObject, 0f, 0f);
                            }
                        }
                    }

                    break;
                }

                case RegionBossSignature.CoilLoops:
                    foreach (Vector2 loop in RegionBossRules.KnotLoopCentres(origin, Flat(_signatureImpact)))
                    {
                        Vector3 at = new Vector3(loop.x, _signatureOrigin.y, loop.y);
                        DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                        DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.KnotLoopKey));
                    }

                    break;

                case RegionBossSignature.WingSpan:
                {
                    // The two bars of her wings' shadow: on the hero's spot and a beat further on.
                    Vector2 mark = Flat(_signatureImpact);
                    Vector2 line = mark - origin;
                    Vector2 along = line.sqrMagnitude > 0.01f ? line.normalized : Vector2.up;
                    Vector2 beat = mark + along * RegionBossRules.WingBeatGap;
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    DungeonShockwave.Spawn(new Vector3(beat.x, _signatureImpact.y, beat.y), null, _controller.gameObject, 0f, 0f);
                    break;
                }

                case RegionBossSignature.BoomSwing:
                {
                    _boomStarboard = RegionBossRules.BoomSwingsStarboard(_boomSwings++);
                    warning = RegionBossTexts.BoomSideKey(_boomStarboard);
                    Vector2 side = origin + (_boomStarboard ? Vector2.right : Vector2.left) * ((RegionBossRules.BoomInner + RegionBossRules.BoomOuter) * 0.5f);
                    DungeonShockwave.Spawn(new Vector3(side.x, _signatureOrigin.y, side.y), null, _controller.gameObject, 0f, 0f);
                    break;
                }
            }
        }

        /// <summary>When the warning of one of the six runs out: false for every other signature, which the main switch resolves.</summary>
        private bool ResolveUpperMapBoss(Vector2 heroFlat, ICombatTarget victim)
        {
            RegionBossSignature signature = _spawn.Signature;
            if (!IsUpperMapBossSignature(signature))
            {
                return false;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.GorgeGust:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.AysGustHits(_aysGust, Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.AysDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.AuroraRoute:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.AuroraRouteHits(_auroraDance, Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.AuroraDamageFactor), ElementType.Ice, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.CraterSpit:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.CraterSpitHits(_eruption, origin, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.CraterDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.CoilLoops:
                    foreach (Vector2 loop in RegionBossRules.KnotLoopCentres(origin, Flat(_signatureImpact)))
                    {
                        DungeonShockwave.Spawn(new Vector3(loop.x, transform.position.y, loop.y), null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.KnotLoopsHit(origin, Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.KnotDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.WingSpan:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.WingSpanHits(origin, Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.WingDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.BoomSwing:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.BoomSwingHits(_boomStarboard, Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.BoomDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;
            }

            return true;
        }
    }
}
