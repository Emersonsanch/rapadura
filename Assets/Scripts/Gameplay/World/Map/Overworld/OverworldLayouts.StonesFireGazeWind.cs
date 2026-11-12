using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of four mechanics on the main road, each in its region's own coordinates: the waking stones of the Campo das Pedras-Dragão
    /// (21, <c>VishapStoneRules</c>, Limiar), the strips of the low fire of the Savana dos Auroques (26, <c>SavannaFireRules</c>, Costa), the
    /// Charco do Ninki Nanka where the calves shine (34, <c>NinkiGazeRules</c>, Delta) and the wind vanes of the Forte dos Ventos (39,
    /// <c>WindVaneRules</c>, Teto). The bounds are the maps' zones.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The Campo das Pedras-Dragão (21): its zone.</summary>
        public static readonly Rect DragonstoneFieldBounds = Rect.MinMaxRect(-700f, -1700f, -300f, -1250f);

        /// <summary>
        /// The twelve vishapakar of the field (21), where the layout raises them: on a ring of 120–150 m round the outpost (squashed to 0.9
        /// north–south), the same angles and radii. Built on first use (the outpost lives in another part of this partial class).
        /// </summary>
        public static Vector2[] VishapStones
        {
            get
            {
                if (_vishapStones == null)
                {
                    var stones = new Vector2[12];
                    for (int i = 0; i < stones.Length; i++)
                    {
                        float a = i * Mathf.PI * 2f / 12f + 0.2f;
                        float r = 120f + 30f * TownStyle.Hash01("vishapakar", i);
                        stones[i] = PedrasDragaoPost + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.9f) * r;
                    }

                    _vishapStones = stones;
                }

                return _vishapStones;
            }
        }

        private static Vector2[] _vishapStones;

        /// <summary>The Savana dos Auroques (26): its zone.</summary>
        public static readonly Rect SavannaFireBounds = Rect.MinMaxRect(-600f, 700f, -150f, 1200f);

        /// <summary>The four strips of old grass the bull-folk burn in turn (26), north–south, 40 m wide, two each side of Aratta's ground.</summary>
        public static readonly Rect[] SavannaFireStrips =
        {
            Rect.MinMaxRect(-580f, 730f, -540f, 1170f),
            Rect.MinMaxRect(-500f, 730f, -460f, 1170f),
            Rect.MinMaxRect(-300f, 730f, -260f, 1170f),
            Rect.MinMaxRect(-220f, 730f, -180f, 1170f),
        };

        /// <summary>The Charco do Ninki Nanka (34): its three zones (the bog, the lantern islet and the dragon's den).</summary>
        public static readonly Rect NinkiBogBounds = Rect.MinMaxRect(-240f, -1800f, 1200f, -1320f);

        /// <summary>The beast that stands in for the Ninki Nanka calf in the bog (the mangrove crocodile, [elite]).</summary>
        public const string NinkiCalfEnemyId = "enemy_forest_mangrove_crocodile";

        /// <summary>The Desfiladeiro and the Forte dos Ventos (39): their zones, where the vanes' wind blows.</summary>
        public static readonly Rect WindFortBounds = Rect.MinMaxRect(240f, 840f, 1200f, 1320f);

        /// <summary>The four wind vanes round the fort (39): north and south on its trails, west by the gorge trail, east on the rock.</summary>
        public static readonly Vector2[] WindVanes =
        {
            new Vector2(986f, 1240f), new Vector2(1100f, 1080f), new Vector2(986f, 930f), new Vector2(780f, 1086f),
        };
    }
}
