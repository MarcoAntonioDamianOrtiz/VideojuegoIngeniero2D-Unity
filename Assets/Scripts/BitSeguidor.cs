
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BitSeguidor : MonoBehaviour
{
    [Header("Jugador")]
    public Transform jugador;

    [Header("Seguimiento")]
    public float velocidad = 2.8f;
    public float distanciaDetras = 1.5f;
    public float margenDetener = 0.25f;
    public float distanciaCorrer = 3f;
    public float velocidadCorrer = 4.5f;

    private Animator animator;
    private string animacionActual = "";

    private Vector2 posicionAnteriorJugador;
    private Vector2 ultimaDireccion = Vector2.down;
    private Vector2 destino;

    private bool siguiendo = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (jugador != null)
        {
            posicionAnteriorJugador = jugador.position;

            Vector2 diferencia =
                (Vector2)jugador.position -
                (Vector2)transform.position;

            if (diferencia.sqrMagnitude > 0.01f)
                ultimaDireccion = diferencia.normalized;

            destino = (Vector2)jugador.position
                - ultimaDireccion * distanciaDetras;
        }

        CambiarAnimacion("Bit_Idle");
    }

    private void LateUpdate()
    {
        if (jugador == null)
            return;

        Vector2 posicionJugador = jugador.position;

        // Detectar hacia donde camina el Ingeniero.
        Vector2 desplazamiento =
            posicionJugador - posicionAnteriorJugador;

        if (desplazamiento.sqrMagnitude > 0.000001f)
        {
            // Elegir una de las cuatro direcciones.
            if (Mathf.Abs(desplazamiento.x) >
                Mathf.Abs(desplazamiento.y))
            {
                ultimaDireccion = desplazamiento.x > 0
                    ? Vector2.right
                    : Vector2.left;
            }
            else
            {
                ultimaDireccion = desplazamiento.y > 0
                    ? Vector2.up
                    : Vector2.down;
            }
        }

        posicionAnteriorJugador = posicionJugador;

        // Bit busca quedarse detras del Ingeniero.
        destino = posicionJugador
            - ultimaDireccion * distanciaDetras;

        Vector2 posicionBit = transform.position;
        Vector2 diferenciaDestino = destino - posicionBit;

        float distancia = diferenciaDestino.magnitude;

        // Evitar que cambie entre caminar y quieto
        // continuamente cuando llega al destino.
        if (siguiendo)
        {
            if (distancia <= margenDetener * 0.5f)
                siguiendo = false;
        }
        else if (distancia > margenDetener)
        {
            siguiendo = true;
        }

        if (!siguiendo)
        {
            CambiarAnimacion("Bit_Idle");
            return;
        }

        // Acelerar si Bit se queda muy atras.
        float velocidadActual = distancia > distanciaCorrer
            ? velocidadCorrer
            : velocidad;

        Vector2 nuevaPosicion = Vector2.MoveTowards(
            posicionBit,
            destino,
            velocidadActual * Time.deltaTime
        );

        transform.position = new Vector3(
            nuevaPosicion.x,
            nuevaPosicion.y,
            transform.position.z
        );

        Vector2 direccion = diferenciaDestino.normalized;

        if (Mathf.Abs(direccion.x) >
            Mathf.Abs(direccion.y))
        {
            CambiarAnimacion(direccion.x > 0
                ? "Caminar_derecha_bit"
                : "Caminar_izquierda_bit");
        }
        else
        {
            CambiarAnimacion(direccion.y > 0
                ? "Caminar_espaldas_bit"
                : "Caminar_frente_bit");
        }
    }

    private void CambiarAnimacion(string nombre)
    {
        if (animacionActual == nombre)
            return;

        animator.Play(nombre, 0, 0f);
        animacionActual = nombre;
    }
}
