#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Completa la ambientación exterior del Nivel 1 a partir del plano de referencia.
/// Solo reconstruye sus propias capas; no sustituye el suelo, las casas ni el Jugador.
/// </summary>
public static class TechnopolisAmbientacionNivel1
{
    private const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    private const string Root = "Assets/Technopolis/Nivel1/";
    private const string BackupFolder = "Assets/Scenes/Respaldos";
    private const string FloorName = "Ambientacion_Piso";
    private const string ParkFloorName = "Ambientacion_Parque";
    private const string PlazaFloorName = "Ambientacion_Plaza";
    private const string PathsFloorName = "Ambientacion_Senderos";
    private const string BuildingName = "Ambientacion_Edificios";
    private const string ObjectsName = "Ambientacion_PlanoReferencia";
    private const int Left = -40, Top = 19, Side = 64;

    private struct Placement
    {
        public string Name, Prefab;
        public Vector3 Position;
        public float Scale;
        public Placement(string name, string prefab, float x, float y, float scale = 1f)
        {
            Name = name;
            Prefab = prefab;
            Position = new Vector3(x, y, 0f);
            Scale = scale;
        }
    }

    // Los edificios principales del usuario permanecen donde están.
    private static readonly Placement[] ExtraBuildings =
    {
        new Placement("Casa_Norte_Oeste", "vivienda_techo_lamina", -31f, 8f, 1.2f),
        new Placement("Casa_Norte_Centro", "vivienda_parches", -23f, 12f, 1.05f),
        new Placement("Casa_Pasaje_Central", "vivienda_ladrillo", -11f, -4f, 1.05f),
        new Placement("Casa_Sur_Este", "vivienda_techo_lamina", 9f, -40f, 1f)
    };

