using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// The accent colour of each of the 16 <see cref="MapRegion"/>s in the Barcas de Luz window (header stripe, filter chip
    /// swatch). One entry per region, in catalog order; a region added later without a colour falls back to
    /// <see cref="Fallback"/> (and the EditMode test fails, so it gets its own).
    /// </summary>
    public static class MapRegionColors
    {
        public static readonly Color Fallback = new Color32(200, 170, 120, 255);

        public static bool HasColor(MapRegion region) => TryGet(region, out _);

        public static Color Of(MapRegion region) => TryGet(region, out Color color) ? color : Fallback;

        private static bool TryGet(MapRegion region, out Color color)
        {
            switch (region)
            {
                case MapRegion.Zarafshan: color = new Color32(104, 168, 76, 255); return true;
                case MapRegion.AreiasFlamejantes: color = new Color32(226, 150, 58, 255); return true;
                case MapRegion.SerraDeKaggen: color = new Color32(150, 124, 98, 255); return true;
                case MapRegion.GrutasDoLimiar: color = new Color32(122, 104, 168, 255); return true;
                case MapRegion.CostaDasMoncoes: color = new Color32(60, 170, 176, 255); return true;
                case MapRegion.DeltaDeJhilpur: color = new Color32(70, 140, 96, 255); return true;
                case MapRegion.TetoDoMundo: color = new Color32(196, 214, 230, 255); return true;
                case MapRegion.PlanicieDeMingcheng: color = new Color32(214, 74, 60, 255); return true;
                case MapRegion.EstepeDosNartes: color = new Color32(196, 180, 92, 255); return true;
                case MapRegion.PicosDeFarnkesh: color = new Color32(132, 150, 176, 255); return true;
                case MapRegion.MarGelado: color = new Color32(120, 196, 238, 255); return true;
                case MapRegion.ArquipelagoDeHinokura: color = new Color32(230, 120, 150, 255); return true;
                case MapRegion.RioDasDozeHoras: color = new Color32(70, 110, 200, 255); return true;
                case MapRegion.MargensDaMistura: color = new Color32(150, 92, 70, 255); return true;
                case MapRegion.LimiarProfundo: color = new Color32(96, 70, 120, 255); return true;
                case MapRegion.ReinosDoCeu: color = new Color32(250, 220, 120, 255); return true;
                default: color = Fallback; return false;
            }
        }
    }
}
