using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Audio
{
    /// <summary>
    /// The music of an area of the continuous world (pure, EditMode-tested), with the tracks that exist
    /// (<see cref="SfxEventMap"/> music entries; a composer only replaces clips): towns play the calm title theme, open
    /// country the world theme by day and the slow story theme at night, wild ground at night and areas far above the hero
    /// the encounter theme of the region's biome (forest / desert / mountain / caves). Bosses keep their own track
    /// (<see cref="MusicDirector"/> on the encounter).
    /// </summary>
    public static class AreaMusicRules
    {
        public static void Pick(AreaMood mood, bool night, BiomeType biome, out MusicContext context, out string variant)
        {
            variant = string.Empty;
            switch (mood)
            {
                case AreaMood.Town:
                    context = MusicContext.Title;
                    return;
                case AreaMood.Danger:
                    context = MusicContext.Encounter;
                    variant = BiomeVariant(biome);
                    return;
                case AreaMood.Wild:
                    context = night ? MusicContext.Encounter : MusicContext.WorldMap;
                    variant = night ? BiomeVariant(biome) : string.Empty;
                    return;
                default:
                    context = night ? MusicContext.Story : MusicContext.WorldMap;
                    return;
            }
        }

        /// <summary>The encounter music variant of <paramref name="biome"/> (same key as <see cref="MusicDirector"/> uses for encounters).</summary>
        public static string BiomeVariant(BiomeType biome) => SfxEventMap.Normalize(biome.ToString());
    }
}
