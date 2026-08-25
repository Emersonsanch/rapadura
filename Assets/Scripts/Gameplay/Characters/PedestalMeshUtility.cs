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
    /// instantiating a character's model prefab. Safe no-op for characters without one.
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
            if (smr == null)
            {
                return;
            }

            Mesh cleaned = Resources.Load<Mesh>("CleanedMeshes/" + characterId + "_NoPedestal");
            if (cleaned == null)
            {
                return;
            }

            smr.sharedMesh = cleaned;
        }
    }
}
