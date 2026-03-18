using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExtractor : EditorWindow
{
    [MenuItem("Tools/Extract Sprites from Atlas")]
    static void ShowWindow()
    {
        GetWindow<SpriteExtractor>("Extract Sprites");
    }

    private Texture2D sourceTexture;

    void OnGUI()
    {
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Extract Sprites"))
        {
            if (sourceTexture == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a source texture.", "OK");
                return;
            }

            ExtractSpritesFromTexture(sourceTexture);
        }
    }

    private void ExtractSpritesFromTexture(Texture2D texture)
    {
        string texturePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

        if (importer == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to get TextureImporter for the selected texture.", "OK");
            return;
        }

        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            EditorUtility.DisplayDialog("Error", "Texture is not in Multiple sprite mode. Please set Sprite Mode to Multiple in the importer settings.", "OK");
            return;
        }

        // Remember the initial Read/Write state
        bool oldReadable = importer.isReadable;
        if (!oldReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        try
        {
            // Load all sprites from the texture
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            if (assets.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No sprites found in this texture. Did you slice it in the Sprite Editor?", "OK");
                return;
            }

            // Create a save folder next to the original texture
            string folderPath = Path.GetDirectoryName(texturePath) + "/" + texture.name + "_Extracted";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            int savedCount = 0;
            foreach (Object obj in assets)
            {
                if (obj is Sprite sprite)
                {
                    // Extract the sprite into a separate texture 
                    Texture2D extractedTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
                    extractedTex.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
                    extractedTex.Apply();

                    byte[] pngData = extractedTex.EncodeToPNG();
                    string filePath = folderPath + "/" + sprite.name + ".png";
                    File.WriteAllBytes(filePath, pngData);

                    // Free up memory
                    DestroyImmediate(extractedTex);
                    savedCount++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Exported {savedCount} sprites to:\n{folderPath}", "OK");
        }
        finally
        {
            // Restore the original Read/Write state
            if (!oldReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }
}