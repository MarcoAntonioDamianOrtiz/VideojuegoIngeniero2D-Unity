using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DeviceApplication = UnityEngine.Device.Application;
using DeviceScreen = UnityEngine.Device.Screen;

// Ajusta la interfaz existente a la zona visible y añade controles táctiles.
[RequireComponent(typeof(Canvas))]
public class RamonaResponsiveUI : MonoBehaviour
{
    private const float Margin = 18f;
    private RectTransform canvasRect;
    private RectTransform dialogo;
    private RectTransform terminal;
    private RectTransform nombre;
    private RectTransform mensaje;
    private RectTransform indicador;
    private RectTransform titulo;
    private RectTransform enunciado;
    private RectTransform vidas;
    private readonly RectTransform[] respuestas = new RectTransform[3];
    private InteraccionDonaRamona ramona;
    private MovimientoJugador jugador;
    private GameObject botonHablar;
    private GameObject cruceta;
    private TextMeshProUGUI textoIndicador;
    private int lastWidth = -1;
    private int lastHeight = -1;
    private Rect lastSafeArea;
    private bool lastMobileState;

    private void Awake()
    {
        canvasRect = (RectTransform)transform;
        dialogo = transform.Find("PanelDialogo") as RectTransform;
        terminal = transform.Find("PanelTerminal") as RectTransform;
        if (dialogo == null || terminal == null) return;

        nombre = dialogo.Find("TextoNombreNPC") as RectTransform;
        mensaje = dialogo.Find("TextoDialogo") as RectTransform;
        indicador = dialogo.Find("TextoIndicador") as RectTransform;
        titulo = terminal.Find("TextoTitulo") as RectTransform;
        enunciado = terminal.Find("TextoEnunciado") as RectTransform;
        vidas = terminal.Find("ContenedorVidas") as RectTransform;
        for (int i = 0; i < respuestas.Length; i++)
            respuestas[i] = terminal.Find("BotonRespuesta" + (i + 1)) as RectTransform;

        ramona = FindFirstObjectByType<InteraccionDonaRamona>();
        jugador = FindFirstObjectByType<MovimientoJugador>();

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
        }

        if (ramona != null)
        {
            Button tocarDialogo = dialogo.GetComponent<Button>();
            if (tocarDialogo == null) tocarDialogo = dialogo.gameObject.AddComponent<Button>();
            tocarDialogo.transition = Selectable.Transition.None;
            tocarDialogo.onClick.AddListener(ramona.Interactuar);
        }

