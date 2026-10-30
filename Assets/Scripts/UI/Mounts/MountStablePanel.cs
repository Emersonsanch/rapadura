using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Mounts
{
    /// <summary>
    /// "Estábulo e Caderno de Montarias" (Docs/Mounts.md §4, §6, §7, phase M2), a modal panel on the world map HUD
    /// opened by the "Montarias" button of <see cref="WorldMapMountHud"/>. Built in code (look in Resources/UI/Mounts.uss).
    /// Three tabs:
    /// <list type="bullet">
    /// <item><b>Nossas montarias</b>: per owned mount its level/XP, hearts, satiety (hungry), health or rest, saddle and
    /// blanket (cycle through the owned ones), "Usar esta" (active mount), "Fazer carinho" and the foods it eats
    /// (paid from the carried bag through <see cref="MountFeeding"/>); plus the house's stalls (<see cref="MountStable"/>).</item>
    /// <item><b>Caderno</b>: the 30 species of <see cref="MountCodex"/>, "???" + hint for the ones not seen yet.</item>
    /// <item><b>Selaria</b>: saddles and blankets for gold, only while the cousins are in the Vila (Haras do Srosh).</item>
    /// </list>
    /// While open it holds GameplayPause and a live <see cref="CarriedInventorySession"/>, handed back on close.
    /// </summary>
    public sealed class MountStablePanel
    {
        public const string PanelName = "mount-stable-panel";
        public const string ScreenId = "mount_stable";

        private enum Tab
        {
            Mounts,
            Codex,
            Saddlery,
        }

        private readonly VisualElement _overlay;
        private readonly Label _title;
        private readonly Button[] _tabs = new Button[3];
        private readonly ScrollView _content;
        private readonly Label _status;
        private readonly Button _closeButton;
        private Tab _tab = Tab.Mounts;
        private CarriedInventorySession _session;
        private InventoryManager _bag;
        private WorldMapController _map;

        public VisualElement Root => _overlay;
        public bool IsOpen => _overlay.style.display == DisplayStyle.Flex;

        public MountStablePanel(bool reducedMotion)
        {
            _overlay = new VisualElement { name = PanelName };
            _overlay.AddToClassList(PanelName);
            MountHudStyles.ApplyTo(_overlay, reducedMotion);
            _overlay.style.display = DisplayStyle.None;

            var window = new VisualElement();
            window.AddToClassList("rap-panel");
            window.AddToClassList("mount-stable-window");
            _overlay.Add(window);

            _title = new Label();
            _title.AddToClassList("mount-stable-title");
            window.Add(_title);

            var tabRow = new VisualElement();
            tabRow.AddToClassList("mount-stable-tabs");
            for (int i = 0; i < _tabs.Length; i++)
            {
                Tab tab = (Tab)i;
                var button = new Button(() => { _tab = tab; Rebuild(); }) { focusable = true };
                button.AddToClassList("mount-stable-tab");
                _tabs[i] = button;
                tabRow.Add(button);
            }

            window.Add(tabRow);

            _content = new ScrollView(ScrollViewMode.Vertical);
            _content.AddToClassList("mount-stable-content");
            window.Add(_content);

            _status = new Label();
            _status.AddToClassList("mount-stable-status");
            window.Add(_status);

            _closeButton = new Button(Close) { focusable = true };
            _closeButton.AddToClassList("mount-stable-close");
            window.Add(_closeButton);
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static string T(string key) => MountTexts.Get(Localization, key);

        private static string F(string key, params object[] args) => MountTexts.Format(Localization, key, args);

        private static string AwakeningNumeral(int awakening)
        {
            string[] numerals = { "I", "II", "III", "IV" };
            return numerals[Mathf.Clamp(awakening, 1, numerals.Length) - 1];
        }

        // ------------------------------------------------------------------

        public void Open(WorldMapController map)
        {
            _map = map;
            if (_session == null && ServiceLocator.TryGet(out SaveManager saveManager))
            {
                _session = new CarriedInventorySession(saveManager);
            }

            _bag = _session?.Open();
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            EventBus.Subscribe<MountCollectionChangedEvent>(OnChanged);
            _status.text = string.Empty;
            Rebuild();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            EventBus.Unsubscribe<MountCollectionChangedEvent>(OnChanged);
            _overlay.style.display = DisplayStyle.None;
            _session?.Close();
            _bag = null;
            GameplayPause.Release(this);
        }

        private void OnChanged(MountCollectionChangedEvent evt) => Rebuild();

        /// <summary>The cousins stand in the Vila (the Haras and its Selaria are there).</summary>
        public static bool IsInVila(WorldMapController map)
        {
            return map != null && map.CanExplore && map.CurrentNode != null && map.CurrentNode.Region == BiomeType.Forest;
        }

        private void Rebuild()
        {
            _title.text = T(MountTexts.StableTitle);
            _closeButton.text = T(MountTexts.Close);
            string[] tabKeys = { MountTexts.TabMounts, MountTexts.TabCodex, MountTexts.TabSaddlery };
            for (int i = 0; i < _tabs.Length; i++)
            {
                _tabs[i].text = T(tabKeys[i]);
                _tabs[i].EnableInClassList("mount-stable-tab--on", (int)_tab == i);
            }

            _content.Clear();
            switch (_tab)
            {
                case Tab.Mounts: BuildMounts(); break;
                case Tab.Codex: BuildCodex(); break;
                default: BuildSaddlery(); break;
            }
        }

        // ------------------------------------------------------------------
        // Tabs
        // ------------------------------------------------------------------

        private void BuildMounts()
        {
            MountCollection collection = MountCollection.ResolveShared();
            MountStableInfo stable = MountStable.Current();
            AddLine(stable.HasHouse ? F(MountTexts.StallsLine, stable.Stalls) : T(MountTexts.NoHouse), "mount-stable-note");
            if (collection.Count == 0)
            {
                AddLine(T(MountTexts.Empty), "mount-stable-note");
                return;
            }

            foreach (OwnedMount mount in collection.Mounts)
            {
                MountDefinition definition = MountCatalog.Get(mount.mountId);
                if (definition == null)
                {
                    continue;
                }

                var card = new VisualElement();
                card.AddToClassList("mount-card");
                bool active = collection.Active == mount;
                card.EnableInClassList("mount-card--active", active);
                _content.Add(card);

                string name = MountTexts.MountName(Localization, definition) + (active ? " · " + T(MountTexts.ActiveTag) : string.Empty);
                Add(card, name, "mount-card-name");
                Add(card, mount.level >= MountRules.MaxLevel
                    ? F(MountTexts.LevelMax, mount.level)
                    : F(MountTexts.LevelLine, mount.level, mount.xp, MountCareRules.XpToNext(mount.level)), "mount-card-line");
                // Growth with the rider (Docs/Progressao500.md §6.1): cap of this hero, Brilho, level it works at, Despertar.
                int riderLevel = MountCollection.RiderLevel;
                if (riderLevel > 0)
                {
                    Add(card, F(MountTexts.GrowthLine, MountGrowthRules.LevelCapFor(riderLevel), riderLevel, mount.glow, MountGrowthRules.MaxGlow), "mount-card-line");
                    int worksAt = MountCollection.EffectiveLevelOf(mount);
                    if (worksAt < mount.level)
                    {
                        Add(card, F(MountTexts.WorksAtLine, worksAt), "mount-card-line");
                    }
                }

                if (mount.awakening > 0)
                {
                    Add(card, F(MountTexts.AwakeningLine, AwakeningNumeral(mount.awakening)), "mount-card-line");
                }
                Add(card, F(MountTexts.HeartsLine, mount.Hearts), "mount-card-line");
                Add(card, F(MountTexts.SatietyLine, Mathf.RoundToInt(mount.satiety)) + (mount.IsHungry ? " " + T(MountTexts.HungryTag) : string.Empty), "mount-card-line");
                Add(card, mount.IsTired
                    ? F(MountTexts.TiredLine, mount.tiredUntilEncounter)
                    : F(MountTexts.HealthLine, Mathf.RoundToInt(MountCollection.HealthOf(mount)), Mathf.RoundToInt(MountCollection.MaxHealthOf(mount))), "mount-card-line");

                var cosmeticRow = Row(card);
                AddCosmeticCycler(cosmeticRow, collection, mount, MountCosmeticSlot.Saddle);
                AddCosmeticCycler(cosmeticRow, collection, mount, MountCosmeticSlot.Blanket);

                var actions = Row(card);
                if (!active)
                {
                    AddButton(actions, T(MountTexts.SetActive), () => collection.SetActive(mount.guid));
                }

                AddButton(actions, T(MountTexts.Cuddle), () => Cuddle(collection, mount, definition));

                var foods = Row(card);
                foreach (MountFoodSpec food in MountCareRules.Foods)
                {
                    if (!MountCareRules.CanEat(definition.Diet, food))
                    {
                        continue;
                    }

                    MountFeedBlock check = MountFeeding.Check(definition.Diet, food, CountInBag, IsDishKnown);
                    Button button = AddButton(foods, F(MountTexts.Feed, T(food.NameKey)), () => Feed(collection, mount, definition, food));
                    button.SetEnabled(check == MountFeedBlock.None);
                }
            }
        }

        private void BuildCodex()
        {
            MountCollection collection = MountCollection.ResolveShared();
            int seen = 0;
            foreach (MountCodexEntry entry in MountCodex.Entries)
            {
                if (collection.CodexSeen.Contains(entry.MountId))
                {
                    seen++;
                }
            }

            AddLine(F(MountTexts.CodexCount, seen, MountCodex.Count), "mount-stable-note");
            foreach (MountCodexEntry entry in MountCodex.Entries)
            {
                bool known = collection.CodexSeen.Contains(entry.MountId);
                var row = new VisualElement();
                row.AddToClassList("mount-codex-row");
                row.EnableInClassList("mount-codex-row--unknown", !known);
                _content.Add(row);
                Add(row, known ? T(entry.NameKey) : T(MountTexts.CodexUnknown), "mount-card-name");
                Add(row, T(entry.HintKey) + (entry.IsAvailable ? string.Empty : " " + T(MountTexts.CodexSoon)), "mount-card-line");
            }
        }

        private void BuildSaddlery()
        {
            if (!IsInVila(_map))
            {
                AddLine(T(MountTexts.SaddleryClosed), "mount-stable-note");
                return;
            }

            MountCollection collection = MountCollection.ResolveShared();
            foreach (MountCosmeticSpec spec in MountCosmetics.All)
            {
                var row = new VisualElement();
                row.AddToClassList("mount-codex-row");
                _content.Add(row);
                Add(row, T(spec.NameKey), "mount-card-name");
                if (collection.OwnsCosmetic(spec.CosmeticId))
                {
                    Add(row, T(MountTexts.Owned), "mount-card-line");
                    continue;
                }

                Button buy = AddButton(row, F(MountTexts.Buy, spec.Price), () => Buy(collection, spec));
                buy.SetEnabled(CountInBag(MealCatalog.ProvisionsItemId) >= spec.Price);
            }
        }

        // ------------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------------

        private void Feed(MountCollection collection, OwnedMount mount, MountDefinition definition, MountFoodSpec food)
        {
            MountFeedBlock block = MountFeeding.TryPay(definition.Diet, food, CountInBag, RemoveFromBag, IsDishKnown);
            string mountName = MountTexts.MountName(Localization, definition);
            switch (block)
            {
                case MountFeedBlock.None:
                    int levelBefore = mount.level;
                    collection.Feed(mount, food, MountRideCare.IsFavorite(definition));
                    Say(F(MountTexts.Fed, mountName, T(food.NameKey), Mathf.RoundToInt(food.Satiety)));
                    if (mount.level > levelBefore)
                    {
                        Say(F(MountTexts.LevelUp, mountName, mount.level));
                    }

                    MountAudio.Play(MountAudio.Sniff);
                    break;
                case MountFeedBlock.Diet: Say(F(MountTexts.CantEat, mountName)); break;
                case MountFeedBlock.UnknownDish: Say(F(MountTexts.UnknownDish, T(food.NameKey))); break;
                default: Say(F(MountTexts.NoFood, T(food.NameKey))); break;
            }

            Rebuild();
        }

        private void Cuddle(MountCollection collection, OwnedMount mount, MountDefinition definition)
        {
            string mountName = MountTexts.MountName(Localization, definition);
            if (collection.TryCuddle(mount, Time.unscaledTime, MountRideCare.IsFavorite(definition)))
            {
                Say(F(MountTexts.Cuddled, mountName));
                MountAudio.Play(definition.VisualRecipe == MountVisualRecipe.Donkey ? MountAudio.Cowbell : MountAudio.Sniff);
            }
            else
            {
                Say(F(MountTexts.CuddleWait, mountName));
            }

            Rebuild();
        }

        private void Buy(MountCollection collection, MountCosmeticSpec spec)
        {
            if (CountInBag(MealCatalog.ProvisionsItemId) < spec.Price || !RemoveFromBag(MealCatalog.ProvisionsItemId, spec.Price))
            {
                Say(T(MountTexts.NotEnoughGold));
                return;
            }

            collection.GrantCosmetic(spec.CosmeticId);
            Say(F(MountTexts.Bought, T(spec.NameKey)));
            Rebuild();
        }

        private void AddCosmeticCycler(VisualElement row, MountCollection collection, OwnedMount mount, MountCosmeticSlot slot)
        {
            string current = slot == MountCosmeticSlot.Saddle ? mount.saddleId : mount.blanketId;
            MountCosmeticSpec spec = MountCosmetics.Get(current);
            string name = spec != null ? T(spec.NameKey) : T(MountTexts.NoneCosmetic);
            Add(row, F(slot == MountCosmeticSlot.Saddle ? MountTexts.SaddleLine : MountTexts.BlanketLine, name), "mount-card-line");

            var options = new List<string> { string.Empty };
            foreach (MountCosmeticSpec c in MountCosmetics.InSlot(slot))
            {
                if (collection.OwnsCosmetic(c.CosmeticId))
                {
                    options.Add(c.CosmeticId);
                }
            }

            Button change = AddButton(row, T(MountTexts.Change), () =>
            {
                int index = options.IndexOf(current ?? string.Empty);
                collection.Equip(mount, slot, options[(index + 1) % options.Count]);
            });
            change.SetEnabled(options.Count > 1);
        }

        // ------------------------------------------------------------------

        private int CountInBag(string itemId) => _bag != null ? _bag.GetTotalCount(itemId) : 0;

        private bool RemoveFromBag(string itemId, int quantity) => _bag != null && _bag.RemoveById(itemId, quantity);

        private static bool IsDishKnown(string recipeId)
        {
            return ServiceLocator.TryGet(out MealService meals) && meals != null && meals.IsKnown(recipeId);
        }

        private void Say(string text) => _status.text = text;

        private void AddLine(string text, string className) => Add(_content, text, className);

        private static Label Add(VisualElement parent, string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static VisualElement Row(VisualElement parent)
        {
            var row = new VisualElement();
            row.AddToClassList("mount-card-row");
            parent.Add(row);
            return row;
        }

        private static Button AddButton(VisualElement parent, string text, System.Action onClick)
        {
            var button = new Button(onClick) { text = text, focusable = true };
            button.AddToClassList("mount-card-button");
            parent.Add(button);
            return button;
        }
    }
}
