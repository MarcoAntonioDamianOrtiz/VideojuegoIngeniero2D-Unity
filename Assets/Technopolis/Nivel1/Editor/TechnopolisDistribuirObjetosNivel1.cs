#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Distribuye los objetos del paquete por zonas del Nivel 1. Reutiliza las
/// instancias que el usuario ya arrastró a la escena y no duplica al reejecutar.
/// </summary>
public static class TechnopolisDistribuirObjetosNivel1
{
    private const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    private const string BackupFolder = "Assets/Scenes/Respaldos";
    private const string PrefabFolder = "Assets/Technopolis/Nivel1/Prefabs/04_Objetos/";
    private const string InteriorName = "Interiores_Pendientes";

    private sealed class Placement
    {
        public readonly string Prefab;
        public readonly string Name;
        public readonly Vector3 Position;
        public readonly Vector3 Scale;

        public Placement(string prefab, float x, float y, float size = 1f, string name = null)
        {
            Prefab = prefab;
            Name = name ?? prefab;
            Position = new Vector3(x, y, 0f);
            Scale = new Vector3(size, size, 1f);
        }
    }

    // Las posiciones usan el mismo Grid de 64 x 64 que el plano de suelo.
    // Se deja libre el paso entre la plaza y el parque (x = -3..0, y = -23).
    private static readonly Placement[] Outdoor =
    {
        // Parque y su acceso.
        new Placement("arbol_barrio", -6f, -29.5f),
        new Placement("arbusto_seco", 6f, -26.5f),
        new Placement("banco_viejo", 5f, -30f),
        new Placement("fuente_pequena", 0f, -30f, 1.25f),
        new Placement("cerca_madera", -8f, -31f),
        new Placement("arbol_barrio", 6f, -27f, 1f, "decor_parque_arbol_02"),
        new Placement("cerca_madera", -6f, -31f, 1f, "decor_parque_cerca_01"),
        new Placement("cerca_madera", -4f, -31f, 1f, "decor_parque_cerca_02"),
        new Placement("cerca_madera", -2f, -31f, 1f, "decor_parque_cerca_03"),
        new Placement("cerca_madera", 0f, -31f, 1f, "decor_parque_cerca_04"),
        new Placement("cerca_madera", 2f, -31f, 1f, "decor_parque_cerca_05"),
        new Placement("cerca_madera", 4f, -31f, 1f, "decor_parque_cerca_06"),
        new Placement("cerca_madera", 6f, -31f, 1f, "decor_parque_cerca_07"),

        // Plaza del barrio, sin tapar los accesos.
        new Placement("arbol_barrio", -4f, -19f, 1f, "decor_plaza_arbol"),
        new Placement("banco_viejo", 0f, -21f, 1f, "decor_plaza_banco"),
        new Placement("bicicleta_vieja", -11.5f, -18f),
        new Placement("poste_luz", -9f, -21f),
        new Placement("papelera", 9f, -21f),

        // Callejón: basura, cajas y muro viejo.
        new Placement("bolsa_basura_negra", -31f, 1f),
        new Placement("bolsas_basura", -34f, -1f),
        new Placement("caja_madera", -36f, 2f),
        new Placement("cajas_apiladas", -29f, 3f),
        new Placement("muro_ladrillo_roto", -38f, 7f),
        new Placement("tambo_oxidado", -38f, -2f),

        // Casa de Alex, viviendas vecinas y calle central.
        new Placement("cartel_madera", -12f, -9f),
        new Placement("cerca_rota", -29f, -13f),
        new Placement("cubeta_agua", -15f, -14f),
        new Placement("tambo_azul", -13f, -14f),
        new Placement("tendedero", -19f, -3f),
        new Placement("poste_electrico", 14f, 2f),
        new Placement("mesa_mercado", -9f, -7f),

        // Panadería y tienda, por delante de sus fachadas.
        new Placement("letrero_pan", 11f, -9f),
        new Placement("maceta_reutilizada", 12.5f, -8f),
        new Placement("pila_cajas_harina", -3f, -9f),

        // Taller: el vehículo queda en el patio, al sur de la fachada.
        new Placement("carro_viejo", 20f, -24f),
        new Placement("llanta_vieja", 14f, -22f),
        new Placement("pila_ladrillos", 15f, -25.5f),

        // Salida bloqueada, sobre el asfalto inferior derecho.
        new Placement("senal_bloqueo", 19f, -36f),
        new Placement("valla_obras", 19f, -40f)
    };

    private static readonly string[] Indoors =
    {
        "cama_bit", "cama_humilde", "computadora_vieja",
        "escritorio_modesto", "plato_bit"
    };

