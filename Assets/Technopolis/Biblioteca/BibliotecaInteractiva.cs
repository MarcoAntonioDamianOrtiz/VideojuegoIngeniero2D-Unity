using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Biblioteca del barrio: índice y vocabulario con audios independientes.</summary>
public sealed class BibliotecaInteractiva : MonoBehaviour
{
    [Serializable]
    public sealed class Entrada
    {
        public string numero;
        [TextArea] public string nahuatl;
        [TextArea] public string espanol;
        public AudioClip audio;
    }

    public Entrada[] entradas;
    public MovimientoJugador movimientoJugador;

    enum Vista { Indice, Mexico, Lenguas, Nahuatl }

    AudioSource reproductor;
    Canvas canvas;
    GameObject libro;
    GameObject aviso;
    Image arteLibro;
    RectTransform paginaIzquierda;
    RectTransform paginaDerecha;
    TextMeshProUGUI ruta;
    TextMeshProUGUI audioEstado;
    Vista vista;
    int entradaActual;
    bool cerca;
    bool abierto;
    bool movimientoEstabaActivo;
    int anchoAnterior;
    int altoAnterior;

    static readonly Color Tinta = new Color32(55, 36, 25, 255);
    static readonly Color Papel = new Color32(224, 189, 134, 255);
    static readonly Color PapelClaro = new Color32(243, 215, 162, 255);
    static readonly Color Cobre = new Color32(99, 50, 31, 255);
    static readonly Color Azul = new Color32(44, 84, 80, 255);
    static readonly Color MaderaOscura = new Color32(53, 30, 22, 255);
    static readonly Color MaderaClara = new Color32(154, 90, 49, 255);
    static readonly Color Dorado = new Color32(222, 172, 87, 255);
    static readonly Color Marfil = new Color32(255, 229, 178, 255);

    void Awake()
    {
        reproductor = GetComponent<AudioSource>();
        if (!reproductor) reproductor = gameObject.AddComponent<AudioSource>();
        reproductor.playOnAwake = false;
        ConstruirInterfaz();
    }

    void Update()
    {
        if (anchoAnterior != Screen.width || altoAnterior != Screen.height)
        {
            anchoAnterior = Screen.width;
            altoAnterior = Screen.height;
            AjustarPaginas();
        }

        if (aviso) aviso.SetActive(cerca && !abierto && !TechnopolisGameFlow.IsMenuOpen);
        if (TechnopolisGameFlow.IsMenuOpen)
        {
            if (abierto) Cerrar();
            return;
        }
        if (PresionoE())
        {
            if (abierto) Cerrar();
            else if (cerca) Abrir();
        }
        if (abierto && audioEstado && !reproductor.isPlaying && audioEstado.text == "Reproduciendo…")
        {
            MusicaBarrio.SilenciarPorBiblioteca(false);
            audioEstado.text = "Audio listo para escuchar";
        }
    }

