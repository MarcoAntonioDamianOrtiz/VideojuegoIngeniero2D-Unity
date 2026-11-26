using UnityEngine;

public class CamaraSeguirJugador : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    private Camera camara;
    private SpriteRenderer terreno;
    private DistritoNivel2 distrito;
    private Vector2? centroInterior;
    private float zoomExterior;
    private Color fondoExterior;
    private CameraClearFlags borradoExterior;

    public void EnfocarInterior(Vector2? centro)
    {
        if (!camara) return;
        if (centro.HasValue && !centroInterior.HasValue)
        {
            zoomExterior = camara.orthographicSize;
            fondoExterior = camara.backgroundColor;
            borradoExterior = camara.clearFlags;
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = new Color32(22, 26, 29, 255);
        }
        if (!centro.HasValue && centroInterior.HasValue)
        {
            camara.orthographicSize = zoomExterior;
            camara.backgroundColor = fondoExterior;
            camara.clearFlags = borradoExterior;
        }
        centroInterior = centro;
        LateUpdate();
    }

    private void Awake()
    {
        camara = GetComponent<Camera>();
        distrito = FindFirstObjectByType<DistritoNivel2>();
        GameObject baseTerreno = GameObject.Find("Terreno_Organico");
        if (baseTerreno)
            terreno = baseTerreno.GetComponentInChildren<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (centroInterior.HasValue)
        {
            Vector2 centro = centroInterior.Value;
            camara.orthographicSize = Mathf.Max(4.8f, 7.5f / Mathf.Max(.1f, camara.aspect));
            transform.position = new Vector3(centro.x, centro.y, transform.position.z);
            return;
        }

        if (objetivo == null)
            return;

        Vector3 destino = objetivo.position;
        if (camara && (terreno || distrito))
        {
            Bounds limites = terreno ? terreno.bounds : distrito.limites;
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
