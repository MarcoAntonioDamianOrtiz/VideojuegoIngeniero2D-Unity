#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Completa las viviendas del barrio alrededor del plano principal. Cada casa
/// recibe un nombre propio para que la acción se pueda ejecutar varias veces.
/// </summary>
public static class TechnopolisCompletarBarrioNivel1
{
    private const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    private const string BackupFolder = "Assets/Scenes/Respaldos";
    private const string PrefabFolder = "Assets/Technopolis/Nivel1/Prefabs/05_Edificios/";

    private sealed class Lot
    {
        public readonly string Name;
        public readonly string Prefab;
        public readonly Vector3 Position;
        public readonly Vector3 Scale;

        public Lot(string name, string prefab, float x, float y, float scale)
        {
            Name = name;
            Prefab = prefab;
            Position = new Vector3(x, y, 0f);
            Scale = new Vector3(scale, scale, 1f);
        }
    }

    private static readonly Lot[] Lots =
    {
        new Lot("Barrio_Oeste_01", "vivienda_techo_lamina", -34f, -10f, 1.5f),
        new Lot("Barrio_Oeste_02", "vivienda_parches", -34f, -20f, 1.5f),
        new Lot("Barrio_Oeste_03", "vivienda_ladrillo", -24f, -21f, 1.5f),
        new Lot("Barrio_Sur_01", "vivienda_ladrillo", -34f, -33f, 1.5f),
        new Lot("Barrio_Sur_02", "vivienda_parches", -23f, -33f, 1.5f),
        new Lot("Barrio_Sur_03", "vivienda_techo_lamina", -13f, -33f, 1.5f),
        new Lot("Barrio_Sur_04", "vivienda_techo_lamina", -34f, -42f, 1.5f),
        new Lot("Barrio_Sur_05", "vivienda_ladrillo", -23f, -42f, 1.5f),
        new Lot("Barrio_Sur_06", "vivienda_parches", -13f, -42f, 1.5f),
        new Lot("Barrio_Sur_07", "vivienda_ladrillo", 0f, -41f, 1.5f),
        new Lot("Mercado_Comunitario", "centro_comunitario", 17.5f, 7f, 1.5f)
    };

    [MenuItem("Tools/Technopolis/Nivel 1/05 Completar viviendas del barrio")]
    public static void Completar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Sal de Play", "Detén el juego antes de colocar casas.", "Aceptar");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Abre el Nivel 1",
                "Abre Assets/Scenes/EscenaNivel1.unity y vuelve a intentarlo.", "Aceptar");
            return;
        }

        Transform parent = Find(scene, "Edificios", out int parentCount);
        if (parentCount != 1 || !parent || !parent.parent || parent.parent.name != "Grid")
        {
            EditorUtility.DisplayDialog("Falta la capa Edificios",
                "La escena necesita Grid > Edificios.", "Aceptar");
            return;
        }

        var prefabs = new GameObject[Lots.Length];
        var existing = new Transform[Lots.Length];
        for (int i = 0; i < Lots.Length; i++)
        {
            Lot lot = Lots[i];
            string path = PrefabFolder + lot.Prefab + ".prefab";
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefabs[i])
            {
                EditorUtility.DisplayDialog("Falta un edificio", "No existe " + path, "Aceptar");
                return;
            }
            existing[i] = Find(scene, lot.Name, out int count);
            if (count > 1)
            {
                EditorUtility.DisplayDialog("Casas duplicadas",
                    "Hay " + count + " objetos llamados " + lot.Name + ".", "Aceptar");
                return;
            }
        }

        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("No se pudo guardar", "La escena no se modificó.", "Aceptar");
            return;
        }
        if (!AssetDatabase.IsValidFolder(BackupFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = BackupFolder + "/EscenaNivel1_AntesBarrio_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(ScenePath, backup))
        {
            EditorUtility.DisplayDialog("No se pudo crear el respaldo",
                "La escena no se modificó. Revisa Assets/Scenes/Respaldos.", "Aceptar");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Completar viviendas del Nivel 1");
        int created = 0, adjusted = 0;
        for (int i = 0; i < Lots.Length; i++)
        {
            Lot lot = Lots[i];
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
                    Debug.LogError("No se pudo crear " + lot.Name + ". Respaldo: " + backup);
                    return;
                }
                go.name = lot.Name;
                Undo.RegisterCreatedObjectUndo(go, "Crear " + lot.Name);
                PrefabUtility.RecordPrefabInstancePropertyModifications(go);
                created++;
            }
            if (go.transform.parent != parent)
                Undo.SetTransformParent(go.transform, parent, "Agrupar " + lot.Name);
            Undo.RecordObject(go.transform, "Ubicar " + lot.Name);
            go.transform.SetPositionAndRotation(lot.Position, Quaternion.identity);
            go.transform.localScale = lot.Scale;
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("Revisa la escena",
                "Las casas están colocadas, pero Unity no pudo guardar. " +
                "Guarda con Ctrl+S. Respaldo: " + backup, "Aceptar");
            return;
        }

        EditorUtility.DisplayDialog("Barrio completado",
            "Casas nuevas: " + created + "; ajustadas: " + adjusted + ".\n" +
            "Se añadieron viviendas al oeste y sur, y el centro comunitario al norte.\n\n" +
            "Respaldo previo: " + backup, "Aceptar");
        Debug.Log("Technopolis: barrio completado. Respaldo: " + backup);
    }

    private static Transform Find(Scene scene, string name, out int count)
    {
        Transform found = null;
        count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
        {
            if (item.name != name) continue;
            found = item;
            count++;
        }
        return found;
    }
}
#endif
