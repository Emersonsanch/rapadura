using System;
using UnityEngine;

namespace Rapadura.Gameplay.PhotoMode
{
    /// <summary>
    /// How far the photo-mode camera may go, scaled to the hero's size (<see cref="ForHeroHeight"/>):
    /// the same limits frame a 1.8 m hero in an encounter arena and the 3.6-unit token on the world map.
    /// Pan is an offset of the orbit focus from the hero's chest, so the camera can never wander off
    /// to frame something else (or peek behind the arena dressing).
    /// </summary>
    [Serializable]
    public struct PhotoCameraLimits
    {
        public float MinDistance;
        public float MaxDistance;
        /// <summary>Degrees; negative looks up at the hero from below the chest.</summary>
        public float MinPitch;
        public float MaxPitch;
        /// <summary>Degrees either way (the "Inclinação" slider).</summary>
        public float MaxRoll;
        public float MinFov;
        public float MaxFov;
        /// <summary>Horizontal reach of the focus offset from the hero.</summary>
        public float MaxPanRadius;
        /// <summary>Vertical range of the focus offset (below / above the chest).</summary>
        public float MinPanHeight;
        public float MaxPanHeight;
        /// <summary>Hard cap of the camera's distance from the hero's chest, whatever the pan + zoom.</summary>
        public float MaxCameraRadius;

        /// <summary>Hero height (metres) the base limits were tuned for.</summary>
        public const float ReferenceHeroHeight = 1.8f;

        /// <summary>Limits for a hero <paramref name="heroHeight"/> tall (scale clamped to 0.5×–4× the reference).</summary>
        public static PhotoCameraLimits ForHeroHeight(float heroHeight)
        {
            float scale = Mathf.Clamp(heroHeight / ReferenceHeroHeight, 0.5f, 4f);
            if (float.IsNaN(scale))
            {
                scale = 1f;
            }

            return new PhotoCameraLimits
            {
                MinDistance = 1f * scale,
                MaxDistance = 8f * scale,
                MinPitch = -15f,
                MaxPitch = 80f,
                MaxRoll = 30f,
                MinFov = 20f,
                MaxFov = 80f,
                MaxPanRadius = 2.5f * scale,
                MinPanHeight = -0.8f * scale,
                MaxPanHeight = 1.5f * scale,
                MaxCameraRadius = 9f * scale,
            };
        }
    }

