using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Dialogue;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Conversar com Dona Cida" on the world map: the region quest-givers (<see cref="RegionQuestCatalog.Npcs"/>)
    /// live on the map, because Main.unity is only loaded for encounters (its village NPCs are switched off
    /// during them). Self-bootstrapping, no scene edit:
    /// <list type="bullet">
    /// <item>After every scene load, if a <see cref="WorldMapHudController"/> exists this component is added
    /// to its GameObject, and it injects one button into the HUD's <c>wm-action-row</c> (next to
    /// "Explorar"), so the HUD's own pointer blocker and visibility cover it.</item>
    /// <item>If the scene has no <see cref="DialogueUIController"/>, one is created on first use with the
    /// HUD's PanelSettings and <c>Resources/UI/DialogueView.uxml</c> (the same view Main.unity uses).</item>
    /// <item>Shown while the HUD is visible, the token is idle in an unlocked region that has a giver, and
    /// nothing holds GameplayPause; "(!)" when there is a quest to take or the boss can be challenged.</item>
    /// <item>Before starting the conversation it sets the boss flags the graph reads
    /// (<see cref="RegionNpcDialogues.ApplyRegionFlags"/>).</item>
    /// </list>
    /// It also registers the region quests with <see cref="QuestManager"/> so rewards of a quest restored
    /// from a save are still known.
    /// </summary>
    [DisallowMultipleComponent]
    public class RegionNpcTalkButton : MonoBehaviour
    {
        public const string ButtonName = "wm-talk-button";
        public const string ButtonClass = "wm-talk-button";
        public const string ActionRowName = "wm-action-row";
        public const string DialogueViewResourcePath = "UI/DialogueView";
        public const int DialogueSortingOrder = 15;

        private const float RefreshInterval = 0.2f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private float _nextRefresh;
        private QuestManager _registeredWith;
        private readonly QuestDialogueContext _questContext = new QuestDialogueContext();

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHud();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHud();

        private static void AttachToHud()
        {
            WorldMapHudController hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<RegionNpcTalkButton>() == null)
            {
                hud.gameObject.AddComponent<RegionNpcTalkButton>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.clicked -= OnTalkClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            RegisterQuests();
            Refresh();
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether the talk button should be on screen.</summary>
        public static bool ShouldShow(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, bool regionUnlocked, bool regionHasNpc)
        {
            return hudVisible && tokenIdle && !paused && !dialogueActive && regionUnlocked && regionHasNpc;
        }

        /// <summary>"(!)" marker: a quest of the giver can be taken now, or the region boss can be challenged.</summary>
        public static bool HasNews(DialogueQuestState firstQuest, DialogueQuestState secondQuest, bool bossReady)
        {
            bool firstOffered = DialogueCondition.MatchesQuestState(DialogueQuestState.NotStarted, firstQuest);
            bool secondOffered = firstQuest == DialogueQuestState.Completed && DialogueCondition.MatchesQuestState(DialogueQuestState.NotStarted, secondQuest);
            return firstOffered || secondOffered || bossReady;
        }

        /// <summary>"Conversar com {0}" / "Conversar com {0} (!)" with the NPC's short name.</summary>
        public static string BuildLabel(RegionNpcSpec npc, bool hasNews, LocalizationManager localization)
        {
            string shortName = Resolve(npc.KeyBase + ".short_name", localization);
            string format = Resolve(hasNews ? RegionQuestTexts.TalkButtonNewsKey : RegionQuestTexts.TalkButtonKey, localization);
            return string.Format(format, shortName);
        }

        private static string Resolve(string key, LocalizationManager localization)
        {
            if (localization != null && localization.HasKey(key))
            {
                return localization.Get(key);
            }

            return RegionQuestTexts.GetFallback(key) ?? key;
        }

        // ------------------------------------------------------------------
        // Runtime
        // ------------------------------------------------------------------

        private void RegisterQuests()
        {
            if (ServiceLocator.TryGet(out QuestManager quests) && quests != _registeredWith)
            {
                RegionQuestCatalog.RegisterAll(quests);
                _registeredWith = quests;
            }
        }

        private bool EnsureButton()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement row = _hudDocument.rootVisualElement.Q<VisualElement>(ActionRowName);
            if (row == null)
            {
                return false;
            }

            if (_button != null && _button.parent == row)
            {
                return true;
            }

            // First time, or the HUD document rebuilt its tree (re-enable): (re)inject.
            if (_button != null)
            {
                _button.clicked -= OnTalkClicked;
                _button.RemoveFromHierarchy();
            }

            _button = new Button { name = ButtonName, focusable = true };
            _button.AddToClassList("wm-explore-button");
            _button.AddToClassList(ButtonClass);
            // Colours come from .wm-talk-button in WorldMapHudView.uss (Rapadura theme), not inline,
            // so hover / focus / press states and the high-contrast theme still apply.
            _button.style.marginLeft = 8f;
            _button.style.display = DisplayStyle.None;
            _button.clicked += OnTalkClicked;
            row.Add(_button);
            return true;
        }

        private void Refresh()
        {
            if (!EnsureButton())
            {
                return;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            RegionNpcSpec npc = CurrentNpc();
            ServiceLocator.TryGet(out DialogueManager dialogue);

            bool show = ShouldShow(
                _hud != null && _hud.IsVisible,
                _worldMap != null && _worldMap.CanExplore,
                GameplayPause.IsPaused,
                dialogue != null && dialogue.IsActive,
                npc != null && _worldMap.Progress != null && _worldMap.Progress.IsUnlocked(npc.Region),
                npc != null);

            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            bool news = HasNews(_questContext.GetQuestState(npc.FirstQuestId), _questContext.GetQuestState(npc.SecondQuestId), _worldMap.CanChallengeCurrentBoss);
            string label = BuildLabel(npc, news, localization);
            if (_button.text != label)
            {
                _button.text = label;
            }
        }

        private RegionNpcSpec CurrentNpc()
        {
            return _worldMap != null && _worldMap.CurrentNode != null ? RegionQuestCatalog.NpcFor(_worldMap.CurrentNode.Region) : null;
        }

        private void OnTalkClicked()
        {
            RegionNpcSpec npc = CurrentNpc();
            if (npc == null || GameplayPause.IsPaused || !ServiceLocator.TryGet(out DialogueManager dialogue) || dialogue.IsActive)
            {
                return;
            }

            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                RegionQuestTexts.EnsureLoaded(localization);
            }

            EnsureDialogueView();

            BiomeType region = npc.Region;
            bool cleared = _worldMap.RegionBosses != null && _worldMap.RegionBosses.IsRegionCleared(region);
            RegionNpcDialogues.ApplyRegionFlags(dialogue, _worldMap.CanChallengeCurrentBoss, cleared);
            dialogue.StartDialogue(RegionNpcDialogues.Get(region));
            _button.style.display = DisplayStyle.None;
        }

        /// <summary>Creates the dialogue box for this scene if it has none (WorldMap.unity ships without one). Also used by <see cref="VillageHubButton"/>.</summary>
        public void EnsureDialogueView()
        {
            if (FindFirstObjectByType<DialogueUIController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var view = Resources.Load<VisualTreeAsset>(DialogueViewResourcePath);
            if (view == null || _hudDocument == null || _hudDocument.panelSettings == null)
            {
                Debug.LogWarning($"[RegionNpcTalkButton] Cannot build the map dialogue box (Resources/{DialogueViewResourcePath}.uxml or the HUD PanelSettings missing).");
                return;
            }

            // Built inactive so UIDocument has its tree before DialogueUIController.OnEnable binds it.
            var host = new GameObject("WorldMapDialogue");
            host.SetActive(false);
            host.transform.SetParent(transform.parent, false);
            var document = host.AddComponent<UIDocument>();
            document.panelSettings = _hudDocument.panelSettings;
            document.visualTreeAsset = view;
            document.sortingOrder = DialogueSortingOrder;
            host.AddComponent<DialogueUIController>();
            host.SetActive(true);
            Rapadura.Audio.UiClickSoundBinder.Register(document);
        }
    }
}