    static bool PresionoE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player")) return;
        cerca = true;
        if (!movimientoJugador) movimientoJugador = otro.GetComponent<MovimientoJugador>();
    }

    void OnTriggerExit2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player")) return;
        cerca = false;
        if (abierto) Cerrar();
    }

    public void Abrir()
    {
        if (!cerca || abierto || TechnopolisGameFlow.IsMenuOpen) return;
        abierto = true;
        entradaActual = 0;
        movimientoEstabaActivo = movimientoJugador && movimientoJugador.enabled;
        if (movimientoJugador) movimientoJugador.enabled = false;
        libro.SetActive(true);
        Mostrar(Vista.Indice);
    }

    public void Cerrar()
    {
        if (!abierto) return;
        reproductor.Stop();
        MusicaBarrio.SilenciarPorBiblioteca(false);
        abierto = false;
        libro.SetActive(false);
        if (movimientoJugador && movimientoEstabaActivo) movimientoJugador.enabled = true;
    }

    void OnDisable()
    {
        if (abierto) Cerrar();
    }

    void ConstruirInterfaz()
    {
        var go = new GameObject("BibliotecaCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var avisoRect = Panel("AvisoBiblioteca", go.transform, Azul, new Vector2(.73f, .04f), new Vector2(.96f, .13f));
        aviso = avisoRect.gameObject;
        Boton("Abrir biblioteca  [E]", avisoRect, new Vector2(0, 0), new Vector2(1, 1), Abrir, Azul, 20);
        aviso.SetActive(false);

        var cubierta = Panel("LibroAbierto", go.transform, new Color(0, 0, 0, .82f), Vector2.zero, Vector2.one);
        libro = cubierta.gameObject;
        var marco = Panel("LibroAntiguo", cubierta, Color.white, new Vector2(.015f, .025f), new Vector2(.985f, .975f));
        arteLibro = marco.GetComponent<Image>();
        arteLibro.sprite = Resources.Load<Sprite>("Biblioteca/libro_antiguo");
        if (!arteLibro.sprite) Debug.LogWarning("No se encontró el arte del libro antiguo.");
        paginaIzquierda = Panel("PaginaIndice", marco, Color.clear, new Vector2(.09f, .14f), new Vector2(.48f, .86f));
        paginaDerecha = Panel("PaginaContenido", marco, Color.clear, new Vector2(.52f, .14f), new Vector2(.91f, .86f));
        var pestana = Panel("PestanaCuero", marco, Cobre, new Vector2(.37f, .86f), new Vector2(.63f, .93f));
        var titulo = Etiqueta("BIBLIOTECA DEL BARRIO", pestana, Vector2.zero, Vector2.one, 21,
            new Color32(255, 229, 174, 255), FontStyles.Bold);
        titulo.alignment = TextAlignmentOptions.Center;
        Boton("X", marco, new Vector2(.90f, .82f), new Vector2(.95f, .90f), Cerrar, Cobre, 27);
        ruta = Etiqueta("", marco, new Vector2(.10f, .055f), new Vector2(.90f, .105f), 16, Marfil);
        libro.SetActive(false);
        AjustarPaginas();
    }

    void AjustarPaginas()
    {
        if (!paginaIzquierda || !paginaDerecha) return;
        bool vertical = Screen.height > Screen.width * 1.12f;
        if (arteLibro)
        {
            arteLibro.sprite = vertical ? null : Resources.Load<Sprite>("Biblioteca/libro_antiguo");
            arteLibro.color = vertical ? Cobre : Color.white;
        }
        paginaIzquierda.GetComponent<Image>().color = vertical ? Papel : Color.clear;
        paginaDerecha.GetComponent<Image>().color = vertical ? PapelClaro : Color.clear;
        if (vertical)
        {
            paginaIzquierda.anchorMin = new Vector2(.045f, .48f);
            paginaIzquierda.anchorMax = new Vector2(.955f, .85f);
            paginaDerecha.anchorMin = new Vector2(.045f, .11f);
            paginaDerecha.anchorMax = new Vector2(.955f, .46f);
        }
        else
        {
            paginaIzquierda.anchorMin = new Vector2(.09f, .14f);
            paginaIzquierda.anchorMax = new Vector2(.48f, .86f);
            paginaDerecha.anchorMin = new Vector2(.52f, .14f);
            paginaDerecha.anchorMax = new Vector2(.91f, .86f);
        }
        paginaIzquierda.offsetMin = paginaIzquierda.offsetMax = Vector2.zero;
        paginaDerecha.offsetMin = paginaDerecha.offsetMax = Vector2.zero;
    }

    void Mostrar(Vista nuevaVista)
    {
        reproductor.Stop();
        MusicaBarrio.SilenciarPorBiblioteca(false);
        vista = nuevaVista;
        Limpiar(paginaIzquierda);
        Limpiar(paginaDerecha);
        audioEstado = null;
        ruta.text = nuevaVista == Vista.Indice ? "Índice" : nuevaVista == Vista.Mexico ? "Índice  >  México" :
            nuevaVista == Vista.Lenguas ? "Índice  >  México  >  Lenguas" : "Índice  >  México  >  Lenguas  >  Náhuatl";

        if (nuevaVista == Vista.Nahuatl) { MostrarEntrada(); return; }
        Encabezado(paginaIzquierda, nuevaVista == Vista.Indice ? "ÍNDICE" : nuevaVista == Vista.Mexico ? "MÉXICO" : "LENGUAS");
        if (nuevaVista == Vista.Indice)
        {
            Fila(paginaIzquierda, "MÉXICO", .76f, () => Mostrar(Vista.Mexico), true, "01");
            Fila(paginaIzquierda, "MUNDO · próximamente", .63f, null, false);
            Fila(paginaIzquierda, "HISTORIA · próximamente", .50f, null, false);
            Fila(paginaIzquierda, "CIENCIA · próximamente", .37f, null, false);
            Fila(paginaIzquierda, "ARTE · próximamente", .24f, null, false);
            Encabezado(paginaDerecha, "BIENVENIDOS");
            Parrafo(paginaDerecha, "Un libro para conocer las voces y los saberes de nuestro mundo.\n\nCapítulo disponible: México.\n\n21 expresiones en náhuatl con audio y traducción.", .20f, .77f);
        }
        else if (nuevaVista == Vista.Mexico)
        {
            Fila(paginaIzquierda, "LENGUAS", .76f, () => Mostrar(Vista.Lenguas), true, "02");
            Fila(paginaIzquierda, "REGIONES · próximamente", .63f, null, false);
            Fila(paginaIzquierda, "TRADICIONES · próximamente", .50f, null, false);
            Fila(paginaIzquierda, "HISTORIA · próximamente", .37f, null, false);
            Volver(paginaIzquierda, () => Mostrar(Vista.Indice));
            Encabezado(paginaDerecha, "MÉXICO");
            Parrafo(paginaDerecha, "Explora las lenguas de México. Los demás capítulos se abrirán en futuras actualizaciones.", .31f, .75f);
        }
        else
        {
            Fila(paginaIzquierda, "NÁHUATL", .76f, () => Mostrar(Vista.Nahuatl), true, "03");
            Fila(paginaIzquierda, "MÁS LENGUAS · próximamente", .63f, null, false);
            Volver(paginaIzquierda, () => Mostrar(Vista.Mexico));
            Encabezado(paginaDerecha, "LENGUAS");
            Parrafo(paginaDerecha, "Escucha el vocabulario náhuatl y lee su transcripción junto a su significado en español.", .32f, .77f);
        }
    }

    void MostrarEntrada()
    {
        Limpiar(paginaIzquierda);
        Limpiar(paginaDerecha);
        if (entradas == null || entradas.Length == 0) return;
        entradaActual = Mathf.Clamp(entradaActual, 0, entradas.Length - 1);
        Entrada e = entradas[entradaActual];
        Encabezado(paginaIzquierda, "NÁHUATL");
        string paginaActual = (entradaActual + 1).ToString("00") + " de " + entradas.Length.ToString("00");
        Parrafo(paginaIzquierda, "VOCABULARIO\n\nEntrada " + paginaActual, .50f, .79f);
        Boton("<  ANTERIOR", paginaIzquierda, new Vector2(.07f, .32f), new Vector2(.46f, .43f), () => CambiarEntrada(-1), Cobre, 17, entradaActual > 0);
        Boton("SIGUIENTE  >", paginaIzquierda, new Vector2(.54f, .32f), new Vector2(.93f, .43f), () => CambiarEntrada(1), Cobre, 17, entradaActual < entradas.Length - 1);
        Volver(paginaIzquierda, () => Mostrar(Vista.Lenguas));

        Encabezado(paginaDerecha, "ESCUCHA Y LEE");
        Etiqueta("Transcripción en náhuatl", paginaDerecha, new Vector2(.07f, .72f), new Vector2(.93f, .80f), 18, Cobre, FontStyles.Bold);
        Etiqueta(e.nahuatl, paginaDerecha, new Vector2(.07f, .48f), new Vector2(.93f, .73f), 29, Tinta);
        Etiqueta("Significado en español", paginaDerecha, new Vector2(.07f, .40f), new Vector2(.93f, .48f), 18, Cobre, FontStyles.Bold);
        Etiqueta(e.espanol, paginaDerecha, new Vector2(.07f, .20f), new Vector2(.93f, .41f), 25, Tinta);
        Boton("ESCUCHAR AUDIO", paginaDerecha, new Vector2(.07f, .075f), new Vector2(.58f, .18f), ReproducirActual, Azul, 19);
        audioEstado = Etiqueta(e.audio ? "Audio listo para escuchar" : "Audio no disponible", paginaDerecha,
            new Vector2(.60f, .075f), new Vector2(.93f, .18f), 15, Tinta);
    }

    void CambiarEntrada(int delta)
    {
        reproductor.Stop();
        MusicaBarrio.SilenciarPorBiblioteca(false);
        entradaActual = Mathf.Clamp(entradaActual + delta, 0, entradas.Length - 1);
        MostrarEntrada();
    }

    void ReproducirActual()
    {
        if (entradas == null || entradaActual >= entradas.Length) return;
        var clip = entradas[entradaActual].audio;
        if (!clip) { if (audioEstado) audioEstado.text = "Audio no disponible"; return; }
        reproductor.Stop();
        reproductor.clip = clip;
        MusicaBarrio.SilenciarPorBiblioteca(true);
        reproductor.Play();
        if (audioEstado) audioEstado.text = "Reproduciendo…";
    }

    static void Limpiar(Transform padre)
    {
        for (int i = padre.childCount - 1; i >= 0; i--) Destroy(padre.GetChild(i).gameObject);
    }

    static RectTransform Panel(string nombre, Transform padre, Color color, Vector2 min, Vector2 max)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        var t = go.GetComponent<RectTransform>();
        t.SetParent(padre, false);
        t.anchorMin = min; t.anchorMax = max;
        t.offsetMin = t.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = color.a > .5f;
        return t;
    }

    static TextMeshProUGUI Etiqueta(string texto, Transform padre, Vector2 min, Vector2 max, int tamano, Color color, FontStyles estilo = FontStyles.Normal)
    {
        var go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        var t = go.GetComponent<RectTransform>();
        t.SetParent(padre, false);
        t.anchorMin = min; t.anchorMax = max;
        t.offsetMin = new Vector2(3, 2); t.offsetMax = new Vector2(-3, -2);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = texto;
        label.fontSize = tamano;
        label.fontStyle = estilo;
        label.color = color;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(13, tamano - 9);
        label.fontSizeMax = tamano;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    static Button Boton(string texto, Transform padre, Vector2 min, Vector2 max, Action accion, Color fondo, int tamano, bool disponible = true)
    {
        var t = Panel("Boton_" + texto, padre, MaderaOscura, min, max);
        var button = t.gameObject.AddComponent<Button>();
        button.interactable = disponible;
        if (accion != null) button.onClick.AddListener(() => accion());

        var cara = Panel("MaderaYPintura", t, disponible ? fondo : new Color32(129, 112, 88, 255), Vector2.zero, Vector2.one);
        cara.offsetMin = new Vector2(4, 5);
        cara.offsetMax = new Vector2(-4, -4);
        cara.GetComponent<Image>().raycastTarget = false;
        button.targetGraphic = cara.GetComponent<Image>();
        var colores = button.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color32(255, 224, 170, 255);
        colores.pressedColor = new Color32(192, 150, 115, 255);
        colores.selectedColor = new Color32(255, 239, 199, 255);
        colores.disabledColor = Color.white;
        colores.fadeDuration = .07f;
        button.colors = colores;

        Color luz = disponible ? (fondo == Azul ? new Color32(116, 178, 154, 255) : Dorado) : new Color32(167, 150, 122, 255);
        Color sombra = disponible ? MaderaOscura : new Color32(105, 91, 77, 255);
        Adorno("LuzSuperior", cara, luz, new Vector2(0, .88f), Vector2.one);
        Adorno("SombraInferior", cara, sombra, Vector2.zero, new Vector2(1, .10f));
        Adorno("CantoIzquierdo", cara, MaderaClara, Vector2.zero, new Vector2(.018f, 1));
        if (texto != "X" && disponible)
        {
            Remache(t, false);
            Remache(t, true);
        }
        var label = Etiqueta(texto, t, new Vector2(.06f, .08f), new Vector2(.94f, .91f), tamano,
            disponible ? Marfil : new Color32(209, 190, 155, 255), FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        return button;
    }

    static RectTransform Adorno(string nombre, Transform padre, Color color, Vector2 min, Vector2 max)
    {
        var t = Panel(nombre, padre, color, min, max);
        t.GetComponent<Image>().raycastTarget = false;
        return t;
    }

    static void Remache(Transform padre, bool derecha)
    {
        var t = Adorno("RemacheBronce", padre, Dorado, Vector2.zero, Vector2.zero);
        t.anchorMin = t.anchorMax = new Vector2(derecha ? 1 : 0, .5f);
        t.sizeDelta = new Vector2(4, 4);
        t.anchoredPosition = new Vector2(derecha ? -9 : 9, 0);
    }

    static void Encabezado(Transform pagina, string titulo)
    {
        Etiqueta(titulo, pagina, new Vector2(.07f, .84f), new Vector2(.93f, .96f), 29, Cobre, FontStyles.Bold);
        Panel("Linea", pagina, Cobre, new Vector2(.07f, .827f), new Vector2(.93f, .835f));
    }

    static void Parrafo(Transform pagina, string texto, float abajo, float arriba)
    {
        Etiqueta(texto, pagina, new Vector2(.08f, abajo), new Vector2(.92f, arriba), 26, Tinta);
    }

    static void Fila(Transform pagina, string texto, float y, Action accion, bool disponible, string capitulo = null)
    {
        var t = Panel("EntradaIndice_" + texto, pagina,
            disponible ? new Color(.55f, .31f, .15f, .19f) : new Color(.45f, .39f, .29f, .07f),
            new Vector2(.07f, y - .095f), new Vector2(.93f, y));
        t.GetComponent<Image>().raycastTarget = disponible;
        var b = t.gameObject.AddComponent<Button>();
        b.targetGraphic = t.GetComponent<Image>();
        if (accion != null) b.onClick.AddListener(() => accion());
        b.interactable = disponible;
        var colores = b.colors;
        colores.highlightedColor = new Color32(255, 232, 187, 255);
        colores.pressedColor = new Color32(194, 154, 107, 255);
        colores.fadeDuration = .07f;
        b.colors = colores;
        Adorno("BordeCobre", t, disponible ? Cobre : new Color32(163, 145, 115, 255),
            Vector2.zero, new Vector2(.012f, 1));
        Adorno("BrilloPapel", t, new Color(1, .90f, .69f, disponible ? .35f : .10f),
            new Vector2(.012f, .94f), Vector2.one);
        var label = Etiqueta(texto, t, new Vector2(.055f, 0), new Vector2(.83f, 1), 22,
            disponible ? Tinta : new Color32(132, 119, 98, 255), disponible ? FontStyles.Bold : FontStyles.Normal);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        if (capitulo != null)
        {
            var sello = Adorno("SelloDeCapitulo", t, Cobre, new Vector2(.85f, .15f), new Vector2(.98f, .85f));
            var numero = Etiqueta(capitulo, sello, Vector2.zero, Vector2.one, 18, Marfil);
            numero.alignment = TextAlignmentOptions.Center;
        }
        Panel("Renglon", pagina, new Color(.37f, .21f, .11f, .24f),
            new Vector2(.07f, y - .103f), new Vector2(.93f, y - .100f));
    }

    static void Volver(Transform pagina, Action accion)
    {
        Boton("<  VOLVER", pagina, new Vector2(.07f, .075f), new Vector2(.43f, .18f), accion, Cobre, 18);
    }
}
