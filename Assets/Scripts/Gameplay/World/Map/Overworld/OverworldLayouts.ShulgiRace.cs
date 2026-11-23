using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The Corrida de Shulgi (map 26, <see cref="Mechanics.ShulgiRaceRules"/>), in the Costa das Monções' own coordinates (the driver adds the
    /// anchor zone's origin, <see cref="CostaAnchorZoneId"/>). The start is between the horn posts of Aratta's South street; the course loops
    /// west and east through the Savana dos Auroques (x −580…−170, z 730…920), clear of the Touro de Fumaça's ring and the stone ring
    /// north-east of the city, and finishes on the south road just outside the gate.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>Where [E] calls the race: between the two pairs of horn posts of the South street.</summary>
        public static Vector2 ShulgiRaceStart => ArattaOffset + CityTownRecipes.ArattaRaceStart;

        /// <summary>Shulgi's ribbon post: the plaza horn post on the east side, where he trades the Fitas de Corrida.</summary>
        public static Vector2 ShulgiRibbonPost => ArattaOffset + CityTownRecipes.ArattaRibbonPost;

        /// <summary>The finish: the south road, 35 m south of the plaza (15 m outside the gate).</summary>
        public static readonly Vector2 ShulgiRaceFinish = new Vector2(-380f, 915f);

        /// <summary>The race's gates, in order, ending at <see cref="ShulgiRaceFinish"/> (about 860 m from the start).</summary>
        public static readonly Vector2[] ShulgiRaceCourse =
        {
            new Vector2(-385f, 870f), new Vector2(-480f, 800f), new Vector2(-545f, 740f), new Vector2(-430f, 735f), new Vector2(-290f, 760f),
            new Vector2(-200f, 840f), new Vector2(-260f, 905f), ShulgiRaceFinish,
        };
    }
}
