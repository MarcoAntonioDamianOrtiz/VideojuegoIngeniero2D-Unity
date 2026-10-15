
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarraFelicidadPerro : MonoBehaviour
{
    [Header("Elementos del HUD")]
    [SerializeField] private Image barraRelleno;
    [SerializeField] private Image iconoPerro;
    [SerializeField] private TMP_Text textoPorcentaje;

    [Header("Sprites de las barras")]
    [SerializeField] private Sprite barraVerde;
    [SerializeField] private Sprite barraAmarilla;
    [SerializeField] private Sprite barraRoja;

    [Header("Rostros del perro")]
    [SerializeField] private Sprite perroFeliz;
    [SerializeField] private Sprite perroSerio;
    [SerializeField] private Sprite perroTriste;

    [Header("Posicion y tamaño VERDE")]
    [SerializeField]
    private Vector2 posicionVerde =
        new Vector2(-431.0315f, 223.25f);

    [SerializeField]
    private Vector2 tamanoVerde =
        new Vector2(290f, 62.5f);

    [Header("Posicion y tamaño AMARILLO")]
    [SerializeField]
    private Vector2 posicionAmarillo =
        new Vector2(-425f, 230f);

    [SerializeField]
    private Vector2 tamanoAmarillo =
        new Vector2(280f, 40f);

    [Header("Posicion y tamaño ROJO")]
    [SerializeField]
    private Vector2 posicionRojo =
        new Vector2(-425f, 223.25f);

    [SerializeField]
    private Vector2 tamanoRojo =
        new Vector2(280f, 62.5f);

    [Header("Configuracion de felicidad")]
    [SerializeField, Range(0, 100)]
    private int felicidad = 100;

    [SerializeField, Min(1f)]
    private float velocidadCambio = 25f;

    private float felicidadVisible;
    private RectTransform rectBarra;

    // Guarda el estado para evitar redimensionar
    // innecesariamente en cada fotograma.
    private int estadoAnterior = -1;

    private readonly Color32 colorVerde =
        new Color32(85, 255, 51, 255);

    private readonly Color32 colorAmarillo =
        new Color32(255, 208, 43, 255);

    private readonly Color32 colorRojo =
        new Color32(255, 56, 56, 255);

    private void Start()
    {
        if (barraRelleno == null ||
            iconoPerro == null ||
            textoPorcentaje == null)
        {
            Debug.LogError(
                "Faltan referencias del HUD del perro.",
                this
            );

            enabled = false;
            return;
        }

        rectBarra = barraRelleno.rectTransform;

        // Anclaje central como en tus capturas.
        rectBarra.anchorMin = new Vector2(0.5f, 0.5f);
        rectBarra.anchorMax = new Vector2(0.5f, 0.5f);
        rectBarra.pivot = new Vector2(0.5f, 0.5f);
        rectBarra.localScale = Vector3.one;

        // Recorte horizontal.
        barraRelleno.type = Image.Type.Filled;
        barraRelleno.fillMethod =
            Image.FillMethod.Horizontal;
        barraRelleno.fillOrigin = 0;
        barraRelleno.preserveAspect = false;
        barraRelleno.color = Color.white;

        felicidadVisible = felicidad;
        ActualizarHUD();
    }

    private void Update()
    {
        felicidadVisible = Mathf.MoveTowards(
            felicidadVisible,
            felicidad,
            velocidadCambio * Time.deltaTime
        );

        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        int porcentaje = Mathf.Clamp(
            Mathf.RoundToInt(felicidadVisible),
            0,
            100
        );

        // La barra disminuye progresivamente.
        barraRelleno.fillAmount =
            Mathf.Clamp01(felicidadVisible / 100f);

        // Actualizar texto.
        textoPorcentaje.text = porcentaje + "%";

        int estado;
        Sprite spritePerro;
        Color colorTexto;

        if (porcentaje > 50)
        {
            estado = 0; // VERDE
            spritePerro = perroFeliz;
            colorTexto = colorVerde;
        }
        else if (porcentaje > 20)
        {
            estado = 1; // AMARILLO
            spritePerro = perroSerio;
            colorTexto = colorAmarillo;
        }
        else
        {
            estado = 2; // ROJO
            spritePerro = perroTriste;
            colorTexto = colorRojo;
        }

        // Solo actualizar sprite y tamaño
        // cuando cambia de estado.
        if (estado != estadoAnterior)
        {
            estadoAnterior = estado;
            CambiarEstadoBarra(estado);
        }

        // Actualizar rostro.
        if (spritePerro != null &&
            iconoPerro.sprite != spritePerro)
        {
            iconoPerro.sprite = spritePerro;
        }

        // Actualizar color del porcentaje.
        textoPorcentaje.color = colorTexto;
    }

    private void CambiarEstadoBarra(int estado)
    {
        Sprite nuevoSprite;
        Vector2 nuevaPosicion;
        Vector2 nuevoTamano;

        switch (estado)
        {
            case 0: // VERDE
                nuevoSprite = barraVerde;
                nuevaPosicion = posicionVerde;
                nuevoTamano = tamanoVerde;
                break;

            case 1: // AMARILLO
                nuevoSprite = barraAmarilla;
                nuevaPosicion = posicionAmarillo;
                nuevoTamano = tamanoAmarillo;
                break;

            default: // ROJO
                nuevoSprite = barraRoja;
                nuevaPosicion = posicionRojo;
                nuevoTamano = tamanoRojo;
                break;
        }

        // Cambiar imagen.
        if (nuevoSprite != null)
        {
            barraRelleno.sprite = nuevoSprite;
        }

        // Cambiar ancho y alto.
        rectBarra.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            nuevoTamano.x
        );

        rectBarra.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            nuevoTamano.y
        );

        // Cambiar posicion.
        rectBarra.anchoredPosition = nuevaPosicion;

        // Mantener escala original.
        rectBarra.localScale = Vector3.one;
    }

    // Cambiar felicidad desde otros scripts.
    public void EstablecerFelicidad(int valor)
    {
        felicidad = Mathf.Clamp(valor, 0, 100);
    }

    public void ModificarFelicidad(int cantidad)
    {
        EstablecerFelicidad(felicidad + cantidad);
    }

    public int ObtenerFelicidad()
    {
        return felicidad;
    }

    [ContextMenu("Probar 100%")]
    private void Probar100()
    {
        EstablecerFelicidad(100);
    }

    [ContextMenu("Probar 50%")]
    private void Probar50()
    {
        EstablecerFelicidad(50);
    }

    [ContextMenu("Probar 20%")]
    private void Probar20()
    {
        EstablecerFelicidad(20);
    }

    [ContextMenu("Probar 0%")]
    private void Probar0()
    {
        EstablecerFelicidad(0);
    }
}
