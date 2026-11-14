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
    [SerializeField] SpriteRenderer perro;

    // Keep depth-sorted objects above the painted ground.
    const int OrdenBaseCiudad = 15000;

    void Start()
    {
        if (!Application.isPlaying || !perro || !jugador) return;

        // The original player has a Rigidbody2D but no physical footprint.
        if (!jugador.TryGetComponent<Collider2D>(out _))
        {
            Bounds bounds = jugador.bounds;
            Transform t = jugador.transform;
            var pies = jugador.gameObject.AddComponent<BoxCollider2D>();
            pies.offset = t.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y + .12f, t.position.z));
            pies.size = new Vector2(.42f / Mathf.Abs(t.lossyScale.x), .24f / Mathf.Abs(t.lossyScale.y));
        }

        foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
        foreach (SpriteRenderer sprite in raiz.GetComponentsInChildren<SpriteRenderer>())
        {
            string nombre = sprite.gameObject.name;
            if (nombre.StartsWith("Casa_") || nombre.StartsWith("Edificio_") ||
                nombre.StartsWith("Negocio_") || nombre.StartsWith("Fuente_") ||
                nombre.StartsWith("Arbol_") || nombre.StartsWith("Arbusto_") ||
                nombre.StartsWith("Seto_") || nombre.StartsWith("Farola_") ||
                nombre.StartsWith("Valla_") || nombre.StartsWith("Parada_Autobus"))
                sprite.sortingOrder = OrdenCiudad(sprite.bounds.min.y);
        }
        ActualizarCiudad();
    }

    static int OrdenCiudad(float y)
    {
        return OrdenBaseCiudad - Mathf.RoundToInt(y * 100f);
    }

    void ActualizarCiudad()
    {
        if (jugador) jugador.sortingOrder = OrdenCiudad(jugador.bounds.min.y);
        if(perro) perro.sortingOrder = OrdenCiudad(perro.bounds.min.y);
    }

    void LateUpdate()
    {
        if (perro || (Application.isPlaying && TechnopolisGameFlow.EsNivel2(gameObject.scene.name)))
        {
            if (Application.isPlaying) ActualizarCiudad();
            return;
        }
        Actualizar();
    }
}
