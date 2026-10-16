
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BitSeguidor : MonoBehaviour
{
    [Header("Jugador")]
    public Transform jugador;

    [Header("Movimiento")]
    public float velocidad = 2.8f;
    public float distanciaDetener = 1.3f;

    private Animator animator;
    private string animacionActual = "";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        CambiarAnimacion("Bit_Idle");
    }

    private void Update()
    {
        if (jugador == null)
            return;

        Vector2 posicionBit = transform.position;
        Vector2 posicionJugador = jugador.position;

        Vector2 diferencia = posicionJugador - posicionBit;
        float distancia = diferencia.magnitude;

        // Si Bit esta cerca del Ingeniero, se detiene.
        if (distancia <= distanciaDetener)
        {
            CambiarAnimacion("Bit_Idle");
            return;
        }

        Vector2 direccion = diferencia.normalized;

        // Avanza sin sobrepasar la distancia de parada.
        float avance = Mathf.Min(
            velocidad * Time.deltaTime,
            distancia - distanciaDetener
        );

        Vector2 nuevaPosicion =
            posicionBit + direccion * avance;

        transform.position = new Vector3(
            nuevaPosicion.x,
            nuevaPosicion.y,
            transform.position.z
        );

        // Animaciones dependiendo de la direccion.
        if (Mathf.Abs(direccion.x) >
            Mathf.Abs(direccion.y))
        {
            if (direccion.x > 0)
                CambiarAnimacion("Caminar_derecha_bit");
            else
                CambiarAnimacion("Caminar_izquierda_bit");
        }
        else
        {
            if (direccion.y > 0)
                CambiarAnimacion("Caminar_espaldas_bit");
            else
                CambiarAnimacion("Caminar_frente_bit");
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
