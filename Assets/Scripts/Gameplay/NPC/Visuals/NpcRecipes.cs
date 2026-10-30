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
