#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class RoundedSpriteGenerator
{
    [MenuItem("IdleBiz/Generate UI/Rounded 16px")]
    public static void GenerateRounded16()
    {
        GenerateRounded("Assets/_Project/Resources/UI", "Rounded_16px.png", 64, 16);
    }

    [MenuItem("IdleBiz/Generate UI/Rounded 24px")]
    public static void GenerateRounded24()
    {
        GenerateRounded("Assets/_Project/Resources/UI", "Rounded_24px.png", 64, 24);
    }

    static void GenerateRounded(string folder, string file, int size, int radius)
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
            AssetDatabase.CreateFolder("Assets/_Project", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/UI"))
            AssetDatabase.CreateFolder("Assets/_Project/Resources", "UI");

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var col = new Color32(255, 255, 255, 255);
        var clear = new Color32(0, 0, 0, 0);

        // nupieðiam apvalintà kvadratà
        int r2 = radius * radius;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool inLeft = x >= radius || (x - radius) * (x - radius) + (y - radius) * (y - radius) <= r2 && y >= radius;
                bool inRight = x < size - radius || (x - (size - radius - 1)) * (x - (size - radius - 1)) + (y - radius) * (y - radius) <= r2 && y >= radius;
                bool inTop = y >= radius || (y - radius) * (y - radius) + (x - radius) * (x - radius) <= r2 && x >= radius;
                bool inBottom = y < size - radius || (y - (size - radius - 1)) * (y - (size - radius - 1)) + (x - radius) * (x - radius) <= r2 && x >= radius;

                bool inside = (x >= radius && x < size - radius) || (y >= radius && y < size - radius) || (inLeft && inTop) || (inRight && inTop) || (inLeft && inBottom) || (inRight && inBottom);
                tex.SetPixel(x, y, inside ? col : clear);
            }
        tex.Apply();

        var path = Path.Combine(folder, file).Replace("\\", "/");
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteBorder = new Vector4(radius, radius, radius, radius); // 9-slice
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        Debug.Log($"[RoundedSpriteGenerator] Generated {path} with radius {radius}px");
    }
}
#endif
