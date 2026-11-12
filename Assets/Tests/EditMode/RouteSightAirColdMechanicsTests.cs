using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Four more mechanics on the main road (Docs/Mapas100.md): the fog of the Copas de Garajonay (05, <see cref="GarajonayFogRules"/>), the 6 m
    /// sight in the sandstorm of the Estrada das Caravanas (07, <see cref="AreiasStormRules.SightRange"/>), the still air of the Mercado dos
    /// Túneis (22, <see cref="TunnelStillAirRules"/>) and the cold of the Geleira do Teto do Mundo (38, <see cref="GlacierColdRules"/>), plus the
    /// sight cut they share with the salt of Namrael (<see cref="BeastSightRules"/>): pure rules, and the places the layouts declare for them.
    /// </summary>
    public class RouteSightAirColdMechanicsTests
    {
        private const double T = 1_700_000_100d; // a multiple of 180 s: the start of a fog cycle

        // ------------------------------------------------------------------
        // The shared sight cut
        // ------------------------------------------------------------------

        [Test]
        public void Sight_HidesOnlyPastTheRange_NeverBosses_NoCutWhenNotPositive()
        {
            Assert.IsTrue(BeastSightRules.Hidden(8f, false, 8.1f));
            Assert.IsFalse(BeastSightRules.Hidden(8f, false, 8f));
            Assert.IsFalse(BeastSightRules.Hidden(8f, true, 50f), "bosses always show");
            Assert.IsFalse(BeastSightRules.Hidden(BeastSightRules.NoLimit, false, 500f));

            Assert.AreEqual(6f, BeastSightRules.Shortest(6f, 10f), 1e-6f, "storm and glare together: the shorter");
            Assert.AreEqual(10f, BeastSightRules.Shortest(0f, 10f), 1e-6f);
            Assert.AreEqual(6f, BeastSightRules.Shortest(6f, 0f), 1e-6f);
            Assert.IsFalse(BeastSightRules.Active(BeastSightRules.Shortest(0f, 0f)));
        }

        // ------------------------------------------------------------------
        // 05 · Copas de Garajonay
        // ------------------------------------------------------------------

        [Test]
        public void Fog_RisesEvery3Minutes_ForThirtyFiveSeconds()
        {
            Assert.AreEqual(0d, T % GarajonayFogRules.CycleSeconds, 1e-6);
            Assert.AreEqual(GarajonayFogPhase.Clear, GarajonayFogRules.PhaseAt(T));
            Assert.AreEqual(GarajonayFogPhase.Clear, GarajonayFogRules.PhaseAt(T + 134.9));
            Assert.AreEqual(GarajonayFogPhase.Rising, GarajonayFogRules.PhaseAt(T + 135));
            Assert.AreEqual(GarajonayFogPhase.Fog, GarajonayFogRules.PhaseAt(T + 145));
            Assert.AreEqual(GarajonayFogPhase.Fog, GarajonayFogRules.PhaseAt(T + 179.9));
            Assert.AreEqual(GarajonayFogPhase.Clear, GarajonayFogRules.PhaseAt(T + 180));
            Assert.AreEqual(GarajonayFogRules.FogIndex(T) + 1L, GarajonayFogRules.FogIndex(T + 180));
        }

        [Test]
        public void Fog_Cuts8m_OnTheFloorOnly()
        {
            Assert.AreEqual(8f, GarajonayFogRules.SightFor(GarajonayFogPhase.Fog, 0f, false, true), 1e-6f);
            Assert.AreEqual(8f, GarajonayFogRules.SightFor(GarajonayFogPhase.Fog, 5.9f, false, true), 1e-6f);
            Assert.IsFalse(BeastSightRules.Active(GarajonayFogRules.SightFor(GarajonayFogPhase.Fog, 6f, false, true)), "up on a platform: above the fog");
            Assert.IsFalse(BeastSightRules.Active(GarajonayFogRules.SightFor(GarajonayFogPhase.Fog, 0f, true, true)), "in the city");
            Assert.IsFalse(BeastSightRules.Active(GarajonayFogRules.SightFor(GarajonayFogPhase.Rising, 0f, false, true)), "only once it is up");
            Assert.IsFalse(BeastSightRules.Active(GarajonayFogRules.SightFor(GarajonayFogPhase.Fog, 0f, false, false)), "a fallen hero");

            Assert.IsTrue(OverworldLayouts.GarajonayFogBounds.Contains(OverworldLayouts.GarajonayOffset), "the city is in the map");
        }

        [Test]
        public void Fog_CrossingEarnsADrop_ThreeADay_ThreeWaiting()
        {
            Assert.IsFalse(GarajonayFogRules.Crossed(19.9f));
            Assert.IsTrue(GarajonayFogRules.Crossed(20f));

            Assert.AreEqual(0, GarajonayFogRules.EarnedOn(0L, 5), "never");
            long stored = GarajonayFogRules.StoreEarned(5, 2);
            Assert.AreEqual(2, GarajonayFogRules.EarnedOn(stored, 5));
            Assert.AreEqual(0, GarajonayFogRules.EarnedOn(stored, 6), "a new day");

            Assert.IsTrue(GarajonayFogRules.CanEarn(2, 0));
            Assert.IsFalse(GarajonayFogRules.CanEarn(3, 0), "three a day");
            Assert.IsFalse(GarajonayFogRules.CanEarn(0, 3), "three waiting for the Coletor");
        }

        // ------------------------------------------------------------------
        // 07 · Estrada das Caravanas
        // ------------------------------------------------------------------

        [Test]
        public void Storm_CutsSightTo6m_WhileItBlows_NotInTownsOrTheCalm()
        {
            Assert.AreEqual(6f, AreiasStormRules.SightRange(AreiasStormPhase.Storm, false, true), 1e-6f);
            Assert.IsFalse(BeastSightRules.Active(AreiasStormRules.SightRange(AreiasStormPhase.Warning, false, true)));
            Assert.IsFalse(BeastSightRules.Active(AreiasStormRules.SightRange(AreiasStormPhase.Calm, false, true)));
            Assert.IsFalse(BeastSightRules.Active(AreiasStormRules.SightRange(AreiasStormPhase.Storm, true, true)), "a town or a free zone");
            Assert.IsFalse(BeastSightRules.Active(AreiasStormRules.SightRange(AreiasStormPhase.Storm, false, false)), "a fallen hero");
        }

        // ------------------------------------------------------------------
        // 22 · Mercado dos Túneis
        // ------------------------------------------------------------------

        [Test]
        public void StillAir_FarFromThePipes_NoBottle_OffTheMarket()
        {
            Vector2[] pipes = OverworldLayouts.TunnelAirPipes;
            foreach (Vector2 pipe in pipes)
            {
                Assert.IsTrue(OverworldLayouts.TunnelMarketBounds.Contains(pipe), $"pipe {pipe} in the map");
            }

            Assert.IsTrue(TunnelStillAirRules.NearPipe(pipes, OverworldLayouts.MercadoTuneisPost), "the market breathes");
            Assert.IsTrue(TunnelStillAirRules.NearPipe(pipes, new Vector2(0f, -1405f)), "30 m round a pipe");
            Assert.IsFalse(TunnelStillAirRules.NearPipe(pipes, new Vector2(0f, -1425f)), "between two pipes of a gallery");
            Assert.IsFalse(TunnelStillAirRules.NearPipe(pipes, new Vector2(-200f, -1350f)), "a clearing off the galleries");

            Assert.IsTrue(TunnelStillAirRules.Stifles(true, false, false, false));
            Assert.IsFalse(TunnelStillAirRules.Stifles(true, false, false, true), "the bottle");
            Assert.IsFalse(TunnelStillAirRules.Stifles(true, false, true, false), "by a pipe");
            Assert.IsFalse(TunnelStillAirRules.Stifles(true, true, false, false), "the market");
            Assert.IsFalse(TunnelStillAirRules.Stifles(false, false, false, false), "out of the map");
            Assert.AreEqual("consumable_item_garrafa_ar_sereno", Rapadura.Gameplay.Inventory.InventoryManager.GetConsumableSourceKey(TunnelStillAirRules.BottleItemId));
        }

        [Test]
        public void StillAir_StaminaFallsSlowly_NeverUnder30Percent()
        {
            Assert.AreEqual(0.1f, TunnelStillAirRules.DrainSeconds(80f, 100f, 1f), 1e-6f, "a tenth of the run's drain");
            Assert.AreEqual(0f, TunnelStillAirRules.DrainSeconds(30f, 100f, 1f), "the floor");
            Assert.AreEqual(0f, TunnelStillAirRules.DrainSeconds(80f, 100f, 0f));
            Assert.AreEqual(-1f, TunnelStillAirRules.StaminaRegenBonus, 1e-6f, "no regen far from the pipes");
        }

        // ------------------------------------------------------------------
        // 38 · Geleira do Teto do Mundo
        // ------------------------------------------------------------------

        [Test]
        public void Cold_FillsIn100Seconds_TwiceAtNight_HalfWithTea()
        {
            Assert.AreEqual(10f, GlacierColdRules.Step(0f, true, false, false, false, 10f), 1e-4f);
            Assert.AreEqual(20f, GlacierColdRules.Step(0f, true, false, true, false, 10f), 1e-4f, "night");
            Assert.AreEqual(5f, GlacierColdRules.Step(0f, true, false, false, true, 10f), 1e-4f, "tea");
            Assert.AreEqual(100f, GlacierColdRules.Step(95f, true, false, false, false, 10f), 1e-4f, "capped");
            Assert.AreEqual(0f, GlacierColdRules.Step(90f, true, true, false, false, 0.1f), "a fire or the Cabana zeroes it");
            Assert.AreEqual(40f, GlacierColdRules.Step(50f, false, false, false, false, 1f), 1e-4f, "off the glacier it falls");

            Assert.AreEqual(GlacierColdBand.Warm, GlacierColdRules.BandOf(59.9f));
            Assert.AreEqual(GlacierColdBand.Chilly, GlacierColdRules.BandOf(60f));
            Assert.AreEqual(GlacierColdBand.Frozen, GlacierColdRules.BandOf(100f));
            Assert.AreEqual(-0.15f, GlacierColdRules.FrozenMoveBonus, 1e-6f);
        }

        [Test]
        public void Cold_TheFiresStandOnTheGlacier_TheCabanaToo()
        {
            foreach (Vector2 fire in OverworldLayouts.GlacierFires)
            {
                Assert.IsTrue(OverworldLayouts.OnGlacier(fire), $"fire {fire} on the glacier");
                Assert.IsTrue(GlacierColdRules.AtFire(OverworldLayouts.GlacierFires, fire + new Vector2(6.9f, 0f)));
            }

            Assert.IsTrue(OverworldLayouts.OnGlacier(OverworldLayouts.CabanaIaqueOffset), "the post is on the glacier (the driver warms it)");
            Assert.IsFalse(GlacierColdRules.AtFire(OverworldLayouts.GlacierFires, OverworldLayouts.CabanaIaqueOffset + new Vector2(0f, 200f)));
            Assert.IsFalse(OverworldLayouts.OnGlacier(new Vector2(-800f, 600f)), "the moraines below");
        }
    }
}
