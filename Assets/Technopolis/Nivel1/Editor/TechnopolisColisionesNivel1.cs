#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ajusta las huellas de los objetos del Nivel 1 y la colisión del jugador.
/// Trabaja sobre la escena y deja los prefabs originales intactos.
/// </summary>
public static class TechnopolisColisionesNivel1
{
    private const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    private const string BackupFolder = "Assets/Scenes/Respaldos";

    private struct Footprint
    {
        public readonly Vector2 Size;
        public readonly Vector2 Offset;

        public Footprint(float width, float height, float centerY)
        {
            Size = new Vector2(width, height);
            Offset = new Vector2(0f, centerY);
        }
    }

    // Medidas locales: el Transform de cada prefab aplica su propia escala.
    // Las casas tienen un pequeño margen lateral para dejar transitable el barrio.
    // Árboles y postes usan solo el tronco.
    private static readonly Dictionary<string, Footprint> Footprints =
        new Dictionary<string, Footprint>
    {
        { "casa_alex", new Footprint(4.7f, 3.7f, 1.85f) },
        { "panaderia_dona_ramona", new Footprint(5.9f, 4.4f, 2.2f) },
        { "tienda_miscelanea", new Footprint(5.3f, 5f, 2.5f) },
        { "taller_mecanico", new Footprint(5.9f, 3.7f, 1.85f) },
        { "vivienda_techo_lamina", new Footprint(4.9f, 4f, 2f) },
        { "vivienda_ladrillo", new Footprint(4.5f, 4f, 2f) },
        { "vivienda_parches", new Footprint(4f, 3.5f, 1.75f) },
        { "centro_comunitario", new Footprint(5.9f, 3.7f, 1.85f) },
        { "arbol_barrio", new Footprint(0.7f, 0.7f, 0.5f) },
        { "arbusto_seco", new Footprint(0.6f, 0.5f, 0.35f) },
        { "banco_viejo", new Footprint(2.5f, 0.65f, 0.45f) },
        { "bicicleta_vieja", new Footprint(1.8f, 0.6f, 0.45f) },
        { "bolsa_basura_negra", new Footprint(0.9f, 0.6f, 0.4f) },
        { "bolsas_basura", new Footprint(2f, 0.8f, 0.5f) },
        { "caja_madera", new Footprint(1.1f, 0.7f, 0.45f) },
        { "cajas_apiladas", new Footprint(2.2f, 1.25f, 0.75f) },
        { "carro_viejo", new Footprint(2.6f, 3.2f, 1.7f) },
        { "cartel_madera", new Footprint(0.5f, 0.4f, 0.35f) },
        { "cerca_madera", new Footprint(1.8f, 0.3f, 0.45f) },
        { "cerca_rota", new Footprint(1.8f, 0.3f, 0.45f) },
        { "cubeta_agua", new Footprint(0.7f, 0.5f, 0.35f) },
        { "fuente_pequena", new Footprint(2.7f, 1.3f, 0.9f) },
        { "letrero_pan", new Footprint(0.5f, 0.4f, 0.35f) },
        { "llanta_vieja", new Footprint(1.2f, 0.5f, 0.35f) },
        { "maceta_reutilizada", new Footprint(0.8f, 0.7f, 0.45f) },
        { "mesa_mercado", new Footprint(2.9f, 0.8f, 0.5f) },
        { "muro_ladrillo_roto", new Footprint(2.8f, 1.4f, 0.8f) },
        { "papelera", new Footprint(0.9f, 0.7f, 0.45f) },
        { "pila_cajas_harina", new Footprint(2.4f, 0.8f, 0.5f) },
        { "pila_ladrillos", new Footprint(2f, 0.6f, 0.4f) },
        { "poste_electrico", new Footprint(0.5f, 0.7f, 0.45f) },
        { "poste_luz", new Footprint(0.35f, 0.6f, 0.4f) },
        { "senal_bloqueo", new Footprint(0.8f, 0.5f, 0.4f) },
        { "tambo_azul", new Footprint(1.1f, 0.8f, 0.5f) },
        { "tambo_oxidado", new Footprint(1.1f, 0.8f, 0.5f) },
        { "valla_obras", new Footprint(2.8f, 0.35f, 0.4f) }
        // El tendedero se puede atravesar por debajo; muebles interiores inactivos.
    };

