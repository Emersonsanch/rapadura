using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the last capital events (Docs/Mapas100.md 71, 77 and 91; <see cref="Mechanics.FinalCityEventsMechanicsDriver"/>), each in its
    /// region's own coordinates (the driver adds the region's origin). The Cais de Mesektet, Dur-Jarras and the Cais da Coluna all stand at (0, 0)
    /// of their regions on flat ground of ±30 m with the safe zone at ±26 m and their streets leaving W, E and S at 19 m; the roads go on as
    /// straight trails, (±30, 0) → (±150, 20) and (0, −30) → (10, −120) at the quay, (±30, 0) → (±150, 10) and (0, −30) → (10, −130) at Dur-Jarras,
    /// (±30, 0) → (±150, 8) and (0, −30) → (60, −140) at the Cais da Coluna. Every road place below sits on its trail 42 m from the plaza, past the
    /// town's flat ground, 30 m and more from the generated clearings round each town.
    /// </summary>
    public static partial class OverworldLayouts
    {
        // ------------------------------------------------------------------
        // A Barca Atrasou (71, Cais de Mesektet)
        // ------------------------------------------------------------------

        /// <summary>The three posts the Shadow ties the Mesektet to (<see cref="MesektetLateBarqueRules"/>), in order W, E, S, on the quay's roads.</summary>
        public static readonly Vector2[] MesektetShadowPosts =
        {
            new Vector2(-42f, 2f), new Vector2(42f, 2f), new Vector2(1.33f, -42f),
        };

        /// <summary>Where the guards of each post come out of the water (same order): 26 m farther out on the same road.</summary>
        public static readonly Vector2[] MesektetScarabRise =
        {
            new Vector2(-68f, 6.33f), new Vector2(68f, 6.33f), new Vector2(4.22f, -68f),
        };

        /// <summary>The Mesektet at the Cais da Barca, in the middle of the quay (the recipe's <c>CityTownRecipes.MesektetBarque</c>).</summary>
        public static readonly Vector2 MesektetBarqueBerth = new Vector2(0f, 4.2f);

        // ------------------------------------------------------------------
        // A Cheia (77, Dur-Jarras)
        // ------------------------------------------------------------------

        /// <summary>Where the flood washes the library's tablets out (<see cref="DurJarrasFloodRules"/>), in order W, E, S, on the town's roads.</summary>
        public static readonly Vector2[] DurJarrasFloodTablets =
        {
            new Vector2(-42f, 1f), new Vector2(42f, 1f), new Vector2(1.2f, -42f),
        };

        /// <summary>The foot of the ziggurat's steps, in front of the Relógio de Zurvan (the clock at (0, 4.2), the ziggurat at (0, 13.2)): the tablets go up here.</summary>
        public static readonly Vector2 DurJarrasZigguratFoot = new Vector2(0f, -1f);

        // ------------------------------------------------------------------
        // Chegada da Caravana (91, Cais da Coluna)
        // ------------------------------------------------------------------

        /// <summary>The three piers of the Barques of Light that take the gifts down (<see cref="CaisCaravanRules"/>), in order W, E, S, on the quay's roads.</summary>
        public static readonly Vector2[] CaisCaravanPiers =
        {
            new Vector2(-42f, 0.8f), new Vector2(42f, 0.8f), new Vector2(6f, -41f),
        };

        /// <summary>Where Ammo's caravan moors and unloads: the south side of the plaza, south of the five fountains and between the two sky barges.</summary>
        public static readonly Vector2 CaisCaravanMooring = new Vector2(0f, -10f);
    }
}
