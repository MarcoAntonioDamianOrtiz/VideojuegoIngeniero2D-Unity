using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MinijuegoAleatorioPanaderia : MonoBehaviour
{
    [System.Serializable]
    public struct PreguntaMinijuego
    {
        [TextArea(2, 4)]
        public string enunciado;
        public string[] opciones; // 3 opciones por pregunta
        public int indiceCorrecto; // 0, 1 o 2
    }

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI textoEnunciado;
    public Image[] imagenesCorazones; // Arrastra Corazon1, Corazon2 y Corazon3
    public Button[] botonesOpciones;   // Arrastra BotonRespuesta1, BotonRespuesta2 y BotonRespuesta3

    [Header("Base de Datos de Preguntas")]
    public List<PreguntaMinijuego> listaPreguntas = new List<PreguntaMinijuego>();

    [Header("Configuración del Juego")]
    public int maxVidas = 3;
    public GameObject panelTerminal; // Para cerrar la terminal al ganar o perder

    public event System.Action<bool> ResultadoCampana;
    private List<PreguntaMinijuego> pendientes;
    private bool terminado;
    bool Corporativa => gameObject.scene.name == "EscenaNivel3Silicio";
    private bool exito;
    private int vidasActuales;
    private int preguntaActualIndice;

    void OnEnable()
    {
        // Se ejecuta automáticamente cada vez que abres la terminal con la 'E'
        IniciarMinijuego();
    }

    public void IniciarMinijuego()
    {
        vidasActuales = maxVidas;
        ActualizarVidasUI();

        // Si la lista está vacía en el inspector, carga las preguntas base por código
        if (listaPreguntas == null || listaPreguntas.Count == 0)
        {
            CargarPreguntasPorDefecto();
        }

        CancelInvoke(nameof(CerrarTerminal));
        terminado = false;
        pendientes = Corporativa ? Diagnostico() : new List<PreguntaMinijuego>(listaPreguntas);
        if (Corporativa)
        {
            var titulo = transform.Find("TextoTitulo");
            if (titulo && titulo.TryGetComponent<TextMeshProUGUI>(out var texto))
                texto.text = "SILICON ALLEY · DIAGNÓSTICO";
        }
        foreach (Button boton in botonesOpciones) boton.interactable = true;
        CargarSiguientePregunta();
    }

    void CargarPreguntasPorDefecto()
    {
        listaPreguntas = new List<PreguntaMinijuego>()
        {
            new PreguntaMinijuego {
                enunciado = "Sintaxis: Doña Ramona quiere calcular el precio total del pan con IVA. ¿Cuál sentencia en C# es correcta?",
                opciones = new string[] { "float total = precio + (precio * 0.16f);", "float total = precio + 0.16;", "total float = precio * 1.16;" },
                indiceCorrecto = 0
            },
            new PreguntaMinijuego {
                enunciado = "Secuencia: ¿Cuál es el orden lógico para vender un pan en el punto de venta?",
                opciones = new string[] { "Cobrar() -> EntregarPan() -> SeleccionarProducto()", "SeleccionarProducto() -> Cobrar() -> EntregarPan()", "EntregarPan() -> SeleccionarProducto() -> Cobrar()" },
                indiceCorrecto = 1
            },
            new PreguntaMinijuego {
                enunciado = "Lógica Booleana: El horno solo prende si hay gas (True) Y la puerta está cerrada (True). Evaluación:",
                opciones = new string[] { "true && true -> TRUE", "true || false -> FALSE", "true && false -> TRUE" },
                indiceCorrecto = 0
            }
        };
    }

    static List<PreguntaMinijuego> Diagnostico() => new List<PreguntaMinijuego>
    {
        new PreguntaMinijuego {
            enunciado = "El servicio falló tras un cambio. ¿Qué revisas primero?",
            opciones = new[] { "Registros y cambios recientes", "Colores de la interfaz", "Nombres de usuarios" },
            indiceCorrecto = 0
        },
        new PreguntaMinijuego {
            enunciado = "Alteraron la configuración. ¿Cómo recuperas el servicio?",
            opciones = new[] { "Borrar los datos", "Restaurar la versión validada", "Desactivar los registros" },
            indiceCorrecto = 1
        },
        new PreguntaMinijuego {
            enunciado = "El servicio ya inicia. ¿Cómo compruebas la reparación?",
            opciones = new[] { "Cerrar la terminal", "Revisar el icono", "Probar el flujo completo" },
            indiceCorrecto = 2
        }
    };

    void CargarSiguientePregunta()
    {
        if (pendientes.Count == 0) return;

        preguntaActualIndice = Random.Range(0, pendientes.Count);
        PreguntaMinijuego q = pendientes[preguntaActualIndice];

        textoEnunciado.text = $"Restantes: {pendientes.Count}  ·  Vidas: {vidasActuales}\n{q.enunciado}";

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            int index = i;
            TextMeshProUGUI btnText = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = q.opciones[i];

            // Limpia clicks previos y asigna el evento del botón
            botonesOpciones[i].onClick.RemoveAllListeners();
            botonesOpciones[i].onClick.AddListener(() => Responder(index));
        }
    }

    public void Responder(int indiceSeleccionado)
    {
        if (terminado || pendientes == null || pendientes.Count == 0 || TechnopolisGameFlow.IsMenuOpen) return;
        bool acierto = indiceSeleccionado == pendientes[preguntaActualIndice].indiceCorrecto;
        var campana = FindFirstObjectByType<TechnopolisCampana>();
        if (campana) campana.Sonido(acierto);
        if (acierto)
        {
            Debug.Log("¡Respuesta Correcta!");
            pendientes.RemoveAt(preguntaActualIndice);

            if (pendientes.Count > 0)
            {
                CargarSiguientePregunta();
            }
            else
            {
                textoEnunciado.text = Corporativa
                    ? "¡SERVICIO RESTAURADO!\nDon Dinero tiene una propuesta para Alex."
                    : "¡SISTEMA REPARADO CON ÉXITO!\nGracias por ayudar a Doña Ramona.";
                if (!Corporativa) EconomiaBarrio.RecibirPagoPanaderia();
                Finalizar(true);
            }
        }
        else
        {
            Debug.Log("Respuesta Incorrecta");
            vidasActuales--;
            ActualizarVidasUI();

            if (vidasActuales <= 0)
            {
                textoEnunciado.text = "¡ERROR DEL SISTEMA!\nInténtalo de nuevo.";
                Finalizar(false);
                return;
            }
            textoEnunciado.text = $"Respuesta incorrecta. Vidas: {vidasActuales}\n{pendientes[preguntaActualIndice].enunciado}";
        }
    }

    void ActualizarVidasUI()
    {
        // Enciende/apaga las imágenes de los corazones según las vidas restantes
        for (int i = 0; i < imagenesCorazones.Length; i++)
        {
            if (i < vidasActuales)
            {
                imagenesCorazones[i].gameObject.SetActive(true);
            }
            else
            {
                imagenesCorazones[i].gameObject.SetActive(false);
            }
        }
    }

    void Finalizar(bool reparada)
    {
        terminado = true;
        exito = reparada;
        foreach (Button boton in botonesOpciones) boton.interactable = false;
        Invoke(nameof(CerrarTerminal), 2f);
    }

    void CerrarTerminal()
    {
        // Cierra el panel de la terminal
        if (panelTerminal != null) panelTerminal.SetActive(false);

        ResultadoCampana?.Invoke(exito);

        // Reactiva el movimiento del personaje si la referencia existe
        InteraccionDonaRamona scriptRamona = FindFirstObjectByType<InteraccionDonaRamona>();
        if (scriptRamona != null && scriptRamona.scriptMovimientoAlex != null)
        {
            scriptRamona.scriptMovimientoAlex.enabled = true;
        }
    }
}