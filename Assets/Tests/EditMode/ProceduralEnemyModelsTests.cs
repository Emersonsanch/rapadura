using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the code-built low-poly enemy models (<see cref="ProceduralEnemyModels"/>):
    /// determinism, the part names the animators look for (including the jointed Leg_ → Shin_ → Foot_
    /// chains the stepping legs need), mobile triangle budgets, sane bounds for ground seating, and every
    /// shipped model-less enemy ending up with a non-empty visual.
    /// </summary>
    public class ProceduralEnemyModelsTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<ProceduralEnemyBlueprint> _blueprints = new List<ProceduralEnemyBlueprint>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            foreach (ProceduralEnemyBlueprint blueprint in _blueprints)
            {
                blueprint.DestroyMeshes();
            }

            _created.Clear();
            _blueprints.Clear();
        }

        private ProceduralEnemyBlueprint Fresh(ProceduralEnemyKind kind)
        {
            ProceduralEnemyBlueprint blueprint = ProceduralEnemyModels.CreateBlueprint(kind);
            _blueprints.Add(blueprint);
            return blueprint;
        }

        private static IEnumerable<ProceduralEnemyKind> Kinds() => ProceduralEnemyModels.AllKinds;

        // Declared before RequiredParts: static fields initialise in text order (after it, the table got null).
        private static readonly string[] WolfParts =
        {
            ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight, ProceduralEnemyParts.Tail, "Tail_1",
            "Leg_FL", "Shin_FL", "Foot_FL", "Leg_FR", "Shin_FR", "Foot_FR", "Leg_HL", "Shin_HL", "Foot_HL", "Leg_HR", "Shin_HR", "Foot_HR",
        };

        private static readonly Dictionary<ProceduralEnemyKind, string[]> RequiredParts = new Dictionary<ProceduralEnemyKind, string[]>
        {
            { ProceduralEnemyKind.BatSwarm, new[] { ProceduralEnemyParts.Swarm, "Bat_0", ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight } },
            { ProceduralEnemyKind.StoneGolem, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.ArmLeft, ProceduralEnemyParts.ArmRight,
                ProceduralEnemyParts.FistLeft, ProceduralEnemyParts.FistRight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.Warlord, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Cape, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.RuinGuardian, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Runes, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.FistLeft, ProceduralEnemyParts.FistRight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.AncientDweller, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight,
                ProceduralEnemyParts.ArmLeft, ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.StolenLight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.Wolf, WolfParts },
            { ProceduralEnemyKind.AlphaWolf, WolfParts },
            { ProceduralEnemyKind.Spider, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Abdomen, "Leg_L0", "Shin_L0", "Foot_L0", "Leg_R3", "Shin_R3", "Foot_R3" } },
            { ProceduralEnemyKind.Scorpion, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.ClawLeft, ProceduralEnemyParts.ClawRight, ProceduralEnemyParts.Tail,
                "Tail_1", "Tail_2", "Tail_3", ProceduralEnemyParts.Stinger, "Leg_L0", "Shin_L0", "Foot_L0", "Leg_R3", "Shin_R3", "Foot_R3" } },
            { ProceduralEnemyKind.Werewolf, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Torso, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw,
                ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight, ProceduralEnemyParts.Tail, "Tail_1", WerewolfRecipe.ShoulderLeft, WerewolfRecipe.ShoulderRight,
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_FR", "Shin_FR", "Foot_FR", "Leg_HL", "Shin_HL", "Foot_HL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.VampireBat, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight } },
            { ProceduralEnemyKind.ShroudOwl, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight,
                ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.FireflySwarm, new[] { ProceduralEnemyParts.Swarm, "Firefly_0", ProceduralEnemyParts.Lantern } },
            { ProceduralEnemyKind.Peccary, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight,
                ProceduralEnemyParts.Tail, "Leg_FL", "Shin_FL", "Foot_FL", "Leg_FR", "Shin_FR", "Foot_FR", "Leg_HL", "Shin_HL", "Foot_HL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.Potoo, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Branch,
                ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight, ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.EmberVulture, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight,
                ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.BrokenLantern, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Lantern } },
            { ProceduralEnemyKind.CrestedHawk, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight,
                ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.LanternFish, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Lantern, ProceduralEnemyParts.WingLeft,
                ProceduralEnemyParts.WingRight, ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.FloatingSkull, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Wisp } },
            { ProceduralEnemyKind.SkeletonWarrior, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.Shield, ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.SkeletonArcher, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.BoneSorcerer, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.Weapon, ProceduralEnemyParts.Orb, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.BoneColossus, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Core,
                ProceduralEnemyParts.RibsLeft, ProceduralEnemyParts.RibsRight, ProceduralEnemyParts.Halo, "HaloSkull_0", "HaloSkull_5", ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight,
                ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight, ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.Jackal, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight,
                ProceduralEnemyParts.Tail, "Tail_1", "Leg_FL", "Shin_FL", "Foot_FL", "Leg_FR", "Shin_FR", "Foot_FR", "Leg_HL", "Shin_HL", "Foot_HL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.SandSnake, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail, "Tail_1", "Tail_2", "Tail_3" } },
            { ProceduralEnemyKind.CrownedOrc, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.ArmLeft, ProceduralEnemyParts.ArmRight,
                ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight, ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight,
                ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.RollingStone, new[] { ProceduralEnemyParts.Body } },
            { ProceduralEnemyKind.IllWindMist, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Swarm, "Puff_0", "Puff_5", ProceduralEnemyParts.Wisp } },
            { ProceduralEnemyKind.Baboon, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail, "Tail_1",
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_FR", "Shin_FR", "Foot_FR", "Leg_HL", "Shin_HL", "Foot_HL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.CaveOoze, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.StolenLight } },
            { ProceduralEnemyKind.DarkCrow, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight,
                ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.ThiefGull, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight,
                ProceduralEnemyParts.Tail } },
            { ProceduralEnemyKind.CoconutCrab, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.ClawLeft, ProceduralEnemyParts.ClawRight,
                "Leg_L0", "Shin_L0", "Foot_L0", "Leg_R3", "Shin_R3", "Foot_R3" } },
            { ProceduralEnemyKind.MangroveCrocodile, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Tail,
                "Tail_1", "Tail_2", "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.GrassLion, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Tail, "Tail_1",
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.DustRhino, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail,
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.ThiefMonkey, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail, "Tail_1", "Tail_2",
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.ThiefOtter, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail, "Tail_1",
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.Python, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail, "Tail_1", "Tail_2", "Tail_3" } },
            { ProceduralEnemyKind.DimmedTroll, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.StolenLight, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight, ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight,
                ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.BrownBear, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.EarLeft,
                ProceduralEnemyParts.EarRight, "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.WildYak, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail,
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.Irbis, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Jaw, ProceduralEnemyParts.Tail, "Tail_1", "Tail_2",
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
            { ProceduralEnemyKind.Almas, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.ArmLeft, ProceduralEnemyParts.ArmRight,
                ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight, ProceduralEnemyParts.ShinLeft, ProceduralEnemyParts.ShinRight,
                ProceduralEnemyParts.FootLeft, ProceduralEnemyParts.FootRight } },
            { ProceduralEnemyKind.GiantMarmot, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Tail, "Tail_1",
                "Leg_FL", "Shin_FL", "Foot_FL", "Leg_HR", "Shin_HR", "Foot_HR" } },
        };

        /// <summary>Legs per kind; every one a Leg_ (hip) → Shin_ (knee) → Foot_ chain for ProceduralLegLocomotion.</summary>
        private static readonly Dictionary<ProceduralEnemyKind, int> LegCounts = new Dictionary<ProceduralEnemyKind, int>
        {
            { ProceduralEnemyKind.BatSwarm, 0 }, { ProceduralEnemyKind.StoneGolem, 2 }, { ProceduralEnemyKind.Warlord, 2 },
            { ProceduralEnemyKind.RuinGuardian, 2 }, { ProceduralEnemyKind.AncientDweller, 2 }, { ProceduralEnemyKind.Wolf, 4 },
            { ProceduralEnemyKind.AlphaWolf, 4 }, { ProceduralEnemyKind.Spider, 8 }, { ProceduralEnemyKind.Scorpion, 8 },
            { ProceduralEnemyKind.Werewolf, 4 }, { ProceduralEnemyKind.VampireBat, 0 }, { ProceduralEnemyKind.ShroudOwl, 0 },
            { ProceduralEnemyKind.FireflySwarm, 0 }, { ProceduralEnemyKind.Peccary, 4 }, { ProceduralEnemyKind.Potoo, 0 },
            { ProceduralEnemyKind.EmberVulture, 0 }, { ProceduralEnemyKind.BrokenLantern, 0 }, { ProceduralEnemyKind.CrestedHawk, 0 },
            { ProceduralEnemyKind.LanternFish, 0 },
            { ProceduralEnemyKind.FloatingSkull, 0 }, { ProceduralEnemyKind.SkeletonWarrior, 2 }, { ProceduralEnemyKind.SkeletonArcher, 2 },
            { ProceduralEnemyKind.BoneSorcerer, 2 }, { ProceduralEnemyKind.BoneColossus, 2 },
            { ProceduralEnemyKind.Jackal, 4 }, { ProceduralEnemyKind.SandSnake, 0 }, { ProceduralEnemyKind.CrownedOrc, 2 },
            { ProceduralEnemyKind.RollingStone, 0 }, { ProceduralEnemyKind.IllWindMist, 0 }, { ProceduralEnemyKind.Baboon, 4 },
            { ProceduralEnemyKind.CaveOoze, 0 }, { ProceduralEnemyKind.DarkCrow, 0 }, { ProceduralEnemyKind.ThiefGull, 0 },
            { ProceduralEnemyKind.CoconutCrab, 8 },
            { ProceduralEnemyKind.MangroveCrocodile, 4 }, { ProceduralEnemyKind.GrassLion, 4 }, { ProceduralEnemyKind.DustRhino, 4 },
            { ProceduralEnemyKind.ThiefMonkey, 4 }, { ProceduralEnemyKind.ThiefOtter, 4 }, { ProceduralEnemyKind.Python, 0 },
            { ProceduralEnemyKind.DimmedTroll, 2 }, { ProceduralEnemyKind.BrownBear, 4 }, { ProceduralEnemyKind.WildYak, 4 },
            { ProceduralEnemyKind.Irbis, 4 }, { ProceduralEnemyKind.Almas, 2 }, { ProceduralEnemyKind.GiantMarmot, 4 },
        };

        [Test]
        public void EveryWalker_HasJointedLegChains([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            int legs = 0;
            foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith(ProceduralEnemyParts.LegPrefix))
                {
                    continue;
                }

                legs++;
                string id = t.name.Substring(ProceduralEnemyParts.LegPrefix.Length);
                Transform shin = t.Find(ProceduralEnemyParts.ShinPrefix + id);
                Assert.IsNotNull(shin, $"{kind}: {t.name} has no knee");
                Transform foot = shin.Find(ProceduralEnemyParts.FootPrefix + id);
                Assert.IsNotNull(foot, $"{kind}: {t.name} has no foot");
                Assert.Greater(t.position.y, foot.position.y, $"{kind}: {t.name} hip above its foot");
            }

            Assert.AreEqual(LegCounts[kind], legs, kind.ToString());
        }

        [Test]
        public void EveryKind_IsDeterministic([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            ProceduralEnemyBlueprint a = Fresh(kind);
            ProceduralEnemyBlueprint b = Fresh(kind);

            Assert.AreEqual(a.Parts.Count, b.Parts.Count);
            Assert.AreEqual(a.Meshes.Count, b.Meshes.Count);
            for (int i = 0; i < a.Parts.Count; i++)
            {
                Assert.AreEqual(a.Parts[i].Name, b.Parts[i].Name);
                Assert.AreEqual(a.Parts[i].Parent, b.Parts[i].Parent);
                Assert.AreEqual(a.Parts[i].Position, b.Parts[i].Position);
            }

            for (int m = 0; m < a.Meshes.Count; m++)
            {
                CollectionAssert.AreEqual(a.Meshes[m].vertices, b.Meshes[m].vertices, $"{kind} mesh {a.Meshes[m].name}");
                CollectionAssert.AreEqual(a.Meshes[m].triangles, b.Meshes[m].triangles);
                CollectionAssert.AreEqual(a.Meshes[m].uv, b.Meshes[m].uv);
            }
        }

        [Test]
        public void EveryKind_HasTheNamedPartsTheAnimatorsUse([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            Assert.AreEqual(EnemyVisualSwapper.VisualName, visual.name);

            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Assert.IsNotNull(rig);
            Assert.AreEqual(kind, rig.Kind);
            foreach (string part in RequiredParts[kind])
            {
                Assert.IsNotNull(rig.GetPart(part), $"{kind}: missing part '{part}'");
            }

            // Legs are found by EnemyProceduralAnimator's own naming rule.
            if (kind != ProceduralEnemyKind.BatSwarm)
            {
                Assert.IsTrue(EnemyProceduralAnimator.IsLegName(ProceduralEnemyParts.LegLeft));
                Assert.IsTrue(EnemyProceduralAnimator.IsLegName(ProceduralEnemyParts.LegRight));
            }

            // No other part may accidentally match the leg rule (it would be twitched like a leg).
            foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
            {
                if (t != visual.transform && !t.name.StartsWith(ProceduralEnemyParts.LegPrefix))
                {
                    Assert.IsFalse(EnemyProceduralAnimator.IsLegName(t.name), $"{kind}: '{t.name}' reads as a leg");
                }
            }

            Assert.IsEmpty(visual.GetComponentsInChildren<Collider>(true), "procedural models must not add colliders");
        }

        [Test]
        public void BatSwarm_HasFiveToSevenBats_EachWithTwoWings()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.BatSwarm, null);
            _created.Add(visual);
            Transform swarm = visual.GetComponent<ProceduralEnemyRig>().GetPart(ProceduralEnemyParts.Swarm);

            Assert.That(swarm.childCount, Is.InRange(5, 7));
            foreach (Transform bat in swarm)
            {
                StringAssert.StartsWith(ProceduralEnemyParts.BatPrefix, bat.name);
                Assert.IsNotNull(bat.GetComponent<MeshFilter>(), bat.name);
                Assert.IsNotNull(bat.Find(ProceduralEnemyParts.WingLeft), bat.name);
                Assert.IsNotNull(bat.Find(ProceduralEnemyParts.WingRight), bat.name);
            }
        }

        [Test]
        public void EveryKind_StaysWithinItsMobileTriangleBudget([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            ProceduralEnemyBlueprint blueprint = Fresh(kind);
            int budget = ProceduralEnemyModels.TriangleBudget(kind);
            Assert.AreEqual(ProceduralEnemyModels.IsBoss(kind) ? 8000 : 3000, budget);
            Assert.Greater(blueprint.TriangleCount, 200, $"{kind}: suspiciously empty");
            Assert.Less(blueprint.TriangleCount, budget, $"{kind}: {blueprint.TriangleCount} triangles");
            TestContext.WriteLine($"{kind}: {blueprint.TriangleCount} triangles, {blueprint.Meshes.Count} meshes, {blueprint.Parts.Count} parts");
        }

        [Test]
        public void EveryKind_SharesOnePaletteMaterial_AndCachedMeshes()
        {
            var materials = new HashSet<Material>();
            foreach (ProceduralEnemyKind kind in ProceduralEnemyModels.AllKinds)
            {
                GameObject first = ProceduralEnemyModels.Instantiate(kind, null);
                GameObject second = ProceduralEnemyModels.Instantiate(kind, null);
                _created.Add(first);
                _created.Add(second);

                MeshFilter[] a = first.GetComponentsInChildren<MeshFilter>();
                MeshFilter[] b = second.GetComponentsInChildren<MeshFilter>();
                Assert.AreEqual(a.Length, b.Length);
                for (int i = 0; i < a.Length; i++)
                {
                    Assert.AreSame(a[i].sharedMesh, b[i].sharedMesh, $"{kind}: meshes must be shared, not rebuilt per instance");
                }

                foreach (MeshRenderer renderer in first.GetComponentsInChildren<MeshRenderer>())
                {
                    materials.Add(renderer.sharedMaterial);
                }
            }

            Assert.AreEqual(1, materials.Count, "all procedural enemies draw with one shared palette material");
        }

        [Test]
        public void EveryKind_HasSaneBounds_FeetNearThePivot_BossesBigger([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            var root = new GameObject("root");
            _created.Add(root);
            visual.transform.SetParent(root.transform, false);

            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds));
            bool floatingSwarm = kind == ProceduralEnemyKind.BatSwarm || kind == ProceduralEnemyKind.FireflySwarm;
            Assert.That(bounds.min.y, Is.InRange(-0.15f, floatingSwarm ? 0.9f : 0.15f), $"{kind}: feet at {bounds.min.y}");
            Assert.That(bounds.size.y, Is.InRange(0.6f, 4f), $"{kind}: height {bounds.size.y}");
            bool lean = kind == ProceduralEnemyKind.Wolf || kind == ProceduralEnemyKind.AlphaWolf || kind == ProceduralEnemyKind.Peccary
                || kind == ProceduralEnemyKind.Jackal || kind == ProceduralEnemyKind.Baboon
                || kind == ProceduralEnemyKind.ThiefMonkey || kind == ProceduralEnemyKind.ThiefOtter || kind == ProceduralEnemyKind.Irbis
                || kind == ProceduralEnemyKind.GiantMarmot || kind == ProceduralEnemyKind.GrassLion;
            Assert.That(bounds.size.x, Is.InRange(lean ? 0.3f : 0.5f, 3.5f), $"{kind}: width {bounds.size.x}");
            Assert.That(bounds.size.z, Is.InRange(0.4f, 3.5f), $"{kind}: depth {bounds.size.z}");
            Assert.That(Mathf.Abs(bounds.center.x), Is.LessThan(0.3f), $"{kind}: model must be centred on the pivot");

            if (kind == ProceduralEnemyKind.AlphaWolf)
            {
                // A four-legged boss reads by bulk, not height: clearly bigger than a pack wolf.
                Assert.Greater(bounds.size.y, 1.25f, "alpha wolf height");
            }
            else if (ProceduralEnemyModels.IsBoss(kind))
            {
                Assert.Greater(bounds.size.y, 2.4f, $"{kind}: a boss must stand taller than the 2 m capsule");
            }
        }

        [Test]
        public void EveryKind_UsesGlowSwatches()
        {
            foreach (ProceduralEnemyKind kind in ProceduralEnemyModels.AllKinds)
            {
                bool glows = false;
                foreach (Mesh mesh in ProceduralEnemyModels.GetBlueprint(kind).Meshes)
                {
                    foreach (Vector2 uv in mesh.uv)
                    {
                        glows |= EnemyModelPalette.IsGlow(EnemyModelPalette.SwatchAt(uv));
                    }
                }

                Assert.IsTrue(glows, $"{kind}: no glowing eyes/cracks/runes/crystals");
            }
        }

        [Test]
        public void Rig_Tick_MovesParts_AndRespectsZeroMotion()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.BatSwarm, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform wing = rig.GetPart(ProceduralEnemyParts.WingRight);
            Quaternion rest = wing.localRotation;

            rig.Tick(0.11f);
            Assert.AreNotEqual(rest, wing.localRotation, "wings flap");

            GameObject golem = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.StoneGolem, null);
            _created.Add(golem);
            var golemRig = golem.GetComponent<ProceduralEnemyRig>();
            Transform arm = golemRig.GetPart(ProceduralEnemyParts.ArmLeft);
            Quaternion armRest = arm.localRotation;
            golemRig.Tick(0.5f, 0f);
            Assert.AreEqual(armRest, arm.localRotation, "zero motion scale keeps the rest pose");

            foreach (ProceduralEnemyKind kind in ProceduralEnemyModels.AllKinds)
            {
                GameObject model = ProceduralEnemyModels.Instantiate(kind, null);
                _created.Add(model);
                Assert.DoesNotThrow(() => model.GetComponent<ProceduralEnemyRig>().Tick(0.016f), kind.ToString());
            }
        }

        [Test]
        public void KindFor_MapsTheProceduralEnemies_AndTheJointedAnimals_AndNothingElse()
        {
            Assert.AreEqual(ProceduralEnemyKind.BatSwarm, ProceduralEnemyModels.KindFor("enemy_cave_bat_swarm"));
            Assert.AreEqual(ProceduralEnemyKind.AncientDweller, ProceduralEnemyModels.KindFor("enemy_cave_dweller"));
            Assert.AreEqual(ProceduralEnemyKind.RuinGuardian, ProceduralEnemyModels.KindFor("enemy_desert_ruin_guardian"));
            Assert.AreEqual(ProceduralEnemyKind.StoneGolem, ProceduralEnemyModels.KindFor("enemy_mountain_stone_golem"));
            Assert.AreEqual(ProceduralEnemyKind.Warlord, ProceduralEnemyModels.KindFor("enemy_mountain_warlord"));
            Assert.AreEqual(ProceduralEnemyKind.Wolf, ProceduralEnemyModels.KindFor("enemy_forest_wolf"));
            Assert.AreEqual(ProceduralEnemyKind.AlphaWolf, ProceduralEnemyModels.KindFor("enemy_forest_alpha_wolf"));
            Assert.AreEqual(ProceduralEnemyKind.Spider, ProceduralEnemyModels.KindFor("enemy_forest_spider"));
            Assert.AreEqual(ProceduralEnemyKind.Scorpion, ProceduralEnemyModels.KindFor("enemy_desert_scorpion"));
            Assert.AreEqual(ProceduralEnemyKind.Werewolf, ProceduralEnemyModels.KindFor("enemy_forest_werewolf"));
            Assert.AreEqual(ProceduralEnemyKind.VampireBat, ProceduralEnemyModels.KindFor("enemy_cave_vampire_bat"));
            Assert.AreEqual(ProceduralEnemyKind.ShroudOwl, ProceduralEnemyModels.KindFor("enemy_forest_shroud_owl"));
            Assert.AreEqual(ProceduralEnemyKind.FireflySwarm, ProceduralEnemyModels.KindFor("enemy_desert_firefly_swarm"));
            Assert.AreEqual(ProceduralEnemyKind.Peccary, ProceduralEnemyModels.KindFor("enemy_forest_peccary"));
            Assert.AreEqual(ProceduralEnemyKind.Potoo, ProceduralEnemyModels.KindFor("enemy_mountain_potoo"));
            Assert.AreEqual(ProceduralEnemyKind.EmberVulture, ProceduralEnemyModels.KindFor("enemy_desert_ember_vulture"));
            Assert.AreEqual(ProceduralEnemyKind.BrokenLantern, ProceduralEnemyModels.KindFor("enemy_desert_broken_lantern"));
            Assert.AreEqual(ProceduralEnemyKind.CrestedHawk, ProceduralEnemyModels.KindFor("enemy_mountain_crested_hawk"));
            Assert.AreEqual(ProceduralEnemyKind.LanternFish, ProceduralEnemyModels.KindFor("enemy_cave_lantern_fish"));
            Assert.AreEqual(ProceduralEnemyKind.FloatingSkull, ProceduralEnemyModels.KindFor("enemy_floating_skull"));
            Assert.AreEqual(ProceduralEnemyKind.SkeletonWarrior, ProceduralEnemyModels.KindFor("enemy_skeleton_warrior"));
            Assert.AreEqual(ProceduralEnemyKind.SkeletonArcher, ProceduralEnemyModels.KindFor("enemy_skeleton_archer"));
            Assert.AreEqual(ProceduralEnemyKind.BoneSorcerer, ProceduralEnemyModels.KindFor("enemy_bone_sorcerer"));
            Assert.AreEqual(ProceduralEnemyKind.BoneColossus, ProceduralEnemyModels.KindFor("boss_bone_colossus"));
            Assert.AreEqual(ProceduralEnemyKind.Jackal, ProceduralEnemyModels.KindFor("enemy_desert_jackal"));
            Assert.AreEqual(ProceduralEnemyKind.SandSnake, ProceduralEnemyModels.KindFor("enemy_desert_sand_snake"));
            Assert.AreEqual(ProceduralEnemyKind.CrownedOrc, ProceduralEnemyModels.KindFor("enemy_desert_crowned_orc"));
            Assert.AreEqual(ProceduralEnemyKind.RollingStone, ProceduralEnemyModels.KindFor("enemy_mountain_rolling_stone"));
            Assert.AreEqual(ProceduralEnemyKind.IllWindMist, ProceduralEnemyModels.KindFor("enemy_mountain_ill_wind_mist"));
            Assert.AreEqual(ProceduralEnemyKind.Baboon, ProceduralEnemyModels.KindFor("enemy_mountain_baboon"));
            Assert.AreEqual(ProceduralEnemyKind.CaveOoze, ProceduralEnemyModels.KindFor("enemy_cave_ooze"));
            Assert.AreEqual(ProceduralEnemyKind.DarkCrow, ProceduralEnemyModels.KindFor("enemy_cave_dark_crow"));
            Assert.AreEqual(ProceduralEnemyKind.ThiefGull, ProceduralEnemyModels.KindFor("enemy_forest_thief_gull"));
            Assert.AreEqual(ProceduralEnemyKind.CoconutCrab, ProceduralEnemyModels.KindFor("enemy_forest_coconut_crab"));
            Assert.AreEqual(ProceduralEnemyKind.MangroveCrocodile, ProceduralEnemyModels.KindFor("enemy_forest_mangrove_crocodile"));
            Assert.AreEqual(ProceduralEnemyKind.GrassLion, ProceduralEnemyModels.KindFor("enemy_forest_grass_lion"));
            Assert.AreEqual(ProceduralEnemyKind.DustRhino, ProceduralEnemyModels.KindFor("enemy_forest_dust_rhino"));
            Assert.AreEqual(ProceduralEnemyKind.ThiefMonkey, ProceduralEnemyModels.KindFor("enemy_forest_thief_monkey"));
            Assert.AreEqual(ProceduralEnemyKind.ThiefOtter, ProceduralEnemyModels.KindFor("enemy_forest_thief_otter"));
            Assert.AreEqual(ProceduralEnemyKind.Python, ProceduralEnemyModels.KindFor("enemy_forest_dimmed_python"));
            Assert.AreEqual(ProceduralEnemyKind.DimmedTroll, ProceduralEnemyModels.KindFor("enemy_forest_dimmed_troll"));
            Assert.AreEqual(ProceduralEnemyKind.BrownBear, ProceduralEnemyModels.KindFor("enemy_mountain_brown_bear"));
            Assert.AreEqual(ProceduralEnemyKind.WildYak, ProceduralEnemyModels.KindFor("enemy_mountain_wild_yak"));
            Assert.AreEqual(ProceduralEnemyKind.Irbis, ProceduralEnemyModels.KindFor("enemy_mountain_irbis"));
            Assert.AreEqual(ProceduralEnemyKind.Almas, ProceduralEnemyModels.KindFor("enemy_mountain_almas"));
            Assert.AreEqual(ProceduralEnemyKind.GiantMarmot, ProceduralEnemyModels.KindFor("enemy_mountain_giant_marmot"));
            Assert.AreEqual(ProceduralEnemyKind.None, ProceduralEnemyModels.KindFor("enemy_unknown_thing"));
            Assert.AreEqual(ProceduralEnemyKind.None, ProceduralEnemyModels.KindFor((string)null));

            foreach (KeyValuePair<string, ProceduralEnemyKind> entry in ProceduralEnemyModels.KindsById)
            {
                // Open-world enemies under Resources/Enemies, the dungeon guardians under their dungeon's folder.
                var definition = Resources.Load<EnemyDefinition>("Enemies/" + entry.Key)
                    ?? Resources.Load<EnemyDefinition>(Rapadura.Gameplay.Dungeon.DungeonRules.EnemyResourcePath(entry.Key));
                Assert.IsNotNull(definition, entry.Key);
                Assert.AreEqual(definition.IsBoss, ProceduralEnemyModels.IsBoss(entry.Value), $"{entry.Key}: boss flag vs model budget");
            }
        }

        [Test]
        public void EveryShippedEnemy_EndsUpWithANonEmptyVisual()
        {
            EnemyDefinition[] definitions = Resources.LoadAll<EnemyDefinition>("Enemies");
            Assert.IsNotEmpty(definitions);
            foreach (EnemyDefinition definition in definitions)
            {
                var root = new GameObject("Enemy_" + definition.name);
                _created.Add(root);
                CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
                capsule.height = 2f;

                Transform visual = EnemyVisualSwapper.Apply(root, definition);
                Assert.IsNotNull(visual, definition.name);
                Assert.IsNull(visual.Find(EnemyVisualSwapper.SnoutName), $"{definition.name}: still a primitive placeholder");

                int triangles = 0;
                foreach (MeshFilter filter in visual.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh != null && filter.GetComponent<Renderer>() != null)
                    {
                        triangles += filter.sharedMesh.triangles.Length / 3;
                    }
                }

                foreach (SkinnedMeshRenderer skinned in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (skinned.sharedMesh != null)
                    {
                        triangles += skinned.sharedMesh.triangles.Length / 3;
                    }
                }

                Assert.Greater(triangles, 0, $"{definition.name}: empty visual");
            }
        }

        [Test]
        public void BossProceduralModel_IsNotBossTinted_WhenBakedAsDefinitionPrefab()
        {
            GameObject prefab = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.Warlord, null);
            _created.Add(prefab);
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.name = "enemy_mountain_warlord";
            _created.Add(definition);
            typeof(EnemyDefinition).GetField("_visualPrefab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, prefab);
            typeof(EnemyDefinition).GetField("_isBoss", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, true);
            typeof(EnemyDefinition).GetField("_visualScale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, 1f);
            typeof(EnemyDefinition).GetField("_visualLocalEulerAngles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, Vector3.zero);

            var root = new GameObject("root");
            _created.Add(root);
            root.AddComponent<CapsuleCollider>().height = 2f;
            Transform visual = EnemyVisualSwapper.Apply(root, definition);

            Assert.AreEqual(EnemyVisualSource.DefinitionPrefab, visual.GetComponent<EnemyVisualMarker>().Kind);
            foreach (MeshRenderer renderer in visual.GetComponentsInChildren<MeshRenderer>())
            {
                Assert.AreSame(ProceduralEnemyModels.SharedMaterial, renderer.sharedMaterial);
            }

            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual, out Bounds bounds));
            Assert.AreEqual(-1f, bounds.min.y, 0.01f);
        }
    }
}
