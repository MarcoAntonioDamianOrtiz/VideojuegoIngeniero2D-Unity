#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Pinta el plano de suelo de Nivel 1 sobre la escena existente, sin tocar Jugador,
/// Edificios, Objetos ni Detalles. Las columnas y filas del plano son inclusivas,
/// empiezan en 1 y las filas aumentan de arriba hacia abajo.
/// </summary>
public static class TechnopolisPintarSueloNivel1
{
    private const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    private const string BackupFolder = "Assets/Scenes/Respaldos";
    private const string TilesRoot = "Assets/Technopolis/Nivel1/Tiles/";
    private const int Left = -40;
    private const int Top = 19;
    private const int Side = 64;

    [MenuItem("Tools/Technopolis/Nivel 1/02 Pintar suelo del plano 64x64")]
    public static void Pintar()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Abre la escena del Nivel 1",
                "Abre Assets/Scenes/EscenaNivel1.unity y vuelve a elegir esta opción.", "Aceptar");
            return;
        }

        Tilemap baseMap = BuscarMapa(scene, "suelo_base");
        Tilemap roadsMap = BuscarMapa(scene, "Caminos");
        if (!baseMap || !roadsMap || baseMap.layoutGrid != roadsMap.layoutGrid)
        {
            EditorUtility.DisplayDialog("Faltan Tilemaps",
                "La escena necesita suelo_base y Caminos como Tilemaps del mismo Grid.", "Aceptar");
            return;
        }

        // Comprueba todos los recursos antes de modificar o guardar la escena.
        TileBase[,] earth = CargarPatron("01_Terreno", "tierra");
        TileBase[,] asphalt = CargarPatron("01_Terreno", "asfalto");
        TileBase[,] gravel = CargarPatron("01_Terreno", "grava");
        TileBase[,] grass = CargarPatron("01_Terreno", "pasto");
        TileBase[,] stone = CargarPatron("02_Caminos", "empedrado");
        TileBase[,] concrete = CargarPatron("02_Caminos", "concreto");
        if (earth == null || asphalt == null || gravel == null || grass == null ||
            stone == null || concrete == null)
        {
            EditorUtility.DisplayDialog("Faltan Tiles",
                "Falta algún bloque 4x4 en Assets/Technopolis/Nivel1/Tiles. " +
                "Revisa Console y vuelve a intentarlo.", "Aceptar");
            return;
        }

        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("No se pudo guardar", "La escena no se modificó.", "Aceptar");
            return;
        }

        if (!AssetDatabase.IsValidFolder(BackupFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = BackupFolder + "/EscenaNivel1_AntesSuelo_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(ScenePath, backup))
        {
            EditorUtility.DisplayDialog("No se pudo crear el respaldo",
                "La escena no se modificó. Revisa que Assets/Scenes/Respaldos pueda escribirse.", "Aceptar");
            return;
        }

        Undo.RegisterCompleteObjectUndo(baseMap, "Pintar suelo base del Nivel 1");
        Undo.RegisterCompleteObjectUndo(roadsMap, "Pintar caminos del Nivel 1");

        // 64 x 64: x = -40..23, y = -44..19. No modifica casillas exteriores.
        // Se siguen las áreas dibujadas en el plano de referencia. Algunas
        // etiquetas del propio dibujo indican rangos distintos a los píxeles.
        var cells = new TileBase[Side * Side];
        for (int row = 1; row <= Side; row++)
        for (int col = 1; col <= Side; col++)
        {
            TileBase[,] pattern = earth;
            if (Dentro(col, row, 1, 14, 1, 14) ||
                Dentro(col, row, 55, 64, 36, 43) ||
                Dentro(col, row, 55, 64, 52, 64))
                pattern = asphalt;
            else if (Callejon(col, row) || TallerGrava(col, row))
                pattern = gravel;
            else if (Parque(col, row))
                pattern = grass;
            else if (HierbaDispersa(col, row))
                pattern = grass;

            // SetTilesBlock recibe las celdas en orden x, luego y ascendente.
            int index = (Side - row) * Side + (col - 1);
            cells[index] = Patron(pattern, col, row);
        }
        baseMap.SetTilesBlock(new BoundsInt(Left, Top - Side + 1, 0, Side, Side, 1), cells);

        // Reconstruye solo las 4096 casillas del plano en Caminos. Los demás
        // objetos, y los posibles Tiles fuera de este rectángulo, permanecen.
        var roads = new TileBase[Side * Side];
        for (int row = 1; row <= Side; row++)
        for (int col = 1; col <= Side; col++)
        {
            TileBase[,] pattern = null;
            if (Plaza(col, row)) pattern = stone;
            else if (Dentro(col, row, 35, 64, 28, 31) ||
                     Dentro(col, row, 52, 64, 50, 51)) pattern = concrete;
            else if (Dentro(col, row, 52, 64, 49, 49)) pattern = gravel;

            roads[(Side - row) * Side + (col - 1)] = pattern == null ? null : Patron(pattern, col, row);
        }
        roadsMap.SetTilesBlock(new BoundsInt(Left, Top - Side + 1, 0, Side, Side, 1), roads);

        TilemapRenderer baseRenderer = baseMap.GetComponent<TilemapRenderer>();
        TilemapRenderer roadsRenderer = roadsMap.GetComponent<TilemapRenderer>();
        if (baseRenderer)
        {
            Undo.RecordObject(baseRenderer, "Orden de suelo base");
            baseRenderer.sortingOrder = -30;
        }
        if (roadsRenderer)
        {
            Undo.RecordObject(roadsRenderer, "Orden de caminos");
            roadsRenderer.sortingOrder = -20;
        }

        baseMap.RefreshAllTiles();
        roadsMap.RefreshAllTiles();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("Revisa la escena",
                "El suelo está pintado, pero Unity no pudo guardar la escena. " +
                "Guárdala manualmente. El respaldo está en " + backup, "Aceptar");
            return;
        }

        EditorUtility.DisplayDialog("Suelo del Nivel 1 pintado",
            "Se pintaron las 4096 casillas del mapa y las zonas de Caminos.\n\n" +
            "Asfalto superior: columnas 1–14, filas 1–14.\n" +
            "Callejón: filas 15–23, con borde escalonado.\n" +
            "Plaza: columnas 31–53, filas 32–42, con abertura.\n" +
            "Parque: columnas 33–47, filas 44–51, con entrada.\n" +
            "Taller: borde de grava y patio de asfalto, filas 32–45.\n" +
            "Salida: columnas 55–64, filas 52–64.\n\n" +
            "Respaldo previo: " + backup, "Aceptar");
        Debug.Log("Technopolis: suelo 64x64 pintado. Respaldo previo: " + backup);
    }

    private static Tilemap BuscarMapa(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Tilemap map in root.GetComponentsInChildren<Tilemap>(true))
            if (map.name == name) return map;
        return null;
    }

    private static TileBase[,] CargarPatron(string folder, string name)
    {
        var pattern = new TileBase[4, 4];
        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
        {
            string path = TilesRoot + folder + "/" + name +
                          "_bloque_f" + row + "_c" + col + ".asset";
            pattern[row, col] = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (!pattern[row, col])
            {
                Debug.LogError("Falta el Tile: " + path);
                return null;
            }
        }
        return pattern;
    }

    private static bool Dentro(int col, int row, int left, int right, int top, int bottom)
    {
        return col >= left && col <= right && row >= top && row <= bottom;
    }

    private static bool Callejon(int col, int row)
    {
        // La grava sobresale dos casillas a la derecha arriba; abajo se estrecha.
        return Dentro(col, row, 1, 16, 15, 18) ||
               Dentro(col, row, 1, 13, 19, 21) ||
               Dentro(col, row, 1, 14, 22, 23);
    }

    private static bool TallerGrava(int col, int row)
    {
        return Dentro(col, row, 54, 64, 32, 35) ||
               Dentro(col, row, 54, 54, 36, 42) ||
               Dentro(col, row, 51, 54, 43, 43) ||
               Dentro(col, row, 51, 64, 44, 45);
    }

    private static bool Parque(int col, int row)
    {
        // Entrada de tierra de cuatro casillas en el borde norte.
        return Dentro(col, row, 33, 47, 44, 51) &&
               !Dentro(col, row, 38, 41, 44, 44);
    }

    private static bool Plaza(int col, int row)
    {
        // Paso de tierra centrado en el borde sur de la plaza.
        return Dentro(col, row, 31, 53, 32, 42) &&
               !Dentro(col, row, 38, 41, 42, 42);
    }

    private static bool HierbaDispersa(int col, int row)
    {
        // Pequeños toques de pasto fuera de las zonas de uso, reproducibles en cada ejecución.
        return (col * 73 + row * 137 + col * row * 11) % 53 == 7;
    }

    private static TileBase Patron(TileBase[,] pattern, int col, int row)
    {
        return pattern[(row - 1) % 4, (col - 1) % 4];
    }

}
#endif
