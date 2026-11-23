namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-eighth pass of beasts (bible §3.2.1, <c>PeaksOwnBeastRecipes</c>): the roles of the Planície de Mingcheng (Docs/Mapas100.md
        /// 42, 46) and of the Ermidas Suspensas of the Picos de Farnkesh (56) that the continuous world still played with another beast after the
        /// twenty-seventh (<see cref="WestOwn2Beasts"/>), placed in the groups of their sheets in the Mingcheng and Farnkesh layouts (their region
        /// constants now point here). Each keeps its stand-in's biome and numbers. The Eco Apagado also rises from the noise of the cliff
        /// (<c>HermitageEchoRules</c>, <c>FarnkeshSilenceMechanicsDriver</c>).
        /// </summary>
        public static class PeaksOwnBeasts
        {
            public const string DimmedEcho = Fark.Echo;
            public const string CrackedBell = Fark.CrackedBell;
            public const string TaotieServant = Ming.TaotieServant;
            public const string QiongqiCub = Ming.QiongqiCub;
            public const string LimeGuardian = Ming.LimeGuardian;
        }
    }
}