    [MenuItem("Tools/Technopolis/Nivel 1/06 Preparar colisiones y punto de inicio")]
    public static void Preparar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Sal de Play", "Detén el juego antes de ajustar colisiones.", "Aceptar");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Abre el Nivel 1",
                "Abre Assets/Scenes/EscenaNivel1.unity y vuelve a intentarlo.", "Aceptar");
            return;
        }

        Transform grid = Find(scene, "Grid");
        Transform buildings = Find(scene, "Edificios");
        Transform objects = Find(scene, "Objetos");
        Transform player = Find(scene, "Jugador");
        CapsuleCollider2D playerCollider = player ? player.GetComponent<CapsuleCollider2D>() : null;
        if (!grid || !buildings || !objects || !player ||
            buildings.parent != grid || objects.parent != grid ||
            !player.GetComponent<Rigidbody2D>() || !playerCollider ||
            Mathf.Abs(player.lossyScale.x) < 0.01f || Mathf.Abs(player.lossyScale.y) < 0.01f)
        {
            EditorUtility.DisplayDialog("Escena incompleta",
                "Se necesitan Grid > Edificios, Grid > Objetos y Jugador con Rigidbody2D y CapsuleCollider2D.",
                "Aceptar");
            return;
        }
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("No se pudo guardar", "La escena no se modificó.", "Aceptar");
            return;
        }
        if (!AssetDatabase.IsValidFolder(BackupFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = BackupFolder + "/EscenaNivel1_AntesColisiones_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(ScenePath, backup))
        {
            EditorUtility.DisplayDialog("No se pudo crear el respaldo",
                "La escena no se modificó. Revisa Assets/Scenes/Respaldos.", "Aceptar");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Colisiones y comienzo del Nivel 1");
        int added = 0, adjusted = 0, preserved = 0;
        CollidersUnder(buildings, ref added, ref adjusted, ref preserved);
        CollidersUnder(objects, ref added, ref adjusted, ref preserved);

        // El transform del jugador está escalado. Mantener la cápsula en
        // unidades del mundo evita que la escala visual cierre los pasillos.
        Undo.RecordObject(playerCollider, "Reducir colisión del Jugador");
        playerCollider.size = new Vector2(0.72f / Mathf.Abs(player.lossyScale.x),
                                          0.90f / Mathf.Abs(player.lossyScale.y));
        playerCollider.offset = new Vector2(0f, 0.45f / Mathf.Abs(player.lossyScale.y));
        playerCollider.direction = CapsuleDirection2D.Vertical;
        playerCollider.isTrigger = false;

        Transform limits = Find(scene, "Limites_Nivel1");
        if (!limits)
        {
            GameObject root = new GameObject("Limites_Nivel1");
            Undo.RegisterCreatedObjectUndo(root, "Crear límites del mapa");
            limits = root.transform;
        }
        if (limits.parent != grid)
            Undo.SetTransformParent(limits, grid, "Agrupar límites");
        SetWall(limits, "Limite_Oeste", new Vector3(-40.5f, -12f, 0f), new Vector2(1f, 66f));
        SetWall(limits, "Limite_Este", new Vector3(24.5f, -12f, 0f), new Vector2(1f, 66f));
        SetWall(limits, "Limite_Norte", new Vector3(-8f, 20.5f, 0f), new Vector2(66f, 1f));
        SetWall(limits, "Limite_Sur", new Vector3(-8f, -44.5f, 0f), new Vector2(66f, 1f));

        bool movedPlayer = false;
        // El Jugador original está en (0,0), dentro de la nueva panadería.
        // Conserva cualquier punto de inicio que el usuario ya haya movido.
        if (player.position.x > -4f && player.position.x < 10f &&
            player.position.y > -8f && player.position.y < 3f)
        {
            Undo.RecordObject(player, "Inicio libre junto a Casa Alex");
            player.position = new Vector3(-17f, -17f, player.position.z);
            movedPlayer = true;
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            EditorUtility.DisplayDialog("Revisa la escena",
                "Las colisiones se configuraron, pero Unity no pudo guardar. " +
                "Guarda con Ctrl+S. Respaldo: " + backup, "Aceptar");
            return;
        }
        EditorUtility.DisplayDialog("Colisiones listas para probar",
            "Colliders nuevos: " + added + "; ajustados: " + adjusted +
            "; otros conservados: " + preserved + ".\n" +
            "La cápsula del Jugador ahora ocupa 0.72 x 0.90 unidades junto a sus pies.\n" +
            "Se cerraron los cuatro bordes del mapa." +
            (movedPlayer ? "\nJugador ubicado en terreno libre junto a Casa Alex." : "") +
            "\n\nPrueba caminar en Game y revisa las entradas.\nRespaldo: " + backup,
            "Aceptar");
        Debug.Log("Technopolis: colisiones del Nivel 1. Respaldo: " + backup);
    }

    private static void CollidersUnder(Transform parent, ref int added, ref int adjusted,
                                       ref int preserved)
    {
        foreach (Transform item in parent.GetComponentsInChildren<Transform>(true))
        {
            if (item == parent || !item.gameObject.activeInHierarchy) continue;
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(item.gameObject) as GameObject;
            string type = prefab ? prefab.name : item.name;
            if (!Footprints.TryGetValue(type, out Footprint footprint)) continue;
            Collider2D existing = item.GetComponent<Collider2D>();
            if (existing && !(existing is BoxCollider2D)) { preserved++; continue; }

            BoxCollider2D box = existing as BoxCollider2D;
            if (box) adjusted++;
            else
            {
                box = Undo.AddComponent<BoxCollider2D>(item.gameObject);
                added++;
            }
            Undo.RecordObject(box, "Huella de " + item.name);
            box.size = footprint.Size;
            box.offset = footprint.Offset;
            box.isTrigger = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(box);
        }
    }

    private static void SetWall(Transform parent, string name, Vector3 position, Vector2 size)
    {
        Transform wall = parent.Find(name);
        if (!wall)
        {
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Crear " + name);
            wall = go.transform;
            Undo.SetTransformParent(wall, parent, "Agrupar " + name);
        }
        Undo.RecordObject(wall, "Ubicar " + name);
        wall.position = position;
        wall.rotation = Quaternion.identity;
        wall.localScale = Vector3.one;
        BoxCollider2D box = wall.GetComponent<BoxCollider2D>();
        if (!box) box = Undo.AddComponent<BoxCollider2D>(wall.gameObject);
        Undo.RecordObject(box, "Ajustar " + name);
        box.size = size;
        box.offset = Vector2.zero;
        box.isTrigger = false;
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
