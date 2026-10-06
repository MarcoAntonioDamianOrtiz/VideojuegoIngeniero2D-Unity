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
    const string TerrainPath = "Assets/Technopolis/Nivel1/Sprites/01_Terreno/terreno_organico_nivel1.png";
    static readonly string[] NewHousePaths = {
        "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_dos_pisos_ocre.png",
        "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_dos_pisos_ladrillo.png",
        "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_lona_azul.png"
    };

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
        if (Mathf.Abs(grid.transform.localScale.y - 1.25f) > .001f)
            throw new InvalidOperationException("El mapa no conserva la proporcion vertical de la referencia.");

        Transform ground = barrio.Find("Suelo_Tiles");
        Transform paths = barrio.Find("Senderos_Tiles");
        Tilemap groundMap = ground ? ground.GetComponent<Tilemap>() : null;
        TilemapRenderer groundRenderer = ground ? ground.GetComponent<TilemapRenderer>() : null;
        TilemapRenderer pathRenderer = paths ? paths.GetComponent<TilemapRenderer>() : null;
        Transform terrain = barrio.Find("Terreno_Base_Sprite/Terreno_Organico");
        SpriteRenderer terrainRenderer = terrain ? terrain.GetComponentInChildren<SpriteRenderer>() : null;
        if (!groundMap || !groundRenderer || !pathRenderer || groundMap.cellBounds.size.x != 64 || groundMap.cellBounds.size.y != 64 ||
            !terrainRenderer || AssetDatabase.GetAssetPath(terrainRenderer.sprite) != TerrainPath ||
            terrain.GetComponentInChildren<Collider2D>() || terrainRenderer.sprite.texture.filterMode != FilterMode.Point ||
            groundRenderer.sortingOrder >= pathRenderer.sortingOrder || pathRenderer.sortingOrder >= terrainRenderer.sortingOrder ||
            terrainRenderer.sortingOrder >= 0)
            throw new InvalidOperationException("Faltan los tiles de 64 x 64 o su orden de dibujo es incorrecto.");

        PixelPerfectCamera pixelCamera = cameraObject.GetComponent<PixelPerfectCamera>();
        if (!pixelCamera || pixelCamera.assetsPPU != 32 || pixelCamera.refResolutionX != 640 || pixelCamera.refResolutionY != 360 ||
            pixelCamera.gridSnapping != PixelPerfectCamera.GridSnapping.UpscaleRenderTexture)
            throw new InvalidOperationException("La camara pixel-perfect no esta configurada.");

        SpriteRenderer[] sprites = barrio.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length < 861) throw new InvalidOperationException($"Faltan sprites independientes: {sprites.Length}.");
        int[] generatedHouses = new int[NewHousePaths.Length];
        foreach (SpriteRenderer sprite in sprites)
        {
            if (!sprite.sprite) throw new InvalidOperationException($"Sprite faltante en {sprite.name}.");
            string path = AssetDatabase.GetAssetPath(sprite.sprite);
            if (path == OldMapPath) throw new InvalidOperationException("La escena aun usa el fondo unico del mapa.");
            for (int i = 0; i < NewHousePaths.Length; i++)
                if (path == NewHousePaths[i])
                {
                    generatedHouses[i]++;
                    if (sprite.sprite.texture.filterMode != FilterMode.Point)
                        throw new InvalidOperationException($"El sprite {path} no usa filtro Point.");
                }
        }
        foreach (int count in generatedHouses)
            if (count < 3) throw new InvalidOperationException("Falta alguna variante nueva de vivienda.");

        Collider2D[] obstacles = barrio.GetComponentsInChildren<Collider2D>();
        Collider2D playerCollider = jugador.GetComponent<Collider2D>();
        if (!playerCollider || obstacles.Length < 76) throw new InvalidOperationException("Faltan colisiones del nivel.");
        foreach (Collider2D obstacle in obstacles)
            if (obstacle.bounds.Intersects(playerCollider.bounds))
                throw new InvalidOperationException($"El jugador empieza dentro de {obstacle.name}.");

        Debug.Log($"TECHNOPOLIS_SPRITES_OK sprites={sprites.Length} viviendasNuevas={string.Join(",", generatedHouses)} terreno={terrainRenderer.sprite.texture.width}x{terrainRenderer.sprite.texture.height} tiles={groundMap.cellBounds.size} colliders={obstacles.Length} escalaY={grid.transform.localScale.y} ppu={pixelCamera.assetsPPU}");
    }
}
#endif
