using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 16 — Reinos do Céu: the sky's pieces of the chunk builder. Before the noise scatter, each chunk gets the fixed pieces of the sheets that stand in it (the two masts, the twelve
    /// jars of the Celestial Wheel, the seven spheres of the Archons, the Column of Glory, the five cracks round the Lookout, Vô Jorge's rocking chair), then the big pieces of its areas,
    /// then the ground detail of its own; the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Reinos do Céu terrain (Região 16, 310–349).</summary>
        public static bool IsCeu(OverworldTerrain t) => OverworldTerrains.IsCeu(t);

        private static bool IsCeuDecor(OverworldDecor decor) => OverworldDecorKit.IsCeuDecor(decor);

        /// <summary>Room (m) a scattered Reinos do Céu piece keeps from the others.</summary>
        private static float CeuClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.CloudBank: return 2.4f;
                case OverworldDecor.LightBollard: return 0.9f;
                case OverworldDecor.LightBuoy: return 1.2f;
                case OverworldDecor.CloudIslet: return 2.6f;
                case OverworldDecor.MehenRail: return 2f;
                case OverworldDecor.DeckCrate: return 1.1f;
                case OverworldDecor.MoonMast: return 2.5f;
                case OverworldDecor.CelestialJar: return 3.8f;
                case OverworldDecor.BronzeGear: return 1.9f;
                case OverworldDecor.ArchonSphere: return 7.5f;
                case OverworldDecor.OrbitShard: return 1.2f;
                case OverworldDecor.LightTree: return 1.8f;
                case OverworldDecor.SongFlower: return 0.8f;
                case OverworldDecor.GardenFount: return 1.6f;
                case OverworldDecor.SunSail: return 3f;
                case OverworldDecor.SunCoil: return 1f;
                case OverworldDecor.CloudSteps: return 2f;
                case OverworldDecor.GloryColumn: return 8f;
                case OverworldDecor.SkyCrack: return 1.4f;
                case OverworldDecor.HoldPillar: return 1f;
                case OverworldDecor.PorchBench: return 1.2f;
                case OverworldDecor.RockingChair: return 1f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] CeuPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.QuayRimWest: return new[] { new BigPiece(OverworldDecor.CloudBank, 0.4f, 2.4f, false), new BigPiece(OverworldDecor.LightBollard, 0.2f, 0.9f, false) };
                case OverworldTerrain.MoonPierApproach: return new[] { new BigPiece(OverworldDecor.LightBollard, 0.45f, 0.9f, false), new BigPiece(OverworldDecor.CloudBank, 0.2f, 2.4f, false) };
                case OverworldTerrain.ColumnQuayGround: return new[] { new BigPiece(OverworldDecor.LightBollard, 0.35f, 0.9f, false), new BigPiece(OverworldDecor.CloudBank, 0.3f, 2.4f, false) };
                case OverworldTerrain.SunPierApproach: return new[] { new BigPiece(OverworldDecor.LightBollard, 0.45f, 0.9f, false) };
                case OverworldTerrain.CityLightsGap: return new[] { new BigPiece(OverworldDecor.CloudIslet, 0.45f, 2.6f, false), new BigPiece(OverworldDecor.CloudBank, 0.2f, 2.4f, false) };
                case OverworldTerrain.RiverMouthWest: return new[] { new BigPiece(OverworldDecor.LightBuoy, 0.4f, 1.2f, false), new BigPiece(OverworldDecor.CloudIslet, 0.2f, 2.6f, false) };
                case OverworldTerrain.CloudIslets: return new[] { new BigPiece(OverworldDecor.CloudIslet, 0.6f, 2.6f, false) };
                case OverworldTerrain.LightRiver: return new[] { new BigPiece(OverworldDecor.LightBuoy, 0.55f, 1.2f, false) };
                case OverworldTerrain.CurrentBanks: return new[] { new BigPiece(OverworldDecor.LightBuoy, 0.3f, 1.2f, false), new BigPiece(OverworldDecor.LightBollard, 0.25f, 0.9f, false) };
                case OverworldTerrain.RiverMouthEast: return new[] { new BigPiece(OverworldDecor.CloudBank, 0.35f, 2.4f, false), new BigPiece(OverworldDecor.CloudIslet, 0.25f, 2.6f, false) };
                case OverworldTerrain.MoonProw: return new[] { new BigPiece(OverworldDecor.MehenRail, 0.5f, 2f, true), new BigPiece(OverworldDecor.DeckCrate, 0.2f, 1.1f, true) };
                case OverworldTerrain.MoonMastDeck: return new[] { new BigPiece(OverworldDecor.MehenRail, 0.6f, 2f, true) };
                case OverworldTerrain.MoonDeck: return new[] { new BigPiece(OverworldDecor.DeckCrate, 0.45f, 1.1f, true), new BigPiece(OverworldDecor.MehenRail, 0.15f, 2f, true) };
                case OverworldTerrain.MoonHold: return new[] { new BigPiece(OverworldDecor.DeckCrate, 0.65f, 1.1f, true) };
                case OverworldTerrain.MoonStern: return new[] { new BigPiece(OverworldDecor.MehenRail, 0.45f, 2f, true), new BigPiece(OverworldDecor.LightBollard, 0.2f, 0.9f, false) };
                case OverworldTerrain.GearDrift: return new[] { new BigPiece(OverworldDecor.BronzeGear, 0.55f, 1.9f, true), new BigPiece(OverworldDecor.CloudBank, 0.15f, 2.4f, false) };
                case OverworldTerrain.JarRimWest: return new[] { new BigPiece(OverworldDecor.BronzeGear, 0.4f, 1.9f, true) };
                case OverworldTerrain.WheelHub: return new[] { new BigPiece(OverworldDecor.BronzeGear, 0.3f, 1.9f, true) };
                case OverworldTerrain.JarRimEast: return new[] { new BigPiece(OverworldDecor.BronzeGear, 0.4f, 1.9f, true), new BigPiece(OverworldDecor.LightBollard, 0.15f, 0.9f, false) };
                case OverworldTerrain.GearDriftEast: return new[] { new BigPiece(OverworldDecor.BronzeGear, 0.5f, 1.9f, true), new BigPiece(OverworldDecor.CloudIslet, 0.2f, 2.6f, false) };
                case OverworldTerrain.OrbitVoidWest: return new[] { new BigPiece(OverworldDecor.OrbitShard, 0.45f, 1.2f, false), new BigPiece(OverworldDecor.CloudIslet, 0.2f, 2.6f, false) };
                case OverworldTerrain.OrbitRidgeWest: return new[] { new BigPiece(OverworldDecor.OrbitShard, 0.5f, 1.2f, false) };
                case OverworldTerrain.StillIslet: return new[] { new BigPiece(OverworldDecor.OrbitShard, 0.3f, 1.2f, false), new BigPiece(OverworldDecor.CloudBank, 0.2f, 2.4f, false) };
                case OverworldTerrain.OrbitRidgeEast: return new[] { new BigPiece(OverworldDecor.OrbitShard, 0.5f, 1.2f, false), new BigPiece(OverworldDecor.CloudIslet, 0.1f, 2.6f, false) };
                case OverworldTerrain.OrbitVoidEast: return new[] { new BigPiece(OverworldDecor.OrbitShard, 0.45f, 1.2f, false), new BigPiece(OverworldDecor.CloudBank, 0.2f, 2.4f, false) };
                case OverworldTerrain.FlowerGarden: return new[] { new BigPiece(OverworldDecor.SongFlower, 0.55f, 0.8f, false), new BigPiece(OverworldDecor.LightTree, 0.2f, 1.8f, false) };
                case OverworldTerrain.LightOrchard: return new[] { new BigPiece(OverworldDecor.LightTree, 0.55f, 1.8f, false), new BigPiece(OverworldDecor.GardenFount, 0.1f, 1.6f, true) };
                case OverworldTerrain.GardenPavilion: return new[] { new BigPiece(OverworldDecor.GardenFount, 0.35f, 1.6f, true), new BigPiece(OverworldDecor.LightTree, 0.2f, 1.8f, false) };
                case OverworldTerrain.SunDeck: return new[] { new BigPiece(OverworldDecor.SunCoil, 0.35f, 1f, true), new BigPiece(OverworldDecor.DeckCrate, 0.2f, 1.1f, true) };
                case OverworldTerrain.SunSailDeck: return new[] { new BigPiece(OverworldDecor.SunCoil, 0.5f, 1f, true) };
                case OverworldTerrain.ColumnStepsWest: return new[] { new BigPiece(OverworldDecor.CloudSteps, 0.45f, 2f, false), new BigPiece(OverworldDecor.HoldPillar, 0.15f, 1f, false) };
                case OverworldTerrain.ColumnFoot: return new[] { new BigPiece(OverworldDecor.CloudSteps, 0.55f, 2f, false) };
                case OverworldTerrain.SkyCracksWest: return new[] { new BigPiece(OverworldDecor.HoldPillar, 0.45f, 1f, false), new BigPiece(OverworldDecor.CloudBank, 0.1f, 2.4f, false) };
                case OverworldTerrain.SkyHolderLookout: return new[] { new BigPiece(OverworldDecor.HoldPillar, 0.35f, 1f, false), new BigPiece(OverworldDecor.CloudIslet, 0.2f, 2.6f, false) };
                case OverworldTerrain.TimeAfterPorch: return new[] { new BigPiece(OverworldDecor.PorchBench, 0.3f, 1.2f, true), new BigPiece(OverworldDecor.LightTree, 0.35f, 1.8f, false) };
                default: return NoPieces;
            }
        }

        /// <summary>The ground details a terrain strews: its map's mark and one or two more, so no two areas look alike (Dressing test).</summary>
        private static OverworldDecor[] CeuDetailsOf(OverworldTerrain terrain)
        {
            const OverworldDecor a = OverworldDecor.CloudWisp;
            const OverworldDecor b = OverworldDecor.GoldMote;
            const OverworldDecor c = OverworldDecor.LilacMote;
            switch (terrain)
            {
                // 91: specks of solid light.
                case OverworldTerrain.QuayRimWest: return new[] { OverworldDecor.LightSpeck, a };
                case OverworldTerrain.MoonPierApproach: return new[] { OverworldDecor.LightSpeck, c };
                case OverworldTerrain.ColumnQuayGround: return new[] { OverworldDecor.LightSpeck, b };
                case OverworldTerrain.SunPierApproach: return new[] { OverworldDecor.LightSpeck, a, b };
                case OverworldTerrain.CityLightsGap: return new[] { OverworldDecor.LightSpeck, a, c };
                // 92: ripples of gold.
                case OverworldTerrain.RiverMouthWest: return new[] { OverworldDecor.GoldRipple, a };
                case OverworldTerrain.CloudIslets: return new[] { OverworldDecor.GoldRipple, b };
                case OverworldTerrain.LightRiver: return new[] { OverworldDecor.GoldRipple, c };
                case OverworldTerrain.CurrentBanks: return new[] { OverworldDecor.GoldRipple, a, b };
                case OverworldTerrain.RiverMouthEast: return new[] { OverworldDecor.GoldRipple, a, c };
                // 93: planks of the moon deck.
                case OverworldTerrain.MoonProw: return new[] { OverworldDecor.LilacPlank, a };
                case OverworldTerrain.MoonMastDeck: return new[] { OverworldDecor.LilacPlank, b };
                case OverworldTerrain.MoonDeck: return new[] { OverworldDecor.LilacPlank, c };
                case OverworldTerrain.MoonHold: return new[] { OverworldDecor.LilacPlank, a, b };
                case OverworldTerrain.MoonStern: return new[] { OverworldDecor.LilacPlank, a, c };
                // 94: filings of bronze.
                case OverworldTerrain.GearDrift: return new[] { OverworldDecor.BronzeFiling, a };
                case OverworldTerrain.JarRimWest: return new[] { OverworldDecor.BronzeFiling, b };
                case OverworldTerrain.WheelHub: return new[] { OverworldDecor.BronzeFiling, c };
                case OverworldTerrain.JarRimEast: return new[] { OverworldDecor.BronzeFiling, a, b };
                case OverworldTerrain.GearDriftEast: return new[] { OverworldDecor.BronzeFiling, a, c };
                // 95: chips of orbit crystal.
                case OverworldTerrain.OrbitVoidWest: return new[] { OverworldDecor.CrystalChip, a };
                case OverworldTerrain.OrbitRidgeWest: return new[] { OverworldDecor.CrystalChip, b };
                case OverworldTerrain.StillIslet: return new[] { OverworldDecor.CrystalChip, c };
                case OverworldTerrain.OrbitRidgeEast: return new[] { OverworldDecor.CrystalChip, a, b };
                case OverworldTerrain.OrbitVoidEast: return new[] { OverworldDecor.CrystalChip, a, c };
                // 96: petals of light.
                case OverworldTerrain.FlowerGarden: return new[] { OverworldDecor.LightPetal, a };
                case OverworldTerrain.LightOrchard: return new[] { OverworldDecor.LightPetal, b };
                case OverworldTerrain.GardenPavilion: return new[] { OverworldDecor.LightPetal, c };
                // 97: grains of sun.
                case OverworldTerrain.SunDeck: return new[] { OverworldDecor.SunGrain, a };
                case OverworldTerrain.SunSailDeck: return new[] { OverworldDecor.SunGrain, b };
                // 98: the edges of the steps.
                case OverworldTerrain.ColumnStepsWest: return new[] { OverworldDecor.StepMark, a };
                case OverworldTerrain.ColumnFoot: return new[] { OverworldDecor.StepMark, b };
                // 99: thin cracks.
                case OverworldTerrain.SkyCracksWest: return new[] { OverworldDecor.SkyCrackLine, a };
                case OverworldTerrain.SkyHolderLookout: return new[] { OverworldDecor.SkyCrackLine, b };
                // 100: apricot dust from the adobe.
                case OverworldTerrain.TimeAfterPorch: return new[] { OverworldDecor.ApricotDust, c };
                default: return null;
            }
        }

        private static void ScatterCeu(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsCeu(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "ceu" + content.Coord.x + "_" + content.Coord.y;
            PlaceCeuFixedPieces(layout, content);
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsCeu(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = CeuPiecesOf(terrain);
                float roll = TownStyle.Hash01(salt, k + 200);
                float sum = 0f;
                foreach (BigPiece piece in pieces)
                {
                    sum += piece.Chance;
                    if (roll >= sum)
                    {
                        continue;
                    }

                    float scale = 0.85f + 0.35f * TownStyle.Hash01(salt, k + 300);
                    float radius = piece.Radius * scale;
                    if ((!piece.NeedsFlat || content.Grid.Flatness(p) > 0.95f) && IsFree(layout, content, p, radius + 1f))
                    {
                        AddCeuProp(layout, content, piece.Decor, p, TownStyle.Hash01(salt, k + 400) * 360f, scale, (int)(TownStyle.Hash(salt, k + 500) & 7), radius, terrain);
                    }

                    break;
                }
            }

            ScatterCeuDetail(layout, content, salt);
        }

        /// <summary>The fixed pieces of the sheets that fall in this chunk.</summary>
        private static void PlaceCeuFixedPieces(OverworldLayout layout, OverworldChunkContent content)
        {
            Vector2 offset = layout.PartAt(content.Rect.center)?.Offset ?? Vector2.zero;
            for (int i = 0; i < OverworldLayouts.CelestialJarCount; i++)
            {
                TryFixedCeu(layout, content, OverworldDecor.CelestialJar, OverworldLayouts.CelestialJarPosition(i) + offset, i * 30f, 1.1f, i, 3.4f);
            }

            for (int i = 0; i < OverworldLayouts.ArchonSphereCount; i++)
            {
                TryFixedCeu(layout, content, OverworldDecor.ArchonSphere, OverworldLayouts.ArchonSpherePosition(i) + offset, 0f, 1f, i, 7f);
            }

            for (int i = 0; i < OverworldLayouts.SkyCrackCount; i++)
            {
                TryFixedCeu(layout, content, OverworldDecor.SkyCrack, OverworldLayouts.SkyCrackPosition(i) + offset, 72f * i, 1.1f, i, 1.6f);
            }

            TryFixedCeu(layout, content, OverworldDecor.MoonMast, OverworldLayouts.MoonMastCenter + offset, 0f, 1f, 0, 2.5f);
            TryFixedCeu(layout, content, OverworldDecor.SunSail, OverworldLayouts.SunMastCenter + offset, 30f, 1f, 0, 3f);
            TryFixedCeu(layout, content, OverworldDecor.GloryColumn, OverworldLayouts.GloryColumnCenter + offset, 0f, 1f, 0, 8f);
            TryFixedCeu(layout, content, OverworldDecor.RockingChair, OverworldLayouts.RockingChairPosition + offset, 200f, 1f, 0, 1.2f);
        }

        private static void TryFixedCeu(OverworldLayout layout, OverworldChunkContent content, OverworldDecor decor, Vector2 p, float yaw, float scale, int variant, float radius)
        {
            if (!content.Rect.Contains(p))
            {
                return;
            }

            OverworldTerrain terrain = content.Grid.Near(p).Terrain;
            if (!IsCeu(terrain) || DistanceToWall(layout, p) < EdgeBelt || !IsFree(layout, content, p, radius + 0.5f))
            {
                return;
            }

            AddCeuProp(layout, content, decor, p, yaw, scale, variant, radius, terrain);
        }

        private static void AddCeuProp(OverworldLayout layout, OverworldChunkContent content, OverworldDecor decor, Vector2 p, float yaw, float scale, int variant, float radius, OverworldTerrain terrain)
        {
            var prop = new OverworldProp { Kind = CityPropKind.Rock, Decor = decor, Position = p, Yaw = yaw, Scale = scale, Variant = variant, Color = RockColor(terrain) };
            content.Props.Add(prop);
            AddSolid(layout, content, prop);
            content.Blocked.Add(new Rect(p.x - radius, p.y - radius, radius * 2f, radius * 2f));
        }

        /// <summary>Ground detail: each terrain strews its own set (no colliders).</summary>
        private static void ScatterCeuDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsCeu(info.Terrain))
                {
                    continue;
                }

                OverworldDecor[] details = CeuDetailsOf(info.Terrain);
                if (details == null || details.Length == 0 || TownStyle.Hash01(salt, k + 22000) >= 0.4f || info.Clearing < 0.5f || DistanceToWall(layout, p) < -10f)
                {
                    continue;
                }

                OverworldDecor decor = details[k % details.Length];
                float room = decor == OverworldDecor.GoldRipple || decor == OverworldDecor.StepMark ? 1.8f : 1.1f;
                if (!IsOpen(layout, content, p, room, false))
                {
                    continue;
                }

                content.Details.Add(new OverworldProp
                {
                    Decor = decor, Position = p, Yaw = TownStyle.Hash01(salt, k + 23000) * 360f, Scale = 0.8f + 0.5f * TownStyle.Hash01(salt, k + 24000), Variant = k,
                });
            }
        }

        // ------------------------------------------------------------------
        // Colliders and meshes
        // ------------------------------------------------------------------

        private static bool AddCeuDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsCeuDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            float radius;
            float height;
            switch (prop.Decor)
            {
                case OverworldDecor.CloudBank: radius = 2.2f; height = 1.9f; break;
                case OverworldDecor.LightBollard: radius = 0.45f; height = 1.7f; break;
                case OverworldDecor.LightBuoy: radius = 0.75f; height = 2.6f; break;
                case OverworldDecor.CloudIslet: radius = 2.4f; height = 1.2f; break;
                case OverworldDecor.DeckCrate: radius = 0.8f; height = 1f; break;
                case OverworldDecor.MoonMast: radius = 0.75f; height = 13f; break;
                case OverworldDecor.CelestialJar: radius = 3.1f; height = 4.2f; break;
                case OverworldDecor.BronzeGear: radius = 1.5f; height = 0.5f; break;
                case OverworldDecor.ArchonSphere: radius = 6.5f; height = 13f; break;
                case OverworldDecor.OrbitShard: radius = 0.9f; height = 3f; break;
                case OverworldDecor.LightTree: radius = 0.45f; height = 3.2f; break;
                case OverworldDecor.GardenFount: radius = 1.2f; height = 1f; break;
                case OverworldDecor.SunSail: radius = 0.75f; height = 14f; break;
                case OverworldDecor.SunCoil: radius = 0.8f; height = 0.6f; break;
                case OverworldDecor.CloudSteps: radius = 1.8f; height = 1.1f; break;
                case OverworldDecor.GloryColumn: radius = 6f; height = 40f; break;
                case OverworldDecor.SkyCrack: radius = 1f; height = 6f; break;
                case OverworldDecor.HoldPillar: radius = 0.55f; height = 4.5f; break;
                case OverworldDecor.PorchBench: radius = 0.9f; height = 0.6f; break;
                case OverworldDecor.RockingChair: radius = 0.5f; height = 1.2f; break;
                case OverworldDecor.MehenRail:
                {
                    // Two posts along its length: the serpent itself is walked past.
                    Vector2 along = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad));
                    foreach (float t in new[] { -1.4f, 1.4f })
                    {
                        Vector2 c = p + along * (t * s);
                        content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - 0.1f, Radius = 0.35f * s, Height = Mathf.Max(0.3f, 0.9f * s) + sink });
                    }

                    return true;
                }

                default: return true; // Flowers and the ground detail have no collider.
            }

            content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = radius * s, Height = Mathf.Max(0.3f, height * s) + sink });
            return true;
        }

        private static bool AppendCeuDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsCeuDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.LightSpeck: b.Stamp(OverworldDecorKit.LightSpeckStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.GoldRipple: b.Stamp(OverworldDecorKit.GoldRippleStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.LilacPlank: b.Stamp(OverworldDecorKit.LilacPlankStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.BronzeFiling: b.Stamp(OverworldDecorKit.BronzeFilingStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.CrystalChip: b.Stamp(OverworldDecorKit.CrystalChipStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.LightPetal: b.Stamp(OverworldDecorKit.LightPetalStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.SunGrain: b.Stamp(OverworldDecorKit.SunGrainStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.StepMark: b.Stamp(OverworldDecorKit.StepMarkStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.SkyCrackLine: b.Stamp(OverworldDecorKit.SkyCrackLineStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.ApricotDust: b.Stamp(OverworldDecorKit.ApricotDustStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.CloudWisp: b.Stamp(OverworldDecorKit.CloudWispStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.GoldMote: b.Stamp(OverworldDecorKit.GoldMoteStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.LilacMote: b.Stamp(OverworldDecorKit.LilacMoteStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.CloudBank: OverworldDecorKit.CloudBank(b, s, prop.Variant); break;
                case OverworldDecor.LightBollard: OverworldDecorKit.LightBollard(b, s, prop.Variant); break;
                case OverworldDecor.LightBuoy: OverworldDecorKit.LightBuoy(b, s, prop.Variant); break;
                case OverworldDecor.CloudIslet: OverworldDecorKit.CloudIslet(b, s, prop.Variant); break;
                case OverworldDecor.MehenRail: OverworldDecorKit.MehenRail(b, s, prop.Variant); break;
                case OverworldDecor.DeckCrate: OverworldDecorKit.DeckCrate(b, s, prop.Variant); break;
                case OverworldDecor.MoonMast: OverworldDecorKit.MoonMast(b, s, prop.Variant); break;
                case OverworldDecor.CelestialJar: OverworldDecorKit.CelestialJar(b, s, prop.Variant); break;
                case OverworldDecor.BronzeGear: OverworldDecorKit.BronzeGear(b, s, prop.Variant); break;
                case OverworldDecor.ArchonSphere: OverworldDecorKit.ArchonSphere(b, s, prop.Variant); break;
                case OverworldDecor.OrbitShard: OverworldDecorKit.OrbitShard(b, s, prop.Variant); break;
                case OverworldDecor.LightTree: OverworldDecorKit.LightTree(b, s, prop.Variant); break;
                case OverworldDecor.SongFlower: OverworldDecorKit.SongFlower(b, s, prop.Variant); break;
                case OverworldDecor.GardenFount: OverworldDecorKit.GardenFount(b, s, prop.Variant); break;
                case OverworldDecor.SunSail: OverworldDecorKit.SunSail(b, s, prop.Variant); break;
                case OverworldDecor.SunCoil: OverworldDecorKit.SunCoil(b, s, prop.Variant); break;
                case OverworldDecor.CloudSteps: OverworldDecorKit.CloudSteps(b, s, prop.Variant); break;
                case OverworldDecor.GloryColumn: OverworldDecorKit.GloryColumn(b, s, prop.Variant); break;
                case OverworldDecor.SkyCrack: OverworldDecorKit.SkyCrack(b, s, prop.Variant); break;
                case OverworldDecor.HoldPillar: OverworldDecorKit.HoldPillar(b, s, prop.Variant); break;
                case OverworldDecor.PorchBench: OverworldDecorKit.PorchBench(b, s, prop.Variant); break;
                case OverworldDecor.RockingChair: OverworldDecorKit.RockingChair(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
