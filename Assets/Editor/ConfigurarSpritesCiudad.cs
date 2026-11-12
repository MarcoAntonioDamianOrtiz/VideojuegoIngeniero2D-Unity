#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ConfigurarSpritesCiudad
{
    [MenuItem("Herramientas/Ciudad Nivel 2/Configurar todos los sprites")]
    public static void Configurar()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] {"Assets/CiudadNivel2"});
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            bool changed = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.filterMode != FilterMode.Point
                || importer.textureCompression != TextureImporterCompression.Uncompressed
                || importer.spritePixelsPerUnit != 128f;
            if (!changed) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            count++;
        }
        Debug.Log("Sprites Ciudad Nivel 2 configurados: " + count);
    }
}
#endif
