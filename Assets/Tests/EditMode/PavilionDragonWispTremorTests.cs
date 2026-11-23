using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Third round of the "Falta" of maps 01–36 (2026-11-23): the pavilion's door, the altar's Pétala de Luz and the Garden's night (06,
    /// <see cref="ZarafshanPavilionRules"/>), the Ninki Nanka's own glow slowing (34, <see cref="NinkiGazeRules.DragonGazeSlows"/>), the cloud of
    /// marsh lights of the freed Pescador de Luzes (33, <see cref="FisherWispCloudRules"/>) and the night tremor of the Salão de Tages (20,
    /// <see cref="TagesTremorRules"/>).
    /// </summary>
    public class PavilionDragonWispTremorTests
    {
        private static long[] Freed(int free)
        {
            var marks = new long[4];
            for (int i = 0; i < free; i++)
            {
                marks[i] = 1_700_000_000L + i;
            }

            return marks;
        }

        // ------------------------------------------------------------------
        // 06 · the pavilion
        // ------------------------------------------------------------------

        [Test]
        public void Door_StandsOnTheWestOfTheColumns_AtTheEndOfTheWestCanal()
        {
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            Vector2 door = ZarafshanPavilionRules.DoorAt(c);
            Assert.That(Vector2.Distance(door, c), Is.InRange(8f, ZarafshanGardenRules.PavilionRadius), "just outside the 8 m columns, inside the mending ring");
            Assert.Less(door.x, c.x, "west, facing the Portão do Jardim");
            ZarafshanGardenSpark west = OverworldLayouts.MihrshahSparks[2];
            Assert.Less(Vector2.Distance(door, west.CanalB), ZarafshanPavilionRules.DoorReach, "the west canal arm ends at the door");
            Assert.Greater(Vector2.Distance(door, c), ZarafshanPavilionRules.AltarReach + ZarafshanPavilionRules.DoorReach - 0.5f, "door and altar prompts do not overlap");
            Assert.IsTrue(ZarafshanPavilionRules.InGarden(c, door));
        }

        [Test]
        public void Door_SealedClosedOpen_OnlyClosedOpensWithE()
        {
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            Vector2 door = ZarafshanPavilionRules.DoorAt(c);
            Assert.AreEqual(MihrshahPavilionState.Sealed, ZarafshanPavilionRules.StateOf(Freed(3), 4, 0L));
            Assert.AreEqual(MihrshahPavilionState.Sealed, ZarafshanPavilionRules.StateOf(Freed(3), 4, 1_700_000_100L), "a door mark without the four sparks opens nothing");
            Assert.AreEqual(MihrshahPavilionState.Closed, ZarafshanPavilionRules.StateOf(Freed(4), 4, 0L));
            Assert.AreEqual(MihrshahPavilionState.Open, ZarafshanPavilionRules.StateOf(Freed(4), 4, 1_700_000_100L));

            Assert.IsTrue(ZarafshanPavilionRules.CanOpenDoor(MihrshahPavilionState.Closed, c, door + new Vector2(-2f, 0f), true));
            Assert.IsFalse(ZarafshanPavilionRules.CanOpenDoor(MihrshahPavilionState.Closed, c, door + new Vector2(-4f, 0f), true), "past 3 m");
            Assert.IsFalse(ZarafshanPavilionRules.CanOpenDoor(MihrshahPavilionState.Sealed, c, door, true));
            Assert.IsFalse(ZarafshanPavilionRules.CanOpenDoor(MihrshahPavilionState.Open, c, door, true), "opened once, for good");
            Assert.IsFalse(ZarafshanPavilionRules.CanOpenDoor(MihrshahPavilionState.Closed, c, door, false), "a fallen hero opens nothing");
        }

        [Test]
        public void Pavilion_MendsOnlyWithTheDoorOpen()
        {
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            Vector2 inside = c + new Vector2(3f, 2f);
            Assert.IsFalse(ZarafshanPavilionRules.Mends(MihrshahPavilionState.Closed, c, inside));
            Assert.IsTrue(ZarafshanPavilionRules.Mends(MihrshahPavilionState.Open, c, inside));
            Assert.IsFalse(ZarafshanPavilionRules.Mends(MihrshahPavilionState.Open, c, c + new Vector2(10f, 0f)), "past the 9 m ring");
        }

        [Test]
        public void Altar_OnePetalPerGameDay_OnlyOpen()
        {
            Assert.AreEqual(MihrshahAltarCheck.Give, ZarafshanPavilionRules.CheckAltar(MihrshahPavilionState.Open, 0, 1, true), "never picked");
            Assert.AreEqual(MihrshahAltarCheck.PickedToday, ZarafshanPavilionRules.CheckAltar(MihrshahPavilionState.Open, 5, 5, true));
            Assert.AreEqual(MihrshahAltarCheck.Give, ZarafshanPavilionRules.CheckAltar(MihrshahPavilionState.Open, 5, 6, true));
            Assert.AreEqual(MihrshahAltarCheck.Shut, ZarafshanPavilionRules.CheckAltar(MihrshahPavilionState.Closed, 0, 6, true));
            Assert.AreEqual(MihrshahAltarCheck.Dead, ZarafshanPavilionRules.CheckAltar(MihrshahPavilionState.Open, 0, 6, false));
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            Assert.IsTrue(ZarafshanPavilionRules.AtAltar(c, c + new Vector2(2f, 0f)));
            Assert.IsFalse(ZarafshanPavilionRules.AtAltar(c, c + new Vector2(3f, 0f)));
            Assert.AreEqual(3, ZarafshanPavilionRules.FirstOpenPetals);
            Assert.AreEqual(1, ZarafshanPavilionRules.DailyPetals);
        }

        [Test]
        public void Night_HidesFarBeasts_ExceptByLitWater_OpenPavilion_OrPetal()
        {
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            Vector2 byNorthCanal = c + new Vector2(4f, 30f);
            Assert.IsFalse(ZarafshanPavilionRules.NearLitCanal(sparks, Freed(0), byNorthCanal), "a dry canal gives no sight");
            Assert.IsTrue(ZarafshanPavilionRules.NearLitCanal(sparks, Freed(1), byNorthCanal), "the north arm (first spark) is lit");
            Assert.IsFalse(ZarafshanPavilionRules.NearLitCanal(sparks, Freed(1), c + new Vector2(8f, 30f)), "past 6 m of the water");

            Assert.AreEqual(ZarafshanPavilionRules.NightSight, ZarafshanPavilionRules.SightRange(true, true, true, false, false, false));
            Assert.IsFalse(BeastSightRules.Active(ZarafshanPavilionRules.SightRange(false, true, true, false, false, false)), "by day nothing is hidden");
            Assert.IsFalse(BeastSightRules.Active(ZarafshanPavilionRules.SightRange(true, false, true, false, false, false)), "outside the walls");
            Assert.IsFalse(BeastSightRules.Active(ZarafshanPavilionRules.SightRange(true, true, true, true, false, false)), "by the lit water");
            Assert.IsFalse(BeastSightRules.Active(ZarafshanPavilionRules.SightRange(true, true, true, false, true, false)), "in the open pavilion");
            Assert.IsFalse(BeastSightRules.Active(ZarafshanPavilionRules.SightRange(true, true, true, false, false, true)), "the petal's light");
            Assert.IsFalse(BeastSightRules.Active(ZarafshanPavilionRules.SightRange(true, true, false, false, false, false)), "a fallen hero sees all");
            Assert.IsFalse(ZarafshanPavilionRules.InGarden(c, c + new Vector2(60f, 0f)), "the wall stands at 58 m");
        }

        [Test]
        public void Petal_LightLastsThreeMinutes_AndTheBoardForgets()
        {
            Assert.IsTrue(ZarafshanPavilionRules.PetalLit(10f, 10f));
            Assert.IsTrue(ZarafshanPavilionRules.PetalLit(10f, 189f));
            Assert.IsFalse(ZarafshanPavilionRules.PetalLit(10f, 190f));
            Assert.IsFalse(ZarafshanPavilionRules.PetalLit(float.NegativeInfinity, 0f), "never eaten");
            LightPetalBoard.Light(100f);
            Assert.IsTrue(LightPetalBoard.Lit(150f));
            LightPetalBoard.Clear();
            Assert.IsFalse(LightPetalBoard.Lit(150f));
        }

        [Test]
        public void Petal_IsARealConsumable_WithItsTexts()
        {
            ItemDefinition petal = Resources.Load<ItemDefinition>("Items/" + ZarafshanPavilionRules.PetalItemId);
            Assert.IsNotNull(petal, "Resources/Items/item_petala_luz.asset");
            Assert.AreEqual(ZarafshanPavilionRules.PetalItemId, petal.ItemId);
            Assert.AreEqual(120f, petal.HealthRestore, 0.01f);
            Assert.AreEqual(60f, petal.ManaRestore, 0.01f);
            foreach (CharacterId hero in new[] { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine })
            {
                Assert.AreNotEqual(MihrshahPavilionTexts.LineFor(hero), MihrshahPavilionTexts.Get(null, MihrshahPavilionTexts.LineFor(hero)), "a line for " + hero);
            }
        }

        // ------------------------------------------------------------------
        // 34 · the dragon's own glow
        // ------------------------------------------------------------------

        [Test]
        public void Dragon_PoisonGaze_SlowsLikeTheCalves()
        {
            Assert.IsTrue(NinkiGazeRules.DragonGazeSlows(RegionBossRules.NinkiNankaId, true, true));
            Assert.IsFalse(NinkiGazeRules.DragonGazeSlows(RegionBossRules.NinkiNankaId, false, true), "her plain blows do not shame");
            Assert.IsFalse(NinkiGazeRules.DragonGazeSlows(RegionBossRules.LightFisherId, true, true), "another boss's poison");
            Assert.IsFalse(NinkiGazeRules.DragonGazeSlows(null, true, true));
            Assert.IsFalse(NinkiGazeRules.DragonGazeSlows(RegionBossRules.NinkiNankaId, true, false), "a fallen hero");
            Assert.AreEqual(-0.4f, NinkiGazeRules.SlowBonus, 1e-5f);
        }

        // ------------------------------------------------------------------
        // 33 · the cloud of the freed Pescador de Luzes
        // ------------------------------------------------------------------

        [Test]
        public void FisherCloud_TwoMinutesOfMana_TenMinutesOfClearNight()
        {
            const long at = 1_700_000_000L;
            Assert.IsFalse(FisherWispCloudRules.CloudOn(FisherWispCloudRules.NeverFreed, at));
            Assert.IsTrue(FisherWispCloudRules.CloudOn(at, at));
            Assert.IsTrue(FisherWispCloudRules.CloudOn(at, at + 119L));
            Assert.IsFalse(FisherWispCloudRules.CloudOn(at, at + 120L));
            Assert.AreEqual(30L, FisherWispCloudRules.CloudSecondsLeft(at, at + 90L));
            Assert.IsTrue(FisherWispCloudRules.NightClear(at, at + 599L));
            Assert.IsFalse(FisherWispCloudRules.NightClear(at, at + 600L));
            Assert.IsFalse(FisherWispCloudRules.NightClear(at, at - 5L), "a clock behind the mark clears nothing");

            Vector2 arena = OverworldLayouts.PescadorDeLuzesArena;
            Assert.IsTrue(FisherWispCloudRules.InCloud(arena, arena + new Vector2(11f, 0f)));
            Assert.IsFalse(FisherWispCloudRules.InCloud(arena, arena + new Vector2(13f, 0f)));
            Assert.AreEqual(10f, FisherWispCloudRules.ManaOver(50f, 100f, 2f), 1e-4f, "5 % a second");
            Assert.AreEqual(5f, FisherWispCloudRules.ManaOver(95f, 100f, 4f), 1e-4f, "never past full");
        }

        [Test]
        public void FisherCloud_ClearsTarchonsNightLikeALantern()
        {
            Assert.IsTrue(BeastSightRules.Active(TarchonLanternRules.SightRange(true, true, false, false)), "the lilac mist hides");
            Assert.IsFalse(BeastSightRules.Active(TarchonLanternRules.SightRange(true, true, true, false)),
                "the driver passes the freed lights as a lit lantern");
        }

        // ------------------------------------------------------------------
        // 20 · the night tremor of the Salão de Tages
        // ------------------------------------------------------------------

        [Test]
        public void Tremor_EveryCycle_WarningAtTheEnd_NightOnly()
        {
            long n = 1_700_000_000L - 1_700_000_000L % TagesTremorRules.CycleSeconds;
            Assert.AreEqual(TagesTremorPhase.Still, TagesTremorRules.PhaseAt(n));
            Assert.AreEqual(TagesTremorPhase.Still, TagesTremorRules.PhaseAt(n + 144L));
            Assert.AreEqual(TagesTremorPhase.Warning, TagesTremorRules.PhaseAt(n + 145L));
            Assert.AreEqual(TagesTremorPhase.Warning, TagesTremorRules.PhaseAt(n + 149L));
            Assert.AreEqual(1L, TagesTremorRules.SecondsToTremor(n + 149L));
            long seen = TagesTremorRules.TremorIndex(n + 10L);
            Assert.IsFalse(TagesTremorRules.Due(seen, n + 149L, true));
            Assert.IsTrue(TagesTremorRules.Due(seen, n + 150L, true));
            Assert.IsFalse(TagesTremorRules.Due(seen, n + 150L, false), "by day the hall is still");
        }

        [Test]
        public void Tremor_ShakesUnheldBurningLamps_Only()
        {
            const long tremor = 1_700_000_100L;
            Vector2 lamp = new Vector2(10f, 10f);
            Assert.IsTrue(TagesTremorRules.Steadied(lamp, new[] { new Vector2(17f, 10f) }));
            Assert.IsFalse(TagesTremorRules.Steadied(lamp, new[] { new Vector2(19f, 10f) }));
            Assert.IsFalse(TagesTremorRules.Steadied(lamp, null));

            long lit = tremor - 100L;
            Assert.AreEqual(lit - 90L, TagesTremorRules.Shaken(lit, tremor, false), "90 s less light");
            Assert.IsTrue(TagesDarkRules.Burning(TagesTremorRules.Shaken(lit, tremor, false), tremor), "200 s burnt of 300: still lit");
            Assert.AreEqual(lit, TagesTremorRules.Shaken(lit, tremor, true), "held steady");
            long old = tremor - 250L;
            Assert.IsFalse(TagesDarkRules.Burning(TagesTremorRules.Shaken(old, tremor, false), tremor), "shaken past its light: out");
            Assert.AreEqual(0L, TagesTremorRules.Shaken(0L, tremor, false), "a lamp never lit stays never lit");
            long outLamp = tremor - 400L;
            Assert.AreEqual(outLamp, TagesTremorRules.Shaken(outLamp, tremor, false), "an out lamp is not touched");
            Assert.Greater(TagesTremorRules.Shaken(50L, 100L, false), 0L, "never written back as 0 (never lit)");
        }
    }
}
