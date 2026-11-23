using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The service points of the race cities that stand in front of a prop rather than on it (<c>CityServicesMechanicsDriver</c>): the start
    /// of the Corrida de Shulgi and Shulgi's ribbon post in Aratta, and the mouth of the Grande Forja in the Salões de Bronze. City-local
    /// coordinates, like the props they belong to.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The start of the Corrida de Shulgi: the middle of the South street, between its two pairs of horn posts (z −8 and −15).</summary>
        public static readonly Vector2 ArattaRaceStart = new Vector2(0f, -11.5f);

        /// <summary>Shulgi's ribbon post: just on the plaza side of the east horn post (5.6, 9.4), where he trades the Fitas de Corrida.</summary>
        public static readonly Vector2 ArattaRibbonPost = new Vector2(5.6f, 8.2f);

        /// <summary>The mouth of the Grande Forja: a metre in front of its face (the forge faces the camera, −z; depth 3.2 m).</summary>
        public static Vector2 BronzeHallsForgeMouth => BronzeHallsGreatForge + new Vector2(0f, -2.6f);
    }
}
