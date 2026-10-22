using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Pure decision logic behind <see cref="BiomeArenaDressing"/> (no scene access), so profile
    /// resolution, show/hide matching, tint maths and prop scattering are EditMode-testable.
    /// </summary>
    public static class BiomeArenaRules
    {
        /// <summary>The arena is only re-dressed for a real encounter; the sandbox Main (no payload) stays as authored.</summary>
        public static bool ShouldDress(bool hasPendingEncounter, BiomeArenaProfile profile)
        {
            return hasPendingEncounter && profile != null;
        }

        /// <summary>First non-null candidate whose <see cref="BiomeArenaProfile.Biome"/> matches, else null.</summary>
        public static BiomeArenaProfile SelectProfile(BiomeType biome, IEnumerable<BiomeArenaProfile> candidates)
        {
            if (candidates == null)
            {
                return null;
            }

            foreach (BiomeArenaProfile candidate in candidates)
            {
                if (candidate != null && candidate.Biome == biome)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>Whether <paramref name="rule"/> applies to an object called <paramref name="objectName"/> tagged <paramref name="objectTag"/>.</summary>
        public static bool Matches(BiomeArenaPropRule rule, string objectName, string objectTag)
        {
            if (rule == null || string.IsNullOrEmpty(rule.NamePrefix) || string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            if (!objectName.StartsWith(rule.NamePrefix, StringComparison.Ordinal))
            {
                return false;
            }

            return string.IsNullOrEmpty(rule.Tag) || string.Equals(rule.Tag, objectTag, StringComparison.Ordinal);
        }

        /// <summary>
        /// The action for an object: the first matching rule's action (and its tint), or
        /// <see cref="BiomeArenaPropAction.Keep"/> with white when no rule matches.
        /// </summary>
        public static BiomeArenaPropAction ResolveAction(string objectName, string objectTag, IReadOnlyList<BiomeArenaPropRule> rules, out Color tint)
        {
            tint = Color.white;
            if (rules == null)
            {
                return BiomeArenaPropAction.Keep;
            }

            for (int i = 0; i < rules.Count; i++)
            {
                BiomeArenaPropRule rule = rules[i];
                if (Matches(rule, objectName, objectTag))
                {
                    tint = rule.Tint;
                    return rule.Action;
                }
            }

            return BiomeArenaPropAction.Keep;
        }

        /// <summary>True if any rule filters by tag (lets the scene walk skip reading GameObject.tag otherwise).</summary>
        public static bool AnyRuleUsesTag(IReadOnlyList<BiomeArenaPropRule> rules)
        {
            if (rules == null)
            {
                return false;
            }

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] != null && !string.IsNullOrEmpty(rules[i].Tag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Blends <paramref name="baseColor"/> toward <paramref name="tint"/> by the tint's alpha, keeping the base alpha (so per-instance variation survives partial tints).</summary>
        public static Color ApplyTint(Color baseColor, Color tint)
        {
            float strength = Mathf.Clamp01(tint.a);
            Color blended = Color.Lerp(baseColor, tint, strength);
            blended.a = baseColor.a;
            return blended;
        }

        /// <summary>
        /// Deterministic, area-uniform scatter of <paramref name="count"/> XZ offsets on the ring
        /// [<paramref name="innerRadius"/>, <paramref name="outerRadius"/>] around the arena centre.
        /// Appends to <paramref name="results"/> (not cleared) so callers can reuse one buffer.
        /// </summary>
        public static void ScatterOnRing(int seed, int count, float innerRadius, float outerRadius, List<Vector2> results)
        {
            if (results == null || count <= 0)
            {
                return;
            }

            float inner = Mathf.Max(0f, Mathf.Min(innerRadius, outerRadius));
            float outer = Mathf.Max(inner, Mathf.Max(innerRadius, outerRadius));
            float innerSq = inner * inner;
            float outerSq = outer * outer;
            var random = new System.Random(seed);

            // Stratify the angle so props spread around the whole ring instead of clumping.
            float slice = Mathf.PI * 2f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = (i + (float)random.NextDouble() * 0.8f + 0.1f) * slice;
                float radius = Mathf.Sqrt(Mathf.Lerp(innerSq, outerSq, (float)random.NextDouble()));
                results.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }
        }

        /// <summary>
        /// Innermost radius (from the arena centre) a procedural prop may be scattered at. Encounter
        /// enemies spawn on a 6-9 m ring around the player, who starts on the arena centre; the biggest
        /// props (rocks up to ~1.4 m radius) plus an enemy capsule need ~2 m on top of that, so props
        /// start at 11 m — with the old 9 m inner edge a golem could spawn inside a rock.
        /// </summary>
        public const float MinPropClearRadius = 11f;

        /// <summary>Scatter inner radius actually used: the profile's, but never inside <see cref="MinPropClearRadius"/>.</summary>
        public static float PropInnerRadius(float profileInnerRadius) => Mathf.Max(profileInnerRadius, MinPropClearRadius);

        /// <summary>Stable per-spawn-entry seed so two entries of one profile don't land on the same ring slots.</summary>
        public static int SeedForEntry(int encounterSeed, int entryIndex)
        {
            unchecked
            {
                return (encounterSeed * 397) ^ ((entryIndex + 1) * 7919);
            }
        }
    }
}
