using Rapadura.Gameplay.Enemies;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The cliff after O Sino que Não Para is stilled (Docs/Mapas100.md 56: "O Sino que Não Para, sino gigante que a sombra fez tocar sem fim;
    /// parado, o paredão fica em silêncio"), pure, on top of <see cref="FarnkeshSilenceRules"/>. With the bell beaten the hermitages are quieter:
    /// whatever fills the noise meter counts less (it joins Lavine and the night), and the meter falls back lower after an Echo.
    /// <code>
    /// bell stilled: what fills the meter ×0.7 (with Lavine and the night: 0.6 × 0.6 × 0.7 ≈ 0.25) · after an Echo the meter is 35 (not 50)
    /// running from empty: full in 12.5 s → ~17.9 s; an Echo every 6.25 s → every ~11.6 s
    /// </code>
    /// Decision (2026-11-23): the silence is felt, not switched off: the Ecos still answer whoever runs the whole cliff, but a cousin who walks
    /// the stretches between the ladders barely wakes them.
    /// </summary>
    public static class HermitageStillBellRules
    {
        public const string BellEnemyId = RegionBossRules.EndlessBellId;
        public const float StilledFactor = 0.7f;
        public const float StilledAfterEcho = 35f;

        /// <summary>The save mark of the cousin's line on the first walk over the stilled cliff (set once, on that event).</summary>
        public const string FirstSilenceKey = "farnkesh.silence.bell_stilled";

        /// <summary>The factor on what fills the meter: Lavine, the night and the stilled bell.</summary>
        public static float Quiet(bool lavine, bool night, bool stilled) => FarnkeshSilenceRules.Quiet(lavine, night) * (stilled ? StilledFactor : 1f);

        /// <summary>The meter after an Echo struck.</summary>
        public static float AfterEcho(bool stilled) => stilled ? StilledAfterEcho : FarnkeshSilenceRules.AfterEcho;
    }
}
