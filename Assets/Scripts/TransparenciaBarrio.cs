using System.Collections.Generic;
using UnityEngine;

public sealed class TransparenciaBarrio : MonoBehaviour
{
    readonly Dictionary<SpriteRenderer,Color> originales=new Dictionary<SpriteRenderer,Color>();
    SpriteRenderer actor;
    float siguiente;
    void Start()
    {
        actor=GetComponent<SpriteRenderer>();
        foreach(var s in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            string name=s.transform.parent?s.transform.parent.name.ToLowerInvariant():s.name.ToLowerInvariant();
            if(s!=actor&&(name.Contains("arbol")||name.StartsWith("vivienda")||name.Contains("panaderia")||name=="taller")&&s.bounds.size.y>2)originales[s]=s.color;
        }
    }
    void LateUpdate()
    {
        if(!actor||Time.unscaledTime<siguiente)return;siguiente=Time.unscaledTime+.08f;
        foreach(var item in originales)
        {
            if(!item.Key)continue;var s=item.Key;var b=s.bounds;
            bool tapa=s.sortingOrder>actor.sortingOrder&&b.Contains(new Vector3(actor.bounds.center.x,actor.bounds.center.y,b.center.z));
            Color c=item.Value;c.a*=tapa?.3f:1f;s.color=c;
        }
    }
    void OnDestroy(){foreach(var item in originales)if(item.Key)item.Key.color=item.Value;}
}
