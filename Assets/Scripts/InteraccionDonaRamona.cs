using UnityEngine;
using TMPro;

public class InteraccionDonaRamona : MonoBehaviour
{
    [Header("UI de Diálogo")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;
    [TextArea(2, 4)]
    public string mensajeNPC = "¡Ay, Alex! Se descompuso la terminal del inventario y no puedo registrar el pan... ¿Podrías ayudarme a reparar el sistema?";

    [Header("UI del Minijuego")]
    public GameObject panelTerminalMinijuego;

    [Header("Referencia al Jugador")]
    public MonoBehaviour scriptMovimientoAlex;

    // Estados internos
    private bool jugadorEnTrigger = false;
    private bool dialogoActivo = false;

    void Start()
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);
        if (panelTerminalMinijuego != null) panelTerminalMinijuego.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Interactuar();
    }

    public bool PuedeInteractuar => jugadorEnTrigger &&
        (panelTerminalMinijuego == null || !panelTerminalMinijuego.activeSelf);

    public bool DialogoActivo => dialogoActivo;

    // Lo llaman tanto el teclado como los controles táctiles.
    public void Interactuar()
    {
        if (!PuedeInteractuar) return;

        if (dialogoActivo) CerrarDialogoYAbrirMinijuego();
        else AbrirDialogo();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnTrigger = true;
            Debug.Log("Habla con Doña Ramona con E o con el botón en pantalla.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnTrigger = false;
            CerrarTodo();
        }
    }

    void AbrirDialogo()
    {
        dialogoActivo = true;
        if (panelDialogo != null) panelDialogo.SetActive(true);
        if (textoDialogo != null) textoDialogo.text = mensajeNPC;
        if (scriptMovimientoAlex != null) scriptMovimientoAlex.enabled = false; // Congelar a Alex
    }

    void CerrarDialogoYAbrirMinijuego()
    {
        dialogoActivo = false;
        if (panelDialogo != null) panelDialogo.SetActive(false);

        // Abre el minijuego de la terminal
        if (panelTerminalMinijuego != null)
        {
            panelTerminalMinijuego.SetActive(true);
        }
    }

    void CerrarTodo()
    {
        dialogoActivo = false;
        if (panelDialogo != null) panelDialogo.SetActive(false);
        if (panelTerminalMinijuego != null) panelTerminalMinijuego.SetActive(false);
        if (scriptMovimientoAlex != null) scriptMovimientoAlex.enabled = true; // Descongelar a Alex
    }
}
