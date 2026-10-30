using Rapadura.Gameplay.PhotoMode;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Seats a hero model on a mount: the photo mode's rig-agnostic <see cref="PhotoPoseRig"/> (Humanoid avatars, Meshy/
    /// Mixamo bone names, rigid fallback for unrigged models) holds the "astride" pose below — thighs forward and apart,
    /// shins hanging, hands forward on the reins — and the whole visual is lifted so the hips sit on the saddle
    /// (<see cref="Apply"/> each LateUpdate, after the hero's animators). <see cref="Release"/> puts the captured pose back.
    /// </summary>
    public sealed class MountRiderPose
    {
        /// <summary>The riding pose (hero space: x right, y up, z forward).</summary>
        public static readonly PhotoPoseDefinition Astride = new PhotoPoseDefinition
        {
            Kind = PhotoPoseKind.Sit,
            LabelKey = "ui.photo.pose.sit",
            LeftThigh = new Vector3(-0.45f, -0.35f, 0.82f),
            LeftShin = new Vector3(-0.15f, -0.95f, -0.15f),
            RightThigh = new Vector3(0.45f, -0.35f, 0.82f),
            RightShin = new Vector3(0.15f, -0.95f, -0.15f),
            LeftUpperArm = new Vector3(-0.2f, -0.75f, 0.6f),
            LeftForeArm = new Vector3(0.1f, -0.15f, 1f),
            RightUpperArm = new Vector3(0.2f, -0.75f, 0.6f),
            RightForeArm = new Vector3(-0.1f, -0.15f, 1f),
            SpinePitch = 6f,
            HeadPitch = -6f,
            RigidPitch = -3f,
        };

        private readonly Transform _visual;
        private readonly Transform _frame;
        private readonly PhotoPoseRig _rig;
        private readonly float _hipsAboveFeet;
        private readonly float _groundOffset;

        /// <param name="groundOffset">How far the ground is below <paramref name="frame"/>'s pivot (the map token floats 0.5 above it).</param>
        public MountRiderPose(Transform visual, Transform frame, float groundOffset = 0f)
        {
            _visual = visual;
            _frame = frame != null ? frame : visual;
            _groundOffset = groundOffset;
            float height = ModelHeight(visual, out float feetY);
            float groundY = _frame != null ? _frame.position.y - groundOffset : feetY;
            _rig = visual != null ? new PhotoPoseRig(visual, _frame, height, groundY) : null;
            _hipsAboveFeet = HipsHeight(visual, groundY, height);
        }

        /// <summary>Height of the hips above the feet at rest (what is lifted to the saddle).</summary>
        public float HipsAboveFeet => _hipsAboveFeet;

        public bool IsRigged => _rig != null && _rig.IsRigged;

        /// <summary>The posed model (null once destroyed).</summary>
        public Transform Visual => _visual;

        /// <summary>Snapshots the current pose and starts blending into <see cref="Astride"/>.</summary>
        public void Begin(bool instant)
        {
            if (_rig == null)
            {
                return;
            }

            _rig.Capture();
            _rig.SetPose(Astride, instant);
        }

        /// <summary>Poses the rider and lifts the visual so the hips are at <paramref name="seatWorldY"/>.</summary>
        public void Apply(float seatWorldY, float time, float deltaTime, bool reducedMotion)
        {
            if (_rig == null || _visual == null)
            {
                return;
            }

            _rig.Apply(time, deltaTime, reducedMotion);
            float feet = _frame != null ? _frame.position.y - _groundOffset : _visual.position.y;
            float lift = Mathf.Max(0f, seatWorldY - feet - _hipsAboveFeet);
            _visual.position += Vector3.up * lift;
        }

        public void Release()
        {
            _rig?.Restore();
        }

        /// <summary>Lift needed for a seat at <paramref name="seatHeight"/> above the ground (pure, tested).</summary>
        public static float LiftFor(float seatHeight, float hipsAboveFeet) => Mathf.Max(0f, seatHeight - hipsAboveFeet);

        private static float ModelHeight(Transform visual, out float feetY)
        {
            feetY = visual != null ? visual.position.y : 0f;
            if (visual == null)
            {
                return 1.6f;
            }

            bool any = false;
            Bounds bounds = default;
            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>())
            {
                if (!any)
                {
                    bounds = renderer.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!any)
            {
                return 1.6f;
            }

            feetY = bounds.min.y;
            return Mathf.Max(0.5f, bounds.size.y);
        }

        private static float HipsHeight(Transform visual, float groundY, float height)
        {
            if (visual != null)
            {
                Animator animator = visual.GetComponentInChildren<Animator>();
                Transform hips = animator != null && animator.isHuman && animator.avatar != null ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
                if (hips == null)
                {
                    hips = FindDeep(visual, "Hips") ?? FindDeep(visual, "mixamorig:Hips");
                }

                if (hips != null)
                {
                    return Mathf.Clamp(hips.position.y - groundY, height * 0.3f, height * 0.65f);
                }
            }

            return height * 0.48f;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (string.Equals(root.name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
