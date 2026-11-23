using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Betume Vivo comes out more at night (Docs/Mapas100.md, map 79: "Noite: as poças acesas iluminam; o Betume Vivo sai mais"), pure. At night a
    /// resting patch of pitch (<see cref="MisturaTarStage.Sticky"/>) that the hero wades into spits one Betume Vivo, then rests; a patch that is lit, burning
    /// or scorched spits nothing, so fire is the answer. And the Betume Vivo is pitch itself: in a burning patch it burns three times as hard as other beasts.
    /// <code>
    /// night only · hero inside a sticky patch · 1 Betume Vivo per patch every 90 s · at most 3 alive · level 318
    /// burning patch: Betume Vivo loses 12% of max life a tick (other beasts 4%) · gone at dawn or when the hero is 90 m away
    /// </code>
    /// </summary>
    public static class NightTarRules
    {
        public const string LivingTarId = "enemy_forest_living_tar";
        public const float PatchRestSeconds = 90f;
        public const int MaxAlive = 3;
        public const int Level = 318;
        public const float LeaveRange = 90f;
        public const float LivingTarBurnFraction = 0.12f;

        /// <summary>Whether the patch spits a Betume Vivo now.</summary>
        /// <param name="readyAt">When (seconds) the patch may spit again; 0 if it never did.</param>
        public static bool Spits(bool night, bool heroInPatch, MisturaTarStage stage, float now, float readyAt, int alive)
        {
            return night && heroInPatch && stage == MisturaTarStage.Sticky && now >= readyAt && alive < MaxAlive;
        }

        /// <summary>When the patch may spit again after spitting at <paramref name="now"/>.</summary>
        public static float NextReady(float now) => now + PatchRestSeconds;

        public static bool IsLivingTar(string enemyId) => enemyId == LivingTarId;

        /// <summary>One tick of a burning patch on a beast: the Betume Vivo catches like the pitch it is.</summary>
        public static float BurnTick(float maxHealth, bool livingTar) => Mathf.Max(0f, maxHealth) * (livingTar ? LivingTarBurnFraction : MisturaTarRules.BeastBurnFraction);

        public static bool Fades(bool night, float heroDistance) => !night || heroDistance > LeaveRange;
    }

    /// <summary>
    /// The Fumaça de Piche catches fire like the pools (Docs/Mapas100.md, map 79: "acender com o fogo do herói como as poças"), pure. The smoke drips
    /// pitch, so a fire skill of the hero that lands on it (its area + 2 m) or is cast from beside it (2 m) lights it: a short fuse, then it bursts.
    /// The burst bites the smoke itself hard, scorches the beasts round it, lights any patch of pitch it reaches (a chain the hero can plan) and
    /// singes the hero who stayed too close, never below 30% of the hero's life.
    /// <code>
    /// fuse 1.2 s · burst: smoke 35% of its max life · other beasts within 5 m 8% (bosses 1%) · patches within 5 m lit · hero within 3.5 m 4% (floor 30%)
    /// a smoke lights at most once every 10 s · at most 8 smokes per cast
    /// </code>
    /// </summary>
    public static class PitchSmokeIgniteRules
    {
        public const string PitchSmokeId = "enemy_mountain_pitch_smoke";
        public const float IgniteMargin = 2f;
        public const float FuseSeconds = 1.2f;
        public const float SelfFraction = 0.35f;
        public const float BurstRadius = 5f;
        public const float BeastFraction = 0.08f;
        public const float BossFraction = 0.01f;
        public const float HeroRadius = 3.5f;
        public const float HeroFraction = 0.04f;
        public const float HeroFloor = 0.3f;
        public const float RelightSeconds = 10f;
        public const int MaxPerCast = 8;

        public static bool IsPitchSmoke(string enemyId) => enemyId == PitchSmokeId;

        /// <summary>Whether a fire skill landing at <paramref name="landing"/> (area <paramref name="area"/>) or cast from <paramref name="caster"/> lights the smoke at <paramref name="smoke"/>.</summary>
        public static bool Reaches(Vector2 smoke, Vector2 landing, float area, Vector2 caster)
        {
            return Vector2.Distance(smoke, landing) <= Mathf.Max(0f, area) + IgniteMargin || Vector2.Distance(smoke, caster) <= IgniteMargin;
        }

        public static bool CanLight(float now, float lastLit) => lastLit <= 0f || now - lastLit >= RelightSeconds;

        public static float SelfBurn(float maxHealth) => Mathf.Max(0f, maxHealth) * SelfFraction;

        public static float BeastBurn(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? BossFraction : BeastFraction);

        public static bool InBurst(Vector2 smoke, Vector2 p) => Vector2.Distance(smoke, p) <= BurstRadius;

        /// <summary>Whether the burst lights a patch (the patch's rim within <see cref="BurstRadius"/> of the smoke).</summary>
        public static bool LightsPatch(MisturaTarPatch patch, Vector2 smoke) => Vector2.Distance(patch.Center, smoke) <= patch.Radius + BurstRadius;

        /// <summary>The singe on the hero: 4% of max life, never taking the hero below 30% of it.</summary>
        public static float HeroBurn(Vector2 smoke, Vector2 hero, float current, float max)
        {
            if (max <= 0f || current <= 0f || Vector2.Distance(smoke, hero) > HeroRadius)
            {
                return 0f;
            }

            return Mathf.Min(max * HeroFraction, Mathf.Max(0f, current - max * HeroFloor));
        }
    }
}
