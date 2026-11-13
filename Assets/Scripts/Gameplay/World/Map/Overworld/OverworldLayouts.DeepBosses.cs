using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The arenas of the map bosses of the other pits of the Limiar Profundo and of the bridge (Docs/Mapas100.md 84, 85, 87, 88, 89; RegionBossRules.MapSpawns):
    /// a stone ring each, at the edge of a trail of its own area (the ring's plateau keeps the road level), with no clearing on it. 86 (A Águia) is in
    /// OverworldLayouts.Profundo.cs.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>O Rei da Fumaça's arena (84): the stone ring by the north road through the Corredores de Cinza, between two banks of smoke.</summary>
        public static readonly Vector2 ReiDaFumacaArena = new Vector2(604f, 480f);

        /// <summary>A Leoa do Fogo Devorador's arena (85): the stone ring by the south row's road through the Salões de Cinza Quente.</summary>
        public static readonly Vector2 LeoaDoFogoArena = new Vector2(480f, -964f);

        /// <summary>O Peixe de Cauda Negra's arena (87): the stone ring by the road south of the Lâmpada 4, in the Cova do Lodo, off the pools of black water.</summary>
        public static readonly Vector2 PeixeDeCaudaNegraArena = new Vector2(1084f, -140f);

        /// <summary>O Dragão do Fundo's arena (88): the stone ring by the road west of the Lâmpada 5, in the labyrinth of the Cova da Treva.</summary>
        public static readonly Vector2 DragaoDoFundoArena = new Vector2(950f, -964f);

        /// <summary>O Guardião do Novelo's arena (89): the stone ring on the crest of the causeway in the Lances do Novelo, the dark globe to the east.</summary>
        public static readonly Vector2 GuardiaoDoNoveloArena = new Vector2(1564f, 100f);

        private static readonly Vector2[] DeepBossArenas =
        {
            ReiDaFumacaArena, LeoaDoFogoArena, PeixeDeCaudaNegraArena, DragaoDoFundoArena, GuardiaoDoNoveloArena,
        };

        /// <summary>The stone rings of the five arenas (the relief levels a plateau under each).</summary>
        private static void AddDeepBossRings(OverworldLayout w)
        {
            foreach (Vector2 arena in DeepBossArenas)
            {
                Landmark(w, MapLandmarkKind.StoneRing, arena.x, arena.y, 0f, 1.1f);
            }
        }

        /// <summary>No clearing on the five arenas (after the clearings are generated): a boss fight is not a spawn point.</summary>
        private static void KeepClearingsOffDeepBossArenas(OverworldLayout w)
        {
            foreach (Vector2 arena in DeepBossArenas)
            {
                KeepClearingsOff(w, arena, 10f);
            }
        }
    }
}
