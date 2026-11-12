using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum NamraelGlarePhase
    {
        None = 0,

        /// <summary>The salt starts to shine: half an hour to reach a tower's shade.</summary>
        Rising = 1,

        /// <summary>Noon on the salt: past 10 m the hero sees nothing.</summary>
        Glare = 2,
    }

    /// <summary>
    /// The blinding salt of the Trono de Namrael (Docs/Mapas100.md 12: "sal que ofusca: ao meio-dia o sal brilha e corta a visão a 10 m (aqui o
    /// dia é o perigoso)"), pure and server-runnable. From 11:00 to 14:00 of the game clock the salt terraces shine: on the open salt the hero
    /// sees no beast farther than 10 m (they are still there, and the archers still shoot). From 10:30 the salt starts to shine (the warning).
    /// The shade of the rammed-earth towers of Qocho's sentinels (14 m round each) and the Torre de Taipa post are safe; bosses always show.
    /// <code>
    /// 10:30–11:00 rising (warning) · 11:00–14:00 glare: beasts past 10 m unseen on the open salt · shade: 14 m round a taipa tower, the post
    /// </code>
    /// </summary>
    public static class NamraelGlareRules
    {
        public const float RisingHour = 10.5f;
        public const float GlareStartHour = 11f;
        public const float GlareEndHour = 14f;
        public const float SightRange = 10f;
        public const float ShadeRadius = 14f;

        public static NamraelGlarePhase PhaseAt(float hour)
        {
            hour = Mathf.Repeat(hour, 24f);
            return hour >= GlareStartHour && hour < GlareEndHour ? NamraelGlarePhase.Glare
                : hour >= RisingHour && hour < GlareStartHour ? NamraelGlarePhase.Rising
                : NamraelGlarePhase.None;
        }

        /// <summary>Whether <paramref name="p"/> is in the shade of one of the <paramref name="towers"/>.</summary>
        public static bool InShade(IReadOnlyList<Vector2> towers, Vector2 p)
        {
            for (int i = 0; towers != null && i < towers.Count; i++)
            {
                if ((towers[i] - p).sqrMagnitude <= ShadeRadius * ShadeRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether the hero is blinded: the glare is on and the hero stands on the open salt (not in shade, not on the post).</summary>
        public static bool Blinded(NamraelGlarePhase phase, bool onSalt, bool shaded, bool onPost) => phase == NamraelGlarePhase.Glare && onSalt && !shaded && !onPost;

        /// <summary>Whether a beast <paramref name="distance"/> metres from a blinded hero is lost in the glare.</summary>
        public static bool Hidden(bool blinded, bool isBoss, float distance) => blinded && !isBoss && distance > SightRange;
    }
}
