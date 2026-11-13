using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Five map bosses of the Rio das Doze Horas and the Margens da Mistura (Docs/Mapas100.md 72, 75, 78, 80, 81): the list, their strength against their
    /// region's world boss, each one's own ability as pure rules, their call-outs and the arms that rise on the windup. The shared checks (asset, biome,
    /// model, page, arena) are <see cref="MapBossTests"/>'s, once it lists them too.
    /// </summary>
    public class RiverMapBossTests
    {
        private static readonly string[] RiverIds =
        {
            RegionBossRules.ReedHippoId, RegionBossRules.LakeGuardianId, RegionBossRules.LostInstantId,
            RegionBossRules.HobabishId, RegionBossRules.UllikummiId,
        };

        private static readonly string[] RiverMaps =
        {
            OverworldLayouts.CamposJuncosMapId, OverworldLayouts.LagoDeFogoMapId, OverworldLayouts.DesertoRelogioMapId,
            OverworldLayouts.CedralHobabishMapId, OverworldLayouts.OmbroUpelluriMapId,
        };

        /// <summary>The world boss of each one's region (13 Apep, 14 O Sonho da Destruição).</summary>
        private static readonly string[] WorldBosses =
        {
            RegionBossRules.ApepId, RegionBossRules.ApepId, RegionBossRules.DreamOfDestructionId,
            RegionBossRules.DreamOfDestructionId, RegionBossRules.DreamOfDestructionId,
        };

        private static readonly RegionBossSignature[] RiverSignatures =
        {
            RegionBossSignature.RippleRing, RegionBossSignature.LakeShadow, RegionBossSignature.LostInstant,
            RegionBossSignature.SevenMantles, RegionBossSignature.GrowingStone,
        };

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
        }

        private static EnemyDefinition Load(string id)
        {
            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
            Assert.IsNotNull(definition, id);
            return definition;
        }

        // ---------------------------------------------------------------- the list

        [Test]
        public void TheRiverMaps_GetTheirFichaBoss_AtTheTopOfTheirBand()
        {
            for (int i = 0; i < RiverIds.Length; i++)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(RiverIds[i]);
                Assert.IsNotNull(spawn, RiverIds[i]);
                Assert.IsTrue(RegionBossRules.IsMapBoss(RiverIds[i]), RiverIds[i]);
                Assert.AreEqual(RiverMaps[i], spawn.MapId, RiverIds[i]);
                MapDefinition map = MapCatalog.Get(spawn.MapId);
                Assert.IsNotNull(map, spawn.MapId);
                Assert.AreEqual(map.MonsterLevelMax, spawn.Level, $"{RiverIds[i]}: the top of its map's band");
                Assert.AreEqual(map.MonsterLevelMax, Load(RiverIds[i]).Level, RiverIds[i]);
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == spawn.MapId), $"{spawn.MapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == spawn.MapId), spawn.MapId);
                Assert.IsTrue(spawn.NeverDies, $"{RiverIds[i]}: freed, it leaves with its line");
            }

            Assert.AreEqual(MapRegion.RioDasDozeHoras, MapCatalog.Get(OverworldLayouts.CamposJuncosMapId).Region);
            Assert.AreEqual(MapRegion.MargensDaMistura, MapCatalog.Get(OverworldLayouts.OmbroUpelluriMapId).Region);
        }

        [Test]
        public void TheRiverMapBosses_AreWeakerThanTheWorldBossOfTheirRegion()
        {
            for (int i = 0; i < RiverIds.Length; i++)
            {
                Assert.Less(Load(RiverIds[i]).MaxHealth, Load(WorldBosses[i]).MaxHealth, RiverIds[i]);
                Assert.Less(Load(RiverIds[i]).AttackDamage, Load(WorldBosses[i]).AttackDamage, RiverIds[i]);
                Assert.LessOrEqual(Load(RiverIds[i]).MaxHealth, 1180, RiverIds[i]);
                Assert.Less(Load(RiverIds[i]).BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, RiverIds[i]);
                Assert.AreEqual(RegionBossRules.MapBossKillXpMultiplier, RegionBossRules.KillXpRankMultiplier(RiverIds[i], true), RiverIds[i]);
            }
        }

        [Test]
        public void EachSignatureIsNewToTheDirector()
        {
            CollectionAssert.AreEquivalent(RiverSignatures, RiverIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in RiverSignatures)
            {
                Assert.AreEqual(1, RegionBossRules.DirectorSpawns.Count(s => s.Signature == signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureInterval(signature), RegionBossRules.SignatureWindup(signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureWindup(signature), 1f, $"{signature}: time to read the warning");
                Assert.IsTrue(RegionBossRules.SignatureEnabled(signature, RegionBossPhase.Storm), signature.ToString());
                Assert.IsFalse(RegionBossRules.SignatureEnabled(signature, RegionBossPhase.Wrath), signature.ToString());
            }
        }

        [Test]
        public void EachArena_IsInItsAreaOfItsMap()
        {
            OverworldLayout world = OverworldLayouts.StitchedWorld;
            foreach (string id in RiverIds)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
                Vector2? arena = RegionBossRules.ArenaIn(world, spawn);
                Assert.IsNotNull(arena, $"{id}: the arena is not in {spawn.AreaId}");
                Assert.AreEqual(spawn.MapId, OverworldRules.ZoneAt(world, arena.Value).MapIdOrOwner, id);
            }
        }

        // ---------------------------------------------------------------- the abilities

        [Test]
        public void ReedHippo_TheRippleStopsAtTheHerosDistance_InOrOutIsSafe()
        {
            Vector2 hippo = new Vector2(12f, -4f);
            const float plunge = 6f;
            Assert.IsTrue(RegionBossRules.RippleHits(hippo, plunge, hippo + Vector2.right * plunge), "stood still");
            Assert.IsTrue(RegionBossRules.RippleHits(hippo, plunge, hippo + Vector2.up * plunge), "walked round him, same distance");
            Assert.IsFalse(RegionBossRules.RippleHits(hippo, plunge, hippo + Vector2.right * (plunge - RegionBossRules.RippleHalfWidth - 0.3f)), "a step in");
            Assert.IsFalse(RegionBossRules.RippleHits(hippo, plunge, hippo + Vector2.right * (plunge + RegionBossRules.RippleHalfWidth + 0.3f)), "a step out");
            Assert.IsFalse(RegionBossRules.RippleHits(hippo, RegionBossRules.RippleReach + 2f, hippo + Vector2.right * (RegionBossRules.RippleReach + 2f)), "the ripple died down");
            Assert.IsTrue(RegionBossRules.RippleHits(hippo, 0.5f, hippo), "right on top of him when he went under");
            Assert.Less(RegionBossRules.RippleHalfWidth * 2f, 4f, "one step clears it");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.ReedHippoId).StrongerAtNight, "the ficha: always night in the fields");
        }

        [Test]
        public void LakeGuardian_HisShadowFallsAwayFromTheFlare_TheLitSideIsSafe()
        {
            Vector2 guardian = new Vector2(-3f, 8f);
            Assert.AreEqual(Vector2.up, RegionBossRules.LakeFlareDirection(0), "north first");
            Assert.AreEqual(Vector2.right, RegionBossRules.LakeFlareDirection(1));
            Assert.AreEqual(Vector2.down, RegionBossRules.LakeFlareDirection(2));
            Assert.AreEqual(0, RegionBossRules.LakeFlareOf(RegionBossRules.LakeFlares), "the flares come round");
            Assert.IsTrue(RegionBossRules.LakeShadowHits(0, guardian, guardian + Vector2.down * 6f), "north flare: in his shadow, south of him");
            Assert.IsFalse(RegionBossRules.LakeShadowHits(0, guardian, guardian + Vector2.up * 6f), "on the lit side");
            Assert.IsFalse(RegionBossRules.LakeShadowHits(0, guardian, guardian + Vector2.right * 6f), "beside him");
            Assert.IsTrue(RegionBossRules.LakeShadowHits(1, guardian, guardian + Vector2.left * 6f), "east flare: the shadow falls west");
            Assert.IsFalse(RegionBossRules.LakeShadowHits(1, guardian, guardian + Vector2.down * 6f), "east flare: south is lit");
            Assert.IsTrue(RegionBossRules.LakeShadowHits(2, guardian, guardian + Vector2.up * 6f), "south flare: the shadow falls north");
            Assert.IsFalse(RegionBossRules.LakeShadowHits(0, guardian, guardian + Vector2.down * (RegionBossRules.LakeShadowLength + 0.5f)), "past the end of his shadow");
            Assert.Less(RegionBossRules.LakeShadowDegrees, 90f, "the sides of him stay lit");
            Assert.Less(RegionBossRules.LakeShadowLength, RegionBossRules.AbilityRange);
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.LakeGuardianId).StrongerAtNight, "the ficha: the lake lights everything at night");
        }

        [Test]
        public void LostInstant_OnlyTheStolenInstantIsSafe_DrawnInsideTheReach()
        {
            Vector2 instant = new Vector2(40f, 40f);
            Vector2 last = instant + new Vector2(5f, 2f);
            Assert.AreEqual(last, RegionBossRules.InstantSpot(instant, last), "a spot inside the reach stays where it was");
            Assert.IsFalse(RegionBossRules.InstantHits(instant, last, last), "went back to the instant");
            Assert.IsFalse(RegionBossRules.InstantHits(instant, last, last + Vector2.up * (RegionBossRules.InstantSafeRadius - 0.3f)));
            Assert.IsTrue(RegionBossRules.InstantHits(instant, last, instant), "right next to it");
            Assert.IsTrue(RegionBossRules.InstantHits(instant, last, last + Vector2.up * (RegionBossRules.InstantSafeRadius + 0.3f)), "just off the instant");
            Assert.IsFalse(RegionBossRules.InstantHits(instant, last, instant + Vector2.left * (RegionBossRules.InstantReach + 0.5f)), "out of its reach");
            Vector2 far = RegionBossRules.InstantSpot(instant, instant + Vector2.right * 40f);
            Assert.AreEqual(RegionBossRules.InstantReach - RegionBossRules.InstantSafeRadius, Vector2.Distance(instant, far), 1e-3f, "a far instant is drawn in");
            Assert.AreEqual(0f, Vector2.Angle(Vector2.right, far - instant), 1e-3f, "along the same line");
            Assert.Greater(RegionBossRules.InstantWindupSeconds, 2.5f, "time to walk back");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.LostInstantId).StrongerAtNight);
        }

        [Test]
        public void Hobabish_TheMantlesStayWhereTheyFall_SevenAtMost()
        {
            var mantles = new List<Vector2>();
            for (int i = 0; i < RegionBossRules.MantleCount; i++)
            {
                Assert.AreEqual(i, RegionBossRules.ThrowMantle(mantles, new Vector2(i * 10f, 0f), i));
            }

            Assert.AreEqual(RegionBossRules.MantleCount, mantles.Count);
            Assert.IsTrue(RegionBossRules.MantlesHit(mantles, new Vector2(0f, 0f)), "the first mantle still flares");
            Assert.IsTrue(RegionBossRules.MantlesHit(mantles, new Vector2(60f, RegionBossRules.MantleRadius - 0.2f)), "the newest");
            Assert.IsFalse(RegionBossRules.MantlesHit(mantles, new Vector2(5f, 0f)), "between two mantles");
            Assert.AreEqual(0, RegionBossRules.ThrowMantle(mantles, new Vector2(0f, 50f), RegionBossRules.MantleCount), "the eighth is the first again");
            Assert.AreEqual(RegionBossRules.MantleCount, mantles.Count);
            Assert.IsFalse(RegionBossRules.MantlesHit(mantles, new Vector2(0f, 0f)), "the oldest went back on him");
            Assert.IsTrue(RegionBossRules.MantlesHit(mantles, new Vector2(0f, 50f)));
            Assert.Less(RegionBossRules.MantleRadius, 3.5f, "a step off a mantle is enough");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.HobabishId).StrongerAtNight);
        }

        [Test]
        public void Ullikummi_EachSlamReachesFarther_TheFootAndBeyondAreSafe()
        {
            Vector2 stone = new Vector2(0f, -20f);
            for (int g = 1; g < RegionBossRules.Growths; g++)
            {
                Assert.Greater(RegionBossRules.GrowthReach(g, false), RegionBossRules.GrowthReach(g - 1, false), $"growth {g}");
                Assert.Greater(RegionBossRules.GrowthReach(g, true), RegionBossRules.GrowthReach(g, false), $"growth {g}: more at night");
            }

            Assert.AreEqual(RegionBossRules.GrowthFirstReach, RegionBossRules.GrowthReach(RegionBossRules.Growths, false), 1e-4f, "cracked, it starts over");
            Assert.AreEqual(0, RegionBossRules.GrowthOf(RegionBossRules.Growths));
            Assert.IsTrue(RegionBossRules.GrowthHits(0, false, stone, stone + Vector2.right * 4f), "inside the first reach");
            Assert.IsFalse(RegionBossRules.GrowthHits(0, false, stone, stone + Vector2.right * 6f), "beyond the first reach");
            Assert.IsTrue(RegionBossRules.GrowthHits(1, false, stone, stone + Vector2.right * 6f), "the next one reaches there");
            Assert.IsFalse(RegionBossRules.GrowthHits(3, false, stone, stone + Vector2.right * (RegionBossRules.GrowthFootSafe - 0.3f)), "hugging its foot");
            float last = RegionBossRules.GrowthReach(RegionBossRules.Growths - 1, true);
            Assert.Less(last, RegionBossRules.AbilityRange, "beyond the biggest slam is still inside the fight");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.UllikummiId).StrongerAtNight, "the ficha: at night Ullikummi grows more");
        }

        // ---------------------------------------------------------------- the call-outs and the model

        [Test]
        public void TheRiverMapBosses_CallOutTheirFlaresInstantMantlesAndGrowth()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { RegionBossTexts.LakeShadowKey, RegionBossTexts.InstantSpotKey };
            keys.AddRange(Enumerable.Range(0, RegionBossRules.LakeFlares).Select(RegionBossTexts.LakeFlareKey));
            keys.AddRange(Enumerable.Range(0, RegionBossRules.MantleCount).Select(RegionBossTexts.MantleKey));
            keys.AddRange(Enumerable.Range(0, RegionBossRules.Growths).Select(RegionBossTexts.GrowthKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
            Assert.AreEqual(RegionBossTexts.LakeFlareKey(0), RegionBossTexts.LakeFlareKey(RegionBossRules.LakeFlares), "the flares come round");
            Assert.AreEqual(RegionBossTexts.MantleKey(0), RegionBossTexts.MantleKey(RegionBossRules.MantleCount), "the mantles come round");
            Assert.AreEqual(RegionBossTexts.GrowthKey(0), RegionBossTexts.GrowthKey(RegionBossRules.Growths), "the growth starts over");
            foreach (string id in RiverIds)
            {
                CollectionAssert.Contains(RegionBossTexts.EnemyIds, id);
            }
        }

        [Test]
        public void EachHasItsOwnModel_AMapBossOne()
        {
            ProceduralEnemyKind[] kinds =
            {
                ProceduralEnemyKind.ReedHippo, ProceduralEnemyKind.LakeGuardian, ProceduralEnemyKind.LostInstant,
                ProceduralEnemyKind.Hobabish, ProceduralEnemyKind.Ullikummi,
            };
            for (int i = 0; i < RiverIds.Length; i++)
            {
                Assert.AreEqual(kinds[i], ProceduralEnemyModels.KindFor(RiverIds[i]), RiverIds[i]);
                Assert.IsTrue(ProceduralEnemyModels.IsMapBoss(kinds[i]), kinds[i].ToString());
                CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, kinds[i]);
            }
        }

        [TestCase(ProceduralEnemyKind.LakeGuardian)]
        [TestCase(ProceduralEnemyKind.LostInstant)]
        [TestCase(ProceduralEnemyKind.Hobabish)]
        [TestCase(ProceduralEnemyKind.Ullikummi)]
        public void TheOnesWithArms_RaiseThem_OnTheWindup(ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform arm = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion rest = arm.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }

            Assert.Greater(Quaternion.Angle(rest, arm.localRotation), 30f, kind.ToString());
        }
    }
}
