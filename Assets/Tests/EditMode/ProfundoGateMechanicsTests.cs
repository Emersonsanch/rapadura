using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The five gates of the pits of the Fortaleza da Última Lâmpada (Docs/Mapas100.md 83, <see cref="ProfundoGateRules"/>): the seals, the
    /// checks, the breath of the pit and the gate lamp, the save marks, and the gates the layout declares (one per pit, plaza to post).
    /// </summary>
    public class ProfundoGateMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.ProfundoRegionId);

        private static readonly string[] PitMaps =
        {
            OverworldLayouts.CovaFumacaMapId, OverworldLayouts.CovaFogoMapId, OverworldLayouts.CovaVentoMapId, OverworldLayouts.CovaLodoMapId, OverworldLayouts.CovaTrevaMapId,
        };

        [Test]
        public void Check_Sealed_ThenTheTide_ThenTheZone_ThenTheSwing()
        {
            Assert.AreEqual(ProfundoGateCheck.Sealed, ProfundoGateRules.Check(false, false, true, 99f));
            Assert.AreEqual(ProfundoGateCheck.TideShut, ProfundoGateRules.Check(true, true, true, 99f));
            Assert.AreEqual(ProfundoGateCheck.ZoneBlocked, ProfundoGateRules.Check(true, false, false, 99f));
            Assert.AreEqual(ProfundoGateCheck.Resting, ProfundoGateRules.Check(true, false, true, 1f));
            Assert.AreEqual(ProfundoGateCheck.Ok, ProfundoGateRules.Check(true, false, true, 3f));
        }

        [Test]
        public void Tide_ShutsTheGates_OnlyWhileItRises()
        {
            Assert.AreEqual(ProfundoTidePhase.Calm, ProfundoLampRules.PhaseAt(10f));
            Assert.AreEqual(ProfundoTidePhase.Rising, ProfundoLampRules.PhaseAt(ProfundoLampRules.CalmSeconds + ProfundoLampRules.WarningSeconds + 1f));
        }

        [Test]
        public void Breath_IsThreePercent_FourAtNight_NeverUnderThirty()
        {
            Assert.AreEqual(3f, ProfundoGateRules.BreathAmount(100f, 100f, false), 1e-4f);
            Assert.AreEqual(4f, ProfundoGateRules.BreathAmount(100f, 100f, true), 1e-4f);
            Assert.AreEqual(1f, ProfundoGateRules.BreathAmount(31f, 100f, true), 1e-4f);
            Assert.AreEqual(0f, ProfundoGateRules.BreathAmount(29f, 100f, false));
            Assert.AreEqual(80f, ProfundoGateRules.LampAmount(1000f, false), 1e-3f);
            Assert.AreEqual(10f, ProfundoGateRules.LampAmount(1000f, true), 1e-3f);
        }

        [Test]
        public void Seals_AreSavedAsTimeMarks_OnePerGate()
        {
            var progress = new WorldMapProgress();
            Assert.IsFalse(ProfundoGateMechanicsDriver.GateOpenIn(progress, 2));
            progress.SetTimeMark(ProfundoGateRules.MarkKey(2), 1_700_000_000L);
            Assert.IsTrue(ProfundoGateMechanicsDriver.GateOpenIn(progress, 2));
            Assert.IsFalse(ProfundoGateMechanicsDriver.GateOpenIn(progress, 3));
            Assert.IsFalse(ProfundoGateMechanicsDriver.GateOpenIn(null, 2));
            var keys = new HashSet<string>();
            for (int i = 0; i < ProfundoGateRules.GateCount; i++)
            {
                Assert.IsTrue(keys.Add(ProfundoGateRules.MarkKey(i)));
            }
        }

        [Test]
        public void Finders_PickTheGateBySpotLandingAndPost()
        {
            ProfundoGate[] gates = OverworldLayouts.ProfundoGates;
            for (int i = 0; i < gates.Length; i++)
            {
                Assert.AreEqual(i, ProfundoGateRules.SpotAt(gates, gates[i].Spot), gates[i].Id);
                Assert.AreEqual(i, ProfundoGateRules.LandingAt(gates, gates[i].Landing), gates[i].Id);
                Assert.AreEqual(i, ProfundoGateRules.PostAt(gates, gates[i].Post + new Vector2(5f, 5f)), gates[i].Id);
            }

            Assert.AreEqual(-1, ProfundoGateRules.SpotAt(gates, Vector2.zero), "the plaza's centre is no gate");
            Assert.AreEqual(-1, ProfundoGateRules.PostAt(gates, gates[0].Landing + new Vector2(-10f, 0f)), "the landing does not break the seal by itself");
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_FiveGates_InTheRampart_OnePerPit()
        {
            ProfundoGate[] gates = OverworldLayouts.ProfundoGates;
            Assert.AreEqual(ProfundoGateRules.GateCount, gates.Length);
            Assert.AreEqual(ProfundoGateRules.GateCount, ProfundoGateTexts.Pits.Length);
            for (int i = 0; i < gates.Length; i++)
            {
                ProfundoGate gate = gates[i];
                Assert.Greater(gate.Spot.y, 0f, gate.Id + " is not on the north arc of the plaza");
                Assert.Less(gate.Spot.magnitude, 18f - 1.3f, gate.Id + " stands inside the rampart");
                Assert.Less(Vector2.Distance(gate.Arrival, gate.Spot), 8f, gate.Id);
                Assert.Greater(Vector2.Distance(gate.Arrival, gate.Spot), ProfundoGateRules.SpotReach * 0.5f, gate.Id);
                Assert.IsNotNull(OverworldRules.SettlementAt(World, gate.Arrival), gate.Id + " arrives outside the Fortress");

                OverworldZone zone = OverworldRules.ZoneAt(World, gate.Landing);
                Assert.IsNotNull(zone, gate.Id);
                Assert.AreEqual(PitMaps[i], zone.MapId ?? zone.OwnerMapId, gate.Id + " lands outside its pit");
                Assert.Greater(Vector2.Distance(gate.Landing, gate.Post), ProfundoGateRules.UnlockRadius, gate.Id + " would break its own seal on arrival");
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, gate.Landing, 3f), gate.Id + " lands in the water");

                float nearest = float.MaxValue;
                foreach (MapPath path in World.World.Paths)
                {
                    nearest = Mathf.Min(nearest, MapLayoutRules.DistanceToSegment(gate.Landing, path.A, path.B));
                }

                Assert.Less(nearest, 6f, gate.Id + " lands far from its post's road");
            }
        }

        [Test]
        public void Layout_TheGatesStandApart()
        {
            ProfundoGate[] gates = OverworldLayouts.ProfundoGates;
            for (int i = 0; i < gates.Length; i++)
            {
                for (int j = i + 1; j < gates.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(gates[i].Spot, gates[j].Spot), ProfundoGateRules.SpotReach * 2f, gates[i].Id + " / " + gates[j].Id);
                }
            }
        }
    }
}
