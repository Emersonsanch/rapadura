using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Lobisomem-do-sertão (Floresta, night mini-boss), about 1.5 m to the ear tips on all fours and 2 m long: a
    /// big, scruffy brown-grey wolf-man, more goofy than scary — one ear flopped over, a pink tongue lolling out
    /// of the side of an under-bite, tufts of fur sticking out everywhere, oversized clawed paws, glowing red eyes
    /// under a heavy brow and a pale chest bib that catches the lantern light (its silhouette in the dark is the
    /// huge maned hump of the shoulders).
    /// <para>
    /// Runs on all fours with the stepping legs and rears up on its hind legs to strike: the chest, head and
    /// forelegs hang from <c>Torso</c>, whose pivot sits at the hips, and <see cref="ProceduralEnemyRig"/> pitches
    /// it up during the attack windup while releasing the forelegs from the ground
    /// (<c>ProceduralLegLocomotion.FrontLegsWeight</c>). Hierarchy:
    /// <c>Body</c> (hips) → <c>Tail</c> → <c>Tail_1</c>; <c>Body</c> → <c>Torso</c> → <c>Head</c> → <c>Ear_L</c>/<c>Ear_R</c>/<c>Jaw</c>;
    /// <c>Torso</c> → <c>Shoulder_L</c>/<c>Shoulder_R</c> → <c>Leg_FL</c>/<c>Leg_FR</c> → <c>Shin_</c> → <c>Foot_</c>;
    /// hind legs <c>Leg_HL</c>/<c>Leg_HR</c> → <c>Shin_</c> → <c>Foot_</c> under the root (long digitigrade hocks).
    /// </para>
    /// </summary>
    public static class WerewolfRecipe
    {
        public const string ShoulderLeft = "Shoulder_L";
        public const string ShoulderRight = "Shoulder_R";

        private const EnemySwatch Fur = EnemySwatch.WolfFur;
        private const EnemySwatch Dark = EnemySwatch.WolfFurDark;
        private const EnemySwatch Belly = EnemySwatch.WolfBelly;
        private const EnemySwatch Eye = EnemySwatch.WarEye;

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.95f, -0.35f), DrawHips);
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.3f), DrawTailBase, Quaternion.Euler(-15f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.1f, -0.26f), DrawTailTip);

            int torso = bp.Mesh(ProceduralEnemyParts.Torso, body, Vector3.zero, DrawTorso);
            int head = bp.Mesh(ProceduralEnemyParts.Head, torso, new Vector3(0f, 0.3f, 0.98f), DrawHead, Quaternion.Euler(12f, 0f, 0f));
            // One ear up, one flopped forward: the goofy half of "goofy-scary".
            bp.Mesh(ProceduralEnemyParts.EarLeft, head, new Vector3(-0.11f, 0.15f, 0.0f), DrawEar, Quaternion.Euler(-8f, 0f, 18f), meshKey: "Ear");
            bp.Mesh(ProceduralEnemyParts.EarRight, head, new Vector3(0.11f, 0.15f, 0.0f), DrawEar, Quaternion.Euler(62f, 0f, -30f), meshKey: "Ear");
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), DrawJaw);

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";

                // Forelegs (arms) hang from the shoulders on the torso: shoulder → elbow (behind) → wrist → big paw.
                int shoulder = bp.Node(side < 0 ? ShoulderLeft : ShoulderRight, torso, new Vector3(0.21f * side, -0.06f, 0.62f));
                int front = bp.Mesh(ProceduralEnemyParts.LegPrefix + "F" + x, shoulder, Vector3.zero,
                    b => DrawUpper(b, new Vector3(0f, -0.42f, -0.06f), 0.1f, 0.075f, false), meshKey: "FrontUpper");
                int frontShin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + "F" + x, front, new Vector3(0f, -0.42f, -0.06f),
                    b => DrawLower(b, new Vector3(0f, -0.39f, 0.06f), 0.07f, 0.055f), meshKey: "FrontLower");
                bp.Mesh(ProceduralEnemyParts.FootPrefix + "F" + x, frontShin, new Vector3(0f, -0.39f, 0.06f), DrawBigPaw, meshKey: "FrontPaw");

                // Hind legs: hip → hock (well behind) → paw.
                int hind = bp.Mesh(ProceduralEnemyParts.LegPrefix + "H" + x, -1, new Vector3(0.18f * side, 0.92f, -0.42f),
                    b => DrawUpper(b, new Vector3(0f, -0.46f, -0.12f), 0.11f, 0.065f, true), meshKey: "HindUpper");
                int hindShin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + "H" + x, hind, new Vector3(0f, -0.46f, -0.12f),
                    b => DrawLower(b, new Vector3(0f, -0.38f, 0.09f), 0.06f, 0.05f), meshKey: "HindLower");
                bp.Mesh(ProceduralEnemyParts.FootPrefix + "H" + x, hindShin, new Vector3(0f, -0.38f, 0.09f), DrawHindPaw, meshKey: "HindPaw");
            }
        }

        /// <summary>A scruffy tuft: a little 4-sided cone poking out of the fur.</summary>
        private static void Tuft(EnemyMeshBuilder b, Vector3 at, float length, Vector3 euler, EnemySwatch swatch = Dark)
        {
            b.Cylinder(at, length * 0.32f, 0f, length, 4, swatch, rotation: Quaternion.Euler(euler));
        }

        private static void DrawHips(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, -0.06f), new Vector3(0.5f, 0.46f, 0.56f), Fur);
            b.Blob(new Vector3(0f, -0.14f, 0.05f), new Vector3(0.36f, 0.24f, 0.4f), Belly, coarse: true);
            b.Blob(new Vector3(0f, 0.12f, -0.04f), new Vector3(0.36f, 0.2f, 0.46f), Dark, coarse: true);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.15f * side, -0.08f, -0.1f), new Vector3(0.24f, 0.4f, 0.38f), Fur, coarse: true);
            }

            // Scruff along the lower back.
            Tuft(b, new Vector3(0f, 0.2f, 0.06f), 0.16f, new Vector3(-30f, 0f, 0f));
            Tuft(b, new Vector3(0.09f, 0.18f, -0.12f), 0.14f, new Vector3(-40f, 0f, -25f));
            Tuft(b, new Vector3(-0.09f, 0.18f, -0.2f), 0.13f, new Vector3(-45f, 0f, 25f));
        }

        private static void DrawTorso(EnemyMeshBuilder b)
        {
            // Pivot at the hips: the ribs and chest run forward along +z.
            b.Blob(new Vector3(0f, -0.04f, 0.24f), new Vector3(0.48f, 0.5f, 0.56f), Fur);
            b.Blob(new Vector3(0f, 0f, 0.56f), new Vector3(0.62f, 0.62f, 0.56f), Fur);
            // The big shaggy mane over the shoulders: the silhouette in the dark.
            b.Blob(new Vector3(0f, 0.16f, 0.6f), new Vector3(0.72f, 0.52f, 0.56f), Dark);
            // Pale chest bib (reads in the lantern light).
            b.Blob(new Vector3(0f, -0.2f, 0.7f), new Vector3(0.36f, 0.34f, 0.28f), Belly, coarse: true);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.21f * side, -0.04f, 0.62f), new Vector3(0.26f, 0.34f, 0.3f), Fur, coarse: true);
                Tuft(b, new Vector3(0.3f * side, 0.12f, 0.6f), 0.2f, new Vector3(-10f, 0f, -70f * side));
                Tuft(b, new Vector3(0.26f * side, 0.26f, 0.46f), 0.18f, new Vector3(-35f, 0f, -45f * side));
                Tuft(b, new Vector3(0.16f * side, -0.3f, 0.66f), 0.14f, new Vector3(170f, 0f, 20f * side), Belly);
            }

            Tuft(b, new Vector3(0f, 0.38f, 0.58f), 0.22f, new Vector3(-25f, 0f, 0f));
            Tuft(b, new Vector3(0f, 0.36f, 0.4f), 0.2f, new Vector3(-40f, 0f, 0f));
            Tuft(b, new Vector3(0f, 0.3f, 0.24f), 0.17f, new Vector3(-50f, 0f, 0f));
            // Thick neck up to the head.
            b.Segment(new Vector3(0f, 0.12f, 0.7f), new Vector3(0f, 0.28f, 0.92f), 0.19f, 0.15f, 6, Fur);
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.04f), new Vector3(0.36f, 0.32f, 0.34f), Fur);
            b.Blob(new Vector3(0f, 0.1f, 0.0f), new Vector3(0.3f, 0.14f, 0.28f), Dark, coarse: true);
            // Heavy brow over the red eyes.
            b.Box(new Vector3(0f, 0.1f, 0.17f), new Vector3(0.26f, 0.05f, 0.06f), Dark, Quaternion.Euler(-10f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.085f * side, 0.055f, 0.18f), new Vector3(0.075f, 0.06f, 0.04f), Eye, coarse: true);
                // Cheek tufts sticking out sideways.
                Tuft(b, new Vector3(0.15f * side, -0.04f, 0.06f), 0.15f, new Vector3(0f, 0f, -80f * side));
                Tuft(b, new Vector3(0.14f * side, -0.1f, 0.0f), 0.12f, new Vector3(20f, 0f, -110f * side));
            }

            // Long muzzle, pale underside, big black nose and two upper fangs.
            b.Cylinder(new Vector3(0f, -0.03f, 0.14f), 0.1f, 0.065f, 0.27f, 6, Fur, Belly, Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, 0.0f, 0.42f), new Vector3(0.09f, 0.07f, 0.07f), EnemySwatch.Black, coarse: true);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Cylinder(new Vector3(0.045f * side, -0.085f, 0.33f), 0.016f, 0f, 0.06f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }
        }

        private static void DrawJaw(EnemyMeshBuilder b)
        {
            // Hinge at the back of the mouth: lower jaw forward, a pink tongue lolling out of the side, under-bite fangs.
            b.Blob(new Vector3(0f, -0.02f, 0.17f), new Vector3(0.16f, 0.07f, 0.28f), Belly, coarse: true);
            b.Blob(new Vector3(0.05f, -0.06f, 0.3f), new Vector3(0.07f, 0.03f, 0.17f), EnemySwatch.Pink, Quaternion.Euler(30f, 12f, -10f), coarse: true);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Cylinder(new Vector3(0.04f * side, 0.01f, 0.27f), 0.014f, 0f, 0.05f, 3, EnemySwatch.Fang);
            }
        }

        private static void DrawEar(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.075f, 0f, 0.22f, 4, Dark);
            b.Cylinder(new Vector3(0f, 0.01f, 0.03f), 0.04f, 0f, 0.14f, 3, EnemySwatch.Pink);
        }

        private static void DrawTailBase(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.1f, -0.26f), 0.06f, 0.09f, 6, Fur);
            Tuft(b, new Vector3(0f, 0.04f, -0.14f), 0.12f, new Vector3(-60f, 0f, 0f));
        }

        private static void DrawTailTip(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.05f, -0.14f), new Vector3(0.18f, 0.18f, 0.34f), Fur);
            Tuft(b, new Vector3(0f, -0.08f, -0.3f), 0.14f, new Vector3(-100f, 0f, 0f), Belly);
            Tuft(b, new Vector3(0.06f, 0.02f, -0.2f), 0.12f, new Vector3(-60f, 0f, -40f));
        }

        /// <summary>Upper leg from the pivot down to <paramref name="knee"/>; a fat thigh for the hind legs, an elbow tuft for the arms.</summary>
        private static void DrawUpper(EnemyMeshBuilder b, Vector3 knee, float topRadius, float bottomRadius, bool thigh)
        {
            b.Segment(knee, new Vector3(0f, 0.05f, 0f), bottomRadius, topRadius, 6, Fur);
            if (thigh)
            {
                b.Blob(new Vector3(0f, -0.12f, -0.01f), new Vector3(0.2f, 0.38f, 0.28f), Fur, coarse: true);
            }
            else
            {
                Tuft(b, knee + new Vector3(0f, 0.02f, -0.05f), 0.13f, new Vector3(-120f, 0f, 0f));
            }

            b.Blob(knee, Vector3.one * (bottomRadius * 2.1f), Fur, coarse: true);
        }

        private static void DrawLower(EnemyMeshBuilder b, Vector3 paw, float topRadius, float bottomRadius)
        {
            b.Segment(paw, Vector3.zero, bottomRadius, topRadius, 5, Dark);
        }

        /// <summary>Oversized front paw with three pale claws (they flash when it rears up).</summary>
        private static void DrawBigPaw(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.03f, 0.05f), new Vector3(0.17f, 0.09f, 0.2f), Dark, coarse: true);
            for (int k = -1; k <= 1; k++)
            {
                b.Cylinder(new Vector3(0.045f * k, -0.04f, 0.13f), 0.018f, 0f, 0.08f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(80f, 0f, 0f));
            }
        }

        private static void DrawHindPaw(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.025f, 0.05f), new Vector3(0.13f, 0.07f, 0.18f), Dark, coarse: true);
            for (int k = -1; k <= 1; k += 2)
            {
                b.Cylinder(new Vector3(0.03f * k, -0.03f, 0.13f), 0.015f, 0f, 0.06f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(80f, 0f, 0f));
            }
        }
    }
}
