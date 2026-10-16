
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarraFelicidadPerro : MonoBehaviour
{
    [Header("Elementos del HUD")]
    [SerializeField] private Image barraRelleno;
    [SerializeField] private Image iconoPerro;
    [SerializeField] private TMP_Text textoPorcentaje;

    [Header("Rostros del perro")]
    [SerializeField] private Sprite perroFeliz;
    [SerializeField] private Sprite perroSerio;
    [SerializeField] private Sprite perroTriste;

    [Header("Configuracion de felicidad")]
    [SerializeField, Range(0, 100)]
    private int felicidad = 100;

    [SerializeField, Min(1f)]
    private float velocidadCambio = 25f;

    private float felicidadVisible;
    private RectTransform rectBarra;

    private readonly Color32 colorVerde =
        new Color32(118, 186, 103, 255);

    private readonly Color32 colorAmarillo =
        new Color32(231, 189, 92, 255);

    private readonly Color32 colorRojo =
        new Color32(210, 105, 82, 255);

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

        rectBarra = HudBarrioVisual.ConfigurarPerro(transform, barraRelleno, iconoPerro, textoPorcentaje);
        HudBarrioVisual.ConfigurarDinero();
        MusicaBarrio.IniciarSiFalta();

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

        rectBarra.anchorMax = new Vector2(Mathf.Clamp01(felicidadVisible / 100f), 1);
        rectBarra.offsetMin = rectBarra.offsetMax = Vector2.zero;

        // Actualizar texto.
        textoPorcentaje.text = porcentaje + "%";

        Sprite spritePerro;
        Color colorTexto;

        if (porcentaje > 50)
        {
            spritePerro = perroFeliz;
            colorTexto = colorVerde;
        }
        else if (porcentaje > 20)
        {
            spritePerro = perroSerio;
            colorTexto = colorAmarillo;
        }
        else
        {
            spritePerro = perroTriste;
            colorTexto = colorRojo;
        }

        // Actualizar rostro.
        if (spritePerro != null &&
            iconoPerro.sprite != spritePerro)
        {
            iconoPerro.sprite = spritePerro;
        }

        barraRelleno.color = colorTexto;
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
