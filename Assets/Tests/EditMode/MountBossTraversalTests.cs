using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// What three of the <see cref="MountBossCatalog"/> mounts do outside their ability (Docs/Mounts.md §3.13, 2026-11-16): the Laurel Lizard
    /// climbs the marked trunks of the Copas de Garajonay (05), the Jhilpur buffalo wades the fords the monsoon flooded (31), the Smoke Cloud
    /// of the freed horse has a visible smoke, and the four boss mounts get a freeing line when they drop. Pure rules only.
    /// </summary>
    public class MountBossTraversalTests
    {
        // ---------------- Lagarto-do-Louro: the marked trunks (05) ----------------

        [Test]
        public void Trunks_OnlyTheLaurelLizardClimbs_Mounted_AfterABreath()
        {
            Assert.IsTrue(GarajonayTrunkRules.IsClimber(true, MountBossCatalog.LaurelLizard));
            Assert.IsFalse(GarajonayTrunkRules.IsClimber(false, MountBossCatalog.LaurelLizard), "on foot, with the lizard waiting");
            foreach (MountSpec spec in MountCatalog.Specs.Where(s => s.MountId != MountBossCatalog.LaurelLizard))
            {
                Assert.IsFalse(GarajonayTrunkRules.IsClimber(true, spec.MountId), spec.MountId + " does not climb");
            }

            Assert.IsFalse(GarajonayTrunkRules.IsClimber(true, null));
            Assert.AreEqual(TrunkCheck.Climb, GarajonayTrunkRules.Check(true, MountBossCatalog.LaurelLizard, float.PositiveInfinity));
            Assert.AreEqual(TrunkCheck.Resting, GarajonayTrunkRules.Check(true, MountBossCatalog.LaurelLizard, GarajonayTrunkRules.RestSeconds - 0.1f));
            Assert.AreEqual(TrunkCheck.Climb, GarajonayTrunkRules.Check(true, MountBossCatalog.LaurelLizard, GarajonayTrunkRules.RestSeconds));
            Assert.AreEqual(TrunkCheck.NeedsLizard, GarajonayTrunkRules.Check(true, MountBossCatalog.JhilpurBuffalo, 99f));
            Assert.AreEqual(TrunkCheck.NeedsLizard, GarajonayTrunkRules.Check(false, null, 99f));
        }

        [Test]
        public void Trunks_ReachAndCourse_GoFromTheTrunkAtHandToTheOtherOne()
        {
            var links = new[] { new LaurelTrunkLink("t", new Vector2(0f, 0f), new Vector2(44f, 0f)) };
            Assert.AreEqual(0, GarajonayTrunkRules.TrunkAt(links, new Vector2(2.9f, 0f), out bool atA));
            Assert.IsTrue(atA);
            Assert.AreEqual(0, GarajonayTrunkRules.TrunkAt(links, new Vector2(42f, 1f), out atA));
            Assert.IsFalse(atA);
            Assert.AreEqual(-1, GarajonayTrunkRules.TrunkAt(links, new Vector2(22f, 0f), out _));
            Assert.AreEqual(-1, GarajonayTrunkRules.TrunkAt(links, new Vector2(3.2f, 0f), out _), "reach 3 m");

            CollectionAssert.AreEqual(new[] { links[0].A, links[0].B }, GarajonayTrunkRules.Course(links[0], true));
            CollectionAssert.AreEqual(new[] { links[0].B, links[0].A }, GarajonayTrunkRules.Course(links[0], false));
            Assert.AreEqual(links[0].B, GarajonayTrunkRules.Landing(links[0], true));
            Assert.AreEqual(links[0].A, GarajonayTrunkRules.Landing(links[0], false));
            Assert.AreEqual(4f, GarajonayTrunkRules.RunSeconds(links[0]), 1e-4f);
            Assert.Greater(GarajonayTrunkRules.CanopyArc, 8.8f, "over the 8.8 m platforms of the canopy");
        }

        [Test]
        public void Trunks_StandOnTheForestFloorOfTheCopas_AndCutAcrossTheStreams()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            LaurelTrunkLink[] links = OverworldLayouts.GarajonayTrunks;
            Assert.AreEqual(3, links.Length);
            Assert.AreEqual(links.Length, links.Select(l => l.Id).Distinct().Count());
            int overWater = 0;
            foreach (LaurelTrunkLink link in links)
            {
                foreach (Vector2 end in new[] { link.A, link.B })
                {
                    OverworldZone zone = OverworldRules.ZoneAt(world, end);
                    Assert.IsNotNull(zone, link.Id);
                    Assert.AreEqual(MapLayouts.GarajonayId, zone.MapId ?? zone.OwnerMapId, link.Id + " outside the Copas");
                    Assert.IsTrue(OverworldLayouts.GarajonayFogBounds.Contains(end), link.Id + " outside the driver's area");
                    Assert.IsNull(OverworldRules.SettlementAt(world, end), link.Id + " in the city");
                    Assert.IsFalse(MapLayoutRules.InRiver(world.World, end, 3f), link.Id + " in a stream");
                    Assert.Greater(Vector2.Distance(end, OverworldLayouts.VelhoDaNevoaArena), 20f, link.Id + " in the Velho da Névoa's ring");
                }

                // Every link is a real shortcut (40 m or more over the canopy), none so long that the run takes more than 10 s.
                float length = Vector2.Distance(link.A, link.B);
                Assert.That(length, Is.InRange(40f, 110f), link.Id);
                Assert.AreEqual(-1, GarajonayTrunkRules.TrunkAt(links.Where(l => l.Id != link.Id).ToList(), link.A, out _), link.Id + " shares a trunk");

                bool crosses = false;
                for (int k = 1; k < 40 && !crosses; k++)
                {
                    crosses = MapLayoutRules.InRiver(world.World, Vector2.Lerp(link.A, link.B, k / 40f), 0f);
                }

                overWater += crosses ? 1 : 0;
            }

            Assert.GreaterOrEqual(overWater, 2, "the two cascade links cross their stream away from its bridge");
        }

        [Test]
        public void Trunks_Texts_InBothLanguages_AndALineForEveryCousin()
        {
            Dictionary<string, LocalizationEntry> byKey = GarajonayTrunkTexts.Entries.ToDictionary(e => e.Key);
            foreach (string key in new[] { GarajonayTrunkTexts.Prompt, GarajonayTrunkTexts.NeedsLizard, GarajonayTrunkTexts.Resting, GarajonayTrunkTexts.Climbs, GarajonayTrunkTexts.Landed })
            {
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].En), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].PtBR), key);
            }

            var lines = new HashSet<string>();
            foreach (CharacterId hero in new[] { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine })
            {
                string key = GarajonayTrunkTexts.LineFor(hero);
                Assert.IsTrue(byKey.ContainsKey(key), hero.ToString());
                lines.Add(key);
            }

            Assert.AreEqual(5, lines.Count, "one line per cousin");
            Assert.AreEqual(GarajonayTrunkTexts.LineJoaquim, GarajonayTrunkTexts.LineFor(null));
        }

        // ---------------- Búfalo-d'Água de Jhilpur: the monsoon fords (31) ----------------

        [Test]
        public void Buffalo_WadesTheFloodedFords_OnlyMounted_AndAloneOfTheSwimmers()
        {
            Assert.IsTrue(MountBossRules.BuffaloWadesFlood(true, MountBossCatalog.JhilpurBuffalo));
            Assert.IsFalse(MountBossRules.BuffaloWadesFlood(false, MountBossCatalog.JhilpurBuffalo), "on foot the current takes the cousin");
            Assert.IsFalse(MountBossRules.BuffaloWadesFlood(true, null));
            foreach (MountSpec spec in MountCatalog.Specs.Where(s => s.MountId != MountBossCatalog.JhilpurBuffalo))
            {
                Assert.IsFalse(MountBossRules.BuffaloWadesFlood(true, spec.MountId), spec.MountId + " (the frog, the capybara, the Ninki Nanka are carried)");
            }

            MountSpec buffalo = MountCatalog.FindSpec(MountBossCatalog.JhilpurBuffalo);
            Assert.AreEqual(MountType.Aquatico, buffalo.Type);
            Assert.AreEqual(MountSize.Grande, buffalo.Size, "heavy enough to stand the current");
        }

        [Test]
        public void Buffalo_TheCurrentItStands_IsTheOneThatThrowsEveryoneElseBack()
        {
            // The monsoon of Jhilpur still floods and pushes (the buffalo changes nothing of the calendar or the fords).
            var ford = new DeltaFord("f", Vector2.zero, Vector2.up, 6f, 4f, false, Vector2.zero, Vector2.zero);
            Assert.IsTrue(JhilpurMonsoonRules.InFordWater(ford, new Vector2(1f, 0f)));
            Assert.Greater(JhilpurMonsoonRules.PushStep(ford, 1, 1f).magnitude, 5.5f, "more than the walk");
            Assert.IsTrue(JhilpurMonsoonRules.IsMonsoon(5) && JhilpurMonsoonRules.IsMonsoon(6) && !JhilpurMonsoonRules.IsMonsoon(4));
            Assert.Greater(MountCatalog.FindSpec(MountBossCatalog.JhilpurBuffalo).Speed, 1f);
        }

        [Test]
        public void Buffalo_FordLine_InBothLanguages()
        {
            LocalizationEntry entry = RouteMechanicsTexts.Entries.Single(e => e.Key == RouteMechanicsTexts.FordBuffalo);
            Assert.IsFalse(string.IsNullOrEmpty(entry.En));
            Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR));
        }

        // ---------------- Cavalo de Fumaça Liberto: the smoke you see ----------------

        [Test]
        public void Smoke_BillowsHoldsAndThins_OverTheCloudsLife()
        {
            const float duration = 5f;
            Assert.AreEqual(0f, MountBossRules.SmokeDensity(-0.1f, duration));
            Assert.AreEqual(0f, MountBossRules.SmokeDensity(0f, duration), 1e-4f, "starts from nothing");
            Assert.AreEqual(0.5f, MountBossRules.SmokeDensity(MountBossRules.SmokeBillowSeconds * 0.5f, duration), 1e-4f);
            Assert.AreEqual(1f, MountBossRules.SmokeDensity(2f, duration), 1e-4f, "full in the middle");
            Assert.AreEqual(0.5f, MountBossRules.SmokeDensity(duration - MountBossRules.SmokeThinSeconds * 0.5f, duration), 1e-4f);
            Assert.AreEqual(0f, MountBossRules.SmokeDensity(duration, duration));
            Assert.AreEqual(0f, MountBossRules.SmokeDensity(1f, 0f));
            Assert.LessOrEqual(MountBossRules.SmokeDensity(0.2f, 0.3f), 1f, "a very short cloud still stays in 0–1");

            MountAbilityDefinition ability = MountCatalog.Get(MountBossCatalog.FreedSmokeHorse).Ability;
            float full = MountBossRules.SmokePuffsPerSecond(ability.Radius, 1f);
            Assert.That(full, Is.InRange(20f, 120f), "a cloud you see, within the particle budget");
            Assert.AreEqual(full * 0.5f, MountBossRules.SmokePuffsPerSecond(ability.Radius, 0.5f), 1e-3f);
            Assert.AreEqual(0f, MountBossRules.SmokePuffsPerSecond(ability.Radius, 0f));
            Assert.AreEqual(120f, MountBossRules.SmokePuffsPerSecond(100f, 1f), "capped");
        }

        // ---------------- The freeing scene ----------------

        [Test]
        public void Freeing_EveryBossMountHasItsLine_InBothLanguages_TheOthersNone()
        {
            Dictionary<string, LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach ((string bossId, string mountId) in MountBossCatalog.BossSources)
            {
                string key = MountBossRules.FreedLineKey(mountId);
                Assert.IsNotNull(key, mountId);
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsTrue(byKey[key].En.Contains("{0}") && byKey[key].PtBR.Contains("{0}"), key + " names the mount");
                Assert.IsFalse(byKey[key].PtBR.Contains("Dona ") || byKey[key].PtBR.Contains("Seu "), key);
            }

            Assert.AreEqual(4, MountBossCatalog.BossSources.Select(s => MountBossRules.FreedLineKey(s.MountId)).Distinct().Count());
            Assert.IsNull(MountBossRules.FreedLineKey(MountBossCatalog.LaurelLizard), "bought, not freed");
            Assert.IsNull(MountBossRules.FreedLineKey(MountBossCatalog.JhilpurBuffalo), "bought, not freed");
            Assert.IsNull(MountBossRules.FreedLineKey(MountCatalog.Pony));
            Assert.IsNull(MountBossRules.FreedLineKey(null));
        }

        [Test]
        public void Freeing_OutsidePlayMode_DoesNothing()
        {
            Assert.DoesNotThrow(() => MountBossFreeing.Play(Vector3.zero, null));
            Assert.DoesNotThrow(() => MountBossFreeing.Play(Vector3.zero, new List<OwnedMount> { new OwnedMount { mountId = MountBossCatalog.FreedSmokeHorse } }));
        }
    }
}
