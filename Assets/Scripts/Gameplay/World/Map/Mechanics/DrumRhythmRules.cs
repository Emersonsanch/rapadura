using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Ritmo do Tambor of the Tambor Rubro, the orcs' race city (map 11, Docs/Mapas100.md §3.3: "a cada 10 min os tambores tocam e, por
    /// 60 s, quem está na cidade ganha +10% de recuperação de fôlego e as tarefas de caça dão +1 ponto"), pure and server-runnable: the beat
    /// runs on the UTC clock (the same for every player, like the fog of Garajonay), so online the server says when the drums play.
    /// <code>
    /// cycle 600 s: 60 s of drums · 540 s quiet · 10 s before the drums a warning
    /// in the city (its ±34 m) while the drums play: +10% stamina recovery (refreshed short effect, ends by itself)
    /// a Tarefa de caça completed while they play, by a hero in the Ermos do Tambor Rubro (the canyons carry the drums): +1 Ponto de Caça
    /// </code>
    /// Decision (2026-11-16): the task point counts in the whole map 11, not only in the city, since there are no beasts inside the palisade.
    /// The cousins' drumming minigame (like the Hinos do Bema) is not part of it yet.
    /// </summary>
    public static class DrumRhythmRules
    {
        public const double CycleSeconds = 600d;
        public const double BeatSeconds = 60d;
        public const double WarningSeconds = 10d;
        public const float StaminaFactor = 1.1f;
        public const int HuntBonusPoints = 1;

        /// <summary>The half size of the city the drums are heard in at full strength (the recipe's ±34 m).</summary>
        public const float CityHalfSize = 34f;

        /// <summary>Second of the cycle at <paramref name="now"/> (UTC seconds), always in [0, <see cref="CycleSeconds"/>).</summary>
        public static double CycleTime(double now)
        {
            double t = now % CycleSeconds;
            return t < 0d ? t + CycleSeconds : t;
        }

        /// <summary>Whether the drums play at <paramref name="now"/> (the first <see cref="BeatSeconds"/> of each cycle).</summary>
        public static bool Playing(double now) => CycleTime(now) < BeatSeconds;

        /// <summary>Whether the drums are about to play (the last <see cref="WarningSeconds"/> of the quiet).</summary>
        public static bool Warning(double now) => CycleTime(now) >= CycleSeconds - WarningSeconds;

        /// <summary>Which beat <paramref name="now"/> belongs to (one number per cycle, to say things once per beat).</summary>
        public static long BeatIndex(double now) => (long)Math.Floor(now / CycleSeconds);

        /// <summary>Seconds until the next beat starts (0 while the drums play).</summary>
        public static double SecondsToBeat(double now) => Playing(now) ? 0d : CycleSeconds - CycleTime(now);

        /// <summary>The city of the drums round <paramref name="cityCentre"/>.</summary>
        public static Rect CityRect(Vector2 cityCentre) =>
            Rect.MinMaxRect(cityCentre.x - CityHalfSize, cityCentre.y - CityHalfSize, cityCentre.x + CityHalfSize, cityCentre.y + CityHalfSize);

        /// <summary>The stamina recovery factor of a hero: <see cref="StaminaFactor"/> in the city while the drums play and the hero lives.</summary>
        public static float HeroStaminaFactor(bool playing, bool inCity, bool alive) => playing && inCity && alive ? StaminaFactor : 1f;

        /// <summary>Extra Pontos de Caça for a task completed now: <see cref="HuntBonusPoints"/> while the drums play, in the Ermos.</summary>
        public static int TaskBonusPoints(bool playing, bool inBadlands) => playing && inBadlands ? HuntBonusPoints : 0;
    }
}
