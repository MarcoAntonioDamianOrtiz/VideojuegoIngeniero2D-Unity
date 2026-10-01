#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TechnopolisVerificarEscenaFiel
{
    const string ScenePath = "Assets/Scenes/EscenaNivel1_FielReferencia.unity";
    const string MapPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/mapa_nivel1_fiel_referencia.png";

    public static void Verificar()
    {
        Sprite map = AssetDatabase.LoadAssetAtPath<Sprite>(MapPath);
        if (!map || map.rect.width != 1254 || map.rect.height != 1254)
            throw new InvalidOperationException("El mapa fiel no se importó como sprite completo de 1254 px.");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) throw new InvalidOperationException("No se pudo abrir la escena fiel.");

        GameObject barrio = null;
        GameObject jugador = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Jugador") jugador = root;
            Transform candidate = root.transform.Find("Barrio_Fiel_Referencia");
            if (candidate) barrio = candidate.gameObject;
        }
        if (!barrio || !jugador) throw new InvalidOperationException("Faltan el barrio o el jugador.");

        Transform visual = barrio.transform.Find("Fondo_Visual/Mapa_Visual_Fiel");
        SpriteRenderer background = visual ? visual.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer player = jugador.GetComponent<SpriteRenderer>();
        if (!background || background.sprite != map || !player || background.sortingOrder >= player.sortingOrder || background.sortingOrder > -1000)
            throw new InvalidOperationException("Falta el mapa visual o su orden de dibujo es incorrecto.");

        GameObject[] roots = scene.GetRootGameObjects();
        if (roots.Length == 0 || roots[roots.Length - 1].name != "Grid")
            throw new InvalidOperationException("Grid debe quedar debajo de los demas objetos en la jerarquia.");
        if (EditorBuildSettings.scenes.Length == 0 || EditorBuildSettings.scenes[0].path != ScenePath || !EditorBuildSettings.scenes[0].enabled)
            throw new InvalidOperationException("La escena fiel debe ser la primera escena del juego.");

        Bounds bounds = background.bounds;
        Debug.Log($"TECHNOPOLIS_FIEL_BOUNDS bounds={bounds} spriteRect={map.rect} pivot={map.pivot} ppu={map.pixelsPerUnit} scale={visual.lossyScale}");
        if (Mathf.Abs(bounds.min.x + 40f) > .02f || Mathf.Abs(bounds.max.x - 24f) > .02f ||
            Mathf.Abs(bounds.min.y + 44f) > .02f || Mathf.Abs(bounds.max.y - 20f) > .02f)
            throw new InvalidOperationException($"El mapa no cubre las 64 x 64 unidades del nivel: {bounds}.");

        Collider2D[] obstacles = barrio.GetComponentsInChildren<Collider2D>();
        Collider2D playerCollider = jugador.GetComponent<Collider2D>();
        if (!playerCollider) throw new InvalidOperationException("El jugador no tiene collider.");
        foreach (Collider2D obstacle in obstacles)
            if (obstacle.bounds.Intersects(playerCollider.bounds))
                throw new InvalidOperationException($"El jugador empieza dentro de {obstacle.name}.");

        int colliders = obstacles.Length;
        if (colliders != 53) throw new InvalidOperationException($"Se esperaban 49 obstaculos y cuatro limites; hay {colliders}.");
        Debug.Log($"TECHNOPOLIS_FIEL_OK sprite={map.rect.width}x{map.rect.height} bounds={bounds} colliders={colliders}");
    }
}
#endif
