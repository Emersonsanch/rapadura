using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of "o painel do aviso" of the Ruínas dos Artífices (08, <c>ArtificesInscriptionRules</c>), in the Areias' own coordinates.</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The Ruínas dos Artífices (08): its zone.</summary>
        public static readonly Rect ArtificesRuinsBounds = Rect.MinMaxRect(-700f, -450f, -100f, 0f);

        /// <summary>
        /// The three inscription stelae beside the avenue of ushabtis (the stelae of <c>MapLayouts.Artifices</c>: (−10.5, −50), (10.5, −80) and
        /// (−10.5, −110) of the map, moved by <see cref="ArtificesOffset"/> (−400, −150); written out, since static fields of other parts of
        /// this partial class may not be set yet), read in this order going south.
        /// </summary>
        public static readonly Vector2[] ArtificesInscriptionStelae =
        {
            new Vector2(-410.5f, -200f),
            new Vector2(-389.5f, -230f),
            new Vector2(-410.5f, -260f),
        };
    }
}
