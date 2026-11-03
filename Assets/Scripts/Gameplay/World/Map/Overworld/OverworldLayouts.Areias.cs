using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 2 — Areias Flamejantes as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, mapas 07–12),
    /// no scene change between the Oásis de Qocho and the sands around it:
    /// <list type="bullet">
    /// <item>the <b>Oásis de Qocho</b> (the region's return city, Vandak's caravanserai, Yardena's cistern) at the plaza
    /// (0, 0) near the north edge, the oasis lake and the karez line coming down from the north; the <b>Estrada das
    /// Caravanas</b> (07, nv 5–7: its hand-made map embedded with the Caravançará do Ammo) west of it, the gate to the
    /// Pomar on the north edge; the <b>Ruínas dos Artífices</b> (08, nv 6–9: the Acampamento das Pirâmides, the avenue of
    /// ushabtis, the Grande Ushabti's ring) south-west; the <b>Palmeiral do Karez</b> south (6–8);</item>
    /// <item>the in-between areas, harder the farther from Qocho: <b>Dunas do Vento Quente</b> (7–10), <b>Salinas de Lop</b>
    /// (8–11), <b>Planície dos Karez</b> (10–14), <b>Oásis das Tamargueiras</b> (10–14), <b>Erg Dourado</b> (11–15),
    /// <b>Cânions Vermelhos</b> (11–15, gate to 13), <b>Vale das Ossadas</b> (12–16), <b>Mesas do Fogo</b> (12–16),
    /// <b>Campo das Pirâmides de Meroé</b> (12–16), <b>Erg das Sete Cristas</b> (15–19), <b>Cânion do Eco</b> (15–19),
    /// <b>Cemitério de Caravanas</b> (17–21), <b>Erg do Sul</b> (17–21);</item>
    /// <item>the far south, each with its post: <b>Galerias do Karez</b> (09, nv 20–26, Boca do Karez), <b>Dunas que
    /// Cantam</b> (10, 22–28, fresh water, Poço das Tamareiras, the crescent lake), <b>Ermos do Tambor Rubro</b> with the
    /// orcs' city (11, 24–32, return city of 11–12) and <b>Trono de Namrael</b> (12, 30–38, poison protection, Torre de
    /// Taipa, the salt throne).</item>
    /// </list>
    /// Monsters reuse today's desert definitions (Escorpião, Abutre de Brasa and Lanterna Quebrada [distância], the ushabti,
    /// the jackal, the karez bats, the ember hawk and the night species: Faíscas Presas, Mardagayl, Ubır). Relief: dunes
    /// in crests, mesas, layered canyons, salt terraces (OverworldRelief.Desert.cs).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string AreiasId = "overworld_areias";

        public const string KarezId = "map_009_karez";
        public const string DunasId = "map_010_dunas";
        public const string TamborRubroMapId = "map_011_tambor_rubro";
        public const string NamraelId = "map_012_namrael";

        public static readonly Vector2 QochoOffset = Vector2.zero;
        public static readonly Vector2 CaravancaraOffset = new Vector2(-200f, 130f);
        public static readonly Vector2 ArtificesOffset = new Vector2(-400f, -150f);
        public static readonly Vector2 BocaDoKarezOffset = new Vector2(0f, -1100f);
        public static readonly Vector2 PocoTamareirasOffset = new Vector2(0f, -1480f);
        public static readonly Vector2 TamborRubroOffset = new Vector2(-800f, -1600f);
        public static readonly Vector2 TorreDeTaipaOffset = new Vector2(800f, -1450f);
        public static readonly Vector2 NamraelThrone = new Vector2(800f, -1810f);

        /// <summary>The existing desert monsters of the region (no new definitions).</summary>
        private static class Sands
        {
            public const string Scorpion = PeriodSpawnTable.DesertScorpion;
            public const string Vulture = "enemy_desert_ember_vulture";
            public const string Lantern = "enemy_desert_broken_lantern";
            public const string Ushabti = PeriodSpawnTable.MountainGolem;
            public const string Jackal = PeriodSpawnTable.ForestWolf;
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Hawk = "enemy_mountain_crested_hawk";
        }

        private static readonly ConditionalWeakTable<OverworldLayout, List<OverworldProp>> DesertPieces =
            new ConditionalWeakTable<OverworldLayout, List<OverworldProp>>();

        /// <summary>The placed desert pieces of <paramref name="layout"/> (pyramids, karez lines, caravans, towers…), empty for other regions.</summary>
        public static IReadOnlyList<OverworldProp> DesertPiecesOf(OverworldLayout layout)
        {
            return layout != null && DesertPieces.TryGetValue(layout, out List<OverworldProp> pieces) ? pieces : (IReadOnlyList<OverworldProp>)System.Array.Empty<OverworldProp>();
        }

        private static OverworldLayout Areias()
        {
            OverworldLayout w = NewRegion(AreiasId, MapRegion.AreiasFlamejantes, Rect.MinMaxRect(-1200f, -1900f, 1200f, 500f));
            w.World.Biome = BiomeType.Desert;
            const string caravanas = MapLayouts.CaravanasId;
            const string artifices = MapLayouts.ArtificesId;
            const string desert = MapRegionTypes.Desert;
            const string rock = MapRegionTypes.Mountain;

            // ---------------- Zones (tile the whole region, no overlap) ----------------
            // North band (z 0…500).
            OverworldZone zErgDourado = Zone(w, "erg_dourado", null, caravanas, -1200f, 0f, -700f, 500f, OverworldTerrain.Erg, 11, 15, 9,
                "Golden Erg", "Erg Dourado", desert, spacing: 105f);
            OverworldZone zEstrada = Zone(w, "estrada_das_caravanas", caravanas, null, -700f, 0f, -100f, 500f, OverworldTerrain.CaravanSteppe, 5, 7, 5,
                "Caravan Road", "Estrada das Caravanas", desert, spacing: 82f);
            OverworldZone zQocho = Zone(w, "oasis_de_qocho", caravanas, null, -100f, -200f, 300f, 500f, OverworldTerrain.Oasis, 5, 6, 5,
                "Qocho Oasis", "Oásis de Qocho", desert, spacing: 78f);
            OverworldZone zDunasBaixas = Zone(w, "dunas_do_vento_quente", null, caravanas, 300f, 0f, 700f, 500f, OverworldTerrain.LowDunes, 7, 10, 6,
                "Hot Wind Dunes", "Dunas do Vento Quente", desert, spacing: 92f);
            OverworldZone zCanions = Zone(w, "canions_vermelhos", null, artifices, 700f, 0f, 1200f, 500f, OverworldTerrain.RedCanyon, 11, 15, 9,
                "Red Canyons", "Cânions Vermelhos", rock, spacing: 100f);
            // Second band (z −450…0).
            OverworldZone zOssadas = Zone(w, "vale_das_ossadas", null, artifices, -1200f, -450f, -700f, 0f, OverworldTerrain.BoneValley, 12, 16, 10,
                "Valley of Bones", "Vale das Ossadas", desert, spacing: 100f);
            OverworldZone zArtifices = Zone(w, "ruinas_dos_artifices", artifices, null, -700f, -450f, -100f, 0f, OverworldTerrain.StoneDesert, 6, 9, 6,
                "Artificers' Ruins", "Ruínas dos Artífices", MapRegionTypes.Ruins, spacing: 86f);
            OverworldZone zPalmeiral = Zone(w, "palmeiral_do_karez", null, caravanas, -100f, -450f, 300f, -200f, OverworldTerrain.PalmGrove, 6, 8, 5,
                "Karez Palm Grove", "Palmeiral do Karez", desert, spacing: 82f);
            OverworldZone zSalinas = Zone(w, "salinas_de_lop", null, artifices, 300f, -450f, 700f, 0f, OverworldTerrain.SaltFlat, 8, 11, 7,
                "Lop Salt Flats", "Salinas de Lop", desert, spacing: 95f);
            OverworldZone zMesas = Zone(w, "mesas_do_fogo", null, artifices, 700f, -450f, 1200f, 0f, OverworldTerrain.Mesas, 12, 16, 10,
                "Fire Mesas", "Mesas do Fogo", rock, spacing: 100f);
            // Third band (z −900…−450).
            OverworldZone zSeteCristas = Zone(w, "erg_das_sete_cristas", null, artifices, -1200f, -900f, -700f, -450f, OverworldTerrain.Erg, 15, 19, 13,
                "Erg of the Seven Crests", "Erg das Sete Cristas", desert, spacing: 105f);
            OverworldZone zMeroe = Zone(w, "piramides_de_meroe", null, artifices, -700f, -900f, -250f, -450f, OverworldTerrain.MeroeRuins, 12, 16, 10,
                "Meroë Pyramid Field", "Campo das Pirâmides de Meroé", MapRegionTypes.Ruins, spacing: 95f);
            OverworldZone zPlanicieKarez = Zone(w, "planicie_dos_karez", null, caravanas, -250f, -900f, 250f, -450f, OverworldTerrain.KarezField, 10, 14, 8,
                "Karez Plain", "Planície dos Karez", desert, spacing: 92f);
            OverworldZone zTamargueiras = Zone(w, "oasis_das_tamargueiras", null, caravanas, 250f, -900f, 700f, -450f, OverworldTerrain.TamariskOasis, 10, 14, 8,
                "Tamarisk Oasis", "Oásis das Tamargueiras", desert, spacing: 92f);
            OverworldZone zEco = Zone(w, "canion_do_eco", null, artifices, 700f, -900f, 1200f, -450f, OverworldTerrain.RedCanyon, 15, 19, 13,
                "Echo Canyon", "Cânion do Eco", rock, spacing: 100f);
            // Fourth band (z −1300…−900).
            OverworldZone zCemiterio = Zone(w, "cemiterio_de_caravanas", null, artifices, -1200f, -1300f, -400f, -900f, OverworldTerrain.BoneValley, 17, 21, 15,
                "Caravan Graveyard", "Cemitério de Caravanas", desert, spacing: 105f);
            OverworldZone zGalerias = Zone(w, "galerias_do_karez", KarezId, null, -400f, -1300f, 400f, -900f, OverworldTerrain.KarezGallery, 20, 26, 18,
                "Karez Galleries", "Galerias do Karez", MapRegionTypes.Underground, spacing: 100f);
            OverworldZone zErgSul = Zone(w, "erg_do_sul", null, artifices, 400f, -1300f, 1200f, -900f, OverworldTerrain.Erg, 17, 21, 15,
                "Southern Erg", "Erg do Sul", desert, spacing: 105f);
            // The far south (z −1900…−1300): 10, 11 and 12.
            OverworldZone zTambor = Zone(w, "ermos_do_tambor_rubro", TamborRubroMapId, null, -1200f, -1900f, -400f, -1300f, OverworldTerrain.Badlands, 24, 32, 22,
                "Red Drum Badlands", "Ermos do Tambor Rubro", rock, spacing: 100f);
            OverworldZone zDunas = Zone(w, "dunas_que_cantam", DunasId, null, -400f, -1900f, 400f, -1300f, OverworldTerrain.SingingDunes, 22, 28, 20,
                "Singing Dunes", "Dunas que Cantam", desert, MapRequirementTags.FreshWater, 100f);
            OverworldZone zNamrael = Zone(w, "trono_de_namrael", NamraelId, null, 400f, -1900f, 1200f, -1300f, OverworldTerrain.SaltThrone, 30, 38, 28,
                "Namrael's Throne", "Trono de Namrael", desert, MapRequirementTags.PoisonProtection, 100f);

            AddAreiasFauna(zErgDourado, zEstrada, zQocho, zDunasBaixas, zCanions, zOssadas, zArtifices, zPalmeiral, zSalinas, zMesas, zSeteCristas,
                zMeroe, zPlanicieKarez, zTamargueiras, zEco, zCemiterio, zGalerias, zErgSul, zTambor, zDunas, zNamrael);

            // ---------------- Settlements ----------------
            // Qocho first: the region's return city and the barca's stop for 07.
            CityTownRecipe qocho = CityTownRecipes.Qocho();
            qocho.Backdrop = false;
            w.Vila = Place(w, caravanas, qocho, QochoOffset, Rect.MinMaxRect(-40f, -42f, 40f, 28f), 46f, true);

            // 07 and 08 as they are (their posts, the karez detour, the avenue of ushabtis); the scatter of the sands dresses them.
            MapLayout caravanasSrc = MapLayouts.For(caravanas);
            Embed(w, caravanasSrc, CaravancaraOffset, withProps: false, authored: false);
            MapLayout artificesSrc = MapLayouts.For(artifices);
            Embed(w, artificesSrc, ArtificesOffset, withProps: false, authored: false);

            // The posts of 09, 10 and 12 and the orcs' city of 11 (return city of 11–12).
            Place(w, KarezId, BocaDoKarez(), BocaDoKarezOffset, DesertPostSafe, 18f, false);
            Place(w, DunasId, PocoDasTamareiras(), PocoTamareirasOffset, DesertPostSafe, 18f, false);
            CityTownRecipe tambor = CityTownRecipes.TamborRubro();
            tambor.Backdrop = false;
            Place(w, TamborRubroMapId, tambor, TamborRubroOffset, Rect.MinMaxRect(-34f, -34f, 34f, 34f), 38f, true);
            Place(w, NamraelId, TorreDeTaipa(), TorreDeTaipaOffset, DesertPostSafe, 18f, false);

            // ---------------- Water ----------------
            // Qocho's oasis lake, east of the town.
            River(w, 26f, new Vector2(112f, -112f), new Vector2(172f, -140f));
            // The karez channel through the palm grove (plank bridge where the south road crosses it).
            Vector2 karezA = new Vector2(-90f, -262f);
            Vector2 karezB = new Vector2(100f, -302f);
            MapRiver karez = River(w, 3f, karezA, karezB, new Vector2(290f, -334f));
            Crossing(karez, On(karezA, karezB, 90f / 190f), 4.5f, true);
            // Brine pools on the salt flats, the tamarisk lake, the seeping wells over the galleries.
            River(w, 14f, new Vector2(430f, -330f), new Vector2(462f, -342f));
            River(w, 18f, new Vector2(590f, -100f), new Vector2(626f, -84f));
            River(w, 40f, new Vector2(420f, -700f), new Vector2(520f, -720f));
            River(w, 10f, new Vector2(-210f, -1000f), new Vector2(-188f, -1010f));
            River(w, 9f, new Vector2(214f, -1190f), new Vector2(236f, -1180f));
            // The crescent lake of the Dunas que Cantam, south of the Poço das Tamareiras.
            var crescent = new List<Vector2>();
            for (float a = 200f; a <= 340.1f; a += 20f)
            {
                crescent.Add(new Vector2(0f, -1715f) + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * 115f);
            }

            River(w, 16f, crescent.ToArray());

            // ---------------- Gates and the island portal ----------------
            Gate(w, "exit_pomar", MapExitKind.Map, MapCatalog.PomarId, Rect.MinMaxRect(-308f, 497f, -292f, 500f), new Vector2(-300f, 491f), Vector2.down);
            Gate(w, "exit_rondaveis", MapExitKind.Map, MapLayouts.RondaveisId, Rect.MinMaxRect(1197f, 242f, 1200f, 258f), new Vector2(1191f, 250f), Vector2.left);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(73f, 58f, 77f, 62f), new Vector2(75f, 50f), Vector2.down);
            Landmark(w, MapLandmarkKind.StoneRing, 75f, 60f, 0f, 0.45f);

            // ---------------- Trails ----------------
            // The caravan road: the Pomar gate down to the Caravançará, on to Qocho's West street.
            Path(w, 5f, -300f, 500f, -300f, 330f, -300f, 130f);
            PathVia(w, 5f, CaravancaraOffset + new Vector2(100f, 0f), new Vector2(-75f, 60f), new Vector2(-46f, 0f));
            // The south road: Qocho's South street, through the palm grove and the karez plain to the posts of 09 and 10.
            Path(w, 5f, 0f, -46f, 0f, -200f, 0f, -450f, 0f, -680f, 0f, -900f, 0f, -1082f);
            Path(w, 4f, 0f, -1118f, 0f, -1300f, 0f, -1462f);
            // 07's south detour to the Artífices; the avenue's branches east (to the south road) and west (to the bones).
            Vector2 caravanasSouth = Port(caravanasSrc, CaravancaraOffset, artifices, MapExitKind.Map, new Vector2(-160f, 40f));
            Vector2 artificesNorth = Port(artificesSrc, ArtificesOffset, caravanas, MapExitKind.Map, new Vector2(-400f, -120f));
            PathVia(w, 3.5f, caravanasSouth, new Vector2(-280f, -40f), artificesNorth);
            Vector2 artificesEast = ArtificesOffset + new Vector2(100f, -60f);
            Vector2 artificesWest = ArtificesOffset + new Vector2(-100f, -40f);
            PathVia(w, 3.5f, artificesEast, new Vector2(-150f, -205f), new Vector2(0f, -200f));
            PathVia(w, 3.5f, artificesWest, new Vector2(-760f, -220f), new Vector2(-950f, -200f));
            // The west: the bones up to the golden erg and back to the caravan road.
            Path(w, 3.5f, -950f, -200f, -960f, 120f, -850f, 380f);
            Path(w, 3.5f, -300f, 330f, -560f, 300f, -850f, 380f);
            // The east road: past the lake to the salt flats and the mesas; north through the dunes to the canyons' gate.
            Path(w, 4f, 0f, -120f, 300f, -60f, 500f, -150f, 720f, -200f, 950f, -150f, 1120f, -220f);
            Path(w, 3.5f, 500f, -150f, 500f, 200f, 800f, 260f, 1200f, 250f);
            // The third band: west to Meroë and the seven crests, east to the tamarisks and the echo canyon.
            Path(w, 3.5f, 0f, -680f, -450f, -680f, -950f, -680f);
            Path(w, 3.5f, 0f, -680f, 470f, -620f, 950f, -680f);
            // Down to the far south: the graveyard to the Tambor Rubro's West street, the southern erg to the Torre de Taipa.
            Path(w, 4f, -450f, -680f, -800f, -1100f, -900f, -1450f, -838f, -1600f);
            Path(w, 4f, 470f, -620f, 800f, -1100f, 800f, -1432f);
            // The Poço das Tamareiras between the two: west to the orcs' East street, east to the Torre's West street.
            Path(w, 3.5f, -18f, -1480f, -400f, -1540f, -762f, -1600f);
            Path(w, 3.5f, 18f, -1480f, 400f, -1470f, 782f, -1450f);
            // Namrael's terraces up to the salt throne; the badlands out to the clan camps.
            Path(w, 3.5f, 800f, -1468f, 800f, -1650f, 800f, -1785f);
            Path(w, 3.5f, -838f, -1600f, -1050f, -1650f, -1110f, -1760f);
            Path(w, 3f, -800f, -1638f, -760f, -1760f, -560f, -1800f);

            // ---------------- Landmarks ----------------
            Landmark(w, MapLandmarkKind.Signpost, -294f, 336f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 6f, -206f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 6f, -686f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 506f, -156f, 30f);
            Landmark(w, MapLandmarkKind.Signpost, -444f, -686f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 476f, -626f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, -944f, -206f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -318f, 445f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 30f, -640f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 1170f, 270f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -980f, 160f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.Campfire, 820f, -1100f, 0f, 0.9f);
            foreach (Vector2 camp in ClanCamps)
            {
                Landmark(w, MapLandmarkKind.Campfire, camp.x, camp.y, 0f, 1.1f);
            }

            Landmark(w, MapLandmarkKind.StoneRing, -560f, -820f, 0f, 1.2f);
            Landmark(w, MapLandmarkKind.StoneRing, 960f, -560f, 0f, 0.8f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            DropLeaningClearings(w);
            DesertPieces.AddOrUpdate(w, PlaceDesertPieces(w));
            return w;
        }

        /// <summary>
        /// A clearing's plateau ramps over the trail beside it; among the dunes, mesas and canyons the trail runs far below or
        /// above the ground the clearing stands on, so its plateau would tilt the trail: those clearings are dropped (the
        /// trails stay level across, Docs/Mapas100.md §0.10). The relief is rebuilt on first use afterwards.
        /// </summary>
        private static void DropLeaningClearings(OverworldLayout w)
        {
            OverworldRelief relief = OverworldRelief.For(w);
            var drop = new List<MapSpawnZone>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                float reach = spawn.Radius * 0.95f + 1f + 12f;
                float h = relief.HeightAt(spawn.Center);
                foreach (MapPath path in w.World.Paths)
                {
                    if (MapLayoutRules.DistanceToSegment(spawn.Center, path.A, path.B) > reach + path.Width * 0.5f)
                    {
                        continue;
                    }

                    Vector2 ab = path.B - path.A;
                    float t = ab.sqrMagnitude > 1e-4f ? Mathf.Clamp01(Vector2.Dot(spawn.Center - path.A, ab) / ab.sqrMagnitude) : 0f;
                    if (Mathf.Abs(relief.HeightAt(path.A + ab * t) - h) > 1.2f)
                    {
                        drop.Add(spawn);
                        break;
                    }
                }
            }

            foreach (MapSpawnZone spawn in drop)
            {
                w.World.Zones.Remove(spawn);
                w.ZoneOfSpawn.Remove(spawn.Id);
            }

            w.ReliefCache = null;
        }

        /// <summary>The three camps of the clans touched by Āz at the edges of the badlands (11).</summary>
        private static readonly Vector2[] ClanCamps = { new Vector2(-1110f, -1400f), new Vector2(-1110f, -1780f), new Vector2(-540f, -1820f) };

        // ------------------------------------------------------------------
        // Placed pieces (streamed by OverworldChunkBuilder.Desert.cs)
        // ------------------------------------------------------------------

        private static List<OverworldProp> PlaceDesertPieces(OverworldLayout w)
        {
            var pieces = new List<OverworldProp>();

            // The karez line coming down to Qocho from the north (it goes on the town's own shafts), and the lines across the
            // karez plain and over the galleries.
            for (int i = 0; i < 34; i++)
            {
                Piece(w, pieces, OverworldDecor.KarezShaft, new Vector2(-7.4f + 3f * Mathf.Sin(i * 0.4f), 84f + i * 12f), i * 37f, 1f, i);
            }

            KarezLine(w, pieces, new Vector2(-230f, -470f), new Vector2(220f, -880f), 17f);
            KarezLine(w, pieces, new Vector2(-380f, -930f), new Vector2(380f, -1270f), 19f);
            KarezLine(w, pieces, new Vector2(-380f, -1270f), new Vector2(-60f, -940f), 21f);

            // Markers along the caravan road and the south road.
            for (int i = 0; i < 6; i++)
            {
                Piece(w, pieces, OverworldDecor.Marker, new Vector2(i % 2 == 0 ? -309f : -291f, 470f - i * 60f), i % 2 == 0 ? 90f : 270f, 1f, i);
            }

            for (int i = 0; i < 9; i++)
            {
                Piece(w, pieces, OverworldDecor.Marker, new Vector2(i % 2 == 0 ? -9f : 9f, -500f - i * 120f), i % 2 == 0 ? 90f : 270f, 1f, i);
            }

            // Caravans: resting ones by the roads, broken ones where the sand won.
            foreach (Vector2 camp in new[] { new Vector2(-340f, 400f), new Vector2(-60f, -640f), new Vector2(330f, -110f), new Vector2(230f, -1440f), new Vector2(-860f, -1080f) })
            {
                Piece(w, pieces, OverworldDecor.CaravanCamp, camp, TownStyle.Hash01("ar_camp", (int)camp.x) * 360f, 1f, (int)camp.y);
            }

            int k = 0;
            foreach (Vector2 wreck in new[]
            {
                new Vector2(-450f, 250f), new Vector2(-610f, 110f), new Vector2(-1000f, -100f), new Vector2(-1080f, -330f), new Vector2(-880f, -980f),
                new Vector2(-1050f, -1200f), new Vector2(-620f, -1160f), new Vector2(-480f, -1000f), new Vector2(620f, 380f), new Vector2(980f, -1050f),
            })
            {
                Piece(w, pieces, OverworldDecor.CaravanWreck, wreck, TownStyle.Hash01("ar_wreck", k) * 360f, 1.1f + 0.3f * TownStyle.Hash01("ar_wreck", k + 50), k);
                k++;
            }

            // The Meroë pyramid field: big steep pyramids facing east, chapels in front.
            k = 0;
            foreach (Vector3 pyramid in new[]
            {
                new Vector3(-560f, -560f, 4f), new Vector3(-505f, -590f, 3f), new Vector3(-625f, -605f, 3.5f), new Vector3(-380f, -800f, 4.5f),
                new Vector3(-320f, -760f, 3f), new Vector3(-455f, -820f, 3.2f), new Vector3(-640f, -830f, 2.8f), new Vector3(-660f, -500f, 2.6f),
                new Vector3(-300f, -560f, 3.4f), new Vector3(-1000f, -760f, 2.4f), new Vector3(-170f, -330f, 2.2f),
            })
            {
                Piece(w, pieces, OverworldDecor.Pyramid, new Vector2(pyramid.x, pyramid.y), 90f + 8f * (TownStyle.Hash01("ar_pyr", k) - 0.5f), pyramid.z, k);
                k++;
            }

            // Natural arches and chimneys in the canyons, the mesas and the badlands.
            k = 0;
            foreach (Vector2 arch in new[] { new Vector2(880f, 120f), new Vector2(1080f, 400f), new Vector2(860f, -820f), new Vector2(-1000f, -1520f), new Vector2(-620f, -1420f), new Vector2(1100f, -300f) })
            {
                Piece(w, pieces, OverworldDecor.RockArch, arch, TownStyle.Hash01("ar_arch", k) * 180f, 1.5f, k);
                k++;
            }

            for (int i = 0; i < 24; i++)
            {
                float x = Mathf.Lerp(-1180f, -420f, TownStyle.Hash01("ar_hoodoo", i));
                float z = Mathf.Lerp(-1880f, -1320f, TownStyle.Hash01("ar_hoodoo", i + 100));
                Piece(w, pieces, OverworldDecor.Hoodoo, new Vector2(x, z), TownStyle.Hash01("ar_hoodoo", i + 200) * 360f, 1.3f + 0.6f * TownStyle.Hash01("ar_hoodoo", i + 300), i);
            }

            // Namrael: the salt throne (a crown of great red crystals) and the rammed-earth towers of Qocho's sentinels.
            Piece(w, pieces, OverworldDecor.SaltCrystal, NamraelThrone, 0f, 3.2f, 0);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                Piece(w, pieces, OverworldDecor.SaltCrystal, NamraelThrone + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 16f, i * 41f, 1.6f, i + 1);
            }

            k = 0;
            foreach (Vector2 tower in new[] { new Vector2(600f, -1350f), new Vector2(1010f, -1380f), new Vector2(640f, -1660f), new Vector2(1060f, -1700f), new Vector2(930f, -1860f), new Vector2(520f, -1840f) })
            {
                Piece(w, pieces, OverworldDecor.TaipaTower, tower, 90f * (k % 4), 1.1f, k);
                k++;
            }

            // The clan camps of the badlands: ochre tents round each fire.
            k = 0;
            foreach (Vector2 camp in ClanCamps)
            {
                for (int i = 0; i < 3; i++)
                {
                    float a = (i * 120f + 40f * k) * Mathf.Deg2Rad;
                    Vector2 at = camp + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 11f;
                    Piece(w, pieces, OverworldDecor.ClanTent, at, CityGeometry.YawFacing(camp - at), 1.1f, i + k * 3);
                }

                k++;
            }

            return pieces;
        }

        private static void KarezLine(OverworldLayout w, List<OverworldProp> pieces, Vector2 from, Vector2 to, float step)
        {
            float length = Vector2.Distance(from, to);
            int n = Mathf.FloorToInt(length / step);
            for (int i = 0; i <= n; i++)
            {
                Piece(w, pieces, OverworldDecor.KarezShaft, Vector2.Lerp(from, to, i / (float)n), i * 53f, 1f, i);
            }
        }

        /// <summary>Adds a placed piece where it fits (off trails, water, towns, gates, landmarks and spawn clearings).</summary>
        private static void Piece(OverworldLayout w, List<OverworldProp> pieces, OverworldDecor decor, Vector2 at, float yaw, float scale, int variant)
        {
            float half = OverworldChunkBuilder.DesertFootprint(decor) * scale;
            if (!CityGeometry.Inflate(w.Bounds, -(MapLayoutRules.WallMargin + half + 4f)).Contains(at) || !DesertPieceFits(w, pieces, at, half))
            {
                return;
            }

            pieces.Add(new OverworldProp { Kind = CityPropKind.Rock, Decor = decor, Position = at, Yaw = yaw, Scale = scale, Variant = variant });
        }

        private static bool DesertPieceFits(OverworldLayout w, List<OverworldProp> pieces, Vector2 at, float half)
        {
            if (OverworldRules.DistanceToSafety(w, at) < half + 4f)
            {
                return false;
            }

            foreach (OverworldSettlement settlement in w.Settlements)
            {
                if (CityGeometry.Inflate(settlement.GroundRect, half + 4f).Contains(at))
                {
                    return false;
                }
            }

            foreach (MapPath path in w.World.Paths)
            {
                if (MapLayoutRules.DistanceToSegment(at, path.A, path.B) < path.Width * 0.5f + half + 2f)
                {
                    return false;
                }
            }

            foreach (MapRiver river in w.World.Rivers)
            {
                if (MapLayoutRules.DistanceToPolyline(at, river.Points) < river.Width * 0.5f + half + 3f)
                {
                    return false;
                }
            }

            foreach (MapSpawnZone zone in w.World.Zones)
            {
                if (Vector2.Distance(at, zone.Center) < zone.Radius + half + 3f)
                {
                    return false;
                }
            }

            foreach (MapLandmark landmark in w.World.Landmarks)
            {
                float reach = landmark.Kind == MapLandmarkKind.StoneRing ? 7.5f * landmark.Scale + 3f : 5f;
                if (Vector2.Distance(at, landmark.Position) < reach + half)
                {
                    return false;
                }
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                if (Vector2.Distance(at, exit.Arrival) < half + 10f || CityGeometry.Inflate(exit.Trigger, half + 6f).Contains(at))
                {
                    return false;
                }
            }

            foreach (OverworldProp other in pieces)
            {
                if (Vector2.Distance(at, other.Position) < half + OverworldChunkBuilder.DesertFootprint(other.Decor) * other.Scale + 1f)
                {
                    return false;
                }
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Posts and the orcs' city (town-kit recipes, plaza at the origin)
        // ------------------------------------------------------------------

        private static readonly Rect DesertPostSafe = Rect.MinMaxRect(-15f, -15f, 16f, 15f);

        /// <summary>A small walled post of the sands: the guardian lamp, one house, the Cais da Barca on the South street.</summary>
        private static CityTownRecipe DesertPost(string id, TownBuildingSpec house, bool westEast)
        {
            var r = new CityTownRecipe
            {
                CityId = id, Region = BiomeType.Desert, PlazaRadius = 4.5f, StreetWidth = 4f, SidewalkWidth = 0.8f, WalkHalfExtent = 16f,
                EntryArm = CityArm.North, AutoYards = false, Backdrop = false, NightLights = 4,
            };
            r.Arms.Add(new CityArmRecipe(CityArm.North, 13f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 13f, exitToMap: true));
            if (westEast)
            {
                r.Arms.Add(new CityArmRecipe(CityArm.West, 13f, exitToMap: true));
                r.Arms.Add(new CityArmRecipe(CityArm.East, 13f, exitToMap: true));
            }

            r.Lots.Add(new CityLotRecipe
            {
                Id = "lampada_" + id, Kind = CityLotKind.LampHouse, Free = true, Center = westEast ? new Vector2(9.5f, 9.5f) : new Vector2(9.5f, 0f),
                Front = westEast ? Vector2.down : Vector2.left, Width = 4.5f, Depth = 5f, Porch = 0f,
                Spec = new TownBuildingSpec { WallColor = TownSwatch.Sandstone, TrimColor = TownSwatch.Turquoise, RoofColor = TownSwatch.Turquoise, JoineryColor = TownSwatch.Walnut, AccentColor = TownSwatch.Gold, WallHeight = 3.4f },
            });
            r.Lots.Add(new CityLotRecipe { Id = "casa_" + id, Kind = CityLotKind.House, Arm = CityArm.North, Side = CityArm.West, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f, Spec = house });
            r.Lots.Add(new CityLotRecipe { Id = "cais_" + id, Kind = CityLotKind.ShipDock, Arm = CityArm.South, Side = CityArm.West, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            return r;
        }

        private static CityProp DesertProp(CityPropKind kind, float x, float z, float yaw, float scale = 1f, int variant = 0)
        {
            return new CityProp { Kind = kind, Position = new Vector2(x, z), Yaw = yaw, Scale = scale, Variant = variant };
        }

        /// <summary>09 · Boca do Karez: Yardena's cistern over the galleries, shafts round the plaza.</summary>
        private static CityTownRecipe BocaDoKarez()
        {
            CityTownRecipe r = DesertPost("boca_do_karez", new TownBuildingSpec
            {
                WallColor = TownSwatch.Adobe, TrimColor = TownSwatch.Sandstone, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Walnut,
                AccentColor = TownSwatch.Turquoise, Roof = TownRoofKind.Dome, Dressing = TownShopDressing.Cistern,
            }, false);
            r.PlazaProps.Add(DesertProp(CityPropKind.KarezShaft, -9.5f, -8.5f, 20f));
            r.PlazaProps.Add(DesertProp(CityPropKind.KarezShaft, 9.5f, -9f, 70f));
            r.PlazaProps.Add(DesertProp(CityPropKind.Jugs, -5.6f, 4.8f, 30f));
            r.PlazaProps.Add(DesertProp(CityPropKind.Crates, 6f, -5.6f, 15f));
            return r;
        }

        /// <summary>10 · Poço das Tamareiras: the well under the date palms, the lake in a crescent beyond the dunes.</summary>
        private static CityTownRecipe PocoDasTamareiras()
        {
            CityTownRecipe r = DesertPost("poco_das_tamareiras", new TownBuildingSpec
            {
                WallColor = TownSwatch.TaipaRose, TrimColor = TownSwatch.Plaster, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Violet,
                AccentColor = TownSwatch.Gold, DryingLoft = true, FrontWindows = 1,
            }, true);
            r.PlazaProps.Add(DesertProp(CityPropKind.Well, -9.5f, -9.5f, 0f));
            r.PlazaProps.Add(DesertProp(CityPropKind.Tree, -11.5f, 10.5f, 20f, 1.2f, 0));
            r.PlazaProps.Add(DesertProp(CityPropKind.Tree, 11f, -11f, 140f, 1.1f, 3));
            r.PlazaProps.Add(DesertProp(CityPropKind.Tree, -12f, -12.5f, 220f, 1f, 0));
            r.PlazaProps.Add(DesertProp(CityPropKind.Camel, 6.5f, -6.5f, 200f));
            return r;
        }

        /// <summary>12 · Torre de Taipa: the sentinels' tall rammed-earth house, markers and crates.</summary>
        private static CityTownRecipe TorreDeTaipa()
        {
            CityTownRecipe r = DesertPost("torre_de_taipa", new TownBuildingSpec
            {
                WallColor = TownSwatch.AdobeOchre, TrimColor = TownSwatch.Sandstone, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.WoodDark,
                AccentColor = TownSwatch.Gold, WallHeight = 4.6f, RammedEarth = true, FrontWindows = 1,
            }, true);
            r.PlazaProps.Add(DesertProp(CityPropKind.Stela, -9.5f, -9.5f, 45f, 0.55f));
            r.PlazaProps.Add(DesertProp(CityPropKind.Crates, 6f, -6f, 15f));
            r.PlazaProps.Add(DesertProp(CityPropKind.Jugs, -6f, 5.6f, 0f));
            return r;
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only)
        // ------------------------------------------------------------------

        private static void AddAreiasFauna(OverworldZone ergDourado, OverworldZone estrada, OverworldZone qocho, OverworldZone dunasBaixas, OverworldZone canions,
            OverworldZone ossadas, OverworldZone artifices, OverworldZone palmeiral, OverworldZone salinas, OverworldZone mesas, OverworldZone seteCristas,
            OverworldZone meroe, OverworldZone planicieKarez, OverworldZone tamargueiras, OverworldZone eco, OverworldZone cemiterio, OverworldZone galerias,
            OverworldZone ergSul, OverworldZone tambor, OverworldZone dunas, OverworldZone namrael)
        {
            const string esc = Sands.Scorpion;
            const string abutre = Sands.Vulture;
            const string lanterna = Sands.Lantern;
            const string ushabti = Sands.Ushabti;
            const string chacal = Sands.Jackal;
            const string morcegos = Sands.Bats;
            const string gaviao = Sands.Hawk;
            NightSpecies faiscas = NightVariantRules.VagaLume;
            NightSpecies mardagayl = NightVariantRules.Lobisomem;
            NightSpecies ubir = NightVariantRules.MorcegoVampiro;

            // Estrada das Caravanas (07): the Escorpião [tanque], the jackal following the caravans [rápido], the Abutre de Brasa
            // [distância] over the broken caravans; at dusk and night the Faíscas Presas, the Mardagayl down the karez on some nights.
            estrada.Fauna.Add(Fauna("escorpioes_da_estrada", "[tanque]", T(N(esc, 3), N(chacal, 1)), T(V(esc, 2), S(faiscas, 2)), 4,
                T(N(esc, 2), S(faiscas, 1)), 24f));
            estrada.Fauna.Add(Fauna("chacais_da_caravana", "[rápido] [elite]", T(N(chacal, 3), N(esc, 1)), T(V(chacal, 3), S(mardagayl, 1)), 4, null, 24f));
            estrada.Fauna.Add(Fauna("abutres_da_estrada", "[distância]", T(N(abutre, 2), N(esc, 1)), T(V(abutre, 2), S(faiscas, 1)), 3, null, 26f));

            // Oásis de Qocho (07): scorpions at the water's edge, jackals in the palms, vultures circling the town.
            qocho.Fauna.Add(Fauna("beira_do_oasis", "[tanque] [enxame]", T(N(esc, 3)), T(V(esc, 2), S(faiscas, 2)), 4, T(N(esc, 2), S(faiscas, 1)), 22f));
            qocho.Fauna.Add(Fauna("chacais_do_oasis", "[rápido]", T(N(chacal, 3), N(esc, 1)), T(V(chacal, 2), S(faiscas, 1)), 3, null, 22f));
            qocho.Fauna.Add(Fauna("abutres_de_qocho", "[distância]", T(N(abutre, 2), N(chacal, 1)), T(V(abutre, 1), S(faiscas, 2)), 3, null, 26f));

            // Palmeiral do Karez: scorpions and jackals under the palms, sparks over the channel at night.
            palmeiral.Fauna.Add(Fauna("tamareiras", "[tanque] [rápido]", T(N(esc, 2), N(chacal, 2)), T(V(esc, 2), S(faiscas, 2)), 4, null, 22f));
            palmeiral.Fauna.Add(Fauna("abutres_das_palmeiras", "[distância]", T(N(abutre, 3)), T(V(abutre, 2), S(faiscas, 1)), 3, null, 26f));

            // Ruínas dos Artífices (08): the ushabtis [tanque] that wake at night, the Lanterna Quebrada [distância].
            artifices.Fauna.Add(Fauna("ushabtis_da_avenida", "[tanque]", T(N(ushabti, 3), N(esc, 1)), T(V(ushabti, 3), S(faiscas, 1)), 4, null, 26f));
            artifices.Fauna.Add(Fauna("lanternas_quebradas", "[distância]", T(N(lanterna, 2), N(esc, 1)), T(V(lanterna, 2), S(faiscas, 1)), 3, null, 26f));

            // Dunas do Vento Quente and the Salinas de Lop.
            dunasBaixas.Fauna.Add(Fauna("escorpioes_da_duna", "[tanque]", T(N(esc, 3)), T(V(esc, 3), S(faiscas, 1)), 4));
            dunasBaixas.Fauna.Add(Fauna("abutres_do_vento", "[distância]", T(N(abutre, 3), N(chacal, 1)), T(V(abutre, 2), S(faiscas, 1)), 3, null, 26f));
            dunasBaixas.Fauna.Add(Fauna("chacais_da_duna", "[rápido]", T(N(chacal, 3)), T(V(chacal, 3), S(faiscas, 1)), 4));
            salinas.Fauna.Add(Fauna("escorpioes_de_sal", "[tanque]", T(N(esc, 3), N(ushabti, 1)), T(V(esc, 3), S(faiscas, 1)), 4));
            salinas.Fauna.Add(Fauna("abutres_do_sal", "[distância]", T(N(abutre, 3)), T(V(abutre, 2), S(faiscas, 2)), 3, null, 26f));

            // Planície dos Karez and the Oásis das Tamargueiras.
            planicieKarez.Fauna.Add(Fauna("bocas_secas", "[enxame]", T(N(morcegos, 2), N(esc, 1)), T(N(morcegos, 2), S(ubir, 1)), 5, null, 22f));
            planicieKarez.Fauna.Add(Fauna("ushabtis_cavadores", "[tanque]", T(N(ushabti, 2), N(esc, 1)), T(V(ushabti, 2), S(faiscas, 1)), 3, null, 28f));
            planicieKarez.Fauna.Add(Fauna("chacais_do_karez", "[rápido]", T(N(chacal, 3), N(morcegos, 1)), T(V(chacal, 3), S(mardagayl, 1)), 4));
            tamargueiras.Fauna.Add(Fauna("tamargueiral", "[rápido] [tanque]", T(N(chacal, 2), N(esc, 2)), T(V(chacal, 2), S(faiscas, 2)), 4));
            tamargueiras.Fauna.Add(Fauna("abutres_do_lago", "[distância]", T(N(abutre, 3), N(chacal, 1)), T(V(abutre, 2), S(faiscas, 1)), 3, null, 26f));

            // Erg Dourado, the Erg das Sete Cristas and the Erg do Sul: scorpions, vultures, the sparks doubling at night.
            foreach (OverworldZone erg in new[] { ergDourado, seteCristas, ergSul })
            {
                erg.Fauna.Add(Fauna("escorpioes_do_erg", "[tanque]", T(N(esc, 4)), T(V(esc, 3), S(faiscas, 2)), 4, null, 26f));
                erg.Fauna.Add(Fauna("abutres_do_erg", "[distância]", T(N(abutre, 3), N(esc, 1)), T(V(abutre, 2), S(faiscas, 2)), 3, null, 26f));
            }

            // Vale das Ossadas and the Cemitério de Caravanas: carrion birds, the old ushabtis, jackals (the Mardagayl at night).
            foreach (OverworldZone bones in new[] { ossadas, cemiterio })
            {
                bones.Fauna.Add(Fauna("carniceiros", "[distância] [rápido]", T(N(abutre, 2), N(chacal, 2)), T(V(abutre, 2), S(mardagayl, 1)), 4, null, 26f));
                bones.Fauna.Add(Fauna("ossos_de_pedra", "[tanque]", T(N(ushabti, 3), N(esc, 1)), T(V(ushabti, 3), S(faiscas, 1)), 3, null, 30f));
            }

            // Cânions Vermelhos, the Cânion do Eco and the Mesas do Fogo: the ember hawks [rápido], scorpions, the lanterns.
            foreach (OverworldZone canyon in new[] { canions, eco })
            {
                canyon.Fauna.Add(Fauna("gavioes_de_brasa", "[rápido] [distância]", T(N(gaviao, 3), N(esc, 1)), T(V(gaviao, 2), S(faiscas, 1)), 4, null, 24f));
                canyon.Fauna.Add(Fauna("escorpioes_rubros", "[tanque]", T(N(esc, 3), N(lanterna, 1)), T(V(esc, 2), V(lanterna, 1), S(faiscas, 1)), 4, null, 26f));
            }

            mesas.Fauna.Add(Fauna("gavioes_das_mesas", "[rápido]", T(N(gaviao, 3), N(abutre, 1)), T(V(gaviao, 2), S(faiscas, 1)), 4, null, 24f));
            mesas.Fauna.Add(Fauna("pedras_vivas", "[tanque]", T(N(ushabti, 3), N(esc, 1)), T(V(ushabti, 2), S(mardagayl, 1)), 3, null, 30f));

            // Campo das Pirâmides de Meroé: ushabtis, lanterns among the pyramids, bats out of the chapels.
            meroe.Fauna.Add(Fauna("ushabtis_de_meroe", "[tanque]", T(N(ushabti, 3), N(esc, 1)), T(V(ushabti, 3), S(faiscas, 1)), 4, null, 28f));
            meroe.Fauna.Add(Fauna("lanternas_de_meroe", "[distância]", T(N(lanterna, 3), N(ushabti, 1)), T(V(lanterna, 2), S(faiscas, 1)), 3, null, 26f));
            meroe.Fauna.Add(Fauna("morcegos_das_capelas", "[enxame]", T(N(morcegos, 3)), T(N(morcegos, 2), S(ubir, 1)), 5, null, 22f));

            // 09 · Galerias do Karez: the karez bats [enxame] (Ubır down the shafts at night), scorpions by the water, ushabti diggers.
            galerias.Fauna.Add(Fauna("morcegos_do_karez", "[enxame]", T(N(morcegos, 3), N(esc, 1)), T(N(morcegos, 2), S(ubir, 1)), 5, null, 22f));
            galerias.Fauna.Add(Fauna("escorpioes_dagua", "[rápido] [tanque]", T(N(esc, 3), N(chacal, 1)), T(V(esc, 3), S(ubir, 1)), 4));
            galerias.Fauna.Add(Fauna("ushabtis_cavadores_fundos", "[tanque]", T(N(ushabti, 3)), T(V(ushabti, 3), N(morcegos, 1)), 3, null, 30f));

            // 10 · Dunas que Cantam: the giant scorpions [tanque], jackals for the spiny lizards [rápido], the sparks doubling at night.
            dunas.Fauna.Add(Fauna("escorpioes_gigantes", "[tanque]", T(N(esc, 4)), T(V(esc, 3), S(faiscas, 2)), 4, null, 28f));
            dunas.Fauna.Add(Fauna("lagartos_da_duna", "[rápido]", T(N(chacal, 3), N(abutre, 1)), T(V(chacal, 2), S(faiscas, 2)), 4));
            dunas.Fauna.Add(Fauna("faiscas_da_duna", "[enxame]", T(N(esc, 1), N(abutre, 1)), T(S(faiscas, 3)), 5, T(S(faiscas, 2), N(esc, 1)), 20f));

            // 11 · Ermos do Tambor Rubro: the ember hawks [rápido], the Escorpião-Rubro [tanque], the rolling ushabtis [controle].
            tambor.Fauna.Add(Fauna("gavioes_do_tambor", "[rápido]", T(N(gaviao, 3), N(esc, 1)), T(V(gaviao, 2), S(faiscas, 2)), 4, null, 24f));
            tambor.Fauna.Add(Fauna("escorpioes_rubros_dos_ermos", "[tanque]", T(N(esc, 3), N(ushabti, 1)), T(V(esc, 3), S(faiscas, 1)), 4, null, 26f));
            tambor.Fauna.Add(Fauna("pedras_rolantes", "[controle] [elite]", T(N(ushabti, 3)), T(V(ushabti, 2), S(mardagayl, 1)), 3, null, 32f));

            // 12 · Trono de Namrael: crystal scorpions, Namrael's guards (night variants), the salt mirrors (lanterns), Ubır in the towers.
            namrael.Fauna.Add(Fauna("escorpioes_cristal", "[tanque] [elite]", T(N(esc, 4)), T(V(esc, 4), S(ubir, 1)), 4, null, 28f));
            namrael.Fauna.Add(Fauna("espelhos_de_sal", "[distância]", T(N(lanterna, 3), N(esc, 1)), T(V(lanterna, 2), S(ubir, 1)), 3, null, 28f));
            namrael.Fauna.Add(Fauna("guardas_de_namrael", "[elite] [rápido]", T(N(esc, 2), N(gaviao, 2)), T(V(esc, 2), V(gaviao, 1), S(faiscas, 1)), 4, null, 30f));
        }
    }
}
