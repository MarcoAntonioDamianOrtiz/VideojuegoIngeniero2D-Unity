using UnityEngine;

/// <summary>Matches the player's draw order to the reference layout's ground anchors.</summary>
[ExecuteAlways]
public sealed class TechnopolisOrdenBarrio : MonoBehaviour
{
    public SpriteRenderer jugador;
    public float escalaVertical=1f;
    public static int Orden(float groundY, float verticalScale=1f) { return Mathf.RoundToInt((20f-groundY/verticalScale)*10f); }
    public void Actualizar()
    {
        if(jugador)jugador.sortingOrder=Orden(jugador.transform.position.y, escalaVertical);
    }
    void LateUpdate() { Actualizar(); }
}
