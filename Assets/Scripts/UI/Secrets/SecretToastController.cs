using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Secrets
{
    /// <summary>
    /// Feedback for the secrets, on the map and in the arenas: "Um segredo!" when a map secret reveals itself,
    /// "Relíquia encontrada!" with the relic and the count (plus the gold line for a map secret, and a closing line
    /// once the collection is complete), "Algo brilha por perto..." when an arena hides a breakable secret and the
    /// gold of one whose relic was already home. Messages queue; each stays <see cref="DisplaySeconds"/> (real time),
    /// hidden while <see cref="GameplayPause.IsPaused"/>.
    ///
    /// Self-bootstrapping like <c>BagFullToastController</c>: one DontDestroyOnLoad host, PanelSettings borrowed from
    /// the scene's HUD (or any UIDocument), view built in code with picking-mode Ignore, look in SecretsOverlay.uss.
    /// </summary>
    public class SecretToastController : MonoBehaviour
    {
        public const float SortingOrder = 830f;
        public const float DisplaySeconds = 3.8f;
        public const int MaxQueued = 6;

        public const string RootName = "secret-toast-root";
        public const string ToastName = "secret-toast";
        public const string TitleName = "secret-toast-title";
        public const string BodyName = "secret-toast-body";

        /// <summary>One queued message.</summary>
        public sealed class Message
        {
            public string Title;
            public string Body;
        }

        private static SecretToastController _instance;

        private readonly Queue<Message> _queue = new Queue<Message>();
        private Message _current;
        private float _remaining;
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _toast;
        private Label _title;
        private Label _body;

        public bool IsShowing => _current != null;
        public int QueuedCount => _queue.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<SecretToastController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(SecretToastController));
            host.AddComponent<UIDocument>();
            host.AddComponent<SecretToastController>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            _uiDocument.sortingOrder = SortingOrder;
        }

        private void OnEnable()
        {
            _root = null;
            EventBus.Subscribe<SecretRevealedEvent>(OnSecretRevealed);
            EventBus.Subscribe<RelicCollectedEvent>(OnRelicCollected);
            EventBus.Subscribe<SecretClaimedEvent>(OnSecretClaimed);
            EventBus.Subscribe<ArenaSecretSpawnedEvent>(OnArenaSecretSpawned);
            EventBus.Subscribe<ArenaSecretBrokenEvent>(OnArenaSecretBroken);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SecretRevealedEvent>(OnSecretRevealed);
            EventBus.Unsubscribe<RelicCollectedEvent>(OnRelicCollected);
            EventBus.Unsubscribe<SecretClaimedEvent>(OnSecretClaimed);
            EventBus.Unsubscribe<ArenaSecretSpawnedEvent>(OnArenaSecretSpawned);
            EventBus.Unsubscribe<ArenaSecretBrokenEvent>(OnArenaSecretBroken);
            _queue.Clear();
            _current = null;
            _root = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // ------------------------------------------------------------------
        // Messages (pure, EditMode-tested)
        // ------------------------------------------------------------------

        public static Message ForReveal(string secretId, LocalizationManager localization)
        {
            WorldSecretDefinition secret = WorldSecretCatalog.Get(secretId);
            return new Message
            {
                Title = SecretTexts.Get(localization, SecretTexts.ToastSecretTitle),
                Body = secret != null ? SecretTexts.Get(localization, secret.RevealKey) : string.Empty,
            };
        }

        public static Message ForRelic(string relicId, int owned, LocalizationManager localization)
        {
            RelicDefinition relic = RelicCatalog.Get(relicId);
            string name = relic != null ? SecretTexts.Get(localization, relic.NameKey) : relicId;
            string body = SecretTexts.Format(localization, SecretTexts.ToastRelicBody, name, owned, RelicCatalog.Count);
            if (relic != null)
            {
                body += "\n" + SecretTexts.Get(localization, relic.PassiveKey);
            }

            if (owned >= RelicCatalog.Count)
            {
                body += "\n" + SecretTexts.Get(localization, SecretTexts.ToastCollectionDone);
            }

            return new Message { Title = SecretTexts.Get(localization, SecretTexts.ToastRelicTitle), Body = body };
        }

        public static Message ForArenaSpawn(BiomeType biome, LocalizationManager localization)
        {
            return new Message
            {
                Title = SecretTexts.Get(localization, SecretTexts.ToastArenaTitle),
                Body = SecretTexts.Get(localization, biome == BiomeType.Desert ? SecretTexts.ToastArenaDig : SecretTexts.ToastArenaRock),
            };
        }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;

        private void OnSecretRevealed(SecretRevealedEvent evt) => Enqueue(ForReveal(evt.SecretId, Localization));

        private void OnRelicCollected(RelicCollectedEvent evt) => Enqueue(ForRelic(evt.RelicId, evt.OwnedCount, Localization));

        /// <summary>The map gold rides on the relic message that the same claim just queued.</summary>
        private void OnSecretClaimed(SecretClaimedEvent evt)
        {
            if (evt.Gold <= 0)
            {
                return;
            }

            string line = SecretTexts.Format(Localization, SecretTexts.ToastMapGold, evt.Gold);
            Message last = LastMessage();
            if (last != null)
            {
                last.Body += "\n" + line;
                if (last == _current && _body != null)
                {
                    _body.text = last.Body;
                }
            }
            else
            {
                Enqueue(new Message { Title = SecretTexts.Get(Localization, SecretTexts.ToastSecretTitle), Body = line });
            }
        }

        private void OnArenaSecretSpawned(ArenaSecretSpawnedEvent evt) => Enqueue(ForArenaSpawn(evt.Biome, Localization));

        private void OnArenaSecretBroken(ArenaSecretBrokenEvent evt)
        {
            // A relic already has its own message (RelicCollectedEvent).
            if (evt.RelicId == null && evt.Gold > 0)
            {
                Enqueue(new Message
                {
                    Title = SecretTexts.Get(Localization, SecretTexts.ToastArenaTitle),
                    Body = SecretTexts.Format(Localization, SecretTexts.ToastArenaGold, evt.Gold),
                });
            }
        }

        private Message LastMessage()
        {
            Message last = _current;
            foreach (Message queued in _queue)
            {
                last = queued;
            }

            return last;
        }

        /// <summary>Queues <paramref name="message"/> (the oldest pending one is dropped past <see cref="MaxQueued"/>).</summary>
        public void Enqueue(Message message)
        {
            if (message == null)
            {
                return;
            }

            while (_queue.Count >= MaxQueued)
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(message);
            if (_current == null)
            {
                ShowNext();
            }
        }

        private void ShowNext()
        {
            _current = _queue.Count > 0 ? _queue.Dequeue() : null;
            if (_current == null)
            {
                if (_toast != null)
                {
                    _toast.style.display = DisplayStyle.None;
                }

                return;
            }

            _remaining = DisplaySeconds;
            if (EnsureBound())
            {
                _title.text = _current.Title;
                _body.text = _current.Body;
                _toast.style.display = GameplayPause.IsPaused ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void Update()
        {
            if (_current == null)
            {
                return;
            }

            // The view may not have had a panel when the message arrived (first frames of a scene).
            if (_toast == null && EnsureBound())
            {
                _title.text = _current.Title;
                _body.text = _current.Body;
            }

            if (_toast == null)
            {
                return;
            }

            bool paused = GameplayPause.IsPaused;
            _toast.style.display = paused ? DisplayStyle.None : DisplayStyle.Flex;
            if (paused)
            {
                return;
            }

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f)
            {
                ShowNext();
            }
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private bool EnsureBound()
        {
            if (_uiDocument == null)
            {
                return false;
            }

            if (_uiDocument.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _uiDocument.panelSettings = donor;
                _uiDocument.sortingOrder = SortingOrder;
            }

            VisualElement documentRoot = _uiDocument.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_root != null && _root.parent == documentRoot)
            {
                return true;
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            _root = CreateView();
            _toast = _root.Q<VisualElement>(ToastName);
            _title = _root.Q<Label>(TitleName);
            _body = _root.Q<Label>(BodyName);
            documentRoot.Add(_root);
            return true;
        }

        /// <summary>The toast tree (root → plate → title, body), hidden, picking-mode Ignore throughout. EditMode-testable.</summary>
        public static VisualElement CreateView()
        {
            var root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(RootName);
            SecretsStyles.ApplyTo(root, addThemeClass: true);
            UiMotionPreferences.ApplyTo(root, UiMotionPreferences.IsReducedMotion());

            var toast = new VisualElement { name = ToastName, pickingMode = PickingMode.Ignore };
            toast.AddToClassList(ToastName);
            toast.style.display = DisplayStyle.None;

            var title = new Label { name = TitleName, pickingMode = PickingMode.Ignore };
            title.AddToClassList(TitleName);
            var body = new Label { name = BodyName, pickingMode = PickingMode.Ignore };
            body.AddToClassList(BodyName);
            toast.Add(title);
            toast.Add(body);
            root.Add(toast);
            return root;
        }

        private PanelSettings FindPanelSettings()
        {
            HudController hud = FindFirstObjectByType<HudController>();
            UIDocument hudDocument = hud != null ? hud.GetComponent<UIDocument>() : null;
            if (hudDocument != null && hudDocument.panelSettings != null)
            {
                return hudDocument.panelSettings;
            }

            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _uiDocument && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }
    }
}
