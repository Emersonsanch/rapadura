using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Pets.Visuals
{
    /// <summary>Part names of Caramelo's model (what <see cref="CarameloCompanion"/> animates).</summary>
    public static class CarameloParts
    {
        public const string Body = "Body";
        public const string Head = "Head";
        public const string EarLeft = "Ear_L";
        public const string EarRight = "Ear_R";
        public const string Tongue = "Tongue";
        public const string Tail = "Tail";
        public const string TailTip = "TailTip";
        public const string PawFrontLeft = "Paw_FL";
        public const string PawFrontRight = "Paw_FR";
        public const string PawBackLeft = "Paw_BL";
        public const string PawBackRight = "Paw_BR";
        /// <summary>Fetched loot held in the mouth (hidden unless carrying): a gold coin or a material bundle.</summary>
        public const string CarryGold = "Carry_Gold";
        public const string CarryBundle = "Carry_Bundle";
    }

    /// <summary>
    /// Caramelo (visible name: Ari), Vó Edna's dog, about 0.55 m at the shoulder: a caramel mutt with a darker saddle
    /// and floppy ears, cream muzzle, belly, "socks" and tail tip, a red collar with a little gold tag.
    /// Built with the enemies' <see cref="EnemyMeshBuilder"/> on their shared palette
    /// (<see cref="EnemyModelPalette"/>: SandstoneDark = caramel, Leather = saddle/ears, SandstoneLight =
    /// cream, WarCape = collar/tongue), so he costs no extra material. Faces +Z, pivot at the feet.
    /// Hierarchy: <c>Body</c> → <c>Head</c> (→ ears, tongue, carried items), <c>Tail</c> → <c>TailTip</c>;
    /// the four paws hang from hip/shoulder pivots under the root (so the body can bob and sit while the
    /// feet stay planted). About 900 triangles.
    /// </summary>
    public static class CarameloRecipe
    {
        public const float ShoulderHeight = 0.55f;
        private const float HipHeight = 0.36f;

        private const EnemySwatch Caramel = EnemySwatch.SandstoneDark;
        private const EnemySwatch Saddle = EnemySwatch.Leather;
        private const EnemySwatch Cream = EnemySwatch.SandstoneLight;
        private const EnemySwatch Collar = EnemySwatch.WarCape;

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(CarameloParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawBody);

            int head = bp.Mesh(CarameloParts.Head, body, new Vector3(0f, 0.17f, 0.36f), DrawHead, Quaternion.Euler(-8f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? CarameloParts.EarRight : CarameloParts.EarLeft, head, new Vector3(0.095f * side, 0.1f, -0.02f),
                    b => DrawEar(b, side), Quaternion.Euler(0f, 0f, -18f * side));
            }

            bp.Mesh(CarameloParts.Tongue, head, new Vector3(0f, -0.085f, 0.17f), DrawTongue, Quaternion.Euler(18f, 0f, 0f));
            bp.Mesh(CarameloParts.CarryGold, head, new Vector3(0f, -0.07f, 0.24f), DrawCoin, Quaternion.Euler(0f, 0f, 90f));
            bp.Mesh(CarameloParts.CarryBundle, head, new Vector3(0f, -0.08f, 0.24f), DrawBundle);

            // Up and back; the tip is its own part so it lags the base (follow-through).
            int tail = bp.Mesh(CarameloParts.Tail, body, new Vector3(0f, 0.08f, -0.36f), DrawTail, Quaternion.Euler(-55f, 0f, 0f));
            bp.Mesh(CarameloParts.TailTip, tail, new Vector3(0f, 0.16f, 0f), DrawTailTip, Quaternion.Euler(-15f, 0f, 0f));

            AddLeg(bp, CarameloParts.PawFrontLeft, new Vector3(-0.1f, HipHeight, 0.24f), false);
            AddLeg(bp, CarameloParts.PawFrontRight, new Vector3(0.1f, HipHeight, 0.24f), false);
            AddLeg(bp, CarameloParts.PawBackLeft, new Vector3(-0.1f, HipHeight, -0.25f), true);
            AddLeg(bp, CarameloParts.PawBackRight, new Vector3(0.1f, HipHeight, -0.25f), true);
        }

        private static void AddLeg(ProceduralEnemyBlueprint bp, string name, Vector3 hip, bool back)
        {
            // Front legs share one mesh, back legs another (mirrored legs are symmetric anyway).
            bp.Mesh(name, -1, hip, b => DrawLeg(b, back), meshKey: back ? "LegBack" : "LegFront");
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, -0.03f), new Vector3(0.32f, 0.29f, 0.7f), Caramel);
            b.Blob(new Vector3(0f, 0.03f, 0.2f), new Vector3(0.31f, 0.32f, 0.34f), Caramel);
            b.Blob(new Vector3(0f, -0.08f, 0.06f), new Vector3(0.24f, 0.15f, 0.46f), Cream);
            b.Blob(new Vector3(0f, 0.12f, -0.08f), new Vector3(0.27f, 0.11f, 0.42f), Saddle);
            b.Blob(new Vector3(0f, -0.05f, 0.3f), new Vector3(0.16f, 0.16f, 0.1f), Cream, coarse: true);

            // Collar round the neck, tag hanging under the chin.
            b.Cylinder(new Vector3(0f, 0.07f, 0.31f), 0.13f, 0.12f, 0.05f, 9, Collar, rotation: Quaternion.Euler(62f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.04f, 0.42f), new Vector3(0.05f, 0.05f, 0.02f), EnemySwatch.Gold, coarse: true);
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.25f, 0.23f, 0.25f), Caramel);
            b.Box(new Vector3(0f, -0.035f, 0.15f), new Vector3(0.12f, 0.1f, 0.16f), Cream, top: Caramel);
            b.Blob(new Vector3(0f, -0.005f, 0.235f), new Vector3(0.06f, 0.045f, 0.04f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.062f * s, 0.05f, 0.105f), new Vector3(0.035f, 0.038f, 0.025f), EnemySwatch.Black, coarse: true);
                // Cream "eyebrow" spots: the mutt's tan points.
                b.Blob(new Vector3(0.055f * s, 0.095f, 0.095f), new Vector3(0.03f, 0.018f, 0.02f), Cream, coarse: true);
            }
        }

        private static void DrawEar(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            // Floppy flap hanging down beside the head (profile in z, y).
            Vector2[] profile =
            {
                new Vector2(-0.045f, 0.01f), new Vector2(0.045f, 0.01f), new Vector2(0.04f, -0.09f),
                new Vector2(0.01f, -0.13f), new Vector2(-0.03f, -0.11f),
            };
            b.Plate(profile, 0.012f, Saddle);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawTongue(EnemyMeshBuilder b)
        {
            b.Box(Vector3.zero, new Vector3(0.05f, 0.012f, 0.07f), Collar);
        }

        private static void DrawCoin(EnemyMeshBuilder b)
        {
            b.Cylinder(new Vector3(0f, -0.008f, 0f), 0.055f, 0.055f, 0.016f, 10, EnemySwatch.Gold, EnemySwatch.Gold, bottomCap: true);
        }

        private static void DrawBundle(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.1f, 0.08f, 0.09f), Saddle, coarse: true);
            b.Box(new Vector3(0f, 0.035f, 0f), new Vector3(0.11f, 0.015f, 0.02f), Cream);
        }

        private static void DrawTail(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.035f, 0.03f, 0.17f, 6, Caramel);
        }

        private static void DrawTailTip(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.03f, 0.005f, 0.13f, 6, Cream);
        }

        private static void DrawLeg(EnemyMeshBuilder b, bool back)
        {
            float length = HipHeight - 0.01f;
            if (back)
            {
                b.Blob(new Vector3(0f, -0.04f, -0.01f), new Vector3(0.11f, 0.2f, 0.15f), Caramel);
            }
            else
            {
                b.Blob(new Vector3(0f, -0.03f, 0f), new Vector3(0.09f, 0.15f, 0.1f), Caramel, coarse: true);
            }

            b.Cylinder(new Vector3(0f, -length, 0f), 0.035f, 0.045f, length, 6, Caramel);
            b.Blob(new Vector3(0f, -length + 0.015f, 0.02f), new Vector3(0.075f, 0.045f, 0.1f), Cream, coarse: true);
        }
    }
}
