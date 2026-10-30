using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Story
{
    /// <summary>The campaign's camera cutscenes (Docs/Narrative.md, "Cutscenes").</summary>
    public enum CutsceneId
    {
        None = -1,
        /// <summary>New game: the cousins arrive at Vó Edna's farm, the Crystal is dark, the shadow seeps out of the Caves.</summary>
        Intro = 0,
        /// <summary>After the final boss (ending panels), before the credits: the light runs home, ends on Vó Edna's porch.</summary>
        Outro = 1,
    }

    /// <summary>Whether a cutscene plays before its beat's story panels or after them (right before the credits).</summary>
    public enum CutscenePlacement
    {
        BeforePanels = 0,
        AfterPanels = 1,
    }

    /// <summary>Who speaks a subtitle line (portrait + name on the subtitle plate; the narrator has neither).</summary>
    public enum CutsceneSpeaker
    {
        Narrator = 0,
        VoEdna = 1,
        Maithe = 2,
        Maria = 3,
        Joaquim = 4,
        Icaro = 5,
        Lavine = 6,
    }

    /// <summary>What the camera looks at, resolved against the code-built world map's region nodes.</summary>
    public enum CutsceneFocus
    {
        /// <summary>Centre of every region node: the whole island.</summary>
        Island = 0,
        /// <summary>The Vila / Vó Edna's farm (the Forest node, see <c>VillageNpcCatalog.VillageRegion</c>).</summary>
        Village = 1,
        Forest = 2,
        Desert = 3,
        Mountain = 4,
        Caves = 5,
    }

    /// <summary>One camera framing: orbit around a <see cref="CutsceneFocus"/> (degrees / map units, see <c>CutsceneCameraMath</c>).</summary>
    public readonly struct CutsceneFraming
    {
        public readonly CutsceneFocus Focus;
        /// <summary>Degrees around the up axis; 0 = looking towards +z, like the map camera.</summary>
        public readonly float Yaw;
        /// <summary>Degrees down from the horizon.</summary>
        public readonly float Pitch;
        /// <summary>Distance from the focus, in map units before <c>CutsceneCameraMath.MapScale</c>.</summary>
        public readonly float Distance;
        public readonly float FieldOfView;

        public CutsceneFraming(CutsceneFocus focus, float yaw, float pitch, float distance, float fieldOfView = 50f)
        {
            Focus = focus;
            Yaw = yaw;
            Pitch = pitch;
            Distance = distance;
            FieldOfView = fieldOfView;
        }
    }

    /// <summary>
    /// One shot: the camera glides from <see cref="From"/> to <see cref="To"/> while one subtitle line is on screen.
    /// <see cref="Shadow"/> / <see cref="Light"/> (0..1) are the cold-shadow and warm-light screen tints at the end of
    /// the shot (they blend from the previous shot's values).
    /// </summary>
    public readonly struct CutsceneShot
    {
        public readonly CutsceneFraming From;
        public readonly CutsceneFraming To;
        public readonly CutsceneSpeaker Speaker;
        public readonly string LineKey;
        public readonly float Shadow;
        public readonly float Light;

        public CutsceneShot(CutsceneFraming from, CutsceneFraming to, CutsceneSpeaker speaker, string lineKey, float shadow = 0f, float light = 0f)
        {
            From = from;
            To = to;
            Speaker = speaker;
            LineKey = lineKey;
            Shadow = shadow;
            Light = light;
        }
    }

    /// <summary>A whole cutscene: title card key, placement relative to its beat, shots in order.</summary>
    public sealed class CutsceneDefinition
    {
        public CutsceneId Id { get; }
        public StoryBeat Beat { get; }
        public CutscenePlacement Placement { get; }
        public string TitleKey { get; }
        public IReadOnlyList<CutsceneShot> Shots { get; }

        public CutsceneDefinition(CutsceneId id, StoryBeat beat, CutscenePlacement placement, string titleKey, IReadOnlyList<CutsceneShot> shots)
        {
            Id = id;
            Beat = beat;
            Placement = placement;
            TitleKey = titleKey;
            Shots = shots ?? new CutsceneShot[0];
        }
    }

    /// <summary>
    /// The shot lists, as plain C# data (no ScriptableObject, no Timeline package — com.unity.timeline is not in the
    /// project and the map is built in code anyway, so a small code-driven player, <c>CutscenePlayerController</c>,
    /// reads these). Line keys live in <see cref="CutsceneTexts"/>. Also the per-region / per-hero boss stinger lines
    /// shown under the boss banner (<see cref="BossStingerKey"/>).
    /// </summary>
    public static class CutsceneScript
    {
        private static CutsceneFraming F(CutsceneFocus focus, float yaw, float pitch, float distance, float fov = 50f) => new CutsceneFraming(focus, yaw, pitch, distance, fov);

        private static CutsceneShot S(CutsceneFraming from, CutsceneFraming to, CutsceneSpeaker speaker, string line, float shadow = 0f, float light = 0f) =>
            new CutsceneShot(from, to, speaker, "cutscene." + line, shadow, light);

        public static readonly CutsceneDefinition Intro = new CutsceneDefinition(CutsceneId.Intro, StoryBeat.Prologue, CutscenePlacement.BeforePanels, "cutscene.intro.title", new[]
        {
            // Wide over the island, settling down towards the farm.
            S(F(CutsceneFocus.Island, -20f, 62f, 26f), F(CutsceneFocus.Island, 5f, 52f, 20f), CutsceneSpeaker.Narrator, "intro.1"),
            S(F(CutsceneFocus.Village, 25f, 48f, 11f), F(CutsceneFocus.Village, 10f, 42f, 8f), CutsceneSpeaker.Lavine, "intro.2"),
            S(F(CutsceneFocus.Village, 10f, 42f, 8f), F(CutsceneFocus.Village, -8f, 38f, 7f), CutsceneSpeaker.Joaquim, "intro.3"),
            S(F(CutsceneFocus.Village, -8f, 38f, 7f), F(CutsceneFocus.Village, -20f, 34f, 5.5f), CutsceneSpeaker.Maithe, "intro.4"),
            // The porch: low and close.
            S(F(CutsceneFocus.Village, 160f, 24f, 4.5f), F(CutsceneFocus.Village, 175f, 20f, 3.6f, 45f), CutsceneSpeaker.VoEdna, "intro.5"),
            // The dark Crystal: the cold creeps in.
            S(F(CutsceneFocus.Village, 0f, 30f, 6f), F(CutsceneFocus.Village, 0f, 46f, 9f), CutsceneSpeaker.Icaro, "intro.6", shadow: 0.25f),
            // The hook: the camera crosses the map to the Caves, a crack in an old stone.
            S(F(CutsceneFocus.Island, 0f, 50f, 18f), F(CutsceneFocus.Caves, 15f, 44f, 9f), CutsceneSpeaker.Narrator, "intro.7", shadow: 0.7f),
            S(F(CutsceneFocus.Caves, 15f, 44f, 9f), F(CutsceneFocus.Caves, 35f, 30f, 5f, 42f), CutsceneSpeaker.Maria, "intro.8", shadow: 0.85f),
            // Back on the porch for the blessing; the shadow lifts a little.
            S(F(CutsceneFocus.Village, 170f, 22f, 4f), F(CutsceneFocus.Village, 180f, 30f, 7f), CutsceneSpeaker.VoEdna, "intro.9", shadow: 0.2f),
        });

        public static readonly CutsceneDefinition Outro = new CutsceneDefinition(CutsceneId.Outro, StoryBeat.Ending, CutscenePlacement.AfterPanels, "cutscene.outro.title", new[]
        {
            S(F(CutsceneFocus.Island, 10f, 64f, 26f), F(CutsceneFocus.Island, -10f, 56f, 22f), CutsceneSpeaker.Narrator, "outro.1", light: 0.35f),
            S(F(CutsceneFocus.Caves, -20f, 36f, 7f), F(CutsceneFocus.Caves, 0f, 40f, 9f), CutsceneSpeaker.Lavine, "outro.2", light: 0.3f),
            // Following the light home: Mountain, then down across the Desert.
            S(F(CutsceneFocus.Mountain, 0f, 44f, 10f), F(CutsceneFocus.Desert, 0f, 44f, 10f), CutsceneSpeaker.Icaro, "outro.3", light: 0.3f),
            S(F(CutsceneFocus.Forest, -15f, 46f, 11f), F(CutsceneFocus.Village, 0f, 42f, 8f), CutsceneSpeaker.Joaquim, "outro.4", light: 0.25f),
            S(F(CutsceneFocus.Village, 0f, 42f, 8f), F(CutsceneFocus.Village, -15f, 36f, 6f), CutsceneSpeaker.Maithe, "outro.5", light: 0.25f),
            // Vó Edna's porch: the last shots never leave it (the credits end there too).
            S(F(CutsceneFocus.Village, 160f, 24f, 4.5f), F(CutsceneFocus.Village, 170f, 22f, 4f, 46f), CutsceneSpeaker.Maria, "outro.6", light: 0.3f),
            S(F(CutsceneFocus.Village, 170f, 22f, 4f, 46f), F(CutsceneFocus.Village, 178f, 20f, 3.5f, 44f), CutsceneSpeaker.VoEdna, "outro.7", light: 0.35f),
            S(F(CutsceneFocus.Village, 178f, 20f, 3.5f, 44f), F(CutsceneFocus.Village, 180f, 34f, 6.5f, 48f), CutsceneSpeaker.Narrator, "outro.8", light: 0.45f),
        });

        /// <summary>Every cutscene, for coverage tests.</summary>
        public static readonly CutsceneDefinition[] All = { Intro, Outro };

        /// <summary>
        /// The cutscene tied to <paramref name="beat"/> at <paramref name="placement"/>, or null. The intro only plays on the
        /// first campaign (a Nova Jornada+ has its own "o Cristal pisca de novo" prologue, the family is already home).
        /// </summary>
        public static CutsceneDefinition For(StoryBeat beat, CutscenePlacement placement, int newGamePlusCycle = 0)
        {
            foreach (CutsceneDefinition definition in All)
            {
                if (definition.Beat != beat || definition.Placement != placement)
                {
                    continue;
                }

                if (definition.Id == CutsceneId.Intro && newGamePlusCycle > 0)
                {
                    return null;
                }

                return definition;
            }

            return null;
        }

        /// <summary>The hero a speaker stands for (null for the narrator / Vó Edna).</summary>
        public static CharacterId? HeroOf(CutsceneSpeaker speaker)
        {
            switch (speaker)
            {
                case CutsceneSpeaker.Maithe: return CharacterId.Maithe;
                case CutsceneSpeaker.Maria: return CharacterId.Maria;
                case CutsceneSpeaker.Joaquim: return CharacterId.Joaquim;
                case CutsceneSpeaker.Icaro: return CharacterId.Icaro;
                case CutsceneSpeaker.Lavine: return CharacterId.Lavine;
                default: return null;
            }
        }

        /// <summary>Lower-case id of a speaker (<c>vo_edna</c>, <c>maithe</c>...): name key and portrait USS modifier.</summary>
        public static string SpeakerId(CutsceneSpeaker speaker) => speaker == CutsceneSpeaker.VoEdna ? "vo_edna" : speaker.ToString().ToLowerInvariant();

        /// <summary>Speaker name key (<c>cutscene.speaker.&lt;id&gt;</c>); null for the narrator.</summary>
        public static string SpeakerNameKey(CutsceneSpeaker speaker) => speaker == CutsceneSpeaker.Narrator ? null : "cutscene.speaker." + SpeakerId(speaker);

        /// <summary>The speaker matching a playable hero.</summary>
        public static CutsceneSpeaker SpeakerOf(CharacterId hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return CutsceneSpeaker.Maithe;
                case CharacterId.Maria: return CutsceneSpeaker.Maria;
                case CharacterId.Joaquim: return CutsceneSpeaker.Joaquim;
                case CharacterId.Icaro: return CutsceneSpeaker.Icaro;
                default: return CutsceneSpeaker.Lavine;
            }
        }

        /// <summary>
        /// Line key of the boss-intro stinger for <paramref name="region"/>: the active cousin's quip
        /// (<c>cutscene.stinger.&lt;region&gt;.&lt;hero&gt;</c>), or the narrator's line without a hero.
        /// </summary>
        public static string BossStingerKey(BiomeType region, CharacterId? hero)
        {
            string regionId = region.ToString().ToLowerInvariant();
            return hero.HasValue ? $"cutscene.stinger.{regionId}.{StoryScript.HeroId(hero.Value)}" : $"cutscene.stinger.{regionId}";
        }

        /// <summary>Every key the cutscenes and stingers can ask for (coverage tests).</summary>
        public static IEnumerable<string> AllKeys()
        {
            foreach (CutsceneDefinition definition in All)
            {
                yield return definition.TitleKey;
                foreach (CutsceneShot shot in definition.Shots)
                {
                    yield return shot.LineKey;
                    string nameKey = SpeakerNameKey(shot.Speaker);
                    if (nameKey != null)
                    {
                        yield return nameKey;
                    }
                }
            }

            foreach (BiomeType region in (BiomeType[])System.Enum.GetValues(typeof(BiomeType)))
            {
                yield return BossStingerKey(region, null);
                foreach (CharacterId hero in (CharacterId[])System.Enum.GetValues(typeof(CharacterId)))
                {
                    yield return BossStingerKey(region, hero);
                }
            }
        }
    }
}
