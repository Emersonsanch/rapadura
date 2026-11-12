using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Four mechanics on the road of Regions 2 and 3 (Docs/Mapas100.md): the sunbeams of the Galerias do Karez (09, <see cref="KarezSunbeamRules"/>),
    /// the song of the Dunas que Cantam (10, <see cref="DuneSongRules"/>), the blinding salt of the Trono de Namrael (12,
    /// <see cref="NamraelGlareRules"/>) and the thin air of the Cume dos Mazans (18, <see cref="MazanThinAirRules"/>): pure rules, and the places the
    /// layouts declare for them inside their maps.
    /// </summary>
    public class SouthSandsSummitMechanicsTests
    {
        // ------------------------------------------------------------------
        // 09 · Galerias do Karez
        // ------------------------------------------------------------------

        [Test]
        public void Karez_ManaComesBackInTheBeams_SlowInTheDark_OnlyByDay()
        {
            Assert.AreEqual(2.5f, KarezSunbeamRules.HeroManaFactor(true, false, true, true), 1e-6f);
            Assert.AreEqual(0.5f, KarezSunbeamRules.HeroManaFactor(true, false, true, false), 1e-6f);
            Assert.AreEqual(1f, KarezSunbeamRules.HeroManaFactor(true, false, false, false), "night: the lamps");
            Assert.AreEqual(1f, KarezSunbeamRules.HeroManaFactor(true, true, true, false), "the post is ordinary ground");
            Assert.AreEqual(1f, KarezSunbeamRules.HeroManaFactor(false, false, true, false), "outside the galleries");

            Assert.IsTrue(KarezSunbeamRules.IsDay(1f));
            Assert.IsTrue(KarezSunbeamRules.IsDay(0.5f));
            Assert.IsFalse(KarezSunbeamRules.IsDay(0.49f));
        }

        [Test]
        public void Karez_BeastsInABeamAreDazzled_NeverBosses()
        {
            Assert.IsTrue(KarezSunbeamRules.Dazzled(true, true, false));
            Assert.IsFalse(KarezSunbeamRules.Dazzled(true, true, true), "bosses");
            Assert.IsFalse(KarezSunbeamRules.Dazzled(false, true, false), "night");
            Assert.IsFalse(KarezSunbeamRules.Dazzled(true, false, false), "in the dark");
            Assert.Less(KarezSunbeamRules.DazzleDamageFactor, 1f);
            Assert.Less(KarezSunbeamRules.DazzleSpeedFactor, 1f);
        }

        [Test]
        public void Karez_BeamsFallDownEveryFourthShaft_InsideTheGalleries()
        {
            var beams = new List<Vector2>();
            KarezSunbeamRules.AddBeamsAlong(beams, Vector2.zero, new Vector2(100f, 0f), 10f);
            Assert.AreEqual(3, beams.Count, "shafts 0, 4 and 8 of 0…10");
            Assert.AreEqual(new Vector2(40f, 0f), beams[1]);
            Assert.AreEqual(1, KarezSunbeamRules.BeamAt(beams, new Vector2(45.9f, 0f)));
            Assert.AreEqual(-1, KarezSunbeamRules.BeamAt(beams, new Vector2(46.1f, 0f)), "6 m round the shaft");

            Vector2[] karez = OverworldLayouts.KarezSunbeams;
            Assert.GreaterOrEqual(karez.Length, 12, "enough beams to fight under");
            foreach (Vector2 beam in karez)
            {
                Assert.IsTrue(OverworldLayouts.KarezGalleriesBounds.Contains(beam), $"beam {beam} inside the galleries");
            }

            for (int i = 0; i < karez.Length; i++)
            {
                for (int j = i + 1; j < karez.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(karez[i], karez[j]), 2f * KarezSunbeamRules.BeamRadius, "beams never merge");
                }
            }

            Assert.AreEqual(-1, KarezSunbeamRules.BeamAt(karez, OverworldLayouts.BocaDoKarezOffset), "the post has its own light");
        }

        // ------------------------------------------------------------------
        // 10 · Dunas que Cantam
        // ------------------------------------------------------------------

        [Test]
        public void Dune_ASlideOfFourMetresSings()
        {
            var slide = new DuneSlide();
            for (int i = 0; i < 15; i++)
            {
                Assert.IsFalse(slide.Feed(-0.25f, 5f), $"frame {i}");
            }

            Assert.IsTrue(slide.Feed(-0.25f, 5f), "4 m down");
            Assert.AreEqual(0f, slide.Descent, 1e-6f, "starts again");
        }

        [Test]
        public void Dune_StoppingOrClimbingBreaksTheSlide()
        {
            var slide = new DuneSlide();
            slide.Feed(-3f, 5f);
            Assert.IsFalse(slide.Feed(-2f, 1.5f), "too slow: not a slide");
            Assert.AreEqual(0f, slide.Descent, 1e-6f);

            slide.Feed(-3f, 5f);
            slide.Feed(0.3f, 5f);
            Assert.AreEqual(3f, slide.Descent, 1e-5f, "a bump of 0.3 m does not break it");
            Assert.IsTrue(slide.Feed(-1f, 5f));

            slide.Feed(-3f, 5f);
            slide.Feed(0.5f, 5f);
            Assert.AreEqual(0f, slide.Descent, 1e-6f, "climbing 0.5 m does");
        }

        [Test]
        public void Dune_SingsOncePer30Seconds_FrightensCommonsWithin14m()
        {
            Assert.IsTrue(DuneSongRules.CanSing(float.NegativeInfinity, 0f));
            Assert.IsTrue(DuneSongRules.CanSing(float.NaN, 0f));
            Assert.IsFalse(DuneSongRules.CanSing(100f, 129.9f));
            Assert.IsTrue(DuneSongRules.CanSing(100f, 130f));
            Assert.AreEqual(10f, DuneSongRules.CooldownLeft(100f, 120f), 1e-4f);

            Assert.IsTrue(DuneSongRules.Scares(false, 14f));
            Assert.IsFalse(DuneSongRules.Scares(false, 14.1f));
            Assert.IsFalse(DuneSongRules.Scares(true, 1f), "never a boss");
            Assert.AreEqual(4f, DuneSongRules.ScareSeconds, 1e-6f);
        }

        [Test]
        public void Dune_TheCrescentLakeAndThePostDoNotSing()
        {
            Assert.IsTrue(OverworldLayouts.InDunesCrescent(OverworldLayouts.DunesCrescentCentre + new Vector2(0f, -115f)), "the bottom of the arc");
            Assert.IsFalse(OverworldLayouts.InDunesCrescent(OverworldLayouts.DunesCrescentCentre), "the sand inside the crescent");
            Assert.IsFalse(OverworldLayouts.InDunesCrescent(OverworldLayouts.DunesCrescentCentre + new Vector2(0f, 115f)), "the open side");
            Assert.IsTrue(OverworldLayouts.SingingDunesBounds.Contains(OverworldLayouts.PocoTamareirasOffset), "the post is in the map (the driver skips it)");
            Assert.IsFalse(OverworldLayouts.InDunesCrescent(OverworldLayouts.PocoTamareirasOffset));
        }

        // ------------------------------------------------------------------
        // 12 · Trono de Namrael
        // ------------------------------------------------------------------

        [Test]
        public void Salt_BlindsAtNoon_WarnsHalfAnHourBefore()
        {
            Assert.AreEqual(NamraelGlarePhase.None, NamraelGlareRules.PhaseAt(10.49f));
            Assert.AreEqual(NamraelGlarePhase.Rising, NamraelGlareRules.PhaseAt(10.5f));
            Assert.AreEqual(NamraelGlarePhase.Glare, NamraelGlareRules.PhaseAt(11f));
            Assert.AreEqual(NamraelGlarePhase.Glare, NamraelGlareRules.PhaseAt(13.99f));
            Assert.AreEqual(NamraelGlarePhase.None, NamraelGlareRules.PhaseAt(14f));
            Assert.AreEqual(NamraelGlarePhase.None, NamraelGlareRules.PhaseAt(0f), "night: good sight under the moon");
            Assert.AreEqual(NamraelGlarePhase.Glare, NamraelGlareRules.PhaseAt(36f), "wraps the day");
        }

        [Test]
        public void Salt_HidesBeastsPast10m_NotInShadeNorOnThePost_NeverBosses()
        {
            Assert.IsTrue(NamraelGlareRules.Blinded(NamraelGlarePhase.Glare, true, false, false));
            Assert.IsFalse(NamraelGlareRules.Blinded(NamraelGlarePhase.Rising, true, false, false));
            Assert.IsFalse(NamraelGlareRules.Blinded(NamraelGlarePhase.Glare, true, true, false), "shade");
            Assert.IsFalse(NamraelGlareRules.Blinded(NamraelGlarePhase.Glare, true, false, true), "the post");
            Assert.IsFalse(NamraelGlareRules.Blinded(NamraelGlarePhase.Glare, false, false, false), "off the salt");

            Assert.IsTrue(NamraelGlareRules.Hidden(true, false, 10.1f));
            Assert.IsFalse(NamraelGlareRules.Hidden(true, false, 10f));
            Assert.IsFalse(NamraelGlareRules.Hidden(true, true, 40f), "bosses always show");
            Assert.IsFalse(NamraelGlareRules.Hidden(false, false, 40f));
        }

        [Test]
        public void Salt_TheTowersGiveShade_OnTheSalt()
        {
            Vector2[] towers = OverworldLayouts.NamraelShadeTowers;
            Assert.AreEqual(6, towers.Length);
            foreach (Vector2 tower in towers)
            {
                Assert.IsTrue(OverworldLayouts.NamraelSaltBounds.Contains(tower), $"tower {tower} on the salt");
                Assert.IsTrue(NamraelGlareRules.InShade(towers, tower + new Vector2(13.9f, 0f)));
            }

            Assert.IsFalse(NamraelGlareRules.InShade(towers, OverworldLayouts.NamraelThrone), "the throne is in the full glare");
            Assert.IsTrue(OverworldLayouts.NamraelSaltBounds.Contains(OverworldLayouts.NamraelThrone));
            Assert.IsTrue(OverworldLayouts.NamraelSaltBounds.Contains(OverworldLayouts.TorreDeTaipaOffset));
        }

        // ------------------------------------------------------------------
        // 18 · Cume dos Mazans
        // ------------------------------------------------------------------

        [Test]
        public void ThinAir_StaminaComesBack30PercentSlowerOnTheTop_MaitheIgnores()
        {
            Rect plateau = OverworldLayouts.MazanThinAirBounds;
            Assert.IsTrue(plateau.Contains(OverworldLayouts.MazanRing), "Mazan's ring is on the top plateau");
            Assert.IsFalse(plateau.Contains(OverworldLayouts.EstelaOffset), "the Estela Alta is below");

            Assert.AreEqual(0.7f, MazanThinAirRules.StaminaFactor(plateau, OverworldLayouts.MazanRing, CharacterId.Joaquim), 1e-6f);
            Assert.AreEqual(0.7f, MazanThinAirRules.StaminaFactor(plateau, OverworldLayouts.MazanRing, null), 1e-6f);
            Assert.AreEqual(1f, MazanThinAirRules.StaminaFactor(plateau, OverworldLayouts.MazanRing, CharacterId.Maithe), 1e-6f);
            Assert.AreEqual(1f, MazanThinAirRules.StaminaFactor(plateau, OverworldLayouts.EstelaOffset, CharacterId.Joaquim), 1e-6f);
            Assert.AreEqual(-0.3f, MazanThinAirRules.StaminaRegenBonus, 1e-6f);
        }
    }
}