    // Pequeños grupos colocados a mano alrededor de los puntos de interés.
    private static readonly Placement[] LandmarkProps =
    {
        new Placement("Mercado_Caja_01", "cajas_apiladas", -4f, 11f, 0.65f),
        new Placement("Mercado_Caja_02", "caja_madera", 4.5f, 11f, 0.8f),
        new Placement("Mercado_Tambo", "tambo_azul", 5.5f, 11f, 0.8f),
        new Placement("Mercado_Banco", "banco_viejo", -4f, 10f, 0.9f),
        new Placement("Mercado_Cerca_Huerto_01", "cerca_madera", 8f, 11.5f),
        new Placement("Mercado_Cerca_Huerto_02", "cerca_madera", 10f, 11.5f),
        new Placement("Mercado_Cerca_Huerto_03", "cerca_madera", 14f, 11.5f),
        new Placement("Mercado_Tambo_Huerto", "tambo_oxidado", 18f, 14f, 0.7f),

        new Placement("Callejon_Basura_01", "bolsas_basura", -37f, 4f, 0.85f),
        new Placement("Callejon_Basura_02", "bolsa_basura_negra", -30f, 1f, 0.75f),
        new Placement("Callejon_Basura_03", "bolsa_basura_negra", -35f, -4f, 0.75f),
        new Placement("Callejon_Basura_04", "bolsas_basura", -38f, -7f, 0.95f),
        new Placement("Callejon_Basura_05", "bolsa_basura_negra", -28f, 5f, 0.8f),
        new Placement("Callejon_Basura_06", "bolsas_basura", -33f, -3f, 0.8f),
        new Placement("Callejon_Caja_Extra", "caja_madera", -27f, 2f, 0.85f),
        new Placement("Callejon_Cajas", "cajas_apiladas", -28f, -3f, 0.72f),
        new Placement("Callejon_Tambo", "tambo_oxidado", -39f, -6f, 0.85f),
        new Placement("Callejon_Llanta", "llanta_vieja", -33f, 2f, 0.9f),
        new Placement("Callejon_Muro", "muro_ladrillo_roto", -39f, -9f, 0.9f),

        new Placement("CasaAlex_Cerca_01", "cerca_rota", -27f, -13.8f),
        new Placement("CasaAlex_Cerca_02", "cerca_rota", -25f, -13.8f),
        new Placement("CasaAlex_Banco", "banco_viejo", -18f, -15.6f, 0.8f),
        new Placement("CasaAlex_Tambo", "tambo_azul", -27.5f, -16.5f, 0.75f),
        new Placement("Vecinos_Tendedero", "tendedero", -20f, -5f, 0.85f),
        new Placement("Vecinos_Caja", "caja_madera", -7f, -7f, 0.85f),
        new Placement("Pasaje_Cerca_01", "cerca_rota", -15f, -5.5f, 0.8f),
        new Placement("Pasaje_Cerca_02", "cerca_rota", -7f, -5.5f, 0.8f),
        new Placement("Pasaje_Tambo", "tambo_azul", -15.5f, -3.5f, 0.7f),
        new Placement("Pasaje_Poste", "poste_luz", -13f, -10.5f, 0.8f),

        new Placement("Panaderia_Maceta_01", "maceta_reutilizada", -4.5f, -10f, 0.85f),
        new Placement("Panaderia_Maceta_02", "maceta_reutilizada", 9.5f, -10f, 0.85f),
        new Placement("Panaderia_Banco", "banco_viejo", 11.5f, -12.5f, 0.8f),
        new Placement("Tienda_Caja", "caja_madera", 15f, -11.5f, 0.8f),
        new Placement("Tienda_Bicicleta", "bicicleta_vieja", 21f, -11.5f, 0.7f),

        new Placement("Plaza_Banco_01", "banco_viejo", -8f, -17.5f, 0.8f),
        new Placement("Plaza_Banco_02", "banco_viejo", 7f, -20.8f, 0.8f),
        new Placement("Plaza_Poste_01", "poste_luz", -8.5f, -22.5f, 0.8f),
        new Placement("Plaza_Poste_02", "poste_luz", 10.5f, -21.8f, 0.8f),
        new Placement("Plaza_Maceta_01", "maceta_reutilizada", 8f, -15f, 0.8f),
        new Placement("Plaza_Maceta_02", "maceta_reutilizada", -9f, -14f, 0.8f),
        new Placement("Plaza_Bicicleta", "bicicleta_vieja", 9.5f, -18.2f, 0.65f),

        new Placement("Parque_Banco_Oeste", "banco_viejo", -7.5f, -31.5f, 0.75f),
        new Placement("Parque_Maceta", "maceta_reutilizada", 8f, -31f, 0.75f),
        new Placement("Parque_Poste", "poste_luz", 10f, -30f, 0.7f),
        new Placement("Parque_Cerca_Norte_01", "cerca_madera", -8f, -23.8f),
        new Placement("Parque_Cerca_Norte_02", "cerca_madera", -6f, -23.8f),
        new Placement("Parque_Cerca_Norte_03", "cerca_madera", 5f, -23.8f),
        new Placement("Parque_Cerca_Norte_04", "cerca_madera", 7f, -23.8f),

        new Placement("BarrioSur_Tendedero", "tendedero", -29f, -38f, 0.8f),
        new Placement("BarrioSur_Bicicleta", "bicicleta_vieja", -17f, -37f, 0.75f),
        new Placement("BarrioSur_Tambo", "tambo_azul", -9f, -39f, 0.75f),
        new Placement("BarrioSur_Cajas", "cajas_apiladas", 6f, -38.5f, 0.7f),
        new Placement("BarrioSur_Cerca_01", "cerca_rota", -38f, -36f, 0.85f),
        new Placement("BarrioSur_Cerca_02", "cerca_madera", -27f, -36f, 0.85f),
        new Placement("BarrioSur_Cerca_03", "cerca_rota", -18f, -36f, 0.85f),
        new Placement("BarrioSur_Poste", "poste_luz", -5.5f, -36f, 0.75f),
        new Placement("Taller_Papelera", "papelera", 13f, -26f, 0.8f),
        new Placement("Taller_Tambo", "tambo_oxidado", 22f, -25.5f, 0.85f),
        new Placement("Salida_Barricada_01", "valla_obras", 14.8f, -38.5f, 0.85f),
        new Placement("Salida_Senal_Extra", "senal_bloqueo", 20f, -36.5f, 0.8f)
    };

