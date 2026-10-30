using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// Which text panels each <see cref="StoryBeat"/> plays, as <see cref="StoryTexts"/> keys
    /// (<c>story.&lt;beatId&gt;.&lt;n&gt;</c>). A beat may have one hero-focused panel
    /// (<c>story.&lt;beatId&gt;.hero.&lt;heroId&gt;</c>) where the narrator follows the cousin the player
    /// chose; without a hero that panel is simply left out. Pure data, so tests can check every key exists.
    ///
    /// On a "Nova Jornada+" (<see cref="NewGamePlusProgress.Cycle"/> 1+) the prologue plays its
    /// new-journey variant instead (<c>story.new_journey.*</c>: Vó Edna sees the Cristal do Sol flicker
    /// again); every other beat is the same in every cycle.
    /// </summary>
    public static class StoryScript
    {
        /// <summary>Script id of the prologue on a Nova Jornada+ (not a <see cref="StoryBeat"/>: it replaces the prologue).</summary>
        public const string NewJourneyId = "new_journey";

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

        /// <summary>Panels of the Nova Jornada+ prologue (<see cref="NewJourneyId"/>).</summary>
        private static readonly string[] NewJourneyLayout = { "1", "2", HeroToken, "3" };

        /// <summary>The script id <paramref name="beat"/> plays in <paramref name="newGamePlusCycle"/>: <see cref="NewJourneyId"/> for the prologue of a Nova Jornada+, else <see cref="StoryBeats.Id"/>.</summary>
        public static string ScriptId(StoryBeat beat, int newGamePlusCycle = 0)
        {
            return beat == StoryBeat.Prologue && newGamePlusCycle > 0 ? NewJourneyId : StoryBeats.Id(beat);
        }

        /// <summary>Sequence title key: <c>story.&lt;scriptId&gt;.title</c> (null for None).</summary>
        public static string TitleKey(StoryBeat beat, int newGamePlusCycle = 0)
        {
            string id = ScriptId(beat, newGamePlusCycle);
            return id != null ? $"story.{id}.title" : null;
        }

        /// <summary>The hero part of a hero-focused key: the lower-case <see cref="CharacterId"/> (<c>maithe</c>, <c>icaro</c>...).</summary>
        public static string HeroId(CharacterId hero) => hero.ToString().ToLowerInvariant();

        /// <summary>Whether <paramref name="beat"/> has a panel that changes with the chosen hero.</summary>
        public static bool HasHeroPanel(StoryBeat beat, int newGamePlusCycle = 0)
        {
            string[] tokens = TokensFor(beat, newGamePlusCycle);
            return tokens != null && System.Array.IndexOf(tokens, HeroToken) >= 0;
        }

        /// <summary>
        /// Panel text keys for <paramref name="beat"/>, in order; the hero panel only when <paramref name="hero"/> is
        /// known. Empty for None. <paramref name="newGamePlusCycle"/> 1+ picks the Nova Jornada+ prologue.
        /// </summary>
        public static List<string> GetPanelKeys(StoryBeat beat, CharacterId? hero, int newGamePlusCycle = 0)
        {
            var keys = new List<string>();
            string id = ScriptId(beat, newGamePlusCycle);
            string[] tokens = TokensFor(beat, newGamePlusCycle);
            if (id == null || tokens == null)
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

        private static string[] TokensFor(StoryBeat beat, int newGamePlusCycle)
        {
            if (beat == StoryBeat.Prologue && newGamePlusCycle > 0)
            {
                return NewJourneyLayout;
            }

            return Layout.TryGetValue(beat, out string[] tokens) ? tokens : null;
        }

        /// <summary>Every key the script can ask for (all beats, the Nova Jornada+ prologue, every hero variant, titles): for coverage tests.</summary>
        public static IEnumerable<string> AllKeys()
        {
            foreach (int cycle in new[] { 0, 1 })
            {
                foreach (StoryBeat beat in StoryBeats.InOrder)
                {
                    if (cycle > 0 && ScriptId(beat, cycle) == StoryBeats.Id(beat))
                    {
                        continue; // same panels as the first campaign
                    }

                    yield return TitleKey(beat, cycle);

                    foreach (string key in GetPanelKeys(beat, null, cycle))
                    {
                        yield return key;
                    }

                    if (!HasHeroPanel(beat, cycle))
                    {
                        continue;
                    }

                    foreach (CharacterId hero in (CharacterId[])System.Enum.GetValues(typeof(CharacterId)))
                    {
                        yield return $"story.{ScriptId(beat, cycle)}.hero.{HeroId(hero)}";
                    }
                }
            }
        }
    }
}
