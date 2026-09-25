using UnityEngine;

public class CamaraSeguirJugador : MonoBehaviour
{
    [SerializeField] private Transform objetivo;

    private void LateUpdate()
    {
        if (objetivo == null)
            return;

        transform.position = new Vector3(
            objetivo.position.x,
            objetivo.position.y,
            transform.position.z
        );
    }
}