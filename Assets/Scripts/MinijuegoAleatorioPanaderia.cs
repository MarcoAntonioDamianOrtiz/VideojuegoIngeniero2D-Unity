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

    void CargarSiguientePregunta()
    {
        if (listaPreguntas.Count == 0) return;

        preguntaActualIndice = Random.Range(0, listaPreguntas.Count);
        PreguntaMinijuego q = listaPreguntas[preguntaActualIndice];

        textoEnunciado.text = q.enunciado;

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
        if (indiceSeleccionado == listaPreguntas[preguntaActualIndice].indiceCorrecto)
        {
            Debug.Log("¡Respuesta Correcta!");
            listaPreguntas.RemoveAt(preguntaActualIndice);

            if (listaPreguntas.Count > 0)
            {
                CargarSiguientePregunta();
            }
            else
            {
                textoEnunciado.text = "¡SISTEMA REPARADO CON ÉXITO!\nGracias por ayudar a Doña Ramona.";
                Invoke("CerrarTerminal", 2f);
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
                Invoke("CerrarTerminal", 2f);
            }
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

    void CerrarTerminal()
    {
        // Cierra el panel de la terminal
        if (panelTerminal != null) panelTerminal.SetActive(false);

        // Reactiva el movimiento del personaje si la referencia existe
        InteraccionDonaRamona scriptRamona = FindObjectOfType<InteraccionDonaRamona>();
        if (scriptRamona != null && scriptRamona.scriptMovimientoAlex != null)
        {
            scriptRamona.scriptMovimientoAlex.enabled = true;
        }
    }
}