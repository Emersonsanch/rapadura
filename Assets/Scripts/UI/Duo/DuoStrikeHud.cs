using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Duo;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Duo
{
    /// <summary>
    /// Arena HUD side of "Golpe em dupla". Self-bootstrapping, no scene edit (same pattern as <c>SecretsMapButtons</c>):
    /// after every scene load it joins the <see cref="HudController"/>'s GameObject and injects into <c>hud-root</c>:
    /// <list type="bullet">
    /// <item>the round "Dupla" button above the skill hotbar (touch, and clickable with the mouse): a fill that rises
    /// with the charge, "Dupla 40%" → "Dupla!" → "Usado", and "com Ícaro · F" under it. Pressing it calls
    /// <see cref="DuoStrikeController.TryTrigger"/>. Shown only while an enemy is alive and a hero + partner exist.</item>
    /// <item>the shout banner (combo name, "Laço de irmãos!" / "Primos juntos!", the two lines), on
    /// <see cref="DuoStrikeStartedEvent"/>, for <see cref="BannerSeconds"/> of game time; no fade with "Reduzir
    /// Movimento"/"Reduzir Flashes"; hidden while gameplay is paused.</item>
    /// </list>
    /// The look lives in Resources/UI/DuoStrike.uss (imports the theme; attached at runtime by <see cref="DuoStrikeStyles"/>).
    /// </summary>
    [DisallowMultipleComponent]
    public class DuoStrikeHud : MonoBehaviour
    {
        public const string LayerName = "duo-layer";
        public const string ButtonName = "duo-strike-button";
        public const string FillName = "duo-strike-fill";
        public const string LabelName = "duo-strike-label";
        public const string PartnerLabelName = "duo-strike-partner";
        public const string BannerName = "duo-banner";
        public const string BannerTitleName = "duo-banner-title";
        public const string BannerTagName = "duo-banner-tag";
        public const string BannerLine1Name = "duo-banner-line1";
        public const string BannerLine2Name = "duo-banner-line2";

        public const string ReadyClass = "duo-strike-button--ready";
        public const string SpentClass = "duo-strike-button--spent";
        public const string SiblingBannerClass = "duo-banner--sibling";
        public const string InstantBannerClass = "duo-banner--instant";

        public const float BannerSeconds = 2.8f;
        private const float RefreshInterval = 0.1f;
        private const string HudRootName = "hud-root";

        private UIDocument _hudDocument;
        private VisualElement _layer;
        private Button _button;
        private VisualElement _fill;
        private Label _label;
        private Label _partnerLabel;
        private VisualElement _banner;
        private Label _bannerTitle;
        private Label _bannerTag;
        private Label _bannerLine1;
        private Label _bannerLine2;
        private float _bannerRemaining;
        private float _nextRefresh;

        public bool IsBannerShowing => _bannerRemaining > 0f;

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
            HudController hud = FindFirstObjectByType<HudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<DuoStrikeHud>() == null)
            {
                hud.gameObject.AddComponent<DuoStrikeHud>();
            }
        }

        private void Awake()
        {
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DuoStrikeStartedEvent>(OnStrikeStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DuoStrikeStartedEvent>(OnStrikeStarted);
            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
            }

            _layer?.RemoveFromHierarchy();
            _layer = null;
            _button = null;
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether the button is on screen.</summary>
        public static bool ShouldShowButton(bool hasController, bool hasCombo, bool hasEnemies, bool executing, bool paused)
        {
            return hasController && hasCombo && (hasEnemies || executing) && !paused;
        }

        /// <summary>Caption of the button for a readiness and charge.</summary>
        public static string ButtonLabel(LocalizationManager localization, DuoStrikeReadiness readiness, float charge)
        {
            switch (readiness)
            {
                case DuoStrikeReadiness.Ready:
                case DuoStrikeReadiness.Executing:
                    return DuoTexts.Get(localization, DuoTexts.HudReady);
                case DuoStrikeReadiness.Spent:
                    return DuoTexts.Get(localization, DuoTexts.HudSpent);
                case DuoStrikeReadiness.Charging:
                    return DuoTexts.Format(localization, DuoTexts.HudCharging, Mathf.FloorToInt(Mathf.Clamp01(charge) * 100f));
                default:
                    return DuoTexts.Get(localization, DuoTexts.HudWait);
            }
        }

        /// <summary>"com Ícaro" (touch) / "com Ícaro · F" (keyboard/gamepad shown).</summary>
        public static string PartnerCaption(LocalizationManager localization, CharacterId partner, string keyHint)
        {
            string caption = DuoTexts.Format(localization, DuoTexts.HudPartner, DuoTexts.CousinName(partner));
            return string.IsNullOrEmpty(keyHint) ? caption : caption + " · " + keyHint;
        }

        // ------------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------------

        private void Update()
        {
            bool paused = GameplayPause.IsPaused;
            if (_bannerRemaining > 0f)
            {
                if (!paused)
                {
                    _bannerRemaining -= Time.deltaTime;
                    if (_bannerRemaining <= 0f)
                    {
                        HideBanner();
                    }
                }

                if (_banner != null)
                {
                    _banner.style.display = _bannerRemaining > 0f && !paused ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!EnsureBuilt())
            {
                return;
            }

            RefreshButton(paused);
        }

        private void RefreshButton(bool paused)
        {
            DuoStrikeController controller = DuoStrikeController.Active;
            DuoComboSpec combo = DuoStrikeController.CurrentCombo;
            bool executing = controller != null && controller.IsExecuting;
            bool show = ShouldShowButton(controller != null, combo != null, DuoStrikeController.HasEnemies, executing, paused);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            DuoStrikeMeter meter = controller.Meter;
            DuoStrikeReadiness readiness = meter.Readiness;
            _label.text = ButtonLabel(localization, readiness, meter.Charge);
            CharacterId? partner = DuoStrikeController.CurrentPartner;
            string hint = TouchControlsPreferences.HasTouchscreen ? string.Empty : controller.KeyHint;
            _partnerLabel.text = partner.HasValue ? PartnerCaption(localization, partner.Value, hint) : string.Empty;
            _fill.style.height = Length.Percent(readiness == DuoStrikeReadiness.Spent ? 0f : Mathf.Clamp01(meter.Charge) * 100f);
            _button.EnableInClassList(ReadyClass, readiness == DuoStrikeReadiness.Ready);
            _button.EnableInClassList(SpentClass, readiness == DuoStrikeReadiness.Spent);
            _button.SetEnabled(readiness == DuoStrikeReadiness.Ready);
        }

        private void OnButtonClicked()
        {
            DuoStrikeController.Active?.TryTrigger();
        }

        private void OnStrikeStarted(DuoStrikeStartedEvent evt)
        {
            if (!EnsureBuilt() || evt.Combo == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _bannerTitle.text = DuoTexts.Get(localization, evt.Combo.NameKey);
            _bannerTag.text = DuoTexts.Get(localization, evt.Combo.IsSiblingBond ? DuoTexts.SiblingBond : DuoTexts.CousinBond);
            _bannerLine1.text = DuoTexts.ShoutLine(localization, evt.Combo, 0);
            _bannerLine2.text = DuoTexts.ShoutLine(localization, evt.Combo, 1);
            _banner.EnableInClassList(SiblingBannerClass, evt.Combo.IsSiblingBond);
            _banner.EnableInClassList(InstantBannerClass, !evt.Fade);
            _bannerRemaining = BannerSeconds;
            _banner.style.display = DisplayStyle.Flex;
            if (evt.Fade)
            {
                _banner.style.opacity = 0f;
                _banner.schedule.Execute(() => _banner.style.opacity = 1f);
            }
            else
            {
                _banner.style.opacity = 1f;
            }

            UiAccessibilityApplier.RequestRefresh();
        }

        private void HideBanner()
        {
            _bannerRemaining = 0f;
            if (_banner != null)
            {
                _banner.style.display = DisplayStyle.None;
                _banner.style.opacity = 0f;
            }
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private bool EnsureBuilt()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement hudRoot = _hudDocument.rootVisualElement.Q<VisualElement>(HudRootName) ?? _hudDocument.rootVisualElement;
            if (_layer != null && _layer.parent == hudRoot)
            {
                return true;
            }

            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
            }

            _layer?.RemoveFromHierarchy();
            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            _layer = CreateView(UiMotionPreferences.IsReducedMotion(accessibility));
            _button = _layer.Q<Button>(ButtonName);
            _fill = _layer.Q<VisualElement>(FillName);
            _label = _layer.Q<Label>(LabelName);
            _partnerLabel = _layer.Q<Label>(PartnerLabelName);
            _banner = _layer.Q<VisualElement>(BannerName);
            _bannerTitle = _layer.Q<Label>(BannerTitleName);
            _bannerTag = _layer.Q<Label>(BannerTagName);
            _bannerLine1 = _layer.Q<Label>(BannerLine1Name);
            _bannerLine2 = _layer.Q<Label>(BannerLine2Name);
            _button.clicked += OnButtonClicked;
            hudRoot.Add(_layer);
            UiAccessibilityApplier.RequestRefresh();
            return true;
        }

        /// <summary>The injected tree (layer → button + banner), hidden, themed via <see cref="DuoStrikeStyles"/>. EditMode-testable.</summary>
        public static VisualElement CreateView(bool reducedMotion)
        {
            var layer = new VisualElement { name = LayerName, pickingMode = PickingMode.Ignore };
            layer.AddToClassList(LayerName);
            DuoStrikeStyles.ApplyTo(layer, reducedMotion);

            var button = new Button { name = ButtonName, focusable = false };
            button.AddToClassList("hud-touch-button");
            button.AddToClassList(ButtonName);
            button.style.display = DisplayStyle.None;

            var fill = new VisualElement { name = FillName, pickingMode = PickingMode.Ignore };
            fill.AddToClassList(FillName);
            var label = new Label { name = LabelName, pickingMode = PickingMode.Ignore };
            label.AddToClassList(LabelName);
            var partner = new Label { name = PartnerLabelName, pickingMode = PickingMode.Ignore };
            partner.AddToClassList(PartnerLabelName);
            button.Add(fill);
            button.Add(label);
            button.Add(partner);

            var banner = new VisualElement { name = BannerName, pickingMode = PickingMode.Ignore };
            banner.AddToClassList(BannerName);
            banner.style.display = DisplayStyle.None;
            banner.style.opacity = 0f;
            foreach (string part in new[] { BannerTagName, BannerTitleName, BannerLine1Name, BannerLine2Name })
            {
                var text = new Label { name = part, pickingMode = PickingMode.Ignore };
                text.AddToClassList(part);
                banner.Add(text);
            }

            layer.Add(banner);
            layer.Add(button);
            return layer;
        }
    }

    /// <summary>Loads Resources/UI/DuoStrike.uss (cached) and attaches it with the theme / reduced-motion classes.</summary>
    public static class DuoStrikeStyles
    {
        public const string ResourcePath = "UI/DuoStrike";
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        public static StyleSheet Sheet
        {
            get
            {
                if (_sheet == null)
                {
                    _sheet = Resources.Load<StyleSheet>(ResourcePath);
                }

                return _sheet;
            }
        }

        public static void ApplyTo(VisualElement root, bool reducedMotion)
        {
            if (root == null)
            {
                return;
            }

            StyleSheet sheet = Sheet;
            if (sheet != null && !root.styleSheets.Contains(sheet))
            {
                root.styleSheets.Add(sheet);
            }

            root.AddToClassList(ThemeClass);
            UiMotionPreferences.ApplyTo(root, reducedMotion);
        }
    }
}
