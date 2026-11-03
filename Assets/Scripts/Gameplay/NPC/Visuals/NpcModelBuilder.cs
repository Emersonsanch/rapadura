using UnityEngine;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>The five bones of an NPC figure (rigid skinning: every vertex belongs to exactly one).</summary>
    public static class NpcBones
    {
        public const int Root = 0;
        public const int Body = 1;
        public const int Head = 2;
        public const int ArmL = 3;
        public const int ArmR = 4;
        public const int Count = 5;

        public static readonly string[] Names = { "Root", "Body", "Head", "ArmL", "ArmR" };
    }

    /// <summary>A built NPC: the skinned mesh (shared by every figure of this recipe) and where its bones sit.</summary>
    public sealed class NpcModel
    {
        public string Id;
        public Mesh Mesh;

        /// <summary>Bone pivots in model space (feet at y = 0, facing +z), indexed by <see cref="NpcBones"/>.</summary>
        public Vector3[] Pivots;

        public float Height;
        public float HeadCenterY;
        public int Triangles;

        /// <summary>Hand sockets in model space (centre of each fist, where the held props pass), left and right.</summary>
        public Vector3 HandL, HandR;

        /// <summary>The arm holds a staff/pole standing on the ground: the idle keeps it still instead of swinging it.</summary>
        public bool GroundedL, GroundedR;
    }

    /// <summary>
    /// Builds an NPC figure from a <see cref="NpcRecipe"/>: chibi proportions like the cousins (a big head, about four and a
    /// half heads tall), a parametric body (age, build, posture, height), a head with a drawn face, hair, beard and
    /// headwear, the clothing silhouette and layers, two arms with volume posed by two-bone IK toward a hand socket per
    /// pose (<see cref="NpcArmPose"/>) and the role props held in those sockets. Feet at y = 0,
    /// facing +z. ~600–1500 triangles, one skinned mesh with five bones (<see cref="NpcBones"/>), all on the shared
    /// <see cref="NpcPalette"/> material. Pure: the same recipe always gives the same mesh.
    /// </summary>
    public static partial class NpcModelBuilder
    {
        /// <summary>Everything the parts need to agree on (built once per recipe).</summary>
        internal struct Rig
        {
            public NpcRecipe R;
            public float H, S, Limb;
            public float HeadH, HeadW, HeadD;
            public float NeckY, HipY, WaistY, ShoulderY, ShoulderX;
            public float Rw, Belly, ShoulderMul;
            public float SkirtY0, SkirtR0;
            public Matrix4x4 BodyM, HeadM;
            public Vector3 HeadCenter; // in head rest space (apply HeadM)
            public float UpperLen, ForeLen, HandLen;
            public float KneeY, KneeR, WaistR;

            /// <summary>Posed model space. Hand = the hand socket (centre of the fist, where a held prop passes).</summary>
            public Vector3 ShoulderL, ShoulderR, HandL, HandR, ForeL, ForeR, ElbowL, ElbowR, WristL, WristR;

            /// <summary>Axis through each fist (a staff, a handle), unit, perpendicular to the forearm.</summary>
            public Vector3 GripL, GripR;

            /// <summary>The arm holds something standing on the ground (staff, pole, stick): the idle must not swing it.</summary>
            public bool GroundedL, GroundedR;

            /// <summary>Front surface z of the torso at height y (body rest space).</summary>
            public float TorsoFrontZ(float y)
            {
                float t = Mathf.InverseLerp(HipY, NeckY, y);
                return Mathf.Lerp(Rw * 0.92f * Belly, Rw * ShoulderMul, t) * 0.78f;
            }

            /// <summary>
            /// Radius of the skirt / lower garment at height y (root space): long garments taper from the waist to the knee and
            /// then flare out to the hem, short ones are a plain frustum.
            /// </summary>
            public float SkirtRadius(float y)
            {
                if (y >= KneeY)
                {
                    return Mathf.Lerp(KneeR, WaistR, Mathf.InverseLerp(KneeY, WaistY, y));
                }

                float t = Mathf.InverseLerp(SkirtY0, KneeY, y);
                return Mathf.Lerp(SkirtR0, KneeR, 1f - (1f - t) * (1f - t));
            }

            /// <summary>Front surface z of the skirt at height y (root space).</summary>
            public float SkirtFrontZ(float y) => SkirtRadius(y) * 0.84f;

            /// <summary>Front z of the figure at height y in body rest space (belly, skirt or chest, whichever sticks out).</summary>
            public float FrontZ(float y)
            {
                float z = y >= HipY ? TorsoFrontZ(y) : 0f;
                if (R.Build == NpcBuild.Stout && y > HipY && y < NeckY - 0.1f * S)
                {
                    z = Mathf.Max(z, Rw * 0.88f);
                }

                if (y < WaistY && y >= SkirtY0)
                {
                    z = Mathf.Max(z, SkirtFrontZ(y));
                }

                return z;
            }

            /// <summary>Half width of the figure at height y in body rest space (what a hanging hand must clear).</summary>
            public float SideX(float y)
            {
                float x = Rw * 0.45f + 0.08f * Limb;
                if (y >= SkirtY0 && y < WaistY)
                {
                    x = Mathf.Max(x, SkirtRadius(y));
                }

                if (y >= HipY)
                {
                    float t = Mathf.InverseLerp(HipY, NeckY, y);
                    x = Mathf.Max(x, Mathf.Lerp(Rw * 0.92f * Belly, Rw * ShoulderMul, t));
                    if (R.Build == NpcBuild.Stout && y < NeckY - 0.1f * S)
                    {
                        x = Mathf.Max(x, Rw * 0.98f * Belly + 0.02f);
                    }
                }

                if (R.Has(NpcLayer.Satchel) && y > HipY - 0.08f * S && y < HipY + 0.14f * S)
                {
                    x = Mathf.Max(x, Rw * 1.02f + 0.05f * S);
                }

                return x;
            }
        }

        public static NpcModel Build(NpcRecipe recipe)
        {
            var b = new NpcMeshBuilder();
            Rig rig = MakeRig(recipe);

            BuildLegsAndSkirt(b, ref rig);
            BuildTorso(b, ref rig);
            BuildLayers(b, ref rig);
            BuildArm(b, ref rig, false);
            BuildArm(b, ref rig, true);
            BuildHead(b, ref rig);
            BuildProps(b, ref rig);

            var pivots = new Vector3[NpcBones.Count];
            pivots[NpcBones.Root] = Vector3.zero;
            pivots[NpcBones.Body] = new Vector3(0f, rig.HipY, 0f);
            pivots[NpcBones.Head] = rig.BodyM.MultiplyPoint3x4(new Vector3(0f, rig.NeckY, 0f));
            pivots[NpcBones.ArmL] = rig.ShoulderL;
            pivots[NpcBones.ArmR] = rig.ShoulderR;

            var bind = new Matrix4x4[NpcBones.Count];
            for (int i = 0; i < bind.Length; i++)
            {
                bind[i] = Matrix4x4.Translate(-pivots[i]);
            }

            Mesh mesh = b.ToMesh("Npc_" + recipe.Id, bind);
            return new NpcModel
            {
                Id = recipe.Id,
                Mesh = mesh,
                Pivots = pivots,
                Height = rig.H,
                HeadCenterY = rig.HeadM.MultiplyPoint3x4(rig.HeadCenter).y,
                Triangles = b.TriangleCount,
                HandL = rig.HandL,
                HandR = rig.HandR,
                GroundedL = rig.GroundedL,
                GroundedR = rig.GroundedR,
            };
        }

        /// <summary>Only the triangle count (tests, budgets) — builds into a throwaway builder, no Mesh object.</summary>
        public static int CountTriangles(NpcRecipe recipe)
        {
            var b = new NpcMeshBuilder();
            Rig rig = MakeRig(recipe);
            BuildLegsAndSkirt(b, ref rig);
            BuildTorso(b, ref rig);
            BuildLayers(b, ref rig);
            BuildArm(b, ref rig, false);
            BuildArm(b, ref rig, true);
            BuildHead(b, ref rig);
            BuildProps(b, ref rig);
            return b.TriangleCount;
        }

        // ------------------------------------------------------------------
        // Proportions
        // ------------------------------------------------------------------

        internal static Rig MakeRig(NpcRecipe r)
        {
            var rig = new Rig { R = r };
            float baseH, headRatio, legRatio;
            switch (r.Age)
            {
                case NpcAge.Child: baseH = 1.15f; headRatio = 0.31f; legRatio = 0.4f; break;
                case NpcAge.Youth: baseH = 1.5f; headRatio = 0.27f; legRatio = 0.43f; break;
                case NpcAge.Elder: baseH = 1.66f; headRatio = 0.25f; legRatio = 0.43f; break;
                default: baseH = 1.75f; headRatio = 0.245f; legRatio = 0.44f; break;
            }

            // Up to 1.6: the giants of the Fortim (Docs/Mapas100.md §3.3) stand head and shoulders over everyone else.
            rig.H = baseH * Mathf.Clamp(r.Height, 0.75f, 1.6f);
            rig.S = rig.H / 1.75f;
            rig.Limb = Mathf.Lerp(1f, rig.S, 0.6f);
            rig.HeadH = rig.H * headRatio;
            rig.HeadW = rig.HeadH * 0.94f;
            rig.HeadD = rig.HeadH * 0.9f;
            rig.NeckY = rig.H - rig.HeadH * 0.97f;
            rig.HipY = rig.NeckY * legRatio;
            rig.WaistY = rig.HipY + (rig.NeckY - rig.HipY) * 0.22f;

            float buildW;
            switch (r.Build)
            {
                case NpcBuild.Slim: buildW = 0.86f; break;
                case NpcBuild.Stout: buildW = 1.14f; break;
                case NpcBuild.Broad: buildW = 1.1f; break;
                default: buildW = 1f; break;
            }

            rig.Rw = 0.19f * Mathf.Lerp(1f, rig.S, 0.7f) * buildW;
            rig.Belly = r.Build == NpcBuild.Stout ? 1.16f : 1f;
            // Shoulders carry the role from far away: the smith and the carpenter are a wedge, the scholar a reed.
            switch (r.Build)
            {
                case NpcBuild.Broad: rig.ShoulderMul = 1.34f; break;
                case NpcBuild.Slim: rig.ShoulderMul = 0.9f; break;
                case NpcBuild.Stout: rig.ShoulderMul = 1.04f; break;
                default: rig.ShoulderMul = 1.06f; break;
            }

            if (r.Age == NpcAge.Elder)
            {
                rig.ShoulderMul *= 0.95f;
            }

            rig.ShoulderY = rig.NeckY - 0.05f * rig.S;
            rig.ShoulderX = rig.Rw * rig.ShoulderMul + 0.03f * rig.Limb;
            rig.WaistR = rig.Rw * 0.98f * rig.Belly;

            switch (r.Garment)
            {
                case NpcGarment.Robe:
                    rig.SkirtY0 = 0.05f; rig.SkirtR0 = rig.Rw * 1.58f * rig.Belly; break;
                case NpcGarment.Kaftan:
                    rig.SkirtY0 = 0.05f; rig.SkirtR0 = rig.Rw * 1.5f * rig.Belly; break;
                case NpcGarment.Dress:
                    rig.SkirtY0 = 0.04f; rig.SkirtR0 = rig.Rw * 1.72f * rig.Belly; break;
                case NpcGarment.Chapan:
                    rig.SkirtY0 = rig.HipY * 0.42f; rig.SkirtR0 = rig.Rw * 1.32f * rig.Belly; break;
                default:
                    rig.SkirtY0 = rig.HipY * 0.78f; rig.SkirtR0 = rig.Rw * 1.12f * rig.Belly; break;
            }

            if (LongSkirt(r.Garment))
            {
                // Narrow at the knee, flaring to the hem: a long robe reads as a bell, not a tube.
                rig.KneeY = Mathf.Lerp(rig.SkirtY0, rig.WaistY, 0.5f);
                rig.KneeR = rig.WaistR * 1.1f + (rig.SkirtR0 - rig.WaistR) * 0.12f;
            }
            else if (r.Garment == NpcGarment.Chapan)
            {
                rig.KneeY = Mathf.Lerp(rig.SkirtY0, rig.WaistY, 0.4f);
                rig.KneeR = rig.WaistR * 1.06f + (rig.SkirtR0 - rig.WaistR) * 0.3f;
            }
            else
            {
                rig.KneeY = rig.SkirtY0;
                rig.KneeR = rig.SkirtR0;
            }

            rig.UpperLen = (rig.NeckY - rig.HipY) * 0.5f;
            rig.ForeLen = rig.UpperLen * 0.86f;
            rig.HandLen = 0.06f * rig.Limb;

            float posture;
            switch (r.Posture)
            {
                case NpcPosture.Proud: posture = -5f; break;
                case NpcPosture.Stooped: posture = 15f; break;
                case NpcPosture.Shy: posture = 5f; break;
                case NpcPosture.Relaxed: posture = 2f; break;
                default: posture = 0f; break;
            }

            var hip = new Vector3(0f, rig.HipY, 0f);
            rig.BodyM = Matrix4x4.Translate(hip) * Matrix4x4.Rotate(Quaternion.Euler(posture, 0f, 0f)) * Matrix4x4.Translate(-hip);

            float headPitch = -posture * 0.8f + (r.Posture == NpcPosture.Shy ? 12f : 0f);
            var neckRest = new Vector3(0f, rig.NeckY, 0f);
            Vector3 neckPosed = rig.BodyM.MultiplyPoint3x4(neckRest);
            rig.HeadM = Matrix4x4.Translate(neckPosed) * Matrix4x4.Rotate(Quaternion.Euler(headPitch, 0f, 0f)) * Matrix4x4.Translate(-neckRest);
            rig.HeadCenter = new Vector3(0f, rig.NeckY + rig.HeadH * 0.5f, 0.015f * rig.S);

            PoseArm(ref rig, false);
            PoseArm(ref rig, true);
            return rig;
        }

        private static bool Grounded(NpcProp p) =>
            p == NpcProp.CaravanStaff || p == NpcProp.LanternPole || p == NpcProp.Crook || p == NpcProp.MoonStaff || p == NpcProp.WalkingStick;

        /// <summary>Half the gap between two hands holding something in front (the thing fits between the palms).</summary>
        private static float FrontGap(NpcRecipe r)
        {
            if (r.Has(NpcProp.Jar)) return 0.1f;
            if (r.Has(NpcProp.Tablets)) return 0.085f;
            if (r.Has(NpcProp.Letter) || r.Has(NpcProp.Flute)) return 0.1f;
            if (r.Has(NpcProp.Cup)) return 0.05f;
            return 0.065f;
        }

        /// <summary>
        /// Where the hand socket of a pose sits (right arm, body rest space; the left arm mirrors x), which way the elbow
        /// points (IK pole) and the axis through the fist. Targets are built from the body's own measures, so a stout
        /// figure holds the cup in front of the belly and a wide dress pushes the hanging hands out a little.
        /// </summary>
        private static void PoseTarget(ref Rig rig, NpcArmPose pose, out Vector3 hand, out Vector3 pole, out Vector3 grip)
        {
            float s = rig.S;
            float reach = rig.UpperLen + rig.ForeLen + rig.HandLen;
            float handHalf = 0.05f * rig.Limb;
            NpcRecipe r = rig.R;
            switch (pose)
            {
                case NpcArmPose.HoldFront:
                {
                    float y = Mathf.Lerp(rig.WaistY, rig.ShoulderY, 0.12f);
                    hand = new Vector3(FrontGap(r) * s, y, rig.FrontZ(y) + 0.085f * s);
                    pole = new Vector3(1f, -0.7f, -0.5f);
                    grip = Vector3.up;
                    break;
                }
                case NpcArmPose.HoldStaff:
                {
                    bool cane = r.Has(NpcProp.WalkingStick);
                    float y = cane ? rig.WaistY - 0.02f * s : Mathf.Lerp(rig.WaistY, rig.ShoulderY, 0.32f);
                    float x = Mathf.Max(rig.ShoulderX + (cane ? 0.07f : 0.05f) * s, rig.SideX(y) + handHalf + 0.05f * s);
                    hand = new Vector3(x, y, rig.FrontZ(y) * 0.6f + (cane ? 0.1f : 0.13f) * s);
                    pole = new Vector3(0.8f, -0.4f, -0.7f);
                    grip = Vector3.up;
                    break;
                }
                case NpcArmPose.HipRest:
                {
                    float y = rig.WaistY + 0.02f * s;
                    hand = new Vector3(rig.SideX(y) + 0.035f * s, y, 0.01f);
                    pole = new Vector3(1f, 0.1f, -0.45f);
                    grip = Vector3.forward;
                    break;
                }
                case NpcArmPose.Carry:
                    hand = new Vector3(rig.ShoulderX + 0.035f * s, rig.ShoulderY + 0.1f * s, 0.15f * s);
                    pole = new Vector3(0.45f, -1f, 0.25f);
                    grip = Vector3.forward;
                    break;
                case NpcArmPose.HoldUp:
                {
                    float y = rig.ShoulderY - 0.12f * s;
                    hand = new Vector3(rig.ShoulderX * 0.78f, y, rig.FrontZ(y) + 0.2f * s);
                    pole = new Vector3(1f, -1f, -0.3f);
                    grip = Vector3.up;
                    break;
                }
                case NpcArmPose.Crook:
                {
                    float y = rig.WaistY + 0.06f * s;
                    hand = new Vector3(rig.ShoulderX * 0.92f, y, rig.FrontZ(y) + 0.22f * s);
                    pole = new Vector3(0.35f, -1f, -0.55f);
                    grip = Vector3.up;
                    break;
                }
                default:
                {
                    // Hanging at rest: the arm falls along the side, elbow soft and a little back, the hand just in front
                    // of the thigh and clear of the skirt (no T or A pose).
                    float y = rig.ShoulderY - reach * 0.9f;
                    float x = Mathf.Max(rig.ShoulderX + 0.015f * s, rig.SideX(y) + handHalf * 0.9f);
                    hand = new Vector3(x, y, 0.045f * s);
                    pole = new Vector3(0.3f, 0f, -1f);
                    grip = Vector3.forward;
                    break;
                }
            }
        }

        /// <summary>Two-bone IK: the elbow for a shoulder→socket reach, bent toward <paramref name="pole"/>.</summary>
        private static Vector3 SolveElbow(Vector3 shoulder, ref Vector3 target, float upper, float lower, Vector3 pole)
        {
            Vector3 to = target - shoulder;
            float d = to.magnitude;
            float maxD = (upper + lower) * 0.985f;
            float minD = Mathf.Abs(upper - lower) + 0.02f;
            Vector3 dir = d > 1e-4f ? to / d : Vector3.down;
            if (d > maxD || d < minD)
            {
                d = Mathf.Clamp(d, minD, maxD);
                target = shoulder + dir * d;
            }

            float along = (upper * upper - lower * lower + d * d) / (2f * d);
            float h = Mathf.Sqrt(Mathf.Max(0f, upper * upper - along * along));
            Vector3 side = pole - dir * Vector3.Dot(pole, dir);
            if (side.sqrMagnitude < 1e-6f)
            {
                side = Vector3.Cross(dir, Vector3.right);
            }

            return shoulder + dir * along + side.normalized * h;
        }

        private static void PoseArm(ref Rig rig, bool right)
        {
            NpcArmPose pose = right ? rig.R.RightArm : rig.R.LeftArm;
            PoseTarget(ref rig, pose, out Vector3 hand, out Vector3 pole, out Vector3 grip);
            float m = right ? 1f : -1f;
            hand.x *= m;
            pole.x *= m;
            grip.x *= m;

            var shoulder = new Vector3(rig.ShoulderX * m, rig.ShoulderY, 0f);
            float lower = rig.ForeLen + rig.HandLen;
            Vector3 elbow = SolveElbow(shoulder, ref hand, rig.UpperLen, lower, pole);
            Vector3 fore = (hand - elbow).normalized;
            Vector3 wrist = hand - fore * rig.HandLen;

            Vector3 s = rig.BodyM.MultiplyPoint3x4(shoulder);
            Vector3 e = rig.BodyM.MultiplyPoint3x4(elbow);
            Vector3 h = rig.BodyM.MultiplyPoint3x4(hand);
            Vector3 w = rig.BodyM.MultiplyPoint3x4(wrist);
            Vector3 f = rig.BodyM.MultiplyVector(fore).normalized;
            Vector3 g = pose == NpcArmPose.HoldStaff ? Vector3.up : rig.BodyM.MultiplyVector(grip); // a staff stands vertical
            g -= f * Vector3.Dot(g, f);
            if (g.sqrMagnitude < 1e-5f)
            {
                g = Vector3.Cross(f, Vector3.right);
            }

            g.Normalize();
            bool grounded = pose == NpcArmPose.HoldStaff && right && System.Array.Exists(rig.R.Props, Grounded);
            if (right)
            {
                rig.ShoulderR = s; rig.ElbowR = e; rig.HandR = h; rig.WristR = w; rig.ForeR = f; rig.GripR = g; rig.GroundedR = grounded;
            }
            else
            {
                rig.ShoulderL = s; rig.ElbowL = e; rig.HandL = h; rig.WristL = w; rig.ForeL = f; rig.GripL = g; rig.GroundedL = grounded;
            }
        }

        // ------------------------------------------------------------------
        // Legs, skirt, torso, layers
        // ------------------------------------------------------------------

        private static bool LongSkirt(NpcGarment g) => g == NpcGarment.Robe || g == NpcGarment.Kaftan || g == NpcGarment.Dress;

        private static void BuildLegsAndSkirt(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            b.Bone = NpcBones.Root;
            b.Matrix = Matrix4x4.identity;
            float lx = rig.Rw * 0.45f;
            float L = rig.Limb;
            bool longSkirt = LongSkirt(r.Garment);

            if (r.HorseBody)
            {
                BuildHorse(b, ref rig);
            }

            foreach (float side in r.HorseBody ? new float[0] : new[] { -1f, 1f })
            {
                float x = lx * side;
                if (!longSkirt)
                {
                    b.Segment(new Vector3(x, rig.SkirtY0 + 0.04f, 0f), new Vector3(x, 0.13f, 0.005f), 0.078f * L, 0.07f * L, 6, r.Trousers, caps: false);
                }

                b.Cylinder(new Vector3(x, 0f, 0.005f), 0.082f * L, 0.074f * L, 0.16f * L, 6, r.Boots, r.Boots);
                float toeZ = longSkirt ? Mathf.Max(0.08f * L, rig.SkirtR0 * 0.84f - 0.05f) : 0.08f * L;
                b.Box(new Vector3(x, 0.045f * L, toeZ), new Vector3(0.12f, 0.09f, 0.15f) * L, r.Boots, noBottom: true);
            }

            // The skirt / lower garment (root bone: it stays put while the chest breathes): waist → knee → a flared hem.
            int sides = longSkirt ? 10 : 8;
            float[] rings = rig.KneeY > rig.SkirtY0 + 0.01f
                ? new[] { rig.SkirtY0, Mathf.Lerp(rig.SkirtY0, rig.KneeY, 0.4f), rig.KneeY, rig.WaistY }
                : new[] { rig.SkirtY0, rig.WaistY };
            for (int i = 0; i + 1 < rings.Length; i++)
            {
                float y0 = rings[i], y1 = rings[i + 1];
                b.Cylinder(new Vector3(0f, y0, 0f), rig.SkirtRadius(y0), rig.SkirtRadius(y1), y1 - y0, sides, r.Main, topCap: false, depth: 0.84f);
            }

            if (r.Garment == NpcGarment.Kaftan || r.Garment == NpcGarment.Dress || r.Garment == NpcGarment.Chapan || r.Garment == NpcGarment.Robe)
            {
                float hemH = (r.Garment == NpcGarment.Robe ? 0.045f : 0.07f) * rig.S;
                float rAt = rig.SkirtRadius(rig.SkirtY0 + hemH);
                b.Cylinder(new Vector3(0f, rig.SkirtY0 - 0.005f, 0f), rig.SkirtR0 * 1.03f, rAt * 1.035f, hemH, sides, r.Trim, topCap: false, depth: 0.84f);
            }

            if (r.Garment == NpcGarment.Kaftan || r.Garment == NpcGarment.Chapan)
            {
                // Front opening band down the skirt.
                var from = new Vector3(0f, rig.SkirtY0 + 0.01f, rig.SkirtFrontZ(rig.SkirtY0) + 0.012f);
                var to = new Vector3(0f, rig.WaistY, rig.SkirtFrontZ(rig.WaistY) + 0.012f);
                SlantedPanel(b, from, to, 0.055f * rig.S, r.Trim);
            }

            if (r.Has(NpcLayer.Patches))
            {
                float y = Mathf.Lerp(rig.SkirtY0, rig.WaistY, 0.45f);
                Vector3 p = new Vector3(rig.Rw * 0.5f, y, rig.SkirtFrontZ(y) * 0.92f + 0.012f);
                b.Decal(p, new Vector2(0.09f, 0.08f) * rig.S, r.Trim, Quaternion.Euler(-8f, 24f, 8f));
            }
        }

        /// <summary>
        /// A centaur's horse half (root bone: it stays put while the chest breathes): the barrel from under the human hips back to
        /// the rump, a chest that joins it to the waist, four legs with dark hooves, the tail in the hair colour and a felt
        /// saddle blanket with a trim band.
        /// </summary>
        private static void BuildHorse(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            float s = rig.S;
            float barrel = 0.17f * s;
            float yb = rig.HipY * 0.95f - barrel * 0.6f;
            var chest = new Vector3(0f, yb, 0.05f * s);
            var rump = new Vector3(0f, yb + 0.02f * s, -0.6f * s);
            b.Segment(chest, rump, barrel, barrel * 0.95f, 8, r.Coat);
            b.Blob(chest + new Vector3(0f, 0.04f * s, 0.03f * s), new Vector3(barrel * 2.1f, barrel * 2.2f, barrel * 2.0f), r.Coat, coarse: true);
            foreach (float z in new[] { 0.02f * s, -0.58f * s })
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    float x = side * 0.085f * s;
                    b.Segment(new Vector3(x, yb - barrel * 0.4f, z), new Vector3(x, 0.08f * s, z + 0.01f * s), 0.045f * s, 0.032f * s, 6, r.Coat, caps: false);
                    b.Box(new Vector3(x, 0.04f * s, z + 0.015f * s), new Vector3(0.075f, 0.08f, 0.09f) * s, NpcSwatch.Charcoal, noBottom: true);
                }
            }

            Vector3 tail = rump + new Vector3(0f, 0.05f * s, -0.14f * s);
            b.Segment(tail, tail + new Vector3(0f, -0.32f, -0.16f) * s, 0.05f * s, 0.015f * s, 5, r.HairColor);
            b.Box(new Vector3(0f, yb + barrel * 0.9f, -0.3f * s), new Vector3(barrel * 2.15f, 0.03f * s, 0.32f * s), r.LayerColor, noBottom: true);
            b.Box(new Vector3(0f, yb + barrel * 0.9f + 0.005f, -0.14f * s), new Vector3(barrel * 2.2f, 0.035f * s, 0.04f * s), r.LayerTrim, noBottom: true);
        }

        /// <summary>A thin flat strip from <paramref name="from"/> to <paramref name="to"/>, facing +z (bands, apron panels).</summary>
        private static void SlantedPanel(NpcMeshBuilder b, Vector3 from, Vector3 to, float width, NpcSwatch swatch, float thickness = 0.02f)
        {
            Vector3 along = to - from;
            float len = along.magnitude;
            if (len < 1e-4f)
            {
                return;
            }

            Vector3 normal = Vector3.Cross(Vector3.right, along).normalized;
            if (normal.z < 0f)
            {
                normal = -normal;
            }

            Quaternion rot = Quaternion.LookRotation(normal, along);

            b.Box((from + to) * 0.5f, new Vector3(width, len, thickness), swatch, rot, noBottom: true);
        }

        private static void BuildTorso(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            b.Bone = NpcBones.Body;
            b.Matrix = rig.BodyM;
            float torsoH = rig.NeckY - rig.HipY;
            b.Cylinder(new Vector3(0f, rig.HipY, 0f), rig.Rw * 0.92f * rig.Belly, rig.Rw * rig.ShoulderMul, torsoH, 8, r.Main, r.Main, depth: 0.78f);
            if (r.Build == NpcBuild.Stout)
            {
                b.Blob(new Vector3(0f, rig.WaistY + torsoH * 0.12f, rig.Rw * 0.3f), new Vector3(rig.Rw * 1.7f, torsoH * 0.62f, rig.Rw * 1.15f), r.Main);
            }

            // Neck.
            b.Cylinder(new Vector3(0f, rig.NeckY - 0.05f * rig.S, 0f), 0.058f * rig.Limb, 0.055f * rig.Limb, 0.11f * rig.S, 6, r.Skin, topCap: false);

            if (r.Garment == NpcGarment.Kaftan || r.Garment == NpcGarment.Chapan)
            {
                var from = new Vector3(0f, rig.WaistY, rig.TorsoFrontZ(rig.WaistY) + 0.012f);
                var to = new Vector3(0f, rig.NeckY - 0.02f, rig.TorsoFrontZ(rig.NeckY) + 0.012f);
                SlantedPanel(b, from, to, 0.055f * rig.S, r.Trim);
            }
            else if (r.Garment == NpcGarment.Dress || r.Garment == NpcGarment.Robe)
            {
                // Collar in the trim colour.
                b.Cylinder(new Vector3(0f, rig.NeckY - 0.045f * rig.S, 0f), rig.Rw * 0.55f, rig.Rw * 0.42f, 0.04f * rig.S, 8, r.Trim, topCap: false, depth: 0.85f);
            }

            if (r.Has(NpcLayer.Patches))
            {
                float y = Mathf.Lerp(rig.WaistY, rig.NeckY, 0.5f);
                b.Decal(new Vector3(-rig.Rw * 0.42f, y, rig.TorsoFrontZ(y) * 0.93f + 0.012f), new Vector2(0.07f, 0.07f) * rig.S, r.LayerTrim, Quaternion.Euler(0f, -26f, -10f));
            }
        }

        private static void BuildLayers(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            float torsoH = rig.NeckY - rig.HipY;
            float s = rig.S;

            if (r.Has(NpcLayer.Apron))
            {
                b.Bone = NpcBones.Body;
                b.Matrix = rig.BodyM;
                float bibTop = rig.WaistY + (rig.NeckY - rig.WaistY) * 0.78f;
                var from = new Vector3(0f, rig.WaistY - 0.02f, rig.TorsoFrontZ(rig.WaistY) + 0.016f);
                var to = new Vector3(0f, bibTop, rig.TorsoFrontZ(bibTop) + 0.016f);
                SlantedPanel(b, from, to, rig.Rw * 1.05f, r.LayerColor, 0.025f);
                b.Cylinder(new Vector3(0f, rig.WaistY - 0.025f, 0f), rig.Rw * 0.99f * rig.Belly + 0.012f, rig.Rw * 0.99f * rig.Belly + 0.012f, 0.035f * s, 8, r.LayerColor, topCap: false, depth: 0.8f);

                b.Bone = NpcBones.Root;
                b.Matrix = Matrix4x4.identity;
                float bottom = Mathf.Max(rig.SkirtY0 + 0.04f, LongSkirt(r.Garment) ? rig.HipY * 0.3f : rig.SkirtY0 - 0.08f);
                float zTop = rig.SkirtFrontZ(rig.WaistY) + 0.02f;
                float zBottom = r.Garment == NpcGarment.Tunic || r.Garment == NpcGarment.Chapan
                    ? Mathf.Max(rig.SkirtFrontZ(Mathf.Max(bottom, rig.SkirtY0)) + 0.02f, zTop)
                    : rig.SkirtFrontZ(bottom) + 0.02f;
                SlantedPanel(b, new Vector3(0f, bottom, zBottom), new Vector3(0f, rig.WaistY, zTop), rig.Rw * 1.3f, r.LayerColor, 0.025f);
                float py = Mathf.Lerp(bottom, rig.WaistY, 0.55f);
                float pz = Mathf.Lerp(zBottom, zTop, 0.55f) + 0.016f;
                b.Decal(new Vector3(0f, py, pz), new Vector2(rig.Rw * 0.8f, 0.09f * s), r.LayerTrim, Quaternion.Euler(-Mathf.Atan2(zBottom - zTop, rig.WaistY - bottom) * Mathf.Rad2Deg, 0f, 0f));
            }

            b.Bone = NpcBones.Body;
            b.Matrix = rig.BodyM;

            if (r.Has(NpcLayer.Vest))
            {
                float h = (rig.NeckY - rig.WaistY) * 0.92f;
                b.Cylinder(new Vector3(0f, rig.WaistY - 0.01f, 0f), rig.Rw * 0.96f * rig.Belly + 0.022f, rig.Rw * rig.ShoulderMul + 0.02f, h, 8, r.LayerColor, topCap: false, depth: 0.8f);
                var from = new Vector3(0f, rig.WaistY, rig.TorsoFrontZ(rig.WaistY) + 0.03f);
                var to = new Vector3(0f, rig.WaistY + h - 0.02f, rig.TorsoFrontZ(rig.WaistY + h) + 0.026f);
                SlantedPanel(b, from, to, 0.07f * s, r.Main, 0.012f);
            }

            if (r.Has(NpcLayer.Sash))
            {
                float rr = rig.Rw * 0.98f * rig.Belly + 0.024f;
                b.Cylinder(new Vector3(0f, rig.WaistY - 0.05f * s, 0f), rr, rr * 0.98f, 0.085f * s, 8, r.LayerColor, topCap: false, depth: 0.82f);
                b.Box(new Vector3(rig.Rw * 0.55f, rig.WaistY - 0.16f * s, rig.TorsoFrontZ(rig.WaistY) + 0.01f), new Vector3(0.07f, 0.2f, 0.02f) * s, r.LayerTrim, Quaternion.Euler(-6f, 12f, 6f));
            }
            else if (r.Has(NpcLayer.Belt))
            {
                float rr = rig.Rw * 0.97f * rig.Belly + 0.02f;
                b.Cylinder(new Vector3(0f, rig.WaistY - 0.04f * s, 0f), rr, rr, 0.05f * s, 8, NpcSwatch.LeatherDark, topCap: false, depth: 0.82f);
                b.Box(new Vector3(0f, rig.WaistY - 0.015f * s, rig.TorsoFrontZ(rig.WaistY) + 0.03f), new Vector3(0.07f, 0.06f, 0.02f) * s, NpcSwatch.Brass);
            }

            if (r.Has(NpcLayer.Blanket))
            {
                // Basotho-style blanket: three bands, alternating colours, from the elbows to the neck.
                float bottomY = rig.NeckY - 0.42f * s;
                float topY = rig.NeckY + 0.02f * s;
                float r0 = rig.Rw * rig.ShoulderMul * 1.36f;
                float r1 = 0.1f * s;
                for (int i = 0; i < 3; i++)
                {
                    float y0 = Mathf.Lerp(bottomY, topY, i / 3f);
                    float y1 = Mathf.Lerp(bottomY, topY, (i + 1) / 3f);
                    float ra = Mathf.Lerp(r0, r1, Mathf.Pow(i / 3f, 1.6f));
                    float rb = Mathf.Lerp(r0, r1, Mathf.Pow((i + 1) / 3f, 1.6f));
                    b.Cylinder(new Vector3(0f, y0, -0.01f), ra, rb, y1 - y0, 8, i % 2 == 0 ? r.LayerColor : r.LayerTrim, topCap: false, depth: 0.86f);
                }
            }
            else if (r.Has(NpcLayer.Shawl))
            {
                float y0 = rig.NeckY - 0.24f * s;
                b.Cylinder(new Vector3(0f, y0, -0.005f), rig.Rw * rig.ShoulderMul * 1.32f, 0.1f * s, 0.27f * s, 8, r.LayerColor, topCap: false, depth: 0.86f);
                b.Cylinder(new Vector3(0f, y0 - 0.02f * s, -0.005f), rig.Rw * rig.ShoulderMul * 1.36f, rig.Rw * rig.ShoulderMul * 1.3f, 0.035f * s, 8, r.LayerTrim, topCap: false, depth: 0.86f);
            }

            if (r.Has(NpcLayer.Scarf))
            {
                b.Cylinder(new Vector3(0f, rig.NeckY - 0.075f * s, 0f), rig.Rw * 0.66f, rig.Rw * 0.5f, 0.12f * s, 8, r.HeadColor, r.HeadColor, depth: 0.92f);
                b.Box(new Vector3(rig.Rw * 0.25f, rig.NeckY - 0.18f * s, rig.TorsoFrontZ(rig.NeckY - 0.18f * s) + 0.02f), new Vector3(0.08f, 0.17f, 0.025f) * s, r.HeadColor, Quaternion.Euler(-10f, 0f, -8f));
            }

            if (r.Has(NpcLayer.Cape))
            {
                float topY = rig.NeckY - 0.02f;
                float bottomY = Mathf.Max(0.18f, rig.HipY * 0.35f);
                float zTop = -rig.Rw * 0.62f;
                float zBottom = -rig.Rw * 1.15f - 0.06f;
                float wTop = rig.Rw * rig.ShoulderMul * 1.05f;
                float wBottom = rig.Rw * 1.5f;
                var a = new Vector3(-wTop, topY, zTop);
                var c = new Vector3(wTop, topY, zTop);
                var d = new Vector3(wBottom, bottomY, zBottom);
                var e = new Vector3(-wBottom, bottomY, zBottom);
                var m = new Vector3(0f, (topY + bottomY) * 0.5f, (zTop + zBottom) * 0.5f - 0.03f);
                b.TwoSided(a, c, m, r.LayerColor, r.LayerTrim);
                b.TwoSided(c, d, m, r.LayerColor, r.LayerTrim);
                b.TwoSided(d, e, m, r.LayerColor, r.LayerTrim);
                b.TwoSided(e, a, m, r.LayerColor, r.LayerTrim);
            }

            if (r.Has(NpcLayer.Satchel))
            {
                var from = new Vector3(-rig.Rw * 0.62f, rig.NeckY - 0.03f, rig.TorsoFrontZ(rig.NeckY) + 0.022f);
                var to = new Vector3(rig.Rw * 0.82f, rig.HipY + 0.08f * s, rig.TorsoFrontZ(rig.HipY) * rig.Belly + 0.03f);
                SlantedPanel(b, from, to, 0.045f * s, NpcSwatch.LeatherDark, 0.015f);
                b.Box(new Vector3(rig.Rw * 1.02f, rig.HipY + 0.02f * s, 0.03f), new Vector3(0.08f, 0.16f, 0.2f) * s, NpcSwatch.Leather, Quaternion.Euler(0f, 0f, 6f), NpcSwatch.LeatherDark);
            }

            _ = torsoH;
        }

        // ------------------------------------------------------------------
        // Arms
        // ------------------------------------------------------------------

        private static bool IsSkin(NpcSwatch swatch) => swatch <= NpcSwatch.SkinDeep || swatch == NpcSwatch.SkinBark
            || swatch == NpcSwatch.SkinOrcOlive || swatch == NpcSwatch.SkinOrcGrey || swatch == NpcSwatch.SkinKoboldRust || swatch == NpcSwatch.SkinKoboldSlate
            || swatch == NpcSwatch.SkinAurochs || swatch == NpcSwatch.SkinAurochsTawny || swatch == NpcSwatch.SkinReef || swatch == NpcSwatch.SkinNacre;

        /// <summary>
        /// One arm with volume: a shoulder cap, a tapered upper arm, an elbow, a forearm ending in a cuff (wide bell sleeves
        /// on long robes, a rolled short sleeve when the arm is bare) and a mitten hand closed around the grip axis, so a
        /// staff or a handle passes through the fist at <see cref="Rig.HandR"/>.
        /// </summary>
        private static void BuildArm(NpcMeshBuilder b, ref Rig rig, bool right)
        {
            NpcRecipe r = rig.R;
            b.Bone = right ? NpcBones.ArmR : NpcBones.ArmL;
            b.Matrix = Matrix4x4.identity;
            Vector3 s = right ? rig.ShoulderR : rig.ShoulderL;
            Vector3 e = right ? rig.ElbowR : rig.ElbowL;
            Vector3 w = right ? rig.WristR : rig.WristL;
            Vector3 h = right ? rig.HandR : rig.HandL;
            Vector3 f = right ? rig.ForeR : rig.ForeL;
            Vector3 g = right ? rig.GripR : rig.GripL;
            float L = rig.Limb;
            bool bare = IsSkin(r.Sleeves);
            bool bell = !bare && (r.Garment == NpcGarment.Robe || r.Garment == NpcGarment.Kaftan);
            NpcSwatch skin = r.Skin;
            NpcSwatch sleeve = r.Sleeves;
            NpcSwatch top = bare ? r.Main : sleeve;
            float bulk = r.Build == NpcBuild.Broad ? 1.22f : r.Build == NpcBuild.Stout ? 1.12f : r.Build == NpcBuild.Slim ? 0.92f : 1f;
            if (r.Age == NpcAge.Child)
            {
                bulk *= 0.92f;
            }

            float upperR = 0.07f * L * bulk;
            float shoulderD = 0.165f * L * bulk;

            // Shoulder cap (in the garment's colour), a little low so it rounds off the top of the torso.
            b.Blob(s + Vector3.down * 0.012f * L, new Vector3(shoulderD * 1.08f, shoulderD, shoulderD), top, coarse: true);

            // Upper arm, tapering to the elbow.
            if (bare)
            {
                Vector3 sleeveEnd = Vector3.Lerp(s, e, 0.42f);
                b.Segment(s, sleeveEnd, upperR * 1.18f, upperR * 1.12f, 6, r.Main, r.Trim, caps: false);
                b.Segment(sleeveEnd - (e - s).normalized * 0.01f, e, upperR * 0.98f, upperR * 0.82f, 6, skin, caps: false);
            }
            else
            {
                b.Segment(s, e, upperR * 1.08f, upperR * 0.86f, 6, sleeve, caps: false);
            }

            NpcSwatch elbowColor = bare ? skin : sleeve;
            b.Blob(e, Vector3.one * upperR * 1.8f, elbowColor, coarse: true);

            // Forearm and cuff.
            float wristR = upperR * 0.74f;
            if (bare)
            {
                b.Segment(e, w, upperR * 0.84f, wristR, 6, skin, caps: false);
            }
            else
            {
                float cuffR = bell ? upperR * 1.45f : upperR * 0.95f;
                float cuffLen = (bell ? 0.07f : 0.045f) * L;
                Vector3 cuffAt = w - f * cuffLen;
                b.Segment(e, cuffAt, upperR * 0.86f, cuffR * 0.94f, 6, sleeve, caps: false);
                NpcSwatch cuff = r.Trim == r.Main || r.Trim == sleeve ? NpcSwatch.Cream : r.Trim;
                b.Segment(cuffAt, w + f * 0.006f, cuffR, cuffR * 1.04f, 6, bell ? sleeve : cuff, cuff, caps: true);
                if (bell)
                {
                    b.Segment(w - f * 0.012f, w + f * 0.008f, cuffR * 1.06f, cuffR * 1.08f, 6, cuff, cuff, caps: false);
                }
            }

            // Mitten hand: a soft fist around the grip axis, with a thumb on top.
            NpcSwatch hand = r.Hands ?? skin;
            Quaternion hr = Quaternion.LookRotation(f, g);
            b.Segment(w - f * 0.01f, w + f * 0.02f * L, wristR * 0.95f, wristR * 0.9f, 5, hand, caps: false);
            b.Blob(h, new Vector3(0.078f, 0.094f, 0.112f) * L * Mathf.Lerp(1f, bulk, 0.5f), hand, hr);
            Vector3 inner = Vector3.Cross(f, g).normalized;
            if (Vector3.Dot(inner, right ? Vector3.left : Vector3.right) < 0f)
            {
                inner = -inner;
            }

            b.Blob(h + g * 0.036f * L + inner * 0.02f * L - f * 0.012f * L, new Vector3(0.04f, 0.04f, 0.058f) * L, hand, hr, coarse: true);
        }

        // ------------------------------------------------------------------
        // Head: face, hair, beard, headwear
        // ------------------------------------------------------------------

        /// <summary>Point on the head's ellipsoid at (x, y) from its centre, plus a rotation facing out of the surface.</summary>
        private static Vector3 OnFace(ref Rig rig, float x, float y, float lift, out Quaternion rotation)
        {
            float a = rig.HeadW * 0.5f, bb = rig.HeadH * 0.5f, c = rig.HeadD * 0.5f;
            float k = 1f - (x * x) / (a * a) - (y * y) / (bb * bb);
            float z = c * Mathf.Sqrt(Mathf.Max(0.02f, k));
            var normal = new Vector3(x / (a * a), y / (bb * bb), z / (c * c)).normalized;
            rotation = Quaternion.FromToRotation(Vector3.forward, normal);
            return rig.HeadCenter + new Vector3(x, y, z) + normal * lift;
        }

        private static void BuildHead(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            b.Bone = NpcBones.Head;
            b.Matrix = rig.HeadM;
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD;
            Vector3 hc = rig.HeadCenter;

            b.Blob(hc, new Vector3(hw, hh, hd), r.Skin);
            BuildFace(b, ref rig);

            bool covers = r.Headwear == NpcHeadwear.Kerchief || r.Headwear == NpcHeadwear.Hood || r.Headwear == NpcHeadwear.Turban
                || r.Headwear == NpcHeadwear.Headwrap || r.Headwear == NpcHeadwear.LampHelmet;
            bool coversEars = covers;

            if (r.ElfEars && !coversEars)
            {
                // Elf ears: a long flat leaf of skin from the side of the head, swept up and back past the hair (it pokes
                // through long hair on purpose: the ears are how an elf reads from the iso camera).
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 root = hc + new Vector3(side * hw * 0.44f, -hh * 0.02f, -hd * 0.06f);
                    Quaternion tilt = Quaternion.FromToRotation(Vector3.up, new Vector3(side * 0.85f, 0.42f, -0.32f).normalized);
                    b.Cylinder(root, 0.042f * rig.S, 0f, 0.17f * rig.S, 4, r.Skin, rotation: tilt, depth: 0.45f, twist: 45f);
                }
            }
            else if (!coversEars && !r.Horns && !r.Gills && r.Hair != NpcHair.Long && r.Hair != NpcHair.Curly)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    b.Blob(hc + new Vector3(side * hw * 0.49f, -hh * 0.05f, -hd * 0.02f), new Vector3(0.05f, 0.085f, 0.06f) * rig.S, r.Skin, coarse: true);
                }
            }

            BuildHair(b, ref rig, covers);
            BuildBeard(b, ref rig);
            BuildHeadwear(b, ref rig);
            if (r.Twigs)
            {
                BuildTwigs(b, ref rig);
            }

            if (r.Horns)
            {
                BuildHorns(b, ref rig);
            }

            if (r.Gills)
            {
                BuildFins(b, ref rig, coversEars);
            }
        }

        /// <summary>
        /// The trunk folk (Arvoredo de Haripushpa): three twigs of bark growing up and out from the crown, each with a tuft of
        /// spring leaf at the tip, over any headwear (they read from the iso camera as a little tree).
        /// </summary>
        private static void BuildTwigs(NpcMeshBuilder b, ref Rig rig)
        {
            float hw = rig.HeadW, hh = rig.HeadH;
            Vector3 hc = rig.HeadCenter;
            float s = rig.S;
            foreach (float x in new[] { -0.6f, 0f, 0.6f })
            {
                Vector3 root = hc + new Vector3(x * hw * 0.3f, hh * 0.42f, -0.01f * s);
                Vector3 tip = root + new Vector3(x * 0.16f, 0.17f - Mathf.Abs(x) * 0.04f, -0.02f) * s;
                b.Segment(root, tip, 0.022f * s, 0.01f * s, 4, rig.R.Skin, caps: false);
                b.Blob(tip, new Vector3(0.08f, 0.06f, 0.08f) * s, NpcSwatch.SpringLeaf, coarse: true);
            }
        }

        /// <summary>
        /// The povo-touro (Aratta): a pair of wide aurochs horns from the sides of the crown, curving out, a little forward and
        /// up (horn ivory, dark tips), and cow ears sticking out sideways under them. Drawn over any headwear: the horns are how
        /// they read from the iso camera.
        /// </summary>
        private static void BuildHorns(NpcMeshBuilder b, ref Rig rig)
        {
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD;
            Vector3 hc = rig.HeadCenter;
            float s = rig.S;
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 root = hc + new Vector3(side * hw * 0.36f, hh * 0.3f, -hd * 0.02f);
                Vector3 p1 = root + new Vector3(side * 0.12f, 0.015f, 0.025f) * s;
                Vector3 p2 = root + new Vector3(side * 0.23f, 0.08f, 0.05f) * s;
                Vector3 p3 = root + new Vector3(side * 0.26f, 0.19f, 0.03f) * s;
                b.Segment(root, p1, 0.042f * s, 0.036f * s, 5, NpcSwatch.Horn);
                b.Segment(p1, p2, 0.036f * s, 0.024f * s, 5, NpcSwatch.Horn);
                b.Segment(p2, p3, 0.024f * s, 0.005f * s, 4, NpcSwatch.Charcoal);

                Vector3 ear = hc + new Vector3(side * hw * 0.55f, hh * 0.06f, -hd * 0.06f);
                b.Blob(ear, new Vector3(0.1f, 0.035f, 0.06f) * s, rig.R.Skin, Quaternion.Euler(0f, 0f, side * -18f), coarse: true);
            }
        }

        /// <summary>
        /// The povo-do-mar (Kirta): a fin in place of each ear, swept back (turquoise on nacre skin, live coral on reef skin),
        /// and a fin crest from the brow to the nape when the head is bare or only banded.
        /// </summary>
        private static void BuildFins(NpcMeshBuilder b, ref Rig rig, bool coversEars)
        {
            NpcRecipe r = rig.R;
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD;
            Vector3 hc = rig.HeadCenter;
            float s = rig.S;
            NpcSwatch fin = r.Skin == NpcSwatch.SkinNacre ? NpcSwatch.Turquoise : NpcSwatch.CoralLive;
            if (!coversEars)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 root = hc + new Vector3(side * hw * 0.47f, -hh * 0.02f, -hd * 0.04f);
                    Vector3 up = root + new Vector3(side * 0.06f, 0.1f, -0.11f) * s;
                    Vector3 down = root + new Vector3(side * 0.05f, -0.05f, -0.09f) * s;
                    Vector3 mid = root + new Vector3(side * 0.07f, 0.02f, -0.14f) * s;
                    b.TwoSided(root, up, mid, fin);
                    b.TwoSided(root, mid, down, fin);
                }
            }

            bool bare = r.Headwear == NpcHeadwear.None || r.Headwear == NpcHeadwear.Headband || r.Headwear == NpcHeadwear.FlowerCrown
                || r.Headwear == NpcHeadwear.SunCirclet;
            if (bare)
            {
                Vector3 brow = hc + new Vector3(0f, hh * 0.46f, hd * 0.2f);
                Vector3 crown = hc + new Vector3(0f, hh * 0.5f + 0.09f * s, -hd * 0.05f);
                Vector3 nape = hc + new Vector3(0f, hh * 0.25f, -hd * 0.5f);
                Vector3 top = hc + new Vector3(0f, hh * 0.5f, -hd * 0.1f);
                b.TwoSided(brow, crown, top, fin);
                b.TwoSided(top, crown, nape, fin);
            }
        }

        private static bool DarkSkin(NpcSwatch skin) => skin == NpcSwatch.SkinBrown || skin == NpcSwatch.SkinDeep;

        /// <summary>Rosy cheeks: the recipe's choice, else children, laughing, shy, bright and kind faces.</summary>
        private static bool Cheeks(NpcRecipe r) => r.RosyCheeks ?? (r.Age == NpcAge.Child || r.Face == NpcFace.Jolly || r.Face == NpcFace.Shy
            || r.Face == NpcFace.Bright || r.Face == NpcFace.Kind);

        /// <summary>
        /// The face, drawn to read from the iso camera at ~10 m: big eyes (white, a dark iris, a shine), thick brows on
        /// everyone, a wedge nose, a mouth shaped by the mood (open laugh, smile, flat line) and rosy cheeks. Lips use a
        /// rose tone on dark skin so the mouth never melts into it.
        /// </summary>
        private static void BuildFace(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            float hw = rig.HeadW, hh = rig.HeadH;
            bool child = r.Age == NpcAge.Child;
            bool dark = DarkSkin(r.Skin);
            float eyeScale = child ? 1.18f : r.Age == NpcAge.Youth ? 1.08f : 1f;
            if (r.Face == NpcFace.Bright)
            {
                eyeScale *= 1.08f;
            }

            var eye = new Vector2(hw * 0.15f, hh * 0.19f) * eyeScale;
            float eyeY = -hh * 0.05f;
            float eyeX = hw * 0.205f;
            const float lift = 0.006f;
            NpcSwatch brow = r.HairColor == NpcSwatch.HairWhite || r.HairColor == NpcSwatch.HairGrey ? NpcSwatch.BrowGrey
                : r.HairColor == NpcSwatch.HairAuburn || r.HairColor == NpcSwatch.HairBrown ? NpcSwatch.HairDark : NpcSwatch.HairBlack;
            NpcSwatch lineColor = NpcSwatch.Lash;

            foreach (float side in new[] { -1f, 1f })
            {
                float x = eyeX * side;
                Quaternion rot;
                Vector3 p;
                switch (r.Face)
                {
                    case NpcFace.Jolly:
                        // Closed, laughing eyes: a thick "^".
                        p = OnFace(ref rig, x - eye.x * 0.2f, eyeY, lift, out rot);
                        b.Decal(p, new Vector2(eye.x * 0.62f, eye.y * 0.2f), lineColor, rot * Quaternion.Euler(0f, 0f, 32f));
                        p = OnFace(ref rig, x + eye.x * 0.2f, eyeY, lift, out rot);
                        b.Decal(p, new Vector2(eye.x * 0.62f, eye.y * 0.2f), lineColor, rot * Quaternion.Euler(0f, 0f, -32f));
                        break;
                    case NpcFace.Dreamy:
                    {
                        // Half-closed: the lower half of the eye under a heavy lid line.
                        p = OnFace(ref rig, x, eyeY - eye.y * 0.18f, lift, out rot);
                        b.Disc(p, new Vector2(eye.x * 0.95f, eye.y * 0.5f), 6, NpcSwatch.Sclera, rot);
                        b.Disc(p + rot * new Vector3(0f, -eye.y * 0.04f, 0.0015f), new Vector2(eye.x * 0.62f, eye.y * 0.42f), 6, NpcSwatch.Eye, rot);
                        b.Decal(p + rot * new Vector3(0f, eye.y * 0.2f, 0.003f), new Vector2(eye.x * 1.12f, eye.y * 0.16f), lineColor, rot);
                        break;
                    }
                    case NpcFace.Calm:
                        // Gentle closed-ish eyes: a soft downward curve.
                        p = OnFace(ref rig, x, eyeY - eye.y * 0.1f, lift, out rot);
                        b.Decal(p + rot * new Vector3(-eye.x * 0.24f, 0f, 0f), new Vector2(eye.x * 0.55f, eye.y * 0.18f), lineColor, rot * Quaternion.Euler(0f, 0f, -14f));
                        b.Decal(p + rot * new Vector3(eye.x * 0.24f, 0f, 0f), new Vector2(eye.x * 0.55f, eye.y * 0.18f), lineColor, rot * Quaternion.Euler(0f, 0f, 14f));
                        break;
                    default:
                    {
                        float squash = r.Face == NpcFace.Shy ? 0.82f : r.Face == NpcFace.Stern ? 0.78f : 1f;
                        p = OnFace(ref rig, x, eyeY, lift, out rot);
                        b.Disc(p, new Vector2(eye.x, eye.y * squash), 7, NpcSwatch.Sclera, rot, 90f);
                        // The iris looks a little inward (and down when shy): friendlier than a straight stare.
                        Vector3 iris = rot * new Vector3(-side * eye.x * 0.1f, -eye.y * (r.Face == NpcFace.Shy ? 0.16f : 0.06f), 0.0015f);
                        b.Disc(p + iris, new Vector2(eye.x * 0.7f, eye.y * squash * 0.8f), 7, NpcSwatch.Eye, rot, 90f);
                        b.Disc(p + iris + rot * new Vector3(eye.x * 0.14f * side, eye.y * 0.18f, 0.0015f), new Vector2(eye.x * 0.3f, eye.y * 0.26f), 5, NpcSwatch.EyeShine, rot);
                        if (r.Face == NpcFace.Stern)
                        {
                            b.Decal(p + rot * new Vector3(0f, eye.y * squash * 0.42f, 0.003f), new Vector2(eye.x * 1.1f, eye.y * 0.16f), lineColor, rot * Quaternion.Euler(0f, 0f, 12f * side));
                        }

                        break;
                    }
                }

                // Brows: on everyone now (they carry the mood further than the eyes do).
                float browTilt;
                float browY = eyeY + eye.y * 0.78f;
                switch (r.Face)
                {
                    case NpcFace.Stern: browTilt = 20f; browY -= eye.y * 0.12f; break;
                    case NpcFace.Bright: browTilt = -8f; browY += eye.y * 0.12f; break;
                    case NpcFace.Shy: browTilt = -16f; break;
                    case NpcFace.Jolly: browTilt = -8f; browY += eye.y * 0.06f; break;
                    case NpcFace.Dreamy: browTilt = -12f; break;
                    default: browTilt = -5f; break;
                }

                float browThick = (r.Face == NpcFace.Stern ? 0.26f : 0.2f) * eye.y * (r.Age == NpcAge.Elder ? 1.25f : 1f);
                p = OnFace(ref rig, x + side * eye.x * 0.05f, browY, lift + 0.002f, out rot);
                b.Decal(p, new Vector2(eye.x * 1.22f, browThick), brow, rot * Quaternion.Euler(0f, 0f, browTilt * side));

                if (Cheeks(r))
                {
                    p = OnFace(ref rig, hw * 0.3f * side, -hh * 0.2f, lift, out rot);
                    b.Disc(p, new Vector2(hw * 0.15f, hh * 0.085f), 6, dark ? NpcSwatch.CheekRose : NpcSwatch.Blush, rot);
                }
            }

            // Nose: a small wedge catching the light (children get a button).
            {
                Vector3 p = OnFace(ref rig, 0f, -hh * 0.13f, -0.004f, out Quaternion rot);
                float nr = hw * (child ? 0.055f : 0.075f);
                b.Cylinder(p, nr, nr * 0.15f, rig.HeadD * (child ? 0.06f : 0.1f), 4, r.Skin, rotation: rot * Quaternion.Euler(100f, 0f, 0f), twist: 45f);
            }

            bool bearded = r.Beard == NpcBeard.Short || r.Beard == NpcBeard.Full || r.Beard == NpcBeard.Long || r.Beard == NpcBeard.LongBraided;
            NpcSwatch lip = dark ? NpcSwatch.Lip : NpcSwatch.Mouth;
            float mouthY = -hh * (bearded ? 0.27f : 0.255f);
            Vector3 mp;
            Quaternion mr;
            if (bearded)
            {
                // The mouth is an opening in the beard (in front of the face).
                BeardShape(ref rig, out Vector3 bc, out Vector3 bs);
                float dy = (rig.HeadCenter.y + mouthY) - bc.y;
                float k = 1f - (dy * dy) / (bs.y * bs.y * 0.25f);
                float z = bc.z + bs.z * 0.5f * Mathf.Sqrt(Mathf.Max(0.05f, k)) + 0.008f;
                mp = new Vector3(0f, rig.HeadCenter.y + mouthY, z);
                mr = Quaternion.identity;
                lip = NpcSwatch.Mouth;
            }
            else
            {
                mp = OnFace(ref rig, 0f, mouthY, lift, out mr);
            }

            switch (r.Face)
            {
                case NpcFace.Jolly:
                    // Wide open laugh: a dark "D" with a tongue.
                    b.Disc(mp, new Vector2(hw * 0.24f, hh * 0.13f), 6, NpcSwatch.Mouth, mr, 0f);
                    b.Disc(mp + mr * new Vector3(0f, -hh * 0.025f, 0.0015f), new Vector2(hw * 0.13f, hh * 0.05f), 5, NpcSwatch.Tongue, mr);
                    break;
                case NpcFace.Bright:
                    b.Disc(mp, new Vector2(hw * 0.14f, hh * 0.09f), 6, NpcSwatch.Mouth, mr, 0f);
                    b.Disc(mp + mr * new Vector3(0f, -hh * 0.018f, 0.0015f), new Vector2(hw * 0.08f, hh * 0.035f), 5, NpcSwatch.Tongue, mr);
                    break;
                case NpcFace.Stern:
                    b.Decal(mp, new Vector2(hw * 0.17f, hh * 0.035f), lip, mr);
                    break;
                case NpcFace.Shy:
                    b.Disc(mp, new Vector2(hw * 0.07f, hh * 0.05f), 5, lip, mr);
                    break;
                default:
                {
                    // A smile: two little strokes turned up at the ends.
                    float wsm = r.Face == NpcFace.Kind ? 0.2f : 0.16f;
                    b.Decal(mp + mr * new Vector3(-hw * wsm * 0.24f, 0f, 0f), new Vector2(hw * wsm * 0.56f, hh * 0.032f), lip, mr * Quaternion.Euler(0f, 0f, -14f));
                    b.Decal(mp + mr * new Vector3(hw * wsm * 0.24f, 0f, 0f), new Vector2(hw * wsm * 0.56f, hh * 0.032f), lip, mr * Quaternion.Euler(0f, 0f, 14f));
                    break;
                }
            }

            if (r.Snout)
            {
                // A kobold's muzzle: a short rounded snout jutting from under the eyes over the mouth, a darker nose pad with
                // two nostrils on top (it reads as a lizard-ish face from the iso camera, never as a mask).
                Vector3 sp = OnFace(ref rig, 0f, -hh * 0.2f, -0.01f, out Quaternion sr);
                b.Blob(sp + sr * new Vector3(0f, 0f, 0.05f * rig.S), new Vector3(hw * 0.5f, hh * 0.3f, 0.13f * rig.S), r.Skin);
                Vector3 pad = sp + sr * new Vector3(0f, hh * 0.04f, 0.115f * rig.S);
                b.Blob(pad, new Vector3(hw * 0.2f, hh * 0.09f, 0.04f * rig.S), NpcSwatch.Charcoal, coarse: true);
                foreach (float side in new[] { -1f, 1f })
                {
                    b.Decal(pad + sr * new Vector3(side * hw * 0.05f, hh * 0.015f, 0.021f * rig.S), new Vector2(0.014f, 0.01f) * rig.S, NpcSwatch.Lash, sr);
                }
            }

            if (r.Horns)
            {
                // The povo-touro's muzzle: broad and low over the mouth, a dark nose pad with two wide nostrils and a brass ring.
                Vector3 sp = OnFace(ref rig, 0f, -hh * 0.24f, -0.01f, out Quaternion sr);
                b.Blob(sp + sr * new Vector3(0f, 0f, 0.045f * rig.S), new Vector3(hw * 0.62f, hh * 0.34f, 0.12f * rig.S), r.Skin);
                Vector3 pad = sp + sr * new Vector3(0f, -hh * 0.02f, 0.1f * rig.S);
                b.Blob(pad, new Vector3(hw * 0.42f, hh * 0.15f, 0.045f * rig.S), NpcSwatch.Charcoal, coarse: true);
                foreach (float side in new[] { -1f, 1f })
                {
                    b.Decal(pad + sr * new Vector3(side * hw * 0.11f, 0f, 0.024f * rig.S), new Vector2(0.02f, 0.013f) * rig.S, NpcSwatch.Lash, sr);
                }

                Vector3 ring = pad + sr * new Vector3(0f, -hh * 0.08f, 0.02f * rig.S);
                const int links = 6;
                for (int i = 0; i < links; i++)
                {
                    float a0 = i * Mathf.PI * 2f / links;
                    float a1 = (i + 1) * Mathf.PI * 2f / links;
                    Vector3 q0 = ring + sr * new Vector3(Mathf.Cos(a0) * 0.028f, Mathf.Sin(a0) * 0.028f, 0f) * rig.S;
                    Vector3 q1 = ring + sr * new Vector3(Mathf.Cos(a1) * 0.028f, Mathf.Sin(a1) * 0.028f, 0f) * rig.S;
                    b.Segment(q0, q1, 0.006f * rig.S, 0.006f * rig.S, 3, NpcSwatch.Brass, caps: false);
                }
            }

            if (r.Gills)
            {
                // The povo-do-mar's gills: three thin slits on each cheek, low, towards the jaw.
                foreach (float side in new[] { -1f, 1f })
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 p = OnFace(ref rig, side * hw * 0.36f, -hh * (0.12f + i * 0.07f), 0.004f, out Quaternion gr);
                        b.Decal(p, new Vector2(0.04f, 0.007f) * rig.S, NpcSwatch.SeaBlue, gr * Quaternion.Euler(0f, 0f, side * 18f));
                    }
                }
            }

            if (r.Tusks)
            {
                // Two small ivory tusks rising from the lower lip at the corners of the mouth (they read from the iso camera).
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 root = mp + mr * new Vector3(side * hw * 0.17f, -hh * 0.04f, 0.006f);
                    b.Cylinder(root, 0.016f * rig.S, 0f, 0.055f * rig.S, 4, NpcSwatch.Ivory, rotation: mr * Quaternion.Euler(-20f, 0f, side * -12f));
                }
            }
        }

        private static void BuildHair(NpcMeshBuilder b, ref Rig rig, bool covered)
        {
            NpcRecipe r = rig.R;
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD;
            Vector3 c = rig.HeadCenter;
            NpcSwatch hair = r.HairColor;

            if (covered)
            {
                if (r.Headwear == NpcHeadwear.Hood || r.Headwear == NpcHeadwear.Kerchief)
                {
                    // A fringe peeking out on the forehead.
                    b.Blob(c + new Vector3(0f, hh * 0.35f, hd * 0.2f), new Vector3(hw * 0.74f, hh * 0.16f, hd * 0.26f), hair);
                }
            }
            else if (r.Hair == NpcHair.Bald)
            {
                if (r.Age == NpcAge.Elder)
                {
                    foreach (float side in new[] { -1f, 1f })
                    {
                        b.Blob(c + new Vector3(side * hw * 0.44f, hh * 0.02f, -hd * 0.12f), new Vector3(hw * 0.22f, hh * 0.3f, hd * 0.5f), hair, coarse: true);
                    }
                }
            }
            else
            {
                float grow = r.Hair == NpcHair.Curly ? 1.16f : 1.08f;
                b.Dome(c + new Vector3(0f, hh * 0.05f, -hd * 0.05f), new Vector3(hw * grow, hh * (grow - 0.02f), hd * (grow + 0.02f)), hair, cutY: 0.08f, backCutZ: 0.22f);
            }

            switch (r.Hair)
            {
                case NpcHair.Long:
                    b.Box(c + new Vector3(0f, -hh * 0.32f, -hd * 0.38f), new Vector3(hw * 0.92f, hh * 0.8f, hd * 0.2f), hair, Quaternion.Euler(-8f, 0f, 0f));
                    break;
                case NpcHair.Bun:
                    if (!covered)
                    {
                        b.Blob(c + new Vector3(0f, hh * 0.4f, -hd * 0.36f), Vector3.one * hh * 0.34f, hair);
                    }

                    break;
                case NpcHair.Topknot:
                    if (!covered)
                    {
                        b.Blob(c + new Vector3(0f, hh * 0.58f, -hd * 0.05f), new Vector3(hh * 0.24f, hh * 0.22f, hh * 0.24f), hair, coarse: true);
                    }

                    break;
                case NpcHair.Ponytail:
                    Chain(b, c + new Vector3(0f, hh * 0.18f, -hd * 0.52f), c + new Vector3(0f, -hh * 0.6f, -hd * 0.62f), 3, hh * 0.18f, hair, NpcSwatch.Brass);
                    break;
                case NpcHair.Braid:
                    Chain(b, c + new Vector3(hw * 0.4f, -hh * 0.18f, hd * 0.06f), c + new Vector3(hw * 0.5f, -hh * 0.82f, hd * 0.3f), 4, hh * 0.15f, hair, NpcSwatch.Brass);
                    break;
                case NpcHair.TwoBraids:
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Chain(b, c + new Vector3(side * hw * 0.4f, -hh * 0.18f, -hd * 0.12f), c + new Vector3(side * hw * 0.48f, -hh * 0.8f, -hd * 0.32f), 3, hh * 0.15f, hair, NpcSwatch.Pomegranate);
                    }

                    break;
                case NpcHair.Curly:
                    if (!covered)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            float a = (i / 4f) * Mathf.PI * 2f + 0.4f;
                            b.Blob(c + new Vector3(Mathf.Cos(a) * hw * 0.45f, hh * 0.3f, Mathf.Sin(a) * hd * 0.3f - hd * 0.12f), Vector3.one * hh * 0.3f, hair, coarse: true);
                        }
                    }

                    break;
                case NpcHair.Spiky:
                    if (!covered)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            float a = -60f + i * 30f;
                            Quaternion rot = Quaternion.Euler(-25f, 0f, a);
                            b.Cylinder(c + rot * new Vector3(0f, hh * 0.42f, 0f) + new Vector3(0f, 0f, -hd * 0.08f), hh * 0.09f, 0f, hh * 0.2f, 4, hair, rotation: rot);
                        }
                    }

                    break;
            }
        }

        /// <summary>A braid or ponytail: <paramref name="count"/> blobs from <paramref name="from"/> to <paramref name="to"/>, tied at the end.</summary>
        private static void Chain(NpcMeshBuilder b, Vector3 from, Vector3 to, int count, float size, NpcSwatch swatch, NpcSwatch tie)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0f : i / (float)(count - 1);
                float k = 1f - t * 0.3f;
                b.Blob(Vector3.Lerp(from, to, t), new Vector3(size, size * 1.25f, size) * k, swatch, coarse: true);
            }

            b.Cylinder(to + Vector3.down * size * 0.5f, size * 0.32f, size * 0.32f, size * 0.25f, 5, tie);
        }

        private static void BeardShape(ref Rig rig, out Vector3 center, out Vector3 size)
        {
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD;
            Vector3 c = rig.HeadCenter;
            if (rig.R.Beard == NpcBeard.Short)
            {
                center = c + new Vector3(0f, -hh * 0.3f, hd * 0.2f);
                size = new Vector3(hw * 0.86f, hh * 0.48f, hd * 0.62f);
            }
            else
            {
                center = c + new Vector3(0f, -hh * 0.38f, hd * 0.24f);
                size = new Vector3(hw * 1.02f, hh * 0.7f, hd * 0.78f);
            }
        }

        private static void BuildBeard(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD;
            Vector3 c = rig.HeadCenter;
            NpcSwatch hair = r.HairColor;
            switch (r.Beard)
            {
                case NpcBeard.None:
                    return;
                case NpcBeard.Moustache:
                {
                    Vector3 p = OnFace(ref rig, 0f, -hh * 0.17f, 0.01f, out Quaternion rot);
                    b.Box(p, new Vector3(hw * 0.36f, hh * 0.065f, 0.035f), hair, rot * Quaternion.Euler(0f, 0f, 0f));
                    return;
                }
                case NpcBeard.Goatee:
                {
                    Vector3 p = OnFace(ref rig, 0f, -hh * 0.36f, -0.005f, out Quaternion rot);
                    b.Cylinder(p, hw * 0.12f, 0f, hh * 0.22f, 5, hair, rotation: Quaternion.Euler(180f - 20f, 0f, 0f));
                    Vector3 m = OnFace(ref rig, 0f, -hh * 0.17f, 0.008f, out rot);
                    b.Box(m, new Vector3(hw * 0.3f, hh * 0.05f, 0.03f), hair, rot);
                    return;
                }
            }

            BeardShape(ref rig, out Vector3 bc, out Vector3 bs);
            b.Blob(bc, bs, hair);
            if (r.Beard != NpcBeard.Short)
            {
                // Sideburns joining the beard to the hair: the beard frames the face instead of floating under it.
                foreach (float side in new[] { -1f, 1f })
                {
                    b.Blob(c + new Vector3(side * hw * 0.4f, -hh * 0.14f, hd * 0.04f), new Vector3(hw * 0.2f, hh * 0.42f, hd * 0.42f), hair, coarse: true);
                }
            }

            Vector3 must = OnFace(ref rig, 0f, -hh * 0.18f, 0.012f, out Quaternion mr);
            b.Box(must + new Vector3(0f, 0f, 0.02f), new Vector3(hw * 0.46f, hh * 0.08f, 0.045f), hair, mr * Quaternion.Euler(0f, 0f, 0f));
            if (r.Beard == NpcBeard.Long)
            {
                // Two tiers: a wide bib and a long point down the chest.
                b.Cylinder(bc + new Vector3(0f, -bs.y * 0.22f, bs.z * 0.1f), hw * 0.44f, hw * 0.3f, hh * 0.3f, 7, hair, rotation: Quaternion.Euler(170f, 0f, 0f), topCap: false);
                b.Cylinder(bc + new Vector3(0f, -bs.y * 0.22f - hh * 0.28f, bs.z * 0.14f), hw * 0.3f, 0f, hh * 0.5f, 6, hair, rotation: Quaternion.Euler(168f, 0f, 0f));
            }
            else if (r.Beard == NpcBeard.LongBraided)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    Chain(b, bc + new Vector3(side * hw * 0.16f, -bs.y * 0.36f, bs.z * 0.25f), bc + new Vector3(side * hw * 0.14f, -bs.y * 1.05f, bs.z * 0.32f), 3, hh * 0.14f, hair, NpcSwatch.Brass);
                }
            }
        }

        private static void BuildHeadwear(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            float hw = rig.HeadW, hh = rig.HeadH, hd = rig.HeadD, s = rig.S;
            Vector3 c = rig.HeadCenter;
            NpcSwatch col = r.HeadColor, trim = r.HeadTrim;

            switch (r.Headwear)
            {
                case NpcHeadwear.Kerchief:
                {
                    b.Dome(c + new Vector3(0f, hh * 0.07f, -hd * 0.16f), new Vector3(hw * 1.18f, hh * 1.14f, hd * 1.18f), col, cutY: -0.35f, backCutZ: 0.1f,
                        openFrontZ: 0.28f, openTopY: 0.42f);
                    // Tail of the knot hanging down the back and the knot under the chin.
                    b.TwoSided(c + new Vector3(-hw * 0.34f, -hh * 0.1f, -hd * 0.5f), c + new Vector3(hw * 0.34f, -hh * 0.1f, -hd * 0.5f), c + new Vector3(0f, -hh * 0.75f, -hd * 0.62f), col, trim);
                    b.Blob(c + new Vector3(0f, -hh * 0.5f, hd * 0.2f), new Vector3(hw * 0.2f, hh * 0.14f, hd * 0.16f), col, coarse: true);
                    for (int i = 0; i < 3; i++)
                    {
                        float a = -50f + i * 50f;
                        Vector3 dir = Quaternion.Euler(-35f, a, 0f) * Vector3.forward;
                        Vector3 p = c + new Vector3(0f, hh * 0.06f, -hd * 0.14f) + Vector3.Scale(dir, new Vector3(hw, hh, hd) * 0.59f);
                        b.Decal(p, Vector2.one * hh * 0.12f, trim, Quaternion.LookRotation(dir));
                    }

                    break;
                }
                case NpcHeadwear.Hood:
                    // Pulled back off the face: the opening keeps the eyes and brows in the light.
                    b.Dome(c + new Vector3(0f, hh * 0.06f, -hd * 0.24f), new Vector3(hw * 1.26f, hh * 1.22f, hd * 1.3f), col, cutY: -0.55f, backCutZ: 0f,
                        openFrontZ: 0.22f, openTopY: 0.48f);
                    b.Bone = NpcBones.Body;
                    b.Matrix = rig.BodyM;
                    b.Cylinder(new Vector3(0f, rig.NeckY - 0.14f * s, -0.01f), rig.Rw * rig.ShoulderMul * 1.15f, hw * 0.5f, 0.2f * s, 8, col, topCap: false, depth: 0.9f);
                    b.Bone = NpcBones.Head;
                    b.Matrix = rig.HeadM;
                    break;
                case NpcHeadwear.Turban:
                {
                    Vector3 basePos = c + new Vector3(0f, hh * 0.16f, -hd * 0.04f);
                    for (int i = 0; i < 3; i++)
                    {
                        float rr = hw * (0.62f - i * 0.045f);
                        b.Cylinder(basePos + Vector3.up * (i * hh * 0.1f), rr, rr * 0.95f, hh * 0.16f, 8, i == 1 ? trim : col, col, rotation: Quaternion.Euler(i % 2 == 0 ? 3f : -3f, 0f, i == 1 ? 2f : -2f), topCap: false, depth: 0.96f);
                    }

                    // The crown, a soft dome closing the top turn.
                    b.Dome(basePos + Vector3.up * (hh * 0.26f), new Vector3(hw * 1.1f, hh * 0.56f, hd * 1.04f), col, cutY: -0.2f);

                    Vector3 front = OnFace(ref rig, 0f, hh * 0.27f, hd * 0.12f, out Quaternion rot);
                    b.Box(front, new Vector3(0.05f, 0.06f, 0.02f) * s, NpcSwatch.Brass, rot);
                    break;
                }
                case NpcHeadwear.Headwrap:
                {
                    Vector3 basePos = c + new Vector3(0f, hh * 0.14f, -hd * 0.03f);
                    b.Cylinder(basePos, hw * 0.58f, hw * 0.55f, hh * 0.16f, 8, col, rotation: Quaternion.Euler(8f, 0f, 0f), topCap: false, depth: 0.97f);
                    b.Dome(c + new Vector3(0f, hh * 0.12f, -hd * 0.04f), new Vector3(hw * 1.08f, hh * 1.02f, hd * 1.08f), col, cutY: 0.15f, backCutZ: 0.3f);
                    b.TwoSided(c + new Vector3(-hw * 0.22f, hh * 0.1f, -hd * 0.52f), c + new Vector3(hw * 0.22f, hh * 0.1f, -hd * 0.52f), c + new Vector3(hw * 0.05f, -hh * 0.9f, -hd * 0.7f), col, trim);
                    break;
                }
                case NpcHeadwear.FeltCap:
                {
                    float y0 = c.y + hh * 0.26f;
                    // Tall kalpak: a clear tapering tower with the split brim turned up front and back.
                    b.Cylinder(new Vector3(0f, y0, c.z - hd * 0.03f), hw * 0.53f, hw * 0.3f, hh * 0.82f, 8, col, col, depth: 0.96f);
                    foreach (float side in new[] { 1f, -1f })
                    {
                        b.Box(new Vector3(0f, y0 + hh * 0.08f, c.z + side * hd * 0.5f), new Vector3(hw * 0.74f, hh * 0.2f, 0.03f), trim, Quaternion.Euler(side * -18f, 0f, 0f));
                    }

                    break;
                }
                case NpcHeadwear.Skullcap:
                    b.Cylinder(new Vector3(0f, c.y + hh * 0.25f, c.z - hd * 0.03f), hw * 0.57f, hw * 0.5f, hh * 0.27f, 4, col, trim, twist: 45f);
                    break;
                case NpcHeadwear.Bandana:
                    b.Dome(c + new Vector3(0f, hh * 0.08f, -hd * 0.02f), new Vector3(hw * 1.07f, hh * 1.02f, hd * 1.08f), col, cutY: 0.25f, backCutZ: 0.45f);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        b.Box(c + new Vector3(side * hw * 0.1f, hh * 0.08f, -hd * 0.6f), new Vector3(0.05f, 0.14f, 0.02f) * s, col, Quaternion.Euler(-30f, 0f, side * 35f));
                    }

                    break;
                case NpcHeadwear.StrawCone:
                    // Mokorotlo: a wide woven cone, a band and the knob on top — unmistakable from the iso camera.
                    b.Cylinder(new Vector3(0f, c.y + hh * 0.18f, c.z), hw * 1.05f, 0f, hh * 0.78f, 10, col, bottomCap: true);
                    b.Cylinder(new Vector3(0f, c.y + hh * 0.3f, c.z), hw * 0.88f, hw * 0.76f, hh * 0.09f, 10, trim, topCap: false);
                    b.Blob(new Vector3(0f, c.y + hh * 0.98f, c.z), Vector3.one * hh * 0.16f, col, coarse: true);
                    break;
                case NpcHeadwear.LampHelmet:
                {
                    b.Dome(c + new Vector3(0f, hh * 0.1f, -hd * 0.02f), new Vector3(hw * 1.12f, hh * 1.04f, hd * 1.12f), col, cutY: 0.1f, backCutZ: 0.3f);
                    b.Cylinder(c + new Vector3(0f, hh * 0.1f, -hd * 0.02f), hw * 0.62f, hw * 0.6f, hh * 0.04f, 10, NpcSwatch.LeatherDark, topCap: true, depth: 1.04f);
                    b.Box(c + new Vector3(0f, hh * 0.13f, hd * 0.5f), new Vector3(hw * 0.7f, hh * 0.035f, hd * 0.22f), NpcSwatch.LeatherDark, Quaternion.Euler(-10f, 0f, 0f), noBottom: false);
                    Vector3 lamp = OnFace(ref rig, 0f, hh * 0.3f, 0.01f, out Quaternion rot);
                    b.Cylinder(lamp, 0.05f * s, 0.045f * s, 0.06f * s, 6, NpcSwatch.Copper, trim, rotation: rot * Quaternion.Euler(90f, 0f, 0f));
                    break;
                }
                case NpcHeadwear.Headband:
                case NpcHeadwear.FlowerCrown:
                case NpcHeadwear.SunCirclet:
                {
                    Vector3 basePos = c + new Vector3(0f, hh * 0.17f, -hd * 0.02f);
                    b.Cylinder(basePos, hw * 0.53f, hw * 0.51f, hh * 0.06f, 8, col, topCap: false, depth: 0.98f, rotation: Quaternion.Euler(6f, 0f, 0f));
                    if (r.Headwear == NpcHeadwear.FlowerCrown)
                    {
                        NpcSwatch[] flowers = { NpcSwatch.FlowerPink, NpcSwatch.FlowerYellow, NpcSwatch.HerbGreen, NpcSwatch.FlowerPink, NpcSwatch.FlowerYellow, NpcSwatch.LimeWhite };
                        for (int i = 0; i < flowers.Length; i++)
                        {
                            float a = (i / (float)flowers.Length) * Mathf.PI * 2f + Mathf.PI * 0.5f;
                            b.Blob(basePos + new Vector3(Mathf.Cos(a) * hw * 0.53f, hh * 0.04f, Mathf.Sin(a) * hd * 0.53f), Vector3.one * hh * 0.12f, flowers[i], coarse: true);
                        }
                    }
                    else if (r.Headwear == NpcHeadwear.SunCirclet)
                    {
                        Vector3 p = OnFace(ref rig, 0f, hh * 0.24f, 0.02f, out Quaternion rot);
                        b.Cylinder(p, hh * 0.08f, hh * 0.08f, 0.015f, 8, NpcSwatch.SunGlow, rotation: rot * Quaternion.Euler(90f, 0f, 0f));
                    }

                    break;
                }
            }
        }
    }
}
