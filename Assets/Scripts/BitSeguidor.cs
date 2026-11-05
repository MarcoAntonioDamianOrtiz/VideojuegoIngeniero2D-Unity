using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class BitSeguidor : MonoBehaviour
{
    [Header("Jugador")]
    public Transform jugador;

    [Header("Movimiento")]
    public float velocidad = 3.4f;
    public float distanciaDetener = 1.1f;
    [SerializeField] private float velocidadAlcance = 5.1f;
    [SerializeField] private float distanciaAlcance = 3f;
    [SerializeField] private float margenReanudar = .28f;
    [SerializeField] private float separacionHuellas = .25f;

    // El sprite de Bit conserva su pivote central. Sus patas quedan debajo de este punto.
    private const float OffsetPatas = .52f;
    private const float LlegadaHuella = .28f;
    private const int MaxHuellas = 120;

    private readonly List<Vector2> huellas = new List<Vector2>(MaxHuellas);
    private readonly RaycastHit2D[] impactos = new RaycastHit2D[8];
    private Animator animator;
    private Rigidbody2D cuerpo;
    private SpriteRenderer dibujo;
    private ContactFilter2D filtro;
    private Vector2 ultimaMuestra;
    private int huellaActual;
    private bool caminando;
    private string animacionActual = string.Empty;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cuerpo = GetComponent<Rigidbody2D>();
        dibujo = GetComponent<SpriteRenderer>();
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        filtro = new ContactFilter2D();
        filtro.useTriggers = false;
        filtro.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    }

    private void Start()
    {
        if (jugador)
        {
            ultimaMuestra = jugador.position;
            huellas.Add(ultimaMuestra);
        }
        CambiarAnimacion("Bit_Idle");
    }

    private void FixedUpdate()
    {
        if (!jugador) return;
        RegistrarHuellas(jugador.position);

        Vector2 patas = cuerpo.position + Vector2.down * OffsetPatas;
        float distanciaJugador = Vector2.Distance(patas, jugador.position);
        if (distanciaJugador <= distanciaDetener ||
            (!caminando && distanciaJugador < distanciaDetener + margenReanudar))
        {
            caminando = false;
            CambiarAnimacion("Bit_Idle");
            return;
        }

        caminando = true;
        while (huellaActual < huellas.Count - 1 &&
               Vector2.Distance(patas, huellas[huellaActual]) < LlegadaHuella)
            huellaActual++;

        Vector2 destino = huellas[huellaActual] + Vector2.up * OffsetPatas;
        Vector2 diferencia = destino - cuerpo.position;
        if (diferencia.sqrMagnitude < .0025f && huellaActual < huellas.Count - 1)
        {
            huellaActual++;
            diferencia = huellas[huellaActual] + Vector2.up * OffsetPatas - cuerpo.position;
        }
        if (diferencia.sqrMagnitude < .0001f)
        {
            CambiarAnimacion("Bit_Idle");
            return;
        }

        float rapidez = distanciaJugador > distanciaAlcance ? velocidadAlcance : velocidad;
        Vector2 paso = diferencia.normalized * Mathf.Min(rapidez * Time.fixedDeltaTime, diferencia.magnitude);
        paso = AjustarPasoPorObstaculos(paso);
        if (paso.sqrMagnitude < .000001f)
        {
            CambiarAnimacion("Bit_Idle");
            return;
        }

        cuerpo.MovePosition(cuerpo.position + paso);
        ActualizarAnimacion(paso);
    }

    private void RegistrarHuellas(Vector2 posicion)
    {
        Vector2 diferencia = posicion - ultimaMuestra;
        while (diferencia.magnitude >= separacionHuellas)
        {
            ultimaMuestra += diferencia.normalized * separacionHuellas;
            huellas.Add(ultimaMuestra);
            diferencia = posicion - ultimaMuestra;
        }
        if (huellas.Count <= MaxHuellas) return;
        int quitar = huellas.Count - MaxHuellas;
        huellas.RemoveRange(0, quitar);
        huellaActual = Mathf.Max(0, huellaActual - quitar);
    }

    private Vector2 AjustarPasoPorObstaculos(Vector2 paso)
    {
        float distancia = paso.magnitude;
        Vector2 direccion = paso / distancia;
        int total = cuerpo.Cast(direccion, filtro, impactos, distancia + .03f);
        if (total == 0) return paso;

        RaycastHit2D cercano = impactos[0];
        for (int i = 1; i < total; i++)
            if (impactos[i].distance < cercano.distance) cercano = impactos[i];

        float libre = Mathf.Max(0, cercano.distance - .025f);
        if (libre > .02f) return direccion * Mathf.Min(distancia, libre);

        // En una esquina avanza paralelo al obstáculo sin atravesarlo.
        Vector2 lateral = direccion - cercano.normal * Vector2.Dot(direccion, cercano.normal);
        if (lateral.sqrMagnitude < .04f) return Vector2.zero;
        lateral.Normalize();
        if (cuerpo.Cast(lateral, filtro, impactos, distancia + .03f) > 0) return Vector2.zero;
        return lateral * distancia * .8f;
    }

    private void ActualizarAnimacion(Vector2 paso)
    {
        if (Mathf.Abs(paso.x) > Mathf.Abs(paso.y))
            CambiarAnimacion(paso.x > 0 ? "Caminar_derecha_bit" : "Caminar_izquierda_bit");
        else
            CambiarAnimacion(paso.y > 0 ? "Caminar_espaldas_bit" : "Caminar_frente_bit");
    }

    private void LateUpdate()
    {
        if (dibujo && dibujo.sprite)
            dibujo.sortingOrder = TechnopolisOrdenBarrio.Orden(dibujo.bounds.min.y, 1.25f);
    }

    private void CambiarAnimacion(string nombre)
    {
        if (animacionActual == nombre) return;
        animator.Play(nombre, 0, 0f);
        animacionActual = nombre;
    }
}
