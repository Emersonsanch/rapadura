using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Second round of the "Falta" of maps 57–100 (Docs/Mapas100.md, 2026-11-23): the late shadows at night (78, <see cref="LateShadowEchoRules"/>), the
    /// Betume Vivo of the night pools (79, <see cref="NightTarRules"/>), the pitch smoke that bursts (79, <see cref="PitchSmokeIgniteRules"/>), the stubborn
    /// sparks and the moon that grows (93, <see cref="MoonDeckSparkRules"/>), the cracks by night (99, <see cref="CeuSkyHoldRules"/>) and the porch that
    /// keeps the day's story (100, <see cref="CeuPorchRules"/>), with their texts.
    /// </summary>
    public class LateFaltaRound2Tests
    {
        private const float Step = 1f / 60f;
        private static readonly CharacterId[] Cousins = { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };

        // ------------------------------------------------------------------
        // 78 · late shadows
        // ------------------------------------------------------------------

        [Test]
        public void LateShadow_RisesOnlyAtNight_InTheDesert_NeverFromBossesOrShadows_AndIsCapped()
        {
            Vector2 desert = new Vector2(600f, 0f);
            Assert.IsTrue(LateShadowEchoRules.LeavesShadow(true, desert, "enemy_forest_living_hourglass", false, false, 0));
            Assert.IsFalse(LateShadowEchoRules.LeavesShadow(false, desert, "enemy_forest_living_hourglass", false, false, 0), "day");
            Assert.IsFalse(LateShadowEchoRules.LeavesShadow(true, desert, "enemy_mountain_lost_instant", true, false, 0), "boss");
            Assert.IsFalse(LateShadowEchoRules.LeavesShadow(true, desert, LateShadowEchoRules.LateShadowId, false, true, 0), "a shadow never leaves a shadow");
            Assert.IsFalse(LateShadowEchoRules.LeavesShadow(true, desert, "x", false, false, LateShadowEchoRules.MaxAlive), "cap");
            Assert.IsTrue(LateShadowEchoRules.LeavesShadow(true, desert, "x", false, false, LateShadowEchoRules.MaxAlive - 1));
            Assert.IsFalse(LateShadowEchoRules.LeavesShadow(true, new Vector2(100f, -900f), "x", false, false, 0), "the Bitumen Wells are not the desert");

            Assert.AreEqual(312, LateShadowEchoRules.ShadowLevel(312));
            Assert.AreEqual(LateShadowEchoRules.FallbackLevel, LateShadowEchoRules.ShadowLevel(0));
            Assert.IsTrue(LateShadowEchoRules.Fades(false, 1f), "dawn takes them");
            Assert.IsTrue(LateShadowEchoRules.Fades(true, LateShadowEchoRules.LeaveRange + 1f));
            Assert.IsFalse(LateShadowEchoRules.Fades(true, 10f));
        }

        [Test]
        public void LateShadow_DesertIsMap78_AndTheBeastExists()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.MisturaRegionId);
            Rect area = LateShadowEchoRules.DesertArea;
            foreach (Vector2 p in new[] { area.center, area.min + Vector2.one * 5f, area.max - Vector2.one * 5f })
            {
                Assert.AreEqual(OverworldLayouts.DesertoRelogioMapId, OverworldRules.ZoneAt(world, p).MapIdOrOwner, p.ToString());
            }

            Assert.IsNotNull(Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{LateShadowEchoRules.LateShadowId}"));
            Assert.IsNotNull(Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{NightTarRules.LivingTarId}"));
            Assert.IsNotNull(Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{PitchSmokeIgniteRules.PitchSmokeId}"));
        }

        // ------------------------------------------------------------------
        // 79 · night pools and the pitch smoke
        // ------------------------------------------------------------------

        [Test]
        public void NightTar_DarkPoolsSpitAtNight_LitOnesDoNot_AndTheBitumenBurnsHarder()
        {
            Assert.IsTrue(NightTarRules.Spits(true, true, MisturaTarStage.Sticky, 10f, 0f, 0));
            Assert.IsFalse(NightTarRules.Spits(false, true, MisturaTarStage.Sticky, 10f, 0f, 0), "day");
            Assert.IsFalse(NightTarRules.Spits(true, false, MisturaTarStage.Sticky, 10f, 0f, 0), "hero outside");
            Assert.IsFalse(NightTarRules.Spits(true, true, MisturaTarStage.Fuse, 10f, 0f, 0), "lit");
            Assert.IsFalse(NightTarRules.Spits(true, true, MisturaTarStage.Burning, 10f, 0f, 0));
            Assert.IsFalse(NightTarRules.Spits(true, true, MisturaTarStage.Scorched, 10f, 0f, 0));
            float next = NightTarRules.NextReady(10f);
            Assert.AreEqual(10f + NightTarRules.PatchRestSeconds, next, 1e-4f);
            Assert.IsFalse(NightTarRules.Spits(true, true, MisturaTarStage.Sticky, next - 1f, next, 0), "the pool rests");
            Assert.IsFalse(NightTarRules.Spits(true, true, MisturaTarStage.Sticky, 10f, 0f, NightTarRules.MaxAlive), "cap");

            Assert.AreEqual(1000f * MisturaTarRules.BeastBurnFraction, NightTarRules.BurnTick(1000f, false), 1e-3f);
            Assert.AreEqual(3f * NightTarRules.BurnTick(1000f, false), NightTarRules.BurnTick(1000f, true), 1e-3f);
            Assert.IsTrue(NightTarRules.IsLivingTar(NightTarRules.LivingTarId));
            Assert.IsFalse(NightTarRules.IsLivingTar(PitchSmokeIgniteRules.PitchSmokeId));
        }

        [Test]
        public void PitchSmoke_LightsFromTheSkillOrTheCaster_BurstsAndChainsIntoThePools()
        {
            Vector2 smoke = new Vector2(0f, 0f);
            Assert.IsTrue(PitchSmokeIgniteRules.Reaches(smoke, new Vector2(4f, 0f), 2.5f, new Vector2(50f, 0f)));
            Assert.IsFalse(PitchSmokeIgniteRules.Reaches(smoke, new Vector2(6f, 0f), 2.5f, new Vector2(50f, 0f)));
            Assert.IsTrue(PitchSmokeIgniteRules.Reaches(smoke, new Vector2(30f, 0f), 0f, new Vector2(1.5f, 0f)), "cast from beside it");

            Assert.IsTrue(PitchSmokeIgniteRules.CanLight(5f, 0f));
            Assert.IsFalse(PitchSmokeIgniteRules.CanLight(5f + PitchSmokeIgniteRules.RelightSeconds - 0.1f, 5f));
            Assert.IsTrue(PitchSmokeIgniteRules.CanLight(5f + PitchSmokeIgniteRules.RelightSeconds, 5f));

            Assert.AreEqual(350f, PitchSmokeIgniteRules.SelfBurn(1000f), 1e-3f);
            Assert.AreEqual(80f, PitchSmokeIgniteRules.BeastBurn(1000f, false), 1e-3f);
            Assert.AreEqual(10f, PitchSmokeIgniteRules.BeastBurn(1000f, true), 1e-3f);

            // The hero is singed only close by, and never below 30%.
            Assert.AreEqual(40f, PitchSmokeIgniteRules.HeroBurn(smoke, new Vector2(2f, 0f), 1000f, 1000f), 1e-3f);
            Assert.AreEqual(0f, PitchSmokeIgniteRules.HeroBurn(smoke, new Vector2(4f, 0f), 1000f, 1000f));
            Assert.AreEqual(10f, PitchSmokeIgniteRules.HeroBurn(smoke, Vector2.zero, 310f, 1000f), 1e-3f);
            Assert.AreEqual(0f, PitchSmokeIgniteRules.HeroBurn(smoke, Vector2.zero, 300f, 1000f));

            MisturaTarPatch patch = OverworldLayouts.TarPatches[0];
            Assert.IsTrue(PitchSmokeIgniteRules.LightsPatch(patch, patch.Center + new Vector2(patch.Radius + PitchSmokeIgniteRules.BurstRadius - 0.5f, 0f)));
            Assert.IsFalse(PitchSmokeIgniteRules.LightsPatch(patch, patch.Center + new Vector2(patch.Radius + PitchSmokeIgniteRules.BurstRadius + 0.5f, 0f)));
        }

        // ------------------------------------------------------------------
        // 93 · stubborn sparks
        // ------------------------------------------------------------------

        [Test]
        public void Sparks_StrikingMakesThemSulk_PatienceConvincesThem_AndTheLanternLightsThem()
        {
            var session = new MoonDeckSparkSession();
            Assert.IsTrue(session.BeginTalk(1));
            Assert.IsFalse(session.BeginTalk(2), "one at a time");
            Assert.AreEqual(MoonSparkEvent.Sulked, session.TickTalk(Step, true, true, 100f));
            Assert.AreEqual(100f + MoonDeckSparkRules.SulkSeconds, session.SulkUntil[1], 1e-3f);

            Assert.IsTrue(session.BeginTalk(1));
            Assert.AreEqual(MoonSparkEvent.Cancelled, session.TickTalk(Step, false, false, 101f));

            Assert.IsTrue(session.BeginTalk(0));
            MoonSparkEvent last = MoonSparkEvent.None;
            float t = 0f;
            while (last == MoonSparkEvent.None && t < 20f)
            {
                last = session.TickTalk(Step, true, false, 102f + t);
                t += Step;
            }

            Assert.AreEqual(MoonSparkEvent.Convinced, last);
            Assert.AreEqual(MoonDeckSparkRules.ConvinceSeconds, t, 0.05f);
            Assert.AreEqual(0, session.Carrying);
            Assert.IsFalse(session.BeginTalk(2), "carrying one");
            Assert.AreEqual(MoonSparkEvent.None, session.TickCarry(true, true, false));
            Assert.AreEqual(MoonSparkEvent.Delivered, session.TickCarry(true, true, true));
            Assert.AreEqual(-1, session.Carrying);

            Assert.IsTrue(session.BeginTalk(3));
            for (float s = 0f; s <= MoonDeckSparkRules.ConvinceSeconds + Step; s += Step)
            {
                session.TickTalk(Step, true, false, 200f);
            }

            Assert.AreEqual(MoonSparkEvent.Dropped, session.TickCarry(false, true, false), "left the ship");
            Assert.IsFalse(session.Busy);
        }

        [Test]
        public void Sparks_LanternsAreDaily_FiveMakeTheFeast_OnceADay()
        {
            var days = new[] { 3, 3, 2, 0, 3 };
            Assert.AreEqual(3, MoonDeckSparkRules.LitCount(days, 3));
            Assert.IsFalse(MoonDeckSparkRules.IsFullMoon(3));
            days[2] = 3;
            days[3] = 3;
            Assert.IsTrue(MoonDeckSparkRules.IsFullMoon(MoonDeckSparkRules.LitCount(days, 3)));
            Assert.IsTrue(MoonDeckSparkRules.FeastDue(5, 2, 3));
            Assert.IsFalse(MoonDeckSparkRules.FeastDue(5, 3, 3), "already held today");
            Assert.IsFalse(MoonDeckSparkRules.FeastDue(4, 0, 3));
            Assert.AreEqual(0, MoonDeckSparkRules.LitCount(days, 4), "a new day, a new moon");

            var sulk = new float[MoonDeckSparkRules.SparkCount];
            Vector2[] places = MoonDeckSparkRules.HidingPlaces;
            var none = new int[MoonDeckSparkRules.SparkCount];
            Assert.AreEqual(2, MoonDeckSparkRules.SparkNear(places, places[2] + Vector2.right, none, 3, sulk, 0f));
            Assert.AreEqual(-1, MoonDeckSparkRules.SparkNear(places, places[2] + Vector2.right * 5f, none, 3, sulk, 0f), "too far");
            var lit = new[] { 0, 0, 3, 0, 0 };
            Assert.AreEqual(-1, MoonDeckSparkRules.SparkNear(places, places[2], lit, 3, sulk, 0f), "lit today");
            sulk[2] = 10f;
            Assert.AreEqual(-1, MoonDeckSparkRules.SparkNear(places, places[2], none, 3, sulk, 5f), "sulking");
            Assert.AreEqual(2, MoonDeckSparkRules.SparkNear(places, places[2], none, 3, sulk, 10f));
            Assert.AreEqual(100f, MoonDeckSparkRules.DeliverHeal(1000f), 1e-3f);
            Assert.IsNotNull(ItemDatabase.GetById(MoonDeckSparkRules.FeastItemId));
        }

        [Test]
        public void Sparks_HideInTheHold_ByTheRoads_AwayFromTheBossRing_AndTheLanternIsTheProwCabin()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.CeuRegionId);
            Assert.AreEqual(MoonDeckSparkRules.SparkCount, MoonDeckSparkRules.HidingPlaces.Length);
            var keys = new HashSet<string>();
            for (int i = 0; i < MoonDeckSparkRules.SparkCount; i++)
            {
                Vector2 p = MoonDeckSparkRules.HidingPlaces[i];
                OverworldZone zone = OverworldRules.ZoneAt(world, p);
                Assert.AreEqual("porao_da_lua", zone.Id, "spark " + i);
                Assert.AreEqual(OverworldLayouts.BarcaDaLuaMapId, zone.MapIdOrOwner, "spark " + i);
                Assert.IsTrue(MoonDeckSparkRules.ShipArea.Contains(p));
                Assert.Greater(Vector2.Distance(p, OverworldLayouts.ClandestinoGrandeArena), 30f, "spark " + i);
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, p, 16f), "spark " + i + " is by a road");
                Assert.IsNull(OverworldRules.SettlementAt(world, p), "spark " + i);
                Assert.IsTrue(keys.Add(MoonDeckSparkRules.MarkKey(i)));
            }

            Assert.AreEqual(OverworldLayouts.CabineDaProaOffset, MoonDeckSparkRules.Lantern);
            Assert.IsTrue(MoonDeckSparkRules.ShipArea.Contains(MoonDeckSparkRules.Lantern));
            Assert.IsTrue(MoonDeckSparkRules.ShipArea.Contains(OverworldLayouts.MoonBarqueBerth));
        }

        // ------------------------------------------------------------------
        // 99 · cracks by night
        // ------------------------------------------------------------------

        [Test]
        public void Cracks_ByNight_HoldLonger_DripFaster_SealShorter_ButMendAndBiteMore()
        {
            var day = new CeuSkyHoldSession();
            Assert.IsTrue(day.TryRaise(0, 1000L));
            CeuSkyEvent e = CeuSkyEvent.None;
            float t = 0f;
            int dayDrips = 0;
            while (e != CeuSkyEvent.Sealed && t < 60f)
            {
                e = day.Tick(Step, true, 1000L);
                dayDrips += e == CeuSkyEvent.Drip ? 1 : 0;
                t += Step;
            }

            Assert.AreEqual(CeuSkyHoldRules.HoldSeconds, t, 0.05f);
            Assert.IsFalse(day.LastSealAtNight);
            Assert.IsTrue(day.IsSealed(0, 1000L + CeuSkyHoldRules.SealSeconds - 1L));

            var night = new CeuSkyHoldSession();
            Assert.IsTrue(night.TryRaise(1, 1000L));
            e = CeuSkyEvent.None;
            t = 0f;
            int nightDrips = 0;
            while (e != CeuSkyEvent.Sealed && t < 60f)
            {
                e = night.Tick(Step, true, 1000L, true);
                nightDrips += e == CeuSkyEvent.Drip ? 1 : 0;
                t += Step;
            }

            Assert.AreEqual(CeuSkyHoldRules.NightHoldSeconds, t, 0.05f);
            Assert.Greater(nightDrips, dayDrips);
            Assert.IsTrue(night.LastSealAtNight);
            Assert.IsTrue(night.IsSealed(1, 1000L + CeuSkyHoldRules.NightSealSeconds - 1L));
            Assert.IsFalse(night.IsSealed(1, 1000L + CeuSkyHoldRules.NightSealSeconds));
            Assert.LessOrEqual(CeuSkyHoldRules.NightSealSeconds, CeuSkyHoldRules.SealSeconds, "the saved check still accepts a night seal");

            Assert.AreEqual(60f, CeuSkyHoldRules.DripAmount(1000f, 1000f, true), 1e-3f);
            Assert.AreEqual(CeuSkyHoldRules.DripAmount(1000f, 1000f), CeuSkyHoldRules.DripAmount(1000f, 1000f, false), 1e-3f);
            Assert.AreEqual(20f, CeuSkyHoldRules.DripAmount(320f, 1000f, true), 1e-3f, "same floor");
            Assert.Greater(CeuSkyHoldRules.SealHeal(1000f, true), CeuSkyHoldRules.SealHeal(1000f));
            Assert.Greater(CeuSkyHoldRules.SealBite(1000f, false, true), CeuSkyHoldRules.SealBite(1000f, false));
            Assert.AreEqual(0f, CeuSkyHoldRules.SealBite(1000f, true, true), "bosses untouched");

            var progress = new WorldMapProgress();
            CeuMechanicsDriver.StoreSeal(progress, 2, 5000L, true);
            Assert.IsTrue(CeuMechanicsDriver.CrackSealedIn(progress, 2, 5000L + CeuSkyHoldRules.NightSealSeconds - 1L));
            Assert.IsFalse(CeuMechanicsDriver.CrackSealedIn(progress, 2, 5000L + CeuSkyHoldRules.NightSealSeconds));
        }

        // ------------------------------------------------------------------
        // 100 · the porch keeps the day's story
        // ------------------------------------------------------------------

        [Test]
        public void Porch_KeepsTheLineForTheSameDay_AndTheStoriesHeard()
        {
            long day = CeuPorchRules.UtcDay(1_800_000_000L);
            Assert.AreEqual(1_800_000_000L / CeuPorchRules.SecondsPerDay, day);
            Assert.AreEqual(-1L, CeuPorchRules.UtcDay(-5L));

            long mark = CeuPorchRules.EncodeResume(day, 2);
            Assert.Greater(mark, 0L);
            Assert.AreEqual(2, CeuPorchRules.ResumeLine(mark, day));
            Assert.AreEqual(0, CeuPorchRules.ResumeLine(mark, day + 1), "another day starts over");
            Assert.AreEqual(0, CeuPorchRules.ResumeLine(0L, day));
            Assert.AreEqual(0, CeuPorchRules.ResumeLine(CeuPorchRules.EncodeResume(day, 0), day));
            Assert.AreEqual(0, CeuPorchRules.ResumeLine(CeuPorchRules.EncodeResume(day, CeuPorchRules.LinesPerStory), day), "the story was over");
            for (int line = 1; line < CeuPorchRules.LinesPerStory; line++)
            {
                Assert.AreEqual(line, CeuPorchRules.ResumeLine(CeuPorchRules.EncodeResume(day, line), day));
            }

            var porch = new CeuPorchSession();
            porch.Begin(1, false, 2);
            Assert.AreEqual(2, porch.Line);
            int nextLines = 0;
            CeuPorchEvent e = CeuPorchEvent.None;
            for (int i = 0; i < 2000 && e != CeuPorchEvent.Finished; i++)
            {
                e = porch.Tick(0.1f, true, false);
                nextLines += e == CeuPorchEvent.NextLine ? 1 : 0;
            }

            Assert.AreEqual(CeuPorchEvent.Finished, e);
            Assert.AreEqual(1, nextLines, "picked up at line 2 of 4: one more line, then the end");

            int mask = 0;
            for (int s = 0; s < CeuPorchRules.Stories; s++)
            {
                Assert.IsFalse(CeuPorchRules.AllHeard(mask));
                mask = CeuPorchRules.WithStory(mask, s);
                mask = CeuPorchRules.WithStory(mask, s);
                Assert.AreEqual(s + 1, CeuPorchRules.HeardCount(mask));
            }

            Assert.IsTrue(CeuPorchRules.AllHeard(mask));
            Assert.AreEqual(mask, CeuPorchRules.WithStory(mask, 7), "out of range changes nothing");
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_HaveBothLanguages_ALineForEachCousin_AndNoFolksyTitles()
        {
            foreach (IReadOnlyList<LocalizationEntry> entries in new[] { MisturaNightTexts.Entries, CeuRound2Texts.Entries })
            {
                var seen = new HashSet<string>();
                foreach (LocalizationEntry entry in entries)
                {
                    Assert.IsTrue(seen.Add(entry.Key), entry.Key);
                    Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                    Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                    StringAssert.DoesNotContain("Dona ", entry.PtBR);
                    StringAssert.DoesNotContain("Seu ", entry.PtBR);
                }
            }

            foreach (CharacterId hero in Cousins)
            {
                foreach (MisturaNightTexts.Moment m in new[] { MisturaNightTexts.Moment.LateShadow, MisturaNightTexts.Moment.NightTar, MisturaNightTexts.Moment.SmokeBurst })
                {
                    string key = MisturaNightTexts.LineFor(m, hero);
                    Assert.IsNotNull(key, hero + " " + m);
                    StringAssert.StartsWith(hero == CharacterId.Icaro ? "Ícaro" : hero.ToString(), MisturaNightTexts.Get(null, key));
                }

                foreach (CeuRound2Texts.Moment m in new[] { CeuRound2Texts.Moment.Feast, CeuRound2Texts.Moment.AllStories })
                {
                    string key = CeuRound2Texts.LineFor(m, hero);
                    Assert.IsNotNull(key, hero + " " + m);
                    StringAssert.StartsWith(hero == CharacterId.Icaro ? "Ícaro" : hero.ToString(), CeuRound2Texts.Get(null, key));
                }
            }

            Assert.IsNull(MisturaNightTexts.LineFor(MisturaNightTexts.Moment.LateShadow, null));
            Assert.IsNull(CeuRound2Texts.LineFor(CeuRound2Texts.Moment.Feast, null));
            StringAssert.Contains("{0}", MisturaNightTexts.Get(null, MisturaNightTexts.SmokeBurst));
            StringAssert.Contains("{0}", CeuRound2Texts.Get(null, CeuRound2Texts.SparkLit));
            StringAssert.Contains("{0}", CeuRound2Texts.Get(null, CeuRound2Texts.PorchKept));
            StringAssert.Contains("Vó Edna", CeuRound2Texts.Get(null, CeuRound2Texts.FeastRadio));
            StringAssert.Contains("Vô Jorge", CeuRound2Texts.Get(null, CeuRound2Texts.AllStoriesMemory));
        }
    }
}
