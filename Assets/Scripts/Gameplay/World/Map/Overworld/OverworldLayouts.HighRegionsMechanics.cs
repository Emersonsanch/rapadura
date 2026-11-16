using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Places of the 2026-11-16 mechanics of Regiões 10–12 (Docs/Mapas100.md 57, 58, 61, 66 and 69), in each region's own coordinates:
    /// the shelter stones of the Garganta dos Ays (<c>GorgeHeadwindRules</c>), the tundra of Omöl that follows the season
    /// (<c>TundraSeasonRules</c>), the three raked-gravel gardens and the hidden tea house of the Jardins de Lava Laqueada
    /// (<c>HinokuraGravelRules</c>) and the ring of the nine furnaces (<c>NineFurnacesPuzzle</c>). No geometry: the drivers draw
    /// light beacons on the ground, so nothing here changes the terrain, the paths or the clearings.
    /// </summary>
    public static partial class OverworldLayouts
    {
        // ---------------- 57 · Garganta dos Ays (Farnkesh coordinates) ----------------

        /// <summary>
        /// The pedras-abrigo of the gorge: three beside the trail from the Gruta Abrigo westwards (4 m off it, on its north side) and two out on
        /// the open gorge floor. Behind one (within <c>GorgeHeadwindRules.ShelterRadius</c>) the headwind does not slow anyone.
        /// </summary>
        public static readonly Vector2[] GorgeShelterStones =
        {
            new Vector2(-799f, 1611f), new Vector2(-899f, 1627f), new Vector2(-999f, 1644f), new Vector2(-1080f, 1540f), new Vector2(-880f, 1440f),
        };

        // ---------------- 61 · Tundra de Omöl (Mar Gelado coordinates) ----------------

        /// <summary>The three areas of map 61 (Tundra do Sul, Tundra de Omöl, Lagos de Omöl): where the season changes the ground.</summary>
        public static readonly Rect TundraSeasonArea = Rect.MinMaxRect(-1200f, -600f, -720f, 840f);

        // ---------------- 66 · Jardins de Lava Laqueada (Hinokura coordinates) ----------------

        /// <summary>
        /// The three raked-gravel gardens round the Casa de Chá de Pedra, in the order of <c>HinokuraGravelRules.Neighbours</c>: under the
        /// cloud pines on the west road, by the garden pond north-west of the tea house, by the stone ring east of the garden road.
        /// </summary>
        public static readonly Vector2[] GravelGardens =
        {
            new Vector2(-280f, 640f), new Vector2(-110f, 700f), new Vector2(150f, 545f),
        };

        /// <summary>The hidden tea house: the moss terrace north-west of the Casa de Chá, clear of the post's safe ground.</summary>
        public static readonly Vector2 HiddenTeaHouse = new Vector2(-40f, 650f);

        // ---------------- 69 · Castelo das Nove Fornalhas (Hinokura coordinates) ----------------

        /// <summary>The centre of the ring of the nine furnaces in the Pátio das Fornalhas, west of the court road.</summary>
        public static readonly Vector2 NineFurnacesCenter = new Vector2(-1430f, 600f);

        /// <summary>Radius (m) of the ring of the nine furnaces.</summary>
        public const float NineFurnacesRadius = 24f;

        /// <summary>The bellows of furnace <paramref name="index"/> (0–8, clockwise from the north as seen from above).</summary>
        public static Vector2 NineFurnace(int index)
        {
            float a = index * Mathf.PI * 2f / 9f;
            return NineFurnacesCenter + new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * NineFurnacesRadius;
        }
    }
}
