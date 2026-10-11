#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the independent menu scene. The level retains its camera, player,
/// NPCs and world; the menu owns its rooftop visual, UI and controller.
/// </summary>
public static class TechnopolisMenuVisualBuilder
{
    const string MenuPath = "Assets/Scenes/ESCENA NUEVA.unity";
    const string LevelPath = "Assets/Scenes/EscenaNivel1_FielReferencia.unity";
    const string Art = "Assets/Sprites/Menu/UI/";
    const string FontPath = "Assets/Sprites/Menu/Fonts/PixelifySans.ttf";

    static Font font;

    [MenuItem("Technopolis/Menu/Reconstruir interfaz visual")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
            UnityEngine.Object.DestroyImmediate(root);

        font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (!font) throw new InvalidOperationException("No se importo la fuente Pixelify Sans.");

        GameObject canvasObject = new GameObject("MenuCanvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        RectTransform scenePlate = Rect("EscenarioMenu", canvasRect,
            Vector2.zero, Vector2.one, new Vector2(.5f, .5f),
            Vector2.zero, Vector2.zero);
        Image rooftop = AddSpriteImage("TejadosAlAtardecer", scenePlate,
            "Assets/Sprites/Menu/EscenarioTejadosAtardecer.png", Vector2.zero,
            Vector2.zero, Vector2.zero, Vector2.one, new Vector2(.5f, .5f));
        rooftop.preserveAspect = false;

        RectTransform characters = Rect("PersonajesMenu", canvasRect,
            Vector2.zero, Vector2.one, new Vector2(.5f, .5f),
            Vector2.zero, Vector2.zero);
        AddSpriteImage("JugadorSentado", characters,
            "Assets/Sprites/Menu/Personajes/JugadorSentado.png", new Vector2(375, 98),
            new Vector2(247, 257), Vector2.zero, Vector2.zero, Vector2.zero);
        AddSpriteImage("PerroBitSentado", characters,
            "Assets/Sprites/Menu/Personajes/PerroBitSentado.png", new Vector2(644, 98),
            new Vector2(130, 169), Vector2.zero, Vector2.zero, Vector2.zero);

        RectTransform decoration = Rect("DecoracionesUI", canvasRect,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(.5f, .5f),
            Vector2.zero, Vector2.zero);
        RectTransform title = Rect("TituloPrincipal", canvasRect,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(92, -22), new Vector2(1210, 208));
        title.localScale = new Vector3(1.1f, 1, 1);
        Text techno = AddText("TECHNO", title, "TECHNO", 170, new Color32(255, 137, 42, 255),
            new Vector2(0, -4), new Vector2(625, 190), TextAnchor.MiddleLeft, 7, 10);
        Text polis = AddText("POLIS", title, "POLIS", 170, new Color32(29, 213, 241, 255),
            new Vector2(617, -4), new Vector2(570, 190), TextAnchor.MiddleLeft, 7, 10);
        // The reference uses tall block lettering. Scale only the editable UI
        // glyphs vertically; the character and environmental sprites stay exact.
        techno.rectTransform.localScale = new Vector3(1, 1.5f, 1);
        polis.rectTransform.localScale = new Vector3(1, 1.5f, 1);

        RectTransform subtitle = Rect("Subtitulo", canvasRect,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(146, -250), new Vector2(1130, 104));
        subtitle.localScale = new Vector3(1.1f, 1.1f, 1);
        AddText("TextoSubtitulo", subtitle, "EL SEMESTRE DEL HAMBRE", 79,
            new Color32(255, 223, 100, 255), Vector2.zero, new Vector2(1130, 100),
            TextAnchor.MiddleLeft, 5, 5);

        RectTransform panel = Rect("PanelMenu", canvasRect,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-34, -174), new Vector2(540, 720));
        AddImage("MarcoIndustrial", panel, "panel_industrial.png", Vector2.zero,
            new Vector2(540, 720), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        RectTransform rows = Rect("BotonesMenu", panel,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            Vector2.zero, new Vector2(540, 720));
        string[] labels = { "NUEVA PARTIDA", "CONTINUAR", "OPCIONES", "CRÉDITOS", "SALIR" };
        string[] names = { "NuevaPartida", "Continuar", "Opciones", "Creditos", "Salir" };
        int[] centerY = { 178, 273, 370, 468, 567 };
        RectTransform[] mainRows = new RectTransform[labels.Length];
        Text[] mainLabels = new Text[labels.Length];
        Image selection = null;
        Image arrow = null;
        for (int i = 0; i < labels.Length; i++)
        {
            RectTransform row = Rect("Boton" + names[i], rows,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(57, -(centerY[i] - 37)), new Vector2(426, 74));
            mainRows[i] = row;
            if (i == 0)
            {
                selection = AddImage("SeleccionVisual", row, "seleccion_menu.png", Vector2.zero,
                    new Vector2(426, 72), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
                arrow = AddImage("IndicadorTriangular", row, "flecha_menu.png", new Vector2(10, -18),
                    new Vector2(34, 42), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            }
            mainLabels[i] = AddText("Texto" + names[i], row, labels[i], i == 0 ? 51 : 52,
                i == 0 ? new Color32(248, 253, 255, 255) : new Color32(216, 222, 224, 255),
                new Vector2(30, 0), new Vector2(386, 74), TextAnchor.MiddleCenter, 3, 2);
        }

        RectTransform optionsPanel = Rect("PanelOpciones", canvasRect,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-34, -174), new Vector2(540, 720));
        AddImage("MarcoOpciones", optionsPanel, "panel_industrial.png", Vector2.zero,
            new Vector2(540, 720), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        AddText("TituloOpciones", optionsPanel, "OPCIONES", 56,
            new Color32(127, 236, 255, 255), new Vector2(71, -114),
            new Vector2(405, 72), TextAnchor.MiddleCenter, 3, 2);
        RectTransform[] optionsRows = new RectTransform[3];
        Text[] optionsLabels = new Text[3];
        string[] optionNames = { "Volumen", "PantallaCompleta", "Volver" };
        string[] optionLabels = { "VOLUMEN  100%", "PANTALLA COMPLETA  SÍ", "VOLVER" };
        int[] optionY = { 260, 380, 565 };
        for (int i = 0; i < optionsRows.Length; i++)
        {
            optionsRows[i] = Rect("Opcion" + optionNames[i], optionsPanel,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(60, -(optionY[i] - 37)), new Vector2(420, 74));
            optionsLabels[i] = AddText("Texto" + optionNames[i], optionsRows[i],
                optionLabels[i], i == 1 ? 31 : 43, Color.white,
                Vector2.zero, new Vector2(420, 74), TextAnchor.MiddleCenter, 2, 2);
        }
        AddText("AyudaOpciones", optionsPanel, "IZQ / DER  PARA CAMBIAR", 27,
            new Color32(171, 193, 207, 255), new Vector2(71, -470),
            new Vector2(405, 45), TextAnchor.MiddleCenter, 1, 1);
        optionsPanel.gameObject.SetActive(false);

        RectTransform creditsPanel = Rect("PanelCreditos", canvasRect,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-34, -174), new Vector2(540, 720));
        AddImage("MarcoCreditos", creditsPanel, "panel_industrial.png", Vector2.zero,
            new Vector2(540, 720), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        AddText("TituloCreditos", creditsPanel, "CRÉDITOS", 56,
            new Color32(127, 236, 255, 255), new Vector2(71, -114),
            new Vector2(405, 72), TextAnchor.MiddleCenter, 3, 2);
        AddText("ContenidoCreditos", creditsPanel,
            "TECHNOPOLIS\nEL SEMESTRE DEL HAMBRE\n\nEQUIPO TECHNOPOLIS\n\nGRACIAS POR JUGAR", 33,
            Color.white, new Vector2(68, -244), new Vector2(410, 270),
            TextAnchor.MiddleCenter, 2, 1);
        RectTransform creditsBack = Rect("CreditoVolver", creditsPanel,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(60, -528), new Vector2(420, 74));
        Text creditsBackLabel = AddText("TextoVolver", creditsBack, "VOLVER", 43,
            Color.white, Vector2.zero, new Vector2(420, 74), TextAnchor.MiddleCenter, 2, 2);
        creditsPanel.gameObject.SetActive(false);

        RectTransform version = Rect("Version", canvasRect,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-38, 92), new Vector2(95, 42));
        AddText("TextoVersion", version, "v1.0", 31,
            new Color32(222, 230, 228, 255), Vector2.zero, new Vector2(95, 42),
            TextAnchor.MiddleRight, 2, 2);

        TechnopolisMenuRuntime runtime = canvasObject.AddComponent<TechnopolisMenuRuntime>();
        runtime.mainPanel = panel.gameObject;
        runtime.mainRows = mainRows;
        runtime.mainLabels = mainLabels;
        runtime.selectionBackground = selection;
        runtime.selectionArrow = arrow;
        runtime.optionsPanel = optionsPanel.gameObject;
        runtime.optionsRows = optionsRows;
        runtime.optionsLabels = optionsLabels;
        runtime.volumeLabel = optionsLabels[0];
        runtime.fullscreenLabel = optionsLabels[1];
        runtime.creditsPanel = creditsPanel.gameObject;
        runtime.creditsBackRow = creditsBack;
        runtime.creditsBackLabel = creditsBackLabel;

        // The menu owns no camera or EventSystem. Pointer input uses the row
        // RectTransforms, so the level keeps its existing camera and UI system.
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("TECHNOPOLIS_MENU_BUILT scene=" + MenuPath);
    }

    [MenuItem("Technopolis/Menu/Ver barrio con menú (aditivo)")]
    public static void ShowAdditivePreview()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(LevelPath, OpenSceneMode.Single);
        EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Additive);
        SceneView.RepaintAll();
        Debug.Log("TECHNOPOLIS_MENU_ADDITIVE_PREVIEW world=" + LevelPath + " overlay=" + MenuPath);
    }

    public static void Verify()
    {
        EditorSceneManager.OpenScene(LevelPath, OpenSceneMode.Single);
        Scene menu = EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Additive);
        if (!menu.IsValid() || !menu.isLoaded) throw new InvalidOperationException("La escena menu no abre de forma aditiva.");
        GameObject[] roots = menu.GetRootGameObjects();
        if (roots.Length != 1 || roots[0].name != "MenuCanvas")
            throw new InvalidOperationException("El menu debe contener solo MenuCanvas como raiz.");
        Canvas canvas = roots[0].GetComponent<Canvas>();
        CanvasScaler scaler = roots[0].GetComponent<CanvasScaler>();
        if (!canvas || canvas.renderMode != RenderMode.ScreenSpaceOverlay || canvas.sortingOrder < 500 ||
            !scaler || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
            scaler.referenceResolution != new Vector2(1920, 1080))
            throw new InvalidOperationException("Canvas o Canvas Scaler mal configurados.");
        if (roots[0].GetComponentsInChildren<Camera>(true).Length != 0 ||
            roots[0].GetComponentsInChildren<AudioListener>(true).Length != 0 ||
            roots[0].GetComponentsInChildren<Button>(true).Length != 0)
            throw new InvalidOperationException("El menu visual duplico una camara, audio o logica de botones.");
        string[] required = { "EscenarioMenu", "PersonajesMenu", "JugadorSentado", "PerroBitSentado",
            "TituloPrincipal", "Subtitulo", "PanelMenu", "BotonesMenu",
            "PanelOpciones", "PanelCreditos", "Version", "DecoracionesUI" };
        foreach (string name in required)
            if (!FindChild(roots[0].transform, name)) throw new InvalidOperationException("Falta " + name);
        string[] absent = { "RamonaSentada", "IndicadoresInferiores", "PresionaStart",
            "BarraInferior", "IconoA", "Seleccionar" };
        foreach (string name in absent)
            if (FindChild(roots[0].transform, name)) throw new InvalidOperationException("Elemento retirado aun visible: " + name);
        if (GameObject.Find("Jugador") == null || GameObject.Find("Ramona_Npc") == null ||
            GameObject.Find("Main Camera") == null)
            throw new InvalidOperationException("La escena original dejo de mostrar sus personajes o camara.");
        GameObject worldCanvas = GameObject.Find("Canvas");
        Transform oldMenu = worldCanvas ? FindChild(worldCanvas.transform, "PanelPrincipal") : null;
        if (!worldCanvas || !oldMenu || oldMenu.gameObject.activeSelf ||
            !FindChild(worldCanvas.transform, "PanelDialogo") ||
            !FindChild(worldCanvas.transform, "PanelTerminal"))
            throw new InvalidOperationException("El menu antiguo debe estar oculto sin afectar el dialogo ni la terminal.");
        Text[] texts = roots[0].GetComponentsInChildren<Text>(true);
        if (texts.Length != 17) throw new InvalidOperationException("Cantidad inesperada de textos del menu.");
        TechnopolisMenuRuntime runtime = roots[0].GetComponent<TechnopolisMenuRuntime>();
        if (!runtime || runtime.mainRows == null || runtime.mainRows.Length != 5 ||
            runtime.optionsRows == null || runtime.optionsRows.Length != 3 ||
            !runtime.creditsPanel)
            throw new InvalidOperationException("Falta el controlador funcional del menu.");
        foreach (Text text in texts)
            if (!text.font || AssetDatabase.GetAssetPath(text.font) != FontPath)
                throw new InvalidOperationException("Falta la fuente pixel art en " + text.name);
        Debug.Log("TECHNOPOLIS_MENU_OK roots=" + roots.Length + " texts=" + texts.Length +
            " images=" + roots[0].GetComponentsInChildren<Image>(true).Length +
            " resolution=" + scaler.referenceResolution + " originalSceneIntact=true");
    }

    /// <summary>A one-off editor render for visual QA, never saved into either scene.</summary>
    public static void CaptureAdditive1920()
    {
        CapturePage1920("Main", "Game_1920x1080.png");
    }

    public static void CaptureAllPages1920()
    {
        CapturePage1920("Main", "Game_1920x1080.png");
        CapturePage1920("Options", "Options_1920x1080.png");
        CapturePage1920("Credits", "Credits_1920x1080.png");
    }

    static void CapturePage1920(string page, string fileName)
    {
        EditorSceneManager.OpenScene(LevelPath, OpenSceneMode.Single);
        Scene menu = EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Additive);
        Camera worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GameObject root = menu.GetRootGameObjects()[0];
        Canvas menuCanvas = root.GetComponent<Canvas>();
        Transform mainPanel = FindChild(root.transform, "PanelMenu");
        Transform optionsPanel = FindChild(root.transform, "PanelOpciones");
        Transform creditsPanel = FindChild(root.transform, "PanelCreditos");
        mainPanel.gameObject.SetActive(page == "Main");
        optionsPanel.gameObject.SetActive(page == "Options");
        creditsPanel.gameObject.SetActive(page == "Credits");
        RenderTexture target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        target.Create();
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = worldCamera.targetTexture;
        try
        {
            worldCamera.targetTexture = target;
            menuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            menuCanvas.worldCamera = worldCamera;
            menuCanvas.planeDistance = 1;
            Canvas.ForceUpdateCanvases();
            worldCamera.Render();
            RenderTexture.active = target;
            Texture2D shot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            shot.Apply();
            string output = Path.Combine(Directory.GetCurrentDirectory(), "Documentacion/Menu/" + fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, shot.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(shot);
            Debug.Log("TECHNOPOLIS_MENU_CAPTURE " + output);
        }
        finally
        {
            worldCamera.targetTexture = previousTarget;
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            menuCanvas.worldCamera = null;
            RenderTexture.active = previousActive;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    public static void VerifyAndCapture()
    {
        Verify();
        CaptureAllPages1920();
    }

    static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max,
        Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static Image AddImage(string name, Transform parent, string asset, Vector2 position,
        Vector2 size, Vector2 min, Vector2 max, Vector2 pivot)
    {
        return AddSpriteImage(name, parent, Art + asset, position, size, min, max, pivot);
    }

    static Image AddSpriteImage(string name, Transform parent, string assetPath, Vector2 position,
        Vector2 size, Vector2 min, Vector2 max, Vector2 pivot)
    {
        RectTransform rect = Rect(name, parent, min, max, pivot, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (!image.sprite) throw new InvalidOperationException("Falta el sprite " + assetPath);
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    static Text AddText(string name, Transform parent, string value, int size, Color color,
        Vector2 position, Vector2 dimensions, TextAnchor alignment, float outline, float shadow)
    {
        RectTransform rect = Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 1), position, dimensions);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        if (shadow > 0)
        {
            Shadow depth = rect.gameObject.AddComponent<Shadow>();
            depth.effectColor = new Color32(2, 9, 25, 235);
            depth.effectDistance = new Vector2(shadow, -shadow);
        }
        if (outline > 0)
        {
            Outline border = rect.gameObject.AddComponent<Outline>();
            border.effectColor = new Color32(3, 5, 19, 255);
            border.effectDistance = new Vector2(outline, -outline);
        }
        return text;
    }
}
#endif
