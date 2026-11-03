using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>What a [distância] enemy shoots (<see cref="EnemyDefinition.Projectile"/>). Append only (serialized).</summary>
    public enum EnemyProjectileKind
    {
        /// <summary>Melee: the strike is a swing (every enemy before the ranged pass).</summary>
        None = 0,
        /// <summary>Abutre de Brasa: a spat ember, fire, straight and quick.</summary>
        Ember = 1,
        /// <summary>Gavião-de-Penacho (and Corvo-da-Treva): a hard flight feather thrown like a dart, straight and fastest.</summary>
        Feather = 2,
        /// <summary>Lanterna Quebrada dos Artífices: a cracked bolt of trapped light that leans a little toward the hero.</summary>
        LanternBolt = 3,
        /// <summary>Peixe-Lanterna Apagado: a slow glob of cold light flicked from its lure, homing gently.</summary>
        LureGlob = 4,
    }

    /// <summary>What the brain does at the strike of a telegraphed attack (<see cref="EnemyRangedAttack.StrikeKindOf"/>).</summary>
    public enum EnemyStrikeKind
    {
        /// <summary>Melee swing: the brain applies the damage to its target (Hitbox or direct, Espinhos apply).</summary>
        Swing = 0,
        /// <summary>[distância] definition: the brain fires <see cref="EnemyDefinition.Projectile"/>; only the shot deals damage.</summary>
        Projectile = 1,
        /// <summary>A component shoots on its own (the ossuary archer/sorcerer watch <see cref="EnemyController.StrikeSequence"/>): the brain deals nothing.</summary>
        External = 2,
    }

    /// <summary>
    /// The shot of the open-map [distância] enemies (Docs/Mapas100.md §0.5: "atira de longe"). The regular
    /// <see cref="EnemyController"/> brain does everything else: it chases until <see cref="EnemyDefinition.AttackRange"/>
    /// (7–9 m instead of ~1.5 m), stops, telegraphs the windup and, at the strike, <see cref="States.EnemyAttackState"/>
    /// calls <see cref="EnemyController.FireProjectile"/> instead of swinging. The flight and the hit are the ossuary's own
    /// <see cref="DungeonProjectile"/> (distance check against the hero's chest, plain <c>ApplyDamage</c>: Defense applies,
    /// Espinhos do not, it is not melee), only dressed per kind here; the ossuary archer/sorcerer keep their own launchers.
    /// Damage is the brain's <see cref="EnemyController.EffectiveAttackDamage"/> (level scaling, NG+, debuffs).
    /// </summary>
    public static class EnemyRangedAttack
    {
        /// <summary>Height (m) above the enemy's pivot the shot leaves from (a flyer hovers about there).</summary>
        public const float MuzzleHeight = 1.2f;

        /// <summary>Forward offset (m) of the muzzle, so the shot starts in front of the body.</summary>
        public const float MuzzleForward = 0.6f;

        /// <summary>The shot flies this much (m) past the attack range before it fizzles.</summary>
        public const float ExtraFlight = 3f;

        /// <summary>
        /// The strike rule: a definition that shoots fires its projectile; an enemy whose shot belongs to a component
        /// (<paramref name="externalShot"/>) gets no brain damage at all; everyone else swings. A shooter never also lands the
        /// melee hit of its AttackDamage at range (the ossuary archer used to: 10 damage at 14 m on top of its arrow).
        /// </summary>
        public static EnemyStrikeKind StrikeKindOf(bool definitionShoots, bool externalShot)
        {
            if (definitionShoots)
            {
                return EnemyStrikeKind.Projectile;
            }

            return externalShot ? EnemyStrikeKind.External : EnemyStrikeKind.Swing;
        }

        public static float SpeedOf(EnemyProjectileKind kind)
        {
            switch (kind)
            {
                case EnemyProjectileKind.Ember: return 12f;
                case EnemyProjectileKind.Feather: return 15f;
                case EnemyProjectileKind.LanternBolt: return 10f;
                case EnemyProjectileKind.LureGlob: return 8f;
                default: return 0f;
            }
        }

        /// <summary>How hard (rad/s) the shot leans toward the hero in flight; 0 = straight (always dodgeable sideways).</summary>
        public static float HomingOf(EnemyProjectileKind kind)
        {
            switch (kind)
            {
                case EnemyProjectileKind.LanternBolt: return 1.2f;
                case EnemyProjectileKind.LureGlob: return 1.6f;
                default: return 0f;
            }
        }

        public static ElementType ElementOf(EnemyProjectileKind kind)
        {
            switch (kind)
            {
                case EnemyProjectileKind.Ember: return ElementType.Fire;
                case EnemyProjectileKind.Feather: return ElementType.Physical;
                case EnemyProjectileKind.LanternBolt: return ElementType.Arcane;
                case EnemyProjectileKind.LureGlob: return ElementType.Dark;
                default: return ElementType.Physical;
            }
        }

        public static Color ColorOf(EnemyProjectileKind kind)
        {
            switch (kind)
            {
                case EnemyProjectileKind.Ember: return new Color(1f, 0.5f, 0.15f);
                case EnemyProjectileKind.Feather: return new Color(0.55f, 0.42f, 0.3f);
                case EnemyProjectileKind.LanternBolt: return DungeonProps.Amber;
                case EnemyProjectileKind.LureGlob: return new Color(0.5f, 0.9f, 1f);
                default: return Color.white;
            }
        }

        /// <summary>Where a shot of an enemy standing at <paramref name="enemy"/> leaves from.</summary>
        public static Vector3 MuzzleOf(Transform enemy) =>
            enemy.position + Vector3.up * MuzzleHeight + enemy.forward * MuzzleForward;

        /// <summary>
        /// Fires one <paramref name="kind"/> shot from <paramref name="from"/> at <paramref name="target"/>'s chest worth
        /// <paramref name="damage"/>, flying up to <paramref name="attackRange"/> + <see cref="ExtraFlight"/>. Null for
        /// <see cref="EnemyProjectileKind.None"/>.
        /// </summary>
        public static DungeonProjectile Launch(EnemyProjectileKind kind, Vector3 from, Transform target, GameObject source, float damage, float attackRange)
        {
            if (kind == EnemyProjectileKind.None)
            {
                return null;
            }

            var root = new GameObject("EnemyShot_" + kind);
            root.transform.position = from;
            var block = new MaterialPropertyBlock();
            Color color = ColorOf(kind);
            switch (kind)
            {
                case EnemyProjectileKind.Ember:
                    DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Ember", from, Quaternion.identity, Vector3.one * 0.22f, color, block, glow: true);
                    break;
                case EnemyProjectileKind.Feather:
                    DungeonProps.Part(root.transform, PrimitiveType.Cube, "Feather", from, Quaternion.identity, new Vector3(0.08f, 0.02f, 0.45f), color, block);
                    DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Tip", from + Vector3.forward * 0.2f, Quaternion.identity, Vector3.one * 0.07f, DungeonProps.Ivory, block, glow: true);
                    break;
                case EnemyProjectileKind.LanternBolt:
                    DungeonProps.Part(root.transform, PrimitiveType.Cube, "Bolt", from, Quaternion.Euler(45f, 45f, 0f), Vector3.one * 0.2f, color, block, glow: true);
                    break;
                case EnemyProjectileKind.LureGlob:
                    DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Glob", from, Quaternion.identity, Vector3.one * 0.28f, color, block, glow: true);
                    DungeonProps.AddLight(root.transform, from, color, 1f, 2.5f);
                    break;
            }

            var projectile = root.AddComponent<DungeonProjectile>();
            projectile.Setup(target, source, damage, SpeedOf(kind), Mathf.Max(0f, attackRange) + ExtraFlight, HomingOf(kind), false, ElementOf(kind), color);
            EnemyShotAudio.Play(kind, from);
            return projectile;
        }
    }
}