        textoIndicador = indicador != null ? indicador.GetComponent<TextMeshProUGUI>() : null;
        lastMobileState = DeviceApplication.isMobilePlatform;
        ActualizarIndicador();
    }

    private void Start()
    {
        if (dialogo == null || terminal == null) return;
        if (ramona != null) botonHablar = CrearBotonHablar();
        if (jugador != null) cruceta = CrearCruceta();
        Ajustar();
    }

    private void LateUpdate()
    {
        if (dialogo == null || terminal == null) return;
        bool modoMovil = DeviceApplication.isMobilePlatform;
        if (modoMovil != lastMobileState)
        {
            lastMobileState = modoMovil;
            ActualizarIndicador();
        }
        if (DeviceScreen.width != lastWidth || DeviceScreen.height != lastHeight || DeviceScreen.safeArea != lastSafeArea)
            Ajustar();

        bool jugando = terminal.gameObject.activeSelf;
        bool hablando = dialogo.gameObject.activeSelf;
        if (botonHablar != null)
            botonHablar.SetActive(modoMovil && ramona != null && ramona.PuedeInteractuar && !hablando && !jugando);
        if (cruceta != null)
            cruceta.SetActive(modoMovil && !hablando && !jugando && jugador != null && jugador.enabled);
    }

    private void ActualizarIndicador()
    {
        if (textoIndicador != null)
            textoIndicador.text = lastMobileState
                ? "Toca aquí para continuar"
                : "Presiona E para continuar";
    }

    private void Ajustar()
    {
        lastWidth = DeviceScreen.width;
        lastHeight = DeviceScreen.height;
        lastSafeArea = DeviceScreen.safeArea;
        if (lastWidth <= 0 || lastHeight <= 0 || canvasRect.rect.width <= 0f) return;

        // Canvas en Screen Space Overlay: convertir el área segura de píxeles a unidades UI.
        float cw = canvasRect.rect.width;
        float ch = canvasRect.rect.height;
        float safeW = cw * lastSafeArea.width / lastWidth;
        float safeH = ch * lastSafeArea.height / lastHeight;
        float cx = cw * (lastSafeArea.center.x / lastWidth - 0.5f);
        float cy = ch * (lastSafeArea.center.y / lastHeight - 0.5f);
        float bottom = cy - safeH * 0.5f;

        float dw = Mathf.Min(1060f, Mathf.Max(280f, safeW - 2f * Margin));
        float dh = Mathf.Min(232f, safeH * 0.38f);
        Colocar(dialogo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f),
            new Vector2(cx, bottom + Margin), new Vector2(dw, dh));
        Colocar(nombre, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -12f), new Vector2(dw - 40f, 36f));
        Colocar(mensaje, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -53f), new Vector2(dw - 40f, Mathf.Max(56f, dh - 103f)));
        Colocar(indicador, new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-20f, 10f), new Vector2(dw - 40f, 30f));
        AjustarTexto(nombre, 29f, 19f);
        AjustarTexto(mensaje, 25f, 16f);
        AjustarTexto(indicador, 21f, 14f);
        if (textoIndicador != null) textoIndicador.alignment = TextAlignmentOptions.Right;

        float tw = Mathf.Min(1080f, Mathf.Max(280f, safeW - 2f * Margin));
        float th = Mathf.Min(590f, Mathf.Max(340f, safeH - 2f * Margin));
        Colocar(terminal, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(cx, cy), new Vector2(tw, th));
        Colocar(titulo, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(24f, -18f), new Vector2(Mathf.Max(110f, tw - 210f), 43f));
        AjustarTexto(titulo, 30f, 17f);

        Colocar(vidas, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-24f, -14f), new Vector2(176f, 56f));
        if (vidas != null)
        {
            HorizontalLayoutGroup layout = vidas.GetComponent<HorizontalLayoutGroup>();
            if (layout != null) layout.enabled = false;
            for (int i = 0; i < vidas.childCount; i++)
            {
                RectTransform corazon = vidas.GetChild(i) as RectTransform;
                Colocar(corazon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(i * 60f, 0f), new Vector2(54f, 52f));
                Image imagen = corazon != null ? corazon.GetComponent<Image>() : null;
                if (imagen != null) imagen.preserveAspect = true;
            }
        }

        float buttonH = Mathf.Clamp((th - 280f) / 3f, 56f, 75f);
        float step = buttonH + 12f;
        float questionH = Mathf.Max(56f, th - 90f - (24f + 2f * step + buttonH) - 18f);
        Colocar(enunciado, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(24f, -90f), new Vector2(tw - 48f, questionH));
        AjustarTexto(enunciado, 27f, 17f);

        for (int i = 0; i < respuestas.Length; i++)
        {
            RectTransform respuesta = respuestas[i];
            Colocar(respuesta, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 24f + (2 - i) * step), new Vector2(tw - 48f, buttonH));
            if (respuesta == null) continue;
            TextMeshProUGUI texto = respuesta.GetComponentInChildren<TextMeshProUGUI>();
            if (texto == null) continue;
            Colocar(texto.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(tw - 76f, buttonH - 8f));
            texto.enableAutoSizing = true;
            texto.fontSizeMin = 16f;
            texto.fontSizeMax = 26f;
            texto.alignment = TextAlignmentOptions.Center;
            texto.enableWordWrapping = true;
            texto.raycastTarget = false;
        }

        if (botonHablar != null)
            Colocar((RectTransform)botonHablar.transform, new Vector2(0.5f, 0.5f),
                new Vector2(1f, 0f), new Vector2(cx + safeW * 0.5f - 22f, bottom + 22f),
                new Vector2(170f, 60f));
        if (cruceta != null)
            Colocar((RectTransform)cruceta.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(cx - safeW * 0.5f + 22f, bottom + 22f),
                new Vector2(184f, 184f));
    }

    private static void Colocar(RectTransform rect, Vector2 ancla, Vector2 pivote,
        Vector2 posicion, Vector2 tamano)
    {
        if (rect == null) return;
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.pivot = pivote;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
        rect.localScale = Vector3.one;
    }

    private static void AjustarTexto(RectTransform rect, float max, float min)
    {
        if (rect == null) return;
        TextMeshProUGUI texto = rect.GetComponent<TextMeshProUGUI>();
        if (texto == null) return;
        texto.enableAutoSizing = true;
        texto.fontSizeMin = min;
        texto.fontSizeMax = max;
        texto.enableWordWrapping = true;
        texto.raycastTarget = false;
    }

    private GameObject CrearBotonHablar()
    {
        GameObject go = CrearPanel("BotonHablarRamona", transform, new Color(0.13f, 0.22f, 0.26f, 0.93f));
        Button boton = go.AddComponent<Button>();
        boton.onClick.AddListener(ramona.Interactuar);
        CrearEtiqueta(go.transform, "HABLAR", 25f);
        go.SetActive(false);
        return go;
    }

    private GameObject CrearCruceta()
    {
        GameObject root = new GameObject("MovimientoTactil", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        CrearDireccion(root.transform, "Arriba", "▲", new Vector2(65f, 122f), Vector2.up);
        CrearDireccion(root.transform, "Izquierda", "◀", new Vector2(6f, 63f), Vector2.left);
        CrearDireccion(root.transform, "Derecha", "▶", new Vector2(124f, 63f), Vector2.right);
        CrearDireccion(root.transform, "Abajo", "▼", new Vector2(65f, 4f), Vector2.down);
        root.SetActive(false);
        return root;
    }

    private void CrearDireccion(Transform parent, string nombreBoton, string simbolo,
        Vector2 posicion, Vector2 direccion)
    {
        GameObject go = CrearPanel(nombreBoton, parent, new Color(0.09f, 0.17f, 0.19f, 0.75f));
        Colocar((RectTransform)go.transform, Vector2.zero, Vector2.zero, posicion, new Vector2(54f, 54f));
        MobileDirectionButton control = go.AddComponent<MobileDirectionButton>();
        control.Configurar(jugador, direccion);
        CrearEtiqueta(go.transform, simbolo, 26f);
    }

    private static GameObject CrearPanel(string nombre, Transform parent, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static void CrearEtiqueta(Transform parent, string contenido, float fontSize)
    {
        GameObject label = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        label.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)label.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(4f, 4f);
        rect.offsetMax = new Vector2(-4f, -4f);
        TextMeshProUGUI texto = label.GetComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = fontSize;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        texto.raycastTarget = false;
    }
}

public class MobileDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IPointerExitHandler
{
    private MovimientoJugador jugador;
    private Vector2 direccion;

    public void Configurar(MovimientoJugador objetivo, Vector2 vector)
    {
        jugador = objetivo;
        direccion = vector;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (jugador != null) jugador.SetTouchDirection(direccion);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (jugador != null) jugador.SetTouchDirection(Vector2.zero);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (jugador != null) jugador.SetTouchDirection(Vector2.zero);
    }

    private void OnDisable()
    {
        if (jugador != null) jugador.SetTouchDirection(Vector2.zero);
    }
}
