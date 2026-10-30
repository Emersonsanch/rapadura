using System.Text;
using Rapadura.Gameplay.World;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// Dresses the hand-placed NPCs of the loaded scenes at runtime (no scene edit): every <see cref="NpcInteractable"/>
    /// without a figure (e.g. Main.unity's Sandbox capsules <c>Npc_Shopkeeper</c>, <c>Npc_HermitMystic</c>...) gets an
    /// <see cref="NpcFigure"/> child standing on the ground, chosen from its dialogue id or its object name, and its own
    /// placeholder <see cref="MeshRenderer"/> is switched off. Colliders, triggers and the interaction stay exactly as
    /// they were. Runs after each scene load.
    /// </summary>
    public static class NpcSceneDresser
    {
        /// <summary>Off in tests/tools that want the bare capsules.</summary>
        public static bool Enabled { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            DressAll();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => DressAll();

        /// <summary>Dresses every undressed NPC in the loaded scenes (active or not). Returns how many got a figure.</summary>
        public static int DressAll()
        {
            if (!Enabled)
            {
                return 0;
            }

            int count = 0;
            foreach (NpcInteractable npc in Object.FindObjectsByType<NpcInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (Dress(npc) != null)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Gives <paramref name="npc"/> its figure (once) and hides its placeholder renderer.</summary>
        public static NpcFigure Dress(NpcInteractable npc)
        {
            if (npc == null)
            {
                return null;
            }

            Transform root = npc.transform;
            if (root.Find(NpcFigure.ObjectName) != null || !HasOnlyPlaceholder(root))
            {
                return null;
            }

            string id = IdFor(npc);
            Vector3 feetWorld = FeetOf(root);
            if (root.TryGetComponent(out MeshRenderer placeholder))
            {
                placeholder.enabled = false;
            }

            NpcFigure figure = NpcFigure.Create(root, id, BiomeType.Forest, Vector3.zero);
            Transform t = figure.transform;
            t.position = feetWorld;
            t.rotation = Quaternion.Euler(0f, root.eulerAngles.y, 0f);
            Vector3 lossy = root.lossyScale;
            t.localScale = new Vector3(SafeInverse(lossy.x), SafeInverse(lossy.y), SafeInverse(lossy.z));
            figure.ResetHome();
            return figure;
        }

        private static readonly string[] PrimitiveMeshes = { "Capsule", "Cylinder", "Cube", "Sphere", "Quad", "Plane" };

        /// <summary>True when the NPC shows nothing but a primitive placeholder (or nothing): a real model is left alone.</summary>
        public static bool HasOnlyPlaceholder(Transform root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.transform != root)
                {
                    return false;
                }
            }

            if (!root.TryGetComponent(out MeshFilter filter) || filter.sharedMesh == null)
            {
                return true;
            }

            return System.Array.IndexOf(PrimitiveMeshes, filter.sharedMesh.name) >= 0;
        }

        private static float SafeInverse(float v) => Mathf.Abs(v) < 1e-4f ? 1f : 1f / v;

        /// <summary>
        /// The recipe id for an interactable: its dialogue asset's id (<c>dialogue_hermit_mystic</c> → <c>hermit_mystic</c>)
        /// when that names someone, else its object name (<c>Npc_RuinsArchivist</c> → <c>ruins_archivist</c>).
        /// </summary>
        public static string IdFor(NpcInteractable npc)
        {
            if (npc.Dialogue != null)
            {
                string fromDialogue = npc.Dialogue.name;
                if (fromDialogue.StartsWith("dialogue_", System.StringComparison.Ordinal))
                {
                    fromDialogue = fromDialogue.Substring("dialogue_".Length);
                }

                if (NpcRecipes.Find(fromDialogue) != null)
                {
                    return fromDialogue;
                }
            }

            return IdFromObjectName(npc.name);
        }

        /// <summary><c>Npc_WindtrailScholar</c> → <c>windtrail_scholar</c>.</summary>
        public static string IdFromObjectName(string objectName)
        {
            string name = objectName ?? string.Empty;
            if (name.StartsWith("Npc_", System.StringComparison.Ordinal))
            {
                name = name.Substring(4);
            }

            var sb = new StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0 && name[i - 1] != '_')
                    {
                        sb.Append('_');
                    }

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c == ' ' || c == '-' ? '_' : c);
                }
            }

            return sb.ToString();
        }

        /// <summary>Where the NPC's feet go: the ground under it (ignoring triggers and itself), else the bottom of its placeholder.</summary>
        private static Vector3 FeetOf(Transform root)
        {
            Vector3 position = root.position;
            float bottom = position.y;
            if (root.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
            {
                bottom = root.TransformPoint(new Vector3(0f, filter.sharedMesh.bounds.min.y, 0f)).y;
            }

            var origin = new Vector3(position.x, Mathf.Max(position.y, bottom) + 0.6f, position.z);
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore);
            float best = float.NegativeInfinity;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(root) || hit.point.y > origin.y)
                {
                    continue;
                }

                best = Mathf.Max(best, hit.point.y);
            }

            return new Vector3(position.x, float.IsNegativeInfinity(best) ? bottom : best, position.z);
        }
    }
}
