using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Pure logic behind the world map HUD (<see cref="WorldMapHudPresenter"/>,
    /// <see cref="WorldMapHudController.ShouldShow"/>): unlock goal + "Vitórias" text, region label
    /// status/text, clock/weather, carried-state parsing and the read-only Mochila summary. Also the
    /// small API it relies on (<see cref="WorldMapGraph.TryGetUnlockRule"/>,
    /// <see cref="SaveManager.TryGetCarriedJson"/>). No manager = pt-BR fallbacks.
    /// </summary>
    public class WorldMapHudPresenterTests
    {
        private class PlayerStatsProbe : ISaveable
        {
            public int Level;
            public string SaveKey => "PlayerStats";
            public object CaptureState() => new PlayerStatsSaveState { level = Level };
            public void RestoreState(object state) { }
        }

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
        }

        [TearDown]
        public void TearDown()
        {
            TestSaveRoot.Exit();
        }

        private static LocalizationManager English()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            return manager;
        }

        private static WorldMapProgress Progress(params BiomeType[] unlocked)
        {
            var progress = new WorldMapProgress();
            foreach (BiomeType region in unlocked)
            {
                progress.Unlock(region);
            }

            return progress;
        }

        private static void Win(WorldMapProgress progress, BiomeType region, int count)
        {
            for (int i = 0; i < count; i++)
            {
                progress.RecordVictory(region);
            }
        }

        // ------------------------------------------------------------------
        // Unlock goal
        // ------------------------------------------------------------------

        [Test]
        public void Graph_TryGetUnlockRule_ReadsDefaultProgression()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();

            Assert.IsTrue(graph.TryGetUnlockRule(BiomeType.Desert, out BiomeType required, out int victories));
            Assert.AreEqual(BiomeType.Forest, required);
            Assert.AreEqual(WorldMapGraph.DefaultVictoriesToUnlock, victories);
            Assert.IsFalse(graph.TryGetUnlockRule(BiomeType.Forest, out _, out _), "the starting region has no rule");
        }

        [Test]
        public void NextUnlock_NewGame_IsDesertFromForestVictories()
        {
            WorldMapProgress progress = Progress();
            Win(progress, BiomeType.Forest, 2);

            UnlockGoal goal = WorldMapHudPresenter.FindNextUnlock(WorldMapGraph.CreateDefault(), progress);

            Assert.IsTrue(goal.HasGoal);
            Assert.AreEqual(BiomeType.Desert, goal.Target);
            Assert.AreEqual(BiomeType.Forest, goal.RequiredRegion);
            Assert.AreEqual(2, goal.Victories);
            Assert.AreEqual(3, goal.Required);
            // Victories only open the boss fight; the boss unlocks the next region.
            Assert.AreEqual("Vitórias: 2/3 para enfrentar o chefe", WorldMapHudPresenter.VictoriesText(null, goal, BiomeType.Forest));
            Assert.AreEqual("Victories: 2/3 to face the boss", WorldMapHudPresenter.VictoriesText(English(), goal, BiomeType.Forest));
            // A region without a boss still unlocks the next one on victories alone.
            Assert.AreEqual("Vitórias: 2/3 para desbloquear Deserto", WorldMapHudPresenter.VictoriesText(null, goal, BiomeType.Forest, bossGated: false));
        }

        [Test]
        public void NextUnlock_GoalInAnotherRegion_NamesWhereToWin()
        {
            WorldMapProgress progress = Progress(BiomeType.Desert);
            Win(progress, BiomeType.Desert, 1);

            UnlockGoal goal = WorldMapHudPresenter.FindNextUnlock(WorldMapGraph.CreateDefault(), progress);

            Assert.AreEqual(BiomeType.Mountain, goal.Target);
            Assert.AreEqual("Vença 3 batalhas em Deserto para enfrentar o chefe (1/3)", WorldMapHudPresenter.VictoriesText(null, goal, BiomeType.Forest));
            Assert.AreEqual("Win 3 battles in Desert to face the boss (1/3)", WorldMapHudPresenter.VictoriesText(English(), goal, BiomeType.Forest));
            Assert.AreEqual("Vitórias em Deserto: 1/3 para desbloquear Montanha", WorldMapHudPresenter.VictoriesText(null, goal, BiomeType.Forest, bossGated: false));
        }

        [Test]
        public void NextUnlock_PrefersRuleOfCurrentRegion_ThenFewestMissing()
        {
            var graph = new WorldMapGraph();
            graph.SetUnlockRule(BiomeType.Desert, BiomeType.Forest, 5);
            graph.SetUnlockRule(BiomeType.Mountain, BiomeType.Caves, 1);
            WorldMapProgress progress = Progress(BiomeType.Caves); // standing on Forest

            Assert.AreEqual(BiomeType.Desert, WorldMapHudPresenter.FindNextUnlock(graph, progress).Target, "current region's rule wins");

            progress.SetCurrentRegion(BiomeType.Caves);
            Assert.AreEqual(BiomeType.Mountain, WorldMapHudPresenter.FindNextUnlock(graph, progress).Target);
        }

        [Test]
        public void NextUnlock_IgnoresRulesBehindLockedRegions_AndReportsAllUnlocked()
        {
            var graph = new WorldMapGraph();
            graph.SetUnlockRule(BiomeType.Caves, BiomeType.Mountain, 1); // Mountain still locked: not the frontier
            Assert.IsFalse(WorldMapHudPresenter.FindNextUnlock(graph, Progress()).HasGoal);

            WorldMapProgress all = Progress(BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves);
            UnlockGoal none = WorldMapHudPresenter.FindNextUnlock(WorldMapGraph.CreateDefault(), all);
            Assert.IsFalse(none.HasGoal);
            Assert.AreEqual("Todas as regiões desbloqueadas", WorldMapHudPresenter.VictoriesText(null, none, BiomeType.Forest));
            Assert.IsFalse(WorldMapHudPresenter.FindNextUnlock(null, all).HasGoal);
        }

        // ------------------------------------------------------------------
        // Region labels
        // ------------------------------------------------------------------

        [Test]
        public void LabelStatus_CurrentTravelableLockedAndFar()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            WorldMapProgress progress = Progress(BiomeType.Desert, BiomeType.Mountain);

            Assert.AreEqual(RegionLabelStatus.Current, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Forest, false));
            Assert.AreEqual(RegionLabelStatus.Travelable, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Desert, false));
            Assert.AreEqual(RegionLabelStatus.None, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Mountain, false), "unlocked but not adjacent");
            Assert.AreEqual(RegionLabelStatus.Locked, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Caves, false));
            Assert.AreEqual(RegionLabelStatus.None, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Desert, true), "no travel hint while walking");
            Assert.AreEqual(RegionLabelStatus.Current, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Forest, true));
            Assert.AreEqual(RegionLabelStatus.None, WorldMapHudPresenter.GetLabelStatus(graph, null, BiomeType.Forest, false));
        }

        [Test]
        public void LabelStatusText_ShowsRequirementWithProgress()
        {
            var graph = new WorldMapGraph();
            graph.SetUnlockRule(BiomeType.Desert, BiomeType.Forest, 3);
            WorldMapProgress locked = Progress();
            Win(locked, BiomeType.Forest, 2);

            Assert.AreEqual("Vença 3 batalhas em Floresta e derrote o chefe (2/3)",
                WorldMapHudPresenter.LabelStatusText(null, graph, locked, BiomeType.Desert, RegionLabelStatus.Locked));
            Assert.AreEqual("Win 3 battles in Forest, then beat its boss (2/3)",
                WorldMapHudPresenter.LabelStatusText(English(), graph, locked, BiomeType.Desert, RegionLabelStatus.Locked));
            Assert.AreEqual("Bloqueado",
                WorldMapHudPresenter.LabelStatusText(null, graph, locked, BiomeType.Caves, RegionLabelStatus.Locked), "no rule");
            Assert.AreEqual("Você está aqui", WorldMapHudPresenter.LabelStatusText(null, graph, locked, BiomeType.Forest, RegionLabelStatus.Current));
            Assert.AreEqual("Toque para viajar", WorldMapHudPresenter.LabelStatusText(null, graph, locked, BiomeType.Desert, RegionLabelStatus.Travelable));
            Assert.AreEqual(string.Empty, WorldMapHudPresenter.LabelStatusText(null, graph, locked, BiomeType.Desert, RegionLabelStatus.None));
        }

        [Test]
        public void VictoriesDone_ButStillLocked_PointsAtTheBossGate()
        {
            // Victories alone no longer open the next region when the boss gate applies
            // (WorldMapController passes RegionBossProgress to ApplyUnlocks): the HUD says what's left.
            var graph = new WorldMapGraph();
            graph.SetUnlockRule(BiomeType.Desert, BiomeType.Forest, 3);
            WorldMapProgress progress = Progress();
            Win(progress, BiomeType.Forest, 7);

            UnlockGoal goal = WorldMapHudPresenter.FindNextUnlock(graph, progress);
            Assert.AreEqual(3, goal.Victories, "capped at the requirement");
            Assert.AreEqual("Derrote o chefe para desbloquear Deserto", WorldMapHudPresenter.VictoriesText(null, goal, BiomeType.Forest));
            Assert.AreEqual("Beat the boss to unlock Desert", WorldMapHudPresenter.VictoriesText(English(), goal, BiomeType.Forest));
            Assert.AreEqual("Derrote o chefe de Floresta para desbloquear Deserto", WorldMapHudPresenter.VictoriesText(null, goal, BiomeType.Desert), "goal in another region names it");
            Assert.AreEqual("Beat the Forest boss to unlock Desert", WorldMapHudPresenter.VictoriesText(English(), goal, BiomeType.Desert));
            Assert.AreEqual("Derrote o chefe de Floresta",
                WorldMapHudPresenter.LabelStatusText(null, graph, progress, BiomeType.Desert, RegionLabelStatus.Locked));
        }

        [Test]
        public void GoalText_BeforeAndAfterTheBossFightIsOffered()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            WorldMapProgress progress = Progress();

            Assert.AreEqual("Vitórias: 0/3 para enfrentar o chefe", WorldMapHudPresenter.GoalText(null, graph, progress));
            Win(progress, BiomeType.Forest, 3);
            Assert.AreEqual("Derrote o chefe para desbloquear Deserto", WorldMapHudPresenter.GoalText(null, graph, progress));
            Assert.AreEqual("Beat the boss to unlock Desert", WorldMapHudPresenter.GoalText(English(), graph, progress));
            Assert.AreEqual(string.Empty, WorldMapHudPresenter.GoalText(null, graph, null));
        }

        [Test]
        public void GoalText_ClearedRegion_SaysSoAndNamesTheNextGoal()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            WorldMapProgress progress = Progress(BiomeType.Desert);
            Win(progress, BiomeType.Forest, 3);
            progress.MarkRegionCleared(BiomeType.Forest);

            // Still standing in Forest: no bare "Vitórias em Deserto: 0/3 para desbloquear Montanha".
            Assert.AreEqual("Região Floresta concluída! Próximo: Vença 3 batalhas em Deserto para enfrentar o chefe (0/3)",
                WorldMapHudPresenter.GoalText(null, graph, progress));
            Assert.AreEqual("Forest cleared! Next: Win 3 battles in Desert to face the boss (0/3)",
                WorldMapHudPresenter.GoalText(English(), graph, progress));

            // Walking into Desert, the goal is local again.
            progress.SetCurrentRegion(BiomeType.Desert);
            Win(progress, BiomeType.Desert, 1);
            Assert.AreEqual("Vitórias: 1/3 para enfrentar o chefe", WorldMapHudPresenter.GoalText(null, graph, progress));
        }

        [Test]
        public void GoalText_LastRegion_ShowsItsOwnBoss_ThenAllDone()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            WorldMapProgress progress = Progress(BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves);
            progress.SetCurrentRegion(BiomeType.Caves);
            Win(progress, BiomeType.Caves, 1);

            // Caves gates nothing, but its boss is still the goal (not "Todas as regiões desbloqueadas").
            Assert.AreEqual("Vitórias: 1/3 para enfrentar o chefe", WorldMapHudPresenter.GoalText(null, graph, progress));
            Win(progress, BiomeType.Caves, 2);
            Assert.AreEqual("Derrote o chefe de Cavernas", WorldMapHudPresenter.GoalText(null, graph, progress));

            progress.MarkRegionCleared(BiomeType.Caves);
            Assert.AreEqual("Região Cavernas concluída! Todas as regiões desbloqueadas", WorldMapHudPresenter.GoalText(null, graph, progress));
            Assert.AreEqual("Caves cleared! Every region unlocked", WorldMapHudPresenter.GoalText(English(), graph, progress));
        }

        [Test]
        public void LabelStatusText_ClearedRegion_IsMarked()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            WorldMapProgress progress = Progress(BiomeType.Desert, BiomeType.Mountain);
            progress.MarkRegionCleared(BiomeType.Forest);
            progress.SetCurrentRegion(BiomeType.Mountain);

            Assert.IsTrue(WorldMapHudPresenter.IsCleared(progress, null, BiomeType.Forest));
            Assert.IsFalse(WorldMapHudPresenter.IsCleared(progress, null, BiomeType.Desert));
            Assert.AreEqual(RegionLabelStatus.None, WorldMapHudPresenter.GetLabelStatus(graph, progress, BiomeType.Forest, false));
            Assert.AreEqual("Concluída", WorldMapHudPresenter.LabelStatusText(null, graph, progress, BiomeType.Forest, RegionLabelStatus.None));
            Assert.AreEqual("Cleared", WorldMapHudPresenter.LabelStatusText(English(), graph, progress, BiomeType.Forest, RegionLabelStatus.None));
            Assert.AreEqual(string.Empty, WorldMapHudPresenter.LabelStatusText(null, graph, progress, BiomeType.Desert, RegionLabelStatus.None));
            // The "aqui"/"viajar" hints still win on a cleared region (its marker is the label's check badge).
            Assert.AreEqual("Você está aqui", WorldMapHudPresenter.LabelStatusText(null, graph, progress, BiomeType.Forest, RegionLabelStatus.Current));
        }

        [Test]
        public void RegionLabels_HideWhileAModalPausesGameplay()
        {
            Assert.IsTrue(WorldMapHudController.ShouldHideRegionLabels(true));
            Assert.IsFalse(WorldMapHudController.ShouldHideRegionLabels(false));
        }

        // ------------------------------------------------------------------
        // Top bar text
        // ------------------------------------------------------------------

        [Test]
        public void HeroRegionAndLevel_Texts()
        {
            Assert.AreEqual("Maithe", WorldMapHudPresenter.HeroName(null, CharacterId.Maithe));
            Assert.AreEqual("Sem herói", WorldMapHudPresenter.HeroName(null, null));
            Assert.AreEqual("Nv. 3", WorldMapHudPresenter.LevelText(null, 3));
            Assert.AreEqual("Nv. 1", WorldMapHudPresenter.LevelText(null, 0));

            Assert.AreEqual("Floresta", WorldMapHudPresenter.RegionName(null, BiomeType.Forest));
            Assert.AreEqual("Cavernas", WorldMapHudPresenter.RegionName(null, BiomeType.Caves));
            Assert.AreEqual("Mountain", WorldMapHudPresenter.RegionName(English(), BiomeType.Mountain));
        }

        [Test]
        public void ClockText_WrapsHourAndLocalizesPhase()
        {
            Assert.AreEqual("Dia 3 · 14h · Noite", WorldMapHudPresenter.ClockText(null, 3, 14, TimeOfDayPhase.Night));
            Assert.AreEqual("Dia 1 · 01h · Amanhecer", WorldMapHudPresenter.ClockText(null, 0, 25, TimeOfDayPhase.Dawn));
            Assert.AreEqual("Day 2 · 07:00 · Day", WorldMapHudPresenter.ClockText(English(), 2, 7, TimeOfDayPhase.Day));
        }

        [Test]
        public void Weather_TextAndIconClass()
        {
            Assert.AreEqual("Tempestade de areia", WorldMapHudPresenter.WeatherText(null, WeatherType.Sandstorm));
            Assert.AreEqual("Snow", WorldMapHudPresenter.WeatherText(English(), WeatherType.Snow));
            Assert.AreEqual("wm-weather-icon--rain", WorldMapHudPresenter.WeatherIconClass(WeatherType.Rain));
            Assert.AreEqual("wm-weather-icon--clear", WorldMapHudPresenter.WeatherIconClass((WeatherType)99));
            Assert.AreSame(WorldMapHudPresenter.WeatherIconClass(WeatherType.Fog), WorldMapHudPresenter.WeatherIconClass(WeatherType.Fog), "no per-call allocation");

            foreach (WeatherType weather in System.Enum.GetValues(typeof(WeatherType)))
            {
                CollectionAssert.Contains(WorldMapHudPresenter.AllWeatherIconClasses, WorldMapHudPresenter.WeatherIconClass(weather));
            }
        }

        // ------------------------------------------------------------------
        // Carried state + Mochila
        // ------------------------------------------------------------------

        [Test]
        public void ParseLevel_FromCarriedJson_DefaultsToOne()
        {
            Assert.AreEqual(4, WorldMapHudPresenter.ParseLevel(JsonUtility.ToJson(new PlayerStatsSaveState { level = 4 })));
            Assert.AreEqual(1, WorldMapHudPresenter.ParseLevel(null));
            Assert.AreEqual(1, WorldMapHudPresenter.ParseLevel("not json"));
            Assert.AreEqual(1, WorldMapHudPresenter.ParseLevel(JsonUtility.ToJson(new PlayerStatsSaveState { level = 0 })));
        }

        [Test]
        public void SaveManager_TryGetCarriedJson_PeeksWithoutConsuming()
        {
            var manager = new SaveManager();
            manager.Initialize();
            Assert.IsFalse(manager.TryGetCarriedJson("PlayerStats", out _));

            var player = new PlayerStatsProbe { Level = 6 };
            manager.Register(player);
            manager.Unregister(player); // Main.unity unloads

            Assert.IsTrue(manager.TryGetCarriedJson("PlayerStats", out string json));
            Assert.AreEqual(6, WorldMapHudPresenter.ParseLevel(json));
            Assert.IsTrue(manager.HasCarriedState("PlayerStats"), "peeking must not drop the carry");
            Assert.IsFalse(manager.TryGetCarriedJson(null, out _));
        }

        private static InventorySaveState SampleInventory()
        {
            return new InventorySaveState
            {
                slots = new[]
                {
                    new InventorySlotData { itemId = "item_gold", quantity = 50 },
                    new InventorySlotData { itemId = "item_axe", quantity = 1 },
                    new InventorySlotData { itemId = "item_fiber", quantity = 3 },
                    new InventorySlotData { itemId = "item_fiber", quantity = 2 },
                    new InventorySlotData(),
                    null,
                },
                equipped = new[]
                {
                    new InventorySaveState.EquippedEntry { itemId = "item_axe" },
                    new InventorySaveState.EquippedEntry(),
                },
            };
        }

        [Test]
        public void Summarize_CountsSlotsGoldEquippedAndMergesStacks()
        {
            BackpackSummary summary = WorldMapHudPresenter.Summarize(SampleInventory());

            Assert.AreEqual(4, summary.UsedSlots);
            Assert.AreEqual(6, summary.TotalSlots);
            Assert.AreEqual(6, summary.TotalItems, "gold is not an item row");
            Assert.AreEqual(50, summary.Gold);
            Assert.AreEqual(1, summary.Equipped);
            Assert.AreEqual(2, summary.Items.Count);
            Assert.AreEqual(new KeyValuePair<string, int>("item_fiber", 5), summary.Items[0]);
            Assert.AreEqual(new KeyValuePair<string, int>("item_axe", 1), summary.Items[1]);

            Assert.IsTrue(WorldMapHudPresenter.Summarize(null).IsEmpty);
        }

        [Test]
        public void Summarize_RoundTripsThroughCarriedJson()
        {
            string json = JsonUtility.ToJson(SampleInventory());
            BackpackSummary summary = WorldMapHudPresenter.Summarize(WorldMapHudPresenter.ParseInventory(json));

            Assert.AreEqual(50, summary.Gold);
            Assert.AreEqual(6, summary.TotalItems);
            Assert.IsNull(WorldMapHudPresenter.ParseInventory("{broken"));
            Assert.IsNull(WorldMapHudPresenter.ParseInventory(string.Empty));
        }

        [Test]
        public void BackpackLines_ListsTotalsAndBiggestStacks()
        {
            BackpackSummary summary = WorldMapHudPresenter.Summarize(SampleInventory());

            List<string> lines = WorldMapHudPresenter.BackpackLines(null, summary, maxItems: 1);

            CollectionAssert.AreEqual(new[]
            {
                "Itens: 6 (4/6 espaços)",
                "Ouro: 50",
                "Equipados: 1",
                "Fibra Vegetal ×5",
                "+1 outros",
            }, lines);

            List<string> custom = WorldMapHudPresenter.BackpackLines(null, summary, 5, id => id.ToUpperInvariant());
            CollectionAssert.Contains(custom, "ITEM_AXE ×1");
        }

        [Test]
        public void BackpackLines_EmptyBackpack()
        {
            CollectionAssert.AreEqual(new[] { "A mochila está vazia." }, WorldMapHudPresenter.BackpackLines(null, new BackpackSummary()));
            CollectionAssert.AreEqual(new[] { "The backpack is empty." }, WorldMapHudPresenter.BackpackLines(English(), null));
        }

        [Test]
        public void ItemDisplayName_UsesContentTextsThenId()
        {
            Assert.AreEqual("Machado", WorldMapHudPresenter.ItemDisplayName(null, "item_axe"));
            Assert.AreEqual("Axe", WorldMapHudPresenter.ItemDisplayName(English(), "item_axe"));
            Assert.AreEqual("item_unknown_thing", WorldMapHudPresenter.ItemDisplayName(null, "item_unknown_thing"));
            Assert.AreEqual(string.Empty, WorldMapHudPresenter.ItemDisplayName(null, null));
        }

        // ------------------------------------------------------------------
        // Visibility rule
        // ------------------------------------------------------------------

        [TestCase(GameFlowState.Boot, true, false)]
        [TestCase(GameFlowState.MainMenu, true, false)]
        [TestCase(GameFlowState.CharacterSelect, true, false)]
        [TestCase(GameFlowState.Encounter, true, false)]
        [TestCase(GameFlowState.World, false, false)]
        [TestCase(GameFlowState.World, true, true)]
        public void ShouldShow_OnlyInWorldWithHero(GameFlowState state, bool hasHero, bool expected)
        {
            Assert.AreEqual(expected, WorldMapHudController.ShouldShow(true, state, hasHero));
        }

        [Test]
        public void ShouldShow_AlwaysWithoutAFlow()
        {
            Assert.IsTrue(WorldMapHudController.ShouldShow(false, GameFlowState.Boot, false), "WorldMap.unity played on its own");
        }

        // ------------------------------------------------------------------
        // Localization
        // ------------------------------------------------------------------

        [Test]
        public void Texts_ResolveUiAndCoreKeysForTheDocumentLocalizer()
        {
            Assert.AreEqual("Missões", WorldMapTexts.Get(null, Rapadura.UI.Common.UiTexts.HudQuests));
            Assert.AreEqual("Deserto", WorldMapTexts.Get(null, "biome.desert.name"));
            Assert.AreEqual("Fechar", WorldMapTexts.Get(null, WorldMapTexts.Close));
            Assert.AreEqual("Close", WorldMapTexts.Get(English(), WorldMapTexts.Close));
            Assert.AreEqual("Quests", WorldMapTexts.Get(English(), Rapadura.UI.Common.UiTexts.HudQuests));
            Assert.AreEqual("unknown.key", WorldMapTexts.Get(null, "unknown.key"));
        }
    }
}
