using System.IO;
using UnityEditor;
using UnityEngine;

public static class ArcadeTextureGenerator
{
    public static Texture2D SaveTexture(Texture2D tex, string assetPath)
    {
        string dir = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.WriteAllBytes(assetPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
    }

    public static Texture2D CreateQuarterTexture(string path)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        float c = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - c) / c;
                float dy = (y - c) / c;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                // Brushed silver base
                float brush = Mathf.Sin(r * 90f) * 0.03f + Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.06f;
                float shade = 0.78f + brush;

                // Outer reeded rim
                if (r > 0.88f && r <= 0.99f)
                {
                    float ridges = Mathf.Sin(angle * 48f) * 0.12f;
                    shade = 0.86f + ridges;
                }
                // Inner beveled ring
                else if (r > 0.78f && r <= 0.83f)
                {
                    shade = 0.62f;
                }
                // Embossed center eagle/profile silhouette ring
                else if (r < 0.52f)
                {
                    float emboss = Mathf.Sin(dx * 10f) * Mathf.Cos(dy * 10f) * 0.08f;
                    float ring = (r > 0.44f) ? -0.10f : 0.05f;
                    shade = 0.82f + emboss + ring;
                }

                shade = Mathf.Clamp01(shade);
                tex.SetPixel(x, y, new Color(shade, shade * 0.99f, shade * 0.96f, 1f));
            }
        }
        tex.Apply();
        return SaveTexture(tex, path);
    }

    public static Texture2D CreatePokerChipTexture(string path, Color bodyColor, Color stripeColor, Color centerColor)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        float c = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - c) / c;
                float dy = (y - c) / c;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx) + Mathf.PI; // 0..2pi

                float grain = (Mathf.PerlinNoise(x * 0.15f, y * 0.15f) - 0.5f) * 0.06f;
                Color col = bodyColor;

                // 6 classic casino edge spots
                float sector = (angle / (Mathf.PI * 2f)) * 6f;
                float frac = sector - Mathf.Floor(sector);
                if (r > 0.68f && frac > 0.35f && frac < 0.65f)
                {
                    col = stripeColor;
                }

                // Dashed inner ring
                float dashSector = (angle / (Mathf.PI * 2f)) * 18f;
                float dashFrac = dashSector - Mathf.Floor(dashSector);
                if (r > 0.56f && r < 0.63f && dashFrac > 0.25f && dashFrac < 0.75f)
                {
                    col = stripeColor;
                }

                // Center inlay medallion
                if (r < 0.50f)
                {
                    col = centerColor;
                    if (r > 0.44f) col = Color.Lerp(centerColor, bodyColor, 0.7f);
                    else if (r < 0.18f) col = Color.Lerp(centerColor, stripeColor, 0.35f);
                }

                col = new Color(
                    Mathf.Clamp01(col.r + grain),
                    Mathf.Clamp01(col.g + grain),
                    Mathf.Clamp01(col.b + grain),
                    1f);
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return SaveTexture(tex, path);
    }

    public static Texture2D CreateCashTexture(string path)
    {
        int w = 256, h = 128;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color billDark = new Color(0.16f, 0.38f, 0.20f);
        Color billLight = new Color(0.72f, 0.80f, 0.66f);
        Color strapCol = new Color(0.92f, 0.78f, 0.22f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)w;
                float v = y / (float)h;
                float paperNoise = (Mathf.PerlinNoise(x * 0.2f, y * 0.2f) - 0.5f) * 0.06f;

                Color col = billLight;

                // Outer ornate green border
                bool border = u < 0.08f || u > 0.92f || v < 0.14f || v > 0.86f;
                bool innerLine = (u > 0.11f && u < 0.89f && v > 0.19f && v < 0.81f) &&
                                 (u < 0.13f || u > 0.87f || v < 0.23f || v > 0.77f);
                if (border || innerLine) col = billDark;

                // Portrait oval in center
                float ox = (u - 0.5f) / 0.16f;
                float oy = (v - 0.5f) / 0.28f;
                float oval = ox * ox + oy * oy;
                if (oval < 1.0f && oval > 0.78f) col = billDark;
                else if (oval <= 0.78f) col = Color.Lerp(billLight, billDark, 0.35f);

                // Yellow paper currency band wrapped around middle
                if (u > 0.41f && u < 0.59f)
                {
                    col = strapCol;
                    if (u < 0.43f || u > 0.57f) col = strapCol * 0.78f;
                }

                col = new Color(
                    Mathf.Clamp01(col.r + paperNoise),
                    Mathf.Clamp01(col.g + paperNoise),
                    Mathf.Clamp01(col.b + paperNoise),
                    1f);
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return SaveTexture(tex, path);
    }

    public static Texture2D CreateGoldBarTexture(string path)
    {
        int w = 256, h = 128;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color goldBase = new Color(0.98f, 0.78f, 0.20f);
        Color goldDark = new Color(0.74f, 0.54f, 0.10f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)w;
                float v = y / (float)h;
                float brushed = (Mathf.PerlinNoise(x * 0.04f, y * 0.45f) - 0.5f) * 0.09f;

                Color col = goldBase;
                // Bevel edges
                if (u < 0.06f || u > 0.94f || v < 0.10f || v > 0.90f)
                {
                    col = goldDark;
                }
                // Stamped inner bullion inset border
                else if ((u > 0.12f && u < 0.88f && v > 0.20f && v < 0.80f) &&
                         (u < 0.15f || u > 0.85f || v < 0.25f || v > 0.75f))
                {
                    col = Color.Lerp(goldBase, goldDark, 0.65f);
                }

                col = new Color(
                    Mathf.Clamp01(col.r + brushed),
                    Mathf.Clamp01(col.g + brushed),
                    Mathf.Clamp01(col.b + brushed),
                    1f);
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return SaveTexture(tex, path);
    }

    public static Texture2D CreateBrushedMetalTexture(string path, Color baseColor, bool diamondPattern)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float streak = (Mathf.PerlinNoise(x * 0.02f, y * 0.35f) - 0.5f) * 0.08f;
                float pattern = 0f;
                if (diamondPattern)
                {
                    float gx = Mathf.Abs(((x % 32) / 32f) - 0.5f);
                    float gy = Mathf.Abs(((y % 32) / 32f) - 0.5f);
                    if (Mathf.Abs(gx + gy - 0.5f) < 0.06f) pattern = 0.09f;
                }

                Color col = new Color(
                    Mathf.Clamp01(baseColor.r + streak + pattern),
                    Mathf.Clamp01(baseColor.g + streak + pattern),
                    Mathf.Clamp01(baseColor.b + streak + pattern),
                    1f);
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return SaveTexture(tex, path);
    }
}
