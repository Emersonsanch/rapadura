using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// Which text panels each <see cref="StoryBeat"/> plays, as <see cref="StoryTexts"/> keys
    /// (<c>story.&lt;beatId&gt;.&lt;n&gt;</c>). A beat may have one hero-focused panel
    /// (<c>story.&lt;beatId&gt;.hero.&lt;heroId&gt;</c>) where the narrator follows the cousin the player
    /// chose; without a hero that panel is simply left out. Pure data, so tests can check every key exists.
    /// </summary>
    public static class StoryScript
    {
        /// <summary>Placeholder in <see cref="Layout"/> for the chosen hero's panel.</summary>
        private const string HeroToken = "hero";

        private static readonly Dictionary<StoryBeat, string[]> Layout = new Dictionary<StoryBeat, string[]>
        {
            { StoryBeat.Prologue, new[] { "1", "2", HeroToken, "3" } },
            { StoryBeat.InterludeForest, new[] { "1", "2", "3" } },
            { StoryBeat.InterludeDesert, new[] { "1", "2", "3" } },
            { StoryBeat.InterludeMountain, new[] { "1", "2", "3" } },
            { StoryBeat.Ending, new[] { "1", "2", "3", HeroToken, "4", "5" } },
        };

        /// <summary>Sequence title key: <c>story.&lt;beatId&gt;.title</c> (null for None).</summary>
        public static string TitleKey(StoryBeat beat)
        {
            string id = StoryBeats.Id(beat);
            return id != null ? $"story.{id}.title" : null;
        }

        /// <summary>The hero part of a hero-focused key: the lower-case <see cref="CharacterId"/> (<c>maithe</c>, <c>icaro</c>...).</summary>
        public static string HeroId(CharacterId hero) => hero.ToString().ToLowerInvariant();

        /// <summary>Whether <paramref name="beat"/> has a panel that changes with the chosen hero.</summary>
        public static bool HasHeroPanel(StoryBeat beat)
        {
            return Layout.TryGetValue(beat, out string[] tokens) && System.Array.IndexOf(tokens, HeroToken) >= 0;
        }

        /// <summary>Panel text keys for <paramref name="beat"/>, in order; the hero panel only when <paramref name="hero"/> is known. Empty for None.</summary>
        public static List<string> GetPanelKeys(StoryBeat beat, CharacterId? hero)
        {
            var keys = new List<string>();
            string id = StoryBeats.Id(beat);
            if (id == null || !Layout.TryGetValue(beat, out string[] tokens))
            {
                return keys;
            }

            foreach (string token in tokens)
            {
                if (token == HeroToken)
                {
                    if (hero.HasValue)
                    {
                        keys.Add($"story.{id}.hero.{HeroId(hero.Value)}");
                    }

                    continue;
                }

                keys.Add($"story.{id}.{token}");
            }

            return keys;
        }

        /// <summary>Every key the script can ask for (all beats, every hero variant, titles): for coverage tests.</summary>
        public static IEnumerable<string> AllKeys()
        {
            foreach (StoryBeat beat in StoryBeats.InOrder)
            {
                yield return TitleKey(beat);

                foreach (string key in GetPanelKeys(beat, null))
                {
                    yield return key;
                }

                if (!HasHeroPanel(beat))
                {
                    continue;
                }

                foreach (CharacterId hero in (CharacterId[])System.Enum.GetValues(typeof(CharacterId)))
                {
                    yield return $"story.{StoryBeats.Id(beat)}.hero.{HeroId(hero)}";
                }
            }
        }
    }
}
