using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The abilities of the last four map bosses of the Reinos do Céu (Docs/Mapas100.md 92, 94, 98, 99; <see cref="RegionBossSignature"/> 100–103):
    /// what each shows when it warns and what it does when the warning runs out. Rules in RegionBossRules.SkyBosses.cs.
    /// </summary>
    public sealed partial class RegionBossAbilities
    {
        // The jar the Arconte da Roda leaves turning, and the drips of the Rachadura Grande and how wide this one opens.
        private int _litJar;
        private int _crackDrips;
        private int _crackDrip;

        /// <summary>Re-armed from the pool: every count starts over.</summary>
        private void ResetSkyMapBosses()
        {
            _litJar = 0;
            _crackDrips = 0;
            _crackDrip = 0;
        }

        private static bool IsSkyMapBossSignature(RegionBossSignature signature) =>
            signature >= RegionBossSignature.DownstreamWake && signature <= RegionBossSignature.WideningCrack;

        /// <summary>The warning of one of the four: what it marks on the ground, and the line it calls out (left as it is when it has no line of its own).</summary>
        private void WarnSkyMapBoss(RegionBossSignature signature, ref string warning)
        {
            if (!IsSkyMapBossSignature(signature))
            {
                return;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.DownstreamWake:
                {
                    // The wake, from the hero's spot downstream: three marks along it, the last one labelled.
                    Vector2 mark = Flat(_signatureImpact);
                    for (int i = 0; i <= 2; i++)
                    {
                        Vector2 spot = mark + RegionBossRules.RowerDownstream * (RegionBossRules.RowerWakeLength * i * 0.5f);
                        Vector3 at = new Vector3(spot.x, _signatureImpact.y, spot.y);
                        DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                        if (i == 2)
                        {
                            DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.RowerWakeKey));
                        }
                    }

                    break;
                }

                case RegionBossSignature.TurningJars:
                {
                    // The jar that keeps turning, labelled, and the hub under him.
                    _litJar = RegionBossRules.WheelLitJar(origin, Flat(_signatureImpact));
                    Vector2 jar = RegionBossRules.WheelJarPosition(origin, _litJar);
                    Vector3 at = new Vector3(jar.x, _signatureOrigin.y, jar.y);
                    DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.WheelJarKey));
                    break;
                }

                case RegionBossSignature.HeldStep:
                {
                    // The step below him, where it is safe.
                    Vector2 below = origin - RegionBossRules.StepUphill * (RegionBossRules.StepHalfDepth + 2f);
                    Vector3 at = new Vector3(below.x, _signatureOrigin.y, below.y);
                    DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.StepBelowKey));
                    break;
                }

                case RegionBossSignature.WideningCrack:
                {
                    // The crack along its axis through her, its two ends marked; the call-out says how wide it opens this time.
                    _crackDrip = _crackDrips++;
                    warning = RegionBossTexts.CrackWidthKey(_crackDrip);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Vector2 end = origin + RegionBossRules.CrackAxis * (RegionBossRules.CrackHalfLength * 0.6f * s);
                        DungeonShockwave.Spawn(new Vector3(end.x, _signatureOrigin.y, end.y), null, _controller.gameObject, 0f, 0f);
                    }

                    break;
                }
            }
        }

        /// <summary>When the warning of one of the four runs out: false for every other signature.</summary>
        private bool ResolveSkyMapBoss(Vector2 heroFlat, ICombatTarget victim)
        {
            RegionBossSignature signature = _spawn.Signature;
            if (!IsSkyMapBossSignature(signature))
            {
                return false;
            }

            Vector2 origin = Flat(_signatureOrigin);
            switch (signature)
            {
                case RegionBossSignature.DownstreamWake:
                {
                    Vector2 mark = Flat(_signatureImpact);
                    Vector2 end = mark + RegionBossRules.RowerDownstream * RegionBossRules.RowerWakeLength;
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    DungeonShockwave.Spawn(new Vector3(end.x, _signatureImpact.y, end.y), null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.RowerWakeHits(mark, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.RowerDamageFactor), ElementType.Holy, _controller.gameObject);
                    }

                    break;
                }

                case RegionBossSignature.TurningJars:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.WheelLockHits(origin, _litJar, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.WheelDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.HeldStep:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.HeldStepHits(origin,RegionBossRules.StepUphill, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.StepDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.WideningCrack:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.CrackDripHits(_crackDrip, origin, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.CrackDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;
            }

            return true;
        }
    }
}
