#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Prepara solo los nuevos PNG de casas, edificios y arboles del Nivel 2.
/// No modifica el Nivel 1, las escenas, ni los Tilemaps existentes.
/// </summary>
public static class TechnopolisNivel2ExtrasTiles
{
    private const string SourceRoot = "Assets/CiudadNivel2/Extras";
    private const string TilesRoot = SourceRoot + "/TilesGenerados";

    [MenuItem("Herramientas/Technopolis/Nivel 2/Preparar sprites y crear Tiles extra")]
    public static void Preparar()
    {
        if (!AssetDatabase.IsValidFolder(SourceRoot))
        {
            EditorUtility.DisplayDialog("Archivos no encontrados",
                "Copia la carpeta Assets/CiudadNivel2/Extras del ZIP a tu proyecto primero.", "OK");
            return;
        }
        EnsureFolder(TilesRoot);
        int total = 0;
        string[] ids = AssetDatabase.FindAssets("t:Texture2D", new []
        {
            SourceRoot + "/Casas", SourceRoot + "/Edificios", SourceRoot + "/Arboles"
        });

        foreach (string id in ids)
        {
            string path = AssetDatabase.GUIDToAssetPath(id);
            if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) continue;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            var spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            bool changed = importer.textureType != TextureImporterType.Sprite ||
                           importer.spriteImportMode != SpriteImportMode.Single ||
                           importer.spritePixelsPerUnit != 128f ||
                           importer.filterMode != FilterMode.Point ||
                           importer.mipmapEnabled ||
                           importer.textureCompression != TextureImporterCompression.Uncompressed ||
                           !importer.alphaIsTransparency ||
                           spriteSettings.spriteAlignment != (int)SpriteAlignment.Custom;
            if (changed)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.ReadTextureSettings(spriteSettings);
                spriteSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                spriteSettings.spritePivot = new Vector2(0.5f, 0f);
                importer.SetTextureSettings(spriteSettings);
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;
            string relDir = Path.GetDirectoryName(path.Substring(SourceRoot.Length + 1))
                .Replace('\\', '/');
            // Path.GetDirectoryName("Casas/Nombre.png") => "Casas".
            if (string.IsNullOrEmpty(relDir)) relDir = "Otros";
            string tileDir = TilesRoot + "/" + relDir;
            EnsureFolder(tileDir);
            string tilePath = tileDir + "/" + Path.GetFileNameWithoutExtension(path) + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, tilePath);
            }
            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = Tile.ColliderType.None; // Colliders ajustables por edificio.
            EditorUtility.SetDirty(tile);
            total++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Tiles preparados",
            "Listos " + total + " sprites.\nAbre Window > 2D > Tile Palette y arrastra los .asset desde " + TilesRoot + ".\n\n" +
            "No se modifico ninguna escena o capa existente.", "OK");
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Replace('\\','/').Split('/');
        string parent = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = parent + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(parent, parts[i]);
            parent = next;
        }
    }
}
#endif
