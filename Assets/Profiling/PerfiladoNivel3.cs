#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

public sealed class PerfiladoNivel3 : MonoBehaviour
{
    const BindingFlags Privado = BindingFlags.NonPublic | BindingFlags.Instance;
    const float Paso = .5f;
    readonly List<Medicion> muestras = new List<Medicion>(12000);
    readonly Dictionary<string, ProfilerRecorder> contadores = new Dictionary<string, ProfilerRecorder>();
    readonly List<Collider2D> contactos = new List<Collider2D>(32);
    readonly List<RaycastHit2D> cruces = new List<RaycastHit2D>(32);
    readonly FrameTiming[] tiempos = new FrameTiming[1];
    readonly List<Vector2> ruta = new List<Vector2>();
    string carpeta;
    string fase = "Calentamiento";
    string corrida;
    float inicio;
    bool midiendo;
    int indiceRuta;
    MovimientoJugador jugador;
    Rigidbody2D cuerpo;
    Transform bit;
    TechnopolisCampana campana;
    MinijuegoAleatorioPanaderia terminal;
    Vector2 centroColision;
    Vector2 tamanoColision;
    ContactFilter2D filtro;
    int puntosAlcanzados;

    struct Medicion
    {
        public string fase;
        public int frame;
        public float segundo;
        public float ms;
        public float x, y;
        public double cpu, render, gpu, espera;
        public long main, physics, memory, gc, draws, batches, triangles;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "-profileNivel3") < 0) return;
        new GameObject("Captura de rendimiento Nivel 3").AddComponent<PerfiladoNivel3>();
    }

    static string Argumento(string clave, string valor)
    {
        string[] args = Environment.GetCommandLineArgs();
        int indice = Array.IndexOf(args, clave);
        return indice >= 0 && indice + 1 < args.Length ? args[indice + 1] : valor;
    }

    IEnumerator Start()
    {
        carpeta = Argumento("-profileOutput", Application.persistentDataPath);
        corrida = Argumento("-profileRun", "baseline");
        Directory.CreateDirectory(carpeta);
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        yield return new WaitForSecondsRealtime(3);
        // Close only the overlay. Do not start/reset a saved game or enable autosaving.
        typeof(TechnopolisGameFlow).GetMethod("CloseMenu", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        yield return new WaitForSecondsRealtime(2);
        jugador = FindFirstObjectByType<MovimientoJugador>();
        cuerpo = jugador.GetComponent<Rigidbody2D>();
        bit = GameObject.Find("Bit").transform;
        campana = FindFirstObjectByType<TechnopolisCampana>();
        terminal = Campo<MinijuegoAleatorioPanaderia>(campana, "terminal");
        Collider2D pies = jugador.GetComponent<Collider2D>();
        centroColision = (Vector2)pies.bounds.center - (Vector2)jugador.transform.position;
        tamanoColision = (Vector2)pies.bounds.size + Vector2.one * .2f;
        filtro = new ContactFilter2D { useTriggers = false };
        filtro.SetLayerMask(Physics2D.GetLayerCollisionMask(jugador.gameObject.layer));
        PrepararRuta();
        yield return new WaitForSecondsRealtime(3);
        EnumerarContadores();
        Profiler.logFile = Path.Combine(carpeta, corrida + ".raw");
        Profiler.enableBinaryLog = true;
        Profiler.maxUsedMemory = 256 * 1024 * 1024;
        Profiler.enabled = true;
        Profiler.SetAreaEnabled(ProfilerArea.GPU, true);
        inicio = Time.realtimeSinceStartup;
        yield return Medir("Reposo", 10);
        yield return Medir("Recorrido con Bit", 40);
        jugador.SetTouchDirection(Vector2.zero);
        // Move to the doorway only between measurement phases.
        cuerpo.position = Campo<Vector2>(campana, "punto");
        jugador.transform.position = cuerpo.position;
        Physics2D.SyncTransforms();
        yield return new WaitForSecondsRealtime(2);
        Llamar(campana, "Interactuar");
        yield return Medir("Dialogo Don Dinero", 6);
        var dialogo = FindFirstObjectByType<TechnopolisDialogoOficina>();
        typeof(TechnopolisDialogoOficina).GetMethod("Elegir", Privado).Invoke(dialogo, new object[] { false });
        yield return Medir("Sabotaje Diego", 6);
        Llamar(campana, "AbrirTerminal");
        yield return Medir("Minijuego pregunta 1", 4);
        ResponderCorrecto();
        yield return Medir("Minijuego pregunta 2", 4);
        ResponderCorrecto();
        yield return Medir("Minijuego pregunta 3", 4);
        ResponderCorrecto();
        yield return new WaitForSecondsRealtime(3);
        yield return Medir("Decision contrato", 5);
        typeof(TechnopolisCampana).GetMethod("ElegirContrato", Privado).Invoke(campana, new object[] { false });
        yield return Medir("Final demo", 5);
        Profiler.enabled = false;
        Profiler.enableBinaryLog = false;
        Profiler.logFile = "";
        Guardar();
        foreach (ProfilerRecorder contador in contadores.Values) contador.Dispose();
        Application.Quit();
    }

    IEnumerator Medir(string nombre, float segundos)
    {
        fase = nombre;
        // Let UI changes and delayed GPU results settle outside the sample window.
        yield return new WaitForSecondsRealtime(.5f);
        midiendo = true;
        yield return new WaitForSecondsRealtime(segundos);
        midiendo = false;
        ScreenCapture.CaptureScreenshot(Path.Combine(carpeta, corrida + "-" + nombre.Replace(" ", "_") + ".png"));
        yield return new WaitForSecondsRealtime(.5f);
    }

    static T Campo<T>(object objeto, string nombre)
    {
        return (T)objeto.GetType().GetField(nombre, Privado).GetValue(objeto);
    }

    static void Llamar(object objeto, string metodo)
    {
        objeto.GetType().GetMethod(metodo, Privado).Invoke(objeto, null);
    }

    void ResponderCorrecto()
    {
        var preguntas = Campo<List<MinijuegoAleatorioPanaderia.PreguntaMinijuego>>(terminal, "pendientes");
        int indice = Campo<int>(terminal, "preguntaActualIndice");
        terminal.Responder(preguntas[indice].indiceCorrecto);
    }

    void EnumerarContadores()
    {
        string[] nombres = { "Main Thread", "Render Thread", "Physics2D.Simulate",
            "Total Used Memory", "GC Allocated In Frame", "Draw Calls Count",
            "Batches Count", "Triangles Count", "CPU Main Thread Frame Time", "GPU Frame Time" };
        var disponibles = new List<ProfilerRecorderHandle>();
        ProfilerRecorderHandle.GetAvailable(disponibles);
        var texto = new StringBuilder("category,name,unit\n");
        foreach (var handle in disponibles)
        {
            var descripcion = ProfilerRecorderHandle.GetDescription(handle);
            texto.Append(descripcion.Category.Name).Append(',').Append(descripcion.Name.Replace(",", ";"))
                .Append(',').Append(descripcion.UnitType).AppendLine();
            if (Array.IndexOf(nombres, descripcion.Name) >= 0 && !contadores.ContainsKey(descripcion.Name))
                contadores.Add(descripcion.Name, ProfilerRecorder.StartNew(descripcion.Category, descripcion.Name, 1,
                    ProfilerRecorderOptions.Default | ProfilerRecorderOptions.SumAllSamplesInFrame));
        }
        File.WriteAllText(Path.Combine(carpeta, corrida + "-counters.csv"), texto.ToString());
    }

    long Valor(string nombre)
    {
        return contadores.TryGetValue(nombre, out var contador) && contador.Valid ? contador.LastValue : -1;
    }

    void Update()
    {
        FrameTimingManager.CaptureFrameTimings();
        if (!jugador || !midiendo || fase != "Recorrido con Bit") return;
        if (indiceRuta >= ruta.Count) { jugador.SetTouchDirection(Vector2.zero); return; }
        Vector2 delta = ruta[indiceRuta] - cuerpo.position;
        if (delta.sqrMagnitude < .0064f)
        {
            indiceRuta++;
            puntosAlcanzados++;
            jugador.SetTouchDirection(Vector2.zero);
        }
        else jugador.SetTouchDirection(delta.normalized);
    }

    void LateUpdate()
    {
        if (!midiendo) return;
        uint recibidos = FrameTimingManager.GetLatestTimings(1, tiempos);
        FrameTiming timing = recibidos > 0 ? tiempos[0] : default;
        muestras.Add(new Medicion {
            fase = fase, frame = Time.frameCount, segundo = Time.realtimeSinceStartup - inicio,
            ms = Time.unscaledDeltaTime * 1000, x = cuerpo.position.x, y = cuerpo.position.y, cpu = timing.cpuMainThreadFrameTime,
            render = timing.cpuRenderThreadFrameTime, gpu = timing.gpuFrameTime,
            espera = timing.cpuMainThreadPresentWaitTime,
            main = Valor("Main Thread"), physics = Valor("Physics2D.Simulate"),
            memory = Valor("Total Used Memory"), gc = Valor("GC Allocated In Frame"),
            draws = Valor("Draw Calls Count"), batches = Valor("Batches Count"), triangles = Valor("Triangles Count")
        });
    }

    bool Libre(Vector2 posicion)
    {
        Physics2D.OverlapBox(posicion + centroColision, tamanoColision, 0, filtro, contactos);
        foreach (Collider2D contacto in contactos)
        {
            if (contacto.transform == jugador.transform || contacto.transform == bit) continue;
            if (contacto.attachedRigidbody && contacto.attachedRigidbody.bodyType != RigidbodyType2D.Static) continue;
            return false;
        }
        return true;
    }

    bool CruceLibre(Vector2 desde, Vector2 hasta)
    {
        Vector2 delta = hasta - desde;
        Physics2D.BoxCast(desde + centroColision, tamanoColision, 0, delta.normalized, filtro, cruces, delta.magnitude);
        foreach (var cruce in cruces)
        {
            Collider2D contacto = cruce.collider;
            if (contacto.transform == jugador.transform || contacto.transform == bit) continue;
            if (contacto.attachedRigidbody && contacto.attachedRigidbody.bodyType != RigidbodyType2D.Static) continue;
            return false;
        }
        return true;
    }

    Vector2Int Celda(Vector2 posicion) => new Vector2Int(Mathf.RoundToInt(posicion.x / Paso), Mathf.RoundToInt(posicion.y / Paso));
    Vector2 Mundo(Vector2Int celda) => new Vector2(celda.x * Paso, celda.y * Paso);

    Vector2Int Cercana(Vector2 posicion)
    {
        Vector2Int centro = Celda(posicion);
        for (int radio = 0; radio < 18; radio++)
        for (int y = -radio; y <= radio; y++)
        for (int x = -radio; x <= radio; x++)
        {
            if (Mathf.Abs(x) != radio && Mathf.Abs(y) != radio) continue;
            Vector2Int celda = centro + new Vector2Int(x, y);
            if (Libre(Mundo(celda))) return celda;
        }
        throw new Exception("No se encontró suelo libre para el recorrido.");
    }

    void PrepararRuta()
    {
        Vector2Int desde = Cercana(cuerpo.position);
        cuerpo.position = Mundo(desde);
        jugador.transform.position = cuerpo.position;
        Physics2D.SyncTransforms();
        Vector2[] destinos = { new Vector2(16, 22), new Vector2(43, 24),
            new Vector2(49, 33), new Vector2(31, 35), Campo<Vector2>(campana, "punto") };
        Vector2Int[] direcciones = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (Vector2 destino in destinos)
        {
            Vector2Int hasta = Cercana(destino);
            var cola = new Queue<Vector2Int>();
            var anteriores = new Dictionary<Vector2Int, Vector2Int>();
            cola.Enqueue(desde);
            anteriores[desde] = desde;
            while (cola.Count > 0 && !anteriores.ContainsKey(hasta))
            {
                var actual = cola.Dequeue();
                foreach (var direccion in direcciones)
                {
                    var vecino = actual + direccion;
                    if (vecino.x < -8 || vecino.y < -8 || vecino.x > 140 || vecino.y > 110 ||
                        anteriores.ContainsKey(vecino) || !Libre(Mundo(vecino)) ||
                        !CruceLibre(Mundo(actual), Mundo(vecino))) continue;
                    anteriores[vecino] = actual;
                    cola.Enqueue(vecino);
                }
            }
            if (!anteriores.ContainsKey(hasta)) continue;
            var tramo = new List<Vector2>();
            for (var celda = hasta; celda != desde; celda = anteriores[celda]) tramo.Add(Mundo(celda));
            tramo.Reverse();
            ruta.AddRange(tramo);
            desde = hasta;
        }
    }

    void Guardar()
    {
        var csv = new StringBuilder("phase,frame,seconds,frame_ms,cpu_main_ms,cpu_render_ms,gpu_ms,present_wait_ms,main_marker_ns,physics2d_ns,memory_bytes,gc_alloc_bytes,draw_calls,batches,triangles,player_x,player_y\n");
        foreach (var m in muestras)
            csv.AppendFormat(CultureInfo.InvariantCulture,
                "{0},{1},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8},{9},{10},{11},{12},{13},{14},{15:F4},{16:F4}\n",
                m.fase, m.frame, m.segundo, m.ms, m.cpu, m.render, m.gpu, m.espera,
                m.main, m.physics, m.memory, m.gc, m.draws, m.batches, m.triangles, m.x, m.y);
        File.WriteAllText(Path.Combine(carpeta, corrida + "-frames.csv"), csv.ToString());
        File.WriteAllText(Path.Combine(carpeta, corrida + "-environment.txt"),
            "UTC=" + DateTime.UtcNow.ToString("O") + "\nUnity=" + Application.unityVersion +
            "\nOS=" + SystemInfo.operatingSystem + "\nCPU=" + SystemInfo.processorType +
            "\nRAM_MB=" + SystemInfo.systemMemorySize + "\nGPU=" + SystemInfo.graphicsDeviceName +
            "\nVRAM_MB=" + SystemInfo.graphicsMemorySize + "\nAPI=" + SystemInfo.graphicsDeviceType +
            "\nResolution=" + Screen.width + "x" + Screen.height + "\nTargetFPS=60\nVSync=0" +
            "\nFrameTimingEnabled=" + FrameTimingManager.IsFeatureEnabled() +
            "\nRouteWaypoints=" + ruta.Count + "\nRouteWaypointsReached=" + puntosAlcanzados +
            "\nCapture=Development Player, no Deep Profiling, scripted movement, normal dialogue/minigame methods.");
    }
}
#endif
