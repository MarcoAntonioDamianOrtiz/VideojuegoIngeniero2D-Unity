// TECHNOPOLIS / NIVEL 3
// Unity 2D Tilemap Editor required (Package Manager > 2D Tilemap Editor).
// Este script solo escribe en el paquete donde se encuentran sus Sprites y la carpeta Generado.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public static class TechnopolisNivel3Editor
{
    // Encontrar la ubicacion real del paquete aunque se haya movido a Assets/Technopolis/nivel3.
    // Se ejecuta en el Editor. No toca ninguna escena existente al cargar.
    static readonly string Root = DetectarCarpetaRaiz();
    static readonly string Sprites = Root + "/Sprites";
    static readonly string TilesDir = Root + "/Generado/Tiles";
    static readonly string PalettesDir = Root + "/Generado/TilePalettes";
    static readonly string PrefabsDir = Root + "/Generado/Prefabs";
    static readonly string ScenesDir = Root + "/Generado/Escenas";

    static string DetectarCarpetaRaiz()
    {
        // Preferimos la carpeta que contiene este mismo script y sus sprites.
        string[] ids = AssetDatabase.FindAssets("TechnopolisNivel3Editor t:MonoScript");
        foreach (string id in ids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(id).Replace('\\', '/');
            const string sufijo = "/Editor/TechnopolisNivel3Editor.cs";
            if (!ruta.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase)) continue;
            string raiz = ruta.Substring(0, ruta.Length - sufijo.Length);
            if (AssetDatabase.IsValidFolder(raiz + "/Sprites")) return raiz;
        }
        // Si el script y las imagenes se copiaron a carpetas distintas, buscar las comunes.
        string[] opciones = { "Assets/Technopolis/nivel3", "Assets/Technopolis_Nivel3" };
        foreach (string opcion in opciones)
            if (AssetDatabase.IsValidFolder(opcion + "/Sprites")) return opcion;
        return "Assets/Technopolis_Nivel3";
    }

    [MenuItem("Tools/Technopolis Nivel 3/1 - Crear Tile Palettes y prefabs")]
    public static void CrearRecursos()
    {
        try
        {
            EnsureDirectory(TilesDir);
            EnsureDirectory(PalettesDir);
            EnsureDirectory(PrefabsDir);
            EnsureDirectory(ScenesDir);
            var spritePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { Sprites })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p, StringComparer.Ordinal).ToList();
            if (spritePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Technopolis Nivel 3", "No se encontraron sprites en " + Sprites, "Cerrar");
                return;
            }

            foreach (string path in spritePaths) ConfigureImporter(path);
            AssetDatabase.Refresh();
            foreach (string path in spritePaths) CrearTile(path);

            var tilesTerreno = spritePaths.Where(p => p.Contains("/Terreno/")).ToList();
            var tilesEdificios = spritePaths.Where(p => p.Contains("/Edificios/")).ToList();
            var tilesDecoracion = spritePaths.Where(p => p.Contains("/Decoracion/")).ToList();

            CrearPaleta("01_Suelos_y_Naturaleza", tilesTerreno.Where(p =>
                TieneInicio(p, "Asfalto_", "Banquetas_", "Plaza_Piedra_", "Pasto_", "Tierra_Caminos_", "Escaleras_Techos_")).ToList(), 10, 1, 1);
            CrearPaleta("02_Calles_y_Cruces", tilesTerreno.Where(p =>
                TieneInicio(p, "Carreteras_", "Cruces_Peatonales_", "Senales_Viales_")).ToList(), 10, 1, 1);
            CrearPaleta("03_Parques_y_Plantas", tilesTerreno.Where(p => TieneInicio(p, "Setos_Vallas_")).Concat(
                tilesDecoracion.Where(p => TieneInicio(p, "Arbol_", "Arbusto_", "Seto_", "Maceta_", "Jardinera_", "Barril_Jardinera"))).ToList(), 6, 4, 4);
            CrearPaleta("04_Mobiliario_y_Servicios", tilesDecoracion.Where(p =>
                TieneInicio(p, "Banco_", "Mesa_", "Farola_", "Bolardo_", "Bicicletero", "Papelera_", "Basurero_", "Barril_Azul", "Caja_", "Cajas_", "Armario_", "Gabinete_", "Panel_", "Paneles_", "Terminal_", "Maquina_", "Parada_", "Valla_", "Puerta_", "Puesto_", "Aire_", "Extractor_", "Ventila_", "Antena_")).ToList(), 7, 4, 4);
            CrearPaleta("05_Senales_y_Monumentos", tilesDecoracion.Where(p =>
                TieneInicio(p, "Marca_", "Bandera_", "Senal_", "Rotulo_", "Pantalla_", "Tablon_", "Fuente_", "Estatua_")).ToList(), 6, 4, 4);
            CrearPaleta("06_Edificios", tilesEdificios, 4, 6, 8);

            foreach (string path in tilesEdificios) CrearPrefab(path, true);
            foreach (string path in tilesDecoracion)
                if (!Path.GetFileNameWithoutExtension(path).StartsWith("Marca_")) CrearPrefab(path, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Technopolis Nivel 3] Generados " + spritePaths.Count + " Tile assets, 6 paletas y prefabs en: " + Root + "/Generado");
            EditorUtility.DisplayDialog("Technopolis - Instalación lista",
                "Se han creado las Tile Palettes, los Tile assets y los prefabs.\n\n" +
                "Abre Window > 2D > Tile Palette y selecciona una de las 6 paletas.\n\n" +
                "Para ver un mapa de ejemplo: Tools > Technopolis Nivel 3 > 2 - Crear escena base.", "Aceptar");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("Error al configurar Nivel 3", "Revisa la Console de Unity. " + e.Message, "Cerrar");
        }
    }

    static void EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }

    static bool TieneInicio(string path, params string[] starts)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        return starts.Any(st => name.StartsWith(st, StringComparison.Ordinal));
    }

    static void ConfigureImporter(string assetPath)
    {
        var t = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (t == null) return;
        bool ground = assetPath.Contains("/Terreno/") || assetPath.Contains("/Marca_");

        // Unity 6: spriteAlignment belongs to TextureImporterSettings,
        // NOT directly to TextureImporter.
        var spriteSettings = new TextureImporterSettings();
        t.ReadTextureSettings(spriteSettings);
        int wantedAlignment = ground ? (int)SpriteAlignment.Center : (int)SpriteAlignment.Custom;
        Vector2 wantedPivot = ground ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);

        bool changed = t.textureType != TextureImporterType.Sprite ||
                       t.spriteImportMode != SpriteImportMode.Single ||
                       t.spritePixelsPerUnit != 96f ||
                       t.filterMode != FilterMode.Point ||
                       t.mipmapEnabled ||
                       t.textureCompression != TextureImporterCompression.Uncompressed ||
                       t.wrapMode != TextureWrapMode.Clamp ||
                       !t.alphaIsTransparency ||
                       spriteSettings.spriteAlignment != wantedAlignment ||
                       (spriteSettings.spritePivot - wantedPivot).sqrMagnitude > 0.000001f;

        if (!changed) return;

        t.textureType = TextureImporterType.Sprite;
        t.spriteImportMode = SpriteImportMode.Single;
        t.spritePixelsPerUnit = 96;
        t.filterMode = FilterMode.Point;
        t.mipmapEnabled = false;
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.wrapMode = TextureWrapMode.Clamp;
        t.alphaIsTransparency = true;

        // Read the effective settings AFTER selecting the texture type.
        t.ReadTextureSettings(spriteSettings);
        spriteSettings.spriteAlignment = wantedAlignment;
        spriteSettings.spritePivot = wantedPivot;
        t.SetTextureSettings(spriteSettings);
        t.SaveAndReimport();
    }

    static string TilePath(string png)
    {
        return TilesDir + "/" + Path.GetFileNameWithoutExtension(png) + ".asset";
    }

    static Tile CrearTile(string png)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(png);
        if (sprite == null) throw new InvalidOperationException("Falta sprite: " + png);
        string path = TilePath(png);
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }
        tile.sprite = sprite;
        tile.color = Color.white;
        tile.transform = Matrix4x4.identity;
        tile.flags = TileFlags.LockTransform;
        tile.colliderType = Tile.ColliderType.None;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    static void CrearPaleta(string nombre, List<string> sprites, int columns, int sx, int sy)
    {
        if (sprites.Count == 0) return;
        // API oficial de com.unity.2d.tilemap.editor: crea un palette prefab reconocible por Tile Palette.
        string expected = PalettesDir + "/" + nombre + ".prefab";
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(expected);
        if (palette == null)
        {
            palette = GridPaletteUtility.CreateNewPalette(PalettesDir, nombre,
                GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual,
                Vector3.one, GridLayout.CellSwizzle.XYZ);
        }
        if (palette == null) throw new InvalidOperationException("No se pudo crear paleta: " + nombre);
        string palettePath = AssetDatabase.GetAssetPath(palette);
        if (string.IsNullOrEmpty(palettePath)) throw new InvalidOperationException("No hay ruta de paleta para: " + nombre);
        var contents = PrefabUtility.LoadPrefabContents(palettePath);
        try
        {
            var tilemap = contents.GetComponentInChildren<Tilemap>(true);
            if (tilemap == null) throw new InvalidOperationException("Paleta sin Tilemap: " + nombre);
            tilemap.ClearAllTiles();
            for (int i = 0; i < sprites.Count; i++)
            {
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(TilePath(sprites[i]));
                var cell = new Vector3Int((i % columns) * sx, -(i / columns) * sy, 0);
                tilemap.SetTile(cell, tile);
            }
            tilemap.RefreshAllTiles();
            PrefabUtility.SaveAsPrefabAsset(contents, palettePath);
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
    }

    static string PrefabPath(string png)
    {
        string dir = png.Contains("/Edificios/") ? "Edificios" : "Decoracion";
        string target = PrefabsDir + "/" + dir;
        EnsureDirectory(target);
        return target + "/" + Path.GetFileNameWithoutExtension(png) + ".prefab";
    }

    static void CrearPrefab(string path, bool esEdificio)
    {
        string target = PrefabPath(path);
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(target);
        if (existing != null) return; // Never overwrite the user's edited prefabs.
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        var go = new GameObject(Path.GetFileNameWithoutExtension(path));
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = esEdificio ? 15 : 18;
            if (esEdificio) go.transform.localScale = Vector3.one * 1.65f;
            // Colliders cover only the feet or base of these tall sprites, not the facade.
            string n = go.name;
            bool col = esEdificio || n.StartsWith("Arbol_") || n.StartsWith("Farola_") ||
                       n.StartsWith("Senal_") || n.StartsWith("Fuente_") ||
                       n.StartsWith("Estatua_") || n.StartsWith("Parada_") ||
                       n.StartsWith("Puesto_") || n.StartsWith("Puerta_");
            if (col)
            {
                var box = go.AddComponent<BoxCollider2D>();
                float w = esEdificio ? 0.66f : 0.45f;
                float h = esEdificio ? 0.24f : 0.21f;
                box.size = new Vector2(Mathf.Max(0.28f, sprite.bounds.size.x * w), h);
                box.offset = new Vector2(0f, h * 0.5f);
            }
            PrefabUtility.SaveAsPrefabAsset(go, target);
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    [MenuItem("Tools/Technopolis Nivel 3/2 - Crear escena base del nivel 3")]
    public static void CrearEscena()
    {
        if (AssetDatabase.LoadAssetAtPath<Tile>(TilesDir + "/Pasto_01.asset") == null)
        {
            CrearRecursos();
            if (AssetDatabase.LoadAssetAtPath<Tile>(TilesDir + "/Pasto_01.asset") == null) return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string scenePath = ScenesDir + "/EscenaNivel3_DistritoDeSilicio_Demo.unity";
        if (File.Exists(scenePath))
        {
            bool overwrite = EditorUtility.DisplayDialog("Escena Nivel 3 ya existe",
                "La escena base ya existe. ¿Sobrescribirla? No afecta a otras escenas de tu proyecto.",
                "Sobrescribir demo", "Cancelar");
            if (!overwrite) return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("NIVEL_3_DISTRITO_DE_SILICIO");
        GameObject gridObj = new GameObject("Grid_Nivel3");
        gridObj.transform.SetParent(root.transform);
        var grid = gridObj.AddComponent<Grid>();
        grid.cellSize = Vector3.one;
        Tilemap terreno = MakeMap("00_Terreno_Base", gridObj, 0);
        Tilemap calles = MakeMap("01_Calles_y_Banquetas", gridObj, 1);
        Tilemap detalles = MakeMap("02_Pintura_Vial_y_Detalles", gridObj, 2);
        const int W = 64, H = 44;

        TileBase Find(string id) => AssetDatabase.LoadAssetAtPath<Tile>(TilesDir + "/" + id + ".asset");
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int variant = (Math.Abs(x * 73 + y * 131 + x * y * 17) % 7) + 1;
                terreno.SetTile(new Vector3Int(x,y,0), Find("Pasto_" + variant.ToString("00")));
            }

        // Sidewalk squares around roads, making a navigable block structure.
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                bool verticalWalk = x >= 12 && x <= 23 || x >= 40 && x <= 51;
                bool horizontalWalk = y >= 12 && y <= 22 || y >= 30 && y <= 40;
                if (verticalWalk || horizontalWalk)
                    calles.SetTile(new Vector3Int(x,y,0), Find("Banquetas_01"));
                bool verticalRoad = x >= 15 && x <= 20 || x >= 43 && x <= 48;
                bool horizontalRoad = y >= 15 && y <= 19 || y >= 33 && y <= 37;
                if (verticalRoad || horizontalRoad)
                {
                    string road = "Asfalto_" + (((x*19 + y*5) % 5) + 1).ToString("00");
                    calles.SetTile(new Vector3Int(x,y,0), Find(road));
                    if (verticalRoad && !horizontalRoad && (x == 17 || x == 46))
                        detalles.SetTile(new Vector3Int(x,y,0), Find("Asfalto_06"));
                    else if (horizontalRoad && !verticalRoad && y == 35)
                        detalles.SetTile(new Vector3Int(x,y,0), Find("Carreteras_08"));
                }
            }

        // Pedestrian central plaza. Directions link its entrances to major routes.
        for (int y = 22; y <= 30; y++)
            for (int x = 26; x <= 39; x++)
                calles.SetTile(new Vector3Int(x,y,0), Find("Plaza_Piedra_" + (((x+y)%5)+1).ToString("00")));
        for (int x=21; x<=42; x++)
            for (int y=25; y<=27; y++)
                if (!(x>=26 && x<=39)) calles.SetTile(new Vector3Int(x,y,0), Find("Banquetas_01"));
        foreach (int x in new[] { 16, 19, 44, 47 })
            foreach (int y in new []{14,20,32,38})
                for (int dy=0;dy<2;dy++) detalles.SetTile(new Vector3Int(x,y+dy,0),Find("Cruces_Peatonales_01"));
        foreach (int y in new[] {15,19,33,37})
            foreach (int x in new[]{13,21,41,49})
                detalles.SetTile(new Vector3Int(x,y,0),Find("Senales_Viales_11"));
        // Residential walkways (paintable with the included palette).
        for (int x=2;x<12;x++) for(int y=8;y<10;y++) calles.SetTile(new Vector3Int(x,y,0),Find("Tierra_Caminos_02"));
        for (int x=52;x<62;x++) for(int y=7;y<10;y++) calles.SetTile(new Vector3Int(x,y,0),Find("Plaza_Piedra_01"));

        GameObject buildings = NewGroup("03_Edificios",root);
        GameObject trees = NewGroup("04_Arboles_y_Jardines",root);
        GameObject street = NewGroup("05_Mobiliario_y_Senales",root);
        GameObject notes = NewGroup("06_Objetivos_de_Tu_Juego_Agregar_Aqui",root);

        // Four main landmarks corresponding to the reference art.
        Place("Edificio_Cybernetics_LTD", 7, 41, buildings);
        Place("Edificio_Global_Corp", 33, 41, buildings);
        Place("Edificio_Bitforce", 8, 12, buildings);
        Place("Edificio_Silicon_Alley", 33, 12, buildings);
        Place("Edificio_Byteforce_Corp", 56, 41, buildings);
        Place("Edificio_Clinica_Distrito", 56, 29, buildings);
        Place("Edificio_Hardware_Electronica", 56, 14, buildings);
        Place("Edificio_Conveniencia_24_7", 6, 29, buildings);
        Place("Edificio_Servicio_Tecnico", 6, 23, buildings);
        Place("Edificio_Departamentos", 25, 42, buildings);
        Place("Casa_Techo_Rojo", 5, 8, buildings);
        Place("Casa_Techo_Azul", 27, 8, buildings);
        Place("Casa_Techo_Gris", 54, 8, buildings);
        Place("Negocio_Fideos", 29, 24, buildings);
        Place("Edificio_Comunicaciones", 61, 42, buildings);

        Place("Fuente_Globo_Holografico", 33, 25, street);
        Place("Rotulo_Distrito_Silicio", 34, 1, street);
        Place("Senal_Direccional_Grande", 42, 24, street);
        Place("Parada_Autobus", 24, 36, street);
        Place("Tablon_Anuncios", 40, 27, street);
        Place("Pantalla_Info", 27, 27, street);
        Place("Estatua_Robot", 57, 5, street);
        Place("Puesto_Comida", 8, 5, street);
        Place("Senal_Azul_Flechas", 49, 25, street);
        Place("Farola_Clasica_01", 24, 20, street);
        Place("Farola_Clasica_02", 40, 21, street);
        Place("Farola_Clasica_01", 24, 31, street);
        Place("Farola_Clasica_02", 40, 31, street);
        Place("Banco_Madera_01", 30, 29, street);
        Place("Banco_Madera_02", 37, 29, street);
        Place("Banco_Madera_01", 30, 22, street);
        Place("Banco_Tecnologico", 37, 22, street);
        Place("Papelera_Reciclaje_Azul", 27, 23, street);
        Place("Maquina_Expendedora", 52, 22, street);
        foreach (var p in new [] {new Vector2(1,40),new Vector2(23,40),new Vector2(38,41),new Vector2(61,31),new Vector2(3,15),new Vector2(60,20),new Vector2(23,7),new Vector2(39,8),new Vector2(2,2),new Vector2(59,2)})
            Place("Arbol_Verde_Grande",p.x,p.y,trees);
        foreach (var p in new [] {new Vector2(24,28),new Vector2(41,29),new Vector2(2,32),new Vector2(52,4)})
            Place("Arbol_Rosa_Florido",p.x,p.y,trees);
        foreach(var p in new [] {new Vector2(27,21),new Vector2(38,21),new Vector2(28,30),new Vector2(38,30)})
            Place("Jardinera_Rosa",p.x,p.y,trees);

        var camObj = new GameObject("Camara_Nivel3_Demo");
        camObj.transform.position = new Vector3(32,22,-15);
        camObj.transform.SetParent(root.transform);
        var cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 30;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f,0.2f,0.24f,1);
        camObj.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();

        var marker = new GameObject("SPAWN_JUGADOR_Y_BIT");
        marker.transform.position = new Vector3(33f,4f,0f);
        marker.transform.SetParent(notes.transform);
        var hints = new GameObject("ARRASTRA_AQUI_TU_PERSONAJE_Y_PERRO_DESDE_TU_NIVEL_1");
        hints.transform.SetParent(notes.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,scenePath);
        AssetDatabase.Refresh();
        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        Debug.Log("[Technopolis Nivel 3] Escena creada: " + scenePath + ". No reemplaza tus escenas anteriores.");
        EditorUtility.DisplayDialog("Escena creada", "Abierta la escena de ejemplo del Nivel 3:\n" + scenePath +
            "\n\nUsa Tile Palette para modificar el suelo y arrastra los prefabs para colocar edificios. " +
            "Agrega tu propio jugador, Bit, objetivos y colisiones refinadas.", "Aceptar");
    }

    static Tilemap MakeMap(string name, GameObject parent, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        var tm = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = order;
        return tm;
    }

    static GameObject NewGroup(string name, GameObject parent)
    {
        var o = new GameObject(name);
        o.transform.SetParent(parent.transform);
        return o;
    }

    static GameObject Place(string n, float x, float y, GameObject parent)
    {
        string location = PrefabsDir + "/" + (n.StartsWith("Edificio_") || n.StartsWith("Casa_") || n.StartsWith("Negocio_") ? "Edificios/" : "Decoracion/") + n + ".prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(location);
        if (prefab == null) { Debug.LogWarning("Falta prefab " + location); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = n;
        go.transform.SetParent(parent.transform);
        go.transform.position = new Vector3(x,y,0);
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder = 100 + Mathf.RoundToInt((44-y)*10);
        return go;
    }
}
#endif
