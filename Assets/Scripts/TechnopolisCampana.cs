using UnityEngine;
using Unity.Profiling;

public sealed class TechnopolisCampana : MonoBehaviour
{
    static readonly ProfilerMarker MedirObjetivo = new ProfilerMarker("Technopolis.Objetivo");
    int nivel;
    int paso;
    MovimientoJugador jugador;
    MinijuegoAleatorioPanaderia terminal;
    Vector2 punto;
    AudioSource audioEvento;
    enum Oficina { Calle, Encargo, Sabotaje, Terminal, Reintento, Contrato, Final, Diego }
    Oficina oficina;
    Texture2D retratoDonDinero;
    Texture2D retratoDiego;
    string final;
    bool cargando;
    bool calibrando;
    int conexion;
    string objetivo;
    HabitantesSilicio habitantes;

    public void Configurar(int numero, MovimientoJugador personaje)
    {
        nivel = numero;
        habitantes=FindFirstObjectByType<HabitantesSilicio>();
        jugador = personaje;
        Vector2 origen = jugador.transform.position;
        foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
        {
            if (!terminal) terminal = raiz.GetComponentInChildren<MinijuegoAleatorioPanaderia>(true);
            if (nivel == 3)
            {
                foreach (BoxCollider2D caja in raiz.GetComponentsInChildren<BoxCollider2D>())
                {
                    if (caja.name != "Edificio_Silicon_Alley") continue;
                    Vector2 frente = new Vector2(caja.bounds.center.x, caja.bounds.min.y - .65f);
                    if (punto == Vector2.zero || (frente - origen).sqrMagnitude < (punto - origen).sqrMagnitude)
                        punto = frente;
                }
            }
            else if (nivel == 2)
            {
                foreach (BoxCollider2D caja in raiz.GetComponentsInChildren<BoxCollider2D>())
                    if (caja.name == "Taller")
                        punto = new Vector2(caja.bounds.center.x, caja.bounds.min.y - .65f);
            }
        }
        objetivo = "Conecta el módulo en orden: alimentación, señal, prueba.";
        var distrito = FindFirstObjectByType<DistritoNivel2>();
        if (nivel == 2 && distrito && distrito.taller) punto = distrito.taller.position;
        audioEvento = gameObject.AddComponent<AudioSource>();
        audioEvento.playOnAwake = false;
        if (nivel == 3 && terminal)
        {
            terminal.ResultadoCampana += ResultadoTerminal;
            retratoDonDinero = Resources.Load<Texture2D>("Personajes/DonDineroTransparente");
            retratoDiego = Resources.Load<Texture2D>("Personajes/DiegoByteTransparente");
        }
    }

    bool Cerca => ((Vector2)jugador.transform.position - punto).sqrMagnitude <= 2.25f || (habitantes&&habitantes.Cerca(jugador.transform.position));
    public bool TieneInteraccion => nivel==3&&oficina==Oficina.Calle&&Cerca&&!TechnopolisGameFlow.IsMenuOpen;
    public string AccionInteraccion => habitantes&&habitantes.CercaDiego(jugador.transform.position)?"Hablar con Diego Byte (E)":"Hablar con Don Dinero (E)";
    public void AbrirInteraccion(){if(TieneInteraccion)Interactuar();}
    bool TerminalAbierta => terminal && terminal.gameObject.activeInHierarchy;

    void Start()
    {
        if (jugador.TryGetComponent<Collider2D>(out _)) return;
        Bounds sprite = jugador.GetComponent<SpriteRenderer>().bounds;
        Transform t = jugador.transform;
        var pies = jugador.gameObject.AddComponent<BoxCollider2D>();
        pies.offset = t.InverseTransformPoint(new Vector3(sprite.center.x, sprite.min.y + .12f, t.position.z));
        pies.size = new Vector2(.42f / Mathf.Abs(t.lossyScale.x), .24f / Mathf.Abs(t.lossyScale.y));
    }

    void Update()
    {
        using (MedirObjetivo.Auto())
        {
            if (!jugador || cargando || oficina != Oficina.Calle || calibrando || TerminalAbierta || TechnopolisGameFlow.IsMenuOpen) return;
            if (Cerca && Input.GetKeyDown(KeyCode.E)) Interactuar();
        }
    }

