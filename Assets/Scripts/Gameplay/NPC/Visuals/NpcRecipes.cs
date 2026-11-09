using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// One look per named resident of the Ilha da Aurora, chosen from their story (Docs/WorldBible_Maniqueismo.md §3.1, the
    /// city/quest texts and the concept sheets in Assets/Textures/NPCs), plus seeded generic villagers in their region's
    /// palette (§2). Every visual choice is tied to a line of lore in the comment above it; Docs/NPCs.md has the table.
    /// Ids are the new names in lower case; the old resident/dialogue ids (<c>cida</c>, <c>severino</c>, ...) resolve
    /// through <see cref="Resolve"/>, so no save or scene has to change.
    /// </summary>
    public static class NpcRecipes
    {
        public static readonly IReadOnlyList<NpcRecipe> Named = new List<NpcRecipe>
        {
            // ------------------------------------------------------------------
            // Vila da Vó Edna (Bosque do Zarafshan): apricot adobe, poplar green, pomegranate, tile turquoise
            // ------------------------------------------------------------------

            // Vó Edna, the family's anchor: short and round (the grandma who feeds everyone), patterned orange headscarf
            // and terracotta dress with an apron from the tandir, a cup of tea in both hands (she serves hortelã to the
            // cousins in quest 1.1) and the amber pendant glowing like the Cristal do Sol she keeps. Sips, slowly.
            new NpcRecipe
            {
                Id = "vo_edna", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Stout, Height = 0.92f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Kind, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairGrey,
                Headwear = NpcHeadwear.Kerchief, HeadColor = NpcSwatch.Saffron, HeadTrim = NpcSwatch.Pomegranate,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Sandstone, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.Cream, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.Pomegranate, LayerTrim = NpcSwatch.Saffron,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Cup, NpcProp.SunPendant },
                Gesture = NpcGesture.Sip,
                Story = "Âncora da família, chá de hortelã, guarda o Cristal do Sol",
            },

            // Mariam, the herbalist: an elder, calm "grandma of everyone" (Face Calm), silver hair under a moss-green hood,
            // linen apron, a wicker basket of herbs and pomegranates on the forearm and a mortar in the other hand (teas and
            // stamina elixirs; the first herb-gathering quests). Grinds slowly.
            new NpcRecipe
            {
                Id = "mariam", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.95f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinFair, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.PoplarGreen, HeadTrim = NpcSwatch.HerbGreen,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Walnut, Trim = NpcSwatch.PoplarGreen, Sleeves = NpcSwatch.PoplarGreen, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.Cream, LayerTrim = NpcSwatch.HerbGreen,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.HerbBasket, NpcProp.MortarPestle },
                Gesture = NpcGesture.Grind,
                Story = "Ervateira calma, cheira a hortelã, chás e missões de ervas",
            },

            // Mestre Adamas, the smith (named after the warrior of light who treads on the monster): broad shoulders, chest
            // out, a laughing face (he laughs out loud), long braided beard with brass rings and a forge bandana, bare
            // forearms and leather gloves, heavy leather apron and tongs on the belt; "my friend" the hammer in his hand,
            // its head glowing with a refining ember. Taps the hammer down in the forge's rhythm.
            new NpcRecipe
            {
                Id = "adamas", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.02f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinLight, Hands = NpcSwatch.LeatherDark, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBrown,
                Beard = NpcBeard.LongBraided, Headwear = NpcHeadwear.Bandana, HeadColor = NpcSwatch.Ochre,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Ochre, Trim = NpcSwatch.Walnut, Sleeves = NpcSwatch.SkinLight, Trousers = NpcSwatch.DesertBlue, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.Brass,
                Props = new[] { NpcProp.Hammer, NpcProp.Tongs },
                Gesture = NpcGesture.Hammer,
                Story = "Ferreiro forte, ri alto, chama o martelo de \"meu amigo\"",
            },

            // Mestre Omóforo, the carpenter (the one who carries the earth on his shoulders): tall and broad, a log on the
            // right shoulder that he never complains about (calm face, steady slow bob under the weight), the other hand
            // at ease on the hip; poplar-green work tunic and a cream sash.
            new NpcRecipe
            {
                Id = "omoforo", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.1f,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Calm, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Walnut, HeadTrim = NpcSwatch.Ochre,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.PoplarGreen, Trim = NpcSwatch.Walnut, Sleeves = NpcSwatch.PoplarGreen, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Cream, LayerTrim = NpcSwatch.Ochre,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Log },
                Gesture = NpcGesture.Carry,
                Story = "Marceneiro que carrega toras nos ombros e nunca reclama",
            },

            // Mestra Narisah, the seamstress (the Messenger who shows herself beautiful): slim and tall, proud posture, a
            // turquoise kaftan with gold bands (Sogdian embroidery), a neat bun with a gold band, a bolt of pomegranate
            // silk on the forearm and spools of thread at the sash, hand on the hip. Tilts her head, admiring.
            new NpcRecipe
            {
                Id = "narisah", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.05f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Bright, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.OldGold,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Turquoise, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.Turquoise, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Pomegranate, LayerTrim = NpcSwatch.OldGold,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HipRest, Props = new[] { NpcProp.FabricBolt, NpcProp.Spools },
                Gesture = NpcGesture.Preen,
                Story = "Costureira elegante e vaidosa do bom gosto, bordados sogdianos",
            },

            // Zaku, the grocer: thrifty and careful (slim, a little hunched over his coins), indigo quilted chapan and a
            // doppa skullcap, a goatee, a coin purse at the belt and a balance scale held up: he counts everything twice
            // (little bouncing counts of the hand).
            new NpcRecipe
            {
                Id = "zaku", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.96f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Calm, Hair = NpcHair.Short, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Indigo, HeadTrim = NpcSwatch.Cream,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Indigo, Trim = NpcSwatch.Cream, Sleeves = NpcSwatch.Indigo, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Belt,
                RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Scale, NpcProp.CoinPurse },
                Gesture = NpcGesture.Count,
                Story = "Merceeiro econômico, confere cada moeda duas vezes",
            },

            // Nana, the market lady (Nanaia, loved in the Silk Road towns): round and cheerful, bright face, auburn braids
            // and a crown of flowers (she loves flowers and runs the seedling nursery), pomegranate dress, a basket of
            // flowers on the arm. Talks with both hands — she knows everything that happens in the village.
            new NpcRecipe
            {
                Id = "nana", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.93f,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Bright, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.HairAuburn,
                Headwear = NpcHeadwear.FlowerCrown, HeadColor = NpcSwatch.PoplarGreen,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Pomegranate, Trim = NpcSwatch.Saffron, Sleeves = NpcSwatch.Cream, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.Cream, LayerTrim = NpcSwatch.FlowerPink,
                LeftArm = NpcArmPose.Crook, Props = new[] { NpcProp.FlowerBasket },
                Gesture = NpcGesture.Chatter,
                Story = "Feirante tagarela que sabe de tudo da vila e ama flores",
            },

            // Srosh, the stablekeeper ("to hear / to obey"): listens to animals and people and speaks little — quiet face, no
            // hat so the ears show, a low ponytail; leather vest over an ochre chapan, a coil of rope at the hip and a
            // grooming brush in the hand. Slow strokes, as if on a horse's neck.
            new NpcRecipe
            {
                Id = "srosh", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Calm, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Moustache,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Ochre, Trim = NpcSwatch.Walnut, Sleeves = NpcSwatch.Ochre, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Vest | NpcLayer.Belt, LayerColor = NpcSwatch.LeatherDark,
                Props = new[] { NpcProp.RopeCoil, NpcProp.Brush },
                Gesture = NpcGesture.Stroke,
                Story = "Tratador da estrebaria, escuta tudo (bicho e gente), fala pouco",
            },

            // Patig, the tracker: patient master of footprints — shoulders forward and head down, reading the mud; forest
            // green tunic and a brown hooded cape to blend into the tugai, feathers in the headband, a bow and a quiver
            // (he teaches that hunting is honouring the animal's light). Scans the ground slowly.
            new NpcRecipe
            {
                Id = "patig", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.02f, Posture = NpcPosture.Shy,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairDark,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Walnut,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.PoplarGreen, Trim = NpcSwatch.Walnut, Sleeves = NpcSwatch.Walnut, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.Walnut, LayerTrim = NpcSwatch.PoplarGreen,
                Props = new[] { NpcProp.Bow, NpcProp.Quiver, NpcProp.HatFeathers },
                Gesture = NpcGesture.Scan,
                Story = "Rastreador paciente, mestre de pegadas, respeita os bichos",
            },

            // Mestre Xroshtag, the Call personified: a big man with a thunder voice (stout, proud chest, mouth open), a
            // pomegranate turban and a saffron kaftan for the Bema hymns, a dutar on his back (he keeps the house of
            // instruments). Raises his arm to call everyone from far away.
            new NpcRecipe
            {
                Id = "xroshtag", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 1.1f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Bright, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.Pomegranate, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Saffron, Trim = NpcSwatch.Pomegranate, Sleeves = NpcSwatch.Saffron, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Indigo, LayerTrim = NpcSwatch.OldGold,
                Props = new[] { NpcProp.Dutar },
                Gesture = NpcGesture.Call,
                Story = "O Chamado: voz de trovão, chama todo mundo de longe, hinos do Bema",
            },

            // Padwaxtag, the Answer: Xroshtag's shy apprentice with a thin voice — a youth, head low, blushing, in his
            // master's colours but smaller (saffron cap and sash on cream), a reed flute held close. Peeks up now and then.
            new NpcRecipe
            {
                Id = "padwaxtag", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 0.95f, Posture = NpcPosture.Shy,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Shy, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Saffron, HeadTrim = NpcSwatch.Pomegranate,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Cream, Trim = NpcSwatch.Saffron, Sleeves = NpcSwatch.Cream, Trousers = NpcSwatch.Indigo,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Saffron, LayerTrim = NpcSwatch.Pomegranate,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Flute },
                Gesture = NpcGesture.Shy,
                Story = "A Resposta: aprendiz tímido de voz fininha",
            },

            // Ramrat, the guardian of the lamp (the Mother of Life's name): serene, all in white with a gold sash and a
            // white hood, an oil lamp cupped in both hands — she always lights one for whoever leaves (the Bênção that
            // protects the backpack). Lifts the lamp slowly.
            new NpcRecipe
            {
                Id = "ramrat", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.RobeWhite, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.RobeWhite, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.RobeWhite, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.OldGold, LayerTrim = NpcSwatch.Saffron,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.OilLamp },
                Gesture = NpcGesture.LanternRaise,
                Story = "Guardiã serena da lâmpada, acende uma para quem sai",
            },

            // ------------------------------------------------------------------
            // Copas de Garajonay (elfos-da-mata, Docs/Mapas100.md §3.3): bark skin, autumn-leaf hair, laurel, moss, mist
            // and heather; long ears swept back (Docs/MetodologiaMapasERacas.md §2.1). Names from the legend of Gara and
            // Jonay (La Gomera) and the guanche chiefs of Tenerife.
            // ------------------------------------------------------------------

            // Gara, the canopy warden (she gives the city's missions): tall and calm, autumn hair loose to the waist under
            // a heather crown, a laurel robe with a mist trim and a moss cape, the cocoon lantern of the bridges on a pole
            // in her hand. Raises it slowly, lighting the rope bridges for whoever walks them at night.
            new NpcRecipe
            {
                Id = "gara", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.08f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinBark, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairAutumn, ElfEars = true,
                Headwear = NpcHeadwear.FlowerCrown, HeadColor = NpcSwatch.Laurel,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Laurel, Trim = NpcSwatch.Mist, Sleeves = NpcSwatch.Laurel, Boots = NpcSwatch.Bark,
                Layers = NpcLayer.Cape | NpcLayer.Sash, LayerColor = NpcSwatch.MossLeaf, LayerTrim = NpcSwatch.Heather,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole },
                Gesture = NpcGesture.LanternRaise,
                Story = "Guardiã das copas, acende as pontes de cipó, dá as missões da cidade",
            },

            // Jonay, the archer (he trains the shot; Maithe adores him): upright and bright-eyed, autumn ponytail and a laurel
            // headband, a moss tunic and a heather scarf, the longbow and the quiver of laurel arrows. Calls out to the
            // apprentices across the clearing.
            new NpcRecipe
            {
                Id = "jonay", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.06f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinBark, Face = NpcFace.Bright, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairAutumn, ElfEars = true,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Laurel,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.MossLeaf, Trim = NpcSwatch.Bark, Sleeves = NpcSwatch.Laurel, Trousers = NpcSwatch.Bark, Boots = NpcSwatch.Bark,
                Layers = NpcLayer.Belt | NpcLayer.Scarf, LayerColor = NpcSwatch.Bark, LayerTrim = NpcSwatch.Heather,
                Props = new[] { NpcProp.Bow, NpcProp.Quiver },
                Gesture = NpcGesture.Call,
                Story = "Arqueiro das copas, treina tiro com a Maithe",
            },

            // Dácil, the herbalist (herbs and mist potions, the city's shop): young and kind, two autumn braids and a crown of
            // moss, a laurel dress with a mist apron and a satchel, a basket of herbs on the arm and a jar of mist water
            // (the drop the laurels drink from the cloud). Pours a measured drop.
            new NpcRecipe
            {
                Id = "dacil", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 0.98f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBark, Face = NpcFace.Kind, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.HairAutumn, ElfEars = true,
                Headwear = NpcHeadwear.FlowerCrown, HeadColor = NpcSwatch.MossLeaf,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Laurel, Trim = NpcSwatch.Heather, Sleeves = NpcSwatch.Mist, Boots = NpcSwatch.Bark,
                Layers = NpcLayer.Apron | NpcLayer.Satchel, LayerColor = NpcSwatch.Mist, LayerTrim = NpcSwatch.MossLeaf,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.HerbBasket, NpcProp.Jar },
                Gesture = NpcGesture.Pour,
                Story = "Ervateira das copas, ervas e poções de névoa",
            },

            // Bencomo, the old chief (graduations and invitations): broad and proud even old, white hair and a long beard, a
            // gold band on the brow, a bark kaftan with gold trim and a laurel cape, the chief's staff in one hand and the
            // invitation scroll in the other. Leans on the staff and nods.
            new NpcRecipe
            {
                Id = "bencomo", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Broad, Height = 1.04f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinBark, Face = NpcFace.Stern, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long, ElfEars = true,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.OldGold,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Bark, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.Bark, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Cape | NpcLayer.Sash, LayerColor = NpcSwatch.Laurel, LayerTrim = NpcSwatch.OldGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.Scroll },
                Gesture = NpcGesture.Lean,
                Story = "Velho chefe dos elfos, graduações e convites",
            },

            // Tinguaro, the tracker (hunting tasks): quiet and patient, a dark topknot and a goatee, a bark chapan under a
            // leather vest, a satchel of snares and a coil of rope, the forked tracking staff of the laurel floor. Reads the
            // moss for tracks, slowly.
            new NpcRecipe
            {
                Id = "tinguaro", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.02f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBark, Face = NpcFace.Calm, Hair = NpcHair.Topknot, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Goatee, ElfEars = true,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Bark, Trim = NpcSwatch.MossLeaf, Sleeves = NpcSwatch.Bark, Trousers = NpcSwatch.Laurel, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Vest | NpcLayer.Belt | NpcLayer.Satchel, LayerColor = NpcSwatch.LeatherDark, LayerTrim = NpcSwatch.MossLeaf,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Crook, NpcProp.RopeCoil },
                Gesture = NpcGesture.Scan,
                Story = "Rastreador das copas, lê rastro no musgo, tarefas de caça",
            },

            // ------------------------------------------------------------------
            // Taa Pwani (Swahili sailors and merchants, Docs/MetodologiaMapasERacas.md §1.4): white kanzus, embroidered
            // vests with gold thread, kofia caps and kilemba turbans, kangas, indigo cloth; coral, teak, sea and sail colours.
            // Names from the ficha (Capitão Liyongo, Mwana) and Swahili words: nuru (light), zawadi (gift), mbaraka (blessing).
            // ------------------------------------------------------------------

            // Captain Liyongo, the nakhoda of the dhows (the city's missions): broad and proud, a full beard, the embroidered kofia,
            // a white kanzu under a gold-threaded indigo vest, a coil of rope on the shoulder and the chart case at his hip.
            // Hand on the hip, calls the crew across the quay.
            new NpcRecipe
            {
                Id = "liyongo", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.06f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Coral, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Coral, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.Coral, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Vest | NpcLayer.Sash, LayerColor = NpcSwatch.Indigo, LayerTrim = NpcSwatch.OldGold,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.RopeCoil, NpcProp.ScrollCase },
                Gesture = NpcGesture.Call,
                Story = "Nakhoda (capitão) dos dhows de Taa Pwani, dá as missões da cidade",
            },

            // Mwana, the pearl merchant (the city's shop): upright and shrewd, a red kanga wrapped round the head, an indigo
            // dress with a pearl hem and a kanga shawl, the brass scale held up and a purse of coins. Counts pearls, twice.
            new NpcRecipe
            {
                Id = "mwana", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Bright, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.KangaRed, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Indigo, Trim = NpcSwatch.Pearl, Sleeves = NpcSwatch.Indigo, Boots = NpcSwatch.Henna,
                Layers = NpcLayer.Shawl | NpcLayer.Sash, LayerColor = NpcSwatch.KangaRed, LayerTrim = NpcSwatch.Sail,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Scale, NpcProp.CoinPurse },
                Gesture = NpcGesture.Count,
                Story = "Mercadora de pérolas, o bazar da cidade: passagem de dhow e o equipamento dos mapas 25–30",
            },

            // Nuru, keeper of the Coral Light: an old woman a little bent, a sail-cloth hood, a long robe with a sea-blue
            // trim and a shawl, the lamp pole of the tower in her hand and an oil lamp at her side. Raises the lamp at dusk.
            new NpcRecipe
            {
                Id = "nuru", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 0.96f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairGrey,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Sail, HeadTrim = NpcSwatch.SeaBlue,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Sail, Trim = NpcSwatch.SeaBlue, Sleeves = NpcSwatch.Sail, Boots = NpcSwatch.Teak,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.SeaBlue, LayerTrim = NpcSwatch.Brass,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole, NpcProp.OilLamp },
                Gesture = NpcGesture.LanternRaise,
                Story = "Guardiã da lâmpada do Farol de Coral (Taa Pwani = a Costa da Lâmpada)",
            },

            // Zawadi, the root weaver (the second mission): young and quick, bare head and a goatee, a laterite tunic with an
            // apron of sail cloth, a bundle of mangrove root on the arm and the spindle he twists the fibre on. Spins, always.
            new NpcRecipe
            {
                Id = "zawadi", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 1.0f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Bright, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Goatee,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Laterite, Trim = NpcSwatch.Sail, Sleeves = NpcSwatch.Laterite, Trousers = NpcSwatch.Teak, Boots = NpcSwatch.Teak,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.Sail, LayerTrim = NpcSwatch.Teak,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Bundle, NpcProp.Spindle },
                Gesture = NpcGesture.Spin,
                Story = "Trança as Botas de Raiz Viva com as raízes do mangue, dá a 2ª missão",
            },

            // Mbaraka, the star navigator: an old sailor, bent, white long beard and a coral kilemba with a sea-blue band, a
            // sail-cloth kanzu under a teak cape, the moon staff he sights the stars with and a case of old charts. Ponders the sky.
            new NpcRecipe
            {
                Id = "mbaraka", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.98f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Dreamy, Hair = NpcHair.Short, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.Coral, HeadTrim = NpcSwatch.SeaBlue,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Sail, Trim = NpcSwatch.SeaBlue, Sleeves = NpcSwatch.Sail, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Cape, LayerColor = NpcSwatch.Teak, LayerTrim = NpcSwatch.Brass,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.MoonStaff, NpcProp.ScrollCase },
                Gesture = NpcGesture.Ponder,
                Story = "Velho navegador, lê as estrelas da Barca da Lua e conta as monções",
            },

            // ------------------------------------------------------------------
            // Tambor Rubro (the orcs of the Red Drum, Docs/Mapas100.md §3.3): tall, olive-green or grey skin, small tusks;
            // ochre-red tunics, dyed hide vests and capes, the yellow cloth band of the camp, bronze and iron. Names from the
            // Hurrian tales (Kessi the hunter, Appu, the hero Gurparanzah) and Hittite/Mitanni history (Queen Ashmunikal,
            // King Tushratta). No war paint, no trophies: honour is work and hospitality.
            // ------------------------------------------------------------------

            // Gurparanzah, the archer chief (the city's missions; Ícaro's G4 trial): tall and proud, a long braid, a bronze
            // headband, an ochre-red tunic under a hide vest with the yellow sash, the bow in hand and the quiver on the back.
            new NpcRecipe
            {
                Id = "gurparanzah", Region = BiomeType.Desert, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.12f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinOrcOlive, Face = NpcFace.Stern, Hair = NpcHair.Braid, HairColor = NpcSwatch.HairBlack, Tusks = true,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Bronze, HeadTrim = NpcSwatch.BannerYellow,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.OchreRed, Trim = NpcSwatch.BannerYellow, Sleeves = NpcSwatch.SkinOrcOlive, Trousers = NpcSwatch.Hide, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Vest | NpcLayer.Sash, LayerColor = NpcSwatch.Hide, LayerTrim = NpcSwatch.BannerYellow,
                LeftArm = NpcArmPose.HoldStaff, RightArm = NpcArmPose.HipRest, Props = new[] { NpcProp.Bow, NpcProp.Quiver },
                Gesture = NpcGesture.Scan,
                Story = "Chefe-arqueira do Tambor Rubro, dá as missões da cidade (a prova G4 do Ícaro passa por ela)",
            },

            // Kessi, the hunter (the second mission): lean and quiet, grey skin, short hair under a hide hood, a hide cape over
            // a red-sandstone tunic, a coil of snare cord and a bundle of hides. Scans the canyon rim.
            new NpcRecipe
            {
                Id = "kessi", Region = BiomeType.Desert, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 1.08f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinOrcGrey, Face = NpcFace.Calm, Hair = NpcHair.Short, HairColor = NpcSwatch.HairDark, Tusks = true,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Hide, HeadTrim = NpcSwatch.OchreRed,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.RedSandstone, Trim = NpcSwatch.Hide, Sleeves = NpcSwatch.RedSandstone, Trousers = NpcSwatch.Hide, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.Hide, LayerTrim = NpcSwatch.Bronze,
                LeftArm = NpcArmPose.Carry, RightArm = NpcArmPose.Crook, Props = new[] { NpcProp.RopeCoil, NpcProp.Bundle },
                Gesture = NpcGesture.Lean,
                Story = "Caçador do Tambor Rubro (o dos contos hurritas), só caça o que o acampamento come; dá a 2ª missão",
            },

            // Appu, the gentle merchant who adopts the camp's orphans: stout and kind, bald with a full grey beard, a felt cap,
            // a sandstone kaftan with a yellow sash, a cup of tea held out and a purse of coins. Chatters with everyone.
            new NpcRecipe
            {
                Id = "appu", Region = BiomeType.Desert, Age = NpcAge.Elder, Build = NpcBuild.Stout, Height = 1.06f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinOrcOlive, Face = NpcFace.Jolly, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Full, Tusks = true,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.OchreRed, HeadTrim = NpcSwatch.BannerYellow,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.RedSandstone, Trim = NpcSwatch.BannerYellow, Sleeves = NpcSwatch.RedSandstone, Boots = NpcSwatch.Hide,
                Layers = NpcLayer.Sash | NpcLayer.Satchel, LayerColor = NpcSwatch.BannerYellow, LayerTrim = NpcSwatch.Hide,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Cup, NpcProp.CoinPurse },
                Gesture = NpcGesture.Chatter,
                Story = "Mercador gentil do conto hurrita, que sonhava com filhos e adota todos os órfãos do acampamento",
            },

            // Ashmunikal, matriarch of the drums (the guardian lamp on the drum tower): an old orc woman, upright, long grey
            // hair under an ochre headwrap, a long robe with a hide shawl, the hand drum and an oil lamp. Beats the hours.
            new NpcRecipe
            {
                Id = "ashmunikal", Region = BiomeType.Desert, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.04f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOrcGrey, Face = NpcFace.Kind, Hair = NpcHair.Long, HairColor = NpcSwatch.HairGrey, Tusks = true,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.OchreRed, HeadTrim = NpcSwatch.BannerYellow,
                Garment = NpcGarment.Robe, Main = NpcSwatch.OchreRed, Trim = NpcSwatch.Bronze, Sleeves = NpcSwatch.OchreRed, Boots = NpcSwatch.Hide,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.Hide, LayerTrim = NpcSwatch.BannerYellow,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.HandDrum, NpcProp.OilLamp },
                Gesture = NpcGesture.Drum,
                Story = "Matriarca dos tambores, guarda a Lâmpada Guardiã do Tambor Rubro no alto da torre do tambor",
            },

            // Tushratta, the drum smith (the city's shop, the Forja de Tambor): broad, a topknot and a short beard, bare olive
            // arms, a hide apron over the ochre tunic, the hammer and the tongs. Beats the iron to the drum.
            new NpcRecipe
            {
                Id = "tushratta", Region = BiomeType.Desert, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.1f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOrcOlive, Face = NpcFace.Bright, Hair = NpcHair.Topknot, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short, Tusks = true,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.OchreRed, Trim = NpcSwatch.Iron, Sleeves = NpcSwatch.SkinOrcOlive, Trousers = NpcSwatch.Ash, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.Hide, LayerTrim = NpcSwatch.Bronze,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Hammer, NpcProp.Tongs },
                Gesture = NpcGesture.Hammer,
                Story = "Ferreiro de cobre e bronze da Forja de Tambor, a loja da cidade (e a teriaga do Trono de Namrael)",
            },

            // ------------------------------------------------------------------
            // Salões de Bronze de Napirasu (the dwarves of the mountain, Docs/Mapas100.md §3.3): short (0.78–0.84) and broad,
            // braided beards with bronze beads, granite-grey and green-copper cloth, bronze bands and leather aprons. Names from
            // Elamite history: queen Napirasu (her bronze statue), Untash-Napirisha (the ziggurat builder), Puzur-Inshushinak
            // (the king of Linear Elamite), Shilhak-Inshushinak, Kidin-Hutran. No helmets of war: they are founders.
            // ------------------------------------------------------------------

            // Napirasu, the master founder (refining and fusion; her hall at the plaza head): stout and upright, two long
            // auburn braids under a bronze headband, a green-copper dress with a leather apron and belt, tongs and hammer.
            new NpcRecipe
            {
                Id = "napirasu", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.82f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Stern, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.HairAuburn,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.BronzeBright, HeadTrim = NpcSwatch.RuneGold,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Verdigris, Trim = NpcSwatch.BronzeBright, Sleeves = NpcSwatch.Granite, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.LeatherDark, LayerTrim = NpcSwatch.BronzeBright,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Tongs, NpcProp.Hammer },
                Gesture = NpcGesture.Hammer,
                Story = "Mestra fundidora dos Salões de Bronze (refino e fusão); guarda o cristal apagado da Grande Forja",
            },

            // Untash, the master builder (the city's missions): broad and bald, a long braided beard with bronze beads, a granite
            // tunic under a leather vest, a scroll of plans and a builder's rod. Measures twice.
            new NpcRecipe
            {
                Id = "untash", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 0.84f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Calm, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.LongBraided,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Granite, Trim = NpcSwatch.BronzeBright, Sleeves = NpcSwatch.Granite, Trousers = NpcSwatch.LeatherDark, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Vest | NpcLayer.Belt, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.BronzeBright,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Scroll, NpcProp.WalkingStick },
                Gesture = NpcGesture.Ponder,
                Story = "Mestre de obras dos Salões de Bronze, dá as missões da cidade; desenha o fole novo da Grande Forja",
            },

            // Puzur, the keeper of the runes (the city's shop, the Portões de Runa): an old dwarf, a long white beard, a green-copper
            // hood and a granite robe with a bronze-trimmed shawl, a set of rune tablets and the quill. Writes the runes.
            new NpcRecipe
            {
                Id = "puzur", Region = BiomeType.Mountain, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.8f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinFair, Face = NpcFace.Kind, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Verdigris, HeadTrim = NpcSwatch.RuneGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Granite, Trim = NpcSwatch.RuneGold, Sleeves = NpcSwatch.Granite, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.Verdigris, LayerTrim = NpcSwatch.BronzeBright,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Tablets, NpcProp.Quill },
                Gesture = NpcGesture.Write,
                Story = "Guardião das runas dos Salões de Bronze; os Portões de Runa, a loja da cidade (picareta, lamparina e chá de gengibre)",
            },

            // Shilhak, the miner (the second mission): young and stout, a short beard, the lamp helmet, a leather apron over a
            // cold-ember tunic, a satchel of ore, the pick on the shoulder and an oil lamp. Sings to the rock.
            new NpcRecipe
            {
                Id = "shilhak", Region = BiomeType.Mountain, Age = NpcAge.Youth, Build = NpcBuild.Stout, Height = 0.8f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Bright, Hair = NpcHair.Short, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.LampHelmet, HeadColor = NpcSwatch.BronzeBright, HeadTrim = NpcSwatch.LampGlow,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.ColdEmber, Trim = NpcSwatch.BronzeBright, Sleeves = NpcSwatch.ColdEmber, Trousers = NpcSwatch.Granite, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron | NpcLayer.Satchel, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.Iron,
                LeftArm = NpcArmPose.Carry, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Pick, NpcProp.OilLamp },
                Gesture = NpcGesture.Carry,
                Story = "Minerador dos Salões de Bronze, canta para a pedra antes da picareta; dá a 2ª missão (paga com chá de gengibre)",
            },

            // Kidin, the cook of the hall: stout and jolly, a full red beard, a felt cap, a bronze-trimmed kaftan under a white
            // apron, a ladle-cup and a water jug. The warmest place in the halls is his oven.
            new NpcRecipe
            {
                Id = "kidin", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.78f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairAuburn, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.ColdEmber, HeadTrim = NpcSwatch.BronzeBright,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Granite, Trim = NpcSwatch.BronzeBright, Sleeves = NpcSwatch.Granite, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.RobeWhite, LayerTrim = NpcSwatch.ColdEmber,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Cup, NpcProp.WaterJug },
                Gesture = NpcGesture.Pour,
                Story = "Cozinheiro do salão; desde que a forja apagou, o forno dele é o lugar mais quente dos Salões",
            },

            // ------------------------------------------------------------------
            // Mercado dos Túneis (the kobolds of the tunnels, Docs/Mapas100.md §3.3): small (0.75–0.8), a short muzzle
            // (NpcRecipe.Snout), smooth rust-ochre or slate-blue scales; canvas, copper and bottle glass, goggles and tools.
            // Names from the Sogdian Ancient Letters (Miwnay and her daughter Shayn) and the Mug documents (Farnkhund,
            // Takhsich). Never misers: they bottle the quiet air and mend what is broken.
            // ------------------------------------------------------------------

            // Miwnay, who runs the market (the city's shop, the Feira da Miwnay): rust scales, a canvas headwrap, a copper
            // kaftan with a lilac sash, a bottle of Ar Sereno held up and the scale. Calls the customers.
            new NpcRecipe
            {
                Id = "miwnay", Region = BiomeType.Caves, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 0.78f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinKoboldRust, Face = NpcFace.Bright, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairDark, Snout = true,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.Straw, HeadTrim = NpcSwatch.SereneAir,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Copper, Trim = NpcSwatch.SereneAir, Sleeves = NpcSwatch.Copper, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Sash | NpcLayer.Satchel, LayerColor = NpcSwatch.SereneAir, LayerTrim = NpcSwatch.Copper,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Jar, NpcProp.Scale },
                Gesture = NpcGesture.Call,
                Story = "Mestra da feira do Mercado dos Túneis (a carta de Miwnay); engarrafa o Ar Sereno, a loja da cidade",
            },

            // Shayn, Miwnay's daughter, who mends and recycles: slate scales, a ponytail under a bandana, goggles, a canvas tunic
            // with a leather apron and patches, the hammer and spools of copper wire. Taps at the scrap.
            new NpcRecipe
            {
                Id = "shayn", Region = BiomeType.Caves, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 0.75f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinKoboldSlate, Face = NpcFace.Kind, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairBlack, Snout = true,
                Headwear = NpcHeadwear.Bandana, HeadColor = NpcSwatch.GlassGreen, HeadTrim = NpcSwatch.Copper,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Straw, Trim = NpcSwatch.Copper, Sleeves = NpcSwatch.Straw, Trousers = NpcSwatch.Basalt, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron | NpcLayer.Patches, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.GlassGreen,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Hammer, NpcProp.Spools, NpcProp.Goggles },
                Gesture = NpcGesture.Hammer,
                Story = "Filha da Miwnay (a carta de Shayn), consertadora: conserto e reciclagem de sucata",
            },

            // Farnkhund, the cart driver of the tunnels: broad for a kobold, rust scales, a felt cap, a basalt chapan with a scarf
            // and belt, the lantern pole of the carts and the ring of switch keys. Raises the lantern down the gallery.
            new NpcRecipe
            {
                Id = "farnkhund", Region = BiomeType.Caves, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 0.8f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinKoboldRust, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Snout = true,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.Basalt, HeadTrim = NpcSwatch.Copper,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Basalt, Trim = NpcSwatch.Copper, Sleeves = NpcSwatch.Basalt, Trousers = NpcSwatch.Leather, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Scarf | NpcLayer.Belt, LayerColor = NpcSwatch.GlassGreen, LayerTrim = NpcSwatch.Copper,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Keys, NpcProp.LanternPole },
                Gesture = NpcGesture.LanternRaise,
                Story = "Vagoneiro dos túneis, conhece cada desvio pelo rangido; dá a 2ª missão (paga com o Óleo do Fogo Eterno)",
            },

            // Takhsich, the inventor (the city's missions, Ícaro's favourite): slim, slate scales, spiky crest of hair, goggles,
            // a green-glass tunic under a patched vest, an oil can and a letter of plans. Mimes the turning of his fans.
            new NpcRecipe
            {
                Id = "takhsich", Region = BiomeType.Caves, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.77f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinKoboldSlate, Face = NpcFace.Bright, Hair = NpcHair.Spiky, HairColor = NpcSwatch.HairDark, Snout = true,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.GlassGreen, Trim = NpcSwatch.Copper, Sleeves = NpcSwatch.GlassGreen, Trousers = NpcSwatch.Basalt, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Vest | NpcLayer.Patches, LayerColor = NpcSwatch.Copper, LayerTrim = NpcSwatch.Basalt,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.OilCan, NpcProp.Letter, NpcProp.Goggles },
                Gesture = NpcGesture.Mime,
                Story = "Inventor do Mercado dos Túneis, faz os cata-ventos que puxam o Ar Sereno; dá as missões da cidade",
            },

            // ------------------------------------------------------------------
            // Aratta, a Cidade dos Chifres (povo-touro, Savana dos Auroques): reed, mat, red clay, golden grass, ribbon blue.
            // Sumerian names (the epics of Enmerkar and Lugalbanda, the Hymn of Shulgi). Big and gentle, horns, a broad muzzle.
            // ------------------------------------------------------------------

            // Enmerkar, the chief builder (the city's missions; in the epic he invented writing on clay to answer the lord of
            // Aratta): broad and proud, the full Sumerian beard, a ribbon headband, a long reed-coloured robe with a red-clay hem
            // and a blue sash, clay tablets held in front and the builder's measuring staff. Explains with both hands.
            new NpcRecipe
            {
                Id = "enmerkar", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.2f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinAurochs, Face = NpcFace.Stern, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Full, Horns = true,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Ribbon, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Reed, Trim = NpcSwatch.Clay, Sleeves = NpcSwatch.SkinAurochs, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Ribbon, LayerTrim = NpcSwatch.OldGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Tablets, NpcProp.WalkingStick },
                Gesture = NpcGesture.Chatter,
                Story = "Chefe construtor de Aratta (o épico de Enmerkar), escreve na argila; dá as missões da cidade",
            },

            // Lugalbanda, the wanderer who got lost in the mountains and came back (exploration, secret maps; the city's shop:
            // the Alforje do Lugalbanda, with the Artífices' relic of map 27): slim and dreamy, tawny coat, a reed hood, a
            // patched chapan with a satchel, a bedroll on the back, a map scroll held up and the walking stick. Scans the horizon.
            new NpcRecipe
            {
                Id = "lugalbanda", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.14f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinAurochsTawny, Face = NpcFace.Dreamy, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Short, Horns = true,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Reed, HeadTrim = NpcSwatch.Ribbon,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Straw, Trim = NpcSwatch.Ribbon, Sleeves = NpcSwatch.Straw, Trousers = NpcSwatch.Leather, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Satchel | NpcLayer.Patches, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.Ribbon,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Scroll, NpcProp.WalkingStick, NpcProp.Bedroll },
                Gesture = NpcGesture.Scan,
                Story = "O andarilho que se perdeu e voltou (o épico de Lugalbanda): mapas secretos e o Alforje, a loja da cidade",
            },

            // Shulgi, the runner (in the hymn he ran from Nippur to Ur in a day; the Corrida de Shulgi, the second mission): young
            // and light, a topknot, a ribbon band, a short golden tunic with a blue sash, a water jug and a race ribbon held up.
            // Restless little hops.
            new NpcRecipe
            {
                Id = "shulgi", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 1.16f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinAurochs, Face = NpcFace.Bright, Hair = NpcHair.Topknot, HairColor = NpcSwatch.HairBlack, Horns = true,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.SandGold, HeadTrim = NpcSwatch.Ribbon,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.SandGold, Trim = NpcSwatch.Ribbon, Sleeves = NpcSwatch.SkinAurochs, Trousers = NpcSwatch.Reed, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Ribbon, LayerTrim = NpcSwatch.SandGold,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.Crook, Props = new[] { NpcProp.Letter, NpcProp.WaterJug },
                Gesture = NpcGesture.Bounce,
                Story = "O corredor (o Hino de Shulgi): treina a Corrida de Shulgi pela savana; dá a 2ª missão da cidade",
            },

            // Ensuhkeshdanna, the rival who became a friend (friendly duels; in the epic the lord of Aratta who lost the contest
            // of riddles to Enmerkar): the biggest of them, a long braided beard, a red-clay chapan with a cape and belt, hand on
            // the hip and the hand drum that calls the duels. Drums.
            new NpcRecipe
            {
                Id = "ensuhkeshdanna", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 1.24f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinAurochsTawny, Face = NpcFace.Jolly, Hair = NpcHair.Braid, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Short, Horns = true,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Clay, Trim = NpcSwatch.Reed, Sleeves = NpcSwatch.Clay, Trousers = NpcSwatch.Leather, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.Ribbon, LayerTrim = NpcSwatch.Reed,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.HandDrum },
                Gesture = NpcGesture.Drum,
                Story = "O rival que virou amigo (o senhor de Aratta no épico de Enmerkar): duelos amistosos na praça",
            },

            // ------------------------------------------------------------------
            // Cidade-Recife de Kirta (povo-do-mar): live coral, mother-of-pearl, reef turquoise, kelp, sand. Ugaritic names (the
            // epics of Kirta and of Aqhat). Fins for ears, gills, nacre or reef-turquoise skin; tails and fishermen's legs live
            // together (here, all on legs: the town is the dry part of the atoll).
            // ------------------------------------------------------------------

            // Kirta, the king who lost his family to the sea and rebuilt everything (the city's missions): upright and calm, a
            // short beard, a coral circlet, a long nacre kaftan with a coral trim and a kelp-green sash, a coral staff. Calls the
            // divers in.
            new NpcRecipe
            {
                Id = "kirta", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.05f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinReef, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Short, Gills = true,
                Headwear = NpcHeadwear.SunCirclet, HeadColor = NpcSwatch.CoralLive, HeadTrim = NpcSwatch.Pearl,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Pearl, Trim = NpcSwatch.CoralLive, Sleeves = NpcSwatch.Pearl, Boots = NpcSwatch.Teak,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Laurel, LayerTrim = NpcSwatch.Pearl,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.MoonStaff, NpcProp.CrystalPendant },
                Gesture = NpcGesture.Call,
                Story = "O rei que perdeu a família no mar e reconstruiu tudo (o épico de Kirta); dá as missões da cidade",
            },

            // Pughat, the heroine who looks after everyone (rescue missions, the second mission): young and quick, nacre skin, a
            // ponytail, a turquoise tunic with a coral sash, a coil of rope and a jar of water. Bounces on her toes, ready to dive.
            new NpcRecipe
            {
                Id = "pughat", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 0.98f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinNacre, Face = NpcFace.Bright, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairBlack, Gills = true,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Turquoise, Trim = NpcSwatch.CoralLive, Sleeves = NpcSwatch.SkinNacre, Trousers = NpcSwatch.SeaBlue, Boots = NpcSwatch.Teak,
                Layers = NpcLayer.Sash | NpcLayer.Belt, LayerColor = NpcSwatch.CoralLive, LayerTrim = NpcSwatch.Pearl,
                LeftArm = NpcArmPose.Carry, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.RopeCoil, NpcProp.WaterJug },
                Gesture = NpcGesture.Bounce,
                Story = "A heroína que cuida de todos (o épico de Aqhat): missões de resgate no recife; dá a 2ª missão da cidade",
            },

            // Danel, the just judge (the city's shop: the diving tools: breathing shells, bottled air bubbles): an elder, a little
            // bent, a long white beard, a pearl hood, a long sea-blue robe with a pearl shawl, the scale of justice and the keys
            // of the shell racks. Ponders before he speaks.
            new NpcRecipe
            {
                Id = "danel", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinReef, Face = NpcFace.Kind, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long, Gills = true,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Pearl, HeadTrim = NpcSwatch.SeaBlue,
                Garment = NpcGarment.Robe, Main = NpcSwatch.SeaBlue, Trim = NpcSwatch.Pearl, Sleeves = NpcSwatch.SeaBlue, Boots = NpcSwatch.Teak,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.Pearl, LayerTrim = NpcSwatch.CoralLive,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Scale, NpcProp.Keys },
                Gesture = NpcGesture.Ponder,
                Story = "O juiz justo (o pai de Aqhat): ferramentas de mergulho, conchas respiráveis e bolhas de ar, a loja da cidade",
            },

            // Aqhat, the archer (the trainer): broad shoulders, nacre skin, short hair and the fin crest, a kelp tunic under a
            // coral vest, the bow of the epic (a gift of the craftsman god, here of the reef) and a quiver. Mimes the draw.
            new NpcRecipe
            {
                Id = "aqhat", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.04f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinNacre, Face = NpcFace.Bright, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Gills = true,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Laurel, Trim = NpcSwatch.Pearl, Sleeves = NpcSwatch.SkinNacre, Trousers = NpcSwatch.SeaBlue, Boots = NpcSwatch.Teak,
                Layers = NpcLayer.Vest | NpcLayer.Belt, LayerColor = NpcSwatch.CoralLive, LayerTrim = NpcSwatch.Pearl,
                LeftArm = NpcArmPose.HoldStaff, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Bow, NpcProp.Quiver },
                Gesture = NpcGesture.Mime,
                Story = "O arqueiro (o épico de Aqhat), o melhor arco do recife: treinador dos primos",
            },

            // ------------------------------------------------------------------
            // Jhilpur, a Cidade nas Raízes (Delta de Jhilpur, Docs/MetodologiaMapasERacas.md §5.1): cotton dhotis and wraps dyed
            // turmeric, indigo and madder, sandalwood, lotus pink, root green and the Living Water. Names of Manichaean
            // missionaries (Salmaios and Abiesous of the Cologne Codex, Nuhzadag the interpreter of Mani) and of the Kellis
            // letters (Tehat). Human; no marks of any living rite.
            // ------------------------------------------------------------------

            // Salmaios, the ascetic who tends the Raiz-Mãe (the city's missions): an old man, thin and at ease, the long white
            // beard, a sandalwood turban with a Living-Water band, a long turmeric kaftan under a cape of root green, the staff
            // with the lotus lantern held up and the case of his old letters. Ponders, then smiles.
            new NpcRecipe
            {
                Id = "salmaios", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 0.98f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Dreamy, Hair = NpcHair.Short, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.Sandalwood, HeadTrim = NpcSwatch.LivingWater,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Turmeric, Trim = NpcSwatch.Sandalwood, Sleeves = NpcSwatch.Turmeric, Boots = NpcSwatch.RootBark,
                Layers = NpcLayer.Cape, LayerColor = NpcSwatch.RootGreen, LayerTrim = NpcSwatch.LivingWater,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.MoonStaff, NpcProp.ScrollCase },
                Gesture = NpcGesture.Ponder,
                Story = "O asceta do Códice de Colônia; aqui, o velho que cuida da Raiz-Mãe e dá as missões da cidade",
            },

            // Nuhzadag, Mani's interpreter, here the boatman who speaks every tongue of the delta (the city's shop: the gear of
            // the gates 32–35): broad-chested and easygoing, a full beard, an indigo skullcap, a long murky-green kaftan under
            // a madder vest and a turmeric sash, the mooring rope on his shoulder and the case of his trade papers. Calls out
            // across the canal.
            new NpcRecipe
            {
                Id = "nuhzadag", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.04f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Indigo, HeadTrim = NpcSwatch.Turmeric,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.MurkWater, Trim = NpcSwatch.Turmeric, Sleeves = NpcSwatch.MurkWater, Boots = NpcSwatch.RootBark,
                Layers = NpcLayer.Vest | NpcLayer.Sash, LayerColor = NpcSwatch.Madder, LayerTrim = NpcSwatch.Turmeric,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.RopeCoil, NpcProp.ScrollCase },
                Gesture = NpcGesture.Call,
                Story = "O intérprete de Mani; aqui, o barqueiro que fala todas as línguas do delta: barcos e trocas, a loja da cidade",
            },

            // Abiesous, the master of the Codex, here the teacher of the boat school (the second mission): an old teacher,
            // upright, long grey hair under a lotus-leaf hood against the monsoon rain, a long indigo robe with a root-green
            // shawl, the punting pole with its lantern and a small oil lamp for the dusk lessons. Raises the lantern.
            new NpcRecipe
            {
                Id = "abiesous", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairGrey,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.RootGreen, HeadTrim = NpcSwatch.Lotus,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Indigo, Trim = NpcSwatch.Sandalwood, Sleeves = NpcSwatch.Indigo, Boots = NpcSwatch.RootBark,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.RootGreen, LayerTrim = NpcSwatch.LivingWater,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole, NpcProp.OilLamp },
                Gesture = NpcGesture.LanternRaise,
                Story = "O mestre do Códice de Colônia; aqui, o professor da escola de barco que ensina a remar na monção",
            },

            // Tehat, a woman of the Kellis letters, here the herbalist of the herb quarter: slim and at ease, a bun under a
            // madder headwrap, a long lotus-pink dress with a turmeric hem, a sandalwood shawl and sash, the scale she weighs
            // the herbs on held up and a purse of seeds. Counts the leaves, twice.
            new NpcRecipe
            {
                Id = "tehat", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.97f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Bright, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.Madder, HeadTrim = NpcSwatch.Turmeric,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Lotus, Trim = NpcSwatch.Turmeric, Sleeves = NpcSwatch.Lotus, Boots = NpcSwatch.Henna,
                Layers = NpcLayer.Shawl | NpcLayer.Sash, LayerColor = NpcSwatch.Sandalwood, LayerTrim = NpcSwatch.RootGreen,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Scale, NpcProp.CoinPurse },
                Gesture = NpcGesture.Count,
                Story = "Uma mulher das cartas de Kellis; aqui, a erveira do bairro de ervas: nim, açafrão-da-terra e lótus",
            },

            // ------------------------------------------------------------------
            // Palafitas de Tarchon (trols, Docs/Mapas100.md §3.3 and Docs/MetodologiaMapasERacas.md §6.5): tall and thin, moss-blue
            // or bark-green skin, slow and gentle; wet wood, leaf, lilac mist, rush, net twine and the will-o'-wisp. Etruscan
            // names (Tarchon, the founder of Tarquinia; Ramtha, Larthia, Vel and Thanchvil from inscriptions and tombs). Trols are
            // the tallest folk of the island: Height near the top of its range, no extra mesh.
            // ------------------------------------------------------------------

            // Tarchon, the founder (the city's missions): very old and very tall, stooped, long grey hair and a long beard, a
            // wet-wood robe under a shawl of old fishing net, leaning on the lantern pole whose will-o'-wisp he caught himself.
            new NpcRecipe
            {
                Id = "tarchon", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 1.25f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinTrollMoss, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Long,
                Garment = NpcGarment.Robe, Main = NpcSwatch.WetWood, Trim = NpcSwatch.Rush, Sleeves = NpcSwatch.WetWood, Boots = NpcSwatch.WetWood,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.NetTwine, LayerTrim = NpcSwatch.LilacMist,
                LeftArm = NpcArmPose.Hang, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole },
                Gesture = NpcGesture.Lean,
                Story = "Tarchon, o fundador lendário de Tarquínia; aqui, o trol mais velho das palafitas, que dá as missões da cidade",
            },

            // Ramtha, the healer (healing and the guardian lamp): tall and slim, two leaf-green braids under a crown of lilac
            // flowers with a wisp at the brow, a lilac-mist dress with a leaf sash, grinding moss in a mortar, a little lamp held up.
            new NpcRecipe
            {
                Id = "ramtha", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.22f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinTrollMoss, Face = NpcFace.Kind, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.SwampLeaf,
                Headwear = NpcHeadwear.FlowerCrown, HeadColor = NpcSwatch.LilacMist, HeadTrim = NpcSwatch.WillWisp,
                Garment = NpcGarment.Dress, Main = NpcSwatch.LilacMist, Trim = NpcSwatch.SwampLeaf, Sleeves = NpcSwatch.LilacMist, Boots = NpcSwatch.WetWood,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.SwampLeaf, LayerTrim = NpcSwatch.WillWisp,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.MortarPestle, NpcProp.OilLamp },
                Gesture = NpcGesture.Grind,
                Story = "Nome etrusco de Tarquínia; aqui, a curandeira das palafitas, que guarda a Lâmpada Guardiã da cidade",
            },

            // Larthia, the basket weaver (the city's shop: baskets, bags and the gear of the gates 33–34): bark-green and broad
            // for a trol, a bun under a rush headband, a rush tunic and a net-twine apron, a basket on the forearm and her purse.
            new NpcRecipe
            {
                Id = "larthia", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.2f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTrollBark, Face = NpcFace.Jolly, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairDark,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Rush, HeadTrim = NpcSwatch.LilacMist,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Rush, Trim = NpcSwatch.WetWood, Sleeves = NpcSwatch.Rush, Trousers = NpcSwatch.WetWood, Boots = NpcSwatch.WetWood,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.NetTwine, LayerTrim = NpcSwatch.SwampLeaf,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.HerbBasket, NpcProp.CoinPurse },
                Gesture = NpcGesture.Count,
                Story = "Nome etrusco de túmulo; aqui, a cesteira das palafitas: cestos, bolsas, barcos de junco e amuletos (a loja da cidade)",
            },

            // Vel, the fisher (the second mission): lanky, spiky moss hair under a cone hat of rush, a leaf tunic, belt and
            // satchel, the coil of net on his shoulder. Carries it with a slow, steady bob.
            new NpcRecipe
            {
                Id = "vel", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.25f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTrollMoss, Face = NpcFace.Bright, Hair = NpcHair.Spiky, HairColor = NpcSwatch.SwampLeaf,
                Headwear = NpcHeadwear.StrawCone, HeadColor = NpcSwatch.Rush, HeadTrim = NpcSwatch.NetTwine,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.SwampLeaf, Trim = NpcSwatch.Rush, Sleeves = NpcSwatch.SwampLeaf, Trousers = NpcSwatch.WetWood, Boots = NpcSwatch.WetWood,
                Layers = NpcLayer.Belt | NpcLayer.Satchel, LayerColor = NpcSwatch.WetWood, LayerTrim = NpcSwatch.LilacMist,
                LeftArm = NpcArmPose.Hang, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.RopeCoil },
                Gesture = NpcGesture.Carry,
                Story = "Nome etrusco de inscrição; aqui, o pescador de bagres das palafitas, que dá a 2ª missão",
            },

            // Thanchvil, the storyteller: old, bark-green, a long white braid under a lilac hood with a wisp, a wet-wood kaftan
            // with lilac hems and a leaf scarf, a cup of lilac tea in one hand; the other arm tells the story.
            new NpcRecipe
            {
                Id = "thanchvil", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.2f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTrollBark, Face = NpcFace.Dreamy, Hair = NpcHair.Braid, HairColor = NpcSwatch.HairWhite,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.LilacMist, HeadTrim = NpcSwatch.WillWisp,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.WetWood, Trim = NpcSwatch.LilacMist, Sleeves = NpcSwatch.WetWood, Boots = NpcSwatch.WetWood,
                Layers = NpcLayer.Scarf, LayerColor = NpcSwatch.SwampLeaf, LayerTrim = NpcSwatch.Rush,
                LeftArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Cup },
                Gesture = NpcGesture.Storytell,
                Story = "Thanchvil Tarnai, nome etrusco de um túmulo de Cerveteri; aqui, a contadora de histórias dos trols apagados",
            },

            // ------------------------------------------------------------------
            // Sangkent, a Cidade-Escadaria (Teto do Mundo, Docs/Mapas100.md §2.1 and map 36): quilted felt and yak wool against
            // the cold, the caravans' silk red and blue, apricot blossom, terrace stone and juniper. Names of Manichaean
            // missionaries and scribes (Abā; Kustai and Baraies of the Cologne Codex; Sisinnios, Mani's successor). Human; no
            // marks of any living rite (no beads, no prayer flags).
            // ------------------------------------------------------------------

            // Abā, the missionary, here the chief of the chutes (the city's missions): broad and proud, a full black beard, a
            // yak-wool felt cap with a red band, a yak-wool chapan with red hems, a blue scarf and a belt, the coil of chute rope
            // on his shoulder and the keys of the stations. Calls the next cart down.
            new NpcRecipe
            {
                Id = "aba", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.04f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.YakWool, HeadTrim = NpcSwatch.SilkRed,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.YakWool, Trim = NpcSwatch.SilkRed, Sleeves = NpcSwatch.YakWool, Boots = NpcSwatch.Juniper,
                Layers = NpcLayer.Belt | NpcLayer.Scarf, LayerColor = NpcSwatch.SilkBlue, LayerTrim = NpcSwatch.Felt,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.RopeCoil, NpcProp.Keys },
                Gesture = NpcGesture.Call,
                Story = "Abā, missionário de Mani; aqui, o chefe das calhas de Sangkent, que dá as missões da cidade",
            },

            // Sisinnios, Mani's successor, here the outfitter of the caravans (the city's shop: the gear of the gates 36, 38, 40):
            // stout and easygoing, curly brown hair and a short beard under a felt hood with a red edge, a long quilted felt
            // kaftan with red hems under a red vest and an apricot sash, a bolt of quilted cloth and his scale. Counts the straps.
            new NpcRecipe
            {
                Id = "sisinnios", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.99f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Kind, Hair = NpcHair.Curly, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Felt, HeadTrim = NpcSwatch.SilkRed,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Felt, Trim = NpcSwatch.SilkRed, Sleeves = NpcSwatch.Felt, Boots = NpcSwatch.YakWool,
                Layers = NpcLayer.Vest | NpcLayer.Sash, LayerColor = NpcSwatch.SilkRed, LayerTrim = NpcSwatch.ApricotBlossom,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.FabricBolt, NpcProp.Scale },
                Gesture = NpcGesture.Count,
                Story = "Sisinnios (Mar Sisin), sucessor de Mani; aqui, o agasalheiro das caravanas: a loja da cidade (roupa térmica, grampos, capa de tempestade)",
            },

            // Kustai, the scribe of the Cologne Codex, here the keeper of the caravan notice board (the second mission): slim and
            // upright, a dark ponytail and a goatee, a blue skullcap, a long silk-blue robe with felt hems, a yak-wool satchel of
            // notices on a belt, the quill and a scroll held up. Writes.
            new NpcRecipe
            {
                Id = "kustai", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.02f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Calm, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.SilkBlue, HeadTrim = NpcSwatch.Felt,
                Garment = NpcGarment.Robe, Main = NpcSwatch.SilkBlue, Trim = NpcSwatch.Felt, Sleeves = NpcSwatch.SilkBlue, Boots = NpcSwatch.Juniper,
                Layers = NpcLayer.Satchel | NpcLayer.Belt, LayerColor = NpcSwatch.YakWool, LayerTrim = NpcSwatch.SilkRed,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Quill, NpcProp.Scroll },
                Gesture = NpcGesture.Write,
                Story = "Kustai, escriba do Códice de Colônia; aqui, quem cuida do quadro de avisos das caravanas e dá a 2ª missão",
            },

            // Baraies, the master of the Codex, here the teacher who counts the maps: old and stooped, a long grey beard under a
            // felt turban with an apricot band, a long terrace-stone robe with felt hems and a juniper shawl
            // edged red, the walking stick and the clay tablets of his maps. Tells the way.
            new NpcRecipe
            {
                Id = "baraies", Region = BiomeType.Mountain, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.96f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinFair, Face = NpcFace.Dreamy, Hair = NpcHair.Short, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.Felt, HeadTrim = NpcSwatch.ApricotBlossom,
                Garment = NpcGarment.Robe, Main = NpcSwatch.PamirStone, Trim = NpcSwatch.Felt, Sleeves = NpcSwatch.PamirStone, Boots = NpcSwatch.YakWool,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.Juniper, LayerTrim = NpcSwatch.SilkRed,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.Tablets },
                Gesture = NpcGesture.Storytell,
                Story = "Baraies, o mestre do Códice de Colônia; aqui, o professor de Sangkent que conta os mapas do Teto do Mundo",
            },

            // ------------------------------------------------------------------
            // Mingcheng, a Cidade dos Nove Anéis (Planície de Mingcheng, Docs/Mapas100.md §2.1 and map 41): red lacquer,
            // imperial gold, jade, lantern paper, grey brick, ink and celadon, the willow green. Names of the Mingjiao, the
            // Manichaeism of China and of the Uyghurs (the title mōzak; Bögü Qaghan; Lin Deng of Fujian; the fuduodan of 694).
            // Human; no marks of any living rite.
            // ------------------------------------------------------------------

            // Mōzak, the title of the Manichaean teachers in Chinese and Sogdian, here the keeper of the lanterns (the city's
            // shop: the Sun Seal of map 42): an elder of grey topknot and goatee under a jade headband, a long celadon robe with
            // lacquer hems and a red sash, the lantern pole in his right hand and the riddle slips in his left. Raises the lantern.
            new NpcRecipe
            {
                Id = "mozak", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.98f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Kind, Hair = NpcHair.Topknot, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Jade, HeadTrim = NpcSwatch.ImperialGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Celadon, Trim = NpcSwatch.Lacquer, Sleeves = NpcSwatch.Celadon, Boots = NpcSwatch.Ink,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Lacquer, LayerTrim = NpcSwatch.ImperialGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole, NpcProp.Letter },
                Gesture = NpcGesture.LanternRaise,
                Story = "Mōzak, título dos mestres maniqueus em chinês e sogdiano; aqui, o guardião das lanternas de Mingcheng: a loja da cidade (o Selo do Sol)",
            },

            // Bögü, the Uyghur khagan who took up the Manichaean faith in 762, here the old chief of the ring guard (the city's
            // missions): broad and proud, a full grey beard under a tall ink felt cap with a gold band, an ink kaftan with gold
            // hems under a grey-brick lamellar vest and a belt, the keys of the gates and a quiver on the back. Calls the watch.
            new NpcRecipe
            {
                Id = "bogu", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Broad, Height = 1.05f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Stern, Hair = NpcHair.Short, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.Ink, HeadTrim = NpcSwatch.ImperialGold,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Ink, Trim = NpcSwatch.ImperialGold, Sleeves = NpcSwatch.Lacquer, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Vest | NpcLayer.Belt, LayerColor = NpcSwatch.GreyBrick, LayerTrim = NpcSwatch.Lacquer,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Keys, NpcProp.Quiver },
                Gesture = NpcGesture.Call,
                Story = "Bögü Qaghan, o khagan uigur que adotou o maniqueísmo em 762; aqui, o velho chefe da guarda dos anéis, que dá as missões da cidade",
            },

            // Lin Deng, the Manichaean of Fujian in the Song era, here the master of the canal (the second mission): slim and
            // easygoing, a black topknot and a thin moustache under a straw cone hat, a jade tunic with ink hems and ink
            // trousers, a lacquer sash, the boat pole and a coil of mooring rope. Scans the water.
            new NpcRecipe
            {
                Id = "lin_deng", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.0f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Calm, Hair = NpcHair.Topknot, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Moustache,
                Headwear = NpcHeadwear.StrawCone, HeadColor = NpcSwatch.Straw, HeadTrim = NpcSwatch.Lacquer,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Jade, Trim = NpcSwatch.Ink, Sleeves = NpcSwatch.Jade, Trousers = NpcSwatch.Ink, Boots = NpcSwatch.Straw,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Lacquer, LayerTrim = NpcSwatch.LanternPaper,
                LeftArm = NpcArmPose.Carry, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.CaravanStaff, NpcProp.RopeCoil },
                Gesture = NpcGesture.Scan,
                Story = "Lin Deng, maniqueu de Fujian da era Song; aqui, o mestre do canal de Mingcheng, que dá a 2ª missão",
            },

            // Fuduodan, the Persian bishop who brought the Book of the Two Principles to the court of China in 694, here the
            // reader who reads the Two Principles aloud to whoever asks: an elder, slim and upright, a long grey beard under a
            // white turban with a gold band, a long white robe with gold hems and a celadon shawl, a scroll held up and its case.
            new NpcRecipe
            {
                Id = "fuduodan", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 1.0f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Dreamy, Hair = NpcHair.Short, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.RobeWhite, HeadTrim = NpcSwatch.ImperialGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.RobeWhite, Trim = NpcSwatch.ImperialGold, Sleeves = NpcSwatch.RobeWhite, Boots = NpcSwatch.Ink,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.Celadon, LayerTrim = NpcSwatch.Lacquer,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Scroll, NpcProp.ScrollCase },
                Gesture = NpcGesture.Read,
                Story = "Fuduodan, o bispo persa que levou o Livro dos Dois Princípios à corte da China em 694; aqui, o leitor que lê os Dois Princípios para quem pedir",
            },

            // ------------------------------------------------------------------
            // Torres Brancas de Amanirenas (altos-elfos, Docs/Mapas100.md §3.3 and map 42): tall dark-skinned elves with white
            // hair, astronomers and architects, in lime white, pale gold, sky blue, silver leaf and Meroë sandstone. Names of
            // the candaces and kings of Kush/Meroë.
            // ------------------------------------------------------------------

            // Amanirenas, the one-eyed candace of Meroë, here the queen-guardian (the city's missions): tall and proud, long
            // white hair under a sun circlet, a lime dress with gold hems and a sky-blue sash, the bow in her hand and the sun
            // pendant. Scans the white hills.
            new NpcRecipe
            {
                Id = "amanirenas", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.12f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Stern, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, ElfEars = true,
                Headwear = NpcHeadwear.SunCirclet, HeadColor = NpcSwatch.PaleGold, HeadTrim = NpcSwatch.SpireLight,
                Garment = NpcGarment.Dress, Main = NpcSwatch.SpireLime, Trim = NpcSwatch.PaleGold, Sleeves = NpcSwatch.SpireLime, Boots = NpcSwatch.MeroeSandstone,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.SkyBlue, LayerTrim = NpcSwatch.PaleGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HipRest, Props = new[] { NpcProp.Bow, NpcProp.SunPendant },
                Gesture = NpcGesture.Scan,
                Story = "Amanirenas, a candace de Meroé de um olho só; aqui, a rainha-guardiã das Torres Brancas, que dá as missões da cidade",
            },

            // Amanitore, the builder candace of Meroë, here the builder of the towers (decoration of the houses): a braid of white
            // hair under a sandstone headwrap, a sandstone tunic with lime hems over ebony trousers, an apron and a belt, the
            // hammer and the clay tablets of her plans. Taps the stone.
            new NpcRecipe
            {
                Id = "amanitore", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.06f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Bright, Hair = NpcHair.Braid, HairColor = NpcSwatch.HairWhite, ElfEars = true,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.MeroeSandstone, HeadTrim = NpcSwatch.PaleGold,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.MeroeSandstone, Trim = NpcSwatch.SpireLime, Sleeves = NpcSwatch.SpireLime, Trousers = NpcSwatch.Ebony, Boots = NpcSwatch.Ebony,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.SilverLeaf, LayerTrim = NpcSwatch.Ebony,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Hammer, NpcProp.Tablets },
                Gesture = NpcGesture.Hammer,
                Story = "Amanitore, a candace construtora de Meroé; aqui, a construtora das torres, que faz os enfeites das casas",
            },

            // Natakamani, the builder king of Meroë, here the master of the mirrors (the city's shop: refining with reflected
            // light): white ponytail and a short beard under a gold skullcap, a sky-blue kaftan with gold hems under a lime vest
            // and a sash, the crystal pendant and his scale. Counts the light.
            new NpcRecipe
            {
                Id = "natakamani", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.08f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Kind, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Short, ElfEars = true,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.PaleGold, HeadTrim = NpcSwatch.SkyBlue,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.SkyBlue, Trim = NpcSwatch.PaleGold, Sleeves = NpcSwatch.SkyBlue, Boots = NpcSwatch.MeroeSandstone,
                Layers = NpcLayer.Vest | NpcLayer.Sash, LayerColor = NpcSwatch.SpireLime, LayerTrim = NpcSwatch.PaleGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.CrystalPendant, NpcProp.Scale },
                Gesture = NpcGesture.Count,
                Story = "Natakamani, o rei construtor de Meroé; aqui, o mestre dos espelhos: a loja da cidade (refino com luz refletida)",
            },

            // Shanakdakhete, the first ruling queen of Meroë, here the one who teaches the light (magic; the second mission):
            // slim and upright, a white bun under a band of light, a silver-leaf robe with gold hems and a sky-blue shawl, the
            // moon staff in her right hand and the crystal held up in her left. Turns the light.
            new NpcRecipe
            {
                Id = "shanakdakhete", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.1f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Calm, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairWhite, ElfEars = true,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.SpireLight, HeadTrim = NpcSwatch.PaleGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.SilverLeaf, Trim = NpcSwatch.PaleGold, Sleeves = NpcSwatch.SilverLeaf, Boots = NpcSwatch.SpireLime,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.SkyBlue, LayerTrim = NpcSwatch.SpireLight,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.MoonStaff, NpcProp.CrystalPendant },
                Gesture = NpcGesture.Mime,
                Story = "Shanakdakhete, a primeira rainha reinante de Meroé; aqui, a que ensina a luz (magia) e dá a 2ª missão",
            },

            // Arkamani, the king of Meroë, here the one who reads the stars (the bestiary of the sky): an elder, slim and
            // stooped, long white hair and a goatee under an ebony hood with a gold edge, an ebony robe with hems of light and a
            // gold belt, the walking stick and the scroll of the sky. Reads the stars.
            new NpcRecipe
            {
                Id = "arkamani", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 1.04f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Dreamy, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Goatee, ElfEars = true,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.Ebony, HeadTrim = NpcSwatch.PaleGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Ebony, Trim = NpcSwatch.SpireLight, Sleeves = NpcSwatch.Ebony, Boots = NpcSwatch.MeroeSandstone,
                Layers = NpcLayer.Belt, LayerColor = NpcSwatch.PaleGold, LayerTrim = NpcSwatch.SkyBlue,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.Scroll },
                Gesture = NpcGesture.Read,
                Story = "Arkamani I, rei de Meroé; aqui, o que lê as estrelas das Torres Brancas (o bestiário do céu)",
            },

            // ------------------------------------------------------------------
            // Acampamento de Soslan, the centaurs' camp (the Estepe dos Nartes, Docs/Mapas100.md §3.3 and map 47): from the
            // waist down a horse (HorseBody: bay, black, grey, dun and the old chief's white), short felt tunics in the dyed Nart
            // red and blue, rawhide and Scythian gold. Names of the Nart sagas of Ossetia (Soslan, Batradz, Satana, Syrdon,
            // Uryzmæg).
            // ------------------------------------------------------------------

            // Soslan, the steel-skinned hero (the city's missions): broad and proud, long black hair and a short beard under a
            // felt cap edged red, a red tunic with gold hems and steel-grey arms (the skin tempered in the smith's water), a
            // rawhide cape and belt, the bow in hand and the quiver; a bay horse. Calls the herd.
            new NpcRecipe
            {
                Id = "soslan", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.08f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Stern, Hair = NpcHair.Long, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.SteppeFelt, HeadTrim = NpcSwatch.NartRed,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.NartRed, Trim = NpcSwatch.ScythGold, Sleeves = NpcSwatch.Steel,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.Rawhide, LayerTrim = NpcSwatch.ScythGold,
                LeftArm = NpcArmPose.HoldStaff, RightArm = NpcArmPose.HipRest, Props = new[] { NpcProp.Bow, NpcProp.Quiver },
                Gesture = NpcGesture.Call, HorseBody = true, Coat = NpcSwatch.HorseBay,
                Story = "Soslan, o herói de pele de aço das sagas dos Nartes; aqui, o centauro que guia a manada e dá as missões do acampamento",
            },

            // Batradz, the smith-warrior: broad, short dark hair and a full beard under a blue bandana, a blue tunic with rawhide
            // hems, a rawhide apron and belt, hammer and tongs; a black horse. Strikes in the rhythm of the forge.
            new NpcRecipe
            {
                Id = "batradz", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.06f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.Bandana, HeadColor = NpcSwatch.NartBlue, HeadTrim = NpcSwatch.ScythGold,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.NartBlue, Trim = NpcSwatch.Rawhide, Sleeves = NpcSwatch.SkinOlive,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.Rawhide, LayerTrim = NpcSwatch.ScythGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Hammer, NpcProp.Tongs },
                Gesture = NpcGesture.Hammer, HorseBody = true, Coat = NpcSwatch.Charcoal,
                Story = "Batradz, o guerreiro temperado na forja das sagas dos Nartes; aqui, o ferreiro das ferraduras e dos aros das carroças",
            },

            // Satana, the wise woman of the house (the second mission): two auburn braids under a gold headband, a felt tunic
            // with red hems, a red shawl, the guardian lamp held up in both hands; a grey horse. Lifts the lamp.
            new NpcRecipe
            {
                Id = "satana", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Kind, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.HairAuburn,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.ScythGold, HeadTrim = NpcSwatch.NartRed,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.SteppeFelt, Trim = NpcSwatch.NartRed, Sleeves = NpcSwatch.SteppeFelt,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.NartRed, LayerTrim = NpcSwatch.ScythGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.OilLamp },
                Gesture = NpcGesture.LanternRaise, HorseBody = true, Coat = NpcSwatch.HorseGrey,
                Story = "Satana, a sábia das sagas dos Nartes; aqui, a guardiã da lâmpada do acampamento e dos conselhos; dá a 2ª missão",
            },

            // Syrdon, the trickster (the city's shop: the Selaria Nômade): slim and at ease, spiky brown hair and a goatee under a
            // blue hood with a gold edge, a rawhide tunic with blue hems, a red sash and a satchel, the coin purse held up and a
            // bundle; a dun horse. Talks with both hands.
            new NpcRecipe
            {
                Id = "syrdon", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.98f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Bright, Hair = NpcHair.Spiky, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.NartBlue, HeadTrim = NpcSwatch.ScythGold,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Rawhide, Trim = NpcSwatch.NartBlue, Sleeves = NpcSwatch.NartBlue,
                Layers = NpcLayer.Satchel | NpcLayer.Sash, LayerColor = NpcSwatch.NartRed, LayerTrim = NpcSwatch.ScythGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.CoinPurse, NpcProp.Bundle },
                Gesture = NpcGesture.Chatter, HorseBody = true, Coat = NpcSwatch.HorseDun,
                Story = "Syrdon, o trapaceiro das sagas dos Nartes; aqui, o seleiro do acampamento (Selaria Nômade), sempre com uma pegadinha boa",
            },

            // Uryzmæg, the old chief: stooped, long grey hair and a long braided beard under a rawhide felt cap, a felt tunic with
            // gold hems and red sleeves, a blue cape and belt, the walking stick and the cup of the Narts; an old white horse.
            // Sips from the cup.
            new NpcRecipe
            {
                Id = "uryzmaeg", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinFair, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.LongBraided,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.Rawhide, HeadTrim = NpcSwatch.ScythGold,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.SteppeFelt, Trim = NpcSwatch.ScythGold, Sleeves = NpcSwatch.NartRed,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.NartBlue, LayerTrim = NpcSwatch.ScythGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.Cup },
                Gesture = NpcGesture.Sip, HorseBody = true, Coat = NpcSwatch.Ivory,
                Story = "Uryzmæg, o velho chefe das sagas dos Nartes; aqui, quem guia o acampamento de mudança em mudança e guarda a Uasamonga",
            },

            // ------------------------------------------------------------------
            // Fortim dos Gigantes-da-Colina (Docs/Mapas100.md §3.3 and map 49): giants a head and shoulders over everyone (Height
            // 1.45–1.55), ruddy and ochre skins, earth, stone, the good smoke, hill green and the Living Fire. Names of the
            // Manichaean Book of Giants (Barakʾel, Virōgdād, Šahmīzād): the giants who dreamt of the end and were sorry.
            // ------------------------------------------------------------------

            // Baraqel, the chief (the city's missions): broad and proud, short brown hair and a full beard, an earth tunic with a
            // green hem and a green cape, a log on his shoulder and the keys of the fort. Calls down the hill.
            new NpcRecipe
            {
                Id = "baraqel", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.55f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinGiantRuddy, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Full,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.HillEarth, Trim = NpcSwatch.HillGreen, Sleeves = NpcSwatch.SkinGiantRuddy,
                Trousers = NpcSwatch.GiantHide, Boots = NpcSwatch.GiantHide,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.HillGreen, LayerTrim = NpcSwatch.LivingEmber,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Log, NpcProp.Keys },
                Gesture = NpcGesture.Call,
                Story = "Barakʾel, gigante do Livro dos Gigantes maniqueu; aqui, o chefe do Fortim, arrependido e gentil, que dá as missões da cidade",
            },

            // Virogdad, the dreamer (the second mission): stout and easy, curly dark hair and a short beard under a hood the colour
            // of the good smoke, a smoke robe with earth hems and an ember sash, the clay tablets of the dreams and the quill.
            // Ponders.
            new NpcRecipe
            {
                Id = "virogdad", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 1.5f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinGiantOchre, Face = NpcFace.Dreamy, Hair = NpcHair.Curly, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.GoodSmoke, HeadTrim = NpcSwatch.HillStone,
                Garment = NpcGarment.Robe, Main = NpcSwatch.GoodSmoke, Trim = NpcSwatch.HillEarth, Sleeves = NpcSwatch.GoodSmoke, Boots = NpcSwatch.GiantHide,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.LivingEmber, LayerTrim = NpcSwatch.HillEarth,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Tablets, NpcProp.Quill },
                Gesture = NpcGesture.Ponder,
                Story = "Virōgdād, gigante do Livro dos Gigantes maniqueu que sonhou com as tábuas; aqui, o sonhador que lê os sonhos e dá a 2ª missão",
            },

            // Shahmizad, the old patriarch (the city's shop: the Panela Grande): stooped, long white hair and a long beard under a
            // green skullcap with an ember band, a stone-grey kaftan with ember hems, a smoke apron and a hide belt, the mortar and
            // a jar of spices. Grinds the cumin.
            new NpcRecipe
            {
                Id = "shahmizad", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Broad, Height = 1.45f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinGiantRuddy, Face = NpcFace.Kind, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.HillGreen, HeadTrim = NpcSwatch.LivingEmber,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.HillStone, Trim = NpcSwatch.LivingEmber, Sleeves = NpcSwatch.HillStone, Boots = NpcSwatch.GiantHide,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.GoodSmoke, LayerTrim = NpcSwatch.GiantHide,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.MortarPestle, NpcProp.Jar },
                Gesture = NpcGesture.Grind,
                Story = "Šahmīzād, o pai dos gigantes no Livro dos Gigantes maniqueu; aqui, o patriarca arrependido que cozinha na Panela Grande",
            },

            // ------------------------------------------------------------------
            // Arvoredo de Haripushpa, the trunk folk (Docs/Mapas100.md §3.3 and map 50): bark skin, moss and new-leaf hair, three
            // twigs growing from the crown (Twigs), moss, spring leaf, catkin and larch. Names of the kings of Kucha
            // (Haripuṣpa, Suvarṇapuṣpa); the Sogdian herbalist is human (Mār Aryāmān Puhr, the Manichaean teacher of the
            // Bezeklik letters).
            // ------------------------------------------------------------------

            // Haripushpa, the eldest (the city's missions): old, broad and stooped, long moss hair and a braided moss beard under a
            // crown of catkins, a moss robe with catkin hems and a larch shawl, the walking stick and a jug of meltwater.
            // Leans and nods, very slowly.
            new NpcRecipe
            {
                Id = "haripushpa", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Broad, Height = 1.2f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinBarkOld, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.TrunkMoss, Beard = NpcBeard.LongBraided,
                Headwear = NpcHeadwear.FlowerCrown, HeadColor = NpcSwatch.Catkin, HeadTrim = NpcSwatch.SpringLeaf,
                Garment = NpcGarment.Robe, Main = NpcSwatch.TrunkMoss, Trim = NpcSwatch.Catkin, Sleeves = NpcSwatch.SkinBarkOld, Boots = NpcSwatch.SkinBarkOld,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.LarchGreen, LayerTrim = NpcSwatch.Catkin,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.WaterJug },
                Gesture = NpcGesture.Lean, Twigs = true,
                Story = "Haripuṣpa, rei de Kucha; aqui, o mais velho do povo-tronco, que fala uma frase por minuto e dá as missões do arvoredo",
            },

            // Suvarnapushpa, the gardener (the city's shop: the Viveiro): slim and upright, two braids of new leaf under a catkin
            // band, a spring-leaf dress with catkin hems and a moss apron, a basket of seedlings and the watering jug. Pours.
            new NpcRecipe
            {
                Id = "suvarnapushpa", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.08f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinBarkYoung, Face = NpcFace.Bright, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.SpringLeaf,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Catkin, HeadTrim = NpcSwatch.SapAmber,
                Garment = NpcGarment.Dress, Main = NpcSwatch.SpringLeaf, Trim = NpcSwatch.Catkin, Sleeves = NpcSwatch.SkinBarkYoung, Boots = NpcSwatch.SkinBarkYoung,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.TrunkMoss, LayerTrim = NpcSwatch.Catkin,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.FlowerBasket, NpcProp.WaterJug },
                Gesture = NpcGesture.Pour, Twigs = true,
                Story = "Suvarṇapuṣpa, rei de Kucha; aqui, a jardineira do povo-tronco, que cuida do Viveiro (mudas, ervas e runas de cura)",
            },

            // Aryaman Puhr, the Sogdian herbalist passing through (the second mission): human, short dark hair and a goatee under an
            // indigo skullcap, a pomegranate chapan with gold hems, a moss satchel and belt bought in the grove, the herb basket
            // on the arm and a letter held up. Chats.
            new NpcRecipe
            {
                Id = "aryaman", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Kind, Hair = NpcHair.Short, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Indigo, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Pomegranate, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.Indigo, Trousers = NpcSwatch.Walnut, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Satchel | NpcLayer.Belt, LayerColor = NpcSwatch.TrunkMoss, LayerTrim = NpcSwatch.Catkin,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.HerbBasket, NpcProp.Letter },
                Gesture = NpcGesture.Chatter,
                Story = "Mār Aryāmān Puhr, mestre maniqueu das cartas sogdianas de Bezeklik; aqui, o herbolário sogdiano de passagem que dá a 2ª missão",
            },

            // ------------------------------------------------------------------
            // Vale dos Sátiros (the Picos de Farnkesh, Docs/Mapas100.md §3.3 and map 53): satyrs with goat legs, small goat horns
            // and pointed ears (Satyr), warm and ruddy satyr skins, brown, tawny and black fur, short tunics in glade green, grape
            // and the gold of the Pactolus. Names of the Phrygian myths of the ficha (Midas, Marsyas, Gordias).
            // ------------------------------------------------------------------

            // Midas, the king of golden hands (the city's missions): proud, curly auburn hair and a short beard under a gold
            // circlet, a grape tunic with gold hems, a gold cape and belt (everything he wears turned gold), a golden cup and a
            // bundle of grapes that have become gold; tawny fur. Now and then dries an eye: he misses the taste of things.
            new NpcRecipe
            {
                Id = "midas", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 0.98f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinSatyr, Face = NpcFace.Kind, Hair = NpcHair.Curly, HairColor = NpcSwatch.HairAuburn, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.SunCirclet, HeadColor = NpcSwatch.PactolusGold, HeadTrim = NpcSwatch.Grape,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Grape, Trim = NpcSwatch.PactolusGold, Sleeves = NpcSwatch.SkinSatyr,
                Layers = NpcLayer.Cape | NpcLayer.Belt, LayerColor = NpcSwatch.PactolusGold, LayerTrim = NpcSwatch.Grape,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Cup, NpcProp.Bundle },
                Gesture = NpcGesture.WipeTear, Satyr = true, Fur = NpcSwatch.FurTawny,
                Story = "Midas, o rei frígio do toque de ouro e do rio Pactolo; aqui, o rei dos sátiros tocado por Āz, que lava as mãos no rio e dá as missões do vale",
            },

            // Marsyas, the master of the double flute (the city's shop: the Flauta Dupla): slim and at ease, spiky brown hair and a
            // goatee under a crown of flowers, a glade-green tunic with flower hems and a grape sash, the double flute held up and
            // a hand drum; brown fur. Restless little hops: the dance is always about to begin.
            new NpcRecipe
            {
                Id = "marsyas", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.96f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinSatyrRuddy, Face = NpcFace.Bright, Hair = NpcHair.Spiky, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.FlowerCrown, HeadColor = NpcSwatch.FlowerPink, HeadTrim = NpcSwatch.GladeGreen,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.GladeGreen, Trim = NpcSwatch.FlowerPink, Sleeves = NpcSwatch.SkinSatyrRuddy,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Grape, LayerTrim = NpcSwatch.PactolusGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Flute, NpcProp.HandDrum },
                Gesture = NpcGesture.Bounce, Satyr = true, Fur = NpcSwatch.FurBrown,
                Story = "Marsyas, o sátiro frígio da flauta dupla; aqui, o mestre de flauta do vale, que ensina canções de viagem e cuida da Flauta Dupla",
            },

            // Gordias, the farmer of the knots (the second mission): old, stout and stooped, short grey hair and a full beard under
            // a straw hat, a vine-green tunic with wicker hems, a leather apron and belt, a coil of rope and the walking stick;
            // black fur. Counts the knots, twice.
            new NpcRecipe
            {
                Id = "gordias", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Stout, Height = 0.95f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinSatyr, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.StrawCone, HeadColor = NpcSwatch.Straw, HeadTrim = NpcSwatch.Grape,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.VineGreen, Trim = NpcSwatch.Wicker, Sleeves = NpcSwatch.Straw,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.Grape,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.RopeCoil, NpcProp.WalkingStick },
                Gesture = NpcGesture.Count, Satyr = true, Fur = NpcSwatch.FurBlack,
                Story = "Gordias, o fazendeiro frígio do nó górdio; aqui, o sátiro dos nós que amarra as videiras, faz quebra-cabeças de corda e dá a 2ª missão",
            },

            // ------------------------------------------------------------------
            // Farnkesh, os Ninhos de Pedra (the Picos de Farnkesh, Docs/Mapas100.md map 54): people of the wind and of silence,
            // lime white, painted red wood, gold, the high sky, yak rope and bell bronze. Names of the Manichaean mission to the
            // east (Shad-Ohrmizd, Sisin, Tadi of the Palmyra story, Ardawan of Mar Ammo's mission).
            // ------------------------------------------------------------------

            // Shad-Ohrmizd, the keeper of the wind bells (the city's missions): old and upright, long white hair and a long beard under
            // a red felt cap with a gold edge, a lime robe with red hems and a sky shawl, the walking stick and a small bell in hand.
            // Leans on the stick and listens.
            new NpcRecipe
            {
                Id = "shad_ohrmizd", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Calm, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.NestRed, HeadTrim = NpcSwatch.NestGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.NestLime, Trim = NpcSwatch.NestRed, Sleeves = NpcSwatch.NestLime, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.HighSky, LayerTrim = NpcSwatch.NestGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.CamelBell },
                Gesture = NpcGesture.Lean,
                Story = "Shād-Ohrmizd, que fundou um ramo maniqueu do oriente; aqui, o guardião dos sinos de vento de Farnkesh, que dá as missões da cidade",
            },

            // Sisin, the master of the rope bridges (the city's shop: the Cordas do Sisin): broad and proud, a black ponytail and a
            // short beard under a red bandana, a yak-rope tunic with red hems and lime sleeves, a red vest and belt, a coil of rope
            // on the shoulder and a bundle of planks. Bears the weight: he pulls the baskets.
            new NpcRecipe
            {
                Id = "sisin", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.04f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Stern, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Bandana, HeadColor = NpcSwatch.NestRed, HeadTrim = NpcSwatch.YakRope,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.YakRope, Trim = NpcSwatch.NestRed, Sleeves = NpcSwatch.NestLime, Trousers = NpcSwatch.Walnut, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Vest | NpcLayer.Belt, LayerColor = NpcSwatch.NestRed, LayerTrim = NpcSwatch.NestGold,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.RopeCoil, NpcProp.Bundle },
                Gesture = NpcGesture.Carry,
                Story = "Sisin (Sisinnios), o sucessor de Mani; aqui, o mestre das pontes de corda que puxa os cestos de Farnkesh e cuida das Cordas do Sisin",
            },

            // Tadi, the folder of wind paper (the second mission): slim and upright, two dark braids under a sky headband, a sky
            // dress with lime hems and cloud sleeves, a cloud apron edged red, a sheet of wind paper held up and a bolt of kite
            // cloth. Flaps her arms like a kite in the wind.
            new NpcRecipe
            {
                Id = "tadi", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.97f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Bright, Hair = NpcHair.TwoBraids, HairColor = NpcSwatch.HairDark,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.HighSky, HeadTrim = NpcSwatch.NestGold,
                Garment = NpcGarment.Dress, Main = NpcSwatch.HighSky, Trim = NpcSwatch.NestLime, Sleeves = NpcSwatch.CloudWhite, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.CloudWhite, LayerTrim = NpcSwatch.NestRed,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Letter, NpcProp.FabricBolt },
                Gesture = NpcGesture.Mime,
                Story = "Tadī, irmã de Nafshā na história da missão maniqueia de Palmira; aqui, a dobradora de papel de vento de Farnkesh que dá a 2ª missão",
            },

            // Ardawan, the braider of yak rope: at ease, curly dark hair and a full beard under a gold skullcap, a red chapan with
            // gold hems and yak-wool sleeves, a yak-wool scarf and satchel, the spindle and a coil of rope. Twirls the spindle.
            new NpcRecipe
            {
                Id = "ardawan", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.02f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Kind, Hair = NpcHair.Curly, HairColor = NpcSwatch.HairDark, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.NestGold, HeadTrim = NpcSwatch.NestRed,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.NestRed, Trim = NpcSwatch.NestGold, Sleeves = NpcSwatch.YakWool, Trousers = NpcSwatch.Walnut, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Scarf | NpcLayer.Satchel, LayerColor = NpcSwatch.YakWool, LayerTrim = NpcSwatch.NestLime,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Spindle, NpcProp.RopeCoil },
                Gesture = NpcGesture.Spin,
                Story = "Ardawān, o príncipe parta da missão de Mar Ammo; aqui, o trançador de corda de lã de iaque das pontes de Farnkesh",
            },

            // ------------------------------------------------------------------
            // Porto da Lua Branca (the Mar Gelado, Docs/Mapas100.md map 59): people of the ice harbour in reindeer hide, seal grey
            // and polar snow, with the lilac and silver of the painted moons and the polar night. Names of the ficha (Yirkap and
            // Pera, heroes of the Komi tales) and of the Manichaean letters of Kellis (Pamour, Makarios). The peoples of the Arctic
            // are living: no figure of their faiths, only forms of clothing for the cold.
            // ------------------------------------------------------------------

            // Yirkap, the ski hunter (the city's missions): slim and upright, a black ponytail and a short beard under a fur hood, a
            // reindeer-hide parka with snow-white hems and seal-grey trousers, a belt and a satchel, the ski pole in the right hand
            // and the bow in the left. Scans the ice for tracks.
            new NpcRecipe
            {
                Id = "yirkap", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.03f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Bright, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.ReindeerHide, HeadTrim = NpcSwatch.PolarSnow,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.ReindeerHide, Trim = NpcSwatch.PolarSnow, Sleeves = NpcSwatch.ReindeerHide, Trousers = NpcSwatch.SealGrey, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Belt | NpcLayer.Satchel, LayerColor = NpcSwatch.SealGrey, LayerTrim = NpcSwatch.MoonSilver,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.CaravanStaff, NpcProp.Bow },
                Gesture = NpcGesture.Scan,
                Story = "Yirkap, o caçador de esqui dos contos komi, que corria mais que as renas; aqui, o caçador que guarda o porto e dá as missões da cidade",
            },

            // Pamour, the furrier and boat-hirer of the quays (the city's shop: the gear of the gates 59–64): stout and at ease, short
            // brown hair and a full beard under a snow-white fur cap, a seal-grey chapan with snow hems and reindeer sleeves, a plank
            // apron and belt, a bolt of white fur and a purse. Counts the pelts, twice.
            new NpcRecipe
            {
                Id = "pamour", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.98f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.PolarSnow, HeadTrim = NpcSwatch.MoonLilac,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.SealGrey, Trim = NpcSwatch.PolarSnow, Sleeves = NpcSwatch.ReindeerHide, Trousers = NpcSwatch.MoonPlank, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.MoonPlank, LayerTrim = NpcSwatch.MoonSilver,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.FabricBolt, NpcProp.CoinPurse },
                Gesture = NpcGesture.Count,
                Story = "Pamour, da família maniqueia das cartas de Kellis; aqui, o peleiro e alugador de barcos do cais que vende o que os mapas do Mar Gelado pedem",
            },

            // Pera, the good giant (the second mission): very tall and broad, long auburn hair and a full beard under a silver
            // headband, a reindeer-hide tunic with seal hems and snow sleeves, a seal-grey belt and a lilac shawl, an ice pick on the
            // shoulder and a log for the boats. Carries the weight easily.
            new NpcRecipe
            {
                Id = "pera", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.12f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinFair, Face = NpcFace.Kind, Hair = NpcHair.Long, HairColor = NpcSwatch.HairAuburn, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.MoonSilver, HeadTrim = NpcSwatch.MoonLilac,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.ReindeerHide, Trim = NpcSwatch.SealGrey, Sleeves = NpcSwatch.PolarSnow, Trousers = NpcSwatch.SealGrey, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Belt | NpcLayer.Shawl, LayerColor = NpcSwatch.MoonLilac, LayerTrim = NpcSwatch.MoonSilver,
                LeftArm = NpcArmPose.HipRest, RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Pick, NpcProp.Log },
                Gesture = NpcGesture.Carry,
                Story = "Pera, o gigante bom (bogatir) dos contos komi; aqui, o gigante que quebra o gelo dos canais para os barcos e dá a 2ª missão",
            },

            // Makarios, the keeper of the moon lanterns of the Cais da Lua: old and stooped, short white hair and a long beard under
            // a lilac hood, a polar-night robe with silver hems and a lilac shawl, the lantern pole in the right hand and a letter for
            // his sons in the left. Raises the lantern to the boats coming in.
            new NpcRecipe
            {
                Id = "makarios", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.97f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Calm, Hair = NpcHair.Short, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.MoonLilac, HeadTrim = NpcSwatch.MoonSilver,
                Garment = NpcGarment.Robe, Main = NpcSwatch.PolarNight, Trim = NpcSwatch.MoonSilver, Sleeves = NpcSwatch.PolarNight, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.MoonLilac, LayerTrim = NpcSwatch.MoonSilver,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole, NpcProp.Letter },
                Gesture = NpcGesture.LanternRaise,
                Story = "Makarios, o pai que escreve aos filhos nas cartas maniqueias de Kellis; aqui, o guardião das lanternas de lua do Cais da Lua, que espera a Barca da Lua",
            },

            // ------------------------------------------------------------------
            // Hinokura, a Ilha-Forja (the Arquipélago de Hinokura, Docs/Mapas100.md map 65): people of the forge isle in red
            // lacquer, black volcanic stone, coral and obsidian, with the ember of the forges and the pine green of the isle.
            // Japanese names chosen for their sense, as the ficha asks (Akane "deep red", Kurogane "black iron", Sango "coral",
            // Hikari "light"); forms of clothing only (robes, work tunics, aprons), no figure of a faith.
            // ------------------------------------------------------------------

            // Mestra Akane, the master of the lacquer (the city's missions): slim and upright, black hair in a bun under a coral
            // headband, a red lacquer robe with coral hems and obsidian sleeves, a pine-green sash and a work apron, the lacquer
            // brush raised in the right hand and the jar of lacquer in the left. Lays a thin coat, the ninth.
            new NpcRecipe
            {
                Id = "akane", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 0.98f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Calm, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.None,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.CoralWhite, HeadTrim = NpcSwatch.HinoLacquer,
                Garment = NpcGarment.Robe, Main = NpcSwatch.HinoLacquer, Trim = NpcSwatch.CoralWhite, Sleeves = NpcSwatch.ObsidianBlack, Boots = NpcSwatch.ObsidianBlack,
                Layers = NpcLayer.Sash | NpcLayer.Apron, LayerColor = NpcSwatch.IslandPine, LayerTrim = NpcSwatch.CoralWhite,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Brush, NpcProp.Jar },
                Gesture = NpcGesture.Stroke,
                Story = "Mestra Akane (\"vermelho vivo\"), a mestra da laca de Hinokura que dá nove demãos finas em cada salão; aqui, dá as missões da cidade",
            },

            // Kurogane, the smith of black iron (the city's shop: the gear of the gates 65–70): very broad and proud, a black
            // topknot and a full beard, a volcanic-stone work tunic with ember hems and lacquer sleeves, a thick obsidian apron and
            // belt, the hammer and the tongs. Strikes the ingot, then looks at it.
            new NpcRecipe
            {
                Id = "kurogane", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.06f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Stern, Hair = NpcHair.Topknot, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.None,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.VolcanicStone, Trim = NpcSwatch.ForgeEmber, Sleeves = NpcSwatch.HinoLacquer, Trousers = NpcSwatch.ObsidianBlack, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Apron | NpcLayer.Belt, LayerColor = NpcSwatch.ObsidianBlack, LayerTrim = NpcSwatch.Iron,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Hammer, NpcProp.Tongs },
                Gesture = NpcGesture.Hammer,
                Story = "Kurogane (\"ferro preto\"), o ferreiro da Ilha-Forja que forja o ferro preto das ilhas; aqui, vende o que os mapas do Arquipélago de Hinokura pedem",
            },

            // Sango, the boatwoman of the Barco-Farol (the second mission): young, slim and at ease, a black ponytail under a
            // pumice bandana, a coral tunic with lacquer hems and pine sleeves, a sash and a satchel, a coil of rope on the left arm
            // and the lighthouse pole in the right. Calls the boats home.
            new NpcRecipe
            {
                Id = "sango", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 0.95f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Bright, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.None,
                Headwear = NpcHeadwear.Bandana, HeadColor = NpcSwatch.Pumice, HeadTrim = NpcSwatch.HinoLacquer,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.CoralWhite, Trim = NpcSwatch.HinoLacquer, Sleeves = NpcSwatch.IslandPine, Trousers = NpcSwatch.VolcanicStone, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Sash | NpcLayer.Satchel, LayerColor = NpcSwatch.HinoTile, LayerTrim = NpcSwatch.CoralWhite,
                LeftArm = NpcArmPose.Carry, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.RopeCoil, NpcProp.LanternPole },
                Gesture = NpcGesture.Call,
                Story = "Sango (\"coral\"), a barqueira que rema o Barco-Farol entre as ilhotas do Recife de Obsidiana; aqui, dá a 2ª missão da cidade",
            },

            // Hikari, the keeper of the sluices of the lava channels: old and stooped, short white hair and a long beard under an
            // obsidian skullcap, a pumice robe with ember hems and a lacquer shawl, a belt with the sluice keys, a walking stick in the
            // right hand and the keys in the left. Weighs which forge to wake today.
            new NpcRecipe
            {
                Id = "hikari", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.94f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Kind, Hair = NpcHair.Short, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.ObsidianBlack, HeadTrim = NpcSwatch.ForgeEmber,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Pumice, Trim = NpcSwatch.ForgeEmber, Sleeves = NpcSwatch.Pumice, Boots = NpcSwatch.ObsidianBlack,
                Layers = NpcLayer.Shawl | NpcLayer.Belt, LayerColor = NpcSwatch.HinoLacquer, LayerTrim = NpcSwatch.CoralWhite,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.Keys },
                Gesture = NpcGesture.Ponder,
                Story = "Hikari (\"luz\"), o velho que abre e fecha as comportas dos canais de lava que movem as forjas de Hinokura; aqui, o guardião das comportas",
            },

            // ------------------------------------------------------------------
            // Cais de Mesektet (the Rio das Doze Horas, Docs/Mapas100.md map 71): people of the night quay in linen, the gold of
            // Kemet, papyrus green, lapis and the night of the river. Names of the tales of Kemet (characters of its literature, not
            // gods), as the ficha asks: Sinuhe, Khunanup, Djedi and Setne; forms of clothing only (linen kilts and robes, broad
            // sashes), no figure of a faith.
            // ------------------------------------------------------------------

            // Sinuhe, the chief boatman (the city's missions): the traveller of the tale who came home; proud and weathered, short
            // black hair and a short beard under a linen headwrap, a linen kaftan with gold hems and lapis sleeves, a sash and a
            // belt of papyrus green, a coil of mooring rope on the left arm and the punting pole in the right. Watches the river.
            new NpcRecipe
            {
                Id = "sinuhe", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Stern, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.NileLinen, HeadTrim = NpcSwatch.KemetGold,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.NileLinen, Trim = NpcSwatch.KemetGold, Sleeves = NpcSwatch.LapisBlue, Trousers = NpcSwatch.QuaySandstone, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Sash | NpcLayer.Belt, LayerColor = NpcSwatch.PapyrusGreen, LayerTrim = NpcSwatch.KemetGold,
                LeftArm = NpcArmPose.Carry, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.RopeCoil, NpcProp.CaravanStaff },
                Gesture = NpcGesture.Scan,
                Story = "Sinuhe, o viajante do conto de Kemet que fugiu para longe e voltou para casa já velho; aqui, o barqueiro-chefe do Cais de Mesektet, que dá as missões da cidade",
            },

            // Khunanup, the merchant (the city's shop: the gear of the hours 71–76): the eloquent peasant of the tale, who won his
            // case by speaking well; stout and at ease, shaven head and a goatee, a linen robe with gold hems and papyrus sleeves, a
            // lapis shawl and a satchel, the balance scale raised in the right hand and the purse in the left. Talks, and talks.
            new NpcRecipe
            {
                Id = "khunanup", Region = BiomeType.Forest, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.97f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Jolly, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.None,
                Garment = NpcGarment.Robe, Main = NpcSwatch.NileLinen, Trim = NpcSwatch.KemetGold, Sleeves = NpcSwatch.PapyrusGreen, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Shawl | NpcLayer.Satchel, LayerColor = NpcSwatch.LapisBlue, LayerTrim = NpcSwatch.KemetGold,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.CoinPurse, NpcProp.Scale },
                Gesture = NpcGesture.Chatter,
                Story = "Khunanup, o \"camponês eloquente\" do conto de Kemet, que ganhou a sua causa falando bonito nove vezes; aqui, o comerciante do cais que negocia falando bonito e vende o que as horas do rio pedem",
            },

            // Djedi, the magician (the second mission; the trainer of magic): the old wonder-worker of the Papyrus Westcar, who
            // ate five hundred loaves a day and was a hundred and ten years old; very old and stooped, shaven head and a long white
            // beard under a lapis skullcap, a robe of the river night with gold hems, a lapis shawl and a belt, the walking stick in
            // the right hand and a crystal pendant held up in the left. Tells the old stories.
            new NpcRecipe
            {
                Id = "djedi", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 0.93f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Dreamy, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.LapisBlue, HeadTrim = NpcSwatch.KemetGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.RiverNight, Trim = NpcSwatch.KemetGold, Sleeves = NpcSwatch.RiverNight, Boots = NpcSwatch.KohlBlack,
                Layers = NpcLayer.Shawl | NpcLayer.Belt, LayerColor = NpcSwatch.LapisBlue, LayerTrim = NpcSwatch.NileLinen,
                LeftArm = NpcArmPose.HoldUp, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.CrystalPendant },
                Gesture = NpcGesture.Storytell,
                Story = "Djedi, o mágico de cento e dez anos do Papiro Westcar; aqui, o treinador de magia do Cais de Mesektet, que dá a 2ª missão da cidade",
            },

            // Setne, the librarian of the Caderno: the prince of the demotic tales who searched for the book of Thoth (here only
            // for books); young and slim, short black hair under a gold headband, a linen tunic with lapis hems and papyrus
            // sleeves, a sash of the river night and a satchel of scrolls, a scroll case in the crook of the left arm and a scroll
            // held open in the right. Reads while he walks.
            new NpcRecipe
            {
                Id = "setne", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 0.96f, Posture = NpcPosture.Upright,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Bright, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.None,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.KemetGold, HeadTrim = NpcSwatch.LapisBlue,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.NileLinen, Trim = NpcSwatch.LapisBlue, Sleeves = NpcSwatch.PapyrusGreen, Trousers = NpcSwatch.NileLinen, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Sash | NpcLayer.Satchel, LayerColor = NpcSwatch.RiverNight, LayerTrim = NpcSwatch.KemetGold,
                LeftArm = NpcArmPose.Crook, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.ScrollCase, NpcProp.Scroll },
                Gesture = NpcGesture.Read,
                Story = "Setne Khamwas, o príncipe dos contos demóticos de Kemet que vivia atrás de livros antigos; aqui, o bibliotecário do Caderno no Cais de Mesektet",
            },

            // ------------------------------------------------------------------
            // Oásis de Qocho (Areias Flamejantes): red sandstone, rose earth, golden sand, desert blue
            // ------------------------------------------------------------------

            // Ammo, the caravaneer, Vô Jorge's road friend: an old, generous storyteller (laughing face, short white beard),
            // a blue headwrap and scarf against the sand with goggles on the brow, a rust kaftan, a bedroll and pack on the
            // back, a satchel and the camel bell of Teimosa, and the tall brass caravan staff hung with bells. Leans on it.
            new NpcRecipe
            {
                Id = "ammo", Region = BiomeType.Desert, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.DesertBlue, HeadTrim = NpcSwatch.Indigo,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.Sandstone, Trim = NpcSwatch.SandGold, Sleeves = NpcSwatch.Sandstone, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Scarf | NpcLayer.Belt | NpcLayer.Satchel,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.CaravanStaff, NpcProp.Bedroll, NpcProp.Goggles, NpcProp.CamelBell },
                Gesture = NpcGesture.Lean,
                Story = "Caravaneiro contador de causos, amigo de estrada do Vô Jorge",
            },

            // Náfsha, the potter ("soul"): hands always covered in red clay (the hand colour is clay), sleeves of skin
            // (rolled up), a stained clay apron over a rose dress, a water jar held in front. Sings (open mouth) and presses
            // the clay in the rhythm of the wheel.
            new NpcRecipe
            {
                Id = "nafsha", Region = BiomeType.Desert, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 0.97f,
                Skin = NpcSwatch.SkinBrown, Hands = NpcSwatch.Clay, Face = NpcFace.Bright, Hair = NpcHair.Bun, HairColor = NpcSwatch.HairDark,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Rose,
                Garment = NpcGarment.Dress, Main = NpcSwatch.Rose, Trim = NpcSwatch.Sandstone, Sleeves = NpcSwatch.SkinBrown, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.Clay, LayerTrim = NpcSwatch.Sandstone,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Jar },
                Gesture = NpcGesture.Knead,
                Story = "Oleira de mãos sujas de barro, canta no compasso do torno",
            },

            // Vandak, the saddler (Nanai-vandak of the Ancient Letters): a clever merchant in a violet chapan and a cream
            // felt cap, a saddler's leather apron, a letter in one hand and a quill in the other — he writes to his family
            // far away. Scribbles.
            new NpcRecipe
            {
                Id = "vandak", Region = BiomeType.Desert, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Kind, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Moustache,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.Cream, HeadTrim = NpcSwatch.HairBlack,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Violet, Trim = NpcSwatch.SandGold, Sleeves = NpcSwatch.Violet, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Apron, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.LeatherDark,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Letter, NpcProp.Quill },
                Gesture = NpcGesture.Write,
                Story = "Seleiro esperto, escreve cartas para a família longe",
            },

            // Yardena of the cistern ("living water"): practical and bossy — chest out, stern brows, a hand on the hip;
            // a water-blue robe and turquoise headwrap, a copper jug in the other hand and the cistern keys at the belt
            // (she guards every drop). Tilts the jug for a measured pour.
            new NpcRecipe
            {
                Id = "yardena", Region = BiomeType.Desert, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Stern, Hair = NpcHair.Braid, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Headwrap, HeadColor = NpcSwatch.Turquoise, HeadTrim = NpcSwatch.DesertBlue,
                Garment = NpcGarment.Robe, Main = NpcSwatch.DesertBlue, Trim = NpcSwatch.Turquoise, Sleeves = NpcSwatch.DesertBlue, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.SandGold, LayerTrim = NpcSwatch.Turquoise,
                RightArm = NpcArmPose.HipRest, Props = new[] { NpcProp.WaterJug, NpcProp.Keys },
                Gesture = NpcGesture.Pour,
                Story = "Cuida da cisterna, prática e mandona, guarda cada gota",
            },

            // Uzzai, the herder: a young joker (laughing, curly hair, no hat), sand-gold tunic and a straw felt vest, the
            // shepherd's crook for goats and camels and a camel bell. Flaps his arms imitating the animals.
            new NpcRecipe
            {
                Id = "uzzai", Region = BiomeType.Desert, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 1.02f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Jolly, Hair = NpcHair.Curly, HairColor = NpcSwatch.HairDark,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.SandGold, Trim = NpcSwatch.Sandstone, Sleeves = NpcSwatch.SandGold, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Vest | NpcLayer.Sash, LayerColor = NpcSwatch.Straw, LayerTrim = NpcSwatch.Sandstone,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Crook, NpcProp.CamelBell },
                Gesture = NpcGesture.Mime,
                Story = "Pastor brincalhão de cabras e camelos, imita bicho",
            },

            // ------------------------------------------------------------------
            // Vila de Litema (Serra de Kaggen): clay red, lime white, granite, blanket blue and yellow
            // ------------------------------------------------------------------

            // Mestre Mihr, the miner (the Living Spirit who built the world): grave and of few words (stern face), broad
            // and tall, a leather lamp-helmet with the lamp lit, a striped highland blanket over a clay tunic, the pick on
            // his shoulder. Bears it steadily.
            new NpcRecipe
            {
                Id = "mihr", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Broad, Height = 1.06f,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Stern, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairBlack, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.LampHelmet, HeadColor = NpcSwatch.Leather, HeadTrim = NpcSwatch.LampGlow,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Clay, Trim = NpcSwatch.Granite, Sleeves = NpcSwatch.Clay, Trousers = NpcSwatch.Granite,
                Layers = NpcLayer.Blanket | NpcLayer.Belt, LayerColor = NpcSwatch.BlanketBlue, LayerTrim = NpcSwatch.BlanketYellow,
                RightArm = NpcArmPose.Carry, Props = new[] { NpcProp.Pick },
                Gesture = NpcGesture.Carry,
                Story = "Mineiro grave, de poucas palavras, coração mole",
            },

            // Coti, the weaver (Kaggen's wife in the |xam tales): she talks to the praying mantis on her loom — here it sits
            // on her shoulder; a yellow headscarf and a blanket-blue dress, the warm striped blanket she weaves for the
            // summit's snow, a spindle hanging from one hand and the wool in the other. Twirls the spindle.
            new NpcRecipe
            {
                Id = "coti", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Average, Height = 0.98f,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Kind, Hair = NpcHair.Curly, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Kerchief, HeadColor = NpcSwatch.BlanketYellow, HeadTrim = NpcSwatch.BlanketBlue,
                Garment = NpcGarment.Dress, Main = NpcSwatch.BlanketBlue, Trim = NpcSwatch.BlanketYellow, Sleeves = NpcSwatch.Clay, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Blanket, LayerColor = NpcSwatch.BlanketYellow, LayerTrim = NpcSwatch.Clay,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Spindle, NpcProp.Mantis },
                Gesture = NpcGesture.Spin,
                Story = "Tecelã de cobertores, conversa com o louva-a-deus da janela",
            },

            // Innaios, the player: lively and silly (laughing face) — he plays out of tune on purpose to make people laugh
            // and to chase away the cold; a conical straw hat with a blue band, a yellow tunic, a lyre held up and a small
            // drum at the hip. Alternating taps on the drum.
            new NpcRecipe
            {
                Id = "innaios", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.02f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.StrawCone, HeadColor = NpcSwatch.Straw, HeadTrim = NpcSwatch.BlanketBlue,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.BlanketYellow, Trim = NpcSwatch.BlanketBlue, Sleeves = NpcSwatch.BlanketYellow, Trousers = NpcSwatch.Granite,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Protea, LayerTrim = NpcSwatch.LimeWhite,
                LeftArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.Lyre, NpcProp.HandDrum },
                Gesture = NpcGesture.Drum,
                Story = "Tocador animado, desafina de propósito para fazer rir",
            },

            // ------------------------------------------------------------------
            // Vila das Lamparinas (Grutas do Limiar): tuff rose, basalt, lamp amber, white robes, copper
            // ------------------------------------------------------------------

            // Parthenos of the lamps (the Virgin of Light): young, sweet and brave — chest out, kind face, a braid falling
            // from a blue hood, a leather vest, satchel and a blue cape for the rope bridges, and the tall hooked lantern
            // pole that keeps the paths lit (she is not afraid of the dark). Lifts the lantern slowly.
            new NpcRecipe
            {
                Id = "parthenos", Region = BiomeType.Caves, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 1.02f, Posture = NpcPosture.Proud,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Kind, Hair = NpcHair.Braid, HairColor = NpcSwatch.HairBrown,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.DesertBlue, HeadTrim = NpcSwatch.OldGold,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Copper, Trim = NpcSwatch.DesertBlue, Sleeves = NpcSwatch.Cream, Trousers = NpcSwatch.Granite, Boots = NpcSwatch.Leather,
                Layers = NpcLayer.Vest | NpcLayer.Belt | NpcLayer.Satchel | NpcLayer.Cape, LayerColor = NpcSwatch.Leather, LayerTrim = NpcSwatch.DesertBlue,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole },
                Gesture = NpcGesture.LanternRaise,
                Story = "Guardiã das lamparinas, doce e corajosa, não tem medo do escuro",
            },

            // Mihrshah, the old miner (shown a garden of light in the tale): a stooped, dreamy, forgetful elder — half-closed
            // eyes, long white beard, his old lamp-helmet with the lamp gone cold (retired), a patched basalt chapan, a
            // walking stick, and a little crystal glowing on his chest: the garden of light crystals he swears he saw.
            new NpcRecipe
            {
                Id = "mihrshah", Region = BiomeType.Caves, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 0.95f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Dreamy, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.LampHelmet, HeadColor = NpcSwatch.Leather, HeadTrim = NpcSwatch.Brass,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Basalt, Trim = NpcSwatch.Tuff, Sleeves = NpcSwatch.Basalt, Trousers = NpcSwatch.ShadowIndigo,
                Layers = NpcLayer.Patches | NpcLayer.Belt, LayerTrim = NpcSwatch.Copper,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.CrystalPendant },
                Gesture = NpcGesture.Lean,
                Story = "Velho mineiro sonhador que viu um jardim de cristais de luz",
            },

            // Aso, the storyteller (Ananse's wife in the Akan tales): round, funny and warm (laughing face), a saffron head-
            // tie and a night-violet dress with a saffron shawl, the little spider puppet of Ananse held up and a small
            // drum at the hip. Wide story-telling sweeps of the arms.
            new NpcRecipe
            {
                Id = "aso", Region = BiomeType.Caves, Age = NpcAge.Adult, Build = NpcBuild.Stout, Height = 0.94f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinDeep, Face = NpcFace.Jolly, Hair = NpcHair.Short, HairColor = NpcSwatch.HairBlack,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.Saffron, HeadTrim = NpcSwatch.Pomegranate,
                Garment = NpcGarment.Dress, Main = NpcSwatch.NightViolet, Trim = NpcSwatch.Saffron, Sleeves = NpcSwatch.NightViolet, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Shawl, LayerColor = NpcSwatch.Saffron, LayerTrim = NpcSwatch.OldGold,
                LeftArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.SpiderPuppet, NpcProp.HandDrum },
                Gesture = NpcGesture.Storytell,
                Story = "Contadora engraçada das histórias de Ananse, faz voz de bicho",
            },

            // Gabryab of the wick: a quick boy (child, spiky hair, bright face) who trims the wicks and refills the oil of
            // the lamps on the rope bridges — a copper oil can and a bundle of white wicks at the belt; restless little
            // hops (he wants to join Lavine's exploring group).
            new NpcRecipe
            {
                Id = "gabryab", Region = BiomeType.Caves, Age = NpcAge.Child, Build = NpcBuild.Slim, Height = 1.05f,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Bright, Hair = NpcHair.Spiky, HairColor = NpcSwatch.HairBrown,
                Garment = NpcGarment.Tunic, Main = NpcSwatch.Copper, Trim = NpcSwatch.RobeWhite, Sleeves = NpcSwatch.Copper, Trousers = NpcSwatch.Basalt,
                Layers = NpcLayer.Belt,
                Props = new[] { NpcProp.OilCan, NpcProp.WickBundle },
                Gesture = NpcGesture.Bounce,
                Story = "Menino rápido que corta pavios, quer ser amigo da Lavine",
            },

            // ------------------------------------------------------------------
            // Torre da Roda das Doze Jarras
            // ------------------------------------------------------------------

            // Adda, the tower keeper: old and emotional (stooped, white beard), the tall white felt cap and indigo robe with
            // a pale sash he always had, the lantern on a pole so the cousins see where to bring the Golden Jar; now and
            // then he dries a tear (he cries when the jar comes back).
            new NpcRecipe
            {
                Id = "adda", Region = BiomeType.Mountain, Age = NpcAge.Elder, Build = NpcBuild.Average, Height = 1.0f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Kind, Hair = NpcHair.Short, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Full,
                Headwear = NpcHeadwear.FeltCap, HeadColor = NpcSwatch.RobeWhite, HeadTrim = NpcSwatch.Cream,
                Garment = NpcGarment.Robe, Main = NpcSwatch.Indigo, Trim = NpcSwatch.Cream, Sleeves = NpcSwatch.Indigo, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.RobeWhite, LayerTrim = NpcSwatch.Cream,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.LanternPole },
                Gesture = NpcGesture.WipeTear,
                Story = "Vigia velho e emotivo da Torre, chora quando a jarra volta",
            },

            // ------------------------------------------------------------------
            // The cousins' personal-story villagers (Main.unity Sandbox, the village hub)
            // ------------------------------------------------------------------

            // The survivor of the old village (Joaquim's): an elder who fled the wave of shadow with what he could carry —
            // stooped, patched walnut chapan, a cloth bundle on his back and a walking stick; he finally slept until
            // sunrise. Leans on the stick.
            new NpcRecipe
            {
                Id = "village_survivor", Region = BiomeType.Forest, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 0.97f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinLight, Face = NpcFace.Kind, Hair = NpcHair.Short, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Short,
                Headwear = NpcHeadwear.Skullcap, HeadColor = NpcSwatch.Walnut, HeadTrim = NpcSwatch.Ochre,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Walnut, Trim = NpcSwatch.Ochre, Sleeves = NpcSwatch.Walnut, Trousers = NpcSwatch.Ochre,
                Layers = NpcLayer.Patches | NpcLayer.Sash, LayerColor = NpcSwatch.Ochre, LayerTrim = NpcSwatch.Cream,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.WalkingStick, NpcProp.Bundle },
                Gesture = NpcGesture.Lean,
                Story = "Sobrevivente da vila do Joaquim, fugiu com uma trouxa",
            },

            // The apprentice of the sun (Maria's: the first lamp of the healing room): a youth in white with a saffron sash
            // and a little sun disc on the brow, a ponytail, holding a lit lamp up for the feverish travellers.
            new NpcRecipe
            {
                Id = "temple_acolyte", Region = BiomeType.Forest, Age = NpcAge.Youth, Build = NpcBuild.Slim, Height = 1.0f,
                Skin = NpcSwatch.SkinOlive, Face = NpcFace.Kind, Hair = NpcHair.Ponytail, HairColor = NpcSwatch.HairDark,
                Headwear = NpcHeadwear.SunCirclet, HeadColor = NpcSwatch.OldGold,
                Garment = NpcGarment.Robe, Main = NpcSwatch.RobeWhite, Trim = NpcSwatch.Saffron, Sleeves = NpcSwatch.RobeWhite, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Saffron, LayerTrim = NpcSwatch.OldGold,
                RightArm = NpcArmPose.HoldUp, Props = new[] { NpcProp.OilLamp },
                Gesture = NpcGesture.LanternRaise,
                Story = "Aprendiz do sol, acende as candeias da sala de cura (Maria)",
            },

            // The wandering scholar of the wind trails (Maithe's): curious (bright face) and wind-blown — a turquoise scarf
            // and a cape flying behind, a scroll case on the back and a scroll in hand to read the spiral glyphs, a walking
            // staff for the high trail. Reads, looks up.
            new NpcRecipe
            {
                Id = "windtrail_scholar", Region = BiomeType.Mountain, Age = NpcAge.Adult, Build = NpcBuild.Slim, Height = 1.06f,
                Skin = NpcSwatch.SkinTan, Face = NpcFace.Bright, Hair = NpcHair.Long, HairColor = NpcSwatch.HairBrown, Beard = NpcBeard.Goatee,
                Headwear = NpcHeadwear.Headband, HeadColor = NpcSwatch.Turquoise,
                Garment = NpcGarment.Chapan, Main = NpcSwatch.Granite, Trim = NpcSwatch.Turquoise, Sleeves = NpcSwatch.Granite, Trousers = NpcSwatch.Walnut,
                Layers = NpcLayer.Scarf | NpcLayer.Cape, LayerColor = NpcSwatch.Turquoise, LayerTrim = NpcSwatch.BlanketBlue,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.Scroll, NpcProp.ScrollCase, NpcProp.WalkingStick },
                Gesture = NpcGesture.Read,
                Story = "Estudioso das trilhas do vento, lê os símbolos em espiral (Maithe)",
            },

            // The archivist of the ruins (Ícaro's: the Artífices' clay tablets): an elder bookworm, round and a little
            // stooped over his reading, a cream turban and a sand-gold kaftan, a satchel of notes, two clay tablets held
            // in both hands.
            new NpcRecipe
            {
                Id = "ruins_archivist", Region = BiomeType.Desert, Age = NpcAge.Elder, Build = NpcBuild.Stout, Height = 0.95f, Posture = NpcPosture.Relaxed,
                Skin = NpcSwatch.SkinBrown, Face = NpcFace.Calm, Hair = NpcHair.Bald, HairColor = NpcSwatch.HairGrey, Beard = NpcBeard.Moustache,
                Headwear = NpcHeadwear.Turban, HeadColor = NpcSwatch.Cream, HeadTrim = NpcSwatch.Ochre,
                Garment = NpcGarment.Kaftan, Main = NpcSwatch.SandGold, Trim = NpcSwatch.OldGold, Sleeves = NpcSwatch.SandGold, Boots = NpcSwatch.Walnut,
                Layers = NpcLayer.Satchel,
                LeftArm = NpcArmPose.HoldFront, RightArm = NpcArmPose.HoldFront, Props = new[] { NpcProp.Tablets },
                Gesture = NpcGesture.Read,
                Story = "Arquivista das ruínas, decifra as tábuas dos Artífices (Ícaro)",
            },

            // The hermit mystic (Lavine's: the Red Night): a tall, stooped elder in a night-violet hood and robe with lilac
            // trim — the lilac mist that lets you look at the light — a red sash for the red moon, a long white beard and
            // a staff crowned by a crescent and a glowing stone. Ponders, hand to chin.
            new NpcRecipe
            {
                Id = "hermit_mystic", Region = BiomeType.Caves, Age = NpcAge.Elder, Build = NpcBuild.Slim, Height = 1.06f, Posture = NpcPosture.Stooped,
                Skin = NpcSwatch.SkinFair, Face = NpcFace.Dreamy, Hair = NpcHair.Long, HairColor = NpcSwatch.HairWhite, Beard = NpcBeard.Long,
                Headwear = NpcHeadwear.Hood, HeadColor = NpcSwatch.NightViolet, HeadTrim = NpcSwatch.Lilac,
                Garment = NpcGarment.Robe, Main = NpcSwatch.NightViolet, Trim = NpcSwatch.Lilac, Sleeves = NpcSwatch.NightViolet, Boots = NpcSwatch.LeatherDark,
                Layers = NpcLayer.Sash, LayerColor = NpcSwatch.Pomegranate, LayerTrim = NpcSwatch.Lilac,
                RightArm = NpcArmPose.HoldStaff, Props = new[] { NpcProp.MoonStaff },
                Gesture = NpcGesture.Ponder,
                Story = "Eremita místico da Noite Rubra, névoa lilás (Lavine)",
            },
        };

        /// <summary>Old resident/dialogue/scene ids → the recipe id (ids never change in saves, Docs/WorldBible §3).</summary>
        private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "zefa", "vo_edna" }, { "vo_zefa", "vo_edna" }, { "edna", "vo_edna" }, { "grandma", "vo_edna" },
            { "maria_gathering_point", "vo_edna" }, { "gathering_point", "vo_edna" },
            { "cida", "mariam" }, { "forest_rezadeira", "mariam" },
            { "severino", "adamas" }, { "blacksmith", "adamas" },
            { "damiao", "omoforo" },
            { "fatima", "narisah" },
            { "lazaro", "zaku" }, { "shopkeeper", "zaku" }, { "general_store", "zaku" },
            { "nenem", "nana" },
            { "tonico_haras", "srosh" }, { "ze_haras", "srosh" },
            { "ze_cacador", "patig" }, { "hunter", "patig" }, { "hunting", "patig" },
            { "bene", "xroshtag" },
            { "socorro_vila", "ramrat" }, { "fe", "ramrat" },
            { "bento", "ammo" }, { "desert_tropeiro", "ammo" }, { "mascate", "ammo" },
            { "rosa", "nafsha" },
            { "vava", "vandak" },
            { "socorro", "yardena" },
            { "tiao", "uzzai" },
            { "tonho", "mihr" }, { "mountain_miner", "mihr" },
            { "graca", "coti" },
            { "quinzinho", "innaios" },
            { "benedita", "parthenos" }, { "caves_lampkeeper", "parthenos" },
            { "candinho", "mihrshah" },
            { "fia", "aso" },
            { "zezinho", "gabryab" },
            { "malaquias", "adda" }, { "seu_malaquias", "adda" },
        };

        private static Dictionary<string, NpcRecipe> _byId;

        /// <summary>The named recipe for <paramref name="id"/> (a new name or an old id), or null.</summary>
        public static NpcRecipe Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (_byId == null)
            {
                _byId = Named.ToDictionary(r => r.Id);
            }

            string key = id.Trim().ToLowerInvariant();
            if (Aliases.TryGetValue(key, out string alias))
            {
                key = alias;
            }

            return _byId.TryGetValue(key, out NpcRecipe recipe) ? recipe : null;
        }

        /// <summary>The named recipe, else a generic villager seeded by <paramref name="id"/> in <paramref name="region"/>'s palette.</summary>
        public static NpcRecipe Resolve(string id, BiomeType region = BiomeType.Forest) => Find(id) ?? Villager(id ?? "villager", region);

        /// <summary>Every id that resolves to a named recipe (aliases included).</summary>
        public static IEnumerable<string> KnownIds => Named.Select(r => r.Id).Concat(Aliases.Keys);

        // ------------------------------------------------------------------
        // Generic villagers
        // ------------------------------------------------------------------

        private sealed class RegionLook
        {
            public NpcSwatch[] Mains, Trims, Skins, Hair;
            public NpcHeadwear[] HatsMen, HatsWomen;
            public NpcProp[] Props;
            public NpcLayer[] Layers;
        }

        private static readonly Dictionary<BiomeType, RegionLook> Looks = new Dictionary<BiomeType, RegionLook>
        {
            // Zarafshan: adobe, poplar, pomegranate, indigo; doppa caps and felt hats, kerchiefs.
            [BiomeType.Forest] = new RegionLook
            {
                Mains = new[] { NpcSwatch.Apricot, NpcSwatch.PoplarGreen, NpcSwatch.Indigo, NpcSwatch.Pomegranate, NpcSwatch.Turquoise, NpcSwatch.Ochre },
                Trims = new[] { NpcSwatch.Cream, NpcSwatch.OldGold, NpcSwatch.Turquoise, NpcSwatch.Saffron },
                Skins = new[] { NpcSwatch.SkinFair, NpcSwatch.SkinLight, NpcSwatch.SkinOlive, NpcSwatch.SkinTan },
                Hair = new[] { NpcSwatch.HairBlack, NpcSwatch.HairDark, NpcSwatch.HairBrown, NpcSwatch.HairAuburn },
                HatsMen = new[] { NpcHeadwear.Skullcap, NpcHeadwear.FeltCap, NpcHeadwear.None, NpcHeadwear.Turban },
                HatsWomen = new[] { NpcHeadwear.Kerchief, NpcHeadwear.Headband, NpcHeadwear.None },
                Props = new[] { NpcProp.Jar, NpcProp.Bundle, NpcProp.Cup },
                Layers = new[] { NpcLayer.Sash, NpcLayer.Vest, NpcLayer.Apron, NpcLayer.Belt },
            },
            // Qocho: sandstone, rose, gold, desert blue; headwraps against the sun.
            [BiomeType.Desert] = new RegionLook
            {
                Mains = new[] { NpcSwatch.Sandstone, NpcSwatch.Rose, NpcSwatch.SandGold, NpcSwatch.DesertBlue, NpcSwatch.Violet, NpcSwatch.Saffron },
                Trims = new[] { NpcSwatch.OldGold, NpcSwatch.Cream, NpcSwatch.VineGreen, NpcSwatch.DesertBlue },
                Skins = new[] { NpcSwatch.SkinOlive, NpcSwatch.SkinTan, NpcSwatch.SkinBrown, NpcSwatch.SkinDeep },
                Hair = new[] { NpcSwatch.HairBlack, NpcSwatch.HairDark },
                HatsMen = new[] { NpcHeadwear.Headwrap, NpcHeadwear.Turban, NpcHeadwear.Skullcap },
                HatsWomen = new[] { NpcHeadwear.Headwrap, NpcHeadwear.Kerchief, NpcHeadwear.Headband },
                Props = new[] { NpcProp.Jar, NpcProp.WaterJug, NpcProp.Bundle },
                Layers = new[] { NpcLayer.Sash, NpcLayer.Scarf, NpcLayer.Belt },
            },
            // Litema: clay red, lime, blanket blue/yellow; striped blankets, woven cone hats.
            [BiomeType.Mountain] = new RegionLook
            {
                Mains = new[] { NpcSwatch.Clay, NpcSwatch.BlanketBlue, NpcSwatch.Straw, NpcSwatch.Granite, NpcSwatch.LimeWhite },
                Trims = new[] { NpcSwatch.BlanketYellow, NpcSwatch.BlanketBlue, NpcSwatch.Protea, NpcSwatch.LimeWhite },
                Skins = new[] { NpcSwatch.SkinBrown, NpcSwatch.SkinDeep, NpcSwatch.SkinTan },
                Hair = new[] { NpcSwatch.HairBlack, NpcSwatch.HairDark },
                HatsMen = new[] { NpcHeadwear.StrawCone, NpcHeadwear.None, NpcHeadwear.Headband },
                HatsWomen = new[] { NpcHeadwear.Kerchief, NpcHeadwear.Headband, NpcHeadwear.None },
                Props = new[] { NpcProp.WalkingStick, NpcProp.Bundle, NpcProp.Jar },
                Layers = new[] { NpcLayer.Blanket, NpcLayer.Blanket, NpcLayer.Sash },
            },
            // Lamparinas: white robes and tuff, copper lamps; hoods and headbands.
            [BiomeType.Caves] = new RegionLook
            {
                Mains = new[] { NpcSwatch.RobeWhite, NpcSwatch.Tuff, NpcSwatch.Basalt, NpcSwatch.ShadowIndigo, NpcSwatch.Copper },
                Trims = new[] { NpcSwatch.Copper, NpcSwatch.OldGold, NpcSwatch.RobeWhite, NpcSwatch.Lilac },
                Skins = new[] { NpcSwatch.SkinFair, NpcSwatch.SkinLight, NpcSwatch.SkinOlive, NpcSwatch.SkinTan },
                Hair = new[] { NpcSwatch.HairBlack, NpcSwatch.HairDark, NpcSwatch.HairBrown },
                HatsMen = new[] { NpcHeadwear.Hood, NpcHeadwear.Skullcap, NpcHeadwear.None },
                HatsWomen = new[] { NpcHeadwear.Hood, NpcHeadwear.Headband, NpcHeadwear.Kerchief },
                Props = new[] { NpcProp.OilLamp, NpcProp.WickBundle, NpcProp.OilCan },
                Layers = new[] { NpcLayer.Sash, NpcLayer.Shawl, NpcLayer.Belt },
            },
        };

        private static readonly Dictionary<string, NpcRecipe> VillagerCache = new Dictionary<string, NpcRecipe>();

        /// <summary>
        /// A generic villager: everything picked by a stable hash of <paramref name="seed"/> from <paramref name="region"/>'s
        /// palette, so the same lot always shows the same person and two lots rarely look alike.
        /// </summary>
        public static NpcRecipe Villager(string seed, BiomeType region)
        {
            string key = "villager:" + region + ":" + seed;
            if (VillagerCache.TryGetValue(key, out NpcRecipe cached))
            {
                return cached;
            }

            RegionLook look = Looks.TryGetValue(region, out RegionLook l) ? l : Looks[BiomeType.Forest];
            uint h = Hash(key);
            int n = 0;
            T Pick<T>(T[] options) => options[(int)((Rotate(h, n++ * 7) & 0x7FFFFFFF) % (uint)options.Length)];
            bool Chance(float p) => ((Rotate(h, n++ * 7) & 0xFFFF) / 65535f) < p;

            bool woman = Chance(0.5f);
            NpcAge age = Pick(new[] { NpcAge.Adult, NpcAge.Adult, NpcAge.Elder, NpcAge.Youth, NpcAge.Child });
            var r = new NpcRecipe
            {
                Id = key,
                Region = region,
                Age = age,
                Build = Pick(new[] { NpcBuild.Average, NpcBuild.Slim, NpcBuild.Stout, NpcBuild.Average, NpcBuild.Broad }),
                Height = 0.92f + Pick(new[] { 0f, 0.04f, 0.08f, 0.12f, 0.02f }),
                Posture = age == NpcAge.Elder ? NpcPosture.Stooped : Pick(new[] { NpcPosture.Upright, NpcPosture.Relaxed, NpcPosture.Proud }),
                Skin = Pick(look.Skins),
                Face = Pick(new[] { NpcFace.Kind, NpcFace.Calm, NpcFace.Bright, NpcFace.Jolly, NpcFace.Kind }),
                HairColor = age == NpcAge.Elder ? Pick(new[] { NpcSwatch.HairGrey, NpcSwatch.HairWhite }) : Pick(look.Hair),
                Main = Pick(look.Mains),
                Trim = Pick(look.Trims),
                Trousers = Pick(new[] { NpcSwatch.Walnut, NpcSwatch.Granite, NpcSwatch.Indigo }),
                Boots = Pick(new[] { NpcSwatch.LeatherDark, NpcSwatch.Leather, NpcSwatch.Walnut }),
                LayerColor = Pick(look.Trims),
                LayerTrim = Pick(look.Mains),
                Story = "Morador da região",
            };

            if (woman)
            {
                r.Hair = Pick(new[] { NpcHair.Long, NpcHair.Bun, NpcHair.Braid, NpcHair.TwoBraids, NpcHair.Ponytail });
                r.Garment = Pick(new[] { NpcGarment.Dress, NpcGarment.Kaftan, NpcGarment.Dress });
                r.Headwear = Pick(look.HatsWomen);
            }
            else
            {
                r.Hair = Pick(new[] { NpcHair.Short, NpcHair.Short, NpcHair.Curly, NpcHair.Bald, NpcHair.Spiky });
                r.Garment = Pick(new[] { NpcGarment.Chapan, NpcGarment.Tunic, NpcGarment.Kaftan });
                r.Headwear = Pick(look.HatsMen);
                if (age == NpcAge.Adult || age == NpcAge.Elder)
                {
                    r.Beard = Pick(new[] { NpcBeard.None, NpcBeard.Short, NpcBeard.Moustache, NpcBeard.Full, NpcBeard.Goatee });
                }
            }

            if (age == NpcAge.Child)
            {
                r.Beard = NpcBeard.None;
                r.Garment = woman ? NpcGarment.Dress : NpcGarment.Tunic;
                r.Headwear = Chance(0.5f) ? NpcHeadwear.None : r.Headwear;
            }

            r.HeadColor = Pick(look.Trims);
            r.HeadTrim = Pick(look.Mains);
            r.Sleeves = Chance(0.3f) ? NpcSwatch.Cream : r.Main;
            r.Layers = Pick(look.Layers) | (Chance(0.25f) ? NpcLayer.Belt : NpcLayer.None);

            if (Chance(0.55f))
            {
                NpcProp prop = Pick(look.Props);
                r.Props = new[] { prop };
                switch (prop)
                {
                    case NpcProp.Jar:
                    case NpcProp.Cup:
                        r.LeftArm = NpcArmPose.HoldFront;
                        r.RightArm = NpcArmPose.HoldFront;
                        break;
                    case NpcProp.OilLamp:
                        r.RightArm = NpcArmPose.HoldUp;
                        break;
                    case NpcProp.WalkingStick:
                        r.RightArm = NpcArmPose.HoldStaff;
                        break;
                }
            }
            else if (Chance(0.4f))
            {
                r.RightArm = NpcArmPose.HipRest;
            }

            r.Gesture = r.Has(NpcProp.WalkingStick) ? NpcGesture.Lean : r.Has(NpcProp.Cup) ? NpcGesture.Sip
                : Pick(new[] { NpcGesture.None, NpcGesture.Chatter, NpcGesture.None, NpcGesture.Ponder });
            VillagerCache[key] = r;
            return r;
        }

        private static uint Hash(string text)
        {
            uint h = 2166136261u;
            foreach (char c in text)
            {
                h = (h ^ c) * 16777619u;
            }

            h ^= h >> 13;
            h *= 0x5bd1e995u;
            h ^= h >> 15;
            return h;
        }

        private static uint Rotate(uint h, int salt)
        {
            uint x = h + (uint)salt * 0x9E3779B9u;
            x ^= x >> 16;
            x *= 0x7feb352du;
            x ^= x >> 15;
            x *= 0x846ca68bu;
            x ^= x >> 16;
            return x;
        }
    }
}
