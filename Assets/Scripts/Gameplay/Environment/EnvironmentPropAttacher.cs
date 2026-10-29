using Rapadura.Gameplay.Environment.Impact;
using Rapadura.Gameplay.Environment.Wind;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Adds <see cref="WindSway"/> (trees, grass tufts, cacti) and <see cref="GroundImpactShaker"/>
    /// (rocks, pebbles, twigs, stalagmites) to the props of a loaded scene, by the naming rules in
    /// <see cref="EnvironmentPropRules"/>. Scenes and prefabs therefore need no hand wiring; placing
    /// either component by hand (to tune one instance) is respected, since existing components are
    /// kept and only re-captured.
    /// </summary>
    public static class EnvironmentPropAttacher
    {
        /// <summary>Scans every active object in the loaded scenes. Cheap enough for once per scene load.</summary>
        public static void AttachAll()
        {
            LODGroup[] lodGroups = Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < lodGroups.Length; i++)
            {
                Attach(lodGroups[i].gameObject);
            }

            MeshFilter[] filters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < filters.Length; i++)
            {
                GameObject go = filters[i].gameObject;
                if (go.GetComponent<LODGroup>() == null)
                {
                    Attach(go);
                }
            }
        }

        /// <summary>
        /// Classifies one object and adds/re-captures the matching behaviour. Returns true when the
        /// object got (or already had) a wind or impact component.
        /// </summary>
        public static bool Attach(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            bool hasLod = go.GetComponent<LODGroup>() != null;
            string meshName = null;
            if (go.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
            {
                meshName = filter.sharedMesh.name;
            }

            bool attached = false;

            WindSwayKind windKind = EnvironmentPropRules.ClassifyWind(go.name, meshName, hasLod);
            if (windKind != WindSwayKind.None)
            {
                if (!go.TryGetComponent(out WindSway sway))
                {
                    sway = go.AddComponent<WindSway>();
                    sway.Configure(windKind);
                }
                else
                {
                    sway.CaptureRest();
                }

                attached = true;
            }

            GroundImpactKind impactKind = EnvironmentPropRules.ClassifyImpact(go.name);
            if (impactKind != GroundImpactKind.None)
            {
                if (!go.TryGetComponent(out GroundImpactShaker shaker))
                {
                    shaker = go.AddComponent<GroundImpactShaker>();
                    shaker.Configure(impactKind, SensitivityFor(go, impactKind));
                }
                else
                {
                    shaker.CaptureRest();
                }

                attached = true;
            }

            return attached;
        }

        /// <summary>Bigger rocks budge less: sensitivity falls with the object's scale (1 at scale 1, ~0.4 at scale 3).</summary>
        private static float SensitivityFor(GameObject go, GroundImpactKind kind)
        {
            if (kind != GroundImpactKind.Boulder)
            {
                return 1f;
            }

            Vector3 s = go.transform.lossyScale;
            float size = Mathf.Max(s.x, s.y, s.z);
            return Mathf.Clamp(1.4f / Mathf.Max(0.5f, size + 0.4f), 0.3f, 1.5f);
        }
    }
}
