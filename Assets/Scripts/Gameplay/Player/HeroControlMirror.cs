using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// A short swap of the hero's steps (Qiongqi, map 46: "troca o certo pelo errado"). While it lasts, <see cref="PlayerInputHandler.MoveInput"/>
    /// (keyboard, gamepad and the HUD joystick) points the other way and a click/tap on the ground sends the hero to the point mirrored
    /// through the hero (<see cref="PlayerClickToMove"/>); a click/tap on an enemy still targets that enemy, so nobody playing by touch is
    /// locked out of the fight. The only reads it changes are those two lines.
    ///
    /// It ends by itself: it is an end time on the game clock, never a flag someone must clear, and any swap is capped at
    /// <see cref="MaxSeconds"/>; so a dead hero, a fight that ends or a scene that changes cannot leave it on. <see cref="End"/> just ends it sooner.
    /// </summary>
    public static class HeroControlMirror
    {
        /// <summary>No swap lasts longer than this, whoever asks.</summary>
        public const float MaxSeconds = 4f;

        private static float _until = -1f;

        /// <summary>True while the steps are swapped.</summary>
        public static bool IsActive => IsActiveAt(Time.time, _until);

        /// <summary>Seconds left of the swap (0 when off).</summary>
        public static float SecondsLeft => Mathf.Max(0f, _until - Time.time);

        /// <summary>Swaps the steps for <paramref name="seconds"/> from now (capped; a swap already running is only ever extended up to the cap).</summary>
        public static void Begin(float seconds) => _until = EndTime(Time.time, _until, seconds);

        /// <summary>Gives the steps back now.</summary>
        public static void End() => _until = -1f;

        /// <summary>The move vector as the hero walks it right now.</summary>
        public static Vector2 Apply(Vector2 move) => IsActive ? MirrorMove(move) : move;

        /// <summary>The ground point a click/tap at <paramref name="point"/> sends the hero at <paramref name="hero"/> to right now.</summary>
        public static Vector3 ApplyDestination(Vector3 hero, Vector3 point) => IsActive ? MirrorPoint(hero, point) : point;

        // ---- Pure rules (EditMode-testable) ----

        public static bool IsActiveAt(float now, float until) => now < until;

        /// <summary>The end time of a swap of <paramref name="seconds"/> asked at <paramref name="now"/>, over one ending at <paramref name="currentUntil"/>: never shortened, never past now + <see cref="MaxSeconds"/>.</summary>
        public static float EndTime(float now, float currentUntil, float seconds)
        {
            float asked = now + Mathf.Clamp(seconds, 0f, MaxSeconds);
            return Mathf.Min(Mathf.Max(currentUntil, asked), now + MaxSeconds);
        }

        /// <summary>Left is right and up is down.</summary>
        public static Vector2 MirrorMove(Vector2 move) => -move;

        /// <summary>The point mirrored through the hero on the ground plane (same distance, opposite side); keeps the hero's height.</summary>
        public static Vector3 MirrorPoint(Vector3 hero, Vector3 point) => new Vector3(2f * hero.x - point.x, hero.y, 2f * hero.z - point.z);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _until = -1f;
    }
}
