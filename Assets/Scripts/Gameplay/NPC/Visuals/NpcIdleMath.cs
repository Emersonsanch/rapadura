using UnityEngine;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>Offsets an idle gesture adds to the figure's bones on one frame (degrees, metres).</summary>
    public struct NpcPoseOffsets
    {
        /// <summary>Arm rotation around the shoulder: x = pitch (negative swings forward/up), z = roll outward.</summary>
        public Vector3 ArmL, ArmR;

        public float HeadPitch, HeadYaw, HeadRoll;
        public float BodyPitch, BodyBob;

        /// <summary>0..1: how much the head may follow the player this frame (1 = free to look).</summary>
        public float LookWeight;
    }

    /// <summary>
    /// The cheap procedural idle of the NPC figures (pure maths, unit-tested): breathing, the role gesture and the head turn
    /// toward the player. A handful of sines per figure per frame; nothing allocates.
    /// </summary>
    public static class NpcIdleMath
    {
        public const float MaxHeadYaw = 65f;
        public const float MaxHeadPitch = 20f;

        /// <summary>Breathing in -1..1 (a slow ~3.6 s cycle, offset by <paramref name="phase"/>).</summary>
        public static float Breath(float time, float phase) => Mathf.Sin((time + phase) * (Mathf.PI * 2f / 3.6f));

        /// <summary>Smooth 0→1→0 bump inside [start, start+length] of every <paramref name="period"/> seconds.</summary>
        public static float Pulse(float time, float period, float start, float length)
        {
            float u = Mathf.Repeat(time, period) - start;
            if (u <= 0f || u >= length)
            {
                return 0f;
            }

            return Mathf.Sin(u / length * Mathf.PI);
        }

        /// <summary>
        /// Head yaw/pitch (degrees, clamped) to look at <paramref name="localTarget"/>, given in the figure's space with the
        /// head pivot at the origin and the figure facing +z. Targets behind the figure give no turn (0, 0).
        /// </summary>
        public static Vector2 LookAngles(Vector3 localTarget)
        {
            var flat = new Vector2(localTarget.x, localTarget.z);
            if (flat.sqrMagnitude < 1e-4f)
            {
                return Vector2.zero;
            }

            float yaw = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            if (Mathf.Abs(yaw) > 120f)
            {
                return Vector2.zero;
            }

            float pitch = -Mathf.Atan2(localTarget.y, flat.magnitude) * Mathf.Rad2Deg;
            return new Vector2(Mathf.Clamp(yaw, -MaxHeadYaw, MaxHeadYaw), Mathf.Clamp(pitch, -MaxHeadPitch, MaxHeadPitch));
        }

        /// <summary>The role gesture at <paramref name="time"/> (seconds, already offset by the figure's phase).</summary>
        public static NpcPoseOffsets Gesture(NpcGesture gesture, float time)
        {
            var o = new NpcPoseOffsets { LookWeight = 1f };
            float t = time;
            float sway = Mathf.Sin(t * 0.9f) * 2f;
            o.ArmL.x = sway;
            o.ArmR.x = -sway;

            switch (gesture)
            {
                case NpcGesture.Hammer:
                {
                    float u = Mathf.Repeat(t, 2.6f);
                    float lift;
                    if (u < 0.9f) lift = Mathf.SmoothStep(0f, 1f, u / 0.9f);
                    else if (u < 1.05f) lift = 1f;
                    else if (u < 1.17f) lift = Mathf.Lerp(1f, -0.08f, (u - 1.05f) / 0.12f);
                    else lift = Mathf.Lerp(-0.08f, 0f, Mathf.Clamp01((u - 1.17f) / 0.3f));
                    o.ArmR.x = -75f * lift;
                    o.HeadPitch = 6f;
                    o.BodyBob = u > 1.1f && u < 1.3f ? -0.008f : 0f;
                    o.LookWeight = 1f - Mathf.Clamp01(lift) * 0.6f;
                    break;
                }
                case NpcGesture.Grind:
                    o.ArmR.x += Mathf.Sin(t * 4f) * 6f;
                    o.ArmR.z = Mathf.Cos(t * 4f) * 6f;
                    o.HeadPitch = 8f;
                    o.LookWeight = 0.7f;
                    break;
                case NpcGesture.Call:
                {
                    float k = Pulse(t, 6.5f, 1f, 2f);
                    o.ArmR.x = -150f * k;
                    o.ArmR.z = 18f * k;
                    o.HeadPitch = -12f * k;
                    o.BodyPitch = -3f * k;
                    o.LookWeight = 1f - k * 0.7f;
                    break;
                }
                case NpcGesture.Count:
                {
                    float k = Pulse(t, 3.6f, 0.2f, 1.8f);
                    o.ArmR.x = -k * (6f + Mathf.Sin(t * 18f) * 7f);
                    o.HeadPitch = 10f * k;
                    o.LookWeight = 1f - k * 0.6f;
                    break;
                }
                case NpcGesture.Chatter:
                    o.ArmL.x = -8f - Mathf.Max(0f, Mathf.Sin(t * 3.1f)) * 22f;
                    o.ArmR.x = -6f - Mathf.Max(0f, Mathf.Sin(t * 2.3f + 1f)) * 26f;
                    o.ArmR.z = 8f + Mathf.Sin(t * 1.7f) * 10f;
                    o.HeadRoll = Mathf.Sin(t * 1.3f) * 6f;
                    o.BodyBob = Mathf.Abs(Mathf.Sin(t * 3f)) * 0.006f;
                    break;
                case NpcGesture.Stroke:
                    o.ArmR.x = -14f + Mathf.Sin(t * 1.6f) * 14f;
                    o.HeadRoll = 7f;
                    o.HeadPitch = 4f;
                    break;
                case NpcGesture.Drum:
                    o.ArmR.x = -8f - Mathf.Max(0f, Mathf.Sin(t * 7f)) * 18f;
                    o.ArmL.x = Mathf.Max(0f, Mathf.Sin(t * 7f + Mathf.PI)) * -6f;
                    o.BodyBob = Mathf.Abs(Mathf.Sin(t * 3.5f)) * 0.012f;
                    o.HeadRoll = Mathf.Sin(t * 3.5f) * 4f;
                    break;
                case NpcGesture.Spin:
                    o.ArmR.z = Mathf.Sin(t * 5f) * 5f;
                    o.ArmR.x = Mathf.Sin(t * 2.5f) * 4f;
                    o.HeadPitch = 8f;
                    o.LookWeight = 0.8f;
                    break;
                case NpcGesture.LanternRaise:
                {
                    float k = Pulse(t, 7f, 0.5f, 3f);
                    o.ArmR.x = -22f * k;
                    o.ArmL.x = -14f * k;
                    o.HeadPitch = -8f * k;
                    break;
                }
                case NpcGesture.Lean:
                    o.BodyPitch = Mathf.Sin(t * 0.7f) * 2f;
                    o.HeadPitch = Pulse(t, 5f, 2f, 0.8f) * 9f;
                    break;
                case NpcGesture.Shy:
                {
                    float peek = Pulse(t, 6f, 3f, 1.4f);
                    o.HeadPitch = 6f - peek * 14f;
                    o.HeadYaw = Mathf.Sin(t * 0.6f) * 6f;
                    o.LookWeight = 0.35f + peek * 0.65f;
                    break;
                }
                case NpcGesture.WipeTear:
                {
                    float k = Pulse(t, 8f, 2f, 1.8f);
                    o.ArmL.x = -115f * k;
                    o.ArmL.z = -25f * k;
                    o.HeadPitch = 10f * k;
                    o.LookWeight = 1f - k;
                    break;
                }
                case NpcGesture.Storytell:
                    o.ArmL.z = 14f + Mathf.Sin(t * 1.4f) * 22f;
                    o.ArmR.z = 14f + Mathf.Sin(t * 1.4f + 1.2f) * 24f;
                    o.ArmR.x = -18f + Mathf.Sin(t * 1.1f) * 16f;
                    o.HeadYaw = Mathf.Sin(t * 0.9f) * 15f;
                    o.BodyBob = Mathf.Abs(Mathf.Sin(t * 1.4f)) * 0.008f;
                    break;
                case NpcGesture.Scan:
                    o.HeadYaw = Mathf.Sin(t * 0.5f) * 35f;
                    o.HeadPitch = 10f;
                    o.LookWeight = 0.85f;
                    break;
                case NpcGesture.Bounce:
                    o.BodyBob = Mathf.Abs(Mathf.Sin(t * 5.5f)) * 0.04f;
                    o.ArmL.x = Mathf.Sin(t * 5.5f) * 12f;
                    o.ArmR.x = -Mathf.Sin(t * 5.5f) * 12f;
                    break;
                case NpcGesture.Carry:
                    o.BodyBob = Mathf.Sin(t * 1.2f) * 0.006f;
                    o.ArmR.x = Mathf.Sin(t * 1.2f) * 1.5f;
                    break;
                case NpcGesture.Pour:
                {
                    float k = Pulse(t, 6f, 1f, 2.2f);
                    o.ArmL.x = -38f * k;
                    o.ArmL.z = -18f * k;
                    o.HeadPitch = 9f * k;
                    o.LookWeight = 1f - k * 0.7f;
                    break;
                }
                case NpcGesture.Write:
                    o.ArmR.x = Mathf.Sin(t * 9f) * 3f;
                    o.ArmR.z = Mathf.Sin(t * 6.5f) * 4f;
                    o.HeadPitch = 12f - Pulse(t, 5f, 3f, 1f) * 14f;
                    o.LookWeight = 0.5f + Pulse(t, 5f, 3f, 1f) * 0.5f;
                    break;
                case NpcGesture.Mime:
                {
                    float k = Pulse(t, 5f, 1f, 1.6f);
                    float flap = Mathf.Sin(t * 14f) * k;
                    o.ArmL.z = 35f * k + flap * 22f;
                    o.ArmR.z = 35f * k + flap * 22f;
                    o.HeadRoll = 10f * k;
                    o.BodyBob = Mathf.Abs(flap) * 0.01f;
                    break;
                }
                case NpcGesture.Knead:
                {
                    float press = Mathf.Max(0f, Mathf.Sin(t * 5f));
                    o.ArmL.x = 4f * press;
                    o.ArmR.x = 4f * press;
                    o.BodyBob = -press * 0.006f;
                    o.HeadPitch = 9f;
                    o.HeadRoll = Mathf.Sin(t * 1.25f) * 6f; // sings along with the wheel
                    o.LookWeight = 0.7f;
                    break;
                }
                case NpcGesture.Ponder:
                {
                    float k = Pulse(t, 7f, 1f, 3.2f);
                    o.ArmL.x = -95f * k;
                    o.ArmL.z = -30f * k;
                    o.HeadPitch = 6f * k;
                    o.HeadRoll = 5f * k;
                    break;
                }
                case NpcGesture.Read:
                {
                    float up = Pulse(t, 6f, 3f, 1.2f);
                    o.HeadPitch = 14f - up * 16f;
                    o.LookWeight = 0.4f + up * 0.6f;
                    break;
                }
                case NpcGesture.Preen:
                    o.HeadRoll = Mathf.Sin(t * 0.8f) * 8f;
                    o.ArmL.x += Pulse(t, 6f, 2f, 1.2f) * -12f;
                    o.BodyPitch = -1.5f;
                    break;
                case NpcGesture.Sip:
                {
                    float k = Pulse(t, 6f, 1.5f, 1.8f);
                    o.ArmL.x = -32f * k;
                    o.ArmR.x = -32f * k;
                    o.HeadPitch = -8f * k;
                    o.LookWeight = 1f - k * 0.8f;
                    break;
                }
            }

            return o;
        }
    }
}
