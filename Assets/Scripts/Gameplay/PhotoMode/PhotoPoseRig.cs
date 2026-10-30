using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.PhotoMode
{
    /// <summary>
    /// Puts the frozen hero into a <see cref="PhotoPoseDefinition"/> while the photo mode is open. Not a
    /// component: <c>PhotoModeController</c> calls <see cref="Apply"/> from its LateUpdate, which runs
    /// after <c>PlayerProceduralAnimator</c> (execution order 100), so the pose is the last word.
    ///
    /// Writes are absolute, never additive: <see cref="Capture"/> snapshots the bones (and the visual
    /// root) once, and every frame restores that snapshot before posing, so nothing accumulates whether
    /// or not the paused Animator keeps rewriting the bones; <see cref="Restore"/> puts the snapshot back
    /// on exit. Bones come from the Humanoid avatar when there is one, else Meshy/Mixamo bone names (same
    /// lookup as PlayerProceduralAnimator). Limbs are aimed (FromToRotation of segment → wanted direction),
    /// so bone axis conventions don't matter; the hero frame comes from the shoulders when the rig has
    /// them. No rig (Maithe's static model, the map capsule) = whole-body fallback on the visual root.
    /// </summary>
    public sealed class PhotoPoseRig
    {
        /// <summary>Seconds to blend into a newly picked pose (real time).</summary>
        public const float BlendSeconds = 0.25f;

        /// <summary>Wave amplitude multiplier with "Reduzir Movimento" on.</summary>
        public const float ReducedMotionWaveScale = 0.35f;

        private readonly Transform _visual;
        private readonly Transform _frame;
        private readonly float _heroHeight;
        private readonly float _groundY;

        private Transform _hips, _spine, _head;
        private Transform _leftUpperArm, _leftForeArm, _leftHand;
        private Transform _rightUpperArm, _rightForeArm, _rightHand;
        private Transform _leftThigh, _leftShin, _leftFoot;
        private Transform _rightThigh, _rightShin, _rightFoot;

        private readonly List<Transform> _snapshotBones = new List<Transform>();
        private readonly List<Quaternion> _snapshotRotations = new List<Quaternion>();
        private Vector3 _hipsLocalPosition;
        private Vector3 _visualLocalPosition;
        private Quaternion _visualLocalRotation;
        private bool _captured;

        private PhotoPoseDefinition _pose;
        private float _blend = 1f;

        /// <param name="visual">The hero model (PlayerProceduralAnimator's visual root / the map token model).</param>
        /// <param name="frame">Transform whose yaw is the hero's facing when the rig gives no shoulders (player root / token).</param>
        public PhotoPoseRig(Transform visual, Transform frame, float heroHeight, float groundY)
        {
            _visual = visual;
            _frame = frame != null ? frame : visual;
            _heroHeight = Mathf.Max(0.1f, heroHeight);
            _groundY = groundY;
            FindBones();
        }

        /// <summary>True when the model has an arm rig (aimed limbs); false = whole-body fallback.</summary>
        public bool IsRigged { get; private set; }

        public PhotoPoseDefinition Pose => _pose;

        /// <summary>Remembers the current (frozen gameplay) pose. Call once, right after the hero stopped.</summary>
        public void Capture()
        {
            _snapshotBones.Clear();
            _snapshotRotations.Clear();

            if (_visual == null)
            {
                return;
            }

            foreach (Transform bone in new[]
                     {
                         _hips, _spine, _head, _leftUpperArm, _leftForeArm, _rightUpperArm, _rightForeArm,
                         _leftThigh, _leftShin, _rightThigh, _rightShin,
                     })
            {
                if (bone != null && !_snapshotBones.Contains(bone))
                {
                    _snapshotBones.Add(bone);
                    _snapshotRotations.Add(bone.localRotation);
                }
            }

            _hipsLocalPosition = _hips != null ? _hips.localPosition : Vector3.zero;
            _visualLocalPosition = _visual.localPosition;
            _visualLocalRotation = _visual.localRotation;
            _captured = true;
        }

        /// <summary>Picks the pose to blend into (null = natural).</summary>
        public void SetPose(PhotoPoseDefinition pose, bool instant)
        {
            if (pose == _pose)
            {
                return;
            }

            _pose = pose;
            _blend = instant ? 1f : 0f;
        }

        /// <summary>Restores the snapshot and applies the pose. <paramref name="time"/>/<paramref name="deltaTime"/> are real (unscaled) time.</summary>
        public void Apply(float time, float deltaTime, bool reducedMotion)
        {
            if (!_captured || _visual == null)
            {
                return;
            }

            RestoreSnapshot();

            if (_pose == null || _pose.IsNeutral)
            {
                return;
            }

            _blend = reducedMotion ? 1f : Mathf.MoveTowards(_blend, 1f, deltaTime / BlendSeconds);
            float w = Mathf.SmoothStep(0f, 1f, _blend);
            float waveScale = reducedMotion ? ReducedMotionWaveScale : 1f;
            float wave = _pose.WaveDegrees * waveScale * Mathf.Sin(time * _pose.WaveFrequency * Mathf.PI * 2f);

            if (IsRigged)
            {
                ApplyRig(w, wave);
            }
            else
            {
                ApplyRigid(w, time, waveScale);
            }
        }

        /// <summary>Puts the captured pose back (photo mode closing). Safe to call more than once.</summary>
        public void Restore()
        {
            if (_captured && _visual != null)
            {
                RestoreSnapshot();
            }

            _captured = false;
        }

        private void RestoreSnapshot()
        {
            for (int i = 0; i < _snapshotBones.Count; i++)
            {
                if (_snapshotBones[i] != null)
                {
                    _snapshotBones[i].localRotation = _snapshotRotations[i];
                }
            }

            if (_hips != null)
            {
                _hips.localPosition = _hipsLocalPosition;
            }

            _visual.localPosition = _visualLocalPosition;
            _visual.localRotation = _visualLocalRotation;
        }

        private void ApplyRig(float w, float wave)
        {
            HeroAxes(out Vector3 right, out Vector3 up, out Vector3 forward);
            Quaternion toWorld = Quaternion.LookRotation(forward, up);

            if (_hips != null && _pose.HipHeight > 0f)
            {
                Vector3 hips = _hips.position;
                float targetY = _groundY + _pose.HipHeight * _heroHeight;
                if (targetY < hips.y)
                {
                    _hips.position = new Vector3(hips.x, Mathf.Lerp(hips.y, targetY, w), hips.z);
                }
            }

            Rotate(_spine, right, _pose.SpinePitch * w);
            Rotate(_spine, up, _pose.SpineYaw * w);
            Rotate(_head, right, _pose.HeadPitch * w);
            Rotate(_head, up, _pose.HeadYaw * w);

            Aim(_leftUpperArm, _leftForeArm, _pose.LeftUpperArm, toWorld, w);
            Aim(_leftForeArm, _leftHand, _pose.LeftForeArm, toWorld, w);
            Aim(_rightUpperArm, _rightForeArm, _pose.RightUpperArm, toWorld, w);
            Aim(_rightForeArm, _rightHand, _pose.RightForeArm, toWorld, w);
            if (Mathf.Abs(wave) > 0.001f)
            {
                Rotate(_rightForeArm, forward, -wave * w);
            }

            Aim(_leftThigh, _leftShin, _pose.LeftThigh, toWorld, w);
            Aim(_leftShin, _leftFoot, _pose.LeftShin, toWorld, w);
            Aim(_rightThigh, _rightShin, _pose.RightThigh, toWorld, w);
            Aim(_rightShin, _rightFoot, _pose.RightShin, toWorld, w);
        }

        private void ApplyRigid(float w, float time, float waveScale)
        {
            HeroAxes(out Vector3 right, out Vector3 up, out Vector3 forward);
            float sway = _pose.RigidSwayDegrees * waveScale * Mathf.Sin(time * Mathf.Max(0.5f, _pose.WaveFrequency) * Mathf.PI * 2f);
            Quaternion tilt = Quaternion.AngleAxis(_pose.RigidPitch * w, right) * Quaternion.AngleAxis((_pose.RigidRoll + sway) * w, forward);

            // Tilt around the feet (the visual's pivot may sit anywhere), then lift.
            Vector3 feet = new Vector3(_visual.position.x, _groundY, _visual.position.z);
            Vector3 offset = _visual.position - feet;
            _visual.SetPositionAndRotation(feet + tilt * offset + up * (_pose.RigidLift * _heroHeight * w), tilt * _visual.rotation);
        }

        /// <summary>Hero frame: right from the shoulders when the rig has them (immune to model axis flips), else the frame transform.</summary>
        private void HeroAxes(out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            up = Vector3.up;
            right = Vector3.zero;

            if (_leftUpperArm != null && _rightUpperArm != null)
            {
                right = Vector3.ProjectOnPlane(_rightUpperArm.position - _leftUpperArm.position, up);
            }

            if (right.sqrMagnitude < 0.0001f)
            {
                right = Vector3.ProjectOnPlane(_frame.right, up);
            }

            if (right.sqrMagnitude < 0.0001f)
            {
                right = Vector3.right;
            }

            right.Normalize();
            forward = Vector3.Cross(right, up).normalized;
        }

        private static void Rotate(Transform bone, Vector3 axis, float degrees)
        {
            if (bone != null && Mathf.Abs(degrees) > 0.001f)
            {
                bone.rotation = Quaternion.AngleAxis(degrees, axis) * bone.rotation;
            }
        }

        /// <summary>Turns <paramref name="bone"/> so the segment toward <paramref name="child"/> points along the hero-space direction.</summary>
        private static void Aim(Transform bone, Transform child, Vector3 heroDirection, Quaternion toWorld, float weight)
        {
            if (bone == null || child == null || heroDirection.sqrMagnitude < 0.0001f || weight <= 0f)
            {
                return;
            }

            Vector3 current = child.position - bone.position;
            if (current.sqrMagnitude < 0.000001f)
            {
                return;
            }

            Vector3 wanted = toWorld * heroDirection.normalized;
            Quaternion delta = Quaternion.FromToRotation(current, wanted);
            bone.rotation = Quaternion.Slerp(Quaternion.identity, delta, weight) * bone.rotation;
        }

        private void FindBones()
        {
            if (_visual == null)
            {
                return;
            }

            Animator animator = _visual.GetComponentInChildren<Animator>();
            bool human = animator != null && animator.isHuman && animator.avatar != null;

            _hips = Bone(animator, human, HumanBodyBones.Hips, "Hips", "mixamorig:Hips");
            _spine = Bone(animator, human, HumanBodyBones.Spine, "Spine02", "Spine", "mixamorig:Spine");
            _head = Bone(animator, human, HumanBodyBones.Head, "Head", "mixamorig:Head");
            _leftUpperArm = Bone(animator, human, HumanBodyBones.LeftUpperArm, "LeftArm", "mixamorig:LeftArm");
            _leftForeArm = Bone(animator, human, HumanBodyBones.LeftLowerArm, "LeftForeArm", "mixamorig:LeftForeArm");
            _leftHand = Bone(animator, human, HumanBodyBones.LeftHand, "LeftHand", "mixamorig:LeftHand");
            _rightUpperArm = Bone(animator, human, HumanBodyBones.RightUpperArm, "RightArm", "mixamorig:RightArm");
            _rightForeArm = Bone(animator, human, HumanBodyBones.RightLowerArm, "RightForeArm", "mixamorig:RightForeArm");
            _rightHand = Bone(animator, human, HumanBodyBones.RightHand, "RightHand", "mixamorig:RightHand");
            _leftThigh = Bone(animator, human, HumanBodyBones.LeftUpperLeg, "LeftUpLeg", "mixamorig:LeftUpLeg");
            _leftShin = Bone(animator, human, HumanBodyBones.LeftLowerLeg, "LeftLeg", "mixamorig:LeftLeg");
            _leftFoot = Bone(animator, human, HumanBodyBones.LeftFoot, "LeftFoot", "mixamorig:LeftFoot");
            _rightThigh = Bone(animator, human, HumanBodyBones.RightUpperLeg, "RightUpLeg", "mixamorig:RightUpLeg");
            _rightShin = Bone(animator, human, HumanBodyBones.RightLowerLeg, "RightLeg", "mixamorig:RightLeg");
            _rightFoot = Bone(animator, human, HumanBodyBones.RightFoot, "RightFoot", "mixamorig:RightFoot");

            IsRigged = _leftUpperArm != null && _leftForeArm != null && _rightUpperArm != null && _rightForeArm != null;
        }

        private Transform Bone(Animator animator, bool human, HumanBodyBones humanBone, params string[] names)
        {
            if (human)
            {
                Transform mapped = animator.GetBoneTransform(humanBone);
                if (mapped != null)
                {
                    return mapped;
                }
            }

            foreach (string boneName in names)
            {
                Transform found = FindDeep(_visual, boneName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string boneName)
        {
            if (string.Equals(root.name, boneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), boneName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
