using System.IO;
using Rapadura.Gameplay.Mounts;
using UnityEditor;
using UnityEngine;

namespace Rapadura.EditorTools
{
    /// <summary>
    /// "Rapadura/Mounts/Bake Starter Mount Assets": writes the M1 starters of <see cref="MountCatalog"/> as
    /// <see cref="MountDefinition"/> assets under <c>Assets/Resources/Mounts</c> (each with its
    /// <see cref="MountAbilityDefinition"/> as a sub-asset) so designers can tune them in the Inspector. Optional: without
    /// the assets <see cref="MountCatalog.Get"/> builds the same definitions from code. Re-running overwrites the values.
    /// </summary>
    public static class MountAssetBaker
    {
        public const string Folder = "Assets/Resources/Mounts";

        [MenuItem("Rapadura/Mounts/Bake Starter Mount Assets")]
        public static void Bake()
        {
            Directory.CreateDirectory(Folder);
            foreach (MountSpec spec in MountCatalog.Specs)
            {
                string path = Folder + "/" + spec.MountId + ".asset";
                var definition = AssetDatabase.LoadAssetAtPath<MountDefinition>(path);
                MountAbilityDefinition ability = null;
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<MountDefinition>();
                    AssetDatabase.CreateAsset(definition, path);
                }
                else
                {
                    ability = definition.Ability;
                }

                if (ability == null || !AssetDatabase.IsSubAsset(ability))
                {
                    ability = ScriptableObject.CreateInstance<MountAbilityDefinition>();
                    AssetDatabase.AddObjectToAsset(ability, definition);
                }

                ability.name = spec.AbilityId;
                ability.Configure(spec.AbilityId, spec.AbilityKind, spec.Cooldown, spec.Duration, spec.Distance, spec.Radius, spec.Magnitude, spec.BossMultiplier);
                definition.Configure(spec, ability);
                definition.name = spec.MountId;
                EditorUtility.SetDirty(ability);
                EditorUtility.SetDirty(definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Mounts] Baked {MountCatalog.Specs.Count} starter mounts into {Folder}.");
        }
    }
}
