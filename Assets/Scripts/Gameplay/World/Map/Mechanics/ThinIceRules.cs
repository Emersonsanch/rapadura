using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>How the ice under a heavy mount is holding (<see cref="ThinIceRules.Stage"/>).</summary>
    public enum ThinIceStage
    {
        Holding = 0,
        /// <summary>The warning: it creaks and the outline goes red, there is still time to leave or dismount.</summary>
        Creaking = 1,
        Broken = 2,
    }

    /// <summary>
    /// The thin ice of the Canais de Gelo (Docs/Mapas100.md, map 60: "montaria pesada quebra o gelo fino; o mapa mostra a
    /// espessura; de noite o gelo racha mais"), pure. Only a heavy mount loads the ice: a hero on foot or on a light mount is
    /// always safe there. The load builds while the heavy mount stands on a patch and drains off it; when the ice breaks the
    /// mount bolts (forced dismount) and the cold water bites, which can never kill: the hero is on foot on the same spot, and on
    /// foot the ice holds, so there is nothing to be stuck in.
    /// <code>
    /// creaks at 1.2 s of load · breaks at 4 s by day, 2.5 s by night (load runs × 1.6) · drains 2× as fast as it builds
    /// break: forced dismount + 8 % of max life, always leaving at least 1
    /// </code>
    /// </summary>
    public static class ThinIceRules
    {
        public const float CreakLoad = 1.2f;
        public const float BreakLoad = 4f;
        public const float NightLoadFactor = 1.6f;
        public const float DrainPerSecond = 2f;
        public const float BreakDamageFraction = 0.08f;

        /// <summary>The mounts too heavy for thin ice (the mammoth of map 60 and the two other great-bodied ones).</summary>
        private static readonly HashSet<string> HeavyMounts = new HashSet<string>
        {
            "mount_woolly_mammoth",
            "mount_elder_tortoise",
            "mount_boi_bumba",
        };

        public static bool IsHeavyMount(string mountId) => !string.IsNullOrEmpty(mountId) && HeavyMounts.Contains(mountId);

        /// <summary>The first patch containing <paramref name="p"/> (region coordinates), or −1.</summary>
        public static int PatchAt(IReadOnlyList<Rect> patches, Vector2 p)
        {
            for (int i = 0; i < patches.Count; i++)
            {
                if (patches[i].Contains(p))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The load after <paramref name="deltaTime"/> seconds, on the patch or off it.</summary>
        public static float Advance(float load, bool heavyOnThinIce, bool night, float deltaTime)
        {
            if (heavyOnThinIce)
            {
                return load + deltaTime * (night ? NightLoadFactor : 1f);
            }

            return Mathf.Max(0f, load - deltaTime * DrainPerSecond);
        }

        public static ThinIceStage Stage(float load)
        {
            if (load >= BreakLoad)
            {
                return ThinIceStage.Broken;
            }

            return load >= CreakLoad ? ThinIceStage.Creaking : ThinIceStage.Holding;
        }

        /// <summary>The cold water's bite: never enough to kill (at most current − 1, never below 0).</summary>
        public static float BreakDamage(float currentHealth, float maxHealth) => Mathf.Clamp(maxHealth * BreakDamageFraction, 0f, Mathf.Max(0f, currentHealth - 1f));
    }
}
