using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// The regional maps after 01–02, in campaign order (Docs/Mapas100.md §5), built through the same 6-step pipeline as
    /// the Pomar and the Tugai: Areias Flamejantes (<c>MapLayouts.Areias.cs</c>: 07, 08), Serra de Kaggen
    /// (<c>MapLayouts.Kaggen.cs</c>: 13, 14) and Grutas do Limiar (<c>MapLayouts.Limiar.cs</c>: 19, 20). This file holds
    /// the dispatcher used by <see cref="For"/>/<see cref="Has"/> and the scatter helpers the regional maps share
    /// (boulder walls of the passes, the rock belt of the caves, keeping the wild props off the settlement's own solids).
    /// <para>
    /// Monsters reuse today's definitions (Resources/Enemies): the Ushabti de Pedra, the Escorpião das Areias, the
    /// bat swarm, the Lobo do Tugai and the Aranha-Tecelã stand in for the sheet's new creatures until those get their
    /// own definitions and models; the night tables use the playable night species of <see cref="NightVariantRules"/>.
    /// </para>
    /// </summary>
    public static partial class MapLayouts
    {
        public const string CaravanasId = "map_007_caravanas";
        public const string ArtificesId = "map_008_artifices";
        public const string RondaveisId = "map_013_rondaveis";
        public const string HahyaId = "map_014_hahya";
        public const string LagoLimiarId = "map_019_lago_limiar";
        public const string TagesId = "map_020_tages";

        /// <summary>The regional maps this build can lay out (besides 01–02), in campaign order.</summary>
        public static readonly IReadOnlyList<string> RegionalMapIds = new[] { CaravanasId, ArtificesId, RondaveisId, HahyaId, LagoLimiarId, TagesId };

        private const string Scorpion = PeriodSpawnTable.DesertScorpion;
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Bats = PeriodSpawnTable.CaveBats;

        private static bool HasRegional(string mapId)
        {
            if (HasZarafshan(mapId))
            {
                return true;
            }

            foreach (string id in RegionalMapIds)
            {
                if (id == mapId)
                {
                    return true;
                }
            }

            return false;
        }

        private static MapLayout BuildRegional(string mapId)
        {
            switch (mapId)
            {
                case BrejoId: return Brejo();
                case AmoreirasId: return Amoreiras();
                case GarajonayId: return Garajonay();
                case MihrshahId: return Mihrshah();
                case CaravanasId: return Caravanas();
                case ArtificesId: return Artifices();
                case RondaveisId: return Rondaveis();
                case HahyaId: return Hahya();
                case LagoLimiarId: return LagoLimiar();
                case TagesId: return Tages();
                default: return null;
            }
        }

        // ------------------------------------------------------------------
        // Shared helpers of the regional maps
        // ------------------------------------------------------------------

        private static MapExitSpec Border(string id, MapExitKind kind, string target, Rect trigger, Vector2 arrival, Vector2 facing)
        {
            return new MapExitSpec { Id = id, Kind = kind, TargetMapId = target, Trigger = trigger, Arrival = arrival, ArrivalFacing = facing };
        }

        private static CityProp Piece(CityPropKind kind, float x, float z, float yaw = 0f, float scale = 1f, int variant = 0)
        {
            return new CityProp { Kind = kind, Position = new Vector2(x, z), Yaw = yaw, Scale = scale, Variant = variant };
        }

        private static CityProp Run(CityPropKind kind, float x0, float z0, float x1, float z1, float scale = 1f, int variant = 0)
        {
            return new CityProp { Kind = kind, Position = new Vector2(x0, z0), End = new Vector2(x1, z1), Scale = scale, Variant = variant };
        }

        /// <summary>A dry-stone (or adobe) pen of four wall runs with a gap on the south side (the currais of the Serra).</summary>
        private static void Pen(CityTownRecipe recipe, Vector2 center, float halfX, float halfZ, float gap)
        {
            float x0 = center.x - halfX, x1 = center.x + halfX, z0 = center.y - halfZ, z1 = center.y + halfZ;
            recipe.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, z1, x1, z1));
            recipe.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, z0, x0, z1));
            recipe.PlazaProps.Add(Run(CityPropKind.FenceRun, x1, z0, x1, z1));
            recipe.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, z0, center.x - gap * 0.5f, z0));
            recipe.PlazaProps.Add(Run(CityPropKind.FenceRun, center.x + gap * 0.5f, z0, x1, z0));
        }

        /// <summary>A wall of boulders from <paramref name="a"/> to <paramref name="b"/> (the sides of a pass, a gallery).</summary>
        private static void RockLine(MapLayout m, Vector2 a, Vector2 b, float spacing, float minScale, float maxScale, string salt)
        {
            float length = Vector2.Distance(a, b);
            if (length < 0.01f)
            {
                return;
            }

            Vector2 d = (b - a) / length;
            var n = new Vector2(-d.y, d.x);
            int i = 0;
            for (float t = 0f; t <= length; t += spacing, i++)
            {
                Vector2 p = a + d * t + n * ((TownStyle.Hash01(salt, i) - 0.5f) * spacing * 0.5f);
                if (!MapLayoutRules.IsFreeForProp(m, p, 1f))
                {
                    continue;
                }

                m.WildProps.Add(new CityProp
                {
                    Kind = CityPropKind.Rock,
                    Position = p,
                    Yaw = TownStyle.Hash01(salt, i + 300) * 360f,
                    Scale = Mathf.Lerp(minScale, maxScale, TownStyle.Hash01(salt, i + 600)),
                });
            }
        }

        /// <summary>The cave and mountain version of <see cref="Backdrop"/>: big rocks just outside the walls, gaps at the exits.</summary>
        private static void RockBackdrop(MapLayout m, string salt)
        {
            Rect b = m.Bounds;
            int i = 0;
            for (float ring = 3f; ring <= 10f; ring += 6f)
            {
                Rect r = CityGeometry.Inflate(b, ring);
                float perimeter = 2f * (r.width + r.height);
                for (float s = 0f; s < perimeter; s += 6.5f, i++)
                {
                    Vector2 p = PerimeterPoint(r, s + TownStyle.Hash01(salt, i) * 2.5f);
                    if (!MapLayoutRules.IsFreeForProp(m, p, 0.5f, backdrop: true))
                    {
                        continue;
                    }

                    m.WildProps.Add(new CityProp
                    {
                        Kind = CityPropKind.Rock,
                        Position = p,
                        Yaw = TownStyle.Hash01(salt, i + 100) * 360f,
                        Scale = 2.6f + TownStyle.Hash01(salt, i + 200) * 2.4f,
                    });
                }
            }
        }

        /// <summary>
        /// Drops the wild props that would stand on the settlement's own solids (buildings, plaza props, the landmarks the
        /// recipe places across the map: ruins, karez shafts, cliffs, pens), so a tree never grows through a pyramid.
        /// Run after the scatter: the town is generated once more from the same recipe (deterministic).
        /// </summary>
        private static void KeepOffSettlement(MapLayout m, float clearance = 1.6f)
        {
            if (m.Settlement == null)
            {
                return;
            }

            CityLayout town = CityLayoutGenerator.Generate(m.Settlement);
            town.WalkBounds = m.Bounds;
            town.Invalidate();
            IReadOnlyList<CitySolid> solids = town.Solids;
            m.WildProps.RemoveAll(prop =>
            {
                float radius = clearance + (prop.Kind == CityPropKind.Rock ? 0.5f * Mathf.Max(1f, prop.Scale) : 0.6f);
                foreach (CitySolid solid in solids)
                {
                    if (solid.DistanceTo(prop.Position) < radius)
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        /// <summary>Adds every zone centre as a clearing (darker grass disc) — only where the ground is grass.</summary>
        private static void ClearingsFromZones(MapLayout m)
        {
            foreach (MapSpawnZone zone in m.Zones)
            {
                m.Clearings.Add(zone.Center);
            }
        }
    }
}
