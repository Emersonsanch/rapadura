using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>
    /// Procedural low-poly stand-ins for the 580 catalog items (no prefabs yet; the <c>_prefabKey</c> slot is where
    /// art goes later): one archetype per function / first word of the name (bed, chest, wardrobe, table, seat,
    /// stove, rug, plant, lamp, picture, curtain, shelf, hammock, hanging piece, small object, tree, vegetable bed,
    /// well, doghouse...), coloured on the house palette and varied by a hash of the id. Local frame: centred on the
    /// footprint, base at y = 0 (ceiling pieces hang down from y = 0), front towards −z (into the room for wall
    /// pieces). Cached per item and layer. Each stays far under the per-rarity triangle budgets (§3.5).
    /// </summary>
    public static class HouseFurnitureMeshes
    {
        private static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();
        private static readonly HouseSwatch[] Cloths = { HouseSwatch.ClothRed, HouseSwatch.ClothBlue, HouseSwatch.ClothGreen, HouseSwatch.ClothYellow, HouseSwatch.ClothPink, HouseSwatch.Purple };

        /// <summary>Footprint size in metres on <paramref name="layer"/> (unrotated: x = width, z = depth; wall: x = width, y = height).</summary>
        public static Vector2 SizeMetres(HouseItemDefinition item, HouseLayer layer)
        {
            (int w, int h) = HousePlacementRules.BaseFootprint(item, layer);
            float cell = layer == HouseLayer.Yard ? HouseLevelSpec.YardCellSize : HouseLevelSpec.InteriorCellSize;
            if (layer == HouseLayer.Surface) return new Vector2(0.24f, 0.24f);
            return new Vector2(w * cell, h * cell);
        }

        public static Mesh Get(HouseItemDefinition item, HouseLayer layer)
        {
            if (item == null) return null;
            string key = item.ItemId + "/" + layer;
            if (Cache.TryGetValue(key, out Mesh cached) && cached != null) return cached;
            var b = new HouseMeshBuilder();
            Build(b, item, layer);
            Mesh mesh = b.ToMesh("HouseItem_" + key);
            Cache[key] = mesh;
            return mesh;
        }

        public static void Build(HouseMeshBuilder b, HouseItemDefinition item, HouseLayer layer)
        {
            Vector2 size = SizeMetres(item, layer);
            uint hash = Hash(item.ItemId);
            HouseSwatch cloth = Cloths[hash % (uint)Cloths.Length];
            HouseSwatch wood = (hash >> 4) % 3 == 0 ? HouseSwatch.WoodDark : HouseSwatch.WoodRaw;
            string word = FirstWord(item.NamePtBR);

            switch (layer)
            {
                case HouseLayer.Rug: Rug(b, size, cloth, hash); return;
                case HouseLayer.Wall: WallPiece(b, item, word, size, cloth, wood); return;
                case HouseLayer.Ceiling: CeilingPiece(b, item, word, size, cloth); return;
                case HouseLayer.Surface: SmallObject(b, item, word, cloth, hash); return;
                case HouseLayer.Yard:
                    if ((item.Placement & HousePlacement.Yard) != 0) { YardPiece(b, item, word, size, cloth, wood); return; }
                    break;
            }

            FloorPiece(b, item, word, size, cloth, wood);
        }

        // ------------------------------------------------------------------

        private static void FloorPiece(HouseMeshBuilder b, HouseItemDefinition item, string word, Vector2 s, HouseSwatch cloth, HouseSwatch wood)
        {
            float w = s.x - 0.06f, d = s.y - 0.06f;
            switch (item.Function)
            {
                case HouseItemFunction.Bed:
                    b.Box(new Vector3(0f, 0.15f, 0f), new Vector3(w, 0.3f, d), wood);
                    b.Box(new Vector3(0f, 0.36f, 0f), new Vector3(w - 0.06f, 0.12f, d - 0.06f), HouseSwatch.Cream);
                    b.Box(new Vector3(0f, 0.44f, -d * 0.12f), new Vector3(w - 0.04f, 0.05f, d * 0.7f), cloth);
                    b.Box(new Vector3(0f, 0.47f, d * 0.36f), new Vector3(w * 0.6f, 0.1f, d * 0.18f), HouseSwatch.White);
                    b.Box(new Vector3(0f, 0.45f, d * 0.5f), new Vector3(w, 0.9f, 0.06f), HouseSwatch.WoodDark);
                    return;
                case HouseItemFunction.Chest:
                case HouseItemFunction.VillageChestLink:
                    b.Box(new Vector3(0f, 0.2f, 0f), new Vector3(w, 0.4f, d), wood);
                    b.Box(new Vector3(0f, 0.43f, 0f), new Vector3(w + 0.02f, 0.08f, d + 0.02f), HouseSwatch.WoodJoint);
                    b.Box(new Vector3(0f, 0.32f, -d * 0.5f - 0.01f), new Vector3(0.08f, 0.1f, 0.03f), item.Function == HouseItemFunction.VillageChestLink ? HouseSwatch.Gold : HouseSwatch.Iron);
                    return;
                case HouseItemFunction.Wardrobe:
                    Tall(b, w, d, 1.8f, wood, true);
                    return;
                case HouseItemFunction.Stove:
                case HouseItemFunction.Forge:
                    b.Box(new Vector3(0f, 0.4f, 0f), new Vector3(w, 0.8f, d), HouseSwatch.Clay);
                    b.Box(new Vector3(0f, 0.82f, 0f), new Vector3(w, 0.05f, d), HouseSwatch.Black);
                    b.Box(new Vector3(0f, 0.3f, -d * 0.5f), new Vector3(w * 0.4f, 0.25f, 0.03f), HouseSwatch.LampGlow);
                    b.Cylinder(new Vector3(w * 0.3f, 0.84f, d * 0.25f), 0.08f, 0.07f, 1.0f, 6, HouseSwatch.Clay);
                    return;
                case HouseItemFunction.PetBed:
                    b.Cylinder(Vector3.zero, Mathf.Min(w, d) * 0.48f, Mathf.Min(w, d) * 0.45f, 0.14f, 8, cloth, HouseSwatch.Cream);
                    return;
            }

            switch (word)
            {
                case "Mesa": case "Mesinha": case "Escrivaninha": case "Bancada": case "Balcão": case "Criado-mudo":
                    Table(b, w, d, word == "Mesinha" || word == "Criado-mudo" ? 0.5f : 0.75f, wood, word == "Mesa" || word == "Mesinha" ? (HouseSwatch?)null : HouseSwatch.WoodJoint);
                    return;
                case "Cadeira": case "Poltrona": case "Sofá": case "Namoradeira":
                    Seat(b, w, d, word == "Cadeira" ? wood : cloth, true, wood);
                    return;
                case "Banco": case "Banquinho": case "Mocho": case "Tamborete": case "Pufe": case "Baú-banco":
                    Seat(b, w, d, word == "Pufe" ? cloth : wood, false, wood);
                    return;
                case "Guarda-roupa": case "Cristaleira": case "Estante": case "Penteadeira": case "Cômoda": case "Aparador": case "Gaveteiro": case "Vitrine":
                    Tall(b, w, d, word == "Estante" || word == "Cristaleira" ? 1.8f : 0.9f, wood, word != "Estante");
                    return;
                case "Geladeira":
                    b.Box(new Vector3(0f, 0.8f, 0f), new Vector3(w, 1.6f, d), HouseSwatch.White);
                    b.Box(new Vector3(w * 0.35f, 1.0f, -d * 0.5f - 0.01f), new Vector3(0.04f, 0.4f, 0.03f), HouseSwatch.Iron);
                    return;
            }

            if (item.Category == HouseItemCategory.Plant || word.StartsWith("Árvore", StringComparison.Ordinal))
            {
                PottedPlant(b, Mathf.Min(w, d), 0.9f + Mathf.Min(w, d) * 0.6f, cloth);
                return;
            }

            if (item.Category == HouseItemCategory.Trophy || word == "Estátua" || word == "Pedestal" || word == "Escultura" || word == "Totem")
            {
                b.Box(new Vector3(0f, 0.2f, 0f), new Vector3(w * 0.8f, 0.4f, d * 0.8f), HouseSwatch.Stone);
                b.Blob(new Vector3(0f, 0.75f, 0f), new Vector3(w * 0.6f, 0.7f, d * 0.6f), HouseSwatch.WoodDark);
                b.Blob(new Vector3(0f, 1.15f, 0f), new Vector3(w * 0.35f, 0.3f, d * 0.35f), HouseSwatch.Gold);
                return;
            }

            if (item.Category == HouseItemCategory.Music)
            {
                b.Cylinder(new Vector3(0f, 0f, 0f), Mathf.Min(w, d) * 0.4f, Mathf.Min(w, d) * 0.4f, 0.5f, 8, cloth, HouseSwatch.Cream);
                b.Box(new Vector3(0f, 0.25f, 0f), new Vector3(Mathf.Min(w, d) * 0.82f, 0.05f, 0.05f), HouseSwatch.Gold);
                return;
            }

            // Generic piece of furniture: wooden body with a coloured top.
            float height = Mathf.Clamp(0.3f + (w + d) * 0.2f, 0.4f, 1.1f);
            b.Box(new Vector3(0f, height * 0.5f, 0f), new Vector3(w, height, d), wood, top: cloth);
        }

        private static void Table(HouseMeshBuilder b, float w, float d, float height, HouseSwatch wood, HouseSwatch? shelf)
        {
            b.Box(new Vector3(0f, height - 0.03f, 0f), new Vector3(w, 0.06f, d), wood, top: HouseSwatch.FloorBoard);
            float lx = w * 0.5f - 0.05f, lz = d * 0.5f - 0.05f;
            foreach (Vector2 c in new[] { new Vector2(-lx, -lz), new Vector2(lx, -lz), new Vector2(-lx, lz), new Vector2(lx, lz) })
            {
                b.Box(new Vector3(c.x, (height - 0.06f) * 0.5f, c.y), new Vector3(0.06f, height - 0.06f, 0.06f), HouseSwatch.WoodJoint);
            }

            if (shelf.HasValue)
            {
                b.Box(new Vector3(0f, 0.18f, 0f), new Vector3(w - 0.1f, 0.04f, d - 0.1f), shelf.Value);
            }
        }

        private static void Seat(HouseMeshBuilder b, float w, float d, HouseSwatch seat, bool back, HouseSwatch wood)
        {
            float seatW = Mathf.Min(w, back ? w : w), seatD = back ? d * 0.8f : d * 0.85f;
            b.Box(new Vector3(0f, 0.38f, back ? -d * 0.08f : 0f), new Vector3(seatW, 0.1f, seatD), seat);
            float lx = seatW * 0.5f - 0.05f, lz = seatD * 0.5f - 0.05f;
            float oz = back ? -d * 0.08f : 0f;
            foreach (Vector2 c in new[] { new Vector2(-lx, -lz), new Vector2(lx, -lz), new Vector2(-lx, lz), new Vector2(lx, lz) })
            {
                b.Box(new Vector3(c.x, 0.17f, c.y + oz), new Vector3(0.05f, 0.34f, 0.05f), wood);
            }

            if (back)
            {
                b.Box(new Vector3(0f, 0.7f, d * 0.4f), new Vector3(seatW, 0.6f, 0.08f), seat);
            }
        }

        private static void Tall(HouseMeshBuilder b, float w, float d, float height, HouseSwatch wood, bool doors)
        {
            b.Box(new Vector3(0f, height * 0.5f, 0f), new Vector3(w, height, d), wood);
            if (doors)
            {
                b.Box(new Vector3(0f, height * 0.5f, -d * 0.5f - 0.005f), new Vector3(0.02f, height - 0.1f, 0.02f), HouseSwatch.WoodJoint);
                b.Box(new Vector3(-0.06f, height * 0.5f, -d * 0.5f - 0.015f), new Vector3(0.03f, 0.08f, 0.03f), HouseSwatch.Iron);
                b.Box(new Vector3(0.06f, height * 0.5f, -d * 0.5f - 0.015f), new Vector3(0.03f, 0.08f, 0.03f), HouseSwatch.Iron);
            }
            else
            {
                for (int i = 1; i < 4; i++)
                {
                    b.Box(new Vector3(0f, height * i / 4f, -d * 0.5f - 0.01f), new Vector3(w - 0.04f, 0.03f, 0.04f), HouseSwatch.WoodJoint);
                }
            }
        }

        private static void PottedPlant(HouseMeshBuilder b, float footprint, float height, HouseSwatch flower)
        {
            float pot = Mathf.Clamp(footprint * 0.35f, 0.1f, 0.3f);
            b.Cylinder(Vector3.zero, pot * 0.8f, pot, pot * 1.4f, 7, HouseSwatch.Clay, HouseSwatch.Dirt);
            b.Blob(new Vector3(0f, pot * 1.4f + height * 0.3f, 0f), new Vector3(footprint * 0.8f, height * 0.6f, footprint * 0.8f), HouseSwatch.Leaf);
            b.Blob(new Vector3(footprint * 0.15f, pot * 1.4f + height * 0.55f, -footprint * 0.1f), new Vector3(footprint * 0.4f, height * 0.35f, footprint * 0.4f), HouseSwatch.GrassDark);
            b.Blob(new Vector3(-footprint * 0.12f, pot * 1.4f + height * 0.62f, -footprint * 0.2f), new Vector3(0.1f, 0.09f, 0.1f), flower == HouseSwatch.ClothGreen ? HouseSwatch.Flower : flower);
        }

        private static void Rug(HouseMeshBuilder b, Vector2 s, HouseSwatch cloth, uint hash)
        {
            float w = s.x - 0.04f, d = s.y - 0.04f;
            bool round = (hash >> 7) % 2 == 0;
            if (round)
            {
                b.Cylinder(new Vector3(0f, 0.002f, 0f), Mathf.Min(w, d) * 0.5f, Mathf.Min(w, d) * 0.5f, 0.015f, 10, cloth, cloth);
                b.Cylinder(new Vector3(0f, 0.004f, 0f), Mathf.Min(w, d) * 0.3f, Mathf.Min(w, d) * 0.3f, 0.015f, 10, HouseSwatch.Cream, HouseSwatch.Cream);
            }
            else
            {
                b.Box(new Vector3(0f, 0.008f, 0f), new Vector3(w, 0.015f, d), cloth);
                b.Box(new Vector3(0f, 0.012f, 0f), new Vector3(w * 0.7f, 0.015f, d * 0.6f), HouseSwatch.Cream);
            }
        }

        private static void WallPiece(HouseMeshBuilder b, HouseItemDefinition item, string word, Vector2 s, HouseSwatch cloth, HouseSwatch wood)
        {
            float w = s.x - 0.04f, h = s.y - 0.04f;
            if (item.Category == HouseItemCategory.Curtain || word == "Persiana")
            {
                b.Box(new Vector3(0f, h - 0.02f, -0.06f), new Vector3(w + 0.1f, 0.04f, 0.04f), HouseSwatch.WoodDark);
                b.Box(new Vector3(-w * 0.36f, h * 0.5f, -0.05f), new Vector3(w * 0.26f, h - 0.04f, 0.03f), cloth);
                b.Box(new Vector3(w * 0.36f, h * 0.5f, -0.05f), new Vector3(w * 0.26f, h - 0.04f, 0.03f), cloth);
                return;
            }

            if (word == "Rede")
            {
                b.Box(new Vector3(-w * 0.5f, h * 0.5f, -0.04f), new Vector3(0.05f, 0.05f, 0.06f), HouseSwatch.Iron);
                b.Box(new Vector3(w * 0.5f, h * 0.5f, -0.04f), new Vector3(0.05f, 0.05f, 0.06f), HouseSwatch.Iron);
                b.Box(new Vector3(0f, h * 0.1f, -0.35f), new Vector3(w * 0.75f, 0.06f, 0.5f), cloth);
                b.Box(new Vector3(-w * 0.43f, h * 0.3f, -0.2f), new Vector3(w * 0.15f, 0.03f, 0.3f), HouseSwatch.Cream, -20f);
                b.Box(new Vector3(w * 0.43f, h * 0.3f, -0.2f), new Vector3(w * 0.15f, 0.03f, 0.3f), HouseSwatch.Cream, 20f);
                return;
            }

            if (word == "Prateleira" || item.SurfaceSlots > 0)
            {
                b.Box(new Vector3(0f, 0.02f, -0.13f), new Vector3(w, 0.04f, 0.26f), wood, top: HouseSwatch.FloorBoard);
                b.Box(new Vector3(-w * 0.4f, -0.08f, -0.06f), new Vector3(0.04f, 0.16f, 0.12f), HouseSwatch.Iron);
                b.Box(new Vector3(w * 0.4f, -0.08f, -0.06f), new Vector3(0.04f, 0.16f, 0.12f), HouseSwatch.Iron);
                return;
            }

            if (item.Function == HouseItemFunction.TrophyStand || item.Category == HouseItemCategory.Trophy)
            {
                b.Box(new Vector3(0f, h * 0.5f, -0.02f), new Vector3(w, h, 0.04f), HouseSwatch.WoodDark);
                b.Blob(new Vector3(0f, h * 0.5f, -0.15f), new Vector3(w * 0.55f, h * 0.6f, 0.25f), HouseSwatch.WoodRaw);
                b.Box(new Vector3(0f, h * 0.12f, -0.05f), new Vector3(w * 0.5f, 0.06f, 0.02f), HouseSwatch.Gold);
                return;
            }

            if (item.Category == HouseItemCategory.Plant)
            {
                b.Box(new Vector3(0f, h * 0.3f, -0.08f), new Vector3(w * 0.5f, 0.15f, 0.14f), HouseSwatch.Clay);
                b.Blob(new Vector3(0f, h * 0.55f, -0.12f), new Vector3(w * 0.8f, h * 0.5f, 0.22f), HouseSwatch.Leaf);
                return;
            }

            // Picture / plaque / instrument on the wall: a frame with a coloured face.
            b.Box(new Vector3(0f, h * 0.5f, -0.025f), new Vector3(w, h, 0.05f), wood);
            b.Box(new Vector3(0f, h * 0.5f, -0.052f), new Vector3(w - 0.08f, h - 0.08f, 0.01f), cloth);
            b.Box(new Vector3(-w * 0.15f, h * 0.55f, -0.058f), new Vector3(w * 0.3f, h * 0.3f, 0.005f), HouseSwatch.Cream);
        }

        private static void CeilingPiece(HouseMeshBuilder b, HouseItemDefinition item, string word, Vector2 s, HouseSwatch cloth)
        {
            float w = s.x - 0.05f, d = s.y - 0.05f;
            if (word == "Varal" || word == "Bandeirinhas" || word == "Pisca-pisca" || word == "Serpentinas")
            {
                b.Box(new Vector3(0f, -0.3f, 0f), new Vector3(w, 0.015f, 0.015f), HouseSwatch.White);
                int n = Mathf.Max(3, Mathf.RoundToInt(w / 0.25f));
                for (int i = 0; i < n; i++)
                {
                    float x = -w * 0.5f + (i + 0.5f) * w / n;
                    b.Triangle(new Vector3(x - 0.08f, -0.3f, 0f), new Vector3(x + 0.08f, -0.3f, 0f), new Vector3(x, -0.48f, 0f), i % 2 == 0 ? cloth : HouseSwatch.ClothYellow);
                    b.Triangle(new Vector3(x - 0.08f, -0.3f, 0f), new Vector3(x, -0.48f, 0f), new Vector3(x + 0.08f, -0.3f, 0f), i % 2 == 0 ? cloth : HouseSwatch.ClothYellow);
                }

                return;
            }

            b.Box(new Vector3(0f, -0.25f, 0f), new Vector3(0.02f, 0.5f, 0.02f), HouseSwatch.Black);
            if (item.Category == HouseItemCategory.Plant || word == "Raiz" || word == "Jiboia")
            {
                b.Cylinder(new Vector3(0f, -0.75f, 0f), 0.12f, 0.16f, 0.2f, 7, HouseSwatch.Clay, HouseSwatch.Dirt);
                b.Blob(new Vector3(0f, -0.8f, 0f), new Vector3(Mathf.Max(0.5f, w), 0.5f, Mathf.Max(0.5f, d)), HouseSwatch.Leaf);
                return;
            }

            if (word == "Mosquiteiro")
            {
                b.Cylinder(new Vector3(0f, -1.6f, 0f), Mathf.Min(w, d) * 0.5f, 0.06f, 1.1f, 8, HouseSwatch.White);
                return;
            }

            b.Cylinder(new Vector3(0f, -0.75f, 0f), Mathf.Min(w, d) * 0.45f, 0.06f, 0.25f, 8, item.Category == HouseItemCategory.Lighting ? HouseSwatch.Iron : cloth);
            b.Blob(new Vector3(0f, -0.78f, 0f), new Vector3(0.16f, 0.16f, 0.16f), HouseSwatch.LampGlow);
        }

        private static void SmallObject(HouseMeshBuilder b, HouseItemDefinition item, string word, HouseSwatch cloth, uint hash)
        {
            if (item.Category == HouseItemCategory.Lighting)
            {
                b.Cylinder(Vector3.zero, 0.08f, 0.06f, 0.05f, 6, HouseSwatch.Iron);
                b.Box(new Vector3(0f, 0.13f, 0f), new Vector3(0.1f, 0.14f, 0.1f), HouseSwatch.LampGlow);
                b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.07f, 0.02f, 0.06f, 6, HouseSwatch.Iron);
                return;
            }

            if (item.Category == HouseItemCategory.Plant)
            {
                PottedPlant(b, 0.24f, 0.25f, cloth);
                return;
            }

            if (item.Function == HouseItemFunction.GuestBook || word == "Livro" || word == "Caderno")
            {
                b.Box(new Vector3(0f, 0.025f, 0f), new Vector3(0.22f, 0.05f, 0.16f), HouseSwatch.WoodDark, top: HouseSwatch.Cream);
                return;
            }

            switch (hash % 3)
            {
                case 0:
                    b.Cylinder(Vector3.zero, 0.07f, 0.09f, 0.18f, 7, HouseSwatch.Clay, cloth);
                    return;
                case 1:
                    b.Box(new Vector3(0f, 0.06f, 0f), new Vector3(0.18f, 0.12f, 0.14f), cloth, top: HouseSwatch.Cream);
                    return;
                default:
                    b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.18f, 0.16f, 0.18f), cloth);
                    return;
            }
        }

        private static void YardPiece(HouseMeshBuilder b, HouseItemDefinition item, string word, Vector2 s, HouseSwatch cloth, HouseSwatch wood)
        {
            float w = s.x - 0.1f, d = s.y - 0.1f;
            switch (word)
            {
                case "Pé": case "Árvore": case "Coqueiro": case "Bananeira":
                    b.Cylinder(Vector3.zero, 0.14f, 0.1f, 1.6f, 6, HouseSwatch.WoodDark);
                    b.Blob(new Vector3(0f, 2.0f, 0f), new Vector3(w, 1.1f, d), HouseSwatch.Leaf);
                    b.Blob(new Vector3(w * 0.2f, 1.7f, -d * 0.15f), new Vector3(w * 0.6f, 0.7f, d * 0.6f), HouseSwatch.GrassDark);
                    b.Blob(new Vector3(-w * 0.2f, 1.75f, -d * 0.3f), new Vector3(0.18f, 0.18f, 0.18f), HouseSwatch.Fruit);
                    return;
                case "Canteiro": case "Horta":
                    b.Box(new Vector3(0f, 0.15f, 0f), new Vector3(w, 0.3f, d), HouseSwatch.WoodRaw, top: HouseSwatch.Dirt);
                    for (int i = 0; i < 3; i++)
                    {
                        b.Blob(new Vector3(-w * 0.3f + i * w * 0.3f, 0.38f, 0f), new Vector3(w * 0.22f, 0.2f, d * 0.6f), HouseSwatch.Leaf);
                    }

                    return;
                case "Poço": case "Cisterna":
                    b.Cylinder(Vector3.zero, Mathf.Min(w, d) * 0.45f, Mathf.Min(w, d) * 0.45f, 0.7f, 8, HouseSwatch.Stone, HouseSwatch.Water);
                    b.Box(new Vector3(-w * 0.4f, 0.9f, 0f), new Vector3(0.08f, 1.0f, 0.08f), HouseSwatch.WoodDark);
                    b.Box(new Vector3(w * 0.4f, 0.9f, 0f), new Vector3(0.08f, 1.0f, 0.08f), HouseSwatch.WoodDark);
                    b.Box(new Vector3(0f, 1.3f, 0f), new Vector3(w * 0.9f, 0.06f, 0.06f), HouseSwatch.WoodRaw);
                    return;
                case "Casinha": case "Galinheiro": case "Chiqueirinho": case "Barraca": case "Tenda":
                    b.Box(new Vector3(0f, 0.3f, 0f), new Vector3(w, 0.6f, d), wood);
                    b.Box(new Vector3(0f, 0.25f, -d * 0.5f - 0.005f), new Vector3(w * 0.35f, 0.4f, 0.02f), HouseSwatch.Black);
                    b.GableRoof(-w * 0.55f, w * 0.55f, -d * 0.55f, d * 0.55f, 0.6f, 0.35f, 0.03f, word == "Barraca" || word == "Tenda" ? cloth : HouseSwatch.Clay, HouseSwatch.WoodJoint);
                    return;
                case "Cerca": case "Porteira":
                    for (float x = -w * 0.5f; x <= w * 0.5f + 0.01f; x += 0.5f)
                    {
                        b.Box(new Vector3(x, 0.4f, 0f), new Vector3(0.08f, 0.8f, 0.08f), HouseSwatch.WoodDark);
                    }

                    b.Box(new Vector3(0f, 0.55f, 0f), new Vector3(w, 0.06f, 0.04f), HouseSwatch.WoodRaw);
                    b.Box(new Vector3(0f, 0.3f, 0f), new Vector3(w, 0.06f, 0.04f), HouseSwatch.WoodRaw);
                    return;
            }

            if (item.Category == HouseItemCategory.Corral && word != "Curral")
            {
                // Animals and their things: a friendly blob on legs.
                b.Blob(new Vector3(0f, 0.35f, 0f), new Vector3(w * 0.6f, 0.35f, d * 0.8f), HouseSwatch.Cream);
                b.Blob(new Vector3(0f, 0.5f, -d * 0.4f), new Vector3(0.2f, 0.2f, 0.2f), HouseSwatch.Cream);
                return;
            }

            float height = Mathf.Clamp(0.4f + (w + d) * 0.15f, 0.5f, 1.4f);
            b.Box(new Vector3(0f, height * 0.5f, 0f), new Vector3(w, height, d), wood, top: cloth);
        }

        private static string FirstWord(string name) => HouseItemNames.FirstWord(name);

        /// <summary>FNV-1a: stable across runs and platforms (string.GetHashCode is not).</summary>
        public static uint Hash(string text)
        {
            uint hash = 2166136261;
            foreach (char c in text ?? string.Empty)
            {
                hash ^= c;
                hash *= 16777619;
            }

            return hash;
        }
    }

    /// <summary>Name helpers shared by runtime code (the importer has its own copy behind UNITY_EDITOR).</summary>
    public static class HouseItemNames
    {
        public static string FirstWord(string name)
        {
            string trimmed = (name ?? string.Empty).Trim();
            int space = trimmed.IndexOf(' ');
            return (space < 0 ? trimmed : trimmed.Substring(0, space)).TrimEnd(',', '.');
        }
    }
}
