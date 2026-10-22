using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>
    /// Meshy's rigged FBX exports fuse a flat "toy figure stand" into the same skinned mesh as the
    /// character body, with no separate submesh/material to hide (see
    /// <c>Assets/Scripts/Editor/PedestalRemover.cs</c> for how the cleaned meshes were generated and
    /// validated character-by-character — not every character has one safe to cut, since a couple of
    /// them have gradual bottom geometry that could just be a robe). This only swaps in a pre-baked
    /// cleaned <c>Mesh</c> asset if one exists for the given character under
    /// <c>Assets/Resources/CleanedMeshes/{CharacterId}_NoPedestal.asset</c> — call right after
    /// instantiating a character's model prefab. Rigged models get it on their SkinnedMeshRenderer;
    /// static models (MeshFilter only) get it too, re-oriented — see <see cref="ApplyToStaticMesh"/>.
    /// Safe no-op for characters without one.
    /// </summary>
    public static class PedestalMeshUtility
    {
        public static void ApplyIfAvailable(GameObject instanceRoot, CharacterId characterId)
        {
            if (instanceRoot == null)
            {
                return;
            }

            SkinnedMeshRenderer smr = instanceRoot.GetComponentInChildren<SkinnedMeshRenderer>();
            MeshFilter filter = smr == null ? instanceRoot.GetComponentInChildren<MeshFilter>() : null;
            if (smr == null && (filter == null || filter.sharedMesh == null))
            {
                return;
            }

            Mesh cleaned = LoadCleanedMesh(characterId);
            if (cleaned == null)
            {
                return;
            }

            if (smr != null)
            {
                smr.sharedMesh = cleaned;
                return;
            }

            ApplyToStaticMesh(filter, cleaned);
        }

        public static Mesh LoadCleanedMesh(CharacterId characterId)
        {
            return Resources.Load<Mesh>("CleanedMeshes/" + characterId + "_NoPedestal");
        }

        /// <summary>
        /// Static (non-rigged) Meshy FBX — e.g. the character-select preview's Joaquim_RO — rendered by
        /// a MeshFilter. The cleaned mesh was cut from the rigged export, which is Y-up, while the static
        /// export is Z-up and stood up by its import's -90° X node rotation; so when the original mesh's
        /// tallest axis is Z and the cleaned one's is Y, the node gets the matching +90° X turn to keep
        /// the hero upright and facing the same way (checked on Joaquim: same texture atlas, same facing).
        /// </summary>
        public static void ApplyToStaticMesh(MeshFilter filter, Mesh cleaned)
        {
            if (filter == null || cleaned == null)
            {
                return;
            }

            Mesh original = filter.sharedMesh;
            filter.sharedMesh = cleaned;

            if (original != null && IsZUp(original.bounds) && !IsZUp(cleaned.bounds))
            {
                filter.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static bool IsZUp(Bounds bounds)
        {
            return bounds.extents.z > bounds.extents.y;
        }
    }
}
