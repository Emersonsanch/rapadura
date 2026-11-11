using System.Collections.Generic;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Numbers and geometry of the Regions 10–16 abilities (<see cref="MountFarRegionCatalog"/>), pure so the server can run them too.</summary>
    public static class MountFarRegionRules
    {
        /// <summary>Index of the point nearest to <paramref name="at"/> within <paramref name="range"/> (flat), or -1.</summary>
        public static int NearestInRange(Vector3 at, IReadOnlyList<Vector3> points, float range)
        {
            int best = -1;
            float bestSqr = range * range;
            for (int i = 0; points != null && i < points.Count; i++)
            {
                float dx = points[i].x - at.x;
                float dz = points[i].z - at.z;
                float sqr = dx * dx + dz * dz;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Rugido do Portão: seconds a critter runs away (0 on bosses, <c>BossMultiplier</c> 0).</summary>
        public static float DreadSeconds(float seconds, bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(seconds, isBoss, bossMultiplier);

        /// <summary>
        /// Whether the mount stays on top of thin ice (map 60). The Rena da Aurora is light on its hooves: only the heavy
        /// mounts of <see cref="ThinIceRules"/> load the ice.
        /// </summary>
        public static bool CrossesThinIce(string mountId) => !ThinIceRules.IsHeavyMount(mountId);

        /// <summary>Wing gates only open for a real flyer (<c>MapRequirementRules</c> reads <see cref="MountSpec.Type"/>).</summary>
        public static bool AnswersWingGates(MountSpec spec) => spec != null && spec.Type == MountType.Voador;
    }

    /// <summary>
    /// What the Regions 10–16 abilities do in the arena; called by <see cref="MountWaveAbilities.Execute"/> for the kinds
    /// it does not handle itself. The numbers come from the ability definition. Returns the toast key, or null.
    /// </summary>
    public static class MountFarRegionAbilities
    {
        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects)
        {
            Vector3 at = ctx.Hero.transform.position;
            switch (ability.Kind)
            {
                case MountAbilityKind.Glide:
                    // Planar de Mesektet: a long lunge on the ground while the model soars (MountController), no landing scare.
                    effects.StartLunge(MountFlightRules.FlightLanding(at, ctx.Hero.transform.forward, ability.Distance), ability.DurationSeconds);
                    MountAudio.Play(MountAudio.WingBeat, at);
                    return MountFarRegionTexts.ToastGlide;

                case MountAbilityKind.WeatherImmunity:
                    // Cascos de Pedra-Pomes: burns and other debuffs are put out and kept out for the duration.
                    ctx.Buffs?.RemoveDebuffs();
                    effects.CleanseRemaining = Mathf.Max(effects.CleanseRemaining, ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Sniff, at);
                    return MountFarRegionTexts.ToastPumice;

                case MountAbilityKind.Untie:
                    return Untie(ability, ctx, at);

                case MountAbilityKind.AuroraRun:
                    ctx.Session?.StartFreeGallop(ability.DurationSeconds);
                    ctx.Buffs?.RemoveDebuffs(StatType.MoveSpeed);
                    effects.SlowImmuneRemaining = Mathf.Max(effects.SlowImmuneRemaining, ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Dash, at);
                    return MountFarRegionTexts.ToastAuroraRun;

                case MountAbilityKind.ThicketStride:
                    ctx.Buffs?.RemoveDebuffs(StatType.MoveSpeed);
                    effects.SlowImmuneRemaining = Mathf.Max(effects.SlowImmuneRemaining, ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Sniff, at);
                    return MountFarRegionTexts.ToastThicket;

                case MountAbilityKind.Reveal:
                    return Reveal(ability, at);

                case MountAbilityKind.Dread:
                    return Dread(ability, ctx, at);

                case MountAbilityKind.BoulderCharge:
                    return BoulderCharge(ability, ctx, effects, at);
            }

            return null;
        }

        private static string Untie(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            List<ArenaSecretObject> secrets = MountAbilityExecutor.FindSecrets(at, ability.Distance);
            var points = new List<Vector3>(secrets.Count);
            foreach (ArenaSecretObject secret in secrets)
            {
                points.Add(secret.transform.position);
            }

            int nearest = MountFarRegionRules.NearestInRange(at, points, ability.Distance);
            if (nearest < 0)
            {
                return MountFarRegionTexts.ToastUntieMiss;
            }

            secrets[nearest].ApplyDamage(1f, ElementType.None, ctx.Hero);
            MountAudio.Play(MountAudio.Sniff, points[nearest]);
            return MountFarRegionTexts.ToastUntieHit;
        }

        private static string Reveal(MountAbilityDefinition ability, Vector3 at)
        {
            int seen = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                if (MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    CombatFxManager.SpawnLootText(enemy.transform.position + Vector3.up * 2f, "!");
                    seen++;
                }
            }

            foreach (ArenaSecretObject secret in MountAbilityExecutor.FindSecrets(at, ability.Radius))
            {
                CombatFxManager.SpawnLootText(secret.transform.position + Vector3.up * 1.6f, "!");
                seen++;
            }

            MountAudio.Play(MountAudio.Sniff, at);
            return seen > 0 ? MountFarRegionTexts.ToastRevealSeen : MountFarRegionTexts.ToastRevealNone;
        }

        private static string Dread(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            int scared = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountFarRegionRules.DreadSeconds(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (seconds <= 0f || !MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                enemy.HoldAggro(seconds, forceAggroWhenHoldEnds: true);
                MountFearDriver.Apply(enemy, ctx.Hero.transform, seconds);
                enemy.Buffs?.ApplyEffect(MountWaveAbilities.Modifier("rugido_do_portao", StatType.AttackDamage, -ability.Magnitude, seconds, debuff: true),
                    MountWaveAbilities.BuffSourcePrefix + ability.AbilityId, ctx.Hero);
                scared++;
            }

            MountAudio.Play(MountAudio.Bray, at);
            return scared > 0 ? MountFarRegionTexts.ToastDread : null;
        }

        private static string BoulderCharge(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            Vector3 forward = ctx.Hero.transform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude < 1e-6f ? Vector3.forward : forward.normalized;
            Vector3 end = at + forward * ability.Distance;

            int hit = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                if (!MountWaveAbilityRules.InChargePath(at, end, enemy.transform.position, ability.Radius)
                    || MountRules.EffectDurationOn(1f, enemy.IsBoss, ability.BossMultiplier) <= 0f)
                {
                    continue;
                }

                MountPushDriver.Apply(enemy, MountWaveAbilityRules.PushDirection(at, enemy.transform.position, forward),
                    MountWaveAbilityRules.KnockbackPush, MountWaveAbilityRules.KnockbackPushSeconds);
                hit++;
            }

            foreach (ArenaSecretObject secret in MountAbilityExecutor.FindSecrets(at, ability.Distance + ability.Radius))
            {
                if (MountWaveAbilityRules.InChargePath(at, end, secret.transform.position, ability.Radius))
                {
                    secret.ApplyDamage(1f, ElementType.None, ctx.Hero);
                    hit++;
                }
            }

            effects.StartLunge(end, ability.DurationSeconds);
            MountAudio.Play(MountAudio.Dash, at);
            return hit > 0 ? MountFarRegionTexts.ToastBoulder : null;
        }
    }
}