    void Interactuar()
    {
        if (nivel == 3)
        {
            if (!terminal) return;
            jugador.enabled = false;
            oficina = habitantes&&habitantes.CercaDiego(jugador.transform.position)?Oficina.Diego:Oficina.Encargo;
            return;
        }
        if (nivel == 2)
        {
            calibrando = true;
            jugador.enabled = false;
        }
    }

    void Conectar(int entrada)
    {
        if (paso > 0) return;
        bool correcta = entrada == conexion;
        Sonido(correcta);
        conexion = correcta ? conexion + 1 : 0;
        if (conexion < 3)
        {
            objetivo = correcta ? $"Conexión {conexion}/3. Sigue el orden: alimentación, señal, prueba."
                : "Orden incorrecto. Reinicia: alimentación, señal, prueba.";
            return;
        }
        paso = 1;
        ProgresoTechnopolis.Premiar("taller", 5000, 20);
        objetivo = "Módulo calibrado. Trabajo del taller completado.";
    }

    void ResultadoTerminal(bool exito)
    {
        if (exito) ProgresoTechnopolis.Premiar("auditoria", 10000, 15);
        jugador.enabled = false;
        oficina = exito ? Oficina.Contrato : Oficina.Reintento;
    }

    void DejarContratoEnSuspenso()
    {
        final = "El contrato sigue sobre la mesa, sin firmar.\n\n" +
            "Alex mira a Bit. Antes de responder, una notificación ilumina su pantalla…\n\n" +
            "Su aventura apenas comienza.\n\nCONTINUARÁ…";
        oficina = Oficina.Final;
        Sonido(true);
    }

    void AbrirTerminal()
    {
        oficina = Oficina.Terminal;
        RestaurarCanvas();
        interfazOculta = false;
        terminal.gameObject.SetActive(true);
    }

    readonly System.Collections.Generic.Dictionary<Canvas, bool> estadoCanvas =
        new System.Collections.Generic.Dictionary<Canvas, bool>();
    bool interfazOculta;
    TechnopolisDialogoOficina dialogoOficina;
    Oficina estadoMostrado = (Oficina)(-1);

    void ActualizarDialogo()
    {
        bool visible = nivel == 3 && oficina != Oficina.Calle &&
            oficina != Oficina.Terminal && !TechnopolisGameFlow.IsMenuOpen;
        if (!visible)
        {
            if (dialogoOficina) dialogoOficina.gameObject.SetActive(false);
            estadoMostrado = (Oficina)(-1);
            return;
        }
        if (!dialogoOficina)
        {
            Transform original = terminal.transform.parent.Find("PanelDialogo");
            if (!original) return;
            var raiz = new GameObject("DialogosOficina", typeof(RectTransform));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(raiz, gameObject.scene);
            dialogoOficina = raiz.AddComponent<TechnopolisDialogoOficina>();
            dialogoOficina.Configurar(original, terminal, retratoDonDinero, retratoDiego);
        }
        if (estadoMostrado == oficina) return;
        estadoMostrado = oficina;
        switch (oficina)
        {
            case Oficina.Diego:
                dialogoOficina.Mostrar(true,"Así que tú eres Alex… Don Dinero te espera en la entrada.\n\nAquí no basta con tener suerte. Veremos qué sabes hacer.","Volver",()=>{oficina=Oficina.Calle;jugador.enabled=true;},null,null);
                break;
            case Oficina.Encargo:
                dialogoOficina.Mostrar(false,
                    "Alex, conecta el módulo y revisa la terminal.\n\nNecesito el diagnóstico antes de aprobar el trabajo.",
                    "Empezar la misión", () => { oficina = Oficina.Sabotaje; Sonido(false); },
                    "Salir de la oficina", () => { oficina = Oficina.Calle; jugador.enabled = true; });
                break;
            case Oficina.Sabotaje:
                dialogoOficina.Mostrar(true,
                    "Diego altera la configuración desde su portátil y el servicio se cae.\n\n«A ver si también sabes arreglar esto.»\n\nResuelve tres pruebas. Tienes tres vidas.",
                    "Diagnosticar el sabotaje", AbrirTerminal, null, null);
                break;
            case Oficina.Reintento:
                dialogoOficina.Mostrar(false,
                    "Se agotaron las vidas. Puedes reiniciar el diagnóstico con tres intentos.",
                    "Reintentar", AbrirTerminal, null, null);
                break;
            case Oficina.Contrato:
                dialogoOficina.Mostrar(false,
                    "Restauraste el sistema. Te ofrezco un contrato millonario.\n\n" +
                    "Pero hay una condición: tendrás que viajar varios meses sin Bit.\n\n" +
                    "Don Dinero desliza el contrato hacia Alex y espera su respuesta…",
                    "Continuar", DejarContratoEnSuspenso, null, null);
                break;
            case Oficina.Final:
                dialogoOficina.Mostrar(false, "FIN DE LA DEMO\n\n" + final,
                    "Volver al menú", TechnopolisGameFlow.ReturnToMenu, null, null);
                break;
        }
    }

