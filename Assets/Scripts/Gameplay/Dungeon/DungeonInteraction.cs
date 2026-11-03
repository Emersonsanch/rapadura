using Rapadura.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Something in a dungeon the hero uses by walking up to it (a barred gate, a sarcophagus, the legendary chest). There is
    /// no global "interact" button in the game (tap-to-move on mobile): like <c>CityResidentPost</c>, the prop reacts when the
    /// hero comes within <see cref="DungeonRules.InteractRadius"/> (<see cref="DungeonProximity"/>), and <see cref="Interact"/>
    /// stays public for a future tap-to-use or an online server.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Whether using it now would do anything (closed gate, unopened sarcophagus).</summary>
        bool CanInteract { get; }

        /// <summary>Uses it as <paramref name="actor"/> (the hero). True when something happened.</summary>
        bool Interact(GameObject actor);

        /// <summary>The "you can use me" glow, on while the hero is near and <see cref="CanInteract"/>.</summary>
        void SetHighlighted(bool highlighted);
    }

    /// <summary>
    /// Walk-up trigger shared by the dungeon's <see cref="IInteractable"/> props: polls the hero's flat distance (no physics
    /// trigger needed), highlights the prop in range and fires <see cref="IInteractable.Interact"/> once per approach (re-armed
    /// beyond <see cref="DungeonRules.InteractRearmRadius"/>). The rule itself is pure (<see cref="Step"/>).
    /// </summary>
    public sealed class DungeonProximity
    {
        private const string PlayerTag = "Player";
        private const float FindInterval = 1f;

        private Transform _hero;
        private float _findTimer;
        private bool _armed = true;
        private bool _highlighted;

        public Transform Hero => _hero;

        /// <summary>Pure: given the flat distance, returns whether to fire now and updates the armed flag.</summary>
        public static bool Step(float flatDistance, ref bool armed)
        {
            if (flatDistance > DungeonRules.InteractRearmRadius)
            {
                armed = true;
                return false;
            }

            if (armed && flatDistance <= DungeonRules.InteractRadius)
            {
                armed = false;
                return true;
            }

            return false;
        }

        public void Tick(IInteractable target, Transform self, float deltaTime)
        {
            if (target == null || self == null)
            {
                return;
            }

            if (_hero == null)
            {
                _findTimer -= deltaTime;
                if (_findTimer > 0f)
                {
                    return;
                }

                _findTimer = FindInterval;
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _hero = player != null ? player.transform : null;
                if (_hero == null)
                {
                    return;
                }
            }

            Vector3 offset = _hero.position - self.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            bool near = distance <= DungeonRules.InteractRearmRadius && target.CanInteract;
            if (near != _highlighted)
            {
                _highlighted = near;
                target.SetHighlighted(near);
            }

            if (Step(distance, ref _armed) && target.CanInteract)
            {
                target.Interact(_hero.gameObject);
            }
        }
    }

    /// <summary>Small presentation helpers of the dungeon: SFX through the router, localized floating texts.</summary>
    public static class DungeonFx
    {
        public const string GateCue = "dungeon.gate_open";
        public const string StoneSlideCue = "dungeon.stone_slide";
        public const string AmbushCue = "dungeon.ambush";
        public const string WallCrumbleCue = "dungeon.wall_crumble";
        public const string ArrowCue = "dungeon.arrow";
        public const string RaiseCue = "dungeon.raise";
        public const string LockedCue = "dungeon.locked";

        public static readonly string[] AllCues = { GateCue, StoneSlideCue, AmbushCue, WallCrumbleCue, ArrowCue, RaiseCue, LockedCue };

        /// <summary>Plays a dungeon cue at <paramref name="position"/> (silent without a router, e.g. in EditMode).</summary>
        public static void Play(string cueId, Vector3 position)
        {
            if (Application.isPlaying && ServiceLocator.TryGet(out SfxEventRouter router) && router != null && router.IsInitialized)
            {
                router.PlayCue(cueId, position);
            }
        }

        public static string Text(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return DungeonTexts.Get(localization, key);
        }

        public static string Format(string key, params object[] args)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return DungeonTexts.Format(localization, key, args);
        }

        /// <summary>A white floating line over <paramref name="position"/>.</summary>
        public static void Say(Vector3 position, string text)
        {
            if (Application.isPlaying && !string.IsNullOrEmpty(text))
            {
                CombatFxManager.SpawnLootText(position, text);
            }
        }

        /// <summary>A warning-coloured floating line over <paramref name="position"/>.</summary>
        public static void Warn(Vector3 position, string text)
        {
            if (Application.isPlaying && !string.IsNullOrEmpty(text))
            {
                CombatFxManager.SpawnLootWarningText(position, text);
            }
        }
    }
}
