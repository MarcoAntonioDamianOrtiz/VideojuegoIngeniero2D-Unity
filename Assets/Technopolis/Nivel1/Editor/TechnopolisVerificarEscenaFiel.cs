#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class TechnopolisVerificarEscenaFiel
{
    const string ScenePath = "Assets/Scenes/EscenaNivel1_FielReferencia.unity";
    const string OldMapPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/mapa_nivel1_fiel_referencia.png";
    const string NewHousePath = "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_ocre_lamina.png";

    public static void Verificar()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) throw new InvalidOperationException("No se pudo abrir la escena principal.");

        GameObject[] roots = scene.GetRootGameObjects();
        if (roots.Length == 0 || roots[roots.Length - 1].name != "Grid")
            throw new InvalidOperationException("Grid debe quedar al final de la jerarquia.");
        if (EditorBuildSettings.scenes.Length == 0 || EditorBuildSettings.scenes[0].path != ScenePath || !EditorBuildSettings.scenes[0].enabled)
            throw new InvalidOperationException("La escena de sprites debe ser la primera escena del juego.");

        GameObject grid = roots[roots.Length - 1];
        GameObject jugador = GameObject.Find("Jugador");
        GameObject cameraObject = GameObject.Find("Main Camera");
        Transform barrio = grid.transform.Find("Barrio_Fiel_Referencia");
        if (!jugador || !cameraObject || !barrio) throw new InvalidOperationException("Falta el jugador, la camara o el barrio.");

        Transform ground = barrio.Find("Suelo_Tiles");
        Transform paths = barrio.Find("Senderos_Tiles");
        Tilemap groundMap = ground ? ground.GetComponent<Tilemap>() : null;
        TilemapRenderer groundRenderer = ground ? ground.GetComponent<TilemapRenderer>() : null;
        TilemapRenderer pathRenderer = paths ? paths.GetComponent<TilemapRenderer>() : null;
        if (!groundMap || !groundRenderer || !pathRenderer || groundMap.cellBounds.size.x != 64 || groundMap.cellBounds.size.y != 64 ||
            groundRenderer.sortingOrder >= pathRenderer.sortingOrder || pathRenderer.sortingOrder >= 0)
            throw new InvalidOperationException("Faltan los tiles de 64 x 64 o su orden de dibujo es incorrecto.");

        PixelPerfectCamera pixelCamera = cameraObject.GetComponent<PixelPerfectCamera>();
        if (!pixelCamera || pixelCamera.assetsPPU != 32 || pixelCamera.refResolutionX != 640 || pixelCamera.refResolutionY != 360 ||
            pixelCamera.gridSnapping != PixelPerfectCamera.GridSnapping.UpscaleRenderTexture)
            throw new InvalidOperationException("La camara pixel-perfect no esta configurada.");

        SpriteRenderer[] sprites = barrio.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length < 750) throw new InvalidOperationException($"Faltan sprites independientes: {sprites.Length}.");
        int generatedHouses = 0;
        foreach (SpriteRenderer sprite in sprites)
        {
            string path = AssetDatabase.GetAssetPath(sprite.sprite);
            if (path == OldMapPath) throw new InvalidOperationException("La escena aun usa el fondo unico del mapa.");
            if (path == NewHousePath) generatedHouses++;
        }
        if (generatedHouses < 8) throw new InvalidOperationException("Faltan las variantes nuevas de vivienda.");

        Collider2D[] obstacles = barrio.GetComponentsInChildren<Collider2D>();
        Collider2D playerCollider = jugador.GetComponent<Collider2D>();
        if (!playerCollider || obstacles.Length < 55) throw new InvalidOperationException("Faltan colisiones del nivel.");
        foreach (Collider2D obstacle in obstacles)
            if (obstacle.bounds.Intersects(playerCollider.bounds))
                throw new InvalidOperationException($"El jugador empieza dentro de {obstacle.name}.");

        Debug.Log($"TECHNOPOLIS_SPRITES_OK sprites={sprites.Length} viviendasNuevas={generatedHouses} tiles={groundMap.cellBounds.size} colliders={obstacles.Length} ppu={pixelCamera.assetsPPU}");
    }
}
#endif
