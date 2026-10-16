using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MusicaBarrio : MonoBehaviour
{
    static MusicaBarrio instancia;
    static bool audioBiblioteca;
    AudioSource fuente;

    public static void IniciarSiFalta()
    {
        if (instancia) return;
        new GameObject("Ambiente_Suave_Barrio").AddComponent<MusicaBarrio>();
    }

    public static void SilenciarPorBiblioteca(bool sonando)
    {
        audioBiblioteca = sonando;
        if (sonando && instancia && instancia.fuente)
            instancia.fuente.volume = 0f;
    }

    void Awake()
    {
        if (instancia && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        fuente = GetComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.loop = true;
        fuente.spatialBlend = 0f;
        fuente.volume = 0f;
        fuente.clip = Resources.Load<AudioClip>("Audio/ambiente_barrio");
        if (fuente.clip) fuente.Play();
        else Debug.LogWarning("No se encontró la música suave del barrio.", this);
    }

    void Update()
    {
        if (!fuente || !fuente.clip) return;
        float meta = audioBiblioteca || TechnopolisGameFlow.IsMenuOpen ? 0f : .18f;
        fuente.volume = Mathf.MoveTowards(fuente.volume, meta, Time.unscaledDeltaTime * .34f);
    }

    void OnDestroy()
    {
        if (instancia != this) return;
        instancia = null;
        audioBiblioteca = false;
    }
}
