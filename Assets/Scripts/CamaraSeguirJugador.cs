using UnityEngine;

public class CamaraSeguirJugador : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    private Camera camara;
    private SpriteRenderer terreno;

    private void Awake()
    {
        camara = GetComponent<Camera>();
        GameObject baseTerreno = GameObject.Find("Terreno_Organico");
        if (baseTerreno)
            terreno = baseTerreno.GetComponentInChildren<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (objetivo == null)
            return;

        Vector3 destino = objetivo.position;
        if (camara && terreno)
        {
            Bounds limites = terreno.bounds;
            float medioAlto = camara.orthographicSize;
            float medioAncho = medioAlto * camara.aspect;
            destino.x = medioAncho >= limites.extents.x
                ? limites.center.x
                : Mathf.Clamp(destino.x, limites.min.x + medioAncho, limites.max.x - medioAncho);
            destino.y = medioAlto >= limites.extents.y
                ? limites.center.y
                : Mathf.Clamp(destino.y, limites.min.y + medioAlto, limites.max.y - medioAlto);
        }
        transform.position = new Vector3(destino.x, destino.y, transform.position.z);
    }
}