    [MenuItem("Tools/Technopolis/Nivel 1/04 Distribuir todos los objetos")]
    public static void Distribuir()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Sal de Play", "Detén el juego antes de ubicar los objetos.", "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Abre el Nivel 1",
                "Abre Assets/Scenes/EscenaNivel1.unity y vuelve a usar esta opción.", "Aceptar");
            return;
        }
        Transform parent = Find(scene, "Objetos", out int parentCount);
        if (parentCount != 1 || !parent || !parent.parent || parent.parent.name != "Grid")
        {
            EditorUtility.DisplayDialog("Falta la capa Objetos",
                "La escena necesita Grid > Objetos.", "Aceptar");
            return;
        }

        // Validación completa antes de guardar el respaldo o mover instancias.
        var prefabs = new GameObject[Outdoor.Length];
        var existing = new Transform[Outdoor.Length];
        for (int i = 0; i < Outdoor.Length; i++)
        {
            Placement spec = Outdoor[i];
            string path = PrefabFolder + spec.Prefab + ".prefab";
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefabs[i])
            {
                EditorUtility.DisplayDialog("Falta un prefab", "No existe " + path, "Aceptar");
                return;
            }
            existing[i] = Find(scene, spec.Name, out int count);
            if (count > 1)
            {
                EditorUtility.DisplayDialog("Nombre duplicado",
                    "Hay " + count + " objetos llamados " + spec.Name +
                    ". Deja solo uno antes de ejecutar esta opción.", "Aceptar");
                return;
            }
        }
        var interiorExisting = new Transform[Indoors.Length];
        for (int i = 0; i < Indoors.Length; i++)
        {
            string path = PrefabFolder + Indoors[i] + ".prefab";
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(path))
            {
                EditorUtility.DisplayDialog("Falta un prefab", "No existe " + path, "Aceptar");
                return;
            }
            interiorExisting[i] = Find(scene, Indoors[i], out int count);
            if (count > 1)
            {
                EditorUtility.DisplayDialog("Nombre duplicado",
                    "Hay " + count + " objetos llamados " + Indoors[i] + ".", "Aceptar");
                return;
            }
        }
        Transform interiorParent = Find(scene, InteriorName, out int interiorParentCount);
        if (interiorParentCount > 1)
        {
            EditorUtility.DisplayDialog("Carpeta duplicada",
                "Hay varias carpetas " + InteriorName + " en la escena.", "Aceptar");
            return;
        }

        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("No se pudo guardar",
                "La escena no se modificó.", "Aceptar");
            return;
        }
        if (!AssetDatabase.IsValidFolder(BackupFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = BackupFolder + "/EscenaNivel1_AntesObjetos_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(ScenePath, backup))
        {
            EditorUtility.DisplayDialog("No se pudo crear el respaldo",
                "La escena no se modificó. Revisa Assets/Scenes/Respaldos.", "Aceptar");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Distribuir objetos del Nivel 1");
        int created = 0, moved = 0;
        for (int i = 0; i < Outdoor.Length; i++)
        {
            Placement spec = Outdoor[i];
            GameObject go;
            if (existing[i])
            {
                go = existing[i].gameObject;
                moved++;
            }
            else
            {
                go = PrefabUtility.InstantiatePrefab(prefabs[i], scene) as GameObject;
                if (!go)
                {
                    Debug.LogError("No se pudo crear " + spec.Name + ". Respaldo: " + backup);
                    return;
                }
                go.name = spec.Name;
                Undo.RegisterCreatedObjectUndo(go, "Crear " + spec.Name);
                PrefabUtility.RecordPrefabInstancePropertyModifications(go);
                created++;
            }

            if (go.transform.parent != parent)
                Undo.SetTransformParent(go.transform, parent, "Agrupar " + spec.Name);
            Undo.RecordObject(go.transform, "Ubicar " + spec.Name);
            go.transform.SetPositionAndRotation(spec.Position, Quaternion.identity);
            go.transform.localScale = spec.Scale;
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
            if (!go.activeSelf)
            {
                Undo.RecordObject(go, "Mostrar " + spec.Name);
                go.SetActive(true);
                PrefabUtility.RecordPrefabInstancePropertyModifications(go);
            }
        }

        // El paquete incluye muebles del cuarto de Alex. El nivel exterior no
        // tiene interior jugable todavía: se conservan, agrupados y ocultos.
        if (!interiorParent)
        {
            GameObject folder = new GameObject(InteriorName);
            Undo.RegisterCreatedObjectUndo(folder, "Crear Interiores Pendientes");
            interiorParent = folder.transform;
        }
        if (interiorParent.parent != parent)
            Undo.SetTransformParent(interiorParent, parent, "Agrupar interiores");
        Undo.RecordObject(interiorParent, "Ubicar interiores");
        interiorParent.localPosition = Vector3.zero;
        interiorParent.localRotation = Quaternion.identity;
        interiorParent.localScale = Vector3.one;
        for (int i = 0; i < Indoors.Length; i++)
        {
            if (!interiorExisting[i]) continue;
            if (interiorExisting[i].parent != interiorParent)
                Undo.SetTransformParent(interiorExisting[i], interiorParent,
                    "Guardar " + Indoors[i] + " para el interior");
            PrefabUtility.RecordPrefabInstancePropertyModifications(interiorExisting[i]);
        }
        if (interiorParent.gameObject.activeSelf)
        {
            Undo.RecordObject(interiorParent.gameObject, "Ocultar interiores pendientes");
            interiorParent.gameObject.SetActive(false);
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("Revisa la escena",
                "Los objetos se movieron, pero Unity no pudo guardar. " +
                "Guarda con Ctrl+S. Respaldo: " + backup, "Aceptar");
            return;
        }

        EditorUtility.DisplayDialog("Objetos distribuidos",
            "Objetos movidos: " + moved + "; objetos nuevos: " + created + ".\n" +
            "Se llenaron callejón, viviendas, plaza, parque, comercios, taller y salida.\n" +
            "Los muebles están guardados en Interiores_Pendientes (oculto).\n\n" +
            "Respaldo anterior: " + backup, "Aceptar");
        Debug.Log("Technopolis: objetos distribuidos. Respaldo: " + backup);
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
