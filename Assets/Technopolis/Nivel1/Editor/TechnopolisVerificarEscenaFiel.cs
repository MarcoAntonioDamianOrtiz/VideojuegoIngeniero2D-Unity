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
    const string ParcelWallPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/muro_patios_ocre.png";
    const string WorkshopCarPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/auto_taller_rojo.png";
    const string ConcreteStairPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/escalera_barrio_concreto_nueva.png";
    const string DirtStairPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/escalera_barrio_tierra_nueva.png";
    const string StrayMuralPath = "Assets/Technopolis/Nivel1/Sprites/04_Objetos/mural_aqui_somos_ciudad.png";
    static readonly string[] NewHousePaths = {
        "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_dos_pisos_azul_referencia.png",
        "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_dos_pisos_roja_referencia.png",
        "Assets/Technopolis/Nivel1/Sprites/05_Edificios/vivienda_taller_lona_referencia.png"
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
        GameObject ramona = GameObject.Find("Ramona_Npc");
        GameObject canvas = GameObject.Find("Canvas");
        GameObject eventSystem = GameObject.Find("EventSystem");
        Transform barrio = grid.transform.Find("Barrio_Fiel_Referencia");
        if (!jugador || !cameraObject || !barrio) throw new InvalidOperationException("Falta el jugador, la camara o el barrio.");
        if (!jugador.CompareTag("Player") || !ramona || !canvas || !eventSystem)
            throw new InvalidOperationException("Falta el jugador etiquetado o la interaccion y UI de Doña Ramona.");
        InteraccionDonaRamona interaction = ramona.GetComponent<InteraccionDonaRamona>();
        BoxCollider2D[] npcColliders = ramona.GetComponents<BoxCollider2D>();
        if (!interaction || !interaction.panelDialogo || !interaction.textoDialogo ||
            !interaction.panelTerminalMinijuego || !interaction.scriptMovimientoAlex ||
            npcColliders.Length < 2 || !npcColliders[1].isTrigger ||
            Mathf.Abs(ramona.transform.position.x - 4f) > .1f ||
            Mathf.Abs(ramona.transform.position.y + 10.4f) > .1f)
            throw new InvalidOperationException("La NPC, el dialogo o el minijuego quedaron desconectados del nuevo mapa.");
        MinijuegoAleatorioPanaderia minigame = canvas.GetComponentInChildren<MinijuegoAleatorioPanaderia>(true);
        if (!minigame || !minigame.textoEnunciado || !minigame.panelTerminal ||
            minigame.imagenesCorazones == null || minigame.imagenesCorazones.Length != 3 ||
            minigame.botonesOpciones == null || minigame.botonesOpciones.Length != 3)
            throw new InvalidOperationException("La terminal de la panaderia perdio sus controles o vidas.");
        foreach (var heart in minigame.imagenesCorazones)
            if (!heart) throw new InvalidOperationException("Falta un corazon de la terminal.");
        foreach (var button in minigame.botonesOpciones)
            if (!button) throw new InvalidOperationException("Falta un boton de respuesta de la terminal.");
        if (Mathf.Abs(grid.transform.localScale.y - 1.25f) > .001f)
            throw new InvalidOperationException("El mapa no conserva la proporcion vertical de la referencia.");

        Transform ground = barrio.Find("Suelo_Tiles");
        Transform paths = barrio.Find("Senderos_Tiles");
        Tilemap groundMap = ground ? ground.GetComponent<Tilemap>() : null;
        TilemapRenderer groundRenderer = ground ? ground.GetComponent<TilemapRenderer>() : null;
        TilemapRenderer pathRenderer = paths ? paths.GetComponent<TilemapRenderer>() : null;
        Transform terrain = barrio.Find("Terreno_Base_Sprite/Terreno_Organico");
        Transform stairs = barrio.Find("Escaleras_Sprites");
        Transform reliefEdges = barrio.Find("Bordes_Desnivel_Colision");
        Transform buildings = barrio.Find("Edificios_Sprites");
        SpriteRenderer terrainRenderer = terrain ? terrain.GetComponentInChildren<SpriteRenderer>() : null;
        if (!groundMap || !groundRenderer || !pathRenderer || groundMap.cellBounds.size.x != 64 || groundMap.cellBounds.size.y != 64 ||
            !terrainRenderer || AssetDatabase.GetAssetPath(terrainRenderer.sprite) != TerrainPath ||
            terrain.GetComponentInChildren<Collider2D>() || terrainRenderer.sprite.texture.filterMode != FilterMode.Point ||
            groundRenderer.sortingOrder >= pathRenderer.sortingOrder || pathRenderer.sortingOrder >= terrainRenderer.sortingOrder ||
            terrainRenderer.sortingOrder >= 0)
            throw new InvalidOperationException("Faltan los tiles de 64 x 64 o su orden de dibujo es incorrecto.");
        if (!stairs || stairs.GetComponentsInChildren<SpriteRenderer>().Length != 18 ||
            !reliefEdges || reliefEdges.GetComponentsInChildren<BoxCollider2D>().Length < 30)
            throw new InvalidOperationException("Faltan escaleras o bordes de desnivel con colision.");
        int concreteStairs = 0;
        int dirtStairs = 0;
        foreach (SpriteRenderer stair in stairs.GetComponentsInChildren<SpriteRenderer>())
        {
            string path = AssetDatabase.GetAssetPath(stair.sprite);
            if (path == ConcreteStairPath) concreteStairs++;
            else if (path == DirtStairPath) dirtStairs++;
            else throw new InvalidOperationException($"La escalera {stair.name} usa un grafico antiguo o faltante.");
            if (stair.sprite.texture.filterMode != FilterMode.Point)
                throw new InvalidOperationException($"La escalera {stair.name} no usa filtro Point.");
        }
        if (concreteStairs < 3 || dirtStairs < 3)
            throw new InvalidOperationException($"Escaleras incompletas: concreto={concreteStairs}, tierra={dirtStairs}.");
        int homes = 0;
        if (!buildings) throw new InvalidOperationException("Falta la capa de edificios.");
        foreach (Transform building in buildings)
            if (building.name.StartsWith("Vivienda_", StringComparison.Ordinal)) homes++;
        if (homes != 54) throw new InvalidOperationException($"Distribucion residencial incompleta: {homes} viviendas.");

        PixelPerfectCamera pixelCamera = cameraObject.GetComponent<PixelPerfectCamera>();
        if (!pixelCamera || pixelCamera.assetsPPU != 32 || pixelCamera.refResolutionX != 640 || pixelCamera.refResolutionY != 360 ||
            pixelCamera.gridSnapping != PixelPerfectCamera.GridSnapping.UpscaleRenderTexture)
            throw new InvalidOperationException("La camara pixel-perfect no esta configurada.");

        SpriteRenderer[] sprites = barrio.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length < 861) throw new InvalidOperationException($"Faltan sprites independientes: {sprites.Length}.");
        int[] generatedHouses = new int[NewHousePaths.Length];
        int parcelWalls = 0;
        int workshopCars = 0;
        int oldFences = 0;
        foreach (SpriteRenderer sprite in sprites)
        {
            if (!sprite.sprite) throw new InvalidOperationException($"Sprite faltante en {sprite.name}.");
            string path = AssetDatabase.GetAssetPath(sprite.sprite);
            if (path == OldMapPath) throw new InvalidOperationException("La escena aun usa el fondo unico del mapa.");
            if (path == StrayMuralPath) throw new InvalidOperationException("El mural azul suelto sigue en el suelo.");
            if (path == ParcelWallPath) parcelWalls++;
            if (path == WorkshopCarPath) workshopCars++;
            if (sprite.transform.parent && sprite.transform.parent.name.StartsWith("cerca_madera_", StringComparison.Ordinal)) oldFences++;
            for (int i = 0; i < NewHousePaths.Length; i++)
                if (path == NewHousePaths[i])
                {
                    generatedHouses[i]++;
                    if (sprite.sprite.texture.filterMode != FilterMode.Point)
                        throw new InvalidOperationException($"El sprite {path} no usa filtro Point.");
                }
        }
        foreach (int count in generatedHouses)
            if (count < 2) throw new InvalidOperationException("Falta alguna variante nueva de vivienda.");
        if (parcelWalls < 90 || workshopCars != 1 || oldFences > 20)
            throw new InvalidOperationException($"Distribucion nueva incompleta: muros={parcelWalls}, auto={workshopCars}, vallas={oldFences}.");

        Collider2D[] obstacles = barrio.GetComponentsInChildren<Collider2D>();
        Collider2D playerCollider = jugador.GetComponent<Collider2D>();
        if (!playerCollider || obstacles.Length < 76) throw new InvalidOperationException("Faltan colisiones del nivel.");
        foreach (Collider2D obstacle in obstacles)
            if (obstacle.bounds.Intersects(playerCollider.bounds))
                throw new InvalidOperationException($"El jugador empieza dentro de {obstacle.name}.");

        Debug.Log($"TECHNOPOLIS_SPRITES_OK sprites={sprites.Length} viviendas={homes} viviendasNuevas={string.Join(",", generatedHouses)} muros={parcelWalls} vallas={oldFences} escalerasConcreto={concreteStairs} escalerasTierra={dirtStairs} autoRojo={workshopCars} terreno={terrainRenderer.sprite.texture.width}x{terrainRenderer.sprite.texture.height} tiles={groundMap.cellBounds.size} colliders={obstacles.Length} ramona={ramona.transform.position} escalaY={grid.transform.localScale.y} ppu={pixelCamera.assetsPPU}");
    }
}
#endif
