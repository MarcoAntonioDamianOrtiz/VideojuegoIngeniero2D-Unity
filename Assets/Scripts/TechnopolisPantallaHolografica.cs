using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class TechnopolisPantallaHolografica : MonoBehaviour
{
    [SerializeField] bool esFinal;

    static readonly Color Tinta = new Color32(213, 255, 251, 255);
    static readonly Color Cian = new Color32(97, 245, 229, 255);
    static readonly Color Borde = new Color32(49, 130, 143, 255);
    static readonly Color Fondo = new Color32(7, 24, 36, 247);

    CanvasGroup entrada;
    RectTransform barrido;
    Action alAceptar;
    float inicio;

    public static void MostrarControles(Action continuar)
    {
        var raiz = new GameObject("Holograma de controles", typeof(RectTransform));
        raiz.AddComponent<TechnopolisPantallaHolografica>().Construir(continuar);
    }

    void Start()
    {
        if (!esFinal) return;

        var camara = new GameObject("Cámara de cierre", typeof(Camera)).GetComponent<Camera>();
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color32(3, 12, 22, 255);
        camara.cullingMask = 0;
        Construir(TechnopolisGameFlow.VolverAlMenuDesdeFinal);
    }

    void Construir(Action continuar)
    {
        alAceptar = continuar;
        inicio = Time.unscaledTime;
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2010;
        var escala = gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1280, 720);
        escala.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        gameObject.AddComponent<GraphicRaycaster>();
        entrada = gameObject.AddComponent<CanvasGroup>();
        entrada.alpha = 0f;

        if (!EventSystem.current)
        {
            var eventos = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventos.transform.SetParent(transform, false);
        }

        var velo = Bloque("Fondo", transform, 0, 0, 0, 0, new Color32(2, 12, 23, 158));
        velo.rectTransform.anchorMin = Vector2.zero;
        velo.rectTransform.anchorMax = Vector2.one;
        velo.raycastTarget = true;

        float ancho = esFinal ? 830 : 984;
        float alto = esFinal ? 386 : 410;
        var marco = Bloque("Marco holográfico", transform, 0, 0, ancho, alto, Borde).rectTransform;
        Bloque("Panel", marco, 0, 0, ancho - 4, alto - 4, Fondo);
        Bloque("Borde superior", marco, 0, alto / 2, ancho - 72, 3, Cian);
        Bloque("Borde inferior", marco, 0, -alto / 2, ancho - 72, 3, Cian);
        for (int y = 1; y < alto / 2 - 6; y += 12)
        {
            var linea = new Color32(91, 255, 235, 11);
            Bloque("Línea de proyección", marco, 0, y, ancho - 8, 1, linea);
            Bloque("Línea de proyección", marco, 0, -y, ancho - 8, 1, linea);
        }
        barrido = Bloque("Barrido", marco, 0, 0, ancho - 8, 3, new Color32(97, 245, 229, 25)).rectTransform;

        if (esFinal)
        {
            Texto("TECHNOPOLIS", marco, 0, 112, 650, 34, 24, Cian).characterSpacing = 8;
            Texto("FIN DE LA DEMO", marco, 0, 35, 790, 98, 76, Tinta).fontStyle = FontStyles.Bold;
            Texto("Continuará…", marco, 0, -41, 650, 40, 29, Cian);
        }
        else
        {
            Texto("CONTROLES", marco, 0, 130, 920, 74, 57, Tinta).fontStyle = FontStyles.Bold;
            Ficha(marco, -348, "Moverse");
            Tecla(marco, -348, 41, 42, "W");
            Tecla(marco, -396, -5, 42, "A");
            Tecla(marco, -348, -5, 42, "S");
            Tecla(marco, -300, -5, 42, "D");
            Ficha(marco, -116, "Correr");
            Tecla(marco, -116, 18, 116, "SHIFT");
            Ficha(marco, 116, "Interactuar");
            Tecla(marco, 116, 18, 56, "E");
            Ficha(marco, 348, "Abrir el mapa");
            Tecla(marco, 348, 18, 56, "M");
        }

        var imagen = Bloque("Continuar", marco, 0, -135, 280, 58, Borde);
        imagen.raycastTarget = true;
        var boton = imagen.gameObject.AddComponent<UnityEngine.UI.Button>();
        boton.targetGraphic = imagen;
        var colores = boton.colors;
        colores.highlightedColor = Cian;
        colores.pressedColor = new Color32(148, 220, 227, 255);
        boton.colors = colores;
        var navegacion = boton.navigation;
        navegacion.mode = Navigation.Mode.None;
        boton.navigation = navegacion;
        boton.onClick.AddListener(Aceptar);
        Bloque("Fondo del botón", imagen.rectTransform, 0, 0, 274, 52, Fondo);
        Texto(esFinal ? "Volver al menú" : "Empezar", imagen.rectTransform, 0, 0, 274, 52, 25, Tinta);
    }

    static void Ficha(Transform padre, float x, string accion)
    {
        Bloque(accion, padre, x, 3, 216, 150, new Color32(22, 57, 70, 235));
        Texto(accion, padre, x, -48, 212, 34, 23, Tinta);
    }

    static void Tecla(Transform padre, float x, float y, float ancho, string etiqueta)
    {
        Bloque("Base de " + etiqueta, padre, x, y - 4, ancho + 4, 44, Borde);
        var borde = Bloque("Tecla " + etiqueta, padre, x, y, ancho, 42, Cian).rectTransform;
        Bloque("Cara", borde, 0, 0, ancho - 4, 38, new Color32(11, 52, 66, 255));
        Texto(etiqueta, borde, 0, 1, ancho - 4, 38, 23, Tinta).fontStyle = FontStyles.Bold;
    }

    static RectTransform Rectangulo(string nombre, Transform padre, float x, float y, float ancho, float alto)
    {
        var rect = new GameObject(nombre, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(padre, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(ancho, alto);
        return rect;
    }

    static UnityEngine.UI.Image Bloque(string nombre, Transform padre, float x, float y, float ancho, float alto, Color color)
    {
        var imagen = Rectangulo(nombre, padre, x, y, ancho, alto).gameObject.AddComponent<UnityEngine.UI.Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return imagen;
    }

    static TMP_Text Texto(string texto, Transform padre, float x, float y, float ancho, float alto, int tamano, Color color)
    {
        var etiqueta = Rectangulo(texto, padre, x, y, ancho, alto).gameObject.AddComponent<TextMeshProUGUI>();
        etiqueta.text = texto;
        etiqueta.color = color;
        etiqueta.fontSize = tamano;
        etiqueta.alignment = TextAlignmentOptions.Center;
        etiqueta.raycastTarget = false;
        return etiqueta;
    }

    void Update()
    {
        if (!entrada) return;
        float edad = Time.unscaledTime - inicio;
        entrada.alpha = Mathf.Clamp01(edad / .35f);
        float alcance = esFinal ? 188f : 200f;
        barrido.anchoredPosition = new Vector2(0f, Mathf.Lerp(alcance, -alcance, Mathf.Repeat(edad / 3.8f, 1f)));

        bool aceptar = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space);
#if ENABLE_INPUT_SYSTEM
        aceptar |= Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
#endif
        if (aceptar) Aceptar();
    }

    void Aceptar()
    {
        if (alAceptar == null || Time.unscaledTime - inicio < .35f) return;
        Action continuar = alAceptar;
        alAceptar = null;
        gameObject.SetActive(false);
        Destroy(gameObject);
        continuar();
    }
}
