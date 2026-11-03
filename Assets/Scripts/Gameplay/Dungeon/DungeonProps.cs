using System;
using Rapadura.Gameplay.Environment;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Builds the Ossuário dos Artífices from primitives at runtime (no assets, no scene edit), like <c>TorreProps</c>: the
    /// stone floor slab, the low hall walls with skull niches and fairy lights, the barred gates, the sarcophagi, the cracked
    /// bone wall and the Artificers' chest. Decorative parts have no collider; every solid piece is named
    /// <c>Ossuary_*</c> and gets its collider from <see cref="SolidPropBinder"/> (the project's one collision system), except
    /// the moving gate bars and the bone wall, which carry their own box (they open / break).
    /// </summary>
    public static class DungeonProps
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        public static readonly Color Stone = new Color(0.36f, 0.33f, 0.31f);
        public static readonly Color StoneDark = new Color(0.22f, 0.2f, 0.2f);
        public static readonly Color Floor = new Color(0.2f, 0.19f, 0.2f);
        public static readonly Color Ivory = new Color(0.9f, 0.86f, 0.74f);
        public static readonly Color BoneDust = new Color(0.82f, 0.78f, 0.68f);
        public static readonly Color Copper = new Color(0.72f, 0.45f, 0.26f);
        public static readonly Color Iron = new Color(0.28f, 0.28f, 0.32f);
        public static readonly Color Gold = new Color(1f, 0.8f, 0.3f);
        public static readonly Color Wisp = new Color(0.35f, 1f, 0.85f);
        public static readonly Color SmokePurple = new Color(0.55f, 0.3f, 0.75f);
        public static readonly Color Amber = new Color(1f, 0.72f, 0.38f);

        public const string Prefix = "Ossuary_";

        /// <summary>The floor slab, walls, niches and lights of the whole hall (under one root, in the given scene's space).</summary>
        public static GameObject BuildHall(DungeonHall hall, Func<Vector3, Vector3> ground)
        {
            DungeonLayout layout = hall.Layout;
            var root = new GameObject("DungeonHall");
            var block = new MaterialPropertyBlock();
            float h = DungeonLayout.WallHeight;
            float t = DungeonLayout.WallThickness;
            float baseY = hall.Origin.y;

            // Dark stone floor (visual only, a hair above the arena floor).
            float bossStart = layout.ColossusGateZ;
            Slab(root.transform, hall, 0f, layout.ColossusGateZ, DungeonLayout.HalfWidth + t, baseY, block);
            Slab(root.transform, hall, bossStart, layout.EndZ, DungeonLayout.BossHalfWidth + t, baseY, block);

            // Left wall: hall then boss chamber, joined by a short cross wall.
            Wall(root.transform, hall, "Wall_L", -(DungeonLayout.HalfWidth + t * 0.5f), 0f, bossStart, block, ground);
            Wall(root.transform, hall, "Wall_L_Boss", -(DungeonLayout.BossHalfWidth + t * 0.5f), bossStart, layout.EndZ, block, ground);
            CrossWall(root.transform, hall, "Wall_L_Step", -DungeonLayout.BossHalfWidth - t, -DungeonLayout.HalfWidth, bossStart, block, ground);

            // Right wall with the niche gap.
            float nicheA = layout.NicheZ - DungeonLayout.NicheHalfLength;
            float nicheB = layout.NicheZ + DungeonLayout.NicheHalfLength;
            float rx = DungeonLayout.HalfWidth + t * 0.5f;
            Wall(root.transform, hall, "Wall_R_A", rx, 0f, nicheA, block, ground);
            Wall(root.transform, hall, "Wall_R_B", rx, nicheB, bossStart, block, ground);
            Wall(root.transform, hall, "Wall_R_Boss", DungeonLayout.BossHalfWidth + t * 0.5f, bossStart, layout.EndZ, block, ground);
            CrossWall(root.transform, hall, "Wall_R_Step", DungeonLayout.HalfWidth, DungeonLayout.BossHalfWidth + t, bossStart, block, ground);

            // The niche: back wall and two side walls (its front is the bone wall).
            float back = DungeonLayout.HalfWidth + DungeonLayout.NicheDepth;
            Wall(root.transform, hall, "Niche_Back", back + t * 0.5f, nicheA - t, nicheB + t, block, ground);
            CrossWall(root.transform, hall, "Niche_Side_A", DungeonLayout.HalfWidth, back + t, nicheA - t * 0.5f, block, ground);
            CrossWall(root.transform, hall, "Niche_Side_B", DungeonLayout.HalfWidth, back + t, nicheB + t * 0.5f, block, ground);
            Slab(root.transform, hall, nicheA, nicheB, 0f, baseY, block, DungeonLayout.HalfWidth, back);

            // End wall of the boss chamber.
            CrossWall(root.transform, hall, "Wall_End", -DungeonLayout.BossHalfWidth - t, DungeonLayout.BossHalfWidth + t, layout.EndZ + t * 0.5f, block, ground);

            // Skull niches on top of the walls: every other one carries a real light (keeps the light count mobile-friendly).
            int index = 0;
            foreach (DungeonSpot spot in layout.LightNiches)
            {
                Vector3 p = ground(hall.ToWorld(spot.X, spot.Z));
                BuildSkullNiche(root.transform, p + Vector3.up * h, hall.Rotation, spot.X < 0f ? 1f : -1f, index++ % 2 == 0, block);
            }

            SolidPropBinder.BindHierarchy(root.transform);
            return root;
        }

        private static void Slab(Transform parent, DungeonHall hall, float z0, float z1, float half, float baseY, MaterialPropertyBlock block,
            float x0 = float.NaN, float x1 = float.NaN)
        {
            float a = float.IsNaN(x0) ? -half : x0;
            float b = float.IsNaN(x1) ? half : x1;
            Vector3 center = hall.ToWorld((a + b) * 0.5f, (z0 + z1) * 0.5f);
            center.y = baseY + 0.015f;
            GameObject slab = Part(parent, PrimitiveType.Cube, "FloorSlab", center, hall.Rotation, new Vector3(b - a, 0.03f, z1 - z0), Floor, block);
            slab.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Wall(Transform parent, DungeonHall hall, string name, float x, float z0, float z1, MaterialPropertyBlock block, Func<Vector3, Vector3> ground)
        {
            if (z1 - z0 < 0.05f)
            {
                return;
            }

            Vector3 center = ground(hall.ToWorld(x, (z0 + z1) * 0.5f)) + Vector3.up * (DungeonLayout.WallHeight * 0.5f);
            Part(parent, PrimitiveType.Cube, Prefix + name, center, hall.Rotation, new Vector3(DungeonLayout.WallThickness, DungeonLayout.WallHeight, z1 - z0), Stone, block);
            // A darker plinth along the foot of the wall.
            Vector3 plinth = ground(hall.ToWorld(x, (z0 + z1) * 0.5f)) + Vector3.up * 0.15f;
            Part(parent, PrimitiveType.Cube, "Plinth", plinth, hall.Rotation, new Vector3(DungeonLayout.WallThickness + 0.12f, 0.3f, z1 - z0), StoneDark, block);
        }

        private static void CrossWall(Transform parent, DungeonHall hall, string name, float x0, float x1, float z, MaterialPropertyBlock block, Func<Vector3, Vector3> ground)
        {
            float width = Mathf.Abs(x1 - x0);
            if (width < 0.05f)
            {
                return;
            }

            Vector3 center = ground(hall.ToWorld((x0 + x1) * 0.5f, z)) + Vector3.up * (DungeonLayout.WallHeight * 0.5f);
            Part(parent, PrimitiveType.Cube, Prefix + name, center, hall.Rotation, new Vector3(width, DungeonLayout.WallHeight, DungeonLayout.WallThickness), Stone, block);
        }

        private static void BuildSkullNiche(Transform parent, Vector3 top, Quaternion rotation, float facing, bool withLight, MaterialPropertyBlock block)
        {
            var niche = new GameObject("SkullNiche");
            niche.transform.SetParent(parent, false);
            niche.transform.SetPositionAndRotation(top, rotation);
            Part(niche.transform, PrimitiveType.Cube, "Shelf", top + Vector3.up * 0.08f, rotation, new Vector3(0.7f, 0.16f, 0.7f), StoneDark, block);
            Vector3 skull = top + Vector3.up * 0.33f;
            Part(niche.transform, PrimitiveType.Sphere, "Skull", skull, rotation, new Vector3(0.34f, 0.3f, 0.36f), Ivory, block);
            Vector3 side = rotation * Vector3.right * (0.09f * facing);
            Part(niche.transform, PrimitiveType.Sphere, "Wisp_L", skull + Vector3.up * 0.03f + rotation * Vector3.forward * 0.08f + side, rotation, Vector3.one * 0.08f, Wisp, block, glow: true);
            Part(niche.transform, PrimitiveType.Sphere, "Wisp_R", skull + Vector3.up * 0.03f - rotation * Vector3.forward * 0.08f + side, rotation, Vector3.one * 0.08f, Wisp, block, glow: true);
            Part(niche.transform, PrimitiveType.Sphere, "Flame", skull + Vector3.up * 0.28f, rotation, new Vector3(0.12f, 0.2f, 0.12f), Wisp, block, glow: true);
            if (withLight)
            {
                Light light = AddLight(niche.transform, skull + Vector3.up * 0.3f, Wisp, 1.1f, 4.5f);
                light.gameObject.AddComponent<DungeonFlicker>();
            }
        }

        /// <summary>A barred gate across the hall at <paramref name="z"/>; the bars rise when it opens.</summary>
        public static LockedDungeonGate BuildGate(DungeonHall hall, float z, string gateId, Func<Vector3, Vector3> ground)
        {
            float half = hall.Layout.HalfWidthAt(z - 0.6f);
            Vector3 center = ground(hall.ToWorld(0f, z));
            var root = new GameObject("DungeonGate_" + gateId);
            root.transform.SetPositionAndRotation(center, hall.Rotation);
            var block = new MaterialPropertyBlock();

            // Two carved pillars (solid) and a lintel with a copper skull.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 p = center + hall.Right * (s * (half + 0.15f)) + Vector3.up * 1.4f;
                Part(root.transform, PrimitiveType.Cube, Prefix + "Pillar_" + (s < 0 ? "L" : "R"), p, hall.Rotation, new Vector3(0.75f, 2.8f, 0.75f), StoneDark, block);
            }

            Part(root.transform, PrimitiveType.Cube, "Lintel", center + Vector3.up * 2.95f, hall.Rotation, new Vector3(half * 2f + 1.5f, 0.35f, 0.8f), Stone, block);
            Part(root.transform, PrimitiveType.Sphere, "LintelSkull", center + Vector3.up * 3.25f, hall.Rotation, new Vector3(0.5f, 0.45f, 0.45f), Copper, block);

            var bars = new GameObject("Bars");
            bars.transform.SetParent(root.transform, false);
            int count = Mathf.Max(5, Mathf.RoundToInt(half * 2f / 0.42f));
            for (int i = 0; i < count; i++)
            {
                float x = -half + (i + 0.5f) * (half * 2f / count);
                Part(bars.transform, PrimitiveType.Cylinder, "Bar", center + hall.Right * x + Vector3.up * 1.3f, hall.Rotation, new Vector3(0.09f, 1.3f, 0.09f), Iron, block);
            }

            for (int k = 0; k < 2; k++)
            {
                Part(bars.transform, PrimitiveType.Cube, "Band", center + Vector3.up * (0.6f + k * 1.4f), hall.Rotation, new Vector3(half * 2f, 0.1f, 0.12f), Iron, block);
            }

            // A bone keyhole with a soft rune glow (the highlight light).
            Part(bars.transform, PrimitiveType.Cube, "Lock", center + Vector3.up * 1.2f - hall.Forward * 0.08f, hall.Rotation, new Vector3(0.36f, 0.42f, 0.12f), Ivory, block);
            Light glow = AddLight(root.transform, center + Vector3.up * 1.4f - hall.Forward * 0.6f, Wisp, 0.8f, 3.5f);

            var body = bars.AddComponent<BoxCollider>();
            body.center = bars.transform.InverseTransformPoint(center + Vector3.up * 1.3f);
            body.size = new Vector3(half * 2f, 2.6f, 0.35f);

            SolidPropBinder.BindHierarchy(root.transform);
            var gate = root.AddComponent<LockedDungeonGate>();
            gate.Configure(gateId, bars.transform, glow);
            return gate;
        }

        /// <summary>A stone sarcophagus against a side wall (<paramref name="spot"/>), its long side along the hall.</summary>
        public static SarcophagusInteractable BuildSarcophagus(DungeonHall hall, DungeonSpot spot, string id, Func<Vector3, Vector3> ground)
        {
            Vector3 p = ground(hall.ToWorld(spot));
            var root = new GameObject(Prefix + "Sarcophagus_" + id);
            root.transform.SetPositionAndRotation(p, hall.Rotation * Quaternion.Euler(0f, spot.X < 0f ? 90f : -90f, 0f));
            var block = new MaterialPropertyBlock();
            Transform r = root.transform;
            Part(r, PrimitiveType.Cube, "Base", r.TransformPoint(0f, 0.4f, 0f), r.rotation, new Vector3(1.9f, 0.8f, 0.95f), Stone, block);
            Part(r, PrimitiveType.Cube, "Trim", r.TransformPoint(0f, 0.12f, 0f), r.rotation, new Vector3(2.0f, 0.22f, 1.05f), StoneDark, block);

            var lid = new GameObject("Lid");
            lid.transform.SetParent(r, false);
            lid.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            Part(lid.transform, PrimitiveType.Cube, "Slab", r.TransformPoint(0f, 0.88f, 0f), r.rotation, new Vector3(2.0f, 0.16f, 1.0f), Stone, block);
            // An ushabti face carved on the lid: ivory oval, two copper eyes.
            Part(lid.transform, PrimitiveType.Sphere, "Face", r.TransformPoint(-0.55f, 1.0f, 0f), r.rotation, new Vector3(0.42f, 0.1f, 0.34f), Ivory, block);
            Part(lid.transform, PrimitiveType.Cube, "Band", r.TransformPoint(0.25f, 0.98f, 0f), r.rotation, new Vector3(0.12f, 0.04f, 0.9f), Copper, block);

            Light glow = AddLight(r, r.TransformPoint(0f, 1.5f, 0.6f), Amber, 0f, 3.5f);
            SolidPropBinder.Bind(root);
            var sarcophagus = root.AddComponent<SarcophagusInteractable>();
            sarcophagus.Configure(id, lid.transform, glow);
            return sarcophagus;
        }

        /// <summary>The Artificers' legendary chest at the back of the niche.</summary>
        public static SarcophagusInteractable BuildLegendaryChest(DungeonHall hall, DungeonSpot spot, string id, string guaranteedItemId, Func<Vector3, Vector3> ground)
        {
            Vector3 p = ground(hall.ToWorld(spot));
            var root = new GameObject(Prefix + "Chest_" + id);
            root.transform.SetPositionAndRotation(p, hall.Rotation * Quaternion.Euler(0f, -90f, 0f));
            var block = new MaterialPropertyBlock();
            Transform r = root.transform;
            Part(r, PrimitiveType.Cube, "Box", r.TransformPoint(0f, 0.35f, 0f), r.rotation, new Vector3(1.2f, 0.7f, 0.8f), Ivory, block);
            Part(r, PrimitiveType.Cube, "GoldBand_A", r.TransformPoint(-0.35f, 0.36f, 0f), r.rotation, new Vector3(0.1f, 0.72f, 0.82f), Gold, block);
            Part(r, PrimitiveType.Cube, "GoldBand_B", r.TransformPoint(0.35f, 0.36f, 0f), r.rotation, new Vector3(0.1f, 0.72f, 0.82f), Gold, block);
            var lid = new GameObject("Lid");
            lid.transform.SetParent(r, false);
            lid.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            Part(lid.transform, PrimitiveType.Cube, "Top", r.TransformPoint(0f, 0.8f, 0f), r.rotation, new Vector3(1.25f, 0.18f, 0.85f), Ivory, block);
            Part(lid.transform, PrimitiveType.Sphere, "Gem", r.TransformPoint(0f, 0.92f, -0.1f), r.rotation, Vector3.one * 0.16f, Wisp, block, glow: true);
            Light glow = AddLight(r, r.TransformPoint(0f, 1.4f, 0f), Gold, 0.9f, 3f);
            SolidPropBinder.Bind(root);
            var chest = root.AddComponent<SarcophagusInteractable>();
            chest.Configure(id, lid.transform, glow, legendary: true, guaranteedItemId: guaranteedItemId);
            return chest;
        }

        /// <summary>The cracked wall of stacked bones closing the niche.</summary>
        public static BreakableBoneWall BuildBoneWall(DungeonHall hall, DungeonSpot spot, string id, Func<Vector3, Vector3> ground)
        {
            Vector3 p = ground(hall.ToWorld(spot));
            var root = new GameObject(Prefix + "BoneWall");
            root.transform.SetPositionAndRotation(p, hall.Rotation);
            var block = new MaterialPropertyBlock();
            Transform r = root.transform;
            float length = DungeonLayout.NicheHalfLength * 2f;
            Part(r, PrimitiveType.Cube, "Core", r.TransformPoint(0f, DungeonLayout.WallHeight * 0.5f, 0f), r.rotation,
                new Vector3(DungeonLayout.WallThickness * 0.8f, DungeonLayout.WallHeight, length), BoneDust, block);
            var random = new System.Random(7);
            for (int row = 0; row < 3; row++)
            {
                for (int i = 0; i < 5; i++)
                {
                    float z = -length * 0.5f + (i + 0.5f) * (length / 5f);
                    float y = 0.3f + row * 0.55f;
                    Part(r, i % 2 == row % 2 ? PrimitiveType.Sphere : PrimitiveType.Capsule, "Bone", r.TransformPoint(-0.05f, y, z),
                        r.rotation * Quaternion.Euler((float)random.NextDouble() * 40f, 0f, 90f), new Vector3(0.32f, 0.3f, 0.32f), Ivory, block);
                }
            }

            // The crack: a dark zigzag on the hall side.
            Part(r, PrimitiveType.Cube, "Crack", r.TransformPoint(-0.3f, 0.9f, 0f), r.rotation * Quaternion.Euler(0f, 0f, 25f), new Vector3(0.04f, 1.1f, 0.06f), StoneDark, block);
            var body = root.AddComponent<BoxCollider>();
            body.center = new Vector3(0f, DungeonLayout.WallHeight * 0.5f, 0f);
            body.size = new Vector3(DungeonLayout.WallThickness, DungeonLayout.WallHeight, length);
            var wall = root.AddComponent<BreakableBoneWall>();
            wall.Configure(id);
            return wall;
        }

        // ------------------------------------------------------------------
        // Shared helpers (also used by the projectiles and the boss's cage)
        // ------------------------------------------------------------------

        public static Light AddLight(Transform parent, Vector3 position, Color color, float intensity, float range)
        {
            var light = new GameObject("Light").AddComponent<Light>();
            light.transform.SetParent(parent, true);
            light.transform.position = position;
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        /// <summary>A coloured primitive without a collider (world-space pose), drawn with the default material and a property block.</summary>
        public static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Quaternion rotation, Vector3 scale, Color color,
            MaterialPropertyBlock block, bool glow = false)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            Transform t = part.transform;
            if (parent != null)
            {
                t.SetParent(parent, true);
            }

            t.SetPositionAndRotation(position, rotation);
            t.localScale = parent != null ? Divide(scale, parent.lossyScale) : scale;

            if (part.TryGetComponent(out Collider collider))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(collider);
                }
                else
                {
                    Object.DestroyImmediate(collider);
                }
            }

            Renderer renderer = part.GetComponent<Renderer>();
            renderer.shadowCastingMode = glow ? ShadowCastingMode.Off : ShadowCastingMode.On;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            block ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            if (glow)
            {
                block.SetColor(EmissionColorId, color * 1.6f);
            }

            renderer.SetPropertyBlock(block);
            return part;
        }

        private static Vector3 Divide(Vector3 a, Vector3 b)
        {
            return new Vector3(b.x != 0f ? a.x / b.x : a.x, b.y != 0f ? a.y / b.y : a.y, b.z != 0f ? a.z / b.z : a.z);
        }
    }

    /// <summary>Gentle fairy-fire flicker of a niche light (scaled time; pauses with the game).</summary>
    public class DungeonFlicker : MonoBehaviour
    {
        private Light _light;
        private float _base;
        private float _seed;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _base = _light != null ? _light.intensity : 1f;
            _seed = (Mathf.Abs(GetInstanceID()) % 97) * 0.37f;
        }

        private void Update()
        {
            if (_light != null)
            {
                float t = Time.time + _seed;
                _light.intensity = _base * (0.85f + 0.1f * Mathf.Sin(t * 3.1f) + 0.05f * Mathf.Sin(t * 7.3f));
            }
        }
    }

    /// <summary>A puff (purple smoke, bone dust) or a spray of pebbles: plain primitives that grow/fly and vanish (no particle material).</summary>
    public class DungeonPuff : MonoBehaviour
    {
        private float _life;
        private float _age;
        private Vector3 _from;
        private Vector3 _to;
        private Vector3 _velocity;
        private bool _ballistic;

        public static void Spawn(Vector3 position, Color color, float size, float seconds, int count = 4)
        {
            var block = new MaterialPropertyBlock();
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                Vector3 offset = new Vector3(Mathf.Cos(a), 0.15f * i, Mathf.Sin(a)) * (size * 0.18f);
                GameObject puff = DungeonProps.Part(null, PrimitiveType.Sphere, "Puff", position + offset, Quaternion.identity, Vector3.one * 0.1f, color, block, glow: true);
                var fx = puff.AddComponent<DungeonPuff>();
                fx._life = seconds * (0.8f + 0.1f * i);
                fx._from = Vector3.one * (size * 0.2f);
                fx._to = Vector3.one * (size * (0.55f + 0.1f * i));
            }
        }

        public static void Pebbles(Vector3 position, Color color, int count)
        {
            var block = new MaterialPropertyBlock();
            var random = new System.Random(count * 31 + 5);
            for (int i = 0; i < count; i++)
            {
                GameObject pebble = DungeonProps.Part(null, PrimitiveType.Cube, "Pebble", position, Quaternion.Euler(i * 37f, i * 53f, 0f), Vector3.one * 0.16f, color, block);
                var fx = pebble.AddComponent<DungeonPuff>();
                fx._ballistic = true;
                fx._life = 1.1f;
                float a = (float)random.NextDouble() * Mathf.PI * 2f;
                fx._velocity = new Vector3(Mathf.Cos(a) * 2.4f, 2f + (float)random.NextDouble() * 2.5f, Mathf.Sin(a) * 2.4f);
                fx._from = fx._to = Vector3.one * 0.16f;
            }
        }

        private void Update()
        {
            _age += Time.deltaTime;
            float k = Mathf.Clamp01(_age / Mathf.Max(0.01f, _life));
            if (_ballistic)
            {
                _velocity += Physics.gravity * Time.deltaTime;
                transform.position += _velocity * Time.deltaTime;
                transform.localScale = _from * (1f - k * k);
            }
            else
            {
                // Grow fast, then shrink away.
                float grow = Mathf.Sin(k * Mathf.PI);
                transform.localScale = Vector3.Lerp(_from, _to, Mathf.Sqrt(k)) * Mathf.Max(0.05f, grow);
                transform.position += Vector3.up * (0.6f * Time.deltaTime);
            }

            if (_age >= _life)
            {
                Destroy(gameObject);
            }
        }
    }
}
