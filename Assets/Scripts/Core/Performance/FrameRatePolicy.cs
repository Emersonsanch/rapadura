namespace Rapadura.Core.Performance
{
    /// <summary>What the player is currently doing, as far as frame pacing cares.</summary>
    public enum FrameRateContext
    {
        /// <summary>Main menu, character select or any scene that is not gameplay.</summary>
        Menu,

        /// <summary>WorldMap.unity — mostly static overworld, token movement only.</summary>
        WorldMap,

        /// <summary>Main.unity (real-time combat/exploration) while not paused.</summary>
        Combat,

        /// <summary>Gameplay paused (pause menu open) — screen is essentially static.</summary>
        Paused
    }

    /// <summary>Frame pacing values to push into Unity (Application.targetFrameRate / QualitySettings.vSyncCount / OnDemandRendering).</summary>
    public readonly struct FrameRateSettings
    {
        /// <summary>Value for <c>Application.targetFrameRate</c>; -1 = platform default (uncapped / vSync-driven).</summary>
        public readonly int TargetFrameRate;

        /// <summary>Value for <c>QualitySettings.vSyncCount</c> (ignored by mobile OSes, which always vSync).</summary>
        public readonly int VSyncCount;

        /// <summary>Value for <c>OnDemandRendering.renderFrameInterval</c>; 1 = render every frame.</summary>
        public readonly int RenderFrameInterval;

        public FrameRateSettings(int targetFrameRate, int vSyncCount, int renderFrameInterval)
        {
            TargetFrameRate = targetFrameRate;
            VSyncCount = vSyncCount;
            RenderFrameInterval = renderFrameInterval;
        }

        public override string ToString()
        {
            return $"target={TargetFrameRate}, vSync={VSyncCount}, renderInterval={RenderFrameInterval}";
        }
    }

    /// <summary>
    /// Pure, Unity-API-free decision table for battery/thermal-friendly frame pacing, kept apart
    /// from <see cref="FrameRateManager"/> so EditMode tests can cover every branch without
    /// touching the real Application/QualitySettings state.
    ///
    /// Policy (see Docs/PlatformRequirements.md "Orçamento de performance"):
    ///   - Desktop: vSync on (1), targetFrameRate -1 — the monitor paces the game, no cap needed.
    ///   - Mobile: vSync off (mobile always presents at display rate anyway), 30 fps in menus,
    ///     world map and pause; 60 fps in combat when the player keeps "60 FPS em combate" on,
    ///     otherwise 30 everywhere (battery saver). While paused, rendering also drops to every
    ///     other frame (15 fps visual) — the pause screen is static, input is still polled at 30 Hz.
    /// </summary>
    public static class FrameRatePolicy
    {
        public const int MobileLowFrameRate = 30;
        public const int MobileHighFrameRate = 60;
        public const int DesktopVSyncCount = 1;
        public const int PausedRenderFrameInterval = 2;

        public static FrameRateSettings Resolve(FrameRateContext context, bool isMobile, bool highFrameRateInCombat)
        {
            if (!isMobile)
            {
                return new FrameRateSettings(-1, DesktopVSyncCount, 1);
            }

            int target = context == FrameRateContext.Combat && highFrameRateInCombat
                ? MobileHighFrameRate
                : MobileLowFrameRate;
            int renderInterval = context == FrameRateContext.Paused ? PausedRenderFrameInterval : 1;

            return new FrameRateSettings(target, 0, renderInterval);
        }

        /// <summary>
        /// Maps the raw state (active scene, pause/menu flags) to a <see cref="FrameRateContext"/>.
        /// Pause and an open main menu win over the scene; scene names are compared case-sensitively
        /// like SceneManager does. Any unrecognized scene counts as <see cref="FrameRateContext.Menu"/>.
        /// </summary>
        public static FrameRateContext ResolveContext(string activeSceneName, string combatSceneName, string worldMapSceneName, bool isPaused, bool isMenuOpen)
        {
            if (isPaused)
            {
                return FrameRateContext.Paused;
            }

            if (isMenuOpen)
            {
                return FrameRateContext.Menu;
            }

            if (!string.IsNullOrEmpty(activeSceneName))
            {
                if (activeSceneName == combatSceneName)
                {
                    return FrameRateContext.Combat;
                }

                if (activeSceneName == worldMapSceneName)
                {
                    return FrameRateContext.WorldMap;
                }
            }

            return FrameRateContext.Menu;
        }
    }
}
