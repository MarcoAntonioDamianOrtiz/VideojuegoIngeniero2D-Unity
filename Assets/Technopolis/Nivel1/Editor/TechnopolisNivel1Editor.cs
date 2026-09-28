#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Solo procesa los sprites de este paquete. No cambia escenas ni personajes.
public sealed class TechnopolisSpriteImportador : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(TechnopolisNivel1Editor.Sprites + "/", StringComparison.Ordinal) || assetPath.Contains("/06_Personajes_NPC/")) return;
        TechnopolisNivel1Editor.Ajustar((TextureImporter)assetImporter, assetPath);
    }
}

public static class TechnopolisNivel1Editor
{
    public const string Root = "Assets/Technopolis/Nivel1";
    public const string Sprites = Root + "/Sprites";
    private static bool EsTile(string path) => path.Contains("/01_Terreno/") || path.Contains("/02_Caminos/") || path.Contains("/03_Detalles/");

    public static void Ajustar(TextureImporter t, string path)
    {
        t.textureType = TextureImporterType.Sprite;
        t.spriteImportMode = SpriteImportMode.Single;
        t.spritePixelsPerUnit = path.Contains("/05_Edificios/") ? 16 : 32;
        t.filterMode = FilterMode.Point;
        t.mipmapEnabled = false;
        t.sRGBTexture = true;
        t.alphaSource = TextureImporterAlphaSource.FromInput;
        t.alphaIsTransparency = true;
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.crunchedCompression = false;
        t.npotScale = TextureImporterNPOTScale.None;
        t.wrapMode = TextureWrapMode.Clamp;
        t.maxTextureSize = 2048;
        var settings = new TextureImporterSettings();
        t.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = EsTile(path) ? new Vector2(.5f, .5f) : new Vector2(.5f, 0f);
        t.SetTextureSettings(settings);
        // Evita que un override antiguo comprima o reduzca estas texturas.
        foreach (string platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
            t.ClearPlatformTextureSettings(platform);
    }

    [MenuItem("Tools/Technopolis/Nivel 1/01 Configurar sprites y crear Tiles + Prefabs")]
    public static void ConfigurarAssets()
    {
        if (!AssetDatabase.IsValidFolder(Sprites)) return;
        Carpeta(Root + "/Materiales");
        string materialPath = Root + "/Materiales/Sprites_Technopolis.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (!material)
        {
            var shader = Shader.Find("Sprites/Default");
            if (!shader) { Debug.LogError("No se encontro Sprites/Default."); return; }
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        int count = 0, created = 0, preserved = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Sprites }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || path.Contains("/06_Personajes_NPC/")) continue;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) continue;
            Ajustar(importer, path);
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) { Debug.LogWarning("No se pudo importar: " + path); continue; }
            count++;
            string category = Path.GetFileName(Path.GetDirectoryName(path));
            string name = Path.GetFileNameWithoutExtension(path);
            string folder = Root + (EsTile(path) ? "/Tiles/" : "/Prefabs/") + category;
            Carpeta(folder);
            string dest = folder + "/" + name + (EsTile(path) ? ".asset" : ".prefab");
            // Conserva los colliders, materiales y ajustes del usuario en recursos existentes.
            if (AssetDatabase.LoadMainAssetAtPath(dest)) { preserved++; continue; }
            if (EsTile(path))
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.color = Color.white;
                tile.colliderType = Tile.ColliderType.None;
                AssetDatabase.CreateAsset(tile, dest);
            }
            else
            {
                var go = new GameObject(name);
                try
                {
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sharedMaterial = material;
                    sr.spriteSortPoint = SpriteSortPoint.Pivot;
                    sr.sortingOrder = 0;
                    // Colliders se ajustan a la huella del objeto al construir el nivel.
                    PrefabUtility.SaveAsPrefabAsset(go, dest);
                }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
            created++;
        }
        CrearPatrones(material);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Assets configurados", $"Sprites: {count}\nRecursos nuevos: {created}\nRecursos existentes conservados: {preserved}\n\nArrastra los Tiles a tu Tile Palette. Los prefabs se arrastran a la escena. Los bloques de suelo 4x4 estan en Prefabs/Patrones_Suelo. Ajusta las colisiones de objetos al construir el nivel.", "Aceptar");
    }

    private static void CrearPatrones(Material material)
    {
        string folder = Root + "/Prefabs/Patrones_Suelo";
        Carpeta(folder);
        foreach (string nombre in new[] { "pasto", "tierra", "grava", "asfalto", "concreto", "empedrado" })
        {
            string dest = folder + "/Patron_" + nombre + "_4x4.prefab";
            if (AssetDatabase.LoadMainAssetAtPath(dest)) continue;
            string cat = nombre == "concreto" || nombre == "empedrado" ? "02_Caminos" : "01_Terreno";
            var go = new GameObject("Patron_" + nombre + "_4x4");
            try
            {
                go.AddComponent<Grid>().cellSize = Vector3.one;
                var child = new GameObject("Suelo");
                child.transform.SetParent(go.transform, false);
                var map = child.AddComponent<Tilemap>();
                var renderer = child.AddComponent<TilemapRenderer>();
                renderer.sharedMaterial = material;
                renderer.sortingOrder = -30;
                bool completo = true;
                for (int fila = 0; fila < 4; fila++)
                for (int col = 0; col < 4; col++)
                {
                    string tilePath = Root + "/Tiles/" + cat + "/" + nombre + "_bloque_f" + fila + "_c" + col + ".asset";
                    var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                    if (!tile) { completo = false; continue; }
                    map.SetTile(new Vector3Int(col, 3 - fila, 0), tile);
                }
                if (completo) PrefabUtility.SaveAsPrefabAsset(go, dest);
                else Debug.LogWarning("Faltan tiles para el patron " + nombre);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }

    private static void Carpeta(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        Carpeta(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
