using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.PhotoMode
{
    public enum PhotoPoseKind
    {
        Neutral,
        Wave,
        Victory,
        Sit,
        /// <summary>Each cousin's ability pose (bow draw, blessing, guard, gadget, shadow veil).</summary>
        Signature,
    }

    /// <summary>
    /// One photo pose, rig-agnostic: limb segments are <em>aimed</em> at directions in hero space
    /// (x = hero's right, y = up, z = facing; zero = keep the frozen gameplay pose), so the same data
    /// works on Humanoid avatars and on Meshy/Mixamo bone names whatever their local axes. Spine/head
    /// are extra rotations around hero axes (pitch + = forward/down). Rigid models (no rig) get the
    /// whole-body fallback (lift as a fraction of hero height, pitch/roll, sway).
    /// </summary>
    public sealed class PhotoPoseDefinition
    {
        public PhotoPoseKind Kind;
        public string LabelKey;

        public Vector3 LeftUpperArm;
        public Vector3 LeftForeArm;
        public Vector3 RightUpperArm;
        public Vector3 RightForeArm;
        public Vector3 LeftThigh;
        public Vector3 LeftShin;
        public Vector3 RightThigh;
        public Vector3 RightShin;

        public float SpinePitch;
        public float SpineYaw;
        public float HeadPitch;
        public float HeadYaw;

        /// <summary>Hips lowered to this fraction of hero height above the feet (0 = keep).</summary>
        public float HipHeight;

        /// <summary>Right forearm swing (degrees, around the facing axis) and its frequency (Hz) — the wave.</summary>
        public float WaveDegrees;
        public float WaveFrequency;

        public float RigidLift;
        public float RigidPitch;
        public float RigidRoll;
        public float RigidSwayDegrees;

        public bool IsNeutral => Kind == PhotoPoseKind.Neutral;
    }

    /// <summary>
    /// The five photo poses of each cousin: Natural, Acenar, Vitória, Sentar and a signature ability pose
    /// (Maithe "Arco tenso", Maria "Bênção da Luz", Joaquim "Guarda das Raízes", Ícaro "Engenhoca",
    /// Lavine "Véu de Sombras"). Applied by <see cref="PhotoPoseRig"/>. Pure data, EditMode-tested.
    /// </summary>
    public static class PhotoPoseLibrary
    {
        public const int PoseCount = 5;

        private static readonly Dictionary<CharacterId, PhotoPoseDefinition[]> ByHero = new Dictionary<CharacterId, PhotoPoseDefinition[]>();
        private static PhotoPoseDefinition[] _generic;

        public static string SignatureLabelKey(CharacterId id) => "ui.photo.pose.signature." + id.ToString().ToLowerInvariant();

        /// <summary>Poses of <paramref name="id"/> (null = no hero known: the four shared poses + Natural again as fifth).</summary>
        public static IReadOnlyList<PhotoPoseDefinition> For(CharacterId? id)
        {
            if (!id.HasValue)
            {
                return _generic ??= new[] { Neutral(), Wave(), Victory(), Sit(), Neutral() };
            }

            if (!ByHero.TryGetValue(id.Value, out PhotoPoseDefinition[] poses))
            {
                poses = new[] { Neutral(), Wave(), Victory(), Sit(), Signature(id.Value) };
                ByHero[id.Value] = poses;
            }

            return poses;
        }

        /// <summary>Pose at <paramref name="index"/>, wrapping in both directions.</summary>
        public static PhotoPoseDefinition Get(CharacterId? id, int index)
        {
            IReadOnlyList<PhotoPoseDefinition> poses = For(id);
            return poses[WrapIndex(index, poses.Count)];
        }

        public static int WrapIndex(int index, int count) => count <= 0 ? 0 : ((index % count) + count) % count;

        private static PhotoPoseDefinition Neutral() => new PhotoPoseDefinition { Kind = PhotoPoseKind.Neutral, LabelKey = "ui.photo.pose.neutral" };

        private static PhotoPoseDefinition Wave() => new PhotoPoseDefinition
        {
            Kind = PhotoPoseKind.Wave,
            LabelKey = "ui.photo.pose.wave",
            RightUpperArm = new Vector3(0.75f, 0.65f, 0.15f),
            RightForeArm = new Vector3(0.1f, 1f, 0.1f),
            HeadYaw = 8f,
            HeadPitch = -4f,
            WaveDegrees = 22f,
            WaveFrequency = 1.6f,
            RigidRoll = 0f,
            RigidSwayDegrees = 5f,
        };

        private static PhotoPoseDefinition Victory() => new PhotoPoseDefinition
        {
            Kind = PhotoPoseKind.Victory,
            LabelKey = "ui.photo.pose.victory",
            LeftUpperArm = new Vector3(-0.4f, 0.92f, 0.05f),
            LeftForeArm = new Vector3(-0.15f, 1f, 0f),
            RightUpperArm = new Vector3(0.4f, 0.92f, 0.05f),
            RightForeArm = new Vector3(0.15f, 1f, 0f),
            SpinePitch = -8f,
            HeadPitch = -12f,
            RigidLift = 0.05f,
            RigidPitch = -6f,
        };

        private static PhotoPoseDefinition Sit() => new PhotoPoseDefinition
        {
            // Cross-legged on the ground, hands on the knees.
            Kind = PhotoPoseKind.Sit,
            LabelKey = "ui.photo.pose.sit",
            LeftThigh = new Vector3(-0.6f, -0.08f, 0.8f),
            LeftShin = new Vector3(0.7f, -0.12f, 0.7f),
            RightThigh = new Vector3(0.6f, -0.08f, 0.8f),
            RightShin = new Vector3(-0.7f, -0.12f, 0.7f),
            LeftUpperArm = new Vector3(-0.3f, -0.85f, 0.4f),
            LeftForeArm = new Vector3(-0.15f, -0.45f, 0.9f),
            RightUpperArm = new Vector3(0.3f, -0.85f, 0.4f),
            RightForeArm = new Vector3(0.15f, -0.45f, 0.9f),
            SpinePitch = 8f,
            HipHeight = 0.12f,
            RigidPitch = -5f,
        };

        private static PhotoPoseDefinition Signature(CharacterId id)
        {
            var pose = new PhotoPoseDefinition { Kind = PhotoPoseKind.Signature, LabelKey = SignatureLabelKey(id) };
            switch (id)
            {
                case CharacterId.Maithe:
                    // Arqueira dos Ventos: bow arm straight ahead, draw hand pulled back to the chin.
                    pose.LeftUpperArm = new Vector3(-0.05f, 0.1f, 1f);
                    pose.LeftForeArm = new Vector3(-0.05f, 0.1f, 1f);
                    pose.RightUpperArm = new Vector3(0.9f, 0.2f, -0.1f);
                    pose.RightForeArm = new Vector3(-0.85f, 0.15f, 0.45f);
                    pose.SpineYaw = -15f;
                    pose.HeadYaw = 10f;
                    pose.RigidRoll = -4f;
                    break;

                case CharacterId.Maria:
                    // Curandeira da Luz: open hands raised forward, eyes to the sky.
                    pose.LeftUpperArm = new Vector3(-0.35f, 0.5f, 0.8f);
                    pose.LeftForeArm = new Vector3(-0.2f, 0.8f, 0.55f);
                    pose.RightUpperArm = new Vector3(0.35f, 0.5f, 0.8f);
                    pose.RightForeArm = new Vector3(0.2f, 0.8f, 0.55f);
                    pose.SpinePitch = -5f;
                    pose.HeadPitch = -16f;
                    pose.RigidPitch = -5f;
                    break;

                case CharacterId.Joaquim:
                    // Guardião das Raízes: wide, low stance, forearms crossed as a shield.
                    pose.LeftUpperArm = new Vector3(-0.4f, -0.3f, 0.85f);
                    pose.LeftForeArm = new Vector3(0.65f, 0.5f, 0.55f);
                    pose.RightUpperArm = new Vector3(0.4f, -0.3f, 0.85f);
                    pose.RightForeArm = new Vector3(-0.65f, 0.5f, 0.55f);
                    pose.LeftThigh = new Vector3(-0.45f, -0.85f, 0.15f);
                    pose.LeftShin = new Vector3(-0.2f, -1f, 0f);
                    pose.RightThigh = new Vector3(0.45f, -0.85f, 0.15f);
                    pose.RightShin = new Vector3(0.2f, -1f, 0f);
                    pose.SpinePitch = 12f;
                    pose.HipHeight = 0.42f;
                    pose.RigidPitch = 6f;
                    break;

                case CharacterId.Icaro:
                    // Mestre das Máquinas: tinkering with a gadget held up in front, other hand on the hip.
                    pose.RightUpperArm = new Vector3(0.2f, -0.25f, 1f);
                    pose.RightForeArm = new Vector3(-0.1f, 0.35f, 1f);
                    pose.LeftUpperArm = new Vector3(-0.55f, -0.8f, -0.1f);
                    pose.LeftForeArm = new Vector3(0.6f, -0.1f, 0.2f);
                    pose.HeadPitch = 20f;
                    pose.HeadYaw = 6f;
                    pose.RigidPitch = 4f;
                    pose.RigidRoll = 3f;
                    break;

                case CharacterId.Lavine:
                    // Feiticeira das Sombras: arms spread low, head bowed, leaning into the spell.
                    pose.LeftUpperArm = new Vector3(-0.85f, -0.45f, 0.2f);
                    pose.LeftForeArm = new Vector3(-0.9f, -0.1f, 0.35f);
                    pose.RightUpperArm = new Vector3(0.85f, -0.45f, 0.2f);
                    pose.RightForeArm = new Vector3(0.9f, -0.1f, 0.35f);
                    pose.SpinePitch = 8f;
                    pose.HeadPitch = 15f;
                    pose.RigidPitch = 7f;
                    break;
            }

            return pose;
        }
    }
}