    /// <summary>Orbit camera state: yaw/pitch/roll in degrees around the (panned) focus, distance in metres, vertical FOV.</summary>
    [Serializable]
    public struct PhotoCameraState
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float Distance;
        public float Fov;
        public Vector3 Pan;
    }

    /// <summary>
    /// Pure orbit/zoom/pan math of the photo mode (<c>PhotoModeController</c> owns input and the scene
    /// queries): clamping to <see cref="PhotoCameraLimits"/>, the orbit pose, camera-relative panning,
    /// collision pull-in and frame-rate independent smoothing. No scene access, EditMode-tested.
    /// </summary>
    public static class PhotoCameraMath
    {
        /// <summary>The state with every axis inside <paramref name="limits"/> (yaw wrapped to [0, 360)).</summary>
        public static PhotoCameraState Clamp(PhotoCameraState state, PhotoCameraLimits limits)
        {
            state.Yaw = Mathf.Repeat(state.Yaw, 360f);
            state.Pitch = Mathf.Clamp(state.Pitch, limits.MinPitch, limits.MaxPitch);
            state.Roll = Mathf.Clamp(state.Roll, -limits.MaxRoll, limits.MaxRoll);
            state.Distance = Mathf.Clamp(state.Distance, limits.MinDistance, limits.MaxDistance);
            state.Fov = Mathf.Clamp(state.Fov, limits.MinFov, limits.MaxFov);
            state.Pan = ClampPan(state.Pan, limits);
            return state;
        }

        /// <summary>Focus offset limited to a cylinder: <see cref="PhotoCameraLimits.MaxPanRadius"/> wide, Min/MaxPanHeight tall.</summary>
        public static Vector3 ClampPan(Vector3 pan, PhotoCameraLimits limits)
        {
            var horizontal = new Vector2(pan.x, pan.z);
            float radius = Mathf.Max(0f, limits.MaxPanRadius);
            if (horizontal.sqrMagnitude > radius * radius)
            {
                horizontal = horizontal.normalized * radius;
            }

            return new Vector3(horizontal.x, Mathf.Clamp(pan.y, limits.MinPanHeight, limits.MaxPanHeight), horizontal.y);
        }

        /// <summary>Camera rotation for <paramref name="state"/>: pitch around the right axis, yaw around up, roll around the view axis.</summary>
        public static Quaternion Rotation(PhotoCameraState state) => Quaternion.Euler(state.Pitch, state.Yaw, state.Roll);

        /// <summary>Unit vector from the focus toward the camera (roll never changes it).</summary>
        public static Vector3 BackDirection(PhotoCameraState state) => Rotation(state) * Vector3.back;

        /// <summary>Camera position <paramref name="distance"/> behind the panned focus of <paramref name="pivot"/>.</summary>
        public static Vector3 Position(Vector3 pivot, PhotoCameraState state, float distance) =>
            pivot + state.Pan + BackDirection(state) * distance;

        /// <summary>
        /// Distance after a collision probe toward the camera: the hit distance minus a skin, never
        /// closer than <paramref name="minDistance"/> (the lens may then sit in a wall, but never inside the hero).
        /// </summary>
        public static float ResolveCollisionDistance(float desiredDistance, bool hit, float hitDistance, float minDistance, float skin)
        {
            if (!hit)
            {
                return desiredDistance;
            }

            float allowed = Mathf.Max(minDistance, hitDistance - Mathf.Max(0f, skin));
            return Mathf.Min(desiredDistance, allowed);
        }

        /// <summary><paramref name="position"/> pulled back inside a sphere of <paramref name="radius"/> around <paramref name="center"/>.</summary>
        public static Vector3 ClampToRadius(Vector3 position, Vector3 center, float radius)
        {
            Vector3 offset = position - center;
            float max = Mathf.Max(0f, radius);
            return offset.sqrMagnitude > max * max ? center + offset.normalized * max : position;
        }

        /// <summary>
        /// World-space pan for WASD / left stick: <paramref name="planar"/>.x strafes along the camera's
        /// yaw-only right, .y moves along its horizontal forward; <paramref name="vertical"/> is world up.
        /// </summary>
        public static Vector3 PlanarPanDelta(float yaw, Vector2 planar, float vertical)
        {
            Quaternion heading = Quaternion.Euler(0f, yaw, 0f);
            return heading * new Vector3(planar.x, 0f, planar.y) + Vector3.up * vertical;
        }

        /// <summary>
        /// Starting state that keeps the current gameplay view direction: yaw/pitch of the camera looking
        /// at <paramref name="focus"/>, distance and FOV clamped. A camera sitting on the focus keeps its own rotation.
        /// </summary>
        public static PhotoCameraState FromCamera(Vector3 cameraPosition, Quaternion cameraRotation, Vector3 focus, float fov, PhotoCameraLimits limits)
        {
            Vector3 toFocus = focus - cameraPosition;
            float distance = toFocus.magnitude;
            Quaternion look = distance > 0.0001f ? Quaternion.LookRotation(toFocus / distance, Vector3.up) : cameraRotation;
            Vector3 euler = look.eulerAngles;

            var state = new PhotoCameraState
            {
                Yaw = euler.y,
                Pitch = Mathf.DeltaAngle(0f, euler.x),
                Roll = 0f,
                Distance = distance > 0.0001f ? distance : limits.MaxDistance * 0.5f,
                Fov = fov,
                Pan = Vector3.zero,
            };

            return Clamp(state, limits);
        }

        /// <summary>Fraction of the remaining gap to close this frame (exponential, frame-rate independent). Sharpness ≤ 0 = snap.</summary>
        public static float DampFactor(float sharpness, float deltaTime)
        {
            if (sharpness <= 0f)
            {
                return 1f;
            }

            return 1f - Mathf.Exp(-sharpness * Mathf.Max(0f, deltaTime));
        }

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> (yaw along the shortest arc).</summary>
        public static PhotoCameraState Damp(PhotoCameraState current, PhotoCameraState target, float sharpness, float deltaTime)
        {
            float t = DampFactor(sharpness, deltaTime);
            return new PhotoCameraState
            {
                Yaw = Mathf.Repeat(current.Yaw + Mathf.DeltaAngle(current.Yaw, target.Yaw) * t, 360f),
                Pitch = Mathf.Lerp(current.Pitch, target.Pitch, t),
                Roll = Mathf.Lerp(current.Roll, target.Roll, t),
                Distance = Mathf.Lerp(current.Distance, target.Distance, t),
                Fov = Mathf.Lerp(current.Fov, target.Fov, t),
                Pan = Vector3.Lerp(current.Pan, target.Pan, t),
            };
        }

        /// <summary>
        /// Zoom step for a scroll reading: a wheel notch (±120 on Windows, ±1 on some platforms) scales the
        /// distance by <paramref name="perNotch"/> (0.9 = 10% closer when scrolling up).
        /// </summary>
        public static float ZoomDistance(float distance, float scroll, float perNotch = 0.9f)
        {
            if (Mathf.Abs(scroll) < 0.01f)
            {
                return distance;
            }

            float notches = Mathf.Abs(scroll) >= 60f ? scroll / 120f : scroll;
            return distance * Mathf.Pow(Mathf.Clamp(perNotch, 0.1f, 0.99f), notches);
        }
    }
}
