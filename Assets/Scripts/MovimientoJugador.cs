using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class MovimientoJugador : MonoBehaviour
{
    [SerializeField] private float velocidad = 3f;

    private Rigidbody2D cuerpo;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 movimiento;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        LeerMovimiento();

        bool caminando = movimiento.sqrMagnitude > 0.01f;
        animator.SetBool("Caminando", caminando);

        if (!caminando)
            return;

        // Movimiento horizontal
        if (Mathf.Abs(movimiento.x) > Mathf.Abs(movimiento.y))
        {
            animator.SetInteger("Direccion", 0);
            spriteRenderer.flipX = movimiento.x < 0f;
        }
        // Movimiento hacia abajo/frente
        else if (movimiento.y < 0f)
        {
            animator.SetInteger("Direccion", 1);
            spriteRenderer.flipX = false;
        }
        // Movimiento hacia arriba/espalda
        else
        {
            animator.SetInteger("Direccion", 2);
            spriteRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        Vector2 nuevaPosicion =
            cuerpo.position +
            movimiento * velocidad * Time.fixedDeltaTime;

        cuerpo.MovePosition(nuevaPosicion);
    }

    private void LeerMovimiento()
    {
#if ENABLE_INPUT_SYSTEM
        movimiento = Vector2.zero;
        Keyboard teclado = Keyboard.current;

        if (teclado != null)
        {
            bool derecha =
                teclado.dKey.isPressed ||
                teclado.rightArrowKey.isPressed;

            bool izquierda =
                teclado.aKey.isPressed ||
                teclado.leftArrowKey.isPressed;

            bool arriba =
                teclado.wKey.isPressed ||
                teclado.upArrowKey.isPressed;

            bool abajo =
                teclado.sKey.isPressed ||
                teclado.downArrowKey.isPressed;

            movimiento.x =
                (derecha ? 1f : 0f) -
                (izquierda ? 1f : 0f);

            movimiento.y =
                (arriba ? 1f : 0f) -
                (abajo ? 1f : 0f);
        }
#else
        movimiento = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
#endif

        movimiento = movimiento.normalized;
    }
}