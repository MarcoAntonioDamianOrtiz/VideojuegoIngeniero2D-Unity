using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class BitSeguidor : MonoBehaviour
{
    public Transform jugador;
    public float velocidad = 3.6f;
    public float distanciaDetener = 2.4f;

    readonly List<Vector2> recorrido = new List<Vector2>();
    readonly List<RaycastHit2D> impactos = new List<RaycastHit2D>();
    Rigidbody2D cuerpo;
    BoxCollider2D caja;
    Animator animator;
    SpriteRenderer dibujo;
    bool alcanzandoJugador;
    Collider2D colisionJugadorIgnorada;
    string animacionActual = string.Empty;
    float siguienteRuta;
    List<Vector2> rutaAlterna = new List<Vector2>();

    void Awake()
    {
        animator = GetComponent<Animator>();
        cuerpo = GetComponent<Rigidbody2D>();
        caja = GetComponent<BoxCollider2D>();
        dibujo = GetComponent<SpriteRenderer>();
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        CambiarAnimacion("Bit_Idle");
    }

    private void FixedUpdate()
    {
        if (!jugador || !caja) return;

        // The player collider may be created by the city scene at startup.
        if (!colisionJugadorIgnorada && jugador.TryGetComponent<Collider2D>(out var colisionJugador))
        {
            Physics2D.IgnoreCollision(caja, colisionJugador);
            colisionJugadorIgnorada = colisionJugador;
        }

        Vector2 origen = cuerpo.position;
        Vector2 destino = (Vector2)jugador.position + Vector2.up * .5f;
        // Remember the corners the player took while a building blocks the shortcut.
        if (recorrido.Count == 0 || (destino - recorrido[recorrido.Count - 1]).sqrMagnitude >= .04f)
            recorrido.Add(destino);

        if (CaminoLibre(origen, destino))
        {
            if (recorrido.Count > 24) recorrido.RemoveRange(0, recorrido.Count - 24);
            float margen = Mathf.Clamp(distanciaDetener, 1f, 1.4f);
            float distancia = Vector2.Distance(origen, destino);
            float umbral = alcanzandoJugador ? margen + .001f : margen + .5f;
            alcanzandoJugador = distancia > umbral;
            if (alcanzandoJugador)
                Avanzar(origen, destino, margen);
            else
                CambiarAnimacion("Bit_Idle");
            return;
        }

        for (int i = recorrido.Count - 1; i >= 0; i--)
        {
            Vector2 punto = recorrido[i];
            if ((punto - origen).sqrMagnitude < .0025f || !CaminoLibre(origen, punto)) continue;
            if (i > 0) recorrido.RemoveRange(0, i);
            alcanzandoJugador = true;
            Avanzar(origen, punto, 0f);
            return;
        }
        if (Time.time >= siguienteRuta)
        {
            siguienteRuta = Time.time + 1f;
            rutaAlterna = BitRutaObstaculos.Buscar(origen, destino, caja, jugador);
        }
        while (rutaAlterna.Count > 0 && Vector2.Distance(origen, rutaAlterna[0]) < .16f) rutaAlterna.RemoveAt(0);
        if (rutaAlterna.Count > 0) Avanzar(origen, rutaAlterna[0], 0);
        else CambiarAnimacion("Bit_Idle");
    }

    private bool CaminoLibre(Vector2 origen, Vector2 destino)
    {
        Vector2 delta = destino - origen;
        float distancia = delta.magnitude;
        if (distancia < .001f) return true;

        Bounds limites = caja.bounds;
        Vector2 centro = origen + (Vector2)limites.center - (Vector2)transform.position;
        var filtro = new ContactFilter2D { useTriggers = false };
        Physics2D.BoxCast(centro, (Vector2)limites.size, 0f, delta / distancia, filtro, impactos, distancia);
        foreach (RaycastHit2D impacto in impactos)
        {
            if (impacto.rigidbody == cuerpo || impacto.transform == jugador ||
                impacto.transform.IsChildOf(jugador)) continue;
            return false;
        }
        return true;
    }

    private void Avanzar(Vector2 origen, Vector2 destino, float margen)
    {
        Vector2 diferencia = destino - origen;
        float distancia = diferencia.magnitude;
        if (distancia <= margen + .001f)
        {
            CambiarAnimacion("Bit_Idle");
            return;
        }

        Vector2 direccion = diferencia / distancia;
        // The player walks at 3 units per second. Bit needs extra speed to catch up.
        float avance = Mathf.Min(Mathf.Max(velocidad, 3.6f) * Time.fixedDeltaTime, distancia - margen);
        cuerpo.MovePosition(origen + direccion * avance);
        ActualizarAnimacion(direccion);
    }


    private void ActualizarAnimacion(Vector2 paso)
    {
        if (Mathf.Abs(paso.x) > Mathf.Abs(paso.y))
            CambiarAnimacion(paso.x > 0 ? "Caminar_derecha_bit" : "Caminar_izquierda_bit");
        else
            CambiarAnimacion(paso.y > 0 ? "Caminar_espaldas_bit" : "Caminar_frente_bit");
    }


    void LateUpdate()
    {
        // Level 3 sorts its actors and buildings together in TechnopolisOrdenBarrio.
        if (gameObject.scene.name != "EscenaNivel3Silicio" && dibujo && dibujo.sprite)
            dibujo.sortingOrder = TechnopolisOrdenBarrio.Orden(dibujo.bounds.min.y, 1.25f);
    }

    private void CambiarAnimacion(string nombre)
    {
        if (animacionActual == nombre) return;
        animator.Play(nombre, 0, 0f);
        animacionActual = nombre;
    }
}
