using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// Procedural sound cues of the night species (Docs/Audio.md, "Bichos da noite"): one "call" when the creature
    /// arrives (and now and then while it prowls) and one cue at the start of each attack windup, played in 3D by
    /// <see cref="NightCreatureVoice"/> through <see cref="SfxEventRouter.PlayCue"/>. Same pattern as
    /// <c>FishingAudio</c>: the cues are added to the live <see cref="SfxEventMap"/> on first use (only missing ids —
    /// a cue authored in <c>Resources/Audio/SfxEventMap.asset</c>, e.g. with a real recording, always wins), using
    /// existing <see cref="ProceduralSfxPreset"/>s, so the shared table needs no edit.
    /// </summary>
    public static class NightCreatureAudio
    {
        public const string WerewolfHowl = "night.werewolf.howl";
        public const string WerewolfGrowl = "night.werewolf.growl";
        public const string VampireBatScreech = "night.vampire_bat.screech";
        public const string VampireBatBite = "night.vampire_bat.bite";
        public const string OwlScreech = "night.owl.screech";
        public const string OwlSwoop = "night.owl.swoop";
        public const string FireflyHum = "night.firefly.hum";
        public const string FireflyFlare = "night.firefly.flare";
        public const string PeccaryGrunt = "night.peccary.grunt";
        public const string PeccaryCharge = "night.peccary.charge";
        public const string PotooSong = "night.potoo.song";
        public const string PotooGape = "night.potoo.gape";

        private static SfxEventMap _ensuredMap;

        /// <summary>Arrival/prowl call of <paramref name="kind"/>, or null.</summary>
        public static string CallCue(ProceduralEnemyKind kind)
        {
            switch (kind)
            {
                case ProceduralEnemyKind.Werewolf: return WerewolfHowl;
                case ProceduralEnemyKind.VampireBat: return VampireBatScreech;
                case ProceduralEnemyKind.ShroudOwl: return OwlScreech;
                case ProceduralEnemyKind.FireflySwarm: return FireflyHum;
                case ProceduralEnemyKind.Peccary: return PeccaryGrunt;
                case ProceduralEnemyKind.Potoo: return PotooSong;
                default: return null;
            }
        }

        /// <summary>Attack-windup cue of <paramref name="kind"/>, or null.</summary>
        public static string AttackCue(ProceduralEnemyKind kind)
        {
            switch (kind)
            {
                case ProceduralEnemyKind.Werewolf: return WerewolfGrowl;
                case ProceduralEnemyKind.VampireBat: return VampireBatBite;
                case ProceduralEnemyKind.ShroudOwl: return OwlSwoop;
                case ProceduralEnemyKind.FireflySwarm: return FireflyFlare;
                case ProceduralEnemyKind.Peccary: return PeccaryCharge;
                case ProceduralEnemyKind.Potoo: return PotooGape;
                default: return null;
            }
        }

        /// <summary>Seconds between prowl calls of <paramref name="kind"/> (min, max); the Lobisomem howls rarely, the fireflies hum often.</summary>
        public static Vector2 CallInterval(ProceduralEnemyKind kind)
        {
            switch (kind)
            {
                case ProceduralEnemyKind.Werewolf: return new Vector2(12f, 18f);
                case ProceduralEnemyKind.FireflySwarm: return new Vector2(4f, 7f);
                case ProceduralEnemyKind.Potoo: return new Vector2(9f, 14f);
                default: return new Vector2(7f, 12f);
            }
        }

        /// <summary>The default cue table (fresh instances on every call).</summary>
        public static List<SfxEventMap.Cue> BuildCues()
        {
            return new List<SfxEventMap.Cue>
            {
                // Goofy "auuuu": a long rising-and-falling swell, low.
                Cue(WerewolfHowl, ProceduralSfxPreset.Swell, 210f, 0.42f, 6f, 0.06f),
                Cue(WerewolfGrowl, ProceduralSfxPreset.Bark, 170f, 0.4f, 0.8f, 0.08f),
                Cue(VampireBatScreech, ProceduralSfxPreset.Chirp, 2600f, 0.3f, 2f, 0.1f),
                Cue(VampireBatBite, ProceduralSfxPreset.Zap, 1700f, 0.32f, 0.5f, 0.08f),
                // Coruja-de-Fumaça: the torn-cloth screech, a falling hiss.
                Cue(OwlScreech, ProceduralSfxPreset.Zap, 2300f, 0.34f, 3f, 0.1f),
                Cue(OwlSwoop, ProceduralSfxPreset.Whoosh, 700f, 0.36f, 0.6f, 0.08f),
                Cue(FireflyHum, ProceduralSfxPreset.Buzz, 520f, 0.18f, 2f, 0.12f),
                Cue(FireflyFlare, ProceduralSfxPreset.Sparkle, 1800f, 0.3f, 0.5f, 0.1f),
                Cue(PeccaryGrunt, ProceduralSfxPreset.Bark, 140f, 0.32f, 1.2f, 0.15f),
                Cue(PeccaryCharge, ProceduralSfxPreset.Crunch, 320f, 0.36f, 0.35f, 0.12f),
                // The real urutau sings a falling, melancholic series: a descending chime.
                Cue(PotooSong, ProceduralSfxPreset.ChimeDown, 540f, 0.3f, 6f, 0.04f),
                Cue(PotooGape, ProceduralSfxPreset.Inhale, 900f, 0.3f, 0.6f, 0.08f),
            };
        }

        /// <summary>Adds the night creature cues <paramref name="map"/> lacks. Returns how many were added.</summary>
        public static int EnsureCues(SfxEventMap map)
        {
            if (map == null)
            {
                return 0;
            }

            int added = 0;
            foreach (SfxEventMap.Cue cue in BuildCues())
            {
                if (!map.TryGetCue(cue.id, out _))
                {
                    map.AddCue(cue);
                    added++;
                }
            }

            return added;
        }

        /// <summary>Plays <paramref name="cueId"/> at <paramref name="position"/> on the live router (no-op without one).</summary>
        public static void Play(string cueId, Vector3 position)
        {
            if (string.IsNullOrEmpty(cueId) || !ServiceLocator.TryGet(out SfxEventRouter router) || router == null || !router.IsInitialized)
            {
                return;
            }

            if (!ReferenceEquals(_ensuredMap, router.Map))
            {
                EnsureCues(router.Map);
                _ensuredMap = router.Map;
            }

            router.PlayCue(cueId, position);
        }

        private static SfxEventMap.Cue Cue(string id, ProceduralSfxPreset preset, float frequency, float volume, float cooldown, float pitchJitter)
        {
            return new SfxEventMap.Cue
            {
                id = id,
                category = AudioCategory.Sfx,
                volume = volume,
                cooldown = cooldown,
                maxVoices = 2,
                pausable = true,
                spatial = true,
                loop = false,
                ducksMusic = false,
                preset = preset,
                frequency = frequency,
                pitchJitter = pitchJitter,
                volumeJitter = 0.1f,
            };
        }
    }
}
