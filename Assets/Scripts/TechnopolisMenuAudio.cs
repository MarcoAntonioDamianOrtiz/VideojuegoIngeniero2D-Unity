using UnityEngine;

/// <summary>Audio exclusivo del menú; sigue funcionando cuando la partida está pausada.</summary>
public sealed class TechnopolisMenuAudio : MonoBehaviour
{
    AudioSource musica;
    AudioClip mover, confirmar, volver;
    float ultimoMovimiento=-1;
    bool saliendo;
    void Awake()
    {
        musica=gameObject.AddComponent<AudioSource>();
        musica.playOnAwake=false;musica.loop=true;musica.spatialBlend=0;
        musica.ignoreListenerPause=true;musica.volume=0;
        musica.clip=Resources.Load<AudioClip>("Audio/menu_atardecer");
        mover=Resources.Load<AudioClip>("Audio/menu_mover");
        confirmar=Resources.Load<AudioClip>("Audio/menu_confirmar");
        volver=Resources.Load<AudioClip>("Audio/menu_volver");
    }
    void OnEnable(){saliendo=false;if(musica&&musica.clip){musica.volume=0;musica.Play();}}
    void Update(){if(musica&&!saliendo)musica.volume=Mathf.MoveTowards(musica.volume,.6f,Time.unscaledDeltaTime*.65f);}
    public void Mover(){if(Time.unscaledTime-ultimoMovimiento<.075f)return;ultimoMovimiento=Time.unscaledTime;Emitir(mover,.3f);}
    public void Confirmar(){Emitir(confirmar,.38f);}
    public void Volver(){Emitir(volver,.3f);}
    public void PrepararSalida()
    {
        if(saliendo||!musica||!musica.isPlaying)return;saliendo=true;
        var go=new GameObject("Desvanecer música del menú");DontDestroyOnLoad(go);
        var cola=go.AddComponent<TechnopolisColaAudio>();
        cola.Configurar(musica.clip,musica.volume,musica.time,.3f,true);musica.Stop();
    }
    static void Emitir(AudioClip clip,float volume)
    {
        if(!clip)return;var go=new GameObject("Sonido de interfaz");DontDestroyOnLoad(go);
        go.AddComponent<TechnopolisColaAudio>().Configurar(clip,volume,0,clip.length+.04f,false);
    }
    void OnDisable(){if(musica)musica.Stop();}
}

public sealed class TechnopolisColaAudio : MonoBehaviour
{
    AudioSource fuente;float inicio,duracion,volumen;bool desvanecer;
    public void Configurar(AudioClip clip,float volume,float tiempo,float segundos,bool fade)
    {
        fuente=gameObject.AddComponent<AudioSource>();fuente.playOnAwake=false;fuente.spatialBlend=0;
        fuente.ignoreListenerPause=true;fuente.clip=clip;fuente.volume=volumen=volume;
        fuente.time=Mathf.Min(tiempo,clip.length-.01f);duracion=segundos;desvanecer=fade;inicio=Time.unscaledTime;fuente.Play();
    }
    void Update(){float age=Time.unscaledTime-inicio;if(desvanecer&&fuente)fuente.volume=volumen*(1-Mathf.Clamp01(age/duracion));if(age>=duracion)Destroy(gameObject);}
}
