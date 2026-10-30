using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Você está em zona segura · Oásis de Qocho" on the world map HUD, while the token is inside a city's limit
    /// (<see cref="WorldMapCityPresence.CityUnderToken"/>), plus a short notice line under it: a new carro de boi stop, the
    /// city the cousins woke up in after a defeat (<see cref="CityProgress.ConsumeRespawnNotice"/>), a finished ox-cart
    /// trip (<see cref="ShowNotice"/>). Self-bootstrapping on the <see cref="WorldMapHudController"/>'s GameObject like
    /// <see cref="VillageHubButton"/>; injected into the HUD's region block, above its action row, with
    /// <c>Resources/UI/CityHub.uss</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public class CitySafeZoneHud : MonoBehaviour
    {
        public const string IndicatorName = "wm-safe-zone-indicator";
        public const string NoticeName = "wm-city-notice";
        public const float NoticeSeconds = 6f;

        private const float RefreshInterval = 0.15f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private VisualElement _indicator;
        private Label _indicatorLabel;
        private Label _notice;
        private WorldMapCityPresence _presence;
        private float _noticeUntil;
        private float _nextRefresh;

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
            if (hud != null && hud.GetComponent<CitySafeZoneHud>() == null)
            {
                hud.gameObject.AddComponent<CitySafeZoneHud>();
            }
        }

        /// <summary>Indicator text for <paramref name="city"/> (null = not shown).</summary>
        public static string IndicatorText(CitySpec city, LocalizationManager localization)
        {
            return city == null ? null : CityTexts.Format(localization, CityTexts.SafeZoneIn, CityTexts.CityName(localization, city));
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Unsubscribe();
            _indicator?.RemoveFromHierarchy();
            _indicator = null;
            _notice?.RemoveFromHierarchy();
            _notice = null;
        }

        /// <summary>Shows <paramref name="text"/> under the indicator for <see cref="NoticeSeconds"/>.</summary>
        public void ShowNotice(string text)
        {
            if (!EnsureElements() || string.IsNullOrEmpty(text))
            {
                return;
            }

            _notice.text = text;
            _noticeUntil = Time.unscaledTime + NoticeSeconds;
            _notice.style.display = DisplayStyle.Flex;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!EnsureElements())
            {
                return;
            }

            Subscribe();

            CitySpec respawn = CityProgress.ResolveShared().ConsumeRespawnNotice();
            if (respawn != null)
            {
                ShowNotice(CityTexts.Format(Localization, CityTexts.Respawned, CityTexts.CityName(Localization, respawn)));
            }

            bool visible = _hud == null || _hud.IsVisible;
            string text = visible ? IndicatorText(_presence != null ? _presence.CityUnderToken : null, Localization) : null;
            _indicator.style.display = text != null ? DisplayStyle.Flex : DisplayStyle.None;
            if (text != null && _indicatorLabel.text != text)
            {
                _indicatorLabel.text = text;
            }

            bool noticeOn = visible && Time.unscaledTime < _noticeUntil;
            _notice.style.display = noticeOn ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Subscribe()
        {
            WorldMapCityPresence current = WorldMapCityPresence.Current;
            if (current == _presence)
            {
                return;
            }

            Unsubscribe();
            _presence = current;
            if (_presence != null)
            {
                _presence.NewCityVisited += OnNewCityVisited;
            }
        }

        private void Unsubscribe()
        {
            if (_presence != null)
            {
                _presence.NewCityVisited -= OnNewCityVisited;
            }

            _presence = null;
        }

        private void OnNewCityVisited(CitySpec city)
        {
            ShowNotice(CityTexts.Format(Localization, CityTexts.NewStop, CityTexts.CityName(Localization, city)));
        }

        private bool EnsureElements()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement block = _hudDocument.rootVisualElement.Q<VisualElement>("wm-region-block");
            if (block == null)
            {
                return false;
            }

            if (_indicator != null && _indicator.parent == block && _notice != null && _notice.parent == block)
            {
                return true;
            }

            _indicator?.RemoveFromHierarchy();
            _notice?.RemoveFromHierarchy();

            var sheet = Resources.Load<StyleSheet>(CityFastTravelPanel.StyleResourcePath);

            _indicator = new VisualElement { name = IndicatorName, pickingMode = PickingMode.Ignore };
            _indicator.AddToClassList("city-indicator");
            if (sheet != null)
            {
                _indicator.styleSheets.Add(sheet);
            }

            var dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.AddToClassList("city-indicator__dot");
            _indicator.Add(dot);
            _indicatorLabel = new Label { pickingMode = PickingMode.Ignore };
            _indicatorLabel.AddToClassList("city-indicator__label");
            _indicator.Add(_indicatorLabel);
            _indicator.style.display = DisplayStyle.None;

            _notice = new Label { name = NoticeName, pickingMode = PickingMode.Ignore };
            _notice.AddToClassList("city-notice");
            if (sheet != null)
            {
                _notice.styleSheets.Add(sheet);
            }

            _notice.style.display = DisplayStyle.None;

            VisualElement row = block.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
            int index = row != null && row.parent == block ? block.IndexOf(row) : block.childCount;
            block.Insert(index, _indicator);
            block.Insert(index + 1, _notice);
            return true;
        }
    }
}
