using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Pets.Visuals;
using Rapadura.Gameplay.Secrets;
using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>
    /// H6 "vida na casa" on top of the furniture of <see cref="HouseInteriorView"/> (Housing.md §5, §6.2, §11.9):
    /// <list type="bullet">
    /// <item>Trophy wall / pedestal: one carved plaque per region boss, lit gold once beaten (DefeatedEnemyRegistry).</item>
    /// <item>Relic showcase: twelve little gems, glowing for each relic found (<see cref="SecretsProgress"/>).</item>
    /// <item>Caderno shelf: one wooden animal per notebook page unlocked (up to nine, BestiaryProgress).</item>
    /// <item>Caramelo's bed: Caramelo naps on it while the cousin is home (the companion's own model, breathing).</item>
    /// <item>Plants: a thirsty one shows a drop; a wilted one droops (never dies).</item>
    /// <item>Set ambients at 6 pieces: warmer lamp (Varanda), sparks over the bench (Oficina), lilac fireflies (Noite
    /// Rubra), faster tail (Cantinho do Caramelo).</item>
    /// </list>
    /// Everything is cosmetic and rebuilt after each furniture rebuild; animation uses unscaled time (the house pauses
    /// gameplay).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HouseLifeDecor : MonoBehaviour
    {
        private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();

        private HouseInteriorView _view;
        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly List<Transform> _fireflies = new List<Transform>();
        private readonly List<Transform> _sparks = new List<Transform>();
        private readonly List<Transform> _drops = new List<Transform>();
        private GameObject _caramelo;
        private Transform _carameloTail;
        private Vector3 _carameloScale;
        private bool _fastWag;
        private Vector3 _fireflyCenter;
        private float _fireflySpread;

        public GameObject Caramelo => _caramelo;
        public int SpawnedCount => _spawned.Count;

        public static HouseLifeDecor Attach(HouseInteriorView view)
        {
            HouseLifeDecor decor = view.GetComponent<HouseLifeDecor>();
            if (decor == null) decor = view.gameObject.AddComponent<HouseLifeDecor>();
            decor._view = view;
            view.FurnitureRebuilt -= decor.Rebuild;
            view.FurnitureRebuilt += decor.Rebuild;
            decor.Rebuild();
            return decor;
        }

        private void OnDestroy()
        {
            if (_view != null) _view.FurnitureRebuilt -= Rebuild;
            Clear();
        }

        private void Clear()
        {
            foreach (GameObject go in _spawned)
            {
                if (go != null) Destroy(go);
            }

            _spawned.Clear();
            _fireflies.Clear();
            _sparks.Clear();
            _drops.Clear();
            _caramelo = null;
            _carameloTail = null;
        }

        /// <summary>Re-attaches every decoration to the current furniture.</summary>
        public void Rebuild()
        {
            Clear();
            if (_view == null || _view.House == null) return;
            HouseService house = _view.House;
            HouseComfortReport comfort = house.Comfort;
            _fastWag = comfort.HasAmbient(HouseSetAmbient.CarameloWag);

            HousePlacedData bench = null;
            HousePlacedData petBed = null;
            foreach (HousePlacedData placed in house.Layout.Placed)
            {
                HouseItemDefinition item = house.Layout.Resolve(placed.itemId);
                if (item == null || !_view.TryGetInstanceObject(placed.instanceId, out GameObject piece)) continue;
                Bounds bounds = LocalBounds(piece);
                switch (item.Function)
                {
                    case HouseItemFunction.TrophyStand: Trophies(house, piece.transform, bounds, placed.layer); break;
                    case HouseItemFunction.RelicCase: Relics(piece.transform, bounds); break;
                    case HouseItemFunction.BestiaryShelf: Animals(piece.transform, bounds); break;
                    case HouseItemFunction.PetBed:
                        if (petBed == null || (petBed.layer == HouseLayer.Yard && placed.layer != HouseLayer.Yard)) petBed = placed;
                        break;
                    case HouseItemFunction.Bench:
                        bench ??= placed;
                        break;
                }

                if (HousePlantRules.IsPlant(item)) Plant(house, placed, piece.transform, bounds);
            }

            if (petBed != null && _view.TryGetInstanceObject(petBed.instanceId, out GameObject bed)) Nap(bed.transform, LocalBounds(bed), petBed.layer);
            if (comfort.HasAmbient(HouseSetAmbient.WorkshopSparks) && bench != null && _view.TryGetInstanceObject(bench.instanceId, out GameObject workbench)) Sparks(workbench.transform, LocalBounds(workbench));
            if (comfort.HasAmbient(HouseSetAmbient.LilacFireflies)) Fireflies(house.Spec);

            Light lamp = _view.WarmLamp;
            if (lamp != null)
            {
                bool warm = comfort.HasAmbient(HouseSetAmbient.WarmLamp);
                lamp.intensity = warm ? 1.8f : 1.4f;
                lamp.color = warm ? new Color(1f, 0.8f, 0.52f) : new Color(1f, 0.86f, 0.62f);
            }
        }

        private static Bounds LocalBounds(GameObject piece)
        {
            MeshFilter filter = piece.GetComponent<MeshFilter>();
            return filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds : new Bounds(Vector3.up * 0.25f, Vector3.one * 0.5f);
        }

        // ------------------------------------------------------------------
        // Displays
        // ------------------------------------------------------------------

        private void Trophies(HouseService house, Transform parent, Bounds bounds, HouseLayer layer)
        {
            string[] bosses = HouseUpgradeRules.BossIds;
            for (int i = 0; i < bosses.Length; i++)
            {
                bool beaten = house.IsBossDefeated(bosses[i]);
                float fx = (i % 2 + 0.5f) / 2f, fy = (i / 2 + 0.5f) / 2f;
                Vector3 local = layer == HouseLayer.Wall
                    ? new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, fx), Mathf.Lerp(bounds.min.y, bounds.max.y, 1f - fy), bounds.min.z - 0.02f)
                    : new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, fx), bounds.max.y, Mathf.Lerp(bounds.min.z, bounds.max.z, fy));
                Spawn(parent, beaten ? "plaque_won" : "plaque_dim", local, layer == HouseLayer.Wall ? Quaternion.identity : Quaternion.Euler(-70f, 0f, 0f));
                if (beaten) Spawn(parent, "boss_head_" + i, local + (layer == HouseLayer.Wall ? new Vector3(0f, 0f, -0.04f) : new Vector3(0f, 0.08f, 0f)), Quaternion.identity);
            }
        }

        private void Relics(Transform parent, Bounds bounds)
        {
            SecretsProgress secrets = SecretsProgress.ResolveShared();
            IReadOnlyList<RelicDefinition> relics = RelicCatalog.All;
            for (int i = 0; i < relics.Count; i++)
            {
                bool found = secrets != null && secrets.HasRelic(relics[i].Id);
                float fx = (i % 6 + 0.5f) / 6f, fz = (i / 6 + 0.5f) / 2f;
                var local = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, fx), bounds.max.y + 0.01f, Mathf.Lerp(bounds.min.z, bounds.max.z, fz));
                Spawn(parent, found ? "relic_found" : "relic_missing", local, Quaternion.identity);
            }
        }

        private void Animals(Transform parent, Bounds bounds)
        {
            int pages = ServiceLocator.TryGet(out BestiaryProgress bestiary) && bestiary != null ? bestiary.UnlockedIds.Count : 0;
            int count = Mathf.Min(9, pages);
            for (int i = 0; i < count; i++)
            {
                float fx = (i % 3 + 0.5f) / 3f;
                float y = Mathf.Lerp(bounds.min.y, bounds.max.y, (i / 3 + 0.35f) / 3f);
                var local = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, fx), y, bounds.center.z);
                Spawn(parent, "wood_animal", local, Quaternion.Euler(0f, i * 37f, 0f));
            }
        }

        private void Plant(HouseService house, HousePlacedData placed, Transform piece, Bounds bounds)
        {
            HousePlantMood mood = house.PlantMood(placed.instanceId);
            if (mood == HousePlantMood.Happy) return;
            if (mood == HousePlantMood.Wilted)
            {
                // Droop: a gentle lean and a little shrink (re-posed by the view on the next rebuild).
                piece.localRotation *= Quaternion.Euler(16f, 0f, 9f);
                piece.localScale = Vector3.Scale(piece.localScale, new Vector3(1f, 0.82f, 1f));
            }

            GameObject drop = Spawn(piece, "water_drop", new Vector3(bounds.center.x, bounds.max.y + 0.18f, bounds.center.z), Quaternion.identity);
            _drops.Add(drop.transform);
        }

        private void Nap(Transform bed, Bounds bounds, HouseLayer layer)
        {
            var root = new GameObject("CarameloNapping");
            root.transform.SetParent(bed, false);
            root.transform.localPosition = layer == HouseLayer.Yard
                ? new Vector3(bounds.center.x, 0.02f, bounds.min.z - 0.35f)
                : new Vector3(bounds.center.x, Mathf.Max(0.04f, bounds.max.y * 0.6f), bounds.center.z);
            root.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            _carameloScale = new Vector3(0.72f, 0.42f, 0.72f);
            root.transform.localScale = _carameloScale;
            GameObject model = CarameloModel.Instantiate(root.transform);
            _carameloTail = FindDeep(model.transform, CarameloParts.Tail);
            _caramelo = root;
            _spawned.Add(root);
        }

        private void Sparks(Transform bench, Bounds bounds)
        {
            for (int i = 0; i < 5; i++)
            {
                var local = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, 0.2f + 0.15f * i), bounds.max.y + 0.1f, bounds.center.z);
                _sparks.Add(Spawn(bench, "spark", local, Quaternion.identity).transform);
            }
        }

        private void Fireflies(HouseLevelSpec spec)
        {
            _fireflyCenter = new Vector3(HouseHullBuilder.InteriorWidth(spec) * 0.5f, HouseHullBuilder.FloorY + 1.3f, HouseHullBuilder.InteriorDepth(spec) * 0.5f);
            _fireflySpread = Mathf.Min(HouseHullBuilder.InteriorWidth(spec), HouseHullBuilder.InteriorDepth(spec)) * 0.35f;
            for (int i = 0; i < 9; i++)
            {
                _fireflies.Add(Spawn(_view.FurnitureRoot, "firefly", _fireflyCenter, Quaternion.identity).transform);
            }
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }

            return null;
        }

        private GameObject Spawn(Transform parent, string meshKey, Vector3 localPosition, Quaternion localRotation)
        {
            var go = new GameObject("Decor_" + meshKey);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            // Undo a mirrored wall piece's flip so plaques and gems keep their shape.
            Vector3 lossy = parent.lossyScale;
            go.transform.localScale = new Vector3(lossy.x < 0f ? -1f : 1f, 1f, 1f);
            go.AddComponent<MeshFilter>().sharedMesh = MeshFor(meshKey);
            go.AddComponent<MeshRenderer>().sharedMaterial = HousePalette.SharedMaterial;
            _spawned.Add(go);
            return go;
        }

        // ------------------------------------------------------------------
        // Animation
        // ------------------------------------------------------------------

        private void Update()
        {
            float t = Time.unscaledTime;
            if (_caramelo != null)
            {
                float breath = 1f + Mathf.Sin(t * 1.6f) * 0.035f;
                _caramelo.transform.localScale = new Vector3(_carameloScale.x, _carameloScale.y * breath, _carameloScale.z);
                if (_carameloTail != null)
                {
                    float speed = _fastWag ? 9f : 2.2f;
                    float swing = _fastWag ? 28f : 10f;
                    _carameloTail.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * speed) * swing, 0f);
                }
            }

            for (int i = 0; i < _fireflies.Count; i++)
            {
                Transform fly = _fireflies[i];
                if (fly == null) continue;
                float a = t * (0.3f + i * 0.04f) + i * 2.1f;
                fly.localPosition = _fireflyCenter + new Vector3(Mathf.Cos(a) * _fireflySpread, Mathf.Sin(a * 1.7f) * 0.5f, Mathf.Sin(a * 0.8f) * _fireflySpread);
                fly.localScale = Vector3.one * (0.6f + 0.4f * Mathf.Abs(Mathf.Sin(t * 2.3f + i)));
            }

            for (int i = 0; i < _sparks.Count; i++)
            {
                Transform spark = _sparks[i];
                if (spark == null) continue;
                float phase = Mathf.Repeat(t * 1.8f + i * 0.37f, 1f);
                spark.localScale = Vector3.one * (phase < 0.25f ? 1f - phase * 4f : 0f);
            }

            foreach (Transform drop in _drops)
            {
                if (drop == null) continue;
                drop.localRotation = Quaternion.Euler(0f, t * 90f, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Small cached meshes (palette material, a handful of triangles each)
        // ------------------------------------------------------------------

        private static readonly HouseSwatch[] BossTones = { HouseSwatch.Iron, HouseSwatch.Stone, HouseSwatch.ClothRed, HouseSwatch.Purple };

        public static Mesh MeshFor(string key)
        {
            if (Meshes.TryGetValue(key, out Mesh cached) && cached != null) return cached;
            var b = new HouseMeshBuilder();
            switch (key)
            {
                case "plaque_won":
                    b.Box(Vector3.zero, new Vector3(0.2f, 0.16f, 0.025f), HouseSwatch.WoodDark);
                    b.Box(new Vector3(0f, -0.055f, -0.014f), new Vector3(0.14f, 0.03f, 0.006f), HouseSwatch.Gold);
                    break;
                case "plaque_dim":
                    b.Box(Vector3.zero, new Vector3(0.2f, 0.16f, 0.025f), HouseSwatch.WoodJoint);
                    break;
                case "relic_found":
                    b.Box(new Vector3(0f, 0.015f, 0f), new Vector3(0.09f, 0.03f, 0.09f), HouseSwatch.WoodDark);
                    b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.07f, 0.09f, 0.07f), HouseSwatch.LampGlow);
                    break;
                case "relic_missing":
                    b.Box(new Vector3(0f, 0.015f, 0f), new Vector3(0.09f, 0.03f, 0.09f), HouseSwatch.WoodJoint);
                    break;
                case "wood_animal":
                    b.Box(new Vector3(0f, 0.05f, 0f), new Vector3(0.12f, 0.06f, 0.05f), HouseSwatch.WoodRaw);
                    b.Box(new Vector3(0.07f, 0.09f, 0f), new Vector3(0.05f, 0.05f, 0.04f), HouseSwatch.WoodRaw);
                    b.Box(new Vector3(-0.04f, 0.01f, 0f), new Vector3(0.02f, 0.03f, 0.04f), HouseSwatch.WoodJoint);
                    b.Box(new Vector3(0.04f, 0.01f, 0f), new Vector3(0.02f, 0.03f, 0.04f), HouseSwatch.WoodJoint);
                    break;
                case "water_drop":
                    b.Blob(Vector3.zero, new Vector3(0.07f, 0.11f, 0.07f), HouseSwatch.Water);
                    break;
                case "spark":
                    b.Blob(Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), HouseSwatch.LampGlow);
                    break;
                case "firefly":
                    b.Blob(Vector3.zero, new Vector3(0.06f, 0.06f, 0.06f), HouseSwatch.Purple);
                    b.Blob(Vector3.zero, new Vector3(0.035f, 0.035f, 0.035f), HouseSwatch.LampGlow);
                    break;
                default:
                    if (key.StartsWith("boss_head_", System.StringComparison.Ordinal) && int.TryParse(key.Substring(10), out int index))
                    {
                        // A little carved walnut head (§11.10; Sogdian woodcarving, no longer the carranca), tinted by the boss's region.
                        HouseSwatch tone = BossTones[Mathf.Clamp(index, 0, BossTones.Length - 1)];
                        b.Blob(Vector3.zero, new Vector3(0.1f, 0.11f, 0.06f), HouseSwatch.WoodRaw);
                        b.Blob(new Vector3(0f, 0.045f, -0.005f), new Vector3(0.11f, 0.04f, 0.05f), tone);
                        b.Box(new Vector3(-0.022f, 0.01f, -0.03f), new Vector3(0.015f, 0.015f, 0.01f), HouseSwatch.Black);
                        b.Box(new Vector3(0.022f, 0.01f, -0.03f), new Vector3(0.015f, 0.015f, 0.01f), HouseSwatch.Black);
                    }
                    else
                    {
                        b.Blob(Vector3.zero, Vector3.one * 0.05f, HouseSwatch.Gold);
                    }

                    break;
            }

            Mesh mesh = b.ToMesh("HouseDecor_" + key);
            Meshes[key] = mesh;
            return mesh;
        }
    }
}
