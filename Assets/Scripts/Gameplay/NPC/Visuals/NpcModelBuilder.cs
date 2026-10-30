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
    }

    /// <summary>
    /// Builds an NPC figure from a <see cref="NpcRecipe"/>: chibi proportions like the cousins (a big head, about four and a
    /// half heads tall), a parametric body (age, build, posture, height), a head with a drawn face, hair, beard and
    /// headwear, the clothing silhouette and layers, two bent arms in a baked pose and the role props. Feet at y = 0,
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
            public Vector3 ShoulderL, ShoulderR, HandL, HandR, ForeL, ForeR; // posed model space
            public Vector3 ElbowL, ElbowR;

            /// <summary>Front surface z of the torso at height y (body rest space).</summary>
            public float TorsoFrontZ(float y)
            {
                float t = Mathf.InverseLerp(HipY, NeckY, y);
                return Mathf.Lerp(Rw * 0.92f * Belly, Rw * ShoulderMul, t) * 0.78f;
            }

            /// <summary>Front surface z of the skirt at height y (root space).</summary>
            public float SkirtFrontZ(float y)
            {
                float t = Mathf.InverseLerp(SkirtY0, WaistY, y);
                return Mathf.Lerp(SkirtR0, Rw * 0.98f * Belly, t) * 0.84f;
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

            rig.H = baseH * Mathf.Clamp(r.Height, 0.75f, 1.25f);
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
                case NpcBuild.Broad: buildW = 1.12f; break;
                default: buildW = 1f; break;
            }

            rig.Rw = 0.19f * Mathf.Lerp(1f, rig.S, 0.7f) * buildW;
            rig.Belly = r.Build == NpcBuild.Stout ? 1.16f : 1f;
            rig.ShoulderMul = r.Build == NpcBuild.Broad ? 1.2f : r.Build == NpcBuild.Slim ? 0.95f : 1f;
            rig.ShoulderY = rig.NeckY - 0.05f * rig.S;
            rig.ShoulderX = rig.Rw * rig.ShoulderMul + 0.03f * rig.Limb;

            switch (r.Garment)
            {
                case NpcGarment.Robe:
                case NpcGarment.Kaftan:
                    rig.SkirtY0 = 0.05f; rig.SkirtR0 = rig.Rw * 1.3f * rig.Belly; break;
                case NpcGarment.Dress:
                    rig.SkirtY0 = 0.04f; rig.SkirtR0 = rig.Rw * 1.5f * rig.Belly; break;
                case NpcGarment.Chapan:
                    rig.SkirtY0 = rig.HipY * 0.42f; rig.SkirtR0 = rig.Rw * 1.26f * rig.Belly; break;
                default:
                    rig.SkirtY0 = rig.HipY * 0.78f; rig.SkirtR0 = rig.Rw * 1.12f * rig.Belly; break;
            }

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

        /// <summary>Upper/forearm pitch (negative = forward/up), roll outward, in degrees.</summary>
        private static void PoseAngles(NpcArmPose pose, out float upperPitch, out float upperRoll, out float forePitch, out float foreRoll)
        {
            switch (pose)
            {
                case NpcArmPose.HoldFront: upperPitch = -30f; upperRoll = -8f; forePitch = -85f; foreRoll = -40f; break;
                case NpcArmPose.HoldStaff: upperPitch = -26f; upperRoll = 16f; forePitch = -62f; foreRoll = 6f; break;
                case NpcArmPose.HipRest: upperPitch = 12f; upperRoll = 40f; forePitch = 20f; foreRoll = -55f; break;
                case NpcArmPose.Carry: upperPitch = -150f; upperRoll = 6f; forePitch = 110f; foreRoll = 0f; break;
                case NpcArmPose.HoldUp: upperPitch = -60f; upperRoll = 6f; forePitch = -150f; foreRoll = -10f; break;
                case NpcArmPose.Crook: upperPitch = -6f; upperRoll = 10f; forePitch = -88f; foreRoll = -22f; break;
                default: upperPitch = -4f; upperRoll = 8f; forePitch = -14f; foreRoll = 5f; break;
            }
        }

        private static Vector3 ArmDirection(float pitch, float rollOut, bool right)
        {
            float roll = right ? rollOut : -rollOut;
            return Quaternion.Euler(pitch, 0f, roll) * Vector3.down;
        }

        private static void PoseArm(ref Rig rig, bool right)
        {
            NpcArmPose pose = right ? rig.R.RightArm : rig.R.LeftArm;
            PoseAngles(pose, out float up, out float ur, out float fp, out float fr);
            float upperLen = (rig.NeckY - rig.HipY) * 0.5f;
            float foreLen = upperLen * 0.95f;
            var shoulder = new Vector3(right ? rig.ShoulderX : -rig.ShoulderX, rig.ShoulderY, 0f);
            Vector3 elbow = shoulder + ArmDirection(up, ur, right) * upperLen;
            Vector3 foreDir = ArmDirection(fp, fr, right);
            Vector3 hand = elbow + foreDir * foreLen;

            Vector3 s = rig.BodyM.MultiplyPoint3x4(shoulder);
            Vector3 e = rig.BodyM.MultiplyPoint3x4(elbow);
            Vector3 h = rig.BodyM.MultiplyPoint3x4(hand);
            Vector3 f = rig.BodyM.MultiplyVector(foreDir).normalized;
            if (right)
            {
                rig.ShoulderR = s; rig.ElbowR = e; rig.HandR = h; rig.ForeR = f;
            }
            else
            {
                rig.ShoulderL = s; rig.ElbowL = e; rig.HandL = h; rig.ForeL = f;
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

            foreach (float side in new[] { -1f, 1f })
            {
                float x = lx * side;
                if (!longSkirt)
                {
                    b.Segment(new Vector3(x, rig.SkirtY0 + 0.04f, 0f), new Vector3(x, 0.13f, 0.005f), 0.078f * L, 0.07f * L, 6, r.Trousers, caps: false);
                }

                b.Cylinder(new Vector3(x, 0f, 0.005f), 0.082f * L, 0.074f * L, 0.16f * L, 6, r.Boots, r.Boots);
                float toeZ = longSkirt ? Mathf.Max(0.08f * L, rig.SkirtR0 * 0.84f - 0.03f) : 0.08f * L;
                b.Box(new Vector3(x, 0.045f * L, toeZ), new Vector3(0.12f, 0.09f, 0.15f) * L, r.Boots, noBottom: true);
            }

            // The skirt / lower garment (root bone: it stays put while the chest breathes).
            float topR = rig.Rw * 0.98f * rig.Belly;
            b.Cylinder(new Vector3(0f, rig.SkirtY0, 0f), rig.SkirtR0, topR, rig.WaistY - rig.SkirtY0, 8, r.Main, topCap: false, depth: 0.84f);

            if (r.Garment == NpcGarment.Kaftan || r.Garment == NpcGarment.Dress || r.Garment == NpcGarment.Chapan)
            {
                float hemH = 0.07f * rig.S;
                float rAt = Mathf.Lerp(rig.SkirtR0, topR, hemH / (rig.WaistY - rig.SkirtY0));
                b.Cylinder(new Vector3(0f, rig.SkirtY0 - 0.005f, 0f), rig.SkirtR0 * 1.035f, rAt * 1.035f, hemH, 8, r.Trim, topCap: false, depth: 0.84f);
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

        private static void BuildArm(NpcMeshBuilder b, ref Rig rig, bool right)
        {
            NpcRecipe r = rig.R;
            b.Bone = right ? NpcBones.ArmR : NpcBones.ArmL;
            b.Matrix = Matrix4x4.identity;
            Vector3 s = right ? rig.ShoulderR : rig.ShoulderL;
            Vector3 e = right ? rig.ElbowR : rig.ElbowL;
            Vector3 h = right ? rig.HandR : rig.HandL;
            Vector3 f = right ? rig.ForeR : rig.ForeL;
            float L = rig.Limb;
            b.Blob(s, new Vector3(0.14f, 0.14f, 0.13f) * L, r.Sleeves, coarse: true);
            b.Segment(s, e, 0.062f * L, 0.054f * L, 6, r.Sleeves);
            b.Segment(e, h, 0.056f * L, 0.05f * L, 6, r.Sleeves);
            b.Blob(h + f * 0.035f * L, new Vector3(0.1f, 0.1f, 0.1f) * L, r.Hands ?? r.Skin, coarse: true);
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

            if (!coversEars && r.Hair != NpcHair.Long && r.Hair != NpcHair.Curly)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    b.Blob(hc + new Vector3(side * hw * 0.49f, -hh * 0.05f, -hd * 0.02f), new Vector3(0.05f, 0.085f, 0.06f) * rig.S, r.Skin, coarse: true);
                }
            }

            BuildHair(b, ref rig, covers);
            BuildBeard(b, ref rig);
            BuildHeadwear(b, ref rig);
        }

        private static void BuildFace(NpcMeshBuilder b, ref Rig rig)
        {
            NpcRecipe r = rig.R;
            float hw = rig.HeadW, hh = rig.HeadH;
            bool child = r.Age == NpcAge.Child;
            float eyeScale = child ? 1.3f : r.Age == NpcAge.Youth ? 1.12f : 1f;
            if (r.Face == NpcFace.Bright)
            {
                eyeScale *= 1.12f;
            }

            var eye = new Vector2(hw * 0.085f, hh * 0.12f) * eyeScale;
            float eyeY = -hh * 0.04f;
            float eyeX = hw * 0.2f;
            const float lift = 0.004f;

            foreach (float side in new[] { -1f, 1f })
            {
                float x = eyeX * side;
                Quaternion rot;
                Vector3 p;
                switch (r.Face)
                {
                    case NpcFace.Jolly:
                        // Closed, laughing eyes: a little "^".
                        p = OnFace(ref rig, x - eye.x * 0.22f, eyeY, lift, out rot);
                        b.Decal(p, new Vector2(eye.x * 0.75f, eye.y * 0.22f), NpcSwatch.Eye, rot * Quaternion.Euler(0f, 0f, 28f));
                        p = OnFace(ref rig, x + eye.x * 0.22f, eyeY, lift, out rot);
                        b.Decal(p, new Vector2(eye.x * 0.75f, eye.y * 0.22f), NpcSwatch.Eye, rot * Quaternion.Euler(0f, 0f, -28f));
                        break;
                    case NpcFace.Dreamy:
                    case NpcFace.Calm:
                        p = OnFace(ref rig, x, eyeY - eye.y * 0.1f, lift, out rot);
                        b.Decal(p, new Vector2(eye.x * 1.1f, eye.y * (r.Face == NpcFace.Dreamy ? 0.32f : 0.5f)), NpcSwatch.Eye, rot);
                        break;
                    case NpcFace.Shy:
                        p = OnFace(ref rig, x, eyeY - eye.y * 0.15f, lift, out rot);
                        b.Decal(p, new Vector2(eye.x, eye.y * 0.7f), NpcSwatch.Eye, rot);
                        break;
                    default:
                        p = OnFace(ref rig, x, eyeY, lift, out rot);
                        b.Decal(p, eye, NpcSwatch.Eye, rot);
                        b.Decal(p + rot * new Vector3(eye.x * 0.18f * side, eye.y * 0.22f, 0.002f), eye * 0.34f, NpcSwatch.EyeShine, rot);
                        break;
                }

                if (r.Face == NpcFace.Stern || r.Face == NpcFace.Bright || r.Age == NpcAge.Elder)
                {
                    float browTilt = r.Face == NpcFace.Stern ? -16f : r.Face == NpcFace.Bright ? 10f : 4f;
                    p = OnFace(ref rig, x, eyeY + eye.y * (r.Face == NpcFace.Bright ? 1.05f : 0.82f), lift + 0.002f, out rot);
                    NpcSwatch brow = r.HairColor == NpcSwatch.HairWhite ? NpcSwatch.HairGrey : r.HairColor;
                    b.Decal(p, new Vector2(eye.x * 1.5f, eye.y * (r.Face == NpcFace.Stern ? 0.3f : 0.2f)), brow, rot * Quaternion.Euler(0f, 0f, browTilt * side));
                }

                if (child || r.Face == NpcFace.Jolly || r.Face == NpcFace.Shy)
                {
                    p = OnFace(ref rig, hw * 0.3f * side, -hh * 0.16f, lift, out rot);
                    b.Decal(p, new Vector2(hw * 0.11f, hh * 0.05f), NpcSwatch.Blush, rot);
                }
            }

            if (r.Age != NpcAge.Child)
            {
                Vector3 p = OnFace(ref rig, 0f, -hh * 0.1f, 0f, out Quaternion rot);
                b.Box(p, new Vector3(hw * 0.09f, hh * 0.1f, rig.HeadD * 0.1f), r.Skin, rot);
            }

            bool bearded = r.Beard == NpcBeard.Short || r.Beard == NpcBeard.Full || r.Beard == NpcBeard.Long || r.Beard == NpcBeard.LongBraided;
            Vector2 mouth;
            switch (r.Face)
            {
                case NpcFace.Jolly: mouth = new Vector2(hw * 0.2f, hh * 0.085f); break;
                case NpcFace.Bright: mouth = new Vector2(hw * 0.1f, hh * 0.06f); break;
                case NpcFace.Stern: mouth = new Vector2(hw * 0.13f, hh * 0.022f); break;
                case NpcFace.Shy: mouth = new Vector2(hw * 0.07f, hh * 0.022f); break;
                default: mouth = new Vector2(hw * 0.14f, hh * 0.028f); break;
            }

            float mouthY = -hh * (bearded ? 0.25f : 0.24f);
            if (bearded)
            {
                // The mouth is an opening in the beard (in front of the face).
                BeardShape(ref rig, out Vector3 bc, out Vector3 bs);
                float dy = (rig.HeadCenter.y + mouthY) - bc.y;
                float k = 1f - (dy * dy) / (bs.y * bs.y * 0.25f);
                float z = bc.z + bs.z * 0.5f * Mathf.Sqrt(Mathf.Max(0.05f, k)) + 0.006f;
                b.Decal(new Vector3(0f, rig.HeadCenter.y + mouthY, z), mouth, NpcSwatch.Mouth);
            }
            else
            {
                Vector3 p = OnFace(ref rig, 0f, mouthY, lift, out Quaternion rot);
                b.Decal(p, mouth, NpcSwatch.Mouth, rot);
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
                    b.Blob(c + new Vector3(0f, hh * 0.3f, hd * 0.3f), new Vector3(hw * 0.72f, hh * 0.2f, hd * 0.3f), hair, coarse: true);
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
                center = c + new Vector3(0f, -hh * 0.35f, hd * 0.2f);
                size = new Vector3(hw * 0.95f, hh * 0.62f, hd * 0.7f);
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
            Vector3 must = OnFace(ref rig, 0f, -hh * 0.17f, 0.012f, out Quaternion mr);
            b.Box(must + new Vector3(0f, 0f, 0.02f), new Vector3(hw * 0.4f, hh * 0.07f, 0.04f), hair, mr);
            if (r.Beard == NpcBeard.Long)
            {
                b.Cylinder(bc + new Vector3(0f, -bs.y * 0.3f, bs.z * 0.08f), hw * 0.34f, 0f, hh * 0.55f, 6, hair, rotation: Quaternion.Euler(172f, 0f, 0f));
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
                    b.Dome(c + new Vector3(0f, hh * 0.06f, -hd * 0.14f), new Vector3(hw * 1.16f, hh * 1.12f, hd * 1.16f), col, cutY: -0.35f, backCutZ: 0.1f);
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
                    b.Dome(c + new Vector3(0f, hh * 0.04f, -hd * 0.17f), new Vector3(hw * 1.24f, hh * 1.2f, hd * 1.24f), col, cutY: -0.55f, backCutZ: 0f);
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
                        float rr = hw * (0.6f - i * 0.06f);
                        b.Cylinder(basePos + Vector3.up * (i * hh * 0.11f), rr, rr * 0.94f, hh * 0.13f, 8, i == 1 ? trim : col, col, rotation: Quaternion.Euler(i % 2 == 0 ? 6f : -5f, 0f, i == 1 ? 4f : -3f), topCap: i == 2, depth: 0.96f);
                    }

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
                    b.Cylinder(new Vector3(0f, y0, c.z - hd * 0.03f), hw * 0.52f, hw * 0.36f, hh * 0.62f, 8, col, col, depth: 0.96f);
                    foreach (float side in new[] { 1f, -1f })
                    {
                        b.Box(new Vector3(0f, y0 + hh * 0.06f, c.z + side * hd * 0.47f), new Vector3(hw * 0.6f, hh * 0.16f, 0.025f), trim, Quaternion.Euler(side * -14f, 0f, 0f));
                    }

                    break;
                }
                case NpcHeadwear.Skullcap:
                    b.Cylinder(new Vector3(0f, c.y + hh * 0.26f, c.z - hd * 0.03f), hw * 0.55f, hw * 0.5f, hh * 0.2f, 4, col, trim, twist: 45f);
                    break;
                case NpcHeadwear.Bandana:
                    b.Dome(c + new Vector3(0f, hh * 0.08f, -hd * 0.02f), new Vector3(hw * 1.07f, hh * 1.02f, hd * 1.08f), col, cutY: 0.25f, backCutZ: 0.45f);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        b.Box(c + new Vector3(side * hw * 0.1f, hh * 0.08f, -hd * 0.6f), new Vector3(0.05f, 0.14f, 0.02f) * s, col, Quaternion.Euler(-30f, 0f, side * 35f));
                    }

                    break;
                case NpcHeadwear.StrawCone:
                    b.Cylinder(new Vector3(0f, c.y + hh * 0.22f, c.z), hw * 0.8f, 0f, hh * 0.62f, 10, col, bottomCap: true);
                    b.Cylinder(new Vector3(0f, c.y + hh * 0.3f, c.z), hw * 0.62f, hw * 0.55f, hh * 0.07f, 10, trim, topCap: false);
                    b.Blob(new Vector3(0f, c.y + hh * 0.86f, c.z), Vector3.one * hh * 0.14f, col, coarse: true);
                    break;
                case NpcHeadwear.LampHelmet:
                {
                    b.Dome(c + new Vector3(0f, hh * 0.1f, -hd * 0.02f), new Vector3(hw * 1.12f, hh * 1.04f, hd * 1.12f), col, cutY: 0.1f, backCutZ: 0.3f);
                    b.Cylinder(c + new Vector3(0f, hh * 0.1f, -hd * 0.02f), hw * 0.62f, hw * 0.6f, hh * 0.04f, 10, NpcSwatch.LeatherDark, topCap: true, depth: 1.04f);
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
