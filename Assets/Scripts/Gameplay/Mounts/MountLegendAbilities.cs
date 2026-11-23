using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Numbers of <see cref="MountLegendCatalog"/>, pure so the server can run them too. Nothing deals damage (§2.2).</summary>
    public static class MountLegendRules
    {
        /// <summary>"Flutua 1 m" (ficha 76): the skiff's resting height over the ground, standing or moving.</summary>
        public const float FloatHeight = 1.0f;

        /// <summary>The skiff glides low in a city or a house (§9.2, like the flyers' <see cref="MountFlightRules.LowHover"/>).</summary>
        public const float LowFloatHeight = 0.3f;

        /// <summary>Gentle bob of the float (m) at full motion.</summary>
        public const float FloatBob = 0.06f;

        /// <summary>Bob cycles per second.</summary>
        public const float FloatBobHz = 0.6f;

        /// <summary>Travessia de Luz: range of the skiff's light.</summary>
        public const float CrossingLampRadius = 12f;

        /// <summary>Travessia de Luz: seconds the dimmed commons in the light lose track of the cousin (bosses never).</summary>
        public const float CrossingDazzleSeconds = 2f;

        /// <summary>Whether <paramref name="recipe"/> floats over the ground instead of walking on its legs (the Barquinha de Luz alone).</summary>
        public static bool Floats(MountVisualRecipe recipe) => recipe == MountVisualRecipe.LightSkiff;

        /// <summary>
        /// The float of the skiff at <paramref name="seconds"/>: <see cref="FloatHeight"/> (or <see cref="LowFloatHeight"/> where flying is
        /// not allowed) plus a slow bob, smaller with <paramref name="reducedMotion"/>. Never under the ground.
        /// </summary>
        public static float Float(float seconds, bool allowed, bool reducedMotion)
        {
            float rest = allowed ? FloatHeight : LowFloatHeight;
            float bob = Mathf.Sin(seconds * FloatBobHz * Mathf.PI * 2f) * FloatBob * (reducedMotion ? 0.25f : 1f);
            return Mathf.Max(0f, rest + bob);
        }

        /// <summary>Travessia de Luz: seconds a critter in the light forgets the cousin (0 on bosses when <paramref name="bossMultiplier"/> is 0).</summary>
        public static float DazzleSeconds(bool isBoss, float bossMultiplier) => MountRules.EffectDurationOn(CrossingDazzleSeconds, isBoss, bossMultiplier);
    }

    /// <summary>
    /// What the ability of <see cref="MountLegendCatalog"/> does in the arena; called by <see cref="MountBossAbilities.Execute"/> for the kinds it
    /// does not handle. Returns the toast key, or null.
    /// </summary>
    public static class MountLegendAbilities
    {
        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx, MountAbilityEffects effects)
        {
            if (ability.Kind != MountAbilityKind.LightCrossing)
            {
                return null;
            }

            // Travessia de Luz: the skiff rows on its own light: the cousin and the group around get light feet, the light opens round
            // them, and the common critters caught in it lose track of the cousin for a moment.
            Vector3 at = ctx.Hero.transform.position;
            string source = MountWaveAbilities.BuffSourcePrefix + ability.AbilityId;
            foreach (PlayerStats ally in MountWaveAbilities.Allies(at, ability.Radius))
            {
                if (ally.TryGetComponent(out BuffController buffs))
                {
                    buffs.ApplyEffect(MountWaveAbilities.Modifier("travessia_de_luz", StatType.MoveSpeed, ability.Magnitude, ability.DurationSeconds, false),
                        source, ally.gameObject);
                }
            }

            int dazzled = 0;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                float seconds = MountLegendRules.DazzleSeconds(enemy.IsBoss, ability.BossMultiplier);
                if (seconds > 0f && MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                {
                    enemy.HoldAggro(seconds, forceAggroWhenHoldEnds: true);
                    dazzled++;
                }
            }

            effects.StartLamp(ability.DurationSeconds, MountLegendRules.CrossingLampRadius, new Color(1f, 0.95f, 0.7f));
            MountAudio.Play(MountAudio.Dash, at);
            return dazzled > 0 ? MountLegendTexts.ToastCrossingDazzle : MountLegendTexts.ToastCrossing;
        }
    }
}
