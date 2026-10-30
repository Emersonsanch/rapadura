using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rapadura.Editor.Mocap
{
    /// <summary>How each bone's rest is matched between the BVH skeleton and the avatar.</summary>
    public enum RetargetPolicy
    {
        /// <summary>BVH T-pose ↔ avatar bind pose for every bone except the arms (A/T-pose fix). The default.</summary>
        RestToRest,
        /// <summary>Old behaviour: every bone with a <see cref="MocapMath.TPoseDirection"/> forced to the same world direction. Kept to reproduce the deformation in tests.</summary>
        NormalizeAll,
    }

    /// <summary>
    /// A hidden instance of a humanoid model, put in its Avatar's bind pose and posed frame by frame from a BVH
    /// by rotation retargeting (see <see cref="MocapMath"/>). Used by <see cref="MocapClipBuilder.Bake"/>, and by
    /// the retarget tests to compare posed bones with the BVH and with the avatar's rest.
    /// Dispose to destroy the instance.
    /// </summary>
    public sealed class MocapRetargetRig : IDisposable
    {
        public readonly BvhClip Bvh;
        public readonly GameObject Instance;
        public readonly Animator Animator;
        public readonly List<RetargetLink> Links;
        public readonly float UnitScale;

        private readonly Dictionary<HumanBodyBones, Quaternion> _sourceRest = new Dictionary<HumanBodyBones, Quaternion>();
        private readonly Dictionary<HumanBodyBones, Quaternion> _targetRest = new Dictionary<HumanBodyBones, Quaternion>();
        private readonly Dictionary<Transform, (Vector3 pos, Quaternion rot, Vector3 scale)> _bind = new Dictionary<Transform, (Vector3, Quaternion, Vector3)>();
        private readonly Vector3[] _pos;
        private readonly Quaternion[] _rot;
        private readonly Transform _hips;
        private readonly int _hipsJoint;
        private readonly float _targetFootY;
        private readonly float _sourceFootY;

        /// <summary>Target hips height ÷ source hips height (both measured in their rest poses).</summary>
        public readonly float HeightRatio;

        public MocapRetargetRig(BvhClip bvh, GameObject model, float unitScale, bool tposeFrame0, RetargetPolicy policy = RetargetPolicy.RestToRest)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Bvh = bvh;
            UnitScale = unitScale;
            Instance = UnityEngine.Object.Instantiate(model);
            Instance.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                Instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                Instance.transform.localScale = Vector3.one;
                Animator = Instance.GetComponentInChildren<Animator>();
                if (Animator == null || Animator.avatar == null || !Animator.avatar.isHuman || !Animator.avatar.isValid)
                    throw new InvalidOperationException($"'{model.name}' has no valid humanoid Avatar (Rig → Animation Type: Humanoid).");

                ApplyAvatarTPose(Instance.transform, Animator.avatar);
                foreach (var t in Instance.GetComponentsInChildren<Transform>(true))
                    _bind[t] = (t.localPosition, t.localRotation, t.localScale);

                bool hasUpperChest = Animator.GetBoneTransform(HumanBodyBones.UpperChest) != null;
                Links = MocapMath.CmuLinks(hasUpperChest)
                    .Where(l => Animator.GetBoneTransform(l.Bone) != null && bvh.IndexOf(l.BvhJoint) >= 0)
                    .ToList();
                if (!Links.Any(l => l.Bone == HumanBodyBones.Hips))
                    throw new InvalidOperationException("BVH has no 'Hips' joint — only the CMU/MotionBuilder naming is mapped (tools/mocap/bvh_prep.py --joint-map renames other skeletons).");

                // Parents first so setting a world rotation never disturbs an already-posed child.
                Links.Sort((a, b) => Depth(Animator.GetBoneTransform(a.Bone)).CompareTo(Depth(Animator.GetBoneTransform(b.Bone))));

                // ---- target (avatar) rest
                foreach (var l in Links)
                {
                    var t = Animator.GetBoneTransform(l.Bone);
                    var rest = t.rotation;
                    var ideal = Normalized(l.Bone, policy);
                    var child = l.UnityChild == HumanBodyBones.LastBone ? null : Animator.GetBoneTransform(l.UnityChild);
                    if (ideal.HasValue && child != null)
                        rest = MocapMath.NormalizeToTPose(child.position - t.position, ideal.Value) * rest;
                    _targetRest[l.Bone] = rest;
                }
                _hips = Animator.GetBoneTransform(HumanBodyBones.Hips);
                _targetFootY = LowestY(Animator, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftToes, HumanBodyBones.RightToes);
                float targetHipsHeight = _hips.position.y - _targetFootY;

                // ---- source (BVH) rest
                int j0 = bvh.Joints.Count;
                _pos = new Vector3[j0];
                _rot = new Quaternion[j0];
                if (tposeFrame0) bvh.Evaluate(0, unitScale, _pos, _rot);
                else EvaluateZeroPose(bvh, unitScale, _pos, _rot);
                foreach (var l in Links)
                {
                    int j = bvh.IndexOf(l.BvhJoint);
                    var rest = _rot[j];
                    var ideal = Normalized(l.Bone, policy);
                    int c = l.BvhChild == null ? -1 : bvh.IndexOf(l.BvhChild);
                    if (ideal.HasValue && c >= 0)
                        rest = MocapMath.NormalizeToTPose(_pos[c] - _pos[j], ideal.Value) * rest;
                    _sourceRest[l.Bone] = rest;
                }
                _hipsJoint = bvh.IndexOf("Hips");
                _sourceFootY = LowestY(bvh, _pos, "LeftFoot", "RightFoot", "LeftToeBase", "RightToeBase");
                float sourceHipsHeight = _pos[_hipsJoint].y - _sourceFootY;
                HeightRatio = sourceHipsHeight > 1e-4f ? targetHipsHeight / sourceHipsHeight : 1f;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(Instance);
                throw;
            }
        }

        public Transform Root => Animator.transform;

        /// <summary>Poses the instance from BVH frame <paramref name="frame"/> (frame 0 = the T-pose when there is one).</summary>
        public void PoseFrame(int frame)
        {
            Bvh.Evaluate(frame, UnitScale, _pos, _rot);
            var hp = _pos[_hipsJoint];
            _hips.position = new Vector3(hp.x * HeightRatio, _targetFootY + (hp.y - _sourceFootY) * HeightRatio, hp.z * HeightRatio);
            foreach (var l in Links)
            {
                var t = Animator.GetBoneTransform(l.Bone);
                t.rotation = MocapMath.Retarget(_rot[Bvh.IndexOf(l.BvhJoint)], _sourceRest[l.Bone], _targetRest[l.Bone]);
            }
        }

        /// <summary>Puts every transform back in the avatar's bind pose.</summary>
        public void ResetToBindPose()
        {
            foreach (var kv in _bind)
            {
                kv.Key.localPosition = kv.Value.pos;
                kv.Key.localRotation = kv.Value.rot;
                kv.Key.localScale = kv.Value.scale;
            }
        }

        /// <summary>Unit direction joint → joint in the last evaluated BVH frame (Unity space); zero if they coincide.</summary>
        public Vector3 SourceDirection(string from, string to)
        {
            Vector3 d = _pos[Bvh.IndexOf(to)] - _pos[Bvh.IndexOf(from)];
            return d.sqrMagnitude > 1e-12f ? d.normalized : Vector3.zero;
        }

        public void Dispose()
        {
            if (Instance != null) UnityEngine.Object.DestroyImmediate(Instance);
        }

        // ------------------------------------------------------------------ helpers

        private static Vector3? Normalized(HumanBodyBones bone, RetargetPolicy policy)
        {
            if (policy == RetargetPolicy.RestToRest && !MocapMath.NormalizesRestDirection(bone)) return null;
            return MocapMath.TPoseDirection(bone);
        }

        /// <summary>Puts the instance in the T-pose stored in the Avatar's HumanDescription (what the avatar was configured with).</summary>
        internal static void ApplyAvatarTPose(Transform instanceRoot, Avatar avatar)
        {
            var byName = new Dictionary<string, Transform>();
            foreach (var t in instanceRoot.GetComponentsInChildren<Transform>(true))
                if (!byName.ContainsKey(t.name)) byName[t.name] = t;
            var skeleton = avatar.humanDescription.skeleton;
            if (skeleton == null) return;
            foreach (var sb in skeleton)
            {
                if (!byName.TryGetValue(sb.name, out var t) || t == instanceRoot) continue;
                t.localPosition = sb.position;
                t.localRotation = sb.rotation;
                t.localScale = sb.scale;
            }
        }

        private static void EvaluateZeroPose(BvhClip bvh, float scale, Vector3[] pos, Quaternion[] rot)
        {
            var saved = bvh.Frames[0];
            var zero = new float[saved.Length];
            var rootJ = bvh.Joints[0];
            for (int c = 0; c < rootJ.Channels.Count; c++)
                if (rootJ.Channels[c].EndsWith("position", StringComparison.OrdinalIgnoreCase))
                    zero[rootJ.ChannelStart + c] = saved[rootJ.ChannelStart + c];
            bvh.Frames[0] = zero;
            try { bvh.Evaluate(0, scale, pos, rot); }
            finally { bvh.Frames[0] = saved; }
        }

        private static float LowestY(Animator a, params HumanBodyBones[] bones)
        {
            float y = float.PositiveInfinity;
            foreach (var b in bones) { var t = a.GetBoneTransform(b); if (t != null) y = Mathf.Min(y, t.position.y); }
            return float.IsPositiveInfinity(y) ? 0f : y;
        }

        private static float LowestY(BvhClip bvh, Vector3[] pos, params string[] joints)
        {
            float y = float.PositiveInfinity;
            foreach (var n in joints) { int j = bvh.IndexOf(n); if (j >= 0) y = Mathf.Min(y, pos[j].y); }
            return float.IsPositiveInfinity(y) ? 0f : y;
        }

        private static int Depth(Transform t)
        {
            int d = 0;
            while (t != null) { d++; t = t.parent; }
            return d;
        }
    }
}
