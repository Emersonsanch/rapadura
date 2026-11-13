using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Numbers and geometry of the map-sheet abilities (<see cref="MountSheetCatalog"/>), pure so the server can run them too.</summary>
    public static class MountSheetRules
    {
        /// <summary>Uivo do Tambor Rubro: seconds the critters stay slowed after they stop running (the drum still in their ears).</summary>
        public const float HowlSlowSeconds = 3f;

        /// <summary>Salto do Capim Alto: height of the eland's leap (the frog's is 2.2 m, the Íbex's double jump 2.5 m).</summary>
        public const float BoundHeight = 3f;

        /// <summary>Cuspe de Luz: how long the spit keeps shining around the olm.</summary>
        public const float SpitLampSeconds = 8f;

        /// <summary>Cuspe de Luz: range of that light.</summary>
        public const float SpitLampRadius = 10f;

        /// <summary>Pisão de Aratta: how far the stomp shoves each common critter (bosses are not moved).</summary>
        public const float StompPush = 2f;

        /// <summary>Uivo do Tambor Rubro: seconds a critter runs away (0 on bosses: the wolf's <c>BossMultiplier</c> is 0).</summary>
        public static float HowlFleeSeconds(float seconds, bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(seconds, isBoss, bossMultiplier);

        /// <summary>Uivo do Tambor Rubro: total seconds of the slow on a critter that ran (the run plus <see cref="HowlSlowSeconds"/>).</summary>
        public static float HowlSlowTotal(float fleeSeconds) => fleeSeconds > 0f ? fleeSeconds + HowlSlowSeconds : 0f;

        /// <summary>Salto do Capim Alto: where the eland lands, <paramref name="distance"/> ahead on the flat (walls still stop the lunge in the scene).</summary>
        public static Vector3 BoundLanding(Vector3 from, Vector3 forward, float distance) => MountFlightRules.FlightLanding(from, forward, distance);

        /// <summary>Salto do Capim Alto: seconds a critter near the take-off loses the rider (bosses never do).</summary>
        public static float LostSightSeconds(float seconds, bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(seconds, isBoss, bossMultiplier);

        /// <summary>
        /// Cuspe de Luz: where the glob lands. The nearest critter ahead within <paramref name="range"/> (in front of the olm,
        /// flat), or the point <paramref name="range"/> straight ahead when there is none.
        /// </summary>
        public static Vector3 SpitTarget(Vector3 from, Vector3 forward, float range, System.Collections.Generic.IReadOnlyList<Vector3> critters)
        {
            Vector3 flat = new Vector3(forward.x, 0f, forward.z);
            flat = flat.sqrMagnitude < 1e-6f ? Vector3.forward : flat.normalized;
            Vector3 best = from + flat * range;
            float bestSqr = range * range;
            for (int i = 0; critters != null && i < critters.Count; i++)
            {
                Vector3 d = critters[i] - from;
                d.y = 0f;
                float sqr = d.sqrMagnitude;
                if (sqr > bestSqr || Vector3.Dot(d, flat) <= 0f)
                {
                    continue;
                }

                bestSqr = sqr;
                best = critters[i];
            }

            return best;
        }

        /// <summary>Cuspe de Luz and Pisão de Aratta: seconds of the effect on a critter (bosses get <paramref name="bossMultiplier"/> of it).</summary>
        public static float EffectSeconds(float seconds, bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(seconds, isBoss, bossMultiplier);

        /// <summary>Pisão de Aratta: whether the stomp shoves this critter (commons only).</summary>
        public static bool StompMoves(bool isBoss) => !isBoss;
    }

    /// <summary>
    /// What the map-sheet abilities do in the arena; called by <see cref="MountMidRegionAbilities.Execute"/> for the kinds it
    /// does not handle. The numbers come from the ability definition and <see cref="MountSheetRules"/>. Returns the toast key, or null.
    /// </summary>
    public static class MountSheetAbilities
    {
        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects)
        {
            Vector3 at = ctx.Hero.transform.position;
            switch (ability.Kind)
            {
                case MountAbilityKind.ShallowsWade:
                    return Wade(ability, ctx, effects, at);

                case MountAbilityKind.DrumHowl:
                    return Howl(ability, ctx, at);

                case MountAbilityKind.LongBound:
                    return Bound(ability, ctx, effects, at);

                case MountAbilityKind.GlowSpit:
                    return Spit(ability, ctx, effects, at);

                case MountAbilityKind.AurochsStomp:
                    return Stomp(ability, ctx, at);

                case MountAbilityKind.StormHide:
                    return Hide(ability, ctx, effects, at);
            }

            return null;
        }

        private static string Wade(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            // Passo de Pernalta: the heron leads the group through the shallows: nobody near stays slowed, and the rider shrugs off
            // new slows for the duration.
            ctx.Buffs?.RemoveDebuffs(StatType.MoveSpeed);
            effects.SlowImmuneRemaining = Mathf.Max(effects.SlowImmuneRemaining, ability.DurationSeconds);
            foreach (PlayerStats ally in MountWaveAbilities.Allies(at, ability.Radius))
            {
                if (ally.TryGetComponent(out BuffController buffs))
                {
                    buffs.RemoveDebuffs(StatType.MoveSpeed);
                }
            }

            MountAudio.Play(MountAudio.WingBeat, at);
            return MountSheetTexts.ToastWade;
        }

        private static string Howl(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            // Uivo do Tambor Rubro: the commons around run for a moment and come back slower; bosses do not flinch.
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            int scared = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountSheetRules.HowlFleeSeconds(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (seconds <= 0f || !MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                enemy.HoldAggro(seconds, forceAggroWhenHoldEnds: true);
                MountFearDriver.Apply(enemy, ctx.Hero.transform, seconds);
                enemy.Buffs?.ApplyEffect(MountWaveAbilities.Modifier("uivo_do_tambor_rubro", StatType.MoveSpeed, -ability.Magnitude,
                    MountSheetRules.HowlSlowTotal(seconds), debuff: true), source, ctx.Hero);
                scared++;
            }

            MountAudio.Play(MountAudio.Bray, at);
            return scared > 0 ? MountSheetTexts.ToastHowl : MountSheetTexts.ToastHowlNone;
        }

        private static string Bound(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            // Salto do Capim Alto: one long leap over the grass and the rocks; the commons near the take-off lose sight of the rider.
            Vector3 landing = MountSheetRules.BoundLanding(at, ctx.Hero.transform.forward, ability.Distance);
            effects.StartHop(MountSheetRules.BoundHeight);
            effects.StartLunge(landing, ability.DurationSeconds);
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountSheetRules.LostSightSeconds(ability.Magnitude, enemy.IsBoss, ability.BossMultiplier);
                if (seconds > 0f && MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    enemy.HoldAggro(seconds, forceAggroWhenHoldEnds: true);
                }
            }

            MountAudio.Play(MountAudio.Dash, at);
            return MountSheetTexts.ToastBound;
        }

        private static string Spit(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            // Cuspe de Luz: a glob of cold light at the nearest critter ahead: whoever is round the splash hits weaker for a while, and
            // the olm's skin keeps glowing in the dark.
            var live = MountWaveAbilities.LiveEnemies();
            var points = new System.Collections.Generic.List<Vector3>(live.Count);
            foreach (EnemyController enemy in live)
            {
                points.Add(enemy.transform.position);
            }

            Vector3 target = MountSheetRules.SpitTarget(at, ctx.Hero.transform.forward, ability.Distance, points);
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            int dazzled = 0;
            foreach (EnemyController enemy in live)
            {
                float seconds = MountSheetRules.EffectSeconds(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (seconds <= 0f || enemy.Buffs == null || !MountWaveAbilityRules.InRadius(target, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                enemy.Buffs.ApplyEffect(MountWaveAbilities.Modifier("cuspe_de_luz", StatType.AttackDamage, -ability.Magnitude, seconds, debuff: true),
                    source, ctx.Hero);
                dazzled++;
            }

            effects.StartLamp(MountSheetRules.SpitLampSeconds, MountSheetRules.SpitLampRadius, new Color(0.75f, 1f, 0.85f));
            MountAudio.Play(MountAudio.Sniff, at);
            return dazzled > 0 ? MountSheetTexts.ToastSpitHit : MountSheetTexts.ToastSpitMiss;
        }

        private static string Stomp(MountAbilityDefinition ability, MountAbilityContext ctx, Vector3 at)
        {
            // Pisão de Aratta: the ground shakes round the aurochs: everything near is slowed (bosses for half as long) and the commons
            // are shoved a step back.
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            Vector3 forward = ctx.Hero.transform.forward;
            int hit = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountSheetRules.EffectSeconds(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (seconds <= 0f || !MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    continue;
                }

                enemy.Buffs?.ApplyEffect(MountWaveAbilities.Modifier("pisao_de_aratta", StatType.MoveSpeed, -ability.Magnitude, seconds, debuff: true),
                    source, ctx.Hero);
                if (MountSheetRules.StompMoves(enemy.IsBoss))
                {
                    MountPushDriver.Apply(enemy, MountWaveAbilityRules.PushDirection(at, enemy.transform.position, forward),
                        MountSheetRules.StompPush, MountWaveAbilityRules.KnockbackPushSeconds);
                }

                hit++;
            }

            MountAudio.Play(MountAudio.Dash, at);
            return hit > 0 ? MountSheetTexts.ToastStomp : null;
        }

        private static string Hide(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects, Vector3 at)
        {
            // Couro de Tempestade: the camel kneels its back to the storm: the rider's debuffs blow off and stay off, nothing slows the
            // rider for the duration, and the group sheltered behind it loses its slows.
            ctx.Buffs?.RemoveDebuffs();
            effects.CleanseRemaining = Mathf.Max(effects.CleanseRemaining, ability.DurationSeconds);
            effects.SlowImmuneRemaining = Mathf.Max(effects.SlowImmuneRemaining, ability.DurationSeconds);
            foreach (PlayerStats ally in MountWaveAbilities.Allies(at, ability.Radius))
            {
                if (ally.TryGetComponent(out BuffController buffs))
                {
                    buffs.RemoveDebuffs(StatType.MoveSpeed);
                }
            }

            MountAudio.Play(MountAudio.Cowbell, at);
            return MountSheetTexts.ToastStorm;
        }
    }
}