    void LateUpdate()
    {
        ActualizarDialogo();
        bool ocultar = nivel == 3 && oficina != Oficina.Calle &&
            oficina != Oficina.Terminal && !TechnopolisGameFlow.IsMenuOpen;
        if (ocultar == interfazOculta) return;
        interfazOculta = ocultar;
        if (ocultar)
        {
            estadoCanvas.Clear();
            foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
            foreach (Canvas canvas in raiz.GetComponentsInChildren<Canvas>(true))
            {
                if (dialogoOficina && canvas == dialogoOficina.CanvasDialogo) continue;
                estadoCanvas[canvas] = canvas.enabled;
                canvas.enabled = false;
            }
        }
        else RestaurarCanvas();
    }

    void RestaurarCanvas()
    {
        foreach (var estado in estadoCanvas)
            if (estado.Key) estado.Key.enabled = estado.Value;
        estadoCanvas.Clear();
    }

    void OnDisable()
    {
        RestaurarCanvas();
        interfazOculta = false;
    }

    public void Sonido(bool acierto)
    {
        if (!audioEvento) return;
        audioEvento.PlayOneShot(acierto ? tonoAcierto : tonoError, .3f);
    }

    static AudioClip tonoAcierto;
    static AudioClip tonoError;
    void Awake()
    {
        if (!tonoAcierto) tonoAcierto = CrearTono("Acierto", 660f, 880f);
        if (!tonoError) tonoError = CrearTono("Error", 330f, 220f);
    }

    static AudioClip CrearTono(string nombre, float primera, float segunda)
    {
        const int frecuencia = 22050;
        float[] muestras = new float[4410];
        float fase = 0f;
        for (int i = 0; i < muestras.Length; i++)
        {
            fase += 2f * Mathf.PI * (i < muestras.Length / 2 ? primera : segunda) / frecuencia;
            muestras[i] = Mathf.Sin(fase) * Mathf.Sin(Mathf.PI * i / muestras.Length);
        }
        AudioClip clip = AudioClip.Create(nombre, muestras.Length, 1, frecuencia, false);
        clip.SetData(muestras, 0);
        return clip;
    }

    void OnGUI()
    {
        if(nivel==3)return;
        if (!jugador || cargando || TerminalAbierta || TechnopolisGameFlow.IsMenuOpen ||
            oficina != Oficina.Calle || (!calibrando && !Cerca)) return;
        float ancho = Mathf.Min(630f, Screen.width - 24f);
        GUILayout.BeginArea(new Rect((Screen.width - ancho) / 2f, Screen.height - 140f, ancho, 130f), GUI.skin.box);
        if (calibrando)
        {
            GUILayout.Label(objetivo, GUI.skin.textArea);
            if (paso == 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Señal")) Conectar(1);
                if (GUILayout.Button("Prueba")) Conectar(2);
                if (GUILayout.Button("Alimentación")) Conectar(0);
                GUILayout.EndHorizontal();
            }
            else if (GUILayout.Button(ProgresoTechnopolis.PuedeAvanzar(3) ? "Viajar a La Ciudad de Silicio" : "Falta " + ProgresoTechnopolis.Requisito(3)))
            {
                if (ProgresoTechnopolis.PuedeAvanzar(3))
                {
                    cargando = true;
                    TechnopolisGameFlow.AvanzarCampana(3);
                }
            }
            if (GUILayout.Button("Salir del taller"))
            {
                calibrando = false;
                jugador.enabled = true;
            }
        }
        else if (GUILayout.Button(nivel == 3 ? "Entrar a la oficina (E)" : "Entrar al taller (E)"))
            Interactuar();
        GUILayout.EndArea();
    }
}
