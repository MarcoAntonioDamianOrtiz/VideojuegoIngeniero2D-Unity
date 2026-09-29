#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Coloca los edificios principales de la referencia del Nivel 1. Si ya hay
/// una instancia con el mismo nombre en la escena, la ajusta sin duplicarla.
/// </summary>
public static class TechnopolisColocarEdificiosNivel1
{
    private const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    private const string BackupFolder = "Assets/Scenes/Respaldos";
    private const string PrefabFolder = "Assets/Technopolis/Nivel1/Prefabs/05_Edificios/";

    private sealed class Building
    {
        public readonly string Name;
        public readonly Vector3 Position;
        public readonly Vector3 Scale;

        public Building(string name, float x, float y, float scale)
        {
            Name = name;
            Position = new Vector3(x, y, 0f);
            Scale = new Vector3(scale, scale, 1f);
        }
    }

    // Escala visual considerando el espacio transparente en cada sprite y su
    // pivote inferior. La fachada de panadería y tienda queda sobre la banqueta.
    private static readonly Building[] Buildings =
    {
        new Building("casa_alex", -21f, -13f, 2f),
        new Building("panaderia_dona_ramona", 3f, -8f, 2f),
        new Building("tienda_miscelanea", 18.5f, -8f, 1.5f),
        new Building("taller_mecanico", 18.5f, -20f, 1.5f),
        new Building("vivienda_techo_lamina", -16f, 5f, 1.5f),
        new Building("vivienda_ladrillo", -5f, 3f, 1.5f),
        new Building("vivienda_parches", 8f, 5f, 1.5f)
    };

    [MenuItem("Tools/Technopolis/Nivel 1/03 Colocar edificios principales")]
    public static void Colocar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Sal de Play", "Detén el juego antes de colocar edificios.", "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Abre el Nivel 1",
                "Abre Assets/Scenes/EscenaNivel1.unity y vuelve a usar esta opción.", "Aceptar");
            return;
        }

        Transform buildingsParent = FindInScene(scene, "Edificios");
        if (!buildingsParent || !buildingsParent.GetComponent<UnityEngine.Tilemaps.Tilemap>() ||
            !buildingsParent.parent || buildingsParent.parent.name != "Grid")
        {
            EditorUtility.DisplayDialog("Falta la capa Edificios",
                "La escena necesita Grid > Edificios para agrupar las casas.", "Aceptar");
            return;
        }

        var prefabs = new GameObject[Buildings.Length];
        var existing = new Transform[Buildings.Length];
        for (int i = 0; i < Buildings.Length; i++)
        {
            string path = PrefabFolder + Buildings[i].Name + ".prefab";
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefabs[i])
            {
                EditorUtility.DisplayDialog("Falta un edificio", "No existe " + path, "Aceptar");
                return;
            }
            int count = CountInScene(scene, Buildings[i].Name, out existing[i]);
            if (count > 1)
            {
                EditorUtility.DisplayDialog("Hay edificios duplicados",
                    "Encontré " + count + " objetos llamados " + Buildings[i].Name +
                    ". Deja solo uno antes de ejecutar esta opción.", "Aceptar");
                return;
            }
        }

        // Guarda la versión del usuario, incluidas las casas que haya colocado
        // localmente, antes de tocar un solo Transform.
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("No se pudo guardar",
                "La escena no se modificó.", "Aceptar");
            return;
        }
        if (!AssetDatabase.IsValidFolder(BackupFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = BackupFolder + "/EscenaNivel1_AntesEdificios_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(ScenePath, backup))
        {
            EditorUtility.DisplayDialog("No se pudo crear el respaldo",
                "La escena no se modificó. Revisa Assets/Scenes/Respaldos.", "Aceptar");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Ubicar edificios del Nivel 1");
        int created = 0, adjusted = 0;
        for (int i = 0; i < Buildings.Length; i++)
        {
            Building spec = Buildings[i];
            GameObject go;
            if (existing[i])
            {
                go = existing[i].gameObject;
                adjusted++;
            }
            else
            {
                go = PrefabUtility.InstantiatePrefab(prefabs[i], scene) as GameObject;
                if (!go)
                {
                    Debug.LogError("No se pudo crear " + spec.Name + ". Respaldo: " + backup);
                    return;
                }
                Undo.RegisterCreatedObjectUndo(go, "Crear " + spec.Name);
                created++;
            }

            if (go.transform.parent != buildingsParent)
                Undo.SetTransformParent(go.transform, buildingsParent, "Agrupar " + spec.Name);
            Undo.RecordObject(go.transform, "Ajustar " + spec.Name);
            go.transform.SetPositionAndRotation(spec.Position, Quaternion.identity);
            go.transform.localScale = spec.Scale;
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("Revisa la escena",
                "Los edificios están colocados, pero Unity no pudo guardar. " +
                "Guarda con Ctrl+S. Respaldo: " + backup, "Aceptar");
            return;
        }

        EditorUtility.DisplayDialog("Edificios principales colocados",
            "Nuevos: " + created + "; ajustados: " + adjusted + ".\n" +
            "Casa de Alex, panadería, tienda, taller y tres viviendas vecinas.\n\n" +
            "Revisa la escena completa en la pestaña Scene.\n" +
            "Respaldo anterior: " + backup, "Aceptar");
        Debug.Log("Technopolis: edificios ubicados. Respaldo: " + backup);
    }

    private static Transform FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            if (item.name == name) return item;
        return null;
    }

    private static int CountInScene(Scene scene, string name, out Transform found)
    {
        int count = 0;
        found = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
        {
            if (item.name != name) continue;
            found = item;
            count++;
        }
        return count;
    }
}
#endif
