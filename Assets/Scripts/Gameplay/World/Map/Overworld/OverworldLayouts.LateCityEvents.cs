using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of three more capital events (Docs/Mapas100.md 54, 59 and 65; <see cref="Mechanics.LateCityEventsMechanicsDriver"/>), each in its
    /// region's own coordinates (the driver adds the region's origin). Farnkesh stands at <see cref="FarnkeshOffset"/> (0, 1080), the Porto da Lua
    /// Branca at <see cref="PortoLuaBrancaOffset"/> and Hinokura at <see cref="HinokuraOffset"/> (both (0, 0)), each on flat ground of ±30 m with its
    /// safe zone at ±26 m. Every place sits on one of the town's three roads (W, E, S), past the town's flat ground: no decor stands on a trail,
    /// the ground there was measured level (within 0.3 m at 6 m round; Farnkesh's sandstone pinnacles beside the roads stay more than 10 m off)
    /// and no clearing comes within 30 m.
    /// </summary>
    public static partial class OverworldLayouts
    {
        // ------------------------------------------------------------------
        // Vendaval (54, Farnkesh)
        // ------------------------------------------------------------------

        /// <summary>
        /// The three bridge moorings where the gusts fall (<see cref="FarnkeshGaleRules"/>), in order W, E, S: the ends of the West, East and South
        /// roads' rope bridges, 60, 65 and 40 m from the plaza (the East and South ones stand clear of the pinnacles that crowd the roads nearer).
        /// </summary>
        public static readonly Vector2[] FarnkeshBridgeMoorings =
        {
            new Vector2(-60f, 1080f), new Vector2(65f, 1083f), new Vector2(1f, 1040f),
        };

        // ------------------------------------------------------------------
        // Noite Polar (59, Porto da Lua Branca)
        // ------------------------------------------------------------------

        /// <summary>The three moon lanterns at the gates (<see cref="PortoPolarNightRules"/>), in order W, E, S, 40 m out on the roads.</summary>
        public static readonly Vector2[] PortoPolarLanterns =
        {
            new Vector2(-40f, -1.25f), new Vector2(40f, -1.25f), new Vector2(1.18f, -40f),
        };

        /// <summary>Where the Creeping Dark rises for each lantern (same order): 28 m farther out on the same road.</summary>
        public static readonly Vector2[] PortoPolarDarkRise =
        {
            new Vector2(-68f, -4.75f), new Vector2(68f, -4.75f), new Vector2(4.5f, -68f),
        };

        // ------------------------------------------------------------------
        // Grande Tremor (65, Hinokura)
        // ------------------------------------------------------------------

        /// <summary>
        /// The three lava channels at the gates (<see cref="HinokuraGreatTremorRules"/>), in order W, E, S: the mouth 24 m out on the road (the
        /// surge comes from the volcano's side), the diverter stone 42 m from the plaza, and the spill 8 m nearer the town (its 5 m pool stays out
        /// of the safe zone).
        /// </summary>
        public static readonly HinokuraLavaDiverter[] HinokuraLavaDiverters =
        {
            new HinokuraLavaDiverter(new Vector2(-66f, 3f), new Vector2(-42f, 1f), new Vector2(-34f, 0.8f)),
            new HinokuraLavaDiverter(new Vector2(66f, 8.5f), new Vector2(42f, 2.82f), new Vector2(34f, 1f)),
            new HinokuraLavaDiverter(new Vector2(6f, -66f), new Vector2(2f, -42f), new Vector2(0.7f, -34f)),
        };
    }
}
