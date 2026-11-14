using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator),typeof(Rigidbody2D),typeof(SpriteRenderer))]
public class BitSeguidor : MonoBehaviour
{
    public Transform jugador;
    public float velocidad=3.6f;
    public float distanciaDetener=1.2f;
    Rigidbody2D cuerpo;
    BoxCollider2D caja;
    Animator animator;
    SpriteRenderer dibujo;
    MovimientoJugador movimientoJugador;
    Collider2D colliderJugador;
    readonly List<RaycastHit2D> impactos=new List<RaycastHit2D>();
    List<Vector2> ruta=new List<Vector2>();
    Vector2 destinoCalculado;
    float siguienteRuta;
    string animacion="";
    bool siguiendo;
    void Awake()
    {
        cuerpo=GetComponent<Rigidbody2D>();caja=GetComponent<BoxCollider2D>();animator=GetComponent<Animator>();dibujo=GetComponent<SpriteRenderer>();
        cuerpo.gravityScale=0;cuerpo.constraints=RigidbodyConstraints2D.FreezeRotation;cuerpo.interpolation=RigidbodyInterpolation2D.Interpolate;cuerpo.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
    }
    void Parar(){cuerpo.linearVelocity=Vector2.zero;Cambiar("Bit_Idle");}
    void FixedUpdate()
    {
        if(!jugador||!caja)return;
        if(!movimientoJugador)movimientoJugador=jugador.GetComponent<MovimientoJugador>();
        if(!colliderJugador){colliderJugador=jugador.GetComponent<Collider2D>();if(colliderJugador)Physics2D.IgnoreCollision(caja,colliderJugador);}
        if(TechnopolisGameFlow.IsMenuOpen||(movimientoJugador&&!movimientoJugador.enabled)){Parar();return;}
        Vector2 origen=cuerpo.position;
        Vector2 offset=(Vector2)caja.bounds.center-origen;
        Vector2 destino=(colliderJugador?(Vector2)colliderJugador.bounds.center:(Vector2)jugador.position)-offset;
        float distancia=Vector2.Distance(origen,destino),margen=Mathf.Clamp(distanciaDetener,1.05f,1.4f);
        if(distancia<=margen||(!siguiendo&&distancia<margen+.22f)){siguiendo=false;ruta.Clear();Parar();return;}
        siguiendo=true;
        Vector2 objetivo;
        bool directo=Libre(origen,destino);
        if(directo){ruta.Clear();objetivo=destino;}
        else
        {
            if(Time.time>=siguienteRuta&&(ruta.Count==0||Vector2.Distance(destino,destinoCalculado)>1f))
            {ruta=BitRutaObstaculos.Buscar(origen,destino,caja,jugador);destinoCalculado=destino;siguienteRuta=Time.time+.65f;}
            while(ruta.Count>0&&Vector2.Distance(origen,ruta[0])<.14f)ruta.RemoveAt(0);
            if(ruta.Count==0){Parar();return;}
            objetivo=ruta[0];
        }
        Vector2 delta=objetivo-origen;float avance=Mathf.Min(Mathf.Max(3.6f,velocidad)*Time.fixedDeltaTime,delta.magnitude);
        if(directo)avance=Mathf.Min(avance,Mathf.Max(0,distancia-margen));
        Vector2 siguiente=origen+delta.normalized*avance;
        if(avance<.0001f||!Libre(origen,siguiente)){ruta.Clear();siguienteRuta=Mathf.Max(siguienteRuta,Time.time+.2f);Parar();return;}
        cuerpo.linearVelocity=Vector2.zero;cuerpo.MovePosition(siguiente);
        if(Mathf.Abs(delta.x)>Mathf.Abs(delta.y))Cambiar(delta.x>0?"Caminar_derecha_bit":"Caminar_izquierda_bit");
        else Cambiar(delta.y>0?"Caminar_espaldas_bit":"Caminar_frente_bit");
    }
    bool Libre(Vector2 desde,Vector2 hasta)
    {
        Vector2 delta=hasta-desde;if(delta.sqrMagnitude<.000001f)return true;
        var f=new ContactFilter2D();f.useTriggers=false;f.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        Vector2 centro=desde+(Vector2)caja.bounds.center-cuerpo.position;
        Physics2D.BoxCast(centro,caja.bounds.size,0,delta.normalized,f,impactos,delta.magnitude);
        foreach(var hit in impactos)if(hit.rigidbody!=cuerpo&&hit.transform!=jugador&&!hit.transform.IsChildOf(jugador))return false;
        return true;
    }
    void Cambiar(string nombre){if(animacion==nombre)return;animator.Play(nombre,0,0);animacion=nombre;}
    void LateUpdate(){if(gameObject.scene.name!="EscenaNivel3Silicio"&&dibujo&&dibujo.sprite){float pie=caja?caja.bounds.min.y:dibujo.bounds.min.y;dibujo.sortingOrder=TechnopolisGameFlow.EsNivel2(gameObject.scene.name)?15000-Mathf.RoundToInt(pie*100):TechnopolisOrdenBarrio.Orden(pie,1.25f);}}
}
