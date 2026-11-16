using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The marked laurel trunks of the Copas de Garajonay (05, <c>GarajonayTrunkRules</c>, Zarafshan region coordinates =
    /// <see cref="GarajonayOffset"/> + the map's own): three pairs, each a shortcut over the canopy that only the Laurel Lizard climbs.
    /// Every end stands on the forest floor, outside the city and away from the streams, the clearings and the Velho da Névoa's ring.
    /// Written as plain numbers (not <see cref="GarajonayOffset"/> + …): static fields of two partial files have no set order.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public static readonly LaurelTrunkLink[] GarajonayTrunks =
        {
            // Over the west stream: from the ferns (samambaial) to the cascade's edge, without going round by the west bridge.
            new LaurelTrunkLink("louro_cascata_oeste", new Vector2(1153f, 250f), new Vector2(1110f, 200f)),

            // Over the east stream: from the moss (musgo) to the east cascade, without going round by the east bridge.
            new LaurelTrunkLink("louro_cascata_leste", new Vector2(1295f, 230f), new Vector2(1337f, 220f)),

            // Over the north forest: from the tall laurels (louros_altos) to the north trail, before the Velho da Névoa's ring,
            // without going back through the city.
            new LaurelTrunkLink("louro_do_velho", new Vector2(1275f, 372f), new Vector2(1187f, 401f)),
        };
    }
}
