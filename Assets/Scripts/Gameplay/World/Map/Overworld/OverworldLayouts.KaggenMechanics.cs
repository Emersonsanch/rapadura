using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of the Serra de Kaggen winds (<c>KaggenWindRules</c>), in the region's own coordinates: the open slopes the gusts sweep, and the rope bridges.</summary>
    public static partial class OverworldLayouts
    {
        public const string KaggenAnchorZoneId = "trilha_dos_rondaveis";

        public static readonly Rect KaggenBounds = Rect.MinMaxRect(-1700f, -900f, 700f, 1500f);

        /// <summary>The Encosta do Vento and the Passo do Hahya.</summary>
        public static readonly Rect[] KaggenWindAreas = { Rect.MinMaxRect(250f, -900f, 700f, 250f), Rect.MinMaxRect(250f, 250f, 700f, 700f) };

        /// <summary>The Qacha's three bridges, the Zarafshan road's bridge and the two of the Passo do Hahya (the region's bridge crossings).</summary>
        public static readonly Vector2[] KaggenBridges =
        {
            new Vector2(-400f, 125f), new Vector2(-400f, -350f), new Vector2(-392f, 558f), new Vector2(608.33f, -100f), new Vector2(515f, 498f), new Vector2(544f, 466f),
        };
    }
}
