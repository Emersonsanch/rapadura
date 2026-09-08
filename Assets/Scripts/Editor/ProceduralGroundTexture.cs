using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Regenerates the ground base color + normal map procedurally — the original Tex_Grass.png
    /// (256x256, 144 distinct colors) is nearly flat, which combined with an 8x8 material tiling
    /// makes the 50x50 ground plane read as one small tile stamped repeatedly (see TODO.md "Revisão
    /// de qualidade visual do ambiente", fase 3). No image-gen API is configured in this project
    /// (fal/OpenRouter absent) so this is done with layered Perlin noise instead — zero cost, and at
    /// 1024x1024 the tile repeat is 4x less visible per-tile than the 256x256 original at the same
    /// world-space tiling.
    /// </summary>
    public static class ProceduralGroundTexture
    {
        private const string BaseColorPath = "Assets/Textures/Environment/Tex_Grass.png";
        private const string NormalPath = "Assets/Textures/Environment/Tex_Grass_Normal.png";
        private const int Size = 1024;

        [MenuItem("Rapadura/Setup/Regenerate Ground Texture")]
        public static void Regenerate()
        {
            var baseColor = BuildBaseColor();
            var normal = BuildNormalFromHeight(baseColor);

            SaveTexture(baseColor, BaseColorPath);
            SaveTexture(normal, NormalPath, isNormalMap: true);

            AssetDatabase.Refresh();

            ApplyToMaterial();
            Debug.Log("[ProceduralGroundTexture] Regenerated ground base color + normal map, applied to Mat_Grass.");
        }

        private static Texture2D BuildBaseColor()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: true);

            // Two base greens (a duller "worn path" tone and a richer "healthy grass" tone) blended
            // by large-scale noise, with a finer high-frequency noise layered on top for per-pixel
            // mottling, plus sparse darker dirt/patch speckles — breaks up the single-flat-tile look
            // without needing any external texture.
            Color dullGreen = new Color(0.29f, 0.36f, 0.16f);
            Color richGreen = new Color(0.20f, 0.47f, 0.16f);
            Color dirt = new Color(0.32f, 0.26f, 0.16f);

            float largeScale = 6f;
            float fineScale = 40f;
            float speckleScale = 90f;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = (float)x / Size;
                    float v = (float)y / Size;

                    float patch = Mathf.PerlinNoise(u * largeScale, v * largeScale);
                    Color blended = Color.Lerp(dullGreen, richGreen, patch);

                    float fine = Mathf.PerlinNoise(u * fineScale + 100f, v * fineScale + 100f);
                    blended *= 0.9f + fine * 0.2f;

                    float speckle = Mathf.PerlinNoise(u * speckleScale + 500f, v * speckleScale + 500f);
                    if (speckle > 0.82f)
                    {
                        float t = Mathf.InverseLerp(0.82f, 1f, speckle);
                        blended = Color.Lerp(blended, dirt, t * 0.6f);
                    }

                    blended.a = 1f;
                    tex.SetPixel(x, y, blended);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D BuildNormalFromHeight(Texture2D baseColor)
        {
            // Derive a cheap height field from the same noise frequencies (not from baseColor's
            // actual pixels, to avoid color-driven bump artifacts), then Sobel it into a tangent-space
            // normal map — gives the ground subtle relief instead of a perfectly flat Lit surface.
            var heights = new float[Size, Size];
            float fineScale = 40f;
            float speckleScale = 90f;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = (float)x / Size;
                    float v = (float)y / Size;
                    float fine = Mathf.PerlinNoise(u * fineScale + 100f, v * fineScale + 100f);
                    float speckle = Mathf.PerlinNoise(u * speckleScale + 500f, v * speckleScale + 500f);
                    heights[x, y] = fine * 0.7f + speckle * 0.3f;
                }
            }

            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: true);
            const float strength = 2.2f;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float hL = heights[(x - 1 + Size) % Size, y];
                    float hR = heights[(x + 1) % Size, y];
                    float hD = heights[x, (y - 1 + Size) % Size];
                    float hU = heights[x, (y + 1) % Size];

                    Vector3 n = new Vector3((hL - hR) * strength, (hD - hU) * strength, 1f).normalized;
                    Color c = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private static void SaveTexture(Texture2D tex, string path, bool isNormalMap = false)
        {
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = isNormalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
        }

        private static void ApplyToMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Grass.mat");
            if (mat == null)
            {
                return;
            }

            var baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorPath);
            var normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath);

            mat.SetTexture("_BaseMap", baseTex);
            mat.SetTexture("_MainTex", baseTex);
            mat.SetTexture("_BumpMap", normalTex);
            mat.EnableKeyword("_NORMALMAP");
            mat.SetFloat("_BumpScale", 0.6f);

            // Halve the tiling now that the source texture is 4x higher resolution (1024 vs 256) —
            // keeps the per-tile world-space footprint roughly the same instead of making each tile
            // 4x larger on the ground.
            mat.SetTextureScale("_BaseMap", new Vector2(6f, 6f));
            mat.SetTextureScale("_BumpMap", new Vector2(6f, 6f));

            EditorUtility.SetDirty(mat);
        }
    }
}
