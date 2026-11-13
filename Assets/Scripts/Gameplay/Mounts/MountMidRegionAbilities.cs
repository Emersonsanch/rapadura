using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Numbers and geometry of the Regions 5–9 abilities (<see cref="MountMidRegionCatalog"/>), pure so the server can run them too.</summary>
    public static class MountMidRegionRules
    {
        /// <summary>Vela da Monção: how far the gust shoves each common critter around the turtle.</summary>
        public const float SailGustPush = 4f;

        /// <summary>Salto do Charco: height of the frog's leap (the double jump of the Íbex is 2.5 m).</summary>
        public const float LeapHeight = 2.2f;

        /// <summary>Salto do Charco: seconds the mud splash slows the critters round the landing (bosses by <c>BossMultiplier</c>).</summary>
        public const float SplashSlowSeconds = 3f;

        /// <summary>Rastro de Luz Branca: range of the white lamp the stag carries while the trail lasts.</summary>
        public const float StagLampRadius = 12f;

        /// <summary>Where the frog lands: <paramref name="distance"/> ahead on the flat (walls still stop the lunge in the scene).</summary>
        public static Vector3 LeapLanding(Vector3 from, Vector3 forward, float distance) => MountFlightRules.FlightLanding(from, forward, distance);

        /// <summary>Salto do Charco: seconds of slow on a critter (bosses get <paramref name="bossMultiplier"/> of it).</summary>
        public static float SplashSeconds(bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(SplashSlowSeconds, isBoss, bossMultiplier);

        /// <summary>Vela da Monção: whether the gust moves this critter (commons only: the turtle's <c>BossMultiplier</c> is 0).</summary>
        public static bool GustMoves(bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(1f, isBoss, bossMultiplier) > 0f;

        /// <summary>Fôlego de Soslan: stamina given back to an ally of <paramref name="maxStamina"/> (a fraction of it, never negative).</summary>
        public static float AllyStamina(float maxStamina, float fraction) => Mathf.Max(0f, maxStamina) * Mathf.Clamp01(fraction);
    }

    /// <summary>
    /// What the Regions 5–9 abilities do in the arena; called by <see cref="MountFarRegionAbilities.Execute"/> for the kinds it
    /// does not handle. The numbers come from the ability definition and <see cref="MountMidRegionRules"/>. Returns the toast
    /// key, or null.
    /// </summary>
    public static class MountMidRegionAbilities
    {
        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects)
        {
            Vector3 at = ctx.Hero.transform.position;
            switch (ability.Kind)
            {
                case MountAbilityKind.MonsoonSail:
                    return SailGust(ability, ctx, at);

                case MountAbilityKind.BogLeap:
                    return Leap(ability, ctx, effects, at);

                case MountAbilityKind.WarmFleece:
                    return Fleece(ability, ctx, at);

                case MountAbilityKind.LightTrail:
                    // Rastro de Luz Branca: a healing trail of white light, and a white lamp that lights the dark round the stag.
                    effects.StartTrail(MountAbilityEffects.TrailKind.Light, ability.DurationSeconds, ability.Radius, ability.Magnitude);
                    effects.StartLamp(ability.DurationSeconds, MountMidRegionRules.StagLampRadius, new Color(0.92f, 0.95f, 1f));
                    MountAudio.Play(MountAudio.AbilityReady, at);
                    return MountMidRegionTexts.ToastLightTrail;

                case MountAbilityKind.SecondWind:
                    return Breath(ability, ctx, at);
            }

            // The map-sheet mounts of the Regions 1–4, 5 and 7 (MountSheetCatalog).
            return MountSheetAbilities.Execute(ability, ctx, effects);
        }

        private static string SailGust(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            // Vela da Monção: the sail fills, the turtle runs without tiring and the gust shoves the commons around away.
            ctx.Session?.StartFreeGallop(ability.DurationSeconds);
            Vector3 forward = ctx.Hero.transform.forward;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                if (!MountMidRegionRules.GustMoves(enemy.IsBoss, ability.BossMultiplier)
                    || !MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                MountPushDriver.Apply(enemy, MountWaveAbilityRules.PushDirection(at, enemy.transform.position, forward),
                    MountMidRegionRules.SailGustPush, MountWaveAbilityRules.KnockbackPushSeconds);
            }

            MountAudio.Play(MountAudio.WingBeat, at);
            return MountMidRegionTexts.ToastSail;
        }

        private static string Leap(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            // Salto do Charco: a big leap ahead (over water and mud); the splash slows the critters round the landing.
            Vector3 landing = MountMidRegionRules.LeapLanding(at, ctx.Hero.transform.forward, ability.Distance);
            effects.StartHop(MountMidRegionRules.LeapHeight);
            effects.StartLunge(landing, ability.DurationSeconds);

            int splashed = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountMidRegionRules.SplashSeconds(enemy.IsBoss, ability.BossMultiplier);
                if (seconds <= 0f || enemy.Buffs == null || !MountWaveAbilityRules.InRadius(landing, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                enemy.Buffs.ApplyEffect(MountWaveAbilities.Modifier("salto_do_charco", StatType.MoveSpeed, -ability.Magnitude, seconds, debuff: true),
                    MountWaveAbilities.BuffSourcePrefix + ability.AbilityId, ctx.Hero);
                splashed++;
            }

            MountAudio.Play(MountAudio.Dash, at);
            return splashed > 0 ? MountMidRegionTexts.ToastLeapSplash : MountMidRegionTexts.ToastLeap;
        }

        private static string Fleece(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            // Lã do Teto: the yak shakes its wool over the group: the cold lets go (slows off) and blows land softer for a while.
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            foreach (PlayerStats ally in MountWaveAbilities.Allies(at, ability.Radius))
            {
                if (ally.TryGetComponent(out BuffController buffs))
                {
                    buffs.RemoveDebuffs(StatType.MoveSpeed);
                    buffs.ApplyEffect(MountWaveAbilities.Modifier("la_do_teto", StatType.Defense, ability.Magnitude, ability.DurationSeconds, false),
                        source, ctx.Hero);
                }
            }

            MountAudio.Play(MountAudio.Cowbell, at);
            return MountMidRegionTexts.ToastFleece;
        }

        private static string Breath(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            // Fôlego de Soslan: the horse is fresh again, and the cousins round it get their breath back too.
            ctx.Session?.RefillStamina();
            foreach (PlayerStats ally in MountWaveAbilities.Allies(at, ability.Radius))
            {
                ally.RestoreStamina(MountMidRegionRules.AllyStamina(ally.MaxStamina, ability.Magnitude));
            }

            MountAudio.Play(MountAudio.Dash, at);
            return MountMidRegionTexts.ToastBreath;
        }
    }
}
