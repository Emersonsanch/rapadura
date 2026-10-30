using System;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>Life stage: sets height and how big the head is (chibi proportions like the five cousins).</summary>
    public enum NpcAge { Child, Youth, Adult, Elder }

    /// <summary>Body width: slim, average, round in the belly (stout) or wide in the shoulders (broad).</summary>
    public enum NpcBuild { Slim, Average, Stout, Broad }

    /// <summary>How they stand: shoulders back (proud), forward (stooped elder), head low (shy), at ease (relaxed).</summary>
    public enum NpcPosture { Upright, Proud, Stooped, Shy, Relaxed }

    public enum NpcHair { Bald, Short, Long, Bun, Braid, TwoBraids, Ponytail, Curly, Topknot, Spiky }

    public enum NpcBeard { None, Moustache, Goatee, Short, Full, Long, LongBraided }

    public enum NpcHeadwear
    {
        None,
        /// <summary>Patterned headscarf tied under the chin (Vó Edna).</summary>
        Kerchief,
        /// <summary>Cloth hood over the head and shoulders.</summary>
        Hood,
        /// <summary>Wound cloth turban, three turns and a crown.</summary>
        Turban,
        /// <summary>Desert headwrap: two turns and a tail down the back.</summary>
        Headwrap,
        /// <summary>Tall felt cap with a split, turned-up brim (kalpak).</summary>
        FeltCap,
        /// <summary>Four-sided embroidered skullcap (doppa).</summary>
        Skullcap,
        /// <summary>Forge bandana with knotted tails.</summary>
        Bandana,
        /// <summary>Conical woven straw hat with a knob (Litema).</summary>
        StrawCone,
        /// <summary>Miner's leather helmet with a lamp on the forehead (trim colour = the lamp).</summary>
        LampHelmet,
        /// <summary>Thin band round the forehead.</summary>
        Headband,
        /// <summary>Band of little flowers.</summary>
        FlowerCrown,
        /// <summary>Band with a small sun disc on the brow.</summary>
        SunCirclet,
    }

    /// <summary>Main clothing silhouette (what the legs look like from far away).</summary>
    public enum NpcGarment
    {
        /// <summary>Plain long robe to the ankles.</summary>
        Robe,
        /// <summary>Long robe with a front band and a hem band.</summary>
        Kaftan,
        /// <summary>Knee-length padded coat over trousers, trimmed front and hem.</summary>
        Chapan,
        /// <summary>Thigh-length tunic over trousers.</summary>
        Tunic,
        /// <summary>Wide long dress with a hem band.</summary>
        Dress,
    }

    /// <summary>Layers worn over the garment (combinable).</summary>
    [Flags]
    public enum NpcLayer
    {
        None = 0,
        Apron = 1 << 0,
        Vest = 1 << 1,
        Sash = 1 << 2,
        Belt = 1 << 3,
        Shawl = 1 << 4,
        Cape = 1 << 5,
        /// <summary>Striped wool blanket over the shoulders (Litema).</summary>
        Blanket = 1 << 6,
        Scarf = 1 << 7,
        /// <summary>Shoulder strap and a bag on the hip.</summary>
        Satchel = 1 << 8,
        /// <summary>Patches on the garment (a life of hard roads).</summary>
        Patches = 1 << 9,
    }

    /// <summary>Static arm pose baked into the mesh (the idle gesture moves the arm around it).</summary>
    public enum NpcArmPose
    {
        Hang,
        /// <summary>Forearm forward, hands meeting in front of the belly (bowl, cup, jar, tablets).</summary>
        HoldFront,
        /// <summary>Arm a little forward and out, hand gripping a tall staff.</summary>
        HoldStaff,
        /// <summary>Hand on the hip, elbow out.</summary>
        HipRest,
        /// <summary>Forearm folded up, hand on top of the shoulder (a log, a pick).</summary>
        Carry,
        /// <summary>Forearm raised in front, holding something up to show.</summary>
        HoldUp,
        /// <summary>Forearm across, holding something at the side (a basket on the forearm).</summary>
        Crook,
    }

    /// <summary>Things they carry: the role reads from the iso camera before the name does.</summary>
    public enum NpcProp
    {
        Hammer, Tongs, HerbBasket, MortarPestle, CaravanStaff, Bedroll, CamelBell, RopeCoil, Brush, Bow, Quiver,
        HatFeathers, LanternPole, OilLamp, HandDrum, Lyre, Dutar, Flute, Jar, FabricBolt, Spools, Pick, OilCan, WickBundle,
        Log, CoinPurse, Scale, FlowerBasket, Letter, Quill, WaterJug, Keys, Crook, Spindle, Tablets, ScrollCase, Scroll,
        WalkingStick, CrystalPendant, SunPendant, Cup, MoonStaff, Goggles, SpiderPuppet, Bundle, Mantis,
    }

    /// <summary>The procedural idle each role plays (Docs/NPCs.md).</summary>
    public enum NpcGesture
    {
        None,
        /// <summary>Lifts the hammer and taps it down (the forge's rhythm).</summary>
        Hammer,
        /// <summary>Small circles of the pestle in the bowl.</summary>
        Grind,
        /// <summary>Raises an arm to call someone far away.</summary>
        Call,
        /// <summary>Little bounces of the hand: counting, twice.</summary>
        Count,
        /// <summary>Talks with both hands.</summary>
        Chatter,
        /// <summary>Slow stroking motion (a horse's neck).</summary>
        Stroke,
        /// <summary>Alternating taps on a drum.</summary>
        Drum,
        /// <summary>Twirls a spindle.</summary>
        Spin,
        /// <summary>Lifts the lantern slowly, as if lighting the way.</summary>
        LanternRaise,
        /// <summary>Leans on the staff, sways, nods.</summary>
        Lean,
        /// <summary>Head low, peeks up now and then.</summary>
        Shy,
        /// <summary>Now and then dries an eye with the back of the hand.</summary>
        WipeTear,
        /// <summary>Wide sweeps of both arms, telling a story.</summary>
        Storytell,
        /// <summary>Looks around slowly, reading the ground.</summary>
        Scan,
        /// <summary>Restless little hops.</summary>
        Bounce,
        /// <summary>Bears a weight on the shoulder: slow, steady bob.</summary>
        Carry,
        /// <summary>Tilts the jug as if pouring a measured drop.</summary>
        Pour,
        /// <summary>Scribbles with the quill.</summary>
        Write,
        /// <summary>Flaps the arms, imitating an animal.</summary>
        Mime,
        /// <summary>Presses the clay with both hands.</summary>
        Knead,
        /// <summary>Hand to chin, pondering.</summary>
        Ponder,
        /// <summary>Reads what is in the hands, looks up now and then.</summary>
        Read,
        /// <summary>An elegant tilt of the head and a small flourish.</summary>
        Preen,
        /// <summary>Lifts the cup to sip, slowly.</summary>
        Sip,
    }

    /// <summary>Face drawn on the head (flat decals, a few triangles).</summary>
    public enum NpcFace { Kind, Jolly, Stern, Bright, Shy, Dreamy, Calm }

    /// <summary>
    /// One NPC's look: body, head, clothing, colours, props and idle. Each named resident of the island has one in
    /// <see cref="NpcRecipes"/>, chosen from their story (Docs/WorldBible_Maniqueismo.md §3.1, Docs/NPCs.md); generic
    /// villagers get a seeded one from their region's palette. Plain data, built into a mesh by <see cref="NpcModelBuilder"/>.
    /// </summary>
    public sealed class NpcRecipe
    {
        public string Id;
        public BiomeType Region = BiomeType.Forest;

        public NpcAge Age = NpcAge.Adult;
        public NpcBuild Build = NpcBuild.Average;
        public NpcPosture Posture = NpcPosture.Upright;

        /// <summary>Height multiplier on the age's base height (1 = typical).</summary>
        public float Height = 1f;

        public NpcSwatch Skin = NpcSwatch.SkinLight;

        /// <summary>Hand colour when not the skin (clay-covered hands, leather gloves).</summary>
        public NpcSwatch? Hands;

        public NpcFace Face = NpcFace.Kind;
        public NpcHair Hair = NpcHair.Short;
        public NpcSwatch HairColor = NpcSwatch.HairDark;
        public NpcBeard Beard = NpcBeard.None;

        public NpcHeadwear Headwear = NpcHeadwear.None;
        public NpcSwatch HeadColor = NpcSwatch.Cream;
        public NpcSwatch HeadTrim = NpcSwatch.OldGold;

        public NpcGarment Garment = NpcGarment.Tunic;
        public NpcSwatch Main = NpcSwatch.Indigo;
        public NpcSwatch Trim = NpcSwatch.OldGold;
        public NpcSwatch Sleeves = NpcSwatch.Indigo;
        public NpcSwatch Trousers = NpcSwatch.Walnut;
        public NpcSwatch Boots = NpcSwatch.LeatherDark;

        public NpcLayer Layers = NpcLayer.None;
        public NpcSwatch LayerColor = NpcSwatch.Leather;
        public NpcSwatch LayerTrim = NpcSwatch.Brass;

        public NpcArmPose LeftArm = NpcArmPose.Hang;
        public NpcArmPose RightArm = NpcArmPose.Hang;
        public NpcProp[] Props = Array.Empty<NpcProp>();
        public NpcGesture Gesture = NpcGesture.None;

        /// <summary>One line of why they look like this (Docs/NPCs.md, the preview labels).</summary>
        public string Story = string.Empty;

        public bool Has(NpcLayer layer) => (Layers & layer) != 0;

        public bool Has(NpcProp prop) => Array.IndexOf(Props, prop) >= 0;

        /// <summary>A short signature of the silhouette (tests: two named NPCs never share one).</summary>
        public string SilhouetteKey =>
            $"{Age}/{Build}/{Posture}/{Hair}/{Beard}/{Headwear}/{Garment}/{(int)Layers}/{LeftArm}/{RightArm}/{string.Join(",", Props)}";
    }
}
