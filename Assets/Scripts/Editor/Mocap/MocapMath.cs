using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Editor.Mocap
{
    /// <summary>
    /// One retarget link: the BVH joint whose world rotation drives a Unity humanoid bone, plus the
    /// "child" on each side used to measure the bone's direction when normalizing both rest poses to an
    /// ideal T-pose (see <see cref="MocapMath.TPoseDirection"/>).
    /// </summary>
    public readonly struct RetargetLink
    {
        public readonly HumanBodyBones Bone;
        public readonly string BvhJoint;
        public readonly string BvhChild;          // null -> no direction normalization
        public readonly HumanBodyBones UnityChild; // LastBone -> no direction normalization

        public RetargetLink(HumanBodyBones bone, string bvhJoint, string bvhChild, HumanBodyBones unityChild)
        {
            Bone = bone; BvhJoint = bvhJoint; BvhChild = bvhChild; UnityChild = unityChild;
        }
    }

    /// <summary>Pure math used by the BVH → Humanoid importer. Kept free of scene/asset access so it is unit-testable.</summary>
    public static class MocapMath
    {
        /// <summary>CMU ASF/AMC length unit ((1/0.45) inch) in meters — the cgspeed BVH conversion keeps these units.</summary>
        public const float CmuUnitToMeters = 0.0254f / 0.45f;

        /// <summary>
        /// BVH is right-handed (Y up, character faces +Z, its left on +X). Unity is left-handed with the same Y/Z,
        /// so we mirror X: the character still faces +Z and its left side lands on Unity's −X (correct for Unity).
        /// </summary>
        public static Vector3 RightToLeft(Vector3 v) => new Vector3(-v.x, v.y, v.z);

        /// <summary>Mirror of a rotation across the YZ plane: M·R·M with M = diag(−1,1,1) ⇒ (x, −y, −z, w).</summary>
        public static Quaternion RightToLeft(Quaternion q) => new Quaternion(q.x, -q.y, -q.z, q.w);

        /// <summary>
        /// Reads one joint's channels from a frame row. Rotations compose in the listed order
        /// (e.g. "Zrotation Yrotation Xrotation" ⇒ Rz·Ry·Rx), the standard BVH convention.
        /// The quaternion is in BVH (right-handed) space — matrix-wise identical to Unity's AngleAxis.
        /// </summary>
        public static void ReadChannels(IList<string> channels, float[] row, int start,
                                        out Vector3 position, out Quaternion rotation, out bool hasPosition)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            hasPosition = false;
            for (int c = 0; c < channels.Count; c++)
            {
                string ch = channels[c];
                float v = row[start + c];
                char axis = char.ToUpperInvariant(ch[0]);
                if (ch.EndsWith("position", System.StringComparison.OrdinalIgnoreCase))
                {
                    hasPosition = true;
                    if (axis == 'X') position.x = v; else if (axis == 'Y') position.y = v; else position.z = v;
                }
                else
                {
                    rotation *= Quaternion.AngleAxis(v, axis == 'X' ? Vector3.right : axis == 'Y' ? Vector3.up : Vector3.forward);
                }
            }
        }

        /// <summary>Ideal T-pose direction (Unity space, character facing +Z) of a bone toward its child. Null ⇒ leave as is.</summary>
        public static Vector3? TPoseDirection(HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Spine:
                case HumanBodyBones.Chest:
                case HumanBodyBones.UpperChest:
                case HumanBodyBones.Neck:
                    return Vector3.up;
                case HumanBodyBones.LeftShoulder:
                case HumanBodyBones.LeftUpperArm:
                case HumanBodyBones.LeftLowerArm:
                    return Vector3.left;
                case HumanBodyBones.RightShoulder:
                case HumanBodyBones.RightUpperArm:
                case HumanBodyBones.RightLowerArm:
                    return Vector3.right;
                case HumanBodyBones.LeftUpperLeg:
                case HumanBodyBones.LeftLowerLeg:
                case HumanBodyBones.RightUpperLeg:
                case HumanBodyBones.RightLowerLeg:
                    return Vector3.down;
                case HumanBodyBones.LeftFoot:
                case HumanBodyBones.RightFoot:
                    return new Vector3(0f, -0.35f, 1f).normalized;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Rotation that makes a bone's measured rest direction point along its ideal T-pose direction,
        /// applied in world space: <c>corrected = NormalizeToTPose(dir, ideal) * restWorld</c>.
        /// Doing this on both skeletons makes their rest poses comparable even when one is an A-pose.
        /// </summary>
        public static Quaternion NormalizeToTPose(Vector3 restDirection, Vector3 idealDirection)
        {
            if (restDirection.sqrMagnitude < 1e-10f) return Quaternion.identity;
            return Quaternion.FromToRotation(restDirection.normalized, idealDirection.normalized);
        }

        /// <summary>
        /// Core retarget step: the source joint's rotation relative to its (T-pose-normalized) rest,
        /// transplanted onto the target bone's (T-pose-normalized) rest. All in the same world frame.
        /// </summary>
        public static Quaternion Retarget(Quaternion sourceWorld, Quaternion sourceRestWorld, Quaternion targetRestWorld)
            => sourceWorld * Quaternion.Inverse(sourceRestWorld) * targetRestWorld;

        /// <summary>
        /// Default CMU (cgspeed MotionBuilder-friendly names) → Unity humanoid mapping.
        /// When the avatar has no UpperChest, Chest is driven by Spine1 so the chest still ends at the neck.
        /// </summary>
        public static List<RetargetLink> CmuLinks(bool avatarHasUpperChest)
        {
            var l = new List<RetargetLink>
            {
                new RetargetLink(HumanBodyBones.Hips, "Hips", null, HumanBodyBones.LastBone),
                new RetargetLink(HumanBodyBones.LeftUpperLeg, "LeftUpLeg", "LeftLeg", HumanBodyBones.LeftLowerLeg),
                new RetargetLink(HumanBodyBones.LeftLowerLeg, "LeftLeg", "LeftFoot", HumanBodyBones.LeftFoot),
                new RetargetLink(HumanBodyBones.LeftFoot, "LeftFoot", "LeftToeBase", HumanBodyBones.LeftToes),
                new RetargetLink(HumanBodyBones.LeftToes, "LeftToeBase", null, HumanBodyBones.LastBone),
                new RetargetLink(HumanBodyBones.RightUpperLeg, "RightUpLeg", "RightLeg", HumanBodyBones.RightLowerLeg),
                new RetargetLink(HumanBodyBones.RightLowerLeg, "RightLeg", "RightFoot", HumanBodyBones.RightFoot),
                new RetargetLink(HumanBodyBones.RightFoot, "RightFoot", "RightToeBase", HumanBodyBones.RightToes),
                new RetargetLink(HumanBodyBones.RightToes, "RightToeBase", null, HumanBodyBones.LastBone),
            };
            if (avatarHasUpperChest)
            {
                l.Add(new RetargetLink(HumanBodyBones.Spine, "LowerBack", "Spine", HumanBodyBones.Chest));
                l.Add(new RetargetLink(HumanBodyBones.Chest, "Spine", "Spine1", HumanBodyBones.UpperChest));
                l.Add(new RetargetLink(HumanBodyBones.UpperChest, "Spine1", "Neck", HumanBodyBones.Neck));
            }
            else
            {
                l.Add(new RetargetLink(HumanBodyBones.Spine, "LowerBack", "Spine", HumanBodyBones.Chest));
                l.Add(new RetargetLink(HumanBodyBones.Chest, "Spine1", "Neck", HumanBodyBones.Neck));
            }
            l.Add(new RetargetLink(HumanBodyBones.Neck, "Neck", "Head", HumanBodyBones.Head));
            l.Add(new RetargetLink(HumanBodyBones.Head, "Head", null, HumanBodyBones.LastBone));
            l.Add(new RetargetLink(HumanBodyBones.LeftShoulder, "LeftShoulder", "LeftArm", HumanBodyBones.LeftUpperArm));
            l.Add(new RetargetLink(HumanBodyBones.LeftUpperArm, "LeftArm", "LeftForeArm", HumanBodyBones.LeftLowerArm));
            l.Add(new RetargetLink(HumanBodyBones.LeftLowerArm, "LeftForeArm", "LeftHand", HumanBodyBones.LeftHand));
            l.Add(new RetargetLink(HumanBodyBones.LeftHand, "LeftHand", null, HumanBodyBones.LastBone));
            l.Add(new RetargetLink(HumanBodyBones.RightShoulder, "RightShoulder", "RightArm", HumanBodyBones.RightUpperArm));
            l.Add(new RetargetLink(HumanBodyBones.RightUpperArm, "RightArm", "RightForeArm", HumanBodyBones.RightLowerArm));
            l.Add(new RetargetLink(HumanBodyBones.RightLowerArm, "RightForeArm", "RightHand", HumanBodyBones.RightHand));
            l.Add(new RetargetLink(HumanBodyBones.RightHand, "RightHand", null, HumanBodyBones.LastBone));
            return l;
        }

        /// <summary>
        /// Converts <see cref="HumanTrait.MuscleName"/> entries to the property names a humanoid AnimationClip uses
        /// (finger muscles are "LeftHand.Index.1 Stretched", body muscles keep their display name).
        /// </summary>
        public static string MuscleToClipProperty(string muscleName)
        {
            string[] fingers = { "Thumb", "Index", "Middle", "Ring", "Little" };
            foreach (var side in new[] { "Left", "Right" })
            {
                foreach (var f in fingers)
                {
                    string prefix = side + " " + f + " ";
                    if (muscleName.StartsWith(prefix))
                    {
                        // "Left Index 1 Stretched" -> "LeftHand.Index.1 Stretched", "Left Index Spread" -> "LeftHand.Index.Spread"
                        string rest = muscleName.Substring(prefix.Length);
                        int sp = rest.IndexOf(' ');
                        if (sp > 0 && char.IsDigit(rest[0]))
                            return side + "Hand." + f + "." + rest.Substring(0, sp) + " " + rest.Substring(sp + 1);
                        return side + "Hand." + f + "." + rest;
                    }
                }
            }
            return muscleName;
        }

        /// <summary>
        /// Spreads the end→start mismatch of a cyclic curve linearly over the clip so the last key equals the first
        /// (classic loop-closure fix). values[0] is untouched; values[n−1] becomes values[0].
        /// </summary>
        public static void CloseLoop(float[] values)
        {
            int n = values.Length;
            if (n < 2) return;
            float err = values[0] - values[n - 1];
            for (int i = 1; i < n; i++) values[i] += err * i / (n - 1);
        }

        /// <summary>Same as <see cref="CloseLoop(float[])"/> for a rotation sequence (slerped correction).</summary>
        public static void CloseLoop(Quaternion[] values)
        {
            int n = values.Length;
            if (n < 2) return;
            Quaternion err = values[0] * Quaternion.Inverse(values[n - 1]);
            for (int i = 1; i < n; i++)
                values[i] = Quaternion.Slerp(Quaternion.identity, err, (float)i / (n - 1)) * values[i];
        }

        /// <summary>Flips quaternion signs so consecutive keys stay in the same hemisphere (avoids curve spins).</summary>
        public static void EnsureContinuity(Quaternion[] q)
        {
            for (int i = 1; i < q.Length; i++)
                if (Quaternion.Dot(q[i - 1], q[i]) < 0f) q[i] = new Quaternion(-q[i].x, -q[i].y, -q[i].z, -q[i].w);
        }
    }
}
