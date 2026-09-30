using UnityEngine;

/// <summary>Matches the player's draw order to the reference layout's ground anchors.</summary>
[ExecuteAlways]
public sealed class TechnopolisOrdenBarrio : MonoBehaviour
{
    public SpriteRenderer jugador;
    public static int Orden(float groundY) { return Mathf.RoundToInt((20f-groundY)*10f); }
    public void Actualizar()
    {
        if(jugador)jugador.sortingOrder=Orden(jugador.transform.position.y);
    }
    void LateUpdate() { Actualizar(); }
}
