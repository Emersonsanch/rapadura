using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The nineteenth pass of beasts (bible §3.2.1): roles of the Costa das Monções (Região 5, Docs/Mapas100.md 27–30) and the Delta de Jhilpur
    /// (Região 6, 32–35) that the overworld still played with another region's beast. Same contract as <see cref="LentRoleBeastsTests"/>: each one
    /// keeps its stand-in's biome (the prefix) and numbers, so the economy of the regions does not move. The lionfish and the kongamato are the
    /// [distância] shooters, with the glob of light of the lantern fish and the feather of the crow they replace.
    /// </summary>
    public class CoastOwnBeastsTests
    {
        private const string Lionfish = "enemy_cave_dimmed_lionfish";
        private const string Kongamato = "enemy_cave_kongamato";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_mountain_kushite_ushabti", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.KushiteUshabti, 52, 58, "enemy_mountain_stone_golem"),
            new OverworldBeastsTests.Beast("enemy_desert_young_scorpion_king", BiomeType.Desert, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.YoungScorpionKing, 52, 58, "enemy_desert_scorpion"),
            new OverworldBeastsTests.Beast("enemy_desert_black_sand_jackal", BiomeType.Desert, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.BlackSandJackal, 52, 58, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_cave_mud_jellyfish", BiomeType.Caves, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.MudJellyfish, 55, 62, "enemy_cave_ooze"),
            new OverworldBeastsTests.Beast(Lionfish, BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.DimmedLionfish, 55, 62, "enemy_cave_lantern_fish"),
            new OverworldBeastsTests.Beast(Kongamato, BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.Kongamato, 64, 72, "enemy_cave_dark_crow"),
            new OverworldBeastsTests.Beast("enemy_cave_thief_octopus", BiomeType.Caves, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.ThiefOctopus, 60, 64, "enemy_cave_ooze"),
            new OverworldBeastsTests.Beast("enemy_forest_snare_root", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.SnareRoot, 61, 68, "enemy_forest_spider"),
            new OverworldBeastsTests.Beast("enemy_forest_ghost_heron", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.GhostHeron, 64, 74, "enemy_forest_thief_gull"),
            new OverworldBeastsTests.Beast("enemy_cave_smoke_dragonfly", BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SmokeDragonfly, 76, 82, "enemy_cave_bat_swarm"),
            new OverworldBeastsTests.Beast("enemy_mountain_smoke_bubble", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SmokeBubble, 70, 88, "enemy_mountain_ill_wind_mist"),
            new OverworldBeastsTests.Beast("enemy_cave_mud_child", BiomeType.Caves, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.MudChild, 70, 88, "enemy_cave_ooze"),
        };

        private static readonly Dictionary<string, EnemyProjectileKind> Shooters = new Dictionary<string, EnemyProjectileKind>
        {
            { Lionfish, EnemyProjectileKind.LureGlob },
            { Kongamato, EnemyProjectileKind.Feather },
        };

        private static IEnumerable<OverworldBeastsTests.Beast> All() => Beasts;

        private readonly List<Object> _created = new List<Object>();

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

            _created.Clear();
            BestiaryCatalog.Invalidate();
        }

        private static EnemyDefinition Load(string id)
        {
            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
            Assert.IsNotNull(definition, $"{id}: not in Resources/Enemies");
            return definition;
        }

        /// <summary>The commons that were in <paramref name="biome"/> before the beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(OverworldBestiaryTexts.EnemyIds).Concat(LateRegionBestiaryTexts.EnemyIds)
                .Concat(MingchengNartesBestiaryTexts.EnemyIds).Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds)
                .Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds)
                .Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds)
                .Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds).Concat(LentRoleBestiaryTexts.EnemyIds));
            BiomeDefinition home = BiomeRegistry.Get(biome);
            Assert.IsNotNull(home, biome.ToString());
            return home.Enemies.Where(e => e?.Enemy != null && !e.Enemy.IsBoss && !newer.Contains(e.Enemy.name)).Select(e => e.Enemy).ToList();
        }

        private GameObject Make(ProceduralEnemyKind kind, out ProceduralEnemyRig rig)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            rig = visual.GetComponent<ProceduralEnemyRig>();
            return visual;
        }

        private static void Windup(ProceduralEnemyRig rig)
        {
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }
        }

        [Test]
        public void Definitions_Load_AreCommons_WithTheSheetsArchetype([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            Assert.IsFalse(definition.IsBoss, beast.Id);
            Assert.AreEqual(beast.Archetype, definition.MotionArchetype, beast.Id);
            Assert.Greater(definition.MaxHealth, 0f, beast.Id);
            Assert.Greater(definition.AttackDamage, 0f, beast.Id);
            Assert.Greater(definition.AttackCooldown, 0f, beast.Id);
            Assert.Greater(definition.ExperienceReward, 0, beast.Id);
            Assert.Greater(definition.LoseTargetRadius, definition.DetectionRadius, $"{beast.Id}: keeps chasing a little past where it noticed the hero");
            Assert.GreaterOrEqual(definition.DetectionRadius, definition.AttackRange, beast.Id);
            Assert.IsNotNull(definition.LootItem, $"{beast.Id}: its base drop (the biome table adds the rest)");
            Assert.IsNull(definition.VisualPrefab, $"{beast.Id}: no prefab, so its own procedural model is drawn");
            string prefix = beast.Biome == BiomeType.Mountain ? "enemy_mountain_" : beast.Biome == BiomeType.Desert ? "enemy_desert_" : beast.Biome == BiomeType.Caves ? "enemy_cave_" : "enemy_forest_";
            StringAssert.StartsWith(prefix, beast.Id, "named after its home biome (enemy_<biome>_, BalanceInvariantsTests)");
        }

        [Test]
        public void EachOne_LivesInItsBiome_AtWeightZero_LikeTheOtherPlacedCommons([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            BiomeRegistry.Invalidate();
            EnemyDefinition definition = Load(beast.Id);
            BiomeDefinition home = BiomeRegistry.FindBiomeOf(definition);
            Assert.IsNotNull(home, $"{beast.Id}: in a biome table (loot + bestiary region)");
            Assert.AreEqual(beast.Biome, home.Type, beast.Id);
            Assert.AreEqual(0, home.Enemies.First(e => e.Enemy == definition).Weight, $"{beast.Id}: weight 0, never rolled by the world-map encounters");
        }

        [Test]
        public void Stats_FollowTheRegionTier_AndScaleIntoTheAreasLevelBand([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            List<EnemyDefinition> commons = OlderCommonsOf(beast.Biome);
            Assert.IsNotEmpty(commons, beast.Biome.ToString());

            Assert.IsTrue(commons.Any(c => c.ExperienceReward == definition.ExperienceReward), $"{beast.Id}: XP of its biome's commons");
            float hp = commons.Average(c => c.MaxHealth);
            float dps = commons.Average(c => c.AttackDamage / c.AttackCooldown);
            Assert.That(definition.MaxHealth / hp, Is.InRange(0.6f, 1.6f), $"{beast.Id}: health against its biome's commons");
            Assert.That(definition.AttackDamage / definition.AttackCooldown / dps, Is.InRange(0.5f, 2f), $"{beast.Id}: damage per second against its biome's commons");

            Assert.AreEqual(0, definition.Level, $"{beast.Id}: takes the area's level");
            Assert.AreEqual(beast.MinLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MinLevel), beast.Id);
            Assert.AreEqual(beast.MaxLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MaxLevel), beast.Id);
            float low = EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MinLevel);
            float high = EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MaxLevel);
            Assert.GreaterOrEqual(high, low, beast.Id);
            Assert.GreaterOrEqual(EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MaxLevel), EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MinLevel), beast.Id);
        }

        [Test]
        public void TheShooters_ThrowFromRange_AndTheRestSwing([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            bool shoots = Shooters.TryGetValue(beast.Id, out EnemyProjectileKind projectile);
            Assert.AreEqual(shoots, definition.IsRanged, beast.Id);
            if (shoots)
            {
                Assert.AreEqual(projectile, definition.Projectile, beast.Id);
                Assert.AreEqual(Load(beast.StandIn).Projectile, definition.Projectile, $"{beast.Id}: the shot of the stand-in it replaces");
                Assert.AreEqual(EnemyMotionArchetype.Flyer, definition.MotionArchetype, $"{beast.Id}: hovers like the other shooters");
                Assert.That(definition.AttackRange, Is.InRange(6f, 10f), $"{beast.Id}: readable on the isometric camera");
                Assert.Greater(definition.LoseTargetRadius, definition.AttackRange, $"{beast.Id}: does not lose the hero while shooting");
                Assert.AreEqual(EnemyStrikeKind.Projectile, EnemyRangedAttack.StrikeKindOf(definition.IsRanged, false), beast.Id);
            }
            else
            {
                Assert.AreEqual(EnemyProjectileKind.None, definition.Projectile, beast.Id);
                Assert.LessOrEqual(definition.AttackRange, 2.5f, $"{beast.Id}: a melee swing");
            }
        }

        [Test]
        public void EachOne_KeepsItsStandInsNumbers_SoTheEconomyDoesNotMove([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            EnemyDefinition standIn = Load(beast.StandIn);
            Assert.AreEqual(standIn.MaxHealth, definition.MaxHealth, beast.Id);
            Assert.AreEqual(standIn.ExperienceReward, definition.ExperienceReward, beast.Id);
            Assert.AreEqual(standIn.AttackDamage, definition.AttackDamage, beast.Id);
            Assert.AreEqual(standIn.AttackCooldown, definition.AttackCooldown, beast.Id);
            Assert.AreEqual(standIn.AttackRange, definition.AttackRange, beast.Id);
            Assert.AreEqual(standIn.MotionArchetype, definition.MotionArchetype, beast.Id);
            Assert.AreEqual(standIn.LootItem, definition.LootItem, beast.Id);
            Assert.AreEqual(standIn.LootDropChance, definition.LootDropChance, beast.Id);
            BiomeRegistry.Invalidate();
            Assert.AreEqual(BiomeRegistry.FindBiomeOf(standIn).Type, BiomeRegistry.FindBiomeOf(definition).Type, $"{beast.Id}: the stand-in's biome");
        }

        [Test]
        public void EachOne_HasABestiaryPage_WithLoreAndFiveCousins_AndAName([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            BestiaryCatalog.Invalidate();
            BestiaryEntry page = BestiaryCatalog.Get(beast.Id);
            Assert.IsNotNull(page, $"{beast.Id}: bestiary page");
            Assert.AreEqual(beast.Biome, page.Region, beast.Id);
            Assert.IsFalse(page.IsDungeon, beast.Id);
            Assert.IsFalse(page.IsBoss, beast.Id);
            Assert.IsNotEmpty(page.Drops, $"{beast.Id}: drops from its biome's loot table");
            Assert.IsNotEmpty(page.Tips, beast.Id);
            Assert.AreEqual(Shooters.ContainsKey(beast.Id), page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: the [distância] tip only for the shooters");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(CoastOwnBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), CoastOwnBestiaryTexts.EnemyIds);
        }

        [Test]
        public void EachOne_HasItsOwnProceduralModel_ThatBuildsAndTicks([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreEqual(beast.Model, ProceduralEnemyModels.KindFor(beast.Id), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsNightSpecies(beast.Model), $"{beast.Id}: a day enemy");
            Assert.IsFalse(ProceduralEnemyModels.IsOssuaryGuardian(beast.Model), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsBoss(beast.Model), beast.Id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, beast.Model);
            Assert.That((int)beast.Model, Is.InRange(400, 411), $"{beast.Id}: the kinds reserved for this pass");
            Assert.AreEqual(EnemyVisualSource.Procedural, EnemyVisualSwapper.ResolveSource(Load(beast.Id)), $"{beast.Id}: drawn with its own model");

            GameObject visual = Make(beast.Model, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig, beast.Id);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Body), $"{beast.Id}: Body");
            Assert.Greater(visual.GetComponentsInChildren<MeshFilter>().Length, 0, beast.Id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f), beast.Id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f, 0f), $"{beast.Id}: Reduzir Movimento");
        }

        [Test]
        public void TheModels_AreNotTheStandInsModels([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreNotEqual(ProceduralEnemyModels.KindFor(beast.StandIn), beast.Model, $"{beast.Id}: its own body, not {beast.StandIn}'s");
        }

        [TestCase(ProceduralEnemyKind.BlackSandJackal)]
        [TestCase(ProceduralEnemyKind.DimmedLionfish)]
        [TestCase(ProceduralEnemyKind.Kongamato)]
        [TestCase(ProceduralEnemyKind.GhostHeron)]
        [TestCase(ProceduralEnemyKind.SnareRoot)]
        public void Biters_OpenTheirJawOrBeakOrNoose_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Assert.IsNotNull(jaw, kind.ToString());
            Assert.AreEqual(ProceduralEnemyParts.Head, jaw.parent.name, $"{kind}: the lower jaw hangs from the head");
            Quaternion rest = jaw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 20f, $"{kind}: opens before the strike");
        }

        [TestCase(ProceduralEnemyKind.Kongamato)]
        [TestCase(ProceduralEnemyKind.GhostHeron)]
        [TestCase(ProceduralEnemyKind.DimmedLionfish)]
        public void TheFlyersAndTheFish_HaveTwoWingsOrFins_AndATail(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingRight), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Tail), kind.ToString());
        }

        [TestCase(ProceduralEnemyKind.MudJellyfish)]
        [TestCase(ProceduralEnemyKind.ThiefOctopus)]
        [TestCase(ProceduralEnemyKind.SmokeBubble)]
        [TestCase(ProceduralEnemyKind.MudChild)]
        public void TheSquashers_FlattenOnTheWindup_AndTheirLightFlares(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform light = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(light, kind.ToString());
            Vector3 bodyRest = body.localScale;
            Vector3 lightRest = light.localScale;
            Windup(rig);
            Assert.Less(body.localScale.y, bodyRest.y, $"{kind}: flattens before it strikes");
            Assert.Greater(light.localScale.x, lightRest.x, $"{kind}: the light flares");
        }

        [Test]
        public void TheScorpionKing_OpensItsClaws_AndDrawsItsStingBack()
        {
            Make(ProceduralEnemyKind.YoungScorpionKing, out ProceduralEnemyRig rig);
            Transform claw = rig.GetPart(ProceduralEnemyParts.ClawLeft);
            Transform tail = rig.GetPart(ProceduralEnemyParts.Tail);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.ClawRight));
            Quaternion clawRest = claw.localRotation;
            Quaternion tailRest = tail.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(clawRest, claw.localRotation), 20f, "the claws open before the pinch");
            Assert.Greater(Quaternion.Angle(tailRest, tail.localRotation), 20f, "the sting draws back before it strikes");
        }

        [Test]
        public void TheKushiteUshabti_RaisesBothArms_OnTheWindup()
        {
            Make(ProceduralEnemyKind.KushiteUshabti, out ProceduralEnemyRig rig);
            Transform left = rig.GetPart(ProceduralEnemyParts.ArmLeft);
            Transform right = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion leftRest = left.localRotation;
            Quaternion rightRest = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(leftRest, left.localRotation), 30f, "left arm");
            Assert.Greater(Quaternion.Angle(rightRest, right.localRotation), 30f, "right arm, the hoe");
        }

        [Test]
        public void TheDragonfly_HasFourWings_AndCurlsItsTail_OnTheWindup()
        {
            Make(ProceduralEnemyKind.SmokeDragonfly, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft));
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingRight));
            Transform tail = rig.GetPart(ProceduralEnemyParts.Stinger);
            Assert.IsNotNull(tail);
            Quaternion rest = tail.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, tail.localRotation), 30f, "the long abdomen curls in before it strikes");
        }
    }
}
