using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>Part names of the procedural enemies (what <see cref="ProceduralEnemyRig"/> and the procedural animator look for).</summary>
    public static class ProceduralEnemyParts
    {
        public const string Body = "Body";
        public const string Head = "Head";
        public const string ArmLeft = "Arm_L";
        public const string ArmRight = "Arm_R";
        public const string FistLeft = "Fist_L";
        public const string FistRight = "Fist_R";
        /// <summary>Legs contain "Leg", so <c>EnemyProceduralAnimator.IsLegName</c> already finds them.</summary>
        public const string LegLeft = "Leg_L";
        public const string LegRight = "Leg_R";
        /// <summary>
        /// Jointed legs (<c>ProceduralLegLocomotion</c> convention): hip pivot <c>Leg_&lt;id&gt;</c> → knee pivot
        /// <c>Shin_&lt;id&gt;</c> → ankle/foot pivot <c>Foot_&lt;id&gt;</c>. Bipeds use the ids L/R, quadrupeds
        /// FL/FR/HL/HR, arthropods L0..L3/R0..R3 (front to back).
        /// </summary>
        public const string LegPrefix = "Leg_";
        public const string ShinPrefix = "Shin_";
        public const string FootPrefix = "Foot_";
        public const string ShinLeft = "Shin_L";
        public const string ShinRight = "Shin_R";
        public const string FootLeft = "Foot_L";
        public const string FootRight = "Foot_R";
        /// <summary>Follow-through parts (springs on turns and footfalls): tail chain <c>Tail</c> → <c>Tail_1</c>.. → <c>Stinger</c>, spider abdomen, scorpion claws.</summary>
        public const string Tail = "Tail";
        public const string TailSegmentPrefix = "Tail_";
        public const string Stinger = "Stinger";
        public const string Abdomen = "Abdomen";
        public const string ClawLeft = "Claw_L";
        public const string ClawRight = "Claw_R";
        public const string WingLeft = "Wing_L";
        public const string WingRight = "Wing_R";
        /// <summary>Bat swarm: the pivot every bat orbits (bats are "Bat_0".."Bat_n" under it).</summary>
        public const string Swarm = "Swarm";
        public const string BatPrefix = "Bat_";
        public const string Cape = "Cape";
        public const string Weapon = "Weapon";
        public const string EarLeft = "Ear_L";
        public const string EarRight = "Ear_R";
        /// <summary>Ruin guardian: the ring of floating rune tablets.</summary>
        public const string Runes = "Runes";
        /// <summary>Ancient dweller: the pile of stolen light hugged against its chest.</summary>
        public const string StolenLight = "StolenLight";
        /// <summary>Werewolf: chest/shoulders/head pivot at the hips; pitches up when it rears to strike (front legs ride it).</summary>
        public const string Torso = "Torso";
        /// <summary>Lower jaw / lower beak (werewolf, potoo): opens during the attack windup.</summary>
        public const string Jaw = "Jaw";
        /// <summary>Firefly swarm: the glowing units are "Firefly_0".."Firefly_n" under <see cref="Swarm"/>, each with a "Lantern".</summary>
        public const string FireflyPrefix = "Firefly_";
        public const string Lantern = "Lantern";
        /// <summary>Potoo: the dead branch it ambushes from (it takes it along, of course).</summary>
        public const string Branch = "Branch";
    }

    /// <summary>
    /// Credit-free, stylized low-poly models for the enemies that have no Meshy model (bat swarm,
    /// stone golem, warlord, ruin guardian, ancient dweller) and for the animals whose Meshy model was a
    /// single static mesh (wolf, alpha wolf, spider, scorpion: replaced so their legs can step),
    /// generated in code from separate named parts so they can be animated without a skeleton. Each kind is built once and cached (meshes and
    /// the palette material are shared by every instance); <see cref="EnemyVisualSwapper"/> instantiates
    /// it when an <see cref="EnemyDefinition"/> has no <see cref="EnemyDefinition.VisualPrefab"/>, and
    /// the editor menu "Rapadura/Enemies/Build Procedural Enemy Models" bakes the same blueprints to
    /// prefab/mesh assets. Models face +Z with the pivot at the feet (y = 0), in metres.
    /// </summary>
    public static class ProceduralEnemyModels
    {
        /// <summary>Mobile triangle budgets.</summary>
        public const int CommonTriangleBudget = 3000;
        public const int BossTriangleBudget = 8000;

        private static readonly Dictionary<ProceduralEnemyKind, ProceduralEnemyBlueprint> Cache =
            new Dictionary<ProceduralEnemyKind, ProceduralEnemyBlueprint>();

        private static Material _sharedMaterial;

        /// <summary>Enemy asset id (EnemyDefinition asset name) → model kind.</summary>
        public static readonly IReadOnlyDictionary<string, ProceduralEnemyKind> KindsById = new Dictionary<string, ProceduralEnemyKind>
        {
            { "enemy_cave_bat_swarm", ProceduralEnemyKind.BatSwarm },
            { "enemy_mountain_stone_golem", ProceduralEnemyKind.StoneGolem },
            { "enemy_mountain_warlord", ProceduralEnemyKind.Warlord },
            { "enemy_desert_ruin_guardian", ProceduralEnemyKind.RuinGuardian },
            { "enemy_cave_dweller", ProceduralEnemyKind.AncientDweller },
            { "enemy_forest_wolf", ProceduralEnemyKind.Wolf },
            { "enemy_forest_alpha_wolf", ProceduralEnemyKind.AlphaWolf },
            { "enemy_forest_spider", ProceduralEnemyKind.Spider },
            { "enemy_desert_scorpion", ProceduralEnemyKind.Scorpion },
            { "enemy_forest_werewolf", ProceduralEnemyKind.Werewolf },
            { "enemy_cave_vampire_bat", ProceduralEnemyKind.VampireBat },
            { "enemy_forest_shroud_owl", ProceduralEnemyKind.ShroudOwl },
            { "enemy_desert_firefly_swarm", ProceduralEnemyKind.FireflySwarm },
            { "enemy_forest_peccary", ProceduralEnemyKind.Peccary },
            { "enemy_mountain_potoo", ProceduralEnemyKind.Potoo },
        };

        public static readonly ProceduralEnemyKind[] AllKinds =
        {
            ProceduralEnemyKind.BatSwarm, ProceduralEnemyKind.StoneGolem, ProceduralEnemyKind.Warlord,
            ProceduralEnemyKind.RuinGuardian, ProceduralEnemyKind.AncientDweller,
            ProceduralEnemyKind.Wolf, ProceduralEnemyKind.AlphaWolf, ProceduralEnemyKind.Spider, ProceduralEnemyKind.Scorpion,
            ProceduralEnemyKind.Werewolf, ProceduralEnemyKind.VampireBat, ProceduralEnemyKind.ShroudOwl,
            ProceduralEnemyKind.FireflySwarm, ProceduralEnemyKind.Peccary, ProceduralEnemyKind.Potoo,
        };

        /// <summary>The night-only species models (Docs/DayNight.md): they also get a <see cref="Rapadura.Gameplay.Night.NightCreatureVoice"/>.</summary>
        public static bool IsNightSpecies(ProceduralEnemyKind kind) => kind >= ProceduralEnemyKind.Werewolf && kind <= ProceduralEnemyKind.Potoo;

        public static ProceduralEnemyKind KindFor(string enemyId)
        {
            return !string.IsNullOrEmpty(enemyId) && KindsById.TryGetValue(enemyId, out ProceduralEnemyKind kind)
                ? kind
                : ProceduralEnemyKind.None;
        }

        public static ProceduralEnemyKind KindFor(EnemyDefinition definition) => KindFor(DefeatedEnemyRegistry.GetId(definition));

        public static bool IsBoss(ProceduralEnemyKind kind)
        {
            return kind == ProceduralEnemyKind.Warlord || kind == ProceduralEnemyKind.RuinGuardian || kind == ProceduralEnemyKind.AncientDweller
                || kind == ProceduralEnemyKind.AlphaWolf;
        }

        public static int TriangleBudget(ProceduralEnemyKind kind) => IsBoss(kind) ? BossTriangleBudget : CommonTriangleBudget;

        /// <summary>
        /// A freshly built blueprint (not cached: the caller owns its meshes). Deterministic: two
        /// calls produce identical meshes.
        /// </summary>
        public static ProceduralEnemyBlueprint CreateBlueprint(ProceduralEnemyKind kind)
        {
            var blueprint = new ProceduralEnemyBlueprint(kind, IsBoss(kind));
            switch (kind)
            {
                case ProceduralEnemyKind.BatSwarm:
                    BatSwarmRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.StoneGolem:
                    StoneGolemRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Warlord:
                    WarlordRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.RuinGuardian:
                    RuinGuardianRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.AncientDweller:
                    AncientDwellerRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Wolf:
                    WolfRecipe.Build(blueprint, false);
                    break;
                case ProceduralEnemyKind.AlphaWolf:
                    WolfRecipe.Build(blueprint, true);
                    break;
                case ProceduralEnemyKind.Spider:
                    SpiderRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Scorpion:
                    ScorpionRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Werewolf:
                    WerewolfRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.VampireBat:
                    VampireBatRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.ShroudOwl:
                    ShroudOwlRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.FireflySwarm:
                    FireflySwarmRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Peccary:
                    PeccaryRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Potoo:
                    PotooRecipe.Build(blueprint);
                    break;
            }

            return blueprint;
        }

        /// <summary>The cached, shared blueprint of <paramref name="kind"/> (built on first use).</summary>
        public static ProceduralEnemyBlueprint GetBlueprint(ProceduralEnemyKind kind)
        {
            if (!Cache.TryGetValue(kind, out ProceduralEnemyBlueprint blueprint) || !IsAlive(blueprint))
            {
                blueprint = CreateBlueprint(kind);
                Cache[kind] = blueprint;
            }

            return blueprint;
        }

        /// <summary>The one material every procedural enemy shares (palette albedo + glow emission).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_sharedMaterial == null)
                {
                    _sharedMaterial = EnemyModelPalette.CreateMaterial(EnemyModelPalette.CreateTexture(), EnemyModelPalette.CreateEmissionTexture());
                    _sharedMaterial.hideFlags = HideFlags.DontSave;
                }

                return _sharedMaterial;
            }
        }

        /// <summary>Instantiates the cached model of <paramref name="kind"/> (shared meshes + material) under <paramref name="parent"/>.</summary>
        public static GameObject Instantiate(ProceduralEnemyKind kind, Transform parent)
        {
            return kind == ProceduralEnemyKind.None ? null : Instantiate(GetBlueprint(kind), SharedMaterial, parent);
        }

        /// <summary>Builds the part hierarchy of <paramref name="blueprint"/> (root named "EnemyVisual", with a <see cref="ProceduralEnemyRig"/>).</summary>
        public static GameObject Instantiate(ProceduralEnemyBlueprint blueprint, Material material, Transform parent)
        {
            var root = new GameObject(EnemyVisualSwapper.VisualName);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            var transforms = new Transform[blueprint.Parts.Count];
            for (int i = 0; i < blueprint.Parts.Count; i++)
            {
                ProceduralEnemyBlueprint.Part part = blueprint.Parts[i];
                var node = new GameObject(part.Name);
                Transform t = node.transform;
                t.SetParent(part.Parent >= 0 ? transforms[part.Parent] : root.transform, false);
                t.SetLocalPositionAndRotation(part.Position, part.Rotation);
                t.localScale = part.Scale;
                transforms[i] = t;

                if (part.Mesh != null)
                {
                    node.AddComponent<MeshFilter>().sharedMesh = part.Mesh;
                    node.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
            }

            root.AddComponent<ProceduralEnemyRig>().Configure(blueprint.Kind);
            if (IsNightSpecies(blueprint.Kind))
            {
                // Arrival call + attack cue through the procedural synth (pool-safe: re-armed on every enable).
                root.AddComponent<Rapadura.Gameplay.Night.NightCreatureVoice>().Configure(blueprint.Kind);
            }

            return root;
        }

        private static bool IsAlive(ProceduralEnemyBlueprint blueprint)
        {
            if (blueprint == null || blueprint.Meshes.Count == 0)
            {
                return false;
            }

            foreach (Mesh mesh in blueprint.Meshes)
            {
                if (mesh == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
