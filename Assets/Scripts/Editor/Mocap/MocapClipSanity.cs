using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor.Mocap
{
    /// <summary>
    /// Cheap health check of a baked humanoid clip: muscle curves far outside ±1 mean the retarget fought the
    /// avatar (the deformed CMU bakes had Shoulder Front-Back 1.23 held for the whole walk, Lower Leg Stretch 1.34,
    /// Upper Leg Twist ±3); a natural walk/run should stay within about ±1.1. Used by the importer's log
    /// and by <see cref="HeroAnimatorRebuilder"/> before it trusts a mocap clip for locomotion.
    /// </summary>
    public static class MocapClipSanity
    {
        /// <summary>|muscle| above this is reported as out of range.</summary>
        public const float MaxMuscle = 1.2f;

        /// <summary>Muscle curves whose peak |value| exceeds <paramref name="limit"/>, worst first.</summary>
        public static List<(string muscle, float peak)> OutOfRange(AnimationClip clip, float limit = MaxMuscle)
        {
            var list = new List<(string, float)>();
            if (clip == null) return list;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (binding.type != typeof(Animator) || binding.propertyName.StartsWith("Root") || binding.propertyName.Contains("Hand.")) continue;
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null) continue;
                float peak = 0f;
                foreach (var k in curve.keys) peak = Mathf.Max(peak, Mathf.Abs(k.value));
                if (peak > limit) list.Add((binding.propertyName, peak));
            }
            list.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            return list;
        }

        public static bool IsHealthy(AnimationClip clip) => clip != null && OutOfRange(clip).Count == 0;

        /// <summary>"" when healthy, else " — out of range: Lower Leg Stretch 1.34, …" (for log lines).</summary>
        public static string Describe(AnimationClip clip)
        {
            var bad = OutOfRange(clip);
            if (bad.Count == 0) return string.Empty;
            var sb = new StringBuilder(" — out of range: ");
            for (int i = 0; i < bad.Count && i < 4; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(bad[i].muscle).Append(' ').Append(bad[i].peak.ToString("0.00"));
            }
            if (bad.Count > 4) sb.Append($" (+{bad.Count - 4})");
            return sb.ToString();
        }
    }
}