    private static readonly string[] Details =
    {
        "maleza_01", "maleza_02", "maleza_03", "piedras_sueltas",
        "papeles_tirados", "parche_tierra", "florcita_silvestre",
        "grieta_suelo", "mancha_aceite"
    };

    [MenuItem("Tools/Technopolis/Nivel 1/07 Completar ambientación del mapa")]
    public static void Completar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Sal de Play", "Detén el juego antes de editar el mapa.", "Aceptar");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Abre el Nivel 1", "Abre Assets/Scenes/EscenaNivel1.unity.", "Aceptar");
            return;
        }
        Transform grid = Find(scene, "Grid");
        Transform buildings = Find(scene, "Edificios");
        Transform objects = Find(scene, "Objetos");
        if (!grid || !buildings || !objects || buildings.parent != grid || objects.parent != grid)
        {
            EditorUtility.DisplayDialog("Falta el mapa", "Se necesitan Grid > Edificios y Grid > Objetos.", "Aceptar");
            return;
        }

        var prefabs = new Dictionary<string, GameObject>();
        foreach (Placement p in ExtraBuildings)
            if (!LoadPrefab("05_Edificios", p.Prefab, prefabs)) return;
        foreach (Placement p in LandmarkProps)
            if (!LoadPrefab("04_Objetos", p.Prefab, prefabs)) return;
        foreach (string name in new[] { "arbusto_seco", "arbol_barrio", "maceta_reutilizada" })
            if (!LoadPrefab("04_Objetos", name, prefabs)) return;
        var sprites = new Dictionary<string, Sprite>();
        foreach (string name in new[]
        {
            "columpios_viejos", "resbaladilla_vieja", "porton_salida_bloqueada", "muro_perimetral",
            "arbol_plaza_monumental", "contenedor_basura_callejon", "puesto_mercado_lona", "huerto_comunitario"
        })
        {
            string path = Root + "Sprites/04_Objetos/" + name + ".png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) { Missing(path); return; }
            sprites.Add(name, sprite);
        }
        var detailTiles = new TileBase[Details.Length];
        for (int i = 0; i < Details.Length; i++)
        {
            string path = Root + "Tiles/03_Detalles/" + Details[i] + ".asset";
            detailTiles[i] = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (!detailTiles[i]) { Missing(path); return; }
        }
        TileBase[,] grass = LoadPattern("01_Terreno", "pasto");
        TileBase[,] concrete = LoadPattern("02_Caminos", "concreto");
        if (grass == null || concrete == null) return;
        var pathTiles = new TileBase[5];
        for (int i = 0; i < pathTiles.Length; i++)
        {
            string path = Root + "Tiles/02_Caminos/camino_tierra_0" + (i + 1) + ".asset";
            pathTiles[i] = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (!pathTiles[i]) { Missing(path); return; }
        }

        if (!EditorSceneManager.SaveScene(scene)) { Missing("No se pudo guardar la escena"); return; }
        if (!AssetDatabase.IsValidFolder(BackupFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = BackupFolder + "/EscenaNivel1_AntesAmbientacion_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(ScenePath, backup)) { Missing("No se pudo crear el respaldo: " + backup); return; }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Completar ambientación del Nivel 1");

        Transform buildingGroup = ReplaceGroup(buildings, BuildingName);
        Transform objectGroup = ReplaceGroup(objects, ObjectsName);
        for (int i = 0; i < ExtraBuildings.Length; i++)
        {
            Placement p = ExtraBuildings[i];
            GameObject go = PlacePrefab(prefabs[p.Prefab], buildingGroup, p);
            var box = Undo.AddComponent<BoxCollider2D>(go);
            box.size = p.Prefab == "vivienda_parches" ? new Vector2(4f, 3.5f) :
                       p.Prefab == "vivienda_ladrillo" ? new Vector2(4.5f, 4f) : new Vector2(4.9f, 4f);
            box.offset = new Vector2(0f, box.size.y / 2f);
        }

        // El límite físico exterior ya lo crea la opción 06. El sprite tiene
        // 11 px transparentes debajo y encima; su borde visible exterior se
        // alinea con el rectángulo de 64 x 64, no con el pivote del sprite.
        for (int x = -36; x <= 20; x += 8)
        {
            PlaceSprite(sprites["muro_perimetral"], objectGroup, "Muro_Norte_" + x, x, 17.6875f, 2f);
            PlaceSprite(sprites["muro_perimetral"], objectGroup, "Muro_Sur_" + x, x, -44.6875f, 2f);
        }
        for (int y = -40; y <= 16; y += 8)
        {
            PlaceSprite(sprites["muro_perimetral"], objectGroup, "Muro_Oeste_" + y, -37.6875f, y, 2f, 90f);
            PlaceSprite(sprites["muro_perimetral"], objectGroup, "Muro_Este_" + y, 21.6875f, y, 2f, -90f);
        }

        GameObject gate = PlaceSprite(sprites["porton_salida_bloqueada"], objectGroup,
                                     "Salida_Distrito_Comercial", 19f, -41f, 1.25f);
        BoxCollider2D gateBox = Undo.AddComponent<BoxCollider2D>(gate);
        gateBox.size = new Vector2(5.6f, 0.48f);
        gateBox.offset = new Vector2(0f, 0.24f);
        GameObject swing = PlaceSprite(sprites["columpios_viejos"], objectGroup,
                                      "Parque_Columpios", -3.8f, -27.5f, 1f);
        AddSwingFeet(swing);
        GameObject slide = PlaceSprite(sprites["resbaladilla_vieja"], objectGroup,
                                      "Parque_Resbaladilla", 4.2f, -27f, 0.92f);
        BoxCollider2D slideBox = Undo.AddComponent<BoxCollider2D>(slide);
        slideBox.size = new Vector2(1.2f, 0.4f);
        slideBox.offset = new Vector2(0f, 0.2f);

        GameObject plazaTree = PlaceSprite(sprites["arbol_plaza_monumental"], objectGroup,
            "Plaza_Arbol_Central", -1f, -18.5f, 1f);
        CircleCollider2D treeBase = Undo.AddComponent<CircleCollider2D>(plazaTree);
        treeBase.radius = 1.2f;
        treeBase.offset = new Vector2(0f, 1.1f);

        GameObject dumpster = PlaceSprite(sprites["contenedor_basura_callejon"], objectGroup,
            "Callejon_Contenedor_Principal", -34f, 4f, 0.95f);
        BoxCollider2D dumpsterBase = Undo.AddComponent<BoxCollider2D>(dumpster);
        dumpsterBase.size = new Vector2(3.6f, 0.65f);
        dumpsterBase.offset = new Vector2(0f, 0.34f);
        PlaceSprite(sprites["puesto_mercado_lona"], objectGroup, "Mercado_Puesto_Lona", 0f, 13f, 1f);
        PlaceSprite(sprites["huerto_comunitario"], objectGroup, "Mercado_Huerto_01", 8f, 14f, 1f);
        PlaceSprite(sprites["huerto_comunitario"], objectGroup, "Mercado_Huerto_02", 13f, 16f, 0.9f);

        foreach (Placement p in LandmarkProps) PlacePrefab(prefabs[p.Prefab], objectGroup, p);
        AdjustEastBuildings(buildings);
        AddTrees(prefabs["arbol_barrio"], objectGroup, buildings);
        AddSmallGreenery(prefabs, objectGroup, buildings);
        AdjustPark(objects);
        int parkTiles = PaintPark(grid, grass);
        int plazaTiles = PaintPlaza(grid, concrete);
        int pathCount = PaintPaths(grid, pathTiles, buildings);
        int details = PaintDetails(grid, detailTiles);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("Guarda la escena", "La ambientación se creó; guarda con Ctrl+S. Respaldo: " + backup, "Aceptar");
            return;
        }
        EditorUtility.DisplayDialog("Ambientación terminada",
            "Muros alineados con el borde del mapa, parque (" + parkTiles + " casillas), plaza (" + plazaTiles +
            " casillas) y senderos (" + pathCount + " casillas). Se añadió una casa de pasaje y detalles de patios.\n" +
            "Detalles de suelo: " + details + ". Los edificios principales y el personaje se conservaron.\n\n" +
            "Recorre callejón, plaza, parque y salida en Play.\nRespaldo: " + backup, "Aceptar");
        Debug.Log("Technopolis: ambientación del Nivel 1. Respaldo: " + backup);
    }

    private static bool LoadPrefab(string folder, string name, Dictionary<string, GameObject> cache)
    {
        if (cache.ContainsKey(name)) return true;
        string path = Root + "Prefabs/" + folder + "/" + name + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!prefab) { Missing(path); return false; }
        cache.Add(name, prefab);
        return true;
    }

    private static TileBase[,] LoadPattern(string folder, string name)
    {
        var tiles = new TileBase[4, 4];
        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
        {
            string path = Root + "Tiles/" + folder + "/" + name +
                          "_bloque_f" + row + "_c" + col + ".asset";
            tiles[row, col] = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (!tiles[row, col]) { Missing(path); return null; }
        }
        return tiles;
    }

    private static void Missing(string path)
    {
        EditorUtility.DisplayDialog("Falta un recurso", path + "\nNo se modificó el mapa.", "Aceptar");
    }

    private static Transform ReplaceGroup(Transform parent, string name)
    {
        Transform old = parent.Find(name);
        if (old) Undo.DestroyObjectImmediate(old.gameObject);
        GameObject group = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(group, "Crear " + name);
        Undo.SetTransformParent(group.transform, parent, "Agrupar " + name);
        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        group.transform.localScale = Vector3.one;
        return group.transform;
    }

    private static GameObject PlacePrefab(GameObject prefab, Transform parent, Placement p)
    {
        GameObject go = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        Undo.RegisterCreatedObjectUndo(go, "Añadir " + p.Name);
        go.name = p.Name;
        go.transform.position = p.Position;
        go.transform.localScale = new Vector3(p.Scale, p.Scale, 1f);
        return go;
    }

    private static GameObject PlaceSprite(Sprite sprite, Transform parent, string name,
                                           float x, float y, float scale, float angle = 0f)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Añadir " + name);
        Undo.SetTransformParent(go.transform, parent, "Agrupar " + name);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(scale, scale, 1f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        SpriteRenderer sr = Undo.AddComponent<SpriteRenderer>(go);
        sr.sprite = sprite;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        sr.sortingOrder = name.StartsWith("Muro_", StringComparison.Ordinal) ? -1 : 0;
        return go;
    }

    private static void AddSwingFeet(GameObject swing)
    {
        for (int i = -1; i <= 1; i += 2)
        {
            BoxCollider2D foot = Undo.AddComponent<BoxCollider2D>(swing);
            foot.size = new Vector2(0.3f, 0.35f);
            foot.offset = new Vector2(i * 1.75f, 0.18f);
        }
    }

    private static void AddTrees(GameObject prefab, Transform parent, Transform buildings)
    {
        Vector2[] positions =
        {
            new Vector2(-37f, 14f), new Vector2(-14f, 15f), new Vector2(8f, 15f),
            new Vector2(21f, 15f), new Vector2(-38f, -12f), new Vector2(-38f, -27f),
            new Vector2(-36f, -38f), new Vector2(-8f, -36f), new Vector2(9f, -36f),
            new Vector2(-11f, -2f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2 at = positions[i];
            if (NearBuilding(buildings, at, 1.5f)) continue;
            PlacePrefab(prefab, parent, new Placement("Decor_Arbol_" + i, "arbol_barrio", at.x, at.y, 0.78f));
        }
    }

    private static void AddSmallGreenery(Dictionary<string, GameObject> prefabs,
                                           Transform parent, Transform buildings)
    {
        int count = 0;
        for (int y = -39; y <= 16; y += 3)
        for (int x = -37; x <= 21; x += 3)
        {
            int hash = Hash(x, y);
            if (hash % 10 > 1 || NearBuilding(buildings, new Vector2(x, y), 0.6f)) continue;
            // Mantener libres el centro de la plaza, el camino y los juegos.
            if (x >= -9 && x <= 11 && y >= -31 && y <= -12) continue;
            string kind = hash % 3 == 0 ? "maceta_reutilizada" : "arbusto_seco";
            PlacePrefab(prefabs[kind], parent,
                new Placement("Decor_Vegetacion_" + count++, kind, x + 0.35f, y - 0.25f, 0.55f));
        }
    }

    private static bool NearBuilding(Transform buildings, Vector2 point, float margin)
    {
        foreach (Collider2D box in buildings.GetComponentsInChildren<Collider2D>())
        {
            Bounds b = box.bounds;
            if (point.x >= b.min.x - margin && point.x <= b.max.x + margin &&
                point.y >= b.min.y - margin && point.y <= b.max.y + margin) return true;
        }
        return false;
    }

    private static void AdjustEastBuildings(Transform buildings)
    {
        // Solo se desplazan instancias aún en las coordenadas que pusieron
        // las opciones 03 y 05. Respeta los ajustes manuales de la escena.
        MoveIfAt(buildings, "tienda_miscelanea", 18.5f, -8f, 17f, -8f);
        MoveIfAt(buildings, "taller_mecanico", 18.5f, -20f, 17f, -20f);
        MoveIfAt(buildings, "Mercado_Comunitario", 17.5f, 7f, 17f, 7f);
    }

    private static void MoveIfAt(Transform parent, string name, float oldX, float oldY, float x, float y)
    {
        Transform item = parent.Find(name);
        if (!item || Mathf.Abs(item.position.x - oldX) > 0.05f ||
            Mathf.Abs(item.position.y - oldY) > 0.05f) return;
        MoveExisting(parent, name, x, y);
    }

    private static void AdjustPark(Transform objects)
    {
        MoveExisting(objects, "arbol_barrio", -8.5f, -29.5f);
        MoveExisting(objects, "decor_parque_arbol_02", 8.3f, -31.2f);
        MoveExisting(objects, "fuente_pequena", 0f, -30.5f);
        MoveExisting(objects, "decor_plaza_arbol", -10.5f, -21.5f);
        string[] fences =
        {
            "cerca_madera", "decor_parque_cerca_01", "decor_parque_cerca_02",
            "decor_parque_cerca_03", "decor_parque_cerca_04", "decor_parque_cerca_05",
            "decor_parque_cerca_06", "decor_parque_cerca_07"
        };
        foreach (string name in fences)
        {
            Transform fence = objects.Find(name);
            if (fence) MoveExisting(objects, name, fence.position.x, -33.5f);
        }
        // La valla anterior cruzaba todo el borde sur. Abrir tres tramos deja
        // una entrada caminable entre el parque y las viviendas.
        foreach (string name in new[] { "decor_parque_cerca_03", "decor_parque_cerca_04", "decor_parque_cerca_05" })
        {
            Transform fence = objects.Find(name);
            if (!fence) continue;
            Undo.RecordObject(fence.gameObject, "Abrir acceso del parque");
            fence.gameObject.SetActive(false);
        }
    }

    private static void MoveExisting(Transform parent, string name, float x, float y)
    {
        Transform item = parent.Find(name);
        if (!item) return;
        Undo.RecordObject(item, "Acomodar " + name);
        item.position = new Vector3(x, y, item.position.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(item);
    }

    private static int PaintPark(Transform grid, TileBase[,] grass)
    {
        Tilemap map = GetFloorMap(grid, ParkFloorName, -25);
        Undo.RegisterCompleteObjectUndo(map, "Ampliar césped del parque");
        var cells = new TileBase[Side * Side];
        int count = 0;
        for (int row = 1; row <= Side; row++)
        for (int col = 1; col <= Side; col++)
        {
            int x = Left + col - 1, y = Top - row + 1;
            bool inside = x >= -9 && x <= 10 && y >= -34 && y <= -24;
            if ((y == -24 && x >= -2 && x <= 2) ||
                (y == -34 && x >= -3 && x <= 1) ||
                ((x == -9 || x == 10) && (y == -24 || y == -34))) inside = false;
            if (inside)
            {
                cells[(Side - row) * Side + col - 1] = grass[(row - 1) % 4, (col - 1) % 4];
                count++;
            }
        }
        map.SetTilesBlock(new BoundsInt(Left, Top - Side + 1, 0, Side, Side, 1), cells);
        map.RefreshAllTiles();
        return count;
    }

    private static int PaintPlaza(Transform grid, TileBase[,] concrete)
    {
        Tilemap map = GetFloorMap(grid, PlazaFloorName, -19);
        Undo.RegisterCompleteObjectUndo(map, "Concreto envejecido de la plaza");
        var cells = new TileBase[Side * Side];
        int count = 0;
        for (int row = 1; row <= Side; row++)
        for (int col = 1; col <= Side; col++)
        {
            int x = Left + col - 1, y = Top - row + 1;
            bool inside = x >= -10 && x <= 12 && y >= -22 && y <= -11;
            if ((y == -11 && (x < -8 || x > 9)) ||
                (y == -22 && x >= -2 && x <= 2) ||
                (x == 12 && (y > -14 || y < -20))) inside = false;
            if (inside)
            {
                cells[(Side - row) * Side + col - 1] = concrete[(row - 1) % 4, (col - 1) % 4];
                count++;
            }
        }
        map.SetTilesBlock(new BoundsInt(Left, Top - Side + 1, 0, Side, Side, 1), cells);
        map.RefreshAllTiles();
        return count;
    }

    private static int PaintPaths(Transform grid, TileBase[] pathTiles, Transform buildings)
    {
        Tilemap map = GetFloorMap(grid, PathsFloorName, -18);
        Tilemap roads = grid.Find("Caminos") ? grid.Find("Caminos").GetComponent<Tilemap>() : null;
        Undo.RegisterCompleteObjectUndo(map, "Trazar senderos del barrio");
        var cells = new TileBase[Side * Side];
        int count = 0;
        for (int row = 1; row <= Side; row++)
        for (int col = 1; col <= Side; col++)
        {
            int x = Left + col - 1, y = Top - row + 1;
            // El pasaje norte enlaza el callejón con las viviendas y baja
            // suavemente hacia la plaza. Los patios sur conservan un paso fino.
            int northY = -2 - (x >= -10 ? 1 : 0);
            bool northLane = x >= -27 && x <= 11 && y >= northY && y <= northY + 1;
            bool westSpur = x >= -16 && x <= -15 && y >= -12 && y <= -4;
            bool southLane = x >= -38 && x <= -10 && y == -35;
            bool parkEntry = x >= -2 && x <= 2 && y >= -30 && y <= -22;
            bool marketEntry = x >= -2 && x <= 1 && y >= 8 && y <= 11;
            if (!northLane && !westSpur && !southLane && !parkEntry && !marketEntry) continue;
            if (roads && roads.HasTile(new Vector3Int(x, y, 0))) continue;
            if (NearBuilding(buildings, new Vector2(x + 0.5f, y + 0.5f), 0.25f)) continue;
            int variation = Hash(x / 2, y / 2) % pathTiles.Length;
            cells[(Side - row) * Side + col - 1] = pathTiles[variation];
            count++;
        }
        map.SetTilesBlock(new BoundsInt(Left, Top - Side + 1, 0, Side, Side, 1), cells);
        map.RefreshAllTiles();
        return count;
    }

    private static Tilemap GetFloorMap(Transform grid, string name, int order)
    {
        Transform existing = grid.Find(name);
        Tilemap map;
        if (!existing)
        {
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Crear " + name);
            Undo.SetTransformParent(go.transform, grid, "Agrupar " + name);
            go.transform.localPosition = Vector3.zero;
            map = Undo.AddComponent<Tilemap>(go);
            Undo.AddComponent<TilemapRenderer>(go);
        }
        else
        {
            map = existing.GetComponent<Tilemap>();
            if (!map) map = Undo.AddComponent<Tilemap>(existing.gameObject);
        }
        TilemapRenderer renderer = map.GetComponent<TilemapRenderer>();
        if (!renderer) renderer = Undo.AddComponent<TilemapRenderer>(map.gameObject);
        Undo.RecordObject(renderer, "Orden de " + name);
        renderer.sortingOrder = order;
        return map;
    }

    private static int PaintDetails(Transform grid, TileBase[] tiles)
    {
        Transform existing = grid.Find(FloorName);
        Tilemap map;
        if (!existing)
        {
            GameObject go = new GameObject(FloorName);
            Undo.RegisterCreatedObjectUndo(go, "Crear detalles de suelo");
            Undo.SetTransformParent(go.transform, grid, "Agrupar detalles de suelo");
            go.transform.localPosition = Vector3.zero;
            map = Undo.AddComponent<Tilemap>(go);
            Undo.AddComponent<TilemapRenderer>(go);
        }
        else
        {
            map = existing.GetComponent<Tilemap>();
            if (!map) map = Undo.AddComponent<Tilemap>(existing.gameObject);
        }
        TilemapRenderer renderer = map.GetComponent<TilemapRenderer>();
        if (!renderer) renderer = Undo.AddComponent<TilemapRenderer>(map.gameObject);
        Undo.RecordObject(renderer, "Orden de detalles de suelo");
        renderer.sortingOrder = -10;
        Undo.RegisterCompleteObjectUndo(map, "Pintar detalles del barrio");

        var cells = new TileBase[Side * Side];
        int count = 0;
        for (int row = 1; row <= Side; row++)
        for (int col = 1; col <= Side; col++)
        {
            int x = Left + col - 1, y = Top - row + 1;
            int hash = Hash(x, y);
            bool plaza = x >= -10 && x <= 12 && y >= -22 && y <= -12;
            bool park = x >= -8 && x <= 8 && y >= -31 && y <= -24;
            bool alley = x <= -25 && y >= -5 && y <= 7;
            bool exit = x >= 14 && y <= -32;
            int chance = alley ? 300 : park ? 220 : plaza ? 85 : exit ? 95 : 150;
            TileBase tile = null;
            if (hash % 1000 < chance)
            {
                int index = hash / 1000 % 7;
                if (plaza) index = hash % 2 == 0 ? 7 : 4;
                else if (park) index = hash % 3 == 0 ? 6 : hash % 2 == 0 ? 0 : 1;
                else if (alley) index = hash % 3 == 0 ? 4 : hash % 2 == 0 ? 3 : 8;
                else if (exit) index = hash % 2 == 0 ? 7 : 8;
                tile = tiles[index];
                count++;
            }
            cells[(Side - row) * Side + (col - 1)] = tile;
        }
        map.SetTilesBlock(new BoundsInt(Left, Top - Side + 1, 0, Side, Side, 1), cells);
        map.RefreshAllTiles();
        return count;
    }

    private static int Hash(int x, int y)
    {
        unchecked
        {
            uint v = (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ 0x9e3779b9u;
            v ^= v >> 16;
            v *= 0x7feb352du;
            v ^= v >> 15;
            return (int)(v & 0x7fffffffu);
        }
    }

    private static Transform Find(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            if (item.name == name) return item;
        return null;
    }
}
#endif
