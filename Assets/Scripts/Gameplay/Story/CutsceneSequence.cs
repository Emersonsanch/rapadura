using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Story
{
    /// <summary>What just happened to a <see cref="CutsceneSequence"/> (audio / analytics hooks; nothing in gameplay depends on it).</summary>
    public enum CutsceneChange
    {
        Started = 0,
        ShotChanged = 1,
        Finished = 2,
        Skipped = 3,
    }

    /// <summary>Raised by <see cref="CutsceneSequence"/> on start / each new shot / finish / skip.</summary>
    public readonly struct CutsceneChangedEvent : IGameEvent
    {
        public readonly CutsceneId Cutscene;
        public readonly CutsceneChange Change;
        public readonly int ShotIndex;
        public readonly int ShotCount;

        public CutsceneChangedEvent(CutsceneId cutscene, CutsceneChange change, int shotIndex, int shotCount)
        {
            Cutscene = cutscene;
            Change = change;
            ShotIndex = shotIndex;
            ShotCount = shotCount;
        }
    }

    /// <summary>
    /// Clock of one cutscene on screen (plain C#, EditMode-tested): the current shot, how far into it the camera is,
    /// the blended screen tints, auto-advance when a shot's time runs out, tap-to-next and skip. The presenter
    /// (<c>CutscenePlayerController</c>) only draws it and moves the camera to <see cref="CutsceneCameraMath.Pose"/>.
    /// </summary>
    public class CutsceneSequence
    {
        private readonly float[] _durations;
        private float _shotTime;

        public CutsceneDefinition Definition { get; }
        public int Index { get; private set; }
        public int Count => Definition.Shots.Count;
        public bool IsFinished { get; private set; }
        public bool WasSkipped { get; private set; }
        public CutsceneShot CurrentShot => Definition.Shots[Mathf.Clamp(Index, 0, Count - 1)];
        public float CurrentDuration => Count > 0 ? _durations[Mathf.Clamp(Index, 0, Count - 1)] : 0f;

        /// <summary>0..1 through the current shot (eased by <see cref="CutsceneCameraMath.Ease"/> for the camera).</summary>
        public float ShotProgress => CurrentDuration > 0f ? Mathf.Clamp01(_shotTime / CurrentDuration) : 1f;

        /// <param name="duration">Seconds per shot (defaults to <see cref="CutsceneRules.DefaultShotSeconds"/>); the presenter passes reading time.</param>
        public CutsceneSequence(CutsceneDefinition definition, Func<CutsceneShot, float> duration = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _durations = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                float seconds = duration != null ? duration(Definition.Shots[i]) : CutsceneRules.DefaultShotSeconds;
                _durations[i] = Mathf.Max(CutsceneRules.MinShotSeconds, seconds);
            }

            IsFinished = Count == 0;
            if (!IsFinished)
            {
                Publish(CutsceneChange.Started);
            }
        }

        private void Publish(CutsceneChange change) => EventBus.Publish(new CutsceneChangedEvent(Definition.Id, change, Index, Count));

        /// <summary>Advances the clock by <paramref name="deltaSeconds"/> (real time); moves to the next shot when this one ends. True while still playing.</summary>
        public bool Tick(float deltaSeconds)
        {
            if (IsFinished)
            {
                return false;
            }

            _shotTime += Mathf.Max(0f, deltaSeconds);
            if (_shotTime >= CurrentDuration)
            {
                return Next();
            }

            return true;
        }

        /// <summary>Jumps to the next shot (tap / Space); finishes after the last. True while still playing.</summary>
        public bool Next()
        {
            if (IsFinished)
            {
                return false;
            }

            if (Index >= Count - 1)
            {
                IsFinished = true;
                Publish(CutsceneChange.Finished);
                return false;
            }

            Index++;
            _shotTime = 0f;
            Publish(CutsceneChange.ShotChanged);
            return true;
        }

        /// <summary>Ends the whole cutscene now ("Pular" / Esc / gamepad East).</summary>
        public void Skip()
        {
            if (IsFinished)
            {
                return;
            }

            WasSkipped = true;
            IsFinished = true;
            Publish(CutsceneChange.Skipped);
        }

        /// <summary>Cold-shadow tint (0..1) now: blends from the previous shot's value over the first half of the shot.</summary>
        public float Shadow => BlendTint(s => s.Shadow);

        /// <summary>Warm-light tint (0..1) now.</summary>
        public float Light => BlendTint(s => s.Light);

        private float BlendTint(Func<CutsceneShot, float> pick)
        {
            if (Count == 0)
            {
                return 0f;
            }

            float target = pick(CurrentShot);
            float previous = Index > 0 ? pick(Definition.Shots[Index - 1]) : 0f;
            return Mathf.Lerp(previous, target, Mathf.Clamp01(ShotProgress * 2f));
        }
    }

    /// <summary>Pure timing / presentation rules for cutscenes.</summary>
    public static class CutsceneRules
    {
        public const float DefaultShotSeconds = 4f;
        public const float MinShotSeconds = 2.5f;
        public const float MaxShotSeconds = 14f;

        /// <summary>Reading speed of the subtitles (characters per second, slow on purpose: 9-year-olds play this).</summary>
        public const float CharactersPerSecond = 14f;

        /// <summary>Seconds a line of <paramref name="characters"/> stays up: reading time + a breath, clamped.</summary>
        public static float ReadingSeconds(int characters) => Mathf.Clamp(1.2f + Mathf.Max(0, characters) / CharactersPerSecond, MinShotSeconds, MaxShotSeconds);

        /// <summary>
        /// Whether a cutscene plays: it exists, cutscenes are on (<paramref name="autoPlay"/> — PlayMode tests switch them
        /// off), and it has not already played for this beat in this session (quitting mid-panels must not loop it).
        /// </summary>
        public static bool ShouldPlay(CutsceneDefinition definition, bool autoPlay, bool alreadyPlayed) => definition != null && autoPlay && !alreadyPlayed;
    }

    /// <summary>
    /// Camera poses for <see cref="CutsceneFraming"/> (pure, EditMode-tested): an orbit around the focus point,
    /// eased between a shot's two framings. "Reduzir Movimento" holds each shot's end framing (a cut, no glide).
    /// </summary>
    public static class CutsceneCameraMath
    {
        /// <summary>Smoothstep: slow in, slow out.</summary>
        public static float Ease(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        /// <summary>Distance multiplier from the map's size: <paramref name="spread"/> = farthest node from the island centre (10 = 1×).</summary>
        public static float MapScale(float spread) => Mathf.Clamp(spread / 10f, 0.5f, 4f);

        /// <summary>The region a focus stands for (null for the whole island). The Vila is the Forest node.</summary>
        public static BiomeType? RegionOf(CutsceneFocus focus)
        {
            switch (focus)
            {
                case CutsceneFocus.Village:
                case CutsceneFocus.Forest: return BiomeType.Forest;
                case CutsceneFocus.Desert: return BiomeType.Desert;
                case CutsceneFocus.Mountain: return BiomeType.Mountain;
                case CutsceneFocus.Caves: return BiomeType.Caves;
                default: return null;
            }
        }

        /// <summary>
        /// World point of <paramref name="focus"/>: its region node, else the centre of every node (<paramref name="island"/>).
        /// <paramref name="nodePosition"/> answers null for a region the map lacks.
        /// </summary>
        public static Vector3 FocusPoint(CutsceneFocus focus, Func<BiomeType, Vector3?> nodePosition, Vector3 island)
        {
            BiomeType? region = RegionOf(focus);
            Vector3? node = region.HasValue && nodePosition != null ? nodePosition(region.Value) : null;
            return node ?? island;
        }

        /// <summary>Centre and spread (farthest point from the centre) of <paramref name="points"/>; zero / 10 when empty.</summary>
        public static void Island(IReadOnlyList<Vector3> points, out Vector3 centre, out float spread)
        {
            centre = Vector3.zero;
            spread = 10f;
            if (points == null || points.Count == 0)
            {
                return;
            }

            foreach (Vector3 p in points)
            {
                centre += p;
            }

            centre /= points.Count;
            spread = 0f;
            foreach (Vector3 p in points)
            {
                spread = Mathf.Max(spread, Vector3.Distance(p, centre));
            }

            spread = Mathf.Max(spread, 1f);
        }

        /// <summary>Camera position / rotation looking at <paramref name="focus"/> from yaw/pitch/distance.</summary>
        public static void Orbit(Vector3 focus, float yaw, float pitch, float distance, out Vector3 position, out Quaternion rotation)
        {
            rotation = Quaternion.Euler(Mathf.Clamp(pitch, 5f, 89f), yaw, 0f);
            position = focus - rotation * Vector3.forward * Mathf.Max(0.5f, distance);
        }

        /// <summary>
        /// The camera pose of <paramref name="shot"/> at <paramref name="progress"/> (0..1). Focus points come from
        /// <paramref name="focusPoint"/>; distances are multiplied by <paramref name="scale"/>. With
        /// <paramref name="reducedMotion"/> the pose is the shot's end framing whatever the progress.
        /// </summary>
        public static void Pose(CutsceneShot shot, float progress, Func<CutsceneFocus, Vector3> focusPoint, float scale, bool reducedMotion,
            out Vector3 position, out Quaternion rotation, out float fieldOfView)
        {
            float t = reducedMotion ? 1f : Ease(progress);
            CutsceneFraming a = shot.From;
            CutsceneFraming b = shot.To;
            Vector3 focusA = focusPoint != null ? focusPoint(a.Focus) : Vector3.zero;
            Vector3 focusB = focusPoint != null ? focusPoint(b.Focus) : Vector3.zero;
            Vector3 focus = Vector3.Lerp(focusA, focusB, t);
            float yaw = Mathf.LerpAngle(a.Yaw, b.Yaw, t);
            float pitch = Mathf.Lerp(a.Pitch, b.Pitch, t);
            float distance = Mathf.Lerp(a.Distance, b.Distance, t) * Mathf.Max(0.1f, scale);
            fieldOfView = Mathf.Clamp(Mathf.Lerp(a.FieldOfView, b.FieldOfView, t), 20f, 90f);
            Orbit(focus, yaw, pitch, distance, out position, out rotation);
        }
    }
}
