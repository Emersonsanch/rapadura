using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// Flat colours of the NPC palette texture (one cell each, 8 x 9). Append only (a cell index is baked in the mesh UVs).
    /// Region rows follow Docs/WorldBible_Maniqueismo.md §2 (Zarafshan, Qocho, Litema, Lamparinas).
    /// </summary>
    public enum NpcSwatch
    {
        // Skin (6 tones: the Silk Road and the African highlands) + face
        SkinFair, SkinLight, SkinOlive, SkinTan, SkinBrown, SkinDeep, Blush, Mouth,
        // Hair and eyes
        HairBlack, HairDark, HairBrown, HairAuburn, HairGrey, HairWhite, Eye, EyeShine,
        // Zarafshan (Vila da Vó Edna): adobe apricot, ochre, walnut, poplar green, tile turquoise, pomegranate, indigo, suzani cream
        Apricot, Ochre, Walnut, PoplarGreen, Turquoise, Pomegranate, Indigo, Cream,
        // Qocho: red sandstone, rose rammed earth, golden sand, violet shade, vine green, old gold, saffron, desert blue
        Sandstone, Rose, SandGold, Violet, VineGreen, OldGold, Saffron, DesertBlue,
        // Litema: clay red, lime white, granite, straw, blanket blue, blanket yellow, protea pink, aloe green
        Clay, LimeWhite, Granite, Straw, BlanketBlue, BlanketYellow, Protea, Aloe,
        // Lamparinas: tuff rose, basalt, shadow indigo, lamp robe white, copper, moss, lilac, night violet
        Tuff, Basalt, ShadowIndigo, RobeWhite, Copper, Moss, Lilac, NightViolet,
        // Materials
        Leather, LeatherDark, Wood, WoodDark, Iron, Steel, Brass, Wicker,
        // Glow (emissive) + small accents
        LampGlow, CrystalGlow, Ember, SunGlow, HerbGreen, FlowerPink, FlowerYellow, Feather,
        // Face read from the iso camera: eye whites, lips/tongue that show on every skin tone, a deeper blush for dark
        // skin, grey brows for white hair, lash line; ivory and charcoal accents
        Sclera, Lip, Tongue, CheekRose, BrowGrey, Lash, Ivory, Charcoal,
        // Copas de Garajonay (elfos-da-mata, Docs/Mapas100.md §3.3): bark skin, laurel #3F6B3A, moss #8DB255, bark #6E4B32,
        // mist #DDE6E2, living water #7FD1C7 (glows a little), heather #E7B9D9, autumn-leaf hair
        SkinBark, Laurel, MossLeaf, Bark, Mist, MistWater, Heather, HairAutumn,
        // Taa Pwani (Swahili sailors and merchants, Docs/Mapas100.md região 5): coral lime #EFE7DA (kanzu white), teak #6B4A2E,
        // sea #2E9BB0, sail cloth #F2E6C9, laterite #B4583A, mother-of-pearl, kanga red, henna
        Coral, Teak, SeaBlue, Sail, Laterite, Pearl, KangaRed, Henna,
        // Tambor Rubro (orcs, Docs/Mapas100.md §3.3): olive-green and grey orc skin, ochre-red #A8452B, red sandstone
        // #C9774B, dyed hide #7A4A2A, the yellow cloth band #E8C547, war-camp bronze, ash
        SkinOrcOlive, SkinOrcGrey, OchreRed, RedSandstone, Hide, BannerYellow, Bronze, Ash,
        // Salões de Bronze (dwarves) and Mercado dos Túneis (kobolds), Docs/Mapas100.md §3.3: rust-ochre and slate-blue
        // smooth kobold scales, bright bronze #B07A3B, green copper #5E9C8A, rune light #FFE08A (glows), cold ember #8C3B2E,
        // bottle-glass green #7FBF8F and the bottled Ar Sereno #C9B8F0 (both glow a little)
        SkinKoboldRust, SkinKoboldSlate, BronzeBright, Verdigris, RuneGold, ColdEmber, GlassGreen, SereneAir,
        // Aratta (povo-touro) and Kirta (povo-do-mar), Docs/Mapas100.md §3.3: dark-brown and tawny aurochs coats, horn ivory,
        // bound reed #C8B27A, the ribbon blue #3F7CC0; turquoise and mother-of-pearl sea-folk skin, live coral #E8836B (glows a little)
        SkinAurochs, SkinAurochsTawny, Horn, Reed, Ribbon, SkinReef, SkinNacre, CoralLive,
        // Jhilpur (Delta de Jhilpur, Docs/Mapas100.md §2.1 and Docs/MetodologiaMapasERacas.md §5.1): turmeric #E3A92B,
        // sandalwood #D9B98C, lotus pink #E8A0B4, root green #4E6B3A, the Living Water #4FC3B0 (glows a little), murky water
        // #5F7F6E, aerial-root bark #6B5236, madder red #A33B32
        Turmeric, Sandalwood, Lotus, RootGreen, LivingWater, MurkWater, RootBark, Madder,
        // Palafitas de Tarchon (trols, Docs/Mapas100.md §3.3 and Docs/MetodologiaMapasERacas.md §6.5): moss-blue trol skin
        // #4F7A82 and the bark-green one #66704F, wet wood #5B4636, leaf #6F8F3F, lilac mist #B9A9D6, the will-o'-wisp
        // #D6E8FF (glows), rush #B9A55A and net twine #8E8466
        SkinTrollMoss, SkinTrollBark, WetWood, SwampLeaf, LilacMist, WillWisp, Rush, NetTwine,
        // Sangkent (Teto do Mundo, Docs/Mapas100.md §2.1 and map 36): yak wool #6E5A48, felt #E3D5BC, apricot blossom #F2B8A0,
        // the caravans' silk red #C0392B and blue #2E86AB, terrace stone #8C8173, juniper #5A3E2B and glacier ice #BFD9E8
        YakWool, Felt, ApricotBlossom, SilkRed, SilkBlue, PamirStone, Juniper, Glacier,
        // Mingcheng (Planície de Mingcheng, Docs/Mapas100.md §2.1 and map 41): lacquer red #B23A2E, imperial gold #D9A82B,
        // jade #5E9E7E, lantern paper #F4E3B5 (glows a little), grey brick #7D7F86, ink #2B2B30, celadon #9DC3B0, willow #8DB25A
        Lacquer, ImperialGold, Jade, LanternPaper, GreyBrick, Ink, Celadon, Willow,
        // Torres Brancas de Amanirenas (altos-elfos, Docs/Mapas100.md §3.3 and map 42): lime white #F3EFE6, pale gold #E3C46B,
        // sky blue #7FB2E5, silver leaf #A8C5B5, Meroë sandstone #C79E6B, the passed-down sunlight #FFF4C2 (glows), carnelian
        // #B5452E and ebony #3A2A22
        SpireLime, PaleGold, SkyBlue, SilverLeaf, MeroeSandstone, SpireLight, Carnelian, Ebony,
        // Acampamento de Soslan (centauros, Docs/Mapas100.md §3.3 and map 47): the horse coats bay #7A4A2C, grey #A8A39C and
        // dun #C2A06A, the dyed Nart felts red #8E2F2F and blue #2F5E8E, steppe felt #D8C9A8, Scythian gold #D4A93A, rawhide #6E4A2E
        HorseBay, HorseGrey, HorseDun, NartRed, NartBlue, SteppeFelt, ScythGold, Rawhide,
        // Fortim dos Gigantes-da-Colina (Docs/Mapas100.md §3.3 and map 49): the giants' ruddy #C98A6A and ochre #B08A5E skins,
        // hill earth #8A6A44, hill stone #9A948A, the good smoke #CFC6B8, the Living Fire's ember #E2763A (glows), hill grass
        // #7F9A4A and giant hide #5A3B26
        SkinGiantRuddy, SkinGiantOchre, HillEarth, HillStone, GoodSmoke, LivingEmber, HillGreen, GiantHide,
        // Arvoredo de Haripushpa (povo-tronco, Docs/Mapas100.md §3.3 and map 50): old bark skin #5E4A3A and young bark #8A7458,
        // trunk moss #6E8F4A, spring leaf #A9C96B, meltwater #86C9D6 (glows a little), catkin #F0D27A, larch #4F6B3A, sap amber #D9902E
        SkinBarkOld, SkinBarkYoung, TrunkMoss, SpringLeaf, MeltWater, Catkin, LarchGreen, SapAmber,
        // Farnkesh, os Ninhos de Pedra (Picos de Farnkesh, Docs/Mapas100.md map 54): lime #F1ECE2, painted red wood #9A3B2E,
        // gold #D9A82B, pinnacle sandstone #B58A60, cloud #EEF3F7, the high sky #8FB8E0, yak rope #8A7A5E, bell bronze #A87B3C
        NestLime, NestRed, NestGold, PinnacleSand, CloudWhite, HighSky, YakRope, BellBronze,
        // Vale dos Sátiros (sátiros, Docs/Mapas100.md §3.3 and map 53): the satyrs' warm #D9A47E and ruddy #B9785A skins, the goat
        // fur brown #6E4E34, tawny #A87E52 and black #3B302A, grape #5E3A6E, the Pactolus gold #E3B341, glade green #9CCB6A
        SkinSatyr, SkinSatyrRuddy, FurBrown, FurTawny, FurBlack, Grape, PactolusGold, GladeGreen,
    }

    /// <summary>
    /// The NPCs' palette texture (same trick as <see cref="EnemyModelPalette"/> and the town kit): every NPC mesh samples
    /// one tiny point-filtered texture of flat colour cells through its UVs, so all residents of the island share a single
    /// URP Lit material (one batch-friendly material, no per-NPC textures). The glow cells (lamps, crystals, embers, the
    /// sun pendant) light up through the emission map.
    /// </summary>
    public static class NpcPalette
    {
        public const int Columns = 8;
        public const int Rows = 24;
        private const int CellPixels = 4;

        private static readonly Color[] Colors =
        {
            // Skin + face
            new Color(0.96f, 0.82f, 0.72f), new Color(0.9f, 0.72f, 0.58f), new Color(0.8f, 0.6f, 0.44f), new Color(0.7f, 0.5f, 0.34f),
            new Color(0.52f, 0.34f, 0.22f), new Color(0.36f, 0.23f, 0.15f), new Color(0.93f, 0.52f, 0.5f), new Color(0.42f, 0.16f, 0.16f),
            // Hair + eyes
            new Color(0.09f, 0.08f, 0.09f), new Color(0.2f, 0.13f, 0.09f), new Color(0.38f, 0.24f, 0.13f), new Color(0.52f, 0.24f, 0.12f),
            new Color(0.62f, 0.6f, 0.58f), new Color(0.92f, 0.91f, 0.88f), new Color(0.1f, 0.07f, 0.07f), new Color(1f, 1f, 1f),
            // Zarafshan  #D9A066 #B7793F #6B4226 #7FA650 #3FA7A3
            new Color(0.85f, 0.63f, 0.4f), new Color(0.72f, 0.47f, 0.25f), new Color(0.42f, 0.26f, 0.15f), new Color(0.5f, 0.65f, 0.31f),
            new Color(0.25f, 0.65f, 0.64f), new Color(0.72f, 0.16f, 0.2f), new Color(0.2f, 0.22f, 0.45f), new Color(0.95f, 0.9f, 0.78f),
            // Qocho  #B5482F #D7A28A #E7C27D #6D4C7D #8DAA3A #C9A23F
            new Color(0.71f, 0.28f, 0.18f), new Color(0.84f, 0.64f, 0.54f), new Color(0.91f, 0.76f, 0.49f), new Color(0.43f, 0.3f, 0.49f),
            new Color(0.55f, 0.67f, 0.23f), new Color(0.79f, 0.64f, 0.25f), new Color(0.93f, 0.6f, 0.15f), new Color(0.22f, 0.34f, 0.58f),
            // Litema  #A0522D #EFE6D8 #8A8A84 #C8A85A #2F4F8F #E3B23C
            new Color(0.63f, 0.32f, 0.18f), new Color(0.94f, 0.9f, 0.85f), new Color(0.54f, 0.54f, 0.52f), new Color(0.78f, 0.66f, 0.35f),
            new Color(0.18f, 0.31f, 0.56f), new Color(0.89f, 0.7f, 0.24f), new Color(0.88f, 0.45f, 0.55f), new Color(0.42f, 0.6f, 0.42f),
            // Lamparinas  #C98F86 #3B3B44 #232046 #F2EEE6 #B06B3A
            new Color(0.79f, 0.56f, 0.53f), new Color(0.23f, 0.23f, 0.27f), new Color(0.14f, 0.13f, 0.28f), new Color(0.95f, 0.93f, 0.9f),
            new Color(0.69f, 0.42f, 0.23f), new Color(0.48f, 0.7f, 0.42f), new Color(0.7f, 0.6f, 0.86f), new Color(0.32f, 0.2f, 0.45f),
            // Materials
            new Color(0.5f, 0.32f, 0.19f), new Color(0.3f, 0.19f, 0.12f), new Color(0.58f, 0.41f, 0.24f), new Color(0.33f, 0.22f, 0.13f),
            new Color(0.34f, 0.35f, 0.39f), new Color(0.74f, 0.76f, 0.8f), new Color(0.85f, 0.66f, 0.3f), new Color(0.74f, 0.58f, 0.34f),
            // Glow + accents
            new Color(1f, 0.72f, 0.3f), new Color(0.62f, 0.9f, 1f), new Color(1f, 0.5f, 0.18f), new Color(1f, 0.86f, 0.4f),
            new Color(0.32f, 0.6f, 0.26f), new Color(0.95f, 0.55f, 0.7f), new Color(1f, 0.85f, 0.3f), new Color(0.86f, 0.84f, 0.8f),
            // Face + accents
            new Color(0.98f, 0.96f, 0.92f), new Color(0.78f, 0.3f, 0.32f), new Color(0.93f, 0.46f, 0.46f), new Color(0.78f, 0.36f, 0.4f),
            new Color(0.46f, 0.44f, 0.43f), new Color(0.05f, 0.035f, 0.035f), new Color(0.9f, 0.85f, 0.72f), new Color(0.18f, 0.18f, 0.2f),
            // Garajonay  bark skin #9C7A5B, #3F6B3A #8DB255 #6E4B32 #DDE6E2 #7FD1C7 #E7B9D9, autumn hair #B5652B
            new Color(0.61f, 0.48f, 0.36f), new Color(0.25f, 0.42f, 0.23f), new Color(0.55f, 0.7f, 0.33f), new Color(0.43f, 0.29f, 0.2f),
            new Color(0.87f, 0.9f, 0.89f), new Color(0.5f, 0.82f, 0.78f), new Color(0.91f, 0.73f, 0.85f), new Color(0.71f, 0.4f, 0.17f),
            // Taa Pwani  coral #EFE7DA, teak #6B4A2E, sea #2E9BB0, sail #F2E6C9, laterite #B4583A, pearl #E6E1EC, kanga #C0392B, henna #8E3B1F
            new Color(0.94f, 0.91f, 0.85f), new Color(0.42f, 0.29f, 0.18f), new Color(0.18f, 0.61f, 0.69f), new Color(0.95f, 0.9f, 0.79f),
            new Color(0.71f, 0.35f, 0.23f), new Color(0.9f, 0.88f, 0.93f), new Color(0.75f, 0.22f, 0.17f), new Color(0.56f, 0.23f, 0.12f),
            // Tambor Rubro  orc olive #6F7D4A, orc grey #7E827A, ochre-red #A8452B, sandstone #C9774B, hide #7A4A2A, band #E8C547, bronze #9C6B30, ash #5E5A55
            new Color(0.44f, 0.49f, 0.29f), new Color(0.49f, 0.51f, 0.48f), new Color(0.66f, 0.27f, 0.17f), new Color(0.79f, 0.47f, 0.29f),
            new Color(0.48f, 0.29f, 0.16f), new Color(0.91f, 0.77f, 0.28f), new Color(0.61f, 0.42f, 0.19f), new Color(0.37f, 0.35f, 0.33f),
            // Salões / Mercado  kobold rust #B0663A, kobold slate #5F7480, bronze #B07A3B, green copper #5E9C8A, rune #FFE08A, cold ember #8C3B2E, glass #7FBF8F, air #C9B8F0
            new Color(0.69f, 0.4f, 0.23f), new Color(0.37f, 0.45f, 0.5f), new Color(0.69f, 0.48f, 0.23f), new Color(0.37f, 0.61f, 0.54f),
            new Color(1f, 0.88f, 0.54f), new Color(0.55f, 0.23f, 0.18f), new Color(0.5f, 0.75f, 0.56f), new Color(0.79f, 0.72f, 0.94f),
            // Aratta / Kirta  aurochs #4A3022, tawny #8A5A34, horn #E2D3B0, reed #C8B27A, ribbon #3F7CC0, reef skin #7FC6C0, nacre skin #E3D6C4, coral #E8836B
            new Color(0.29f, 0.19f, 0.13f), new Color(0.54f, 0.35f, 0.2f), new Color(0.89f, 0.83f, 0.69f), new Color(0.78f, 0.7f, 0.48f),
            new Color(0.25f, 0.49f, 0.75f), new Color(0.5f, 0.78f, 0.75f), new Color(0.89f, 0.84f, 0.77f), new Color(0.91f, 0.51f, 0.42f),
            // Jhilpur  turmeric #E3A92B, sandalwood #D9B98C, lotus #E8A0B4, root green #4E6B3A, Living Water #4FC3B0, murky water #5F7F6E, root bark #6B5236, madder #A33B32
            new Color(0.89f, 0.66f, 0.17f), new Color(0.85f, 0.73f, 0.55f), new Color(0.91f, 0.63f, 0.71f), new Color(0.31f, 0.42f, 0.23f),
            new Color(0.31f, 0.76f, 0.69f), new Color(0.37f, 0.5f, 0.43f), new Color(0.42f, 0.32f, 0.21f), new Color(0.64f, 0.23f, 0.2f),
            // Tarchon  trol moss #4F7A82, trol bark #66704F, wet wood #5B4636, leaf #6F8F3F, lilac mist #B9A9D6, wisp #D6E8FF, rush #B9A55A, net twine #8E8466
            new Color(0.31f, 0.48f, 0.51f), new Color(0.4f, 0.44f, 0.31f), new Color(0.36f, 0.27f, 0.21f), new Color(0.44f, 0.56f, 0.25f),
            new Color(0.73f, 0.66f, 0.84f), new Color(0.84f, 0.91f, 1f), new Color(0.73f, 0.65f, 0.35f), new Color(0.56f, 0.52f, 0.4f),
            // Sangkent  yak wool #6E5A48, felt #E3D5BC, apricot blossom #F2B8A0, silk red #C0392B, silk blue #2E86AB, terrace stone #8C8173, juniper #5A3E2B, glacier #BFD9E8
            new Color(0.43f, 0.35f, 0.28f), new Color(0.89f, 0.84f, 0.74f), new Color(0.95f, 0.72f, 0.63f), new Color(0.75f, 0.22f, 0.17f),
            new Color(0.18f, 0.53f, 0.67f), new Color(0.55f, 0.51f, 0.45f), new Color(0.35f, 0.24f, 0.17f), new Color(0.75f, 0.85f, 0.91f),
            // Mingcheng  lacquer #B23A2E, imperial gold #D9A82B, jade #5E9E7E, lantern paper #F4E3B5, grey brick #7D7F86, ink #2B2B30, celadon #9DC3B0, willow #8DB25A
            new Color(0.7f, 0.23f, 0.18f), new Color(0.85f, 0.66f, 0.17f), new Color(0.37f, 0.62f, 0.49f), new Color(0.96f, 0.89f, 0.71f),
            new Color(0.49f, 0.5f, 0.53f), new Color(0.17f, 0.17f, 0.19f), new Color(0.62f, 0.76f, 0.69f), new Color(0.55f, 0.7f, 0.35f),
            // Torres Brancas  lime #F3EFE6, pale gold #E3C46B, sky blue #7FB2E5, silver leaf #A8C5B5, Meroë sandstone #C79E6B, sunlight #FFF4C2, carnelian #B5452E, ebony #3A2A22
            new Color(0.95f, 0.94f, 0.9f), new Color(0.89f, 0.77f, 0.42f), new Color(0.5f, 0.7f, 0.9f), new Color(0.66f, 0.77f, 0.71f),
            new Color(0.78f, 0.62f, 0.42f), new Color(1f, 0.96f, 0.76f), new Color(0.71f, 0.27f, 0.18f), new Color(0.23f, 0.16f, 0.13f),
            // Soslan  bay #7A4A2C, grey #A8A39C, dun #C2A06A, Nart red #8E2F2F, Nart blue #2F5E8E, steppe felt #D8C9A8, Scythian gold #D4A93A, rawhide #6E4A2E
            new Color(0.48f, 0.29f, 0.17f), new Color(0.66f, 0.64f, 0.61f), new Color(0.76f, 0.63f, 0.42f), new Color(0.56f, 0.18f, 0.18f),
            new Color(0.18f, 0.37f, 0.56f), new Color(0.85f, 0.79f, 0.66f), new Color(0.83f, 0.66f, 0.23f), new Color(0.43f, 0.29f, 0.18f),
            // Fortim  giant ruddy #C98A6A, giant ochre #B08A5E, hill earth #8A6A44, hill stone #9A948A, good smoke #CFC6B8, ember #E2763A, hill green #7F9A4A, giant hide #5A3B26
            new Color(0.79f, 0.54f, 0.42f), new Color(0.69f, 0.54f, 0.37f), new Color(0.54f, 0.42f, 0.27f), new Color(0.6f, 0.58f, 0.54f),
            new Color(0.81f, 0.78f, 0.72f), new Color(0.89f, 0.46f, 0.23f), new Color(0.5f, 0.6f, 0.29f), new Color(0.35f, 0.23f, 0.15f),
            // Arvoredo  old bark #5E4A3A, young bark #8A7458, trunk moss #6E8F4A, spring leaf #A9C96B, meltwater #86C9D6, catkin #F0D27A, larch #4F6B3A, sap amber #D9902E
            new Color(0.37f, 0.29f, 0.23f), new Color(0.54f, 0.45f, 0.35f), new Color(0.43f, 0.56f, 0.29f), new Color(0.66f, 0.79f, 0.42f),
            new Color(0.53f, 0.79f, 0.84f), new Color(0.94f, 0.82f, 0.48f), new Color(0.31f, 0.42f, 0.23f), new Color(0.85f, 0.56f, 0.18f),
            // Farnkesh  lime #F1ECE2, painted red #9A3B2E, gold #D9A82B, pinnacle sand #B58A60, cloud #EEF3F7, high sky #8FB8E0, yak rope #8A7A5E, bell bronze #A87B3C
            new Color(0.95f, 0.93f, 0.89f), new Color(0.6f, 0.23f, 0.18f), new Color(0.85f, 0.66f, 0.17f), new Color(0.71f, 0.54f, 0.38f),
            new Color(0.93f, 0.95f, 0.97f), new Color(0.56f, 0.72f, 0.88f), new Color(0.54f, 0.48f, 0.37f), new Color(0.66f, 0.48f, 0.24f),
            // Vale dos Sátiros  satyr #D9A47E, satyr ruddy #B9785A, fur brown #6E4E34, fur tawny #A87E52, fur black #3B302A, grape #5E3A6E, Pactolus gold #E3B341, glade green #9CCB6A
            new Color(0.85f, 0.64f, 0.49f), new Color(0.73f, 0.47f, 0.35f), new Color(0.43f, 0.31f, 0.2f), new Color(0.66f, 0.49f, 0.32f),
            new Color(0.23f, 0.19f, 0.16f), new Color(0.37f, 0.23f, 0.43f), new Color(0.89f, 0.7f, 0.25f), new Color(0.61f, 0.8f, 0.42f),
        };

        public static int Count => Colors.Length;

        /// <summary>Emission strength per swatch (multiplied onto its colour); 0 = not glowing.</summary>
        public static float GlowOf(NpcSwatch swatch)
        {
            switch (swatch)
            {
                case NpcSwatch.LampGlow: return 1f;
                case NpcSwatch.CrystalGlow: return 0.8f;
                case NpcSwatch.Ember: return 0.9f;
                case NpcSwatch.SunGlow: return 0.8f;
                case NpcSwatch.MistWater: return 0.35f;
                case NpcSwatch.RuneGold: return 0.8f;
                case NpcSwatch.GlassGreen: return 0.4f;
                case NpcSwatch.SereneAir: return 0.45f;
                case NpcSwatch.CoralLive: return 0.3f;
                case NpcSwatch.LivingWater: return 0.35f;
                case NpcSwatch.WillWisp: return 0.6f;
                case NpcSwatch.LanternPaper: return 0.35f;
                case NpcSwatch.SpireLight: return 0.6f;
                case NpcSwatch.LivingEmber: return 0.7f;
                case NpcSwatch.MeltWater: return 0.3f;
                default: return 0f;
            }
        }

        public static Color ColorOf(NpcSwatch swatch) => Colors[(int)swatch];

        /// <summary>UV at the centre of <paramref name="swatch"/>'s cell.</summary>
        public static Vector2 UvOf(NpcSwatch swatch)
        {
            int index = (int)swatch;
            return new Vector2((index % Columns + 0.5f) / Columns, (index / Columns + 0.5f) / Rows);
        }

        /// <summary>Swatch whose cell contains <paramref name="uv"/> (tests, debugging).</summary>
        public static NpcSwatch SwatchAt(Vector2 uv)
        {
            int column = Mathf.Clamp(Mathf.FloorToInt(uv.x * Columns), 0, Columns - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt(uv.y * Rows), 0, Rows - 1);
            return (NpcSwatch)(row * Columns + column);
        }

        private static Material _material;

        /// <summary>The one material every NPC shares (built once, kept for the session; never saved).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_material == null)
                {
                    _material = EnemyModelPalette.CreateMaterial(Bake("NpcPalette", false), Bake("NpcPaletteEmission", true));
                    _material.name = "NpcPalette";
                    _material.hideFlags = HideFlags.DontSave;
                }

                return _material;
            }
        }

        /// <summary>Destroys the shared material and its textures (editor cleanup before Play; rebuilt on next use).</summary>
        public static void DestroyMaterial()
        {
            if (_material == null)
            {
                _material = null;
                return;
            }

            DestroyTexture(_material, "_BaseMap");
            DestroyTexture(_material, "_EmissionMap");
            Object.DestroyImmediate(_material);
            _material = null;
        }

        private static void DestroyTexture(Material material, string property)
        {
            if (material.HasProperty(property))
            {
                Texture texture = material.GetTexture(property);
                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }

        private static Texture2D Bake(string textureName, bool emission)
        {
            int width = Columns * CellPixels;
            int height = Rows * CellPixels;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };

            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y / CellPixels) * Columns + x / CellPixels;
                    Color color = Colors[index];
                    if (emission)
                    {
                        color *= GlowOf((NpcSwatch)index);
                        color.a = 1f;
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
