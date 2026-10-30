using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Daily;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Save;
using Rapadura.UI.Common;
using Rapadura.UI.Menus;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Daily
{
    /// <summary>State of one cell of the 7-day track.</summary>
    public enum DailyTrackCellState
    {
        Claimed,
        Today,
        Upcoming,
    }

    /// <summary>
    /// The themed "Presente do dia da Vó Zefa" popup: title, subtitle, the 7-day track (each day's rewards; days already
    /// received ticked, today's highlighted), a note (stepped back / result / preview hint) and the buttons. Built in code
    /// (Resources/UI/DailyRewards.uss), shared by the map (<see cref="DailyLoginMapPopup"/>, can claim) and the title
    /// (<see cref="DailyLoginTitleBanner"/>, preview only).
    /// </summary>
    public sealed class DailyLoginPopupView
    {
        public const string OverlayName = "daily-login-overlay";
        public const string PanelName = "daily-login-panel";
        public const string TitleName = "daily-login-title";
        public const string SubtitleName = "daily-login-subtitle";
        public const string TrackName = "daily-login-track";
        public const string CellClass = "daily-login-cell";
        public const string CellDayClass = "daily-login-cell-day";
        public const string CellRewardClass = "daily-login-cell-reward";
        public const string CellStateClass = "daily-login-cell-state";
        public const string NoteName = "daily-login-note";
        public const string ClaimName = "daily-login-claim";
        public const string CloseName = "daily-login-close";

        public readonly VisualElement Overlay;
        public readonly Label Title;
        public readonly Label Subtitle;
        public readonly VisualElement Track;
        public readonly Label Note;
        public readonly Button Claim;
        public readonly Button Close;

        public DailyLoginPopupView()
        {
            Overlay = new VisualElement { name = OverlayName };
            Overlay.AddToClassList(OverlayName);
            DailyStyles.ApplyTo(Overlay);
            Overlay.style.display = DisplayStyle.None;

            var panel = new VisualElement { name = PanelName };
            panel.AddToClassList(PanelName);
            panel.AddToClassList("rap-panel");
            Overlay.Add(panel);

            Title = new Label { name = TitleName };
            Title.AddToClassList(TitleName);
            Subtitle = new Label { name = SubtitleName };
            Subtitle.AddToClassList(SubtitleName);
            Track = new VisualElement { name = TrackName };
            Track.AddToClassList(TrackName);
            Note = new Label { name = NoteName };
            Note.AddToClassList(NoteName);

            var buttons = new VisualElement();
            buttons.AddToClassList("daily-login-buttons");
            Claim = new Button { name = ClaimName, focusable = true };
            Claim.AddToClassList(ClaimName);
            Claim.AddToClassList("rap-button");
            Claim.AddToClassList("rap-button--primary");
            Close = new Button { name = CloseName, focusable = true };
            Close.AddToClassList(CloseName);
            Close.AddToClassList("rap-button");
            buttons.Add(Claim);
            buttons.Add(Close);

            panel.Add(Title);
            panel.Add(Subtitle);
            panel.Add(Track);
            panel.Add(Note);
            panel.Add(buttons);
        }

        /// <summary>Cell state for <paramref name="cellDay"/>: before today's day received, today's highlighted, the rest ahead.</summary>
        public static DailyTrackCellState CellState(int cellDay, DailyLoginOffer offer, int lastClaimedDay)
        {
            if (offer.CanClaim)
            {
                return cellDay < offer.Day ? DailyTrackCellState.Claimed : cellDay == offer.Day ? DailyTrackCellState.Today : DailyTrackCellState.Upcoming;
            }

            return cellDay <= lastClaimedDay ? DailyTrackCellState.Claimed : DailyTrackCellState.Upcoming;
        }

        /// <summary>"20× Ouro", "Peça rara surpresa", the event invite's name (or its gold fallback).</summary>
        public static string RewardLine(LocalizationManager localization, DailyRewardEntry entry, int cyclesCompleted)
        {
            if (entry.Kind == DailyRewardKind.RareGear)
            {
                return DailyTexts.Get(localization, DailyTexts.LoginRareGear);
            }

            (ItemDefinition item, int quantity) = DailyLoginService.Resolve(entry, cyclesCompleted);
            string name = item != null ? item.LocalizedName(localization) : entry.ItemId;
            return quantity > 1 ? quantity + "× " + name : name;
        }

        /// <summary>Fills the track and texts. <paramref name="canClaimHere"/> false = title preview.</summary>
        public void Populate(LocalizationManager localization, DailyLoginOffer offer, DailyLoginState state, bool canClaimHere)
        {
            Title.text = DailyTexts.Get(localization, DailyTexts.LoginTitle);
            Subtitle.text = DailyTexts.Get(localization, DailyTexts.LoginSubtitle);
            Claim.text = DailyTexts.Get(localization, DailyTexts.LoginClaim);
            Close.text = DailyTexts.Get(localization, DailyTexts.LoginClose);

            Track.Clear();
            for (int day = 1; day <= DailyLoginRules.TrackLength; day++)
            {
                DailyTrackCellState cellState = CellState(day, offer, state != null ? state.LastClaimedDay : 0);
                var cell = new VisualElement();
                cell.AddToClassList(CellClass);
                cell.AddToClassList(CellClass + "--" + cellState.ToString().ToLowerInvariant());
                if (day == DailyLoginRules.TrackLength)
                {
                    cell.AddToClassList(CellClass + "--big");
                }

                var dayLabel = new Label(DailyTexts.Format(localization, DailyTexts.LoginDay, day));
                dayLabel.AddToClassList(CellDayClass);
                cell.Add(dayLabel);
                foreach (DailyRewardEntry entry in DailyLoginRules.RewardsFor(day))
                {
                    var reward = new Label(RewardLine(localization, entry, state != null ? state.CyclesCompleted : 0));
                    reward.AddToClassList(CellRewardClass);
                    cell.Add(reward);
                }

                string stateText = cellState == DailyTrackCellState.Claimed ? "✓ " + DailyTexts.Get(localization, DailyTexts.LoginClaimed)
                    : cellState == DailyTrackCellState.Today ? DailyTexts.Get(localization, DailyTexts.LoginToday) : string.Empty;
                var stateLabel = new Label(stateText);
                stateLabel.AddToClassList(CellStateClass);
                cell.Add(stateLabel);
                Track.Add(cell);
            }

            string note = !canClaimHere
                ? DailyTexts.Get(localization, DailyTexts.LoginPreviewHint)
                : offer.CanClaim && offer.SteppedBack ? DailyTexts.Get(localization, DailyTexts.LoginSteppedBack) : string.Empty;
            SetNote(note);
            Claim.style.display = canClaimHere && offer.CanClaim ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetNote(string text)
        {
            Note.text = text ?? string.Empty;
            Note.style.display = string.IsNullOrEmpty(Note.text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <summary>"Você recebeu: 20× Ouro" plus one "… foi para o Baú" line per reward that overflowed.</summary>
        public static string ResultText(LocalizationManager localization, IReadOnlyList<DailyRewardGrant> grants)
        {
            if (grants == null || grants.Count == 0)
            {
                return string.Empty;
            }

            var names = new List<string>();
            var toChest = new List<string>();
            foreach (DailyRewardGrant grant in grants)
            {
                string name = !string.IsNullOrEmpty(grant.GeneratedName) ? grant.GeneratedName : grant.Item != null ? grant.Item.LocalizedName(localization) : "?";
                int total = grant.ToBag + grant.ToChest;
                names.Add(total > 1 ? total + "× " + name : name);
                if (grant.ToChest > 0)
                {
                    toChest.Add(DailyTexts.Format(localization, DailyTexts.LoginToChest, name));
                }
            }

            string text = DailyTexts.Format(localization, DailyTexts.LoginGotIt, string.Join(", ", names));
            return toChest.Count > 0 ? text + "\n" + string.Join("\n", toChest) : text;
        }
    }

    /// <summary>
    /// World map side of the daily login reward. Self-bootstrapping (joins the <see cref="WorldMapHudController"/>, no
    /// scene edit): when the map HUD is visible, nothing else holds the pause and today's reward is claimable, it opens
    /// the popup once. "Receber" gives the reward through a short <see cref="CarriedInventorySession"/> (the map has no
    /// Player) — the bag's grid fit API decides what fits, the rest goes to the Baú da Vó Zefa — and shows what came.
    /// Registered as a HUD screen (Esc / Start closes it); holds <see cref="GameplayPause"/> while open.
    /// </summary>
    [DisallowMultipleComponent]
    public class DailyLoginMapPopup : MonoBehaviour
    {
        public const string ScreenId = "daily-login";
        private const float RefreshInterval = 0.5f;
        private const float OpenDelaySeconds = 1.5f;

        private WorldMapHudController _hud;
        private UIDocument _document;
        private DailyLoginPopupView _view;
        private bool _open;
        private bool _screenRegistered;
        private string _shownForDay;
        private float _visibleSince = -1f;
        private float _nextRefresh;

        public bool IsOpen => _open;

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
            if (hud != null && hud.GetComponent<DailyLoginMapPopup>() == null)
            {
                hud.gameObject.AddComponent<DailyLoginMapPopup>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _document = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();
            if (_view != null)
            {
                _view.Claim.clicked -= OnClaim;
                _view.Close.clicked -= Close;
                _view.Overlay.RemoveFromHierarchy();
                _view = null;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!_screenRegistered && _hud != null)
            {
                _hud.RegisterScreen(ScreenId, () => _open, Open, Close);
                _screenRegistered = true;
            }

            bool visible = _hud != null && _hud.IsVisible;
            if (!visible)
            {
                _visibleSince = -1f;
                return;
            }

            if (_visibleSince < 0f)
            {
                _visibleSince = Time.unscaledTime;
            }

            if (_open || GameplayPause.IsPaused || Time.unscaledTime - _visibleSince < OpenDelaySeconds
                || !ServiceLocator.TryGet(out DailyLoginService service) || service == null)
            {
                return;
            }

            if (service.Offer.CanClaim && _shownForDay != service.TodayKey)
            {
                _shownForDay = service.TodayKey;
                Open();
            }
        }

        public void Open()
        {
            if (_open || !EnsureView() || !ServiceLocator.TryGet(out DailyLoginService service) || service == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _view.Populate(localization, service.Offer, service.Snapshot, canClaimHere: true);
            _open = true;
            _view.Overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            (_view.Claim.style.display == DisplayStyle.Flex ? _view.Claim : _view.Close).Focus();
            UiAccessibilityApplier.RequestRefresh();
        }

        public void Close()
        {
            if (!_open)
            {
                return;
            }

            _open = false;
            if (_view != null)
            {
                _view.Overlay.style.display = DisplayStyle.None;
            }

            GameplayPause.Release(this);
        }

        private void OnClaim()
        {
            if (!ServiceLocator.TryGet(out DailyLoginService service) || service == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            ServiceLocator.TryGet(out VillageChestStorage chest);
            InventoryManager bag = FindFirstObjectByType<InventoryManager>();
            CarriedInventorySession session = null;
            if (bag == null)
            {
                ServiceLocator.TryGet(out SaveManager saveManager);
                session = new CarriedInventorySession(saveManager);
                bag = session.Open(transform);
            }

            List<DailyRewardGrant> grants = service.Claim(bag, chest);
            session?.Close();

            _view.Populate(localization, service.Offer, service.Snapshot, canClaimHere: true);
            _view.SetNote(DailyLoginPopupView.ResultText(localization, grants));
            _view.Close.Focus();
        }

        private bool EnsureView()
        {
            VisualElement root = _document != null ? _document.rootVisualElement : null;
            if (root == null)
            {
                return false;
            }

            if (_view == null)
            {
                _view = new DailyLoginPopupView();
                _view.Claim.clicked += OnClaim;
                _view.Close.clicked += Close;
            }

            if (_view.Overlay.parent != root)
            {
                root.Add(_view.Overlay);
            }

            return true;
        }
    }

    /// <summary>
    /// Title screen side: a small "Presente do dia esperando!" ribbon under the main menu (joins the
    /// <see cref="MainMenuController"/>, no scene edit). No save is loaded on the title, so it reads the PlayerPrefs mirror
    /// (<see cref="DailyLoginService.ReadPreview"/>) and opens the track in preview mode; the claim happens on the map.
    /// </summary>
    [DisallowMultipleComponent]
    public class DailyLoginTitleBanner : MonoBehaviour
    {
        public const string BannerName = "daily-login-title-banner";

        private UIDocument _document;
        private Button _banner;
        private DailyLoginPopupView _view;
        private float _nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            MainMenuController menu = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (menu != null && menu.GetComponent<DailyLoginTitleBanner>() == null)
            {
                menu.gameObject.AddComponent<DailyLoginTitleBanner>();
            }
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            if (_banner != null)
            {
                _banner.clicked -= OpenPreview;
                _banner.RemoveFromHierarchy();
                _banner = null;
            }

            if (_view != null)
            {
                _view.Close.clicked -= ClosePreview;
                _view.Overlay.RemoveFromHierarchy();
                _view = null;
            }
        }

        /// <summary>Preview offer from the mirror: what the map would offer today.</summary>
        public static DailyLoginOffer PreviewOffer(DailyLoginState mirror, string todayKey) => DailyLoginRules.Evaluate(mirror, todayKey);

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + 1f;
            VisualElement root = _document != null ? _document.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            if (_banner == null)
            {
                _banner = new Button { name = BannerName, focusable = true };
                _banner.AddToClassList(BannerName);
                DailyStyles.ApplyTo(_banner);
                _banner.clicked += OpenPreview;
            }

            if (_banner.parent != root)
            {
                root.Add(_banner);
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            DailyLoginOffer offer = PreviewOffer(DailyLoginService.ReadPreview(), DailyLoginRules.ToDayKey(System.DateTime.Now));
            _banner.text = DailyTexts.Get(localization, offer.CanClaim ? DailyTexts.LoginTitleBanner : DailyTexts.LoginTitleBannerDone);
        }

        private void OpenPreview()
        {
            VisualElement root = _document != null ? _document.rootVisualElement : null;
            if (root == null)
            {
                return;
            }

            if (_view == null)
            {
                _view = new DailyLoginPopupView();
                _view.Close.clicked += ClosePreview;
            }

            if (_view.Overlay.parent != root)
            {
                root.Add(_view.Overlay);
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            DailyLoginState mirror = DailyLoginService.ReadPreview();
            _view.Populate(localization, PreviewOffer(mirror, DailyLoginRules.ToDayKey(System.DateTime.Now)), mirror, canClaimHere: false);
            _view.Overlay.style.display = DisplayStyle.Flex;
            _view.Close.Focus();
            UiAccessibilityApplier.RequestRefresh();
        }

        private void ClosePreview()
        {
            if (_view != null)
            {
                _view.Overlay.style.display = DisplayStyle.None;
            }

            _banner?.Focus();
        }
    }
}
