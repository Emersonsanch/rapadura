namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The slide of one hero down a dune (<see cref="DuneSongRules"/>): the metres come down since the slide began. Climbing more than
    /// <see cref="DuneSongRules.ClimbBreak"/> or slowing under <see cref="DuneSongRules.MinSlideSpeed"/> breaks it.
    /// </summary>
    public sealed class DuneSlide
    {
        /// <summary>Metres come down in this slide.</summary>
        public float Descent { get; private set; }

        private float _climb;

        /// <summary>
        /// Feeds one frame: <paramref name="dy"/> is the hero's height change, <paramref name="horizontalSpeed"/> its speed over the sand (m/s).
        /// True on the frame the slide reaches <see cref="DuneSongRules.SlideDrop"/> (the slide starts again from zero).
        /// </summary>
        public bool Feed(float dy, float horizontalSpeed)
        {
            if (horizontalSpeed < DuneSongRules.MinSlideSpeed)
            {
                Reset();
                return false;
            }

            if (dy < 0f)
            {
                Descent -= dy;
                _climb = 0f;
            }
            else
            {
                _climb += dy;
                if (_climb > DuneSongRules.ClimbBreak)
                {
                    Reset();
                    return false;
                }
            }

            if (Descent < DuneSongRules.SlideDrop)
            {
                return false;
            }

            Reset();
            return true;
        }

        public void Reset()
        {
            Descent = 0f;
            _climb = 0f;
        }
    }

    /// <summary>
    /// The song of the Dunas que Cantam (Docs/Mapas100.md 10: "canto da duna: escorregar duna abaixo faz a areia cantar e assusta os comuns por
    /// 4 s (a cada 30 s; não vale em chefe)"), pure and server-runnable. A hero who runs down a dune face (4 m come down without stopping, at
    /// 2 m/s or more, climbing no more than 0.4 m on the way) makes the sand sing: every beast within 14 m of the hero, bosses excepted, is
    /// frozen with fright for 4 s (a stun). The dune sings once every 30 s at most; a slide in between is just a slide. Nothing on the post or in
    /// the crescent lake.
    /// <code>
    /// slide: 4 m down, ≥ 2 m/s, climb ≤ 0.4 m · song: beasts within 14 m stunned 4 s, bosses never · once per 30 s
    /// </code>
    /// </summary>
    public static class DuneSongRules
    {
        public const float SlideDrop = 4f;
        public const float MinSlideSpeed = 2f;
        public const float ClimbBreak = 0.4f;
        public const float CooldownSeconds = 30f;
        public const float ScareSeconds = 4f;
        public const float ScareRadius = 14f;

        /// <summary>Whether the dune may sing at <paramref name="now"/> after it last sang at <paramref name="lastSong"/> (seconds; NaN or −∞ = never).</summary>
        public static bool CanSing(float lastSong, float now) => float.IsNaN(lastSong) || now - lastSong >= CooldownSeconds;

        /// <summary>Seconds until the dune may sing again (0 when it may).</summary>
        public static float CooldownLeft(float lastSong, float now) => CanSing(lastSong, now) ? 0f : CooldownSeconds - (now - lastSong);

        /// <summary>Whether the song frightens a beast <paramref name="distance"/> metres from the hero.</summary>
        public static bool Scares(bool isBoss, float distance) => !isBoss && distance <= ScareRadius;
    }
}
