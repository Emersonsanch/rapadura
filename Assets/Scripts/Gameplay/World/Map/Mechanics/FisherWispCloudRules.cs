using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The cloud of marsh lights the freed Pescador de Luzes lets go (Docs/Mapas100.md 33: "O Pescador de Luzes, bagre gigante que engoliu
    /// fogos-fátuos para 'ter luz só para ele'; libertado, solta uma nuvem de faíscas"), pure and server-runnable (unix seconds in, so every
    /// player of a server sees the same cloud).
    /// <code>
    /// the Pescador falls → a time mark (the unix second, written only then)
    /// the cloud: 120 s over his ring, 12 m round it: alive inside, 5 % of max mana a second, up to full
    /// the freed lights fly over the swamp: for 600 s the lilac night mist of Tarchon hides no beast (as with a lantern)
    /// </code>
    /// Decision (2026-11-23): the cloud pays the fight back (mana, the boss drains it with his lure) and the lights then do what the Lanterna de
    /// Fogo-Fátuo does, for ten minutes and for free, so beating him at night is worth it. He comes back as every map boss does; each victory
    /// lets a new cloud go.
    /// </summary>
    public static class FisherWispCloudRules
    {
        public const long CloudSeconds = 120L;
        public const float CloudRadius = 12f;
        public const float ManaPerSecond = 0.05f;
        public const long NightClearSeconds = 600L;

        /// <summary>Read back when the save has no mark: never freed.</summary>
        public const long NeverFreed = 0L;

        /// <summary>Time mark (the unix second of the last victory over the Pescador de Luzes).</summary>
        public const string FreedKey = "tarchon.fisher.freed";

        public static bool Freed(long freedAt) => freedAt > 0L;

        public static bool CloudOn(long freedAt, long now) => Freed(freedAt) && now >= freedAt && now - freedAt < CloudSeconds;

        public static bool NightClear(long freedAt, long now) => Freed(freedAt) && now >= freedAt && now - freedAt < NightClearSeconds;

        public static long CloudSecondsLeft(long freedAt, long now) => CloudOn(freedAt, now) ? CloudSeconds - (now - freedAt) : 0L;

        public static bool InCloud(Vector2 arena, Vector2 p) => (p - arena).sqrMagnitude <= CloudRadius * CloudRadius;

        /// <summary>Mana given back over <paramref name="seconds"/> inside the cloud, never past full.</summary>
        public static float ManaOver(float current, float max, float seconds) => ZarafshanGardenRules.MendOver(current, max, ManaPerSecond, seconds);
    }
}
