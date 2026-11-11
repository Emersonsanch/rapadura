using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The life of a patch of pitch (<see cref="MisturaTarRules.Advance"/>).</summary>
    public enum MisturaTarStage
    {
        /// <summary>The resting state: it slows whoever wades in.</summary>
        Sticky = 0,
        /// <summary>The warning: lit but not yet burning, the heat already thins the pitch so the hero can walk out at full pace.</summary>
        Fuse = 1,
        Burning = 2,
        /// <summary>Burnt hard: walks like a road until the pitch seeps back.</summary>
        Scorched = 3,
    }

    /// <summary>A patch of pitch (region coordinates).</summary>
    public readonly struct MisturaTarPatch
    {
        public readonly Vector2 Center;
        public readonly float Radius;

        public MisturaTarPatch(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    /// <summary>A patch's stage and the seconds left in it.</summary>
    public readonly struct MisturaTarState
    {
        public readonly MisturaTarStage Stage;
        public readonly float Remaining;

        public MisturaTarState(MisturaTarStage stage, float remaining)
        {
            Stage = stage;
            Remaining = remaining;
        }
    }

    /// <summary>
    /// Lighting the pitch of the Poços de Betume (Docs/Mapas100.md, map 79: "Fogo Vivo queima poças e abre caminho por 30 s; cuidado: queima
    /// todo mundo perto"), pure. Any fire skill lights a patch it reaches; there is always a fuse first, and the pitch only ever slows, so a hero
    /// with no fire still walks through. The fire bites the beasts in full (the kill is the hero's) but can never kill the hero. The way
    /// stays open for 30 s counted from the first flame.
    /// <code>
    /// sticky: hero × 0.55 · fuse 3.5 s (warning, full pace) · burning 5 s · scorched 25 s (burning + scorched = 30 s open) · then sticky again
    /// burn: 1 tick/s within the patch + 1.5 m · hero 3 % of max life, always leaving at least 1 · beasts 4 % of max life
    /// </code>
    /// </summary>
    public static class MisturaTarRules
    {
        public const float StickyFactor = 0.55f;
        public const float FuseSeconds = 3.5f;
        public const float BurnSeconds = 5f;
        public const float ScorchedSeconds = 25f;
        public const float BurnMargin = 1.5f;
        public const float BurnTickSeconds = 1f;
        public const float HeroBurnFraction = 0.03f;
        public const float BeastBurnFraction = 0.04f;

        /// <summary>How far past a patch's rim (m) a fire skill still lights it, on top of the skill's own area.</summary>
        public const float IgniteMargin = 2f;

        public static readonly MisturaTarState Resting = new MisturaTarState(MisturaTarStage.Sticky, 0f);

        /// <summary>Lights a resting patch; a patch already lit keeps its own clock.</summary>
        public static MisturaTarState Ignite(MisturaTarState state) => state.Stage == MisturaTarStage.Sticky ? new MisturaTarState(MisturaTarStage.Fuse, FuseSeconds) : state;

        /// <summary>The patch after <paramref name="deltaTime"/> seconds, through as many stages as the time covers.</summary>
        public static MisturaTarState Advance(MisturaTarState state, float deltaTime)
        {
            MisturaTarStage stage = state.Stage;
            float remaining = state.Remaining - deltaTime;
            while (stage != MisturaTarStage.Sticky && remaining <= 0f)
            {
                switch (stage)
                {
                    case MisturaTarStage.Fuse:
                        stage = MisturaTarStage.Burning;
                        remaining += BurnSeconds;
                        break;
                    case MisturaTarStage.Burning:
                        stage = MisturaTarStage.Scorched;
                        remaining += ScorchedSeconds;
                        break;
                    default:
                        stage = MisturaTarStage.Sticky;
                        break;
                }
            }

            return stage == MisturaTarStage.Sticky ? Resting : new MisturaTarState(stage, remaining);
        }

        /// <summary>The hero's pace on a patch: only the resting pitch holds them back.</summary>
        public static float SpeedFactor(MisturaTarStage stage) => stage == MisturaTarStage.Sticky ? StickyFactor : 1f;

        /// <summary>The patch under <paramref name="p"/>, or −1.</summary>
        public static int PatchAt(IReadOnlyList<MisturaTarPatch> patches, Vector2 p)
        {
            for (int i = 0; i < patches.Count; i++)
            {
                if ((p - patches[i].Center).sqrMagnitude <= patches[i].Radius * patches[i].Radius)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Whether a fire skill landing at <paramref name="point"/> with its area lights the patch.</summary>
        public static bool Reaches(MisturaTarPatch patch, Vector2 point, float areaRadius) => Vector2.Distance(patch.Center, point) <= patch.Radius + Mathf.Max(0f, areaRadius) + IgniteMargin;

        /// <summary>Whether <paramref name="p"/> is close enough to burn while the patch is on fire.</summary>
        public static bool InBurnReach(MisturaTarPatch patch, Vector2 p) => Vector2.Distance(patch.Center, p) <= patch.Radius + BurnMargin;

        /// <summary>One tick of fire on the hero: never enough to kill (at most current − 1, never below 0).</summary>
        public static float HeroBurn(float currentHealth, float maxHealth) => Mathf.Clamp(maxHealth * HeroBurnFraction, 0f, Mathf.Max(0f, currentHealth - 1f));

        /// <summary>One tick of fire on a beast.</summary>
        public static float BeastBurn(float maxHealth) => Mathf.Max(0f, maxHealth * BeastBurnFraction);
    }
}
