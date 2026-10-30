using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// Feeds the carry load (<see cref="EncumbranceRules"/>) into the existing speed path, with no scene edit and no
    /// change to the motor's own logic. Self-bootstrapping like <see cref="MountController"/>:
    /// <list type="bullet">
    /// <item><b>Arena / city</b>: joins the hero (<see cref="PlayerStats"/> + <see cref="PlayerMotor"/>) and reads the live
    /// <see cref="InventoryManager"/> (backpack + equipped), the hero's level and Vitality, and the active mount. It sets
    /// <see cref="PlayerMotor.LoadFactor"/> (on foot), <see cref="MountController.LoadFactor"/> and
    /// <see cref="MountController.LoadAllowsGallop"/> (mounted).</item>
    /// <item><b>World map</b>: joins the <see cref="WorldMapController"/> and reads the carried bag/level from the
    /// <see cref="SaveManager"/>, driving <see cref="WorldMapMountRider.LoadFactor"/>.</item>
    /// </list>
    /// Every factor only multiplies an already-capped multiplier down (<see cref="EncumbranceRules.ApplyFactor"/>), so the
    /// speed cap stays an upper bound. Raises a <see cref="MountNotices"/> line when the load band changes.
    /// Nothing is saved: the load is derived from the inventory every time (save-compatible).
    /// </summary>
    [DisallowMultipleComponent]
    public class EncumbranceTracker : MonoBehaviour
    {
        public const float RefreshInterval = 0.25f;
        private const string CarriedInventoryKey = "Inventory";
        private const string CarriedStatsKey = "PlayerStats";
        private const string CarriedAttributesKey = "AttributeSet";

        /// <summary>The last evaluation (HUD, bag panel). <see cref="HasCurrent"/> is false before the first one.</summary>
        public static EncumbranceSnapshot Current { get; private set; }

        public static bool HasCurrent { get; private set; }

        /// <summary>Raised after every evaluation whose numbers changed.</summary>
        public static event Action<EncumbranceSnapshot> Changed;

        private PlayerMotor _motor;
        private PlayerStats _stats;
        private AttributeSet _attributes;
        private InventoryManager _inventory;
        private bool _onMap;
        private float _nextRefresh;
        private bool _hasBand;
        private LoadBand _lastBand;
        private bool _lastMounted;

        private string _cachedInventoryJson;
        private float _cachedCarriedLoad;
        private string _cachedStatsJson;
        private int _cachedLevel = 1;
        private string _cachedAttributesJson;
        private int _cachedVitality;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

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
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null && stats.GetComponent<PlayerMotor>() != null)
            {
                if (stats.GetComponent<EncumbranceTracker>() == null)
                {
                    stats.gameObject.AddComponent<EncumbranceTracker>();
                }

                return;
            }

            WorldMapController map = FindFirstObjectByType<WorldMapController>();
            if (map != null && map.GetComponent<EncumbranceTracker>() == null)
            {
                map.gameObject.AddComponent<EncumbranceTracker>();
            }
        }

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _stats = GetComponent<PlayerStats>();
            _attributes = GetComponent<AttributeSet>();
            _inventory = GetComponent<InventoryManager>();
            _onMap = _motor == null && GetComponent<WorldMapController>() != null;
        }

        private void OnDisable()
        {
            // Leave nothing slowed behind.
            if (_motor != null)
            {
                _motor.LoadFactor = 1f;
            }

            MountController controller = GetComponent<MountController>();
            if (controller != null)
            {
                controller.LoadFactor = 1f;
                controller.LoadAllowsGallop = true;
            }

            if (_onMap && WorldMapMountRider.Current != null)
            {
                WorldMapMountRider.Current.LoadFactor = 1f;
                WorldMapMountRider.Current.LoadAllowsGallop = true;
            }

            HasCurrent = false;
            _hasBand = false;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        // ------------------------------------------------------------------
        // Evaluation
        // ------------------------------------------------------------------

        /// <summary>Re-evaluates now (also used by the bag panel right after a change).</summary>
        public void Refresh()
        {
            EncumbranceSnapshot snapshot = _onMap ? EvaluateMap() : EvaluateArena();
            Apply(snapshot);

            bool changed = !HasCurrent || !SameNumbers(Current, snapshot);
            Current = snapshot;
            HasCurrent = true;
            if (changed)
            {
                Changed?.Invoke(snapshot);
            }

            NotifyBand(snapshot);
        }

        private EncumbranceSnapshot EvaluateArena()
        {
            if (_inventory == null)
            {
                _inventory = GetComponent<InventoryManager>();
            }

            float load = _inventory != null ? _inventory.GetCarriedWeight() : 0f;
            int level = _stats != null ? _stats.Level : 1;
            int vitality = _attributes != null ? _attributes.GetTotal(AttributeType.Vitality) : 0;
            MountController controller = MountController.Active;
            bool mounted = controller != null && controller.IsActive;
            return Build(load, level, vitality, mounted, mounted ? controller.Definition : null, mounted ? controller.ActiveMount : null);
        }

        private EncumbranceSnapshot EvaluateMap()
        {
            ServiceLocator.TryGet(out SaveManager saves);
            float load = CarriedLoad(saves);
            int level = CarriedLevel(saves);
            int vitality = CarriedVitality(saves);
            WorldMapMountRider rider = WorldMapMountRider.Current;
            bool mounted = rider != null && rider.IsMounted;
            return Build(load, level, vitality, mounted, mounted ? rider.Definition : null, mounted ? rider.ActiveMount : null);
        }

        private static EncumbranceSnapshot Build(float load, int level, int vitality, bool mounted, MountDefinition definition, OwnedMount owned)
        {
            bool riding = mounted && definition != null;
            return EncumbranceRules.Evaluate(load, level, vitality, riding,
                riding ? definition.MountId : null,
                riding ? definition.Size : MountSize.Medio,
                owned != null ? owned.level : 1);
        }

        private void Apply(EncumbranceSnapshot snapshot)
        {
            if (_onMap)
            {
                WorldMapMountRider rider = WorldMapMountRider.Current;
                if (rider != null)
                {
                    rider.LoadFactor = snapshot.SpeedFactor;
                    rider.LoadAllowsGallop = snapshot.CanGallop;
                }

                return;
            }

            // The motor ignores its LoadFactor while the mount drives the speed, and vice versa.
            EncumbranceSnapshot onFoot = new EncumbranceSnapshot(snapshot.Load, snapshot.HeroCapacity, false, 0f);
            if (_motor != null)
            {
                _motor.LoadFactor = onFoot.SpeedFactor;
            }

            MountController controller = MountController.Active;
            if (controller != null)
            {
                controller.LoadFactor = snapshot.Mounted ? snapshot.SpeedFactor : 1f;
                controller.LoadAllowsGallop = snapshot.CanGallop;
            }
        }

        private void NotifyBand(EncumbranceSnapshot snapshot)
        {
            LoadBand band = snapshot.Band;
            bool mounted = snapshot.Mounted;
            if (!_hasBand)
            {
                // First look after a scene load: only speak up when it is already a problem.
                _hasBand = true;
                _lastBand = band;
                _lastMounted = mounted;
                if (band != LoadBand.Light)
                {
                    Raise(band, mounted, lighter: false);
                }

                return;
            }

            if (band == _lastBand && mounted == _lastMounted)
            {
                return;
            }

            bool stanceChanged = mounted != _lastMounted;
            bool lighter = band < _lastBand;
            _lastBand = band;
            _lastMounted = mounted;
            if (stanceChanged && band == LoadBand.Light)
            {
                return; // dismounting/mounting light: nothing to say
            }

            Raise(band, mounted, lighter && !stanceChanged);
        }

        private static void Raise(LoadBand band, bool mounted, bool lighter)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            string key = EncumbranceTexts.ToastKey(band, mounted, lighter);
            if (key == null)
            {
                return;
            }

            string mountName = string.Empty;
            if (mounted)
            {
                MountDefinition definition = MountCollection.ResolveShared().ActiveDefinition;
                mountName = definition != null ? MountTexts.MountName(localization, definition) : string.Empty;
            }

            MountNotices.Raise(EncumbranceTexts.Format(localization, key, mountName));
        }

        private static bool SameNumbers(EncumbranceSnapshot a, EncumbranceSnapshot b)
        {
            return a.Mounted == b.Mounted
                && Mathf.Approximately(a.Load, b.Load)
                && Mathf.Approximately(a.HeroCapacity, b.HeroCapacity)
                && Mathf.Approximately(a.MountCapacity, b.MountCapacity);
        }

        // ------------------------------------------------------------------
        // Carried state (world map)
        // ------------------------------------------------------------------

        private float CarriedLoad(SaveManager saves)
        {
            string json = null;
            if (saves == null || !saves.TryGetCarriedJson(CarriedInventoryKey, out json))
            {
                json = null;
            }

            if (json == _cachedInventoryJson)
            {
                return _cachedCarriedLoad;
            }

            _cachedInventoryJson = json;
            _cachedCarriedLoad = EncumbranceRules.WeightOf(TryParse<InventorySaveState>(json), ResolveItem);
            return _cachedCarriedLoad;
        }

        private int CarriedLevel(SaveManager saves)
        {
            string json = null;
            if (saves == null || !saves.TryGetCarriedJson(CarriedStatsKey, out json))
            {
                json = null;
            }

            if (json != _cachedStatsJson)
            {
                _cachedStatsJson = json;
                PlayerStatsSaveState state = TryParse<PlayerStatsSaveState>(json);
                _cachedLevel = state != null ? Mathf.Max(1, state.level) : 1;
            }

            return _cachedLevel;
        }

        private int CarriedVitality(SaveManager saves)
        {
            string json = null;
            if (saves == null || !saves.TryGetCarriedJson(CarriedAttributesKey, out json))
            {
                json = null;
            }

            if (json != _cachedAttributesJson)
            {
                _cachedAttributesJson = json;
                AttributeSetSaveState state = TryParse<AttributeSetSaveState>(json);
                _cachedVitality = state != null ? Mathf.Max(0, state.vitality) : 0;
            }

            return _cachedVitality;
        }

        private static T TryParse<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static ItemDefinition ResolveItem(string itemId)
        {
            return string.IsNullOrEmpty(itemId) ? null : ItemDatabase.GetById(itemId);
        }
    }
}
