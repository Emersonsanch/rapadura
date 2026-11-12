using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 3 — Serra de Kaggen as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10, maps 13–18), no scene
    /// change between the Vila de Litema and the areas around it:
    /// <list type="bullet">
    /// <item>the <b>Vila de Litema</b> at the plaza (0, 0), near the east side, with the hand-made <b>Trilha dos Rondavéis</b>
    /// (13, nv 9–11) embedded round it on the golden grass slopes; around it the <b>Terraços de Litema</b> (farming terraces,
    /// gate to 08 on the south edge), the <b>Encosta do Vento</b> (aloes, gate to the Zarafshan on the east edge), the
    /// <b>Capinzal dos Baobás</b> and the <b>Torrentes de Qacha</b> (the meltwater torrent and its bridges), all nv 10–14;</item>
    /// <item>north-east, the hand-made <b>Passo do Hahya</b> (14, nv 10–13, wool cloak) embedded with its Boca da Mina do
    /// Mihr, its torrent running on south through the aloe slope, and the gallery gate to the Grutas do Limiar (19);</item>
    /// <item>the in-between areas, harder the farther from Litema: <b>Pastos dos Currais</b> (13–17, dry-stone kraals and
    /// rondavels), <b>Passos Estreitos</b> (15–20, narrow passes between rock walls), <b>Escarpas de Arenito</b> (18–24) and
    /// the <b>Campos de Neve</b> (22–28) under the summit;</item>
    /// <item>the far north and west, each with its outpost: <b>Abrigos Pintados</b> (15, 26–32, Abrigo do Eland),
    /// <b>Muralhas de Pedra Seca</b> (16, 28–34, Recinto Curvo), <b>Salões de Bronze de Napirasu</b> (17, 30–38, pickaxe and
    /// torch; the dwarves' halls, return city of 17–18) and the <b>Estela Alta</b> and <b>Cume dos Mazans</b> (18, 36–44,
    /// blizzard resistance: the snowy peak in the mist).</item>
    /// </list>
    /// Monsters: the mountain beasts (stone ushabti, crested hawk, potoo, Pedra que Rola, Neblina do Vento Ruim, Babuíno
    /// Apagado) with the wolf, boar and bat swarm, and the
    /// night species of <see cref="NightVariantRules"/>. Maps 15–18 have no hand-made layout of their own (they are walked
    /// into here; the Barca does not reach them yet).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string KaggenId = "overworld_kaggen";

        public const string AbrigosId = "map_015_abrigos";
        public const string PedraSecaId = "map_016_pedra_seca";
        public const string NapirasuId = "map_017_napirasu";
        public const string MazansId = "map_018_mazans";

        /// <summary>[distância] of the Serra (Mapas100 13): the crested hawk throws its feather darts from the ledges.</summary>
        private const string KaggenHawk = "enemy_mountain_crested_hawk";

        /// <summary>The Serra's own beasts (Docs/WorldBible_Maniqueismo.md §3.2.1): the Pedra que Rola [controle], the Neblina do
        /// Vento Ruim [controle] and the Babuíno Apagado [rápido].</summary>
        private const string KaggenRollingStone = "enemy_mountain_rolling_stone";
        private const string KaggenMist = "enemy_mountain_ill_wind_mist";
        private const string KaggenBaboon = "enemy_mountain_baboon";

        public static readonly Vector2 LitemaOffset = Vector2.zero;
        public static readonly Vector2 HahyaOffset = new Vector2(475f, 475f);
        public static readonly Vector2 AbrigoOffset = new Vector2(150f, 1300f);

        /// <summary>O Grande Borrão's arena (15, map boss): the stone ring under the sandstone wall, by the trail west of the Abrigo do Eland.</summary>
        public static readonly Vector2 GrandeBorraoArena = new Vector2(-267f, 1335f);

        /// <summary>O Fole Apagado's arena (17, map boss): the stone ring in the Salões de Napirasu, by the trail down to the Great Forge.</summary>
        public static readonly Vector2 FoleApagadoArena = new Vector2(-1384f, -228f);
        public static readonly Vector2 RecintoOffset = new Vector2(-900f, 1300f);
        public static readonly Vector2 SaloesOffset = new Vector2(-1500f, -300f);
        public static readonly Vector2 EstelaOffset = new Vector2(-1500f, 500f);

        /// <summary>The summit: Mazan's ring of stones on the top of the Serra.</summary>
        public static readonly Vector2 MazanRing = new Vector2(-1500f, 1240f);

        private static OverworldLayout Kaggen()
        {
            OverworldLayout w = NewRegion(KaggenId, MapRegion.SerraDeKaggen, Rect.MinMaxRect(-1700f, -900f, 700f, 1500f));
            w.World.Biome = BiomeType.Mountain;
            const string rondaveis = MapLayouts.RondaveisId;
            const string hahya = MapLayouts.HahyaId;
            const string mountain = MapRegionTypes.Mountain;

            // ---------------- Zones (tile the whole region, no overlap) ----------------
            OverworldZone zTrilha = Zone(w, "trilha_dos_rondaveis", rondaveis, null, -250f, -250f, 250f, 250f, OverworldTerrain.GoldenSlopes, 9, 11, 8,
                "Rondavel Trail", "Trilha dos Rondavéis", mountain, spacing: 80f);
            OverworldZone zTerracos = Zone(w, "terracos_de_litema", null, rondaveis, -250f, -900f, 250f, -250f, OverworldTerrain.FarmTerraces, 10, 12, 8,
                "Litema Terraces", "Terraços de Litema", mountain, spacing: 85f);
            OverworldZone zEncosta = Zone(w, "encosta_do_vento", null, rondaveis, 250f, -900f, 700f, 250f, OverworldTerrain.AloeSlopes, 10, 12, 8,
                "Windy Aloe Slope", "Encosta do Vento", mountain, spacing: 90f);
            OverworldZone zBaobas = Zone(w, "capinzal_dos_baobas", null, rondaveis, -250f, 250f, 250f, 700f, OverworldTerrain.BaobabGrass, 10, 13, 9,
                "Baobab Grassland", "Capinzal dos Baobás", MapRegionTypes.Plain, spacing: 90f);
            OverworldZone zTorrentes = Zone(w, "torrentes_de_qacha", null, hahya, -550f, -900f, -250f, 700f, OverworldTerrain.Torrents, 11, 14, 9,
                "Qacha Torrents", "Torrentes de Qacha", mountain, spacing: 90f);
            OverworldZone zHahya = Zone(w, "passo_do_hahya", hahya, null, 250f, 250f, 700f, 700f, OverworldTerrain.HighPass, 10, 13, 9,
                "Hahya's Pass", "Passo do Hahya", mountain, MapRequirementTags.WoolCloak, 85f);
            OverworldZone zPastos = Zone(w, "pastos_dos_currais", null, rondaveis, -900f, -900f, -550f, 700f, OverworldTerrain.Kraals, 13, 17, 11,
                "Kraal Pastures", "Pastos dos Currais", mountain, spacing: 95f);
            OverworldZone zPassos = Zone(w, "passos_estreitos", null, hahya, -900f, 700f, 700f, 1100f, OverworldTerrain.NarrowPass, 15, 20, 13,
                "Narrow Passes", "Passos Estreitos", mountain, spacing: 100f);
            OverworldZone zEscarpas = Zone(w, "escarpas_de_arenito", null, AbrigosId, -1300f, -900f, -900f, 700f, OverworldTerrain.SandstoneScarp, 18, 24, 16,
                "Sandstone Scarps", "Escarpas de Arenito", mountain, spacing: 100f);
            OverworldZone zAbrigos = Zone(w, "abrigos_pintados", AbrigosId, null, -550f, 1100f, 700f, 1500f, OverworldTerrain.PaintedShelters, 26, 32, 24,
                "Painted Shelters", "Abrigos Pintados", mountain, spacing: 100f);
            OverworldZone zPedraSeca = Zone(w, "muralhas_pedra_seca", PedraSecaId, null, -1300f, 1100f, -550f, 1500f, OverworldTerrain.DryStone, 28, 34, 26,
                "Drystone Walls", "Muralhas de Pedra Seca", MapRegionTypes.Ruins, spacing: 100f);
            OverworldZone zNeve = Zone(w, "campos_de_neve", null, MazansId, -1300f, 700f, -900f, 1100f, OverworldTerrain.Snowfield, 22, 28, 20,
                "Kaggen Snowfields", "Campos de Neve", mountain, spacing: 100f);
            OverworldZone zSaloes = Zone(w, "saloes_de_napirasu", NapirasuId, null, -1700f, -900f, -1300f, 300f, OverworldTerrain.BronzeHalls, 30, 38, 28,
                "Napirasu's Bronze Halls", "Salões de Bronze de Napirasu", mountain, MapRequirementTags.PickaxeTorch, 100f);
            OverworldZone zEstela = Zone(w, "estela_alta", MazansId, null, -1700f, 300f, -1300f, 700f, OverworldTerrain.StelaPlateau, 36, 40, 34,
                "High Stela", "Estela Alta", mountain, MapRequirementTags.BlizzardResistance, 100f);
            OverworldZone zCume = Zone(w, "cume_dos_mazans", MazansId, null, -1700f, 700f, -1300f, 1500f, OverworldTerrain.Summit, 38, 44, 36,
                "Mazan Summit", "Cume dos Mazans", mountain, MapRequirementTags.BlizzardResistance, 100f);

            AddKaggenFauna(zTrilha, zTerracos, zEncosta, zBaobas, zTorrentes, zHahya, zPastos, zPassos, zEscarpas, zAbrigos, zPedraSeca, zNeve, zSaloes,
                zEstela, zCume);

            // ---------------- The hand-made maps, embedded; the outposts of 15–18 ----------------
            MapLayout rondaveisSrc = MapLayouts.For(rondaveis);
            w.Vila = Embed(w, rondaveisSrc, LitemaOffset, withProps: false, authored: false);
            w.Vila.IsReturnCity = true;
            MapLayout hahyaSrc = MapLayouts.For(hahya);
            Embed(w, hahyaSrc, HahyaOffset, withProps: true, withRivers: false);

            Place(w, AbrigosId, Post("abrigo_do_eland", TownSwatch.Sandstone, TownSwatch.Thatch, TownRoofKind.Cone, TownShopDressing.Weaver, true), AbrigoOffset,
                PostSafe, 18f, false);
            Place(w, PedraSecaId, Post("recinto_curvo", TownSwatch.Stone, TownSwatch.Thatch, TownRoofKind.Cone, TownShopDressing.Ferraria, false), RecintoOffset,
                PostSafe, 18f, false);
            Place(w, NapirasuId, CityTownRecipes.BronzeHalls(), SaloesOffset, Rect.MinMaxRect(-20f, -19f, 21f, 20f), 24f, true);
            Place(w, MazansId, Post("estela_alta", TownSwatch.Stone, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Mercearia, false), EstelaOffset,
                PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Torrente de Qacha: meltwater from under the passes, north to south down its green gully; four bridges, a ford.
            Vector2 q0 = new Vector2(-410f, 690f), q1 = new Vector2(-380f, 470f), q2 = new Vector2(-430f, 240f), q3 = new Vector2(-370f, 10f);
            Vector2 q4 = new Vector2(-420f, -230f), q5 = new Vector2(-380f, -470f), q6 = new Vector2(-430f, -700f), q7 = new Vector2(-400f, -900f);
            MapRiver qacha = River(w, 7f, q0, q1, q2, q3, q4, q5, q6, q7);
            Vector2 qachaWest = On(q2, q3, 0.5f);
            Vector2 qachaSouth = On(q4, q5, 0.5f);
            Vector2 qachaNorth = On(q0, q1, 0.6f);
            Crossing(qacha, qachaWest, 4.5f, true);
            Crossing(qacha, qachaSouth, 4.5f, true);
            Crossing(qacha, qachaNorth, 4f, true);
            Crossing(qacha, On(q6, q7, 0.4f), 5f, false);

            // The Hahya torrent (its middle is the hand-made pass): from the spring under the passes down the aloe slope to the
            // south edge, a bridge on the road to the Zarafshan.
            var torrent = new MapRiver { Width = 7f };
            torrent.Points.AddRange(new[] { new Vector2(310f, 640f), new Vector2(345f, 560f) });
            if (hahyaSrc != null && hahyaSrc.Rivers.Count > 0)
            {
                foreach (Vector2 p in hahyaSrc.Rivers[0].Points)
                {
                    torrent.Points.Add(p + HahyaOffset);
                }

                foreach (MapCrossing crossing in hahyaSrc.Rivers[0].Crossings)
                {
                    torrent.Crossings.Add(new MapCrossing { At = crossing.At + HahyaOffset, Width = crossing.Width, IsBridge = crossing.IsBridge });
                }
            }

            Vector2 t0 = new Vector2(620f, 40f), t1 = new Vector2(600f, -200f);
            torrent.Points.AddRange(new[] { new Vector2(630f, 340f), new Vector2(650f, 200f), t0, t1, new Vector2(640f, -500f), new Vector2(600f, -900f) });
            Vector2 eastBridge = On(t0, t1, 140f / 240f);
            Crossing(torrent, eastBridge, 4.5f, true);
            Crossing(torrent, new Vector2(640f, -500f), 5f, false);
            w.World.Rivers.Add(torrent);

            // Tarns: the meltwater pond under the passes, a lake on the Estela plateau, one in the kraal pastures.
            River(w, 20f, new Vector2(-470f, 1000f), new Vector2(-420f, 1030f));
            River(w, 24f, new Vector2(-1600f, 620f), new Vector2(-1560f, 640f));
            River(w, 16f, new Vector2(-760f, -700f), new Vector2(-720f, -730f));

            // ---------------- Gates out of the region ----------------
            Gate(w, "exit_zarafshan", MapExitKind.Map, MapCatalog.PomarId, Rect.MinMaxRect(697f, -108f, 700f, -92f), new Vector2(691f, -100f), Vector2.left);
            Gate(w, "exit_artifices", MapExitKind.Map, MapLayouts.ArtificesId, Rect.MinMaxRect(-8f, -900f, 8f, -897f), new Vector2(0f, -891f), Vector2.up);
            Gate(w, "exit_lago_limiar", MapExitKind.Map, MapLayouts.LagoLimiarId, Rect.MinMaxRect(697f, 439f, 700f, 455f), new Vector2(691f, 447f), Vector2.left);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(148f, -32f, 152f, -28f), new Vector2(150f, -40f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, 150f, -30f, 0f, 0.45f);

            // ---------------- Trails ----------------
            // Litema's roads out (they start where the Trilha dos Rondavéis' trails ended).
            Vector2 westFork = new Vector2(-180f, 20f);
            PathVia(w, 4f, new Vector2(-100f, 0f), westFork, new Vector2(-280f, 60f), qachaWest, new Vector2(-560f, 110f), new Vector2(-760f, 60f),
                new Vector2(-1000f, 0f), new Vector2(-1200f, -120f), SaloesOffset + new Vector2(24f, 0f));
            Path(w, 4f, -50f, -160f, -40f, -420f, 0f, -650f, 0f, -900f);
            PathVia(w, 4f, new Vector2(100f, -100f), new Vector2(220f, -108f), new Vector2(440f, -104f), eastBridge, new Vector2(700f, -100f));
            Path(w, 3f, 150f, -40f, 160f, -105f);
            // To the Passo do Hahya and on to the Limiar gate.
            Vector2 hahyaWest = Port(hahyaSrc, HahyaOffset, rondaveis, MapExitKind.Map, new Vector2(375f, 475f));
            Vector2 hahyaSouth = Port(hahyaSrc, HahyaOffset, null, MapExitKind.WorldMap, new Vector2(475f, 375f));
            Vector2 hahyaEast = Port(hahyaSrc, HahyaOffset, MapLayouts.LagoLimiarId, MapExitKind.Map, new Vector2(575f, 447f));
            PathVia(w, 3.5f, new Vector2(220f, -108f), new Vector2(260f, 120f), new Vector2(320f, 330f), hahyaWest);
            PathVia(w, 3.5f, hahyaSouth, new Vector2(470f, 150f), new Vector2(440f, -104f));
            PathVia(w, 3.5f, hahyaEast, new Vector2(700f, hahyaEast.y));
            // The terraces and the south-west trail over the Qacha to the kraals, the scarps and Napirasu.
            Path(w, 3f, -40f, -420f, 230f, -430f);
            Path(w, 3f, 0f, -650f, -220f, -640f);
            PathVia(w, 3.5f, new Vector2(-40f, -420f), new Vector2(-230f, -380f), qachaSouth, new Vector2(-600f, -380f), new Vector2(-900f, -500f),
                new Vector2(-1200f, -120f));
            // North: the baobabs, the narrow passes (bridge over the Qacha's head), the shelters, the walls, the snow, the summit.
            Vector2 passFork = new Vector2(60f, 850f);
            PathVia(w, 3.5f, westFork, new Vector2(-140f, 200f), new Vector2(-60f, 420f), new Vector2(0f, 700f), passFork, new Vector2(140f, 1100f),
                AbrigoOffset + new Vector2(0f, -24f));
            PathVia(w, 3.5f, AbrigoOffset + new Vector2(24f, 0f), new Vector2(420f, 1330f), new Vector2(650f, 1300f));
            PathVia(w, 3.5f, AbrigoOffset + new Vector2(-24f, 0f), new Vector2(-200f, 1320f), new Vector2(-500f, 1300f), RecintoOffset + new Vector2(24f, 0f));
            PathVia(w, 3.5f, new Vector2(-140f, 200f), new Vector2(-300f, 420f), qachaNorth, new Vector2(-560f, 600f), new Vector2(-700f, 700f));
            PathVia(w, 3.5f, passFork, new Vector2(-220f, 900f), new Vector2(-600f, 880f), new Vector2(-900f, 900f),
                new Vector2(-1150f, 950f), new Vector2(-1350f, 1050f), MazanRing + new Vector2(0f, -26f));
            Path(w, 3f, -900f, 900f, -900f, 1276f);
            PathVia(w, 3f, MazanRing + new Vector2(0f, -26f), new Vector2(-1480f, 850f), EstelaOffset + new Vector2(0f, 24f));
            Path(w, 3.5f, -760f, 60f, -720f, 400f, -700f, 700f, -600f, 880f);
            PathVia(w, 3.5f, new Vector2(-1000f, 0f), new Vector2(-1100f, 300f), new Vector2(-1250f, 450f), EstelaOffset + new Vector2(24f, 0f));
            Path(w, 3f, 320f, 330f, 120f, 520f, -60f, 420f);
            Path(w, 3f, -1600f, -560f, -1524f, -300f);
            Path(w, 3f, 600f, 1300f, 620f, 1440f);

            // ---------------- Landmarks ----------------
            Landmark(w, MapLandmarkKind.StoneRing, MazanRing.x, MazanRing.y, 0f, 1.3f);
            Landmark(w, MapLandmarkKind.StoneRing, -760f, 1230f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.StoneRing, -1080f, -620f, 0f, 0.7f);
            Landmark(w, MapLandmarkKind.StoneRing, GrandeBorraoArena.x, GrandeBorraoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, FoleApagadoArena.x, FoleApagadoArena.y, 0f, 1.1f);
            Sign(w, westFork, new Vector2(-280f, 60f), 4f, 0f);
            Sign(w, new Vector2(220f, -108f), new Vector2(440f, -104f), 4f, 20f);
            Sign(w, new Vector2(-40f, -420f), new Vector2(-230f, -380f), 4f, 0f);
            Sign(w, passFork, new Vector2(140f, 1100f), 3.5f, -20f);
            Sign(w, new Vector2(-1000f, 0f), new Vector2(-1200f, -120f), 4f, 30f);
            Sign(w, new Vector2(-900f, 900f), new Vector2(-1150f, 950f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -560f, 140f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 30f, -680f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.Campfire, 660f, -60f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 660f, 480f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -1180f, -90f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -1120f, 960f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 30f, 870f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -24f, -880f, 0f, 0.8f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            TrimSteepClearings(w);
            return w;
        }

        /// <summary>A signpost on the edge of the trail from <paramref name="at"/> towards <paramref name="toward"/> (it stands on the trail's level).</summary>
        private static void Sign(OverworldLayout w, Vector2 at, Vector2 toward, float width, float yaw)
        {
            Vector2 d = (toward - at).normalized;
            Vector2 p = at + d * 3f + new Vector2(-d.y, d.x) * (width * 0.5f + 0.4f);
            Landmark(w, MapLandmarkKind.Signpost, p.x, p.y, yaw);
        }

        /// <summary>Trail edge to clearing edge (m) the Serra's generated clearings keep: their plateau ramp never tilts a trail.</summary>
        public const float ClearingTrailGap = 12.5f;

        /// <summary>
        /// The mountain's relief is strong: a generated clearing whose plateau ramp would reach a trail (tilting it across) or
        /// that would sit on a valley side or a town's ramp (its ground no longer level) is dropped. The hand-made 13 and 14
        /// keep theirs. The relief is rebuilt afterwards from the clearings that stay.
        /// </summary>
        private static void TrimSteepClearings(OverworldLayout w)
        {
            var drop = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                if (!IsGenerated(w, spawn))
                {
                    continue;
                }

                foreach (MapPath path in w.World.Paths)
                {
                    if (MapLayoutRules.DistanceToSegment(spawn.Center, path.A, path.B) < path.Width * 0.5f + spawn.Radius + ClearingTrailGap)
                    {
                        drop.Add(spawn.Id);
                        break;
                    }
                }
            }

            RemoveSpawns(w, drop);
            OverworldRelief relief = OverworldRelief.For(w);
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                if (!IsGenerated(w, spawn))
                {
                    continue;
                }

                float h0 = relief.SurfaceAt(spawn.Center);
                for (int k = 0; k < 16; k++)
                {
                    float a = k * Mathf.PI / 8f;
                    Vector2 p = spawn.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * spawn.Radius * 0.8f;
                    if (Mathf.Abs(relief.SurfaceAt(p) - h0) > 0.3f)
                    {
                        drop.Add(spawn.Id);
                        break;
                    }
                }
            }

            RemoveSpawns(w, drop);
            w.ReliefCache = null;
        }

        private static bool IsGenerated(OverworldLayout w, MapSpawnZone spawn)
        {
            return w.ZoneOfSpawn.TryGetValue(spawn.Id, out OverworldZone zone) && spawn.Id.StartsWith(zone.Id + "_");
        }

        private static void RemoveSpawns(OverworldLayout w, HashSet<string> ids)
        {
            if (ids.Count == 0)
            {
                return;
            }

            w.World.Zones.RemoveAll(s => ids.Contains(s.Id));
            foreach (string id in ids)
            {
                w.ZoneOfSpawn.Remove(id);
            }
        }

        // ------------------------------------------------------------------
        // The outposts of 15–18 (no hand-made layout: the Rondavel style of the town kit, like the Boca da Mina)
        // ------------------------------------------------------------------

        private static readonly Rect PostSafe = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

        /// <summary>A Serra outpost: a workshop at the back of the plaza, two rondavels, the Cais da Barca, lamps and crates.</summary>
        private static CityTownRecipe Post(string id, TownSwatch wall, TownSwatch roof, TownRoofKind roofKind, TownShopDressing dressing, bool rockArt)
        {
            var posto = new CityTownRecipe
            {
                CityId = id, Region = BiomeType.Mountain, PlazaRadius = 4.5f, StreetWidth = 3.5f, SidewalkWidth = 0.8f, WalkHalfExtent = 16f,
                EntryArm = CityArm.West, AutoYards = false, Backdrop = false, NightLights = 4,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_oficina", Kind = CityLotKind.Workshop, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down, Width = 7f, Depth = 5f,
                Porch = 1.4f,
                Spec = new TownBuildingSpec
                {
                    WallColor = wall, TrimColor = TownSwatch.LimeWhite, RoofColor = roof, JoineryColor = TownSwatch.WoodDark, AccentColor = TownSwatch.ClothIndigo,
                    SignColor = TownSwatch.ClothYellow, Roof = roofKind, Dressing = dressing, WallHeight = 3f, Chimney = dressing == TownShopDressing.Ferraria,
                },
            });
            posto.Lots.Add(new CityLotRecipe { Id = id + "_cais", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe { Id = id + "_casa1", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f });
            posto.Lots.Add(new CityLotRecipe { Id = id + "_casa2", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 9f, Width = 5f, Depth = 4.4f, Porch = 1f });
            posto.PlazaProps.Add(Prop(CityPropKind.Crates, -3f, -6f, 20f));
            posto.PlazaProps.Add(Prop(CityPropKind.Jugs, 3.2f, -5.6f, 40f));
            posto.PlazaProps.Add(Prop(CityPropKind.HayBale, -8.5f, -7.5f, 65f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            if (rockArt)
            {
                posto.PlazaProps.Add(Prop(CityPropKind.RockArt, -9f, 6f, CityGeometry.YawFacing(Vector2.right)));
            }
            else
            {
                posto.PlazaProps.Add(Prop(CityPropKind.Stela, -9f, 6.5f, CityGeometry.YawFacing(Vector2.down), 0.8f));
            }

            return posto;
        }

        private static CityProp Prop(CityPropKind kind, float x, float z, float yaw, float scale = 1f)
        {
            return new CityProp { Kind = kind, Position = new Vector2(x, z), Yaw = yaw, Scale = scale };
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only; Mapas100 13–18 archetypes)
        // ------------------------------------------------------------------

        private static void AddKaggenFauna(OverworldZone trilha, OverworldZone terracos, OverworldZone encosta, OverworldZone baobas, OverworldZone torrentes,
            OverworldZone hahya, OverworldZone pastos, OverworldZone passos, OverworldZone escarpas, OverworldZone abrigos, OverworldZone pedraSeca,
            OverworldZone neve, OverworldZone saloes, OverworldZone estela, OverworldZone cume)
        {
            NightSpecies aveGraveto = NightVariantRules.MaeDaLua;

            // Trilha dos Rondavéis (13): the Ushabti de Pedra [tanque], the Pedra que Rola [controle] and the hawk on the ledges;
            // the Babuíno Apagado [rápido] in the tall grass; at night the Ave-Graveto de Omöl and the smoke owl.
            trilha.Fauna.Add(Fauna("pedras_vivas", "[tanque] [controle] [distância]", T(N(Golem, 2), N(KaggenRollingStone, 2), N(KaggenHawk, 1)),
                T(V(Golem, 2), V(KaggenRollingStone, 1), S(aveGraveto, 1)), 3, null, 28f));
            trilha.Fauna.Add(Fauna("capim_alto", "[rápido]", T(N(KaggenBaboon, 3), N(Boar, 1)), T(V(KaggenBaboon, 3), S(NightVariantRules.RasgaMortalha, 1)), 4,
                T(N(KaggenBaboon, 3), V(KaggenBaboon, 1)), 22f));

            // Terraços de Litema: boars in the sorghum, ushabtis on the retaining walls.
            terracos.Fauna.Add(Fauna("sorgo", "[rápido] [tanque]", T(N(Boar, 3), N(Wolf, 1)), T(S(NightVariantRules.Queixada, 2), V(Wolf, 1)), 4, null, 26f));
            terracos.Fauna.Add(Fauna("muretas", "[tanque]", T(N(Golem, 2), N(Potoo, 1)), T(V(Golem, 2), S(aveGraveto, 1)), 3, null, 30f));

            // Encosta do Vento: the crested hawks ride the wind [distância]; ushabtis among the aloes.
            encosta.Fauna.Add(Fauna("gavioes", "[distância]", T(N(KaggenHawk, 3), N(Wolf, 1)), T(V(KaggenHawk, 2), S(aveGraveto, 1)), 3));
            encosta.Fauna.Add(Fauna("aloes", "[tanque]", T(N(Golem, 3)), T(V(Golem, 2), S(NightVariantRules.RasgaMortalha, 1)), 3, null, 30f));

            // Capinzal dos Baobás: wolves and boars in the open grass, the Ave-Graveto (potoo) in the baobabs.
            baobas.Fauna.Add(Fauna("baobas", "[rápido]", T(N(Wolf, 2), N(Boar, 2)), T(V(Wolf, 2), S(NightVariantRules.Lobisomem, 1)), 4));
            baobas.Fauna.Add(Fauna("ave_graveto", "[distância]", T(N(Potoo, 3), N(KaggenHawk, 1)), T(S(aveGraveto, 2), V(Potoo, 1)), 3));

            // Torrentes de Qacha: wolves along the banks, bat swarms from the wet caves.
            torrentes.Fauna.Add(Fauna("margens", "[rápido]", T(N(Wolf, 3), N(KaggenHawk, 1)), T(V(Wolf, 3), S(NightVariantRules.Lobisomem, 1)), 4));
            torrentes.Fauna.Add(Fauna("grutas_da_torrente", "[enxame]", T(N(Bats, 3), N(Wolf, 1)), T(N(Bats, 2), S(NightVariantRules.MorcegoVampiro, 1)), 5, null, 22f));

            // Passo do Hahya (14): ushabtis [tanque], the Morcego da Mina [enxame], the hawk over the pass, the Neblina do Vento
            // Ruim [controle] rolling down from the snow.
            hahya.Fauna.Add(Fauna("passo", "[tanque] [distância]", T(N(Golem, 3), N(KaggenHawk, 1)), T(V(Golem, 2), S(aveGraveto, 1)), 3, null, 30f));
            hahya.Fauna.Add(Fauna("morcegos_da_mina", "[enxame] [rápido]", T(N(Bats, 3), N(Wolf, 1)), T(V(Bats, 2), S(NightVariantRules.MorcegoVampiro, 1)), 5, null, 22f));
            hahya.Fauna.Add(Fauna("neblina_do_passo", "[controle]", T(N(KaggenMist, 3), N(KaggenHawk, 1)), T(V(KaggenMist, 2), S(aveGraveto, 1)), 4, null, 26f));

            // Pastos dos Currais: the wild herd and the wolves round the kraals, hawks over the pasture.
            pastos.Fauna.Add(Fauna("currais", "[rápido]", T(N(Wolf, 3), N(Boar, 1)), T(V(Wolf, 3), S(NightVariantRules.Lobisomem, 1)), 4));
            pastos.Fauna.Add(Fauna("rebanho_bravo", "[tanque]", T(N(Boar, 3), N(Golem, 1)), T(S(NightVariantRules.Queixada, 2), V(Golem, 1)), 4, null, 28f));
            pastos.Fauna.Add(Fauna("gavioes_do_pasto", "[distância]", T(N(KaggenHawk, 3)), T(V(KaggenHawk, 2), S(aveGraveto, 1)), 3));

            // Passos Estreitos: ushabtis and hawks on the walls, the lynx (wolf) in the gaps, the Neblina do Vento Ruim at dusk.
            passos.Fauna.Add(Fauna("paredoes", "[tanque] [distância]", T(N(Golem, 2), N(KaggenHawk, 2)), T(V(Golem, 2), V(KaggenHawk, 1), S(aveGraveto, 1)), 4, null, 28f));
            passos.Fauna.Add(Fauna("lince", "[rápido] [controle]", T(N(EarlyBeasts.DimmedLynx, 3), N(KaggenMist, 1)), T(V(EarlyBeasts.DimmedLynx, 2), V(KaggenMist, 1), S(NightVariantRules.MorcegoVampiro, 1)), 4,
                T(N(Wolf, 2), N(KaggenMist, 2))));

            // Escarpas de Arenito: sandstone ushabtis, the black eagles (hawks) on the steps.
            escarpas.Fauna.Add(Fauna("arenito", "[tanque]", T(N(Golem, 3), N(Potoo, 1)), T(V(Golem, 2), S(NightVariantRules.RasgaMortalha, 1)), 3, null, 30f));
            escarpas.Fauna.Add(Fauna("aguias_pretas", "[distância]", T(N(KaggenHawk, 3), N(Potoo, 1)), T(V(KaggenHawk, 2), S(aveGraveto, 1)), 3));

            // Abrigos Pintados (15): the Borrão [rápido] (the wolf of shadow) and the Babuínos Apagados, thieves [enxame]; the
            // Ushabti de Arenito [tanque]; the black eagle [distância].
            abrigos.Fauna.Add(Fauna("borroes", "[rápido] [enxame]", T(N(EarlyBeasts.Smudge, 3), N(KaggenBaboon, 2)), T(V(EarlyBeasts.Smudge, 2), V(KaggenBaboon, 1), S(NightVariantRules.Lobisomem, 1)), 5));
            abrigos.Fauna.Add(Fauna("ushabti_de_arenito", "[tanque] [distância]", T(N(Golem, 3), N(KaggenHawk, 1)), T(V(Golem, 2), S(aveGraveto, 1)), 4, null, 30f));

            // Muralhas de Pedra Seca (16): the granite ushabtis [tanque] (and the lesser Mazan at night), the stone crows [distância].
            pedraSeca.Fauna.Add(Fauna("muralhas", "[tanque] [elite]", T(N(Golem, 3)), T(V(Golem, 3), S(NightVariantRules.Lobisomem, 1)), 4, null, 32f));
            pedraSeca.Fauna.Add(Fauna("gralhas_de_pedra", "[distância]", T(N(EarlyBeasts.StoneJackdaw, 3), N(KaggenHawk, 1)), T(V(EarlyBeasts.StoneJackdaw, 2), V(KaggenHawk, 1), S(aveGraveto, 1)), 4));
            pedraSeca.Fauna.Add(Fauna("lagartos", "[rápido]", T(N(Wolf, 3), N(Boar, 1)), T(V(Wolf, 3), S(NightVariantRules.Queixada, 1)), 4));

            // Campos de Neve: snow ushabtis and wolves, the Neblina do Vento Ruim [controle].
            neve.Fauna.Add(Fauna("neve", "[tanque] [rápido]", T(N(Golem, 3), N(Wolf, 1)), T(V(Golem, 2), S(NightVariantRules.Lobisomem, 1)), 4, null, 30f));
            neve.Fauna.Add(Fauna("neblina", "[controle] [distância]", T(N(KaggenMist, 2), N(KaggenHawk, 2)), T(V(KaggenMist, 1), S(aveGraveto, 2), V(KaggenHawk, 1)), 4));

            // Salões de Bronze (17): slag golems [tanque], ember bats [enxame].
            saloes.Fauna.Add(Fauna("escoria", "[tanque]", T(N(Golem, 3), N(Bats, 1)), T(V(Golem, 2), S(NightVariantRules.MorcegoVampiro, 1)), 4, null, 30f));
            saloes.Fauna.Add(Fauna("morcegos_de_brasa", "[enxame]", T(N(Bats, 3), N(Golem, 1)), T(V(Bats, 2), S(NightVariantRules.MorcegoVampiro, 1)), 5, null, 22f));

            // Estela Alta and the Cume dos Mazans (18): ice ushabtis and the lesser Mazans [elite] [tanque], the storm eagles
            // [distância], the Neblina do Vento Ruim [controle] over the summit; Mazan himself stays the world boss.
            estela.Fauna.Add(Fauna("pastores_perdidos", "[rápido]", T(N(Wolf, 3), N(KaggenHawk, 1)), T(V(Wolf, 2), S(NightVariantRules.Lobisomem, 1)), 4));
            estela.Fauna.Add(Fauna("ushabti_de_gelo", "[tanque]", T(N(Golem, 3)), T(V(Golem, 2), S(aveGraveto, 1)), 3, null, 32f));
            cume.Fauna.Add(Fauna("mazans", "[elite] [tanque]", T(N(Golem, 3)), T(V(Golem, 3), S(NightVariantRules.Lobisomem, 1)), 3, null, 34f));
            cume.Fauna.Add(Fauna("aguias_da_tempestade", "[distância]", T(N(KaggenHawk, 3), N(Potoo, 1)), T(V(KaggenHawk, 3), S(aveGraveto, 1)), 4));
            cume.Fauna.Add(Fauna("neblina_do_cume", "[controle] [distância]", T(N(KaggenMist, 3), N(KaggenHawk, 1)), T(V(KaggenMist, 2), S(aveGraveto, 1)), 4, null, 26f));
        }
    }
}
