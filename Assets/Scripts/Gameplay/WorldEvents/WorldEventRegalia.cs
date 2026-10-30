using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>
    /// Code-built low-poly props that mark the scheduled-event enemies, drawn with the procedural enemies' own
    /// <see cref="EnemyMeshBuilder"/> and palette material (one shared material, no new texture):
    /// <list type="bullet">
    /// <item><b>Coroa do arconte</b> (world bosses): a gold band with five spikes alternating gold and smoke (the Treva),
    /// a glowing crystal at each spike's foot (emissive palette cell: it reads at night).</item>
    /// <item><b>Coroinha de lata dourada</b> (Invasão de Āz, Docs/WorldBible_Maniqueismo.md §3.4): the small crooked tin
    /// crown Āz jams on every critter she enchants, with a stolen spark on the front. The Capitã's is bigger.</item>
    /// </list>
    /// Parented to the model's <c>Head</c> part (it nods with the gait and the attack) or to the top of the model when it
    /// has none (the bat swarm). Meshes are built once and shared. Models face +Z with the pivot at the base.
    /// </summary>
    public static class WorldEventRegalia
    {
        public const string CrownName = "WorldBossCrown";
        public const string HatName = "CangacoHat";

        private static Mesh _crown;
        private static Mesh _hat;
        private static readonly List<Renderer> Renderers = new List<Renderer>();

        public static Mesh CrownMesh
        {
            get
            {
                if (_crown == null)
                {
                    _crown = BuildCrown();
                }

                return _crown;
            }
        }

        public static Mesh HatMesh
        {
            get
            {
                if (_hat == null)
                {
                    _hat = BuildHat();
                }

                return _hat;
            }
        }

        /// <summary>Crown of 1 m diameter at scale 1.</summary>
        public static Mesh BuildCrown()
        {
            var b = new EnemyMeshBuilder();
            b.Cylinder(Vector3.zero, 0.5f, 0.54f, 0.16f, 10, EnemySwatch.Gold, bottomCap: false);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Sin(angle) * 0.47f, 0.12f, Mathf.Cos(angle) * 0.47f);
                EnemySwatch spike = i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.Shadow;
                b.Cylinder(at, 0.11f, 0f, i == 0 ? 0.42f : 0.32f, 4, spike);
                b.Crystal(at + new Vector3(0f, -0.06f, 0f) + at.normalized * 0.06f, 0.05f, 0.1f, EnemySwatch.Crystal);
            }

            Mesh mesh = b.ToMesh("WorldEvent_Crown");
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        /// <summary>
        /// Āz's little tin crown, 1 m wide at scale 1: a thin dented gold band, six uneven points with a bead on each tip
        /// and a stolen spark (glowing <see cref="EnemySwatch.StolenLight"/>) stuck on the front ("tudo o que brilha é
        /// meu"). Sits a bit crooked, like it was jammed on in a hurry.
        /// </summary>
        public static Mesh BuildHat()
        {
            var b = new EnemyMeshBuilder();
            Matrix4x4 saved = b.Matrix;
            b.Matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 9f), Vector3.one);

            b.Cylinder(Vector3.zero, 0.46f, 0.5f, 0.14f, 8, EnemySwatch.Gold, bottomCap: false);
            for (int i = 0; i < 6; i++)
            {
                float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Sin(angle) * 0.45f, 0.11f, Mathf.Cos(angle) * 0.45f);
                float height = i % 2 == 0 ? 0.3f : 0.22f;
                b.Cylinder(at, 0.1f, 0f, height, 4, EnemySwatch.Gold);
                b.Cylinder(at + new Vector3(0f, height - 0.02f, 0f), 0.035f, 0.035f, 0.05f, 5, EnemySwatch.Gold);
            }

            // The stolen spark on the front of the band.
            b.Crystal(new Vector3(0f, 0.02f, 0.5f), 0.07f, 0.13f, EnemySwatch.StolenLight, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Matrix = saved;

            Mesh mesh = b.ToMesh("WorldEvent_CangacoHat");
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        /// <summary>Puts the crown on <paramref name="enemy"/> (idempotent). <paramref name="widthOfHead"/>: crown diameter ÷ head width.</summary>
        public static GameObject AddCrown(GameObject enemy, float widthOfHead = 1.1f) => Add(enemy, CrownName, CrownMesh, widthOfHead, 0f);

        /// <summary>Puts Āz's tin crown on <paramref name="enemy"/> (idempotent). Ids (<see cref="HatName"/>, the mesh name) stay from the old hat.</summary>
        public static GameObject AddHat(GameObject enemy, float widthOfHead = 0.95f) => Add(enemy, HatName, HatMesh, widthOfHead, 0.06f);

        private static GameObject Add(GameObject enemy, string name, Mesh mesh, float widthOfHead, float sink)
        {
            if (enemy == null || mesh == null)
            {
                return null;
            }

            Transform visual = enemy.transform.Find(EnemyVisualSwapper.VisualName);
            Transform root = visual != null ? visual : enemy.transform;
            Transform existing = FindDeep(root, name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            Transform head = FindDeep(root, ProceduralEnemyParts.Head);
            Bounds whole = BoundsOf(root, null);
            Bounds headBounds = head != null ? BoundsOf(head, root) : whole;
            if (head == null || headBounds.size.sqrMagnitude < 0.0001f)
            {
                head = root;
                headBounds = whole;
            }

            float width = Mathf.Max(headBounds.size.x, headBounds.size.z);
            if (width < 0.05f)
            {
                width = Mathf.Max(0.4f, whole.size.x * 0.4f);
            }

            float diameter = Mathf.Clamp(width * widthOfHead, 0.25f, 3f);
            var prop = new GameObject(name);
            prop.transform.SetParent(head, false);
            prop.transform.position = new Vector3(headBounds.center.x, headBounds.max.y - diameter * sink - diameter * 0.08f, headBounds.center.z);
            prop.transform.rotation = Quaternion.LookRotation(Flat(enemy.transform.forward), Vector3.up);
            float parentScale = Mathf.Max(0.0001f, head.lossyScale.x);
            prop.transform.localScale = Vector3.one * (diameter / parentScale);
            prop.AddComponent<MeshFilter>().sharedMesh = mesh;
            prop.AddComponent<MeshRenderer>().sharedMaterial = ProceduralEnemyModels.SharedMaterial;
            return prop;
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }

        /// <summary>World bounds of the renderers directly on <paramref name="t"/> (a part) or under it (the model root).</summary>
        private static Bounds BoundsOf(Transform t, Transform modelRoot)
        {
            var bounds = new Bounds(t.position, Vector3.zero);
            bool any = false;
            if (modelRoot != null)
            {
                // A part: only its own mesh (children are ears, jaws… that would widen it).
                if (t.TryGetComponent(out Renderer own))
                {
                    return own.bounds;
                }
            }

            t.GetComponentsInChildren(false, Renderers);
            foreach (Renderer renderer in Renderers)
            {
                if (renderer == null || renderer is ParticleSystemRenderer)
                {
                    continue;
                }

                if (!any)
                {
                    bounds = renderer.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            Renderers.Clear();
            return bounds;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
