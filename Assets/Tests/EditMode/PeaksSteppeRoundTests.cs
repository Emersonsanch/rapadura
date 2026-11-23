using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 2026-11-23 round on the sheets 39, 51, 53 and 56 (Docs/Mapas100.md): the valley's wind in O Senhor do Cata-Vento's gusts, the
    /// Pactolo's water on O Ouro de Midas, the Baile do Vale, the silent skills of the hermitages and the rare horse of the herding.
    /// </summary>
    public class PeaksSteppeRoundTests
    {
        [SetUp]
        public void SetUp() => MidasWashBoard.Reset();

        [TearDown]
        public void TearDown() => MidasWashBoard.Reset();

        // ---------------- 39 · the valley's wind ----------------

        private static Vector2 ValleyOf(params int[] states)
        {
            var marks = new Dictionary<string, long>();
            for (int i = 0; i < states.Length; i++)
            {
                marks[WindVaneRules.StateKey(i)] = WindVaneRules.MarkOf(states[i]);
            }

            return VaneValleyWindRules.ValleyWind(key => marks.TryGetValue(key, out long mark) ? mark : 0L);
        }

        [Test]
        public void Valley_AsTheBossLeftIt_ChangesNoGust()
        {
            Vector2 valley = VaneValleyWindRules.ValleyWind(null);
            Assert.Less(valley.magnitude, 1e-4f, "one vane each way: no breeze");
            for (int g = 0; g < RegionBossRules.VaneWinds; g++)
            {
                Assert.AreEqual(VaneValleyEffect.None, VaneValleyWindRules.EffectOn(RegionBossRules.WindBlowsToward(RegionBossRules.VaneWindFor(g)), valley));
            }
        }

        [Test]
        public void Valley_AllNorth_BreaksTheNorthWind_CarriesTheSouthWind()
        {
            Vector2 valley = ValleyOf(0, 0, 0, 0);
            Assert.AreEqual(1f, valley.y, 1e-4f);

            // From the north the gust blows south: against the valley.
            Assert.AreEqual(VaneValleyEffect.Broken, VaneValleyWindRules.EffectOn(RegionBossRules.WindBlowsToward(VaneWind.North), valley));
            Assert.AreEqual(VaneValleyEffect.Carried, VaneValleyWindRules.EffectOn(RegionBossRules.WindBlowsToward(VaneWind.South), valley));
            Assert.AreEqual(VaneValleyEffect.None, VaneValleyWindRules.EffectOn(RegionBossRules.WindBlowsToward(VaneWind.East), valley));
            Assert.AreEqual(VaneValleyEffect.None, VaneValleyWindRules.EffectOn(RegionBossRules.WindBlowsToward(VaneWind.West), valley));
        }

        [Test]
        public void Valley_NeedsHalfStrength()
        {
            Assert.AreEqual(0.5f, ValleyOf(0, 0, 0, 2).magnitude, 1e-4f);
            Assert.AreEqual(VaneValleyEffect.Broken, VaneValleyWindRules.EffectOn(Vector2.down, ValleyOf(0, 0, 0, 2)), "three of four is enough");
            Assert.AreEqual(VaneValleyEffect.None, VaneValleyWindRules.EffectOn(Vector2.down, ValleyOf(0, 0, 2, 2)), "two against two: nothing");
            Assert.AreEqual(VaneValleyEffect.Broken, VaneValleyWindRules.EffectOn(Vector2.down, ValleyOf(0, 0, 1, 3)), "two north and two across that cancel: 0.5 north");
        }

        [Test]
        public void Gust_WithoutTheValley_IsTheBossesOwnRule_BrokenNeverHits_CarriedReachesFurther()
        {
            Vector2 vane = new Vector2(10f, 10f);
            foreach (VaneWind from in new[] { VaneWind.North, VaneWind.East, VaneWind.South, VaneWind.West })
            {
                Vector2 toward = RegionBossRules.WindBlowsToward(from);
                for (int x = -20; x <= 20; x += 4)
                {
                    for (int y = -20; y <= 20; y += 4)
                    {
                        Vector2 hero = vane + new Vector2(x, y);
                        Assert.AreEqual(RegionBossRules.VaneGustHits(from, vane, hero),
                            VaneValleyWindRules.GustHits(VaneValleyEffect.None, toward, vane, hero, RegionBossRules.GustReach, RegionBossRules.GustLee));
                        Assert.IsFalse(VaneValleyWindRules.GustHits(VaneValleyEffect.Broken, toward, vane, hero, RegionBossRules.GustReach, RegionBossRules.GustLee));
                    }
                }
            }

            Vector2 down = RegionBossRules.WindBlowsToward(VaneWind.North);
            Vector2 far = vane + down * 19f;
            Assert.IsFalse(VaneValleyWindRules.GustHits(VaneValleyEffect.None, down, vane, far, RegionBossRules.GustReach, RegionBossRules.GustLee));
            Assert.IsTrue(VaneValleyWindRules.GustHits(VaneValleyEffect.Carried, down, vane, far, RegionBossRules.GustReach, RegionBossRules.GustLee));
            Assert.IsFalse(VaneValleyWindRules.GustHits(VaneValleyEffect.Carried, down, vane, vane + down * 21f, RegionBossRules.GustReach, RegionBossRules.GustLee));
            Assert.IsFalse(VaneValleyWindRules.GustHits(VaneValleyEffect.Carried, down, vane, vane - down * 5f, RegionBossRules.GustReach, RegionBossRules.GustLee), "upwind stays safe");
        }

        // ---------------- 53 · the Pactolo's water on O Ouro de Midas ----------------

        [Test]
        public void Wash_FillsThreePours_KeptTenMinutes()
        {
            Assert.IsTrue(MidasWashRules.CanFill(true, 0));
            Assert.IsTrue(MidasWashRules.CanFill(true, 2));
            Assert.IsFalse(MidasWashRules.CanFill(true, MidasWashRules.Pours), "already full");
            Assert.IsFalse(MidasWashRules.CanFill(false, 0), "only in the river");
            Assert.AreEqual(3, MidasWashRules.PoursLeft(3, 100d, 100d + MidasWashRules.CarrySeconds - 1d));
            Assert.AreEqual(0, MidasWashRules.PoursLeft(3, 100d, 100d + MidasWashRules.CarrySeconds), "dried");
            Assert.AreEqual(0, MidasWashRules.PoursLeft(0, 100d, 100d));
        }

        [Test]
        public void Wash_PoursOnlyByTheLivingStatue()
        {
            Assert.IsTrue(MidasWashRules.CanPour(1, true, MidasWashRules.PourReach));
            Assert.IsFalse(MidasWashRules.CanPour(1, true, MidasWashRules.PourReach + 0.1f));
            Assert.IsFalse(MidasWashRules.CanPour(1, false, 1f));
            Assert.IsFalse(MidasWashRules.CanPour(0, true, 1f));
        }

        [Test]
        public void Wash_StopsTheTouchForSixSeconds_ALaterPourExtends()
        {
            Assert.IsFalse(MidasWashBoard.StopsTouch(10d), "dry hands: the touch works");
            MidasWashBoard.Poured(10d);
            Assert.IsTrue(MidasWashBoard.StopsTouch(15.9d));
            Assert.IsFalse(MidasWashBoard.StopsTouch(16d));
            Assert.AreEqual(1, MidasWashBoard.Stopped);
            MidasWashBoard.Poured(14d);
            Assert.AreEqual(20d, MidasWashBoard.StillUntil, 1e-9);
            Assert.AreEqual(20d, MidasWashRules.StillUntil(20d, 1d), 1e-9, "never shortened");
        }

        [Test]
        public void Wash_TheRiverIsFarFromTheStatue_SoTheWaterIsCarried()
        {
            Assert.Greater(Rapadura.Gameplay.World.Map.Areas.MapLayoutRules.DistanceToPolyline(OverworldLayouts.OuroDeMidasArena, OverworldLayouts.PactoloRiver),
                MidasWashRules.PourReach + PactoloGildRules.WashReach, "no spot is both in the river and by the statue");
        }

        // ---------------- 53 · the Baile do Vale ----------------

        [Test]
        public void Dance_Beats_SpeedUp()
        {
            Assert.AreEqual(SatyrDanceRules.CountIn, SatyrDanceRules.BeatTime(0), 1e-4f);
            Assert.AreEqual(6.6f, SatyrDanceRules.BeatTime(3), 1e-4f);
            Assert.AreEqual(7.5f, SatyrDanceRules.BeatTime(4), 1e-4f);
            Assert.AreEqual(10.9f, SatyrDanceRules.BeatTime(8), 1e-4f);
            Assert.AreEqual(13f, SatyrDanceRules.BeatTime(11), 1e-4f);
            Assert.AreEqual(24, SatyrDanceRules.MaxPoints);
        }

        [Test]
        public void Dance_EveryStepOnTheBeat_IsGold()
        {
            var run = new SatyrDanceRun();
            for (int i = 0; i < SatyrDanceRules.BeatCount; i++)
            {
                Assert.AreEqual(SatyrStep.OnBeat, run.Press(SatyrDanceRules.BeatTime(i) + 0.05f));
            }

            Assert.AreEqual(24, run.Points);
            Assert.IsTrue(run.Over(SatyrDanceRules.BeatTime(11) + 0.1f));
            Assert.AreEqual(3, SatyrDanceRules.Medal(run.Points));
        }

        [Test]
        public void Dance_CloseSteps_AndLostNotes()
        {
            var run = new SatyrDanceRun();
            Assert.AreEqual(SatyrStep.Close, run.Press(SatyrDanceRules.BeatTime(0) - 0.25f));
            Assert.AreEqual(0, run.Tick(SatyrDanceRules.BeatTime(1)), "the next note is still open");
            Assert.AreEqual(1, run.Tick(SatyrDanceRules.BeatTime(1) + SatyrDanceRules.OkWindow + 0.01f), "beat 1 went by");
            Assert.AreEqual(2, run.NextBeat);
            Assert.AreEqual(1, run.Points);
            Assert.AreEqual(1, run.Missed);
        }

        [Test]
        public void Dance_MashingNeverWins()
        {
            var run = new SatyrDanceRun();
            for (float t = 0f; t <= SatyrDanceRules.EndTime; t += 0.1f)
            {
                run.Press(t);
            }

            Assert.AreEqual(0, SatyrDanceRules.Medal(run.Points), "stumbles eat what the lucky steps gave");
            Assert.Greater(run.Stumbles, 50);
        }

        [Test]
        public void Dance_Medals_Purse_AndStep()
        {
            Assert.AreEqual(3, SatyrDanceRules.Medal(20));
            Assert.AreEqual(2, SatyrDanceRules.Medal(19));
            Assert.AreEqual(2, SatyrDanceRules.Medal(14));
            Assert.AreEqual(1, SatyrDanceRules.Medal(8));
            Assert.AreEqual(0, SatyrDanceRules.Medal(7));
            Assert.AreEqual(50, SatyrDanceRules.Purse(3, true));
            Assert.AreEqual(15, SatyrDanceRules.Purse(1, true));
            Assert.AreEqual(0, SatyrDanceRules.Purse(3, false), "once a UTC day");
            Assert.AreEqual(0, SatyrDanceRules.Purse(0, true));
            Assert.AreEqual(SatyrDanceRules.GoldBuffValue, SatyrDanceRules.BuffOf(3));
            Assert.AreEqual(SatyrDanceRules.BuffValue, SatyrDanceRules.BuffOf(1));
            Assert.AreEqual(0f, SatyrDanceRules.BuffOf(0));
        }

        [Test]
        public void Dance_StageIsInTheValeSquare()
        {
            Vector2 stage = OverworldLayouts.ValeSatirosOffset + SatyrDanceRules.StageFromTown;
            Assert.Less(Vector2.Distance(stage, OverworldLayouts.ValeSatirosOffset), OverworldLayouts.FarnkeshCityGround * 0.5f);
            Assert.IsTrue(OverworldLayouts.FarnkeshBounds.Contains(stage));
            Assert.IsTrue(SatyrDanceRules.OnStage(stage, stage + Vector2.right * 5.9f));
            Assert.IsFalse(SatyrDanceRules.LeftStage(stage, stage + Vector2.right * 7.9f));
            Assert.IsTrue(SatyrDanceRules.LeftStage(stage, stage + Vector2.right * 8.1f));
        }

        // ---------------- 56 · the silent skills ----------------

        private static readonly string[] AllSkills =
        {
            "skill_icaro_campo_magnetico", "skill_icaro_drone_de_ataque", "skill_icaro_exercito_mecanico", "skill_icaro_granada_eletrica", "skill_icaro_torreta_automatica",
            "skill_joaquim_escudo_das_raizes", "skill_joaquim_muralha_de_pedra", "skill_joaquim_provocacao_heroica", "skill_joaquim_terremoto", "skill_joaquim_ultima_resistencia",
            "skill_lavine_correntes_das_trevas", "skill_lavine_eclipse_eterno", "skill_lavine_explosao_arcana", "skill_lavine_nevoa_da_ilusao", "skill_lavine_orbe_sombria",
            "skill_maithe_chuva_de_flechas", "skill_maithe_disparo_explosivo", "skill_maithe_flecha_precisa", "skill_maithe_passo_do_vento", "skill_maithe_tempestade_de_mil_flechas",
            "skill_maria_bencao_da_vida", "skill_maria_cura_divina", "skill_maria_escudo_sagrado", "skill_maria_milagre_celestial", "skill_maria_purificacao",
        };

        [Test]
        public void Silent_EveryCousinHasAQuietSkill_AndALoudOne()
        {
            foreach (string cousin in new[] { "maithe", "icaro", "joaquim", "maria", "lavine" })
            {
                string[] own = AllSkills.Where(id => id.StartsWith("skill_" + cousin + "_")).ToArray();
                Assert.AreEqual(5, own.Length, cousin);
                Assert.IsTrue(own.Any(id => SilentSkillRules.NoiseOf(id) == SkillNoise.Quiet), cousin + " has a quiet skill");
                Assert.IsTrue(own.Any(id => SilentSkillRules.NoiseOf(id) == SkillNoise.Loud), cousin + " has a loud skill");
            }

            Assert.AreEqual(SkillNoise.Plain, SilentSkillRules.NoiseOf("skill_icaro_drone_de_ataque"));
            Assert.AreEqual(SkillNoise.Plain, SilentSkillRules.NoiseOf(null));
            Assert.AreEqual(SkillNoise.Plain, SilentSkillRules.NoiseOf("skill_arrow"));
        }

        [Test]
        public void Silent_QuietCalms_LoudFills_TheQuietFactorOnlyOnGains()
        {
            Assert.AreEqual(SilentSkillRules.QuietCalm, SilentSkillRules.CastNoise(SkillNoise.Quiet, 0.36f));
            Assert.AreEqual(SilentSkillRules.LoudNoise * 0.6f, SilentSkillRules.CastNoise(SkillNoise.Loud, 0.6f), 1e-4f);
            Assert.AreEqual(FarnkeshSilenceRules.HitNoise, SilentSkillRules.CastNoise(SkillNoise.Plain, 1f), 1e-4f, "a plain skill is heard like a blow");
            Assert.AreEqual(0f, SilentSkillRules.AfterCast(5f, SkillNoise.Quiet, 1f));
            Assert.AreEqual(FarnkeshSilenceRules.Max, SilentSkillRules.AfterCast(95f, SkillNoise.Loud, 1f));
        }

        [Test]
        public void Silent_BlowsRightAfterAQuietCast_MakeNoNoise()
        {
            Assert.AreEqual(0, SilentSkillRules.HitsHeard(4, 0f));
            Assert.AreEqual(0, SilentSkillRules.HitsHeard(4, SilentSkillRules.MuffleSeconds - 0.01f));
            Assert.AreEqual(4, SilentSkillRules.HitsHeard(4, SilentSkillRules.MuffleSeconds));
            Assert.AreEqual(4, SilentSkillRules.HitsHeard(4, float.PositiveInfinity), "never cast");
        }

        // ---------------- 51 · the rare horse of the herding ----------------

        [Test]
        public void Herd_RollIsRepeatable_AndAboutThreePercent()
        {
            Assert.AreEqual(PrairieHerdMountRules.Roll(20400, 2, 7), PrairieHerdMountRules.Roll(20400, 2, 7));
            int wins = 0;
            int total = 0;
            for (long day = 20000; day < 20800; day++)
            {
                for (int horse = 0; horse < PrairieHerdRules.HorseCount; horse++)
                {
                    double roll = PrairieHerdMountRules.Roll(day, horse, day - 19990);
                    Assert.GreaterOrEqual(roll, 0d);
                    Assert.Less(roll, 1d);
                    total++;
                    if (roll < PrairieHerdMountRules.Chance)
                    {
                        wins++;
                    }
                }
            }

            float rate = wins / (float)total;
            Assert.Greater(rate, 0.02f);
            Assert.Less(rate, 0.04f);
        }

        [Test]
        public void Herd_SureAtFortyPoints_NeverWhenOwned()
        {
            Assert.IsTrue(PrairieHerdMountRules.Earns(false, PrairieHerdMountRules.SurePoints, 0.99d));
            Assert.IsFalse(PrairieHerdMountRules.Earns(false, PrairieHerdMountRules.SurePoints - 1, 0.99d));
            Assert.IsTrue(PrairieHerdMountRules.Earns(false, 3, 0.01d));
            Assert.IsFalse(PrairieHerdMountRules.Earns(true, 100, 0d));
            Assert.AreEqual("mount_pinto_horse", PrairieHerdMountRules.MountId);
            Assert.IsNotNull(Rapadura.Gameplay.Mounts.MountCatalog.Get(PrairieHerdMountRules.MountId), "the sheet's own mount exists");
        }

        // ---------------- texts ----------------

        [Test]
        public void Texts_HaveBothLanguages_AndALineForEveryCousin()
        {
            foreach (var entry in PeaksSteppeRoundTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }

            Assert.AreEqual(PeaksSteppeRoundTexts.Entries.Count, PeaksSteppeRoundTexts.Entries.Select(e => e.Key).Distinct().Count(), "no key twice");
            var keys = new HashSet<string>(PeaksSteppeRoundTexts.Entries.Select(e => e.Key));
            foreach (string prefix in new[] { PeaksSteppeRoundTexts.VaneLinePrefix, PeaksSteppeRoundTexts.WashLinePrefix, PeaksSteppeRoundTexts.DanceLinePrefix, PeaksSteppeRoundTexts.HerdLinePrefix })
            {
                foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine })
                {
                    Assert.IsTrue(keys.Contains(PeaksSteppeRoundTexts.LineFor(prefix, hero)), prefix + hero);
                }

                Assert.IsTrue(keys.Contains(PeaksSteppeRoundTexts.LineFor(prefix, null)));
            }

            Assert.AreEqual("No tempo!", PeaksSteppeRoundTexts.Get(null, PeaksSteppeRoundTexts.DanceOnBeat));
        }
    }
}
