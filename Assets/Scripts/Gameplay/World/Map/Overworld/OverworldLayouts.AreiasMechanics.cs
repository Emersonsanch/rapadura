using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the Areias Flamejantes sandstorm (<c>AreiasStormRules</c>), in the region's own coordinates: the shelters are the
    /// region's camps, wrecks, campfires and rock arches, so the hero can tell them at a glance; the free zones are never hit (the
    /// oasis, the palms and the galleries under the sand).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string AreiasAnchorZoneId = "oasis_de_qocho";

        public static readonly Rect AreiasBounds = Rect.MinMaxRect(-1200f, -1900f, 1200f, 500f);

        public static readonly HashSet<string> AreiasStormFreeZones = new HashSet<string>
        {
            "oasis_de_qocho", "palmeiral_do_karez", "oasis_das_tamargueiras", "galerias_do_karez",
        };

        public static readonly Vector2[] AreiasStormShelters =
        {
            // Resting caravans.
            new Vector2(-340f, 400f), new Vector2(-60f, -640f), new Vector2(330f, -110f), new Vector2(230f, -1440f), new Vector2(-860f, -1080f),
            // Broken caravans.
            new Vector2(-450f, 250f), new Vector2(-610f, 110f), new Vector2(-1000f, -100f), new Vector2(-1080f, -330f), new Vector2(-880f, -980f),
            new Vector2(-1050f, -1200f), new Vector2(-620f, -1160f), new Vector2(-480f, -1000f), new Vector2(620f, 380f), new Vector2(980f, -1050f),
            // Campfires and stone rings.
            new Vector2(-318f, 445f), new Vector2(30f, -640f), new Vector2(1170f, 270f), new Vector2(-980f, 160f), new Vector2(820f, -1100f),
            new Vector2(-560f, -820f), new Vector2(960f, -560f), new Vector2(-272f, -36f), new Vector2(-590f, 318f),
            // Rock arches.
            new Vector2(880f, 120f), new Vector2(1080f, 400f), new Vector2(860f, -820f), new Vector2(-1000f, -1520f), new Vector2(-620f, -1420f), new Vector2(1100f, -300f),
        };
    }
}
