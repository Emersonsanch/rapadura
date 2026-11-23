namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-seventh pass of beasts (bible §3.2.1, <c>WestOwn2BeastRecipes</c>): the roles of the Costa das Monções (Docs/Mapas100.md
        /// 25–29), the Delta de Jhilpur (33, 35) and the Planície de Mingcheng (41, 46, the Vale da Névoa) that the continuous world still played
        /// with another beast after the twenty-fourth (<see cref="WestOwnBeasts"/>), placed in the groups of their sheets in the Costa, Delta and
        /// Mingcheng layouts (their region constants now point here). Each keeps its stand-in's biome and numbers. The Carneiro de Pedra (27), the
        /// Dingonek (29) and the Lanterna de Charada (41) got a group of their own; the wolves of the Teto's junipers, pastures and snowfields went
        /// to the Lobo de Gelo that already had the glacier (<see cref="TetoIceWolf"/>).
        /// </summary>
        public static class WestOwn2Beasts
        {
            public const string StripedJackal = Coast.StripedJackal;
            public const string SmokeHyena = Coast.SmokeHyena;
            public const string StoneRam = Coast.StoneRam;
            public const string Dingonek = Coast.Dingonek;
            public const string MossCrocodile = Delta.MossCrocodile;
            public const string ShadowGharial = Delta.ShadowGharial;
            public const string ShenMist = Ming.ShenMist;
            public const string HundunBubble = Ming.HundunBubble;
            public const string RiddleLantern = Ming.RiddleLantern;

            /// <summary>Not a beast of this pass (MidStandInBeastRecipes made it): the wolf packs of the Teto it took over here.</summary>
            public const string TetoIceWolf = MidBeasts.IceWolf;
        }
    }
}
