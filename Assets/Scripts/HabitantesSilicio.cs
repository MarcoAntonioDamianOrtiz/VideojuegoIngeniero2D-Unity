using UnityEngine;
/// <summary>Habitantes visibles de la oficina, con el mismo orden de profundidad de Silicio.</summary>
public sealed class HabitantesSilicio : MonoBehaviour
{
    public Transform donDinero,diegoByte;
    static Vector2 Pies(Transform t){var sr=t.GetComponent<SpriteRenderer>();return sr?new Vector2(sr.bounds.center.x,sr.bounds.min.y):(Vector2)t.position;}
    public bool CercaDiego(Vector2 p)=>diegoByte&&Vector2.Distance(p,Pies(diegoByte))<1.9f&&(!donDinero||Vector2.Distance(p,Pies(diegoByte))<Vector2.Distance(p,Pies(donDinero)));
    public bool Cerca(Vector2 p)=>(donDinero&&Vector2.Distance(p,Pies(donDinero))<2f)||(diegoByte&&Vector2.Distance(p,Pies(diegoByte))<2f);
    static void Ordenar(Transform t){if(!t)return;var sr=t.GetComponent<SpriteRenderer>();if(sr)sr.sortingOrder=15000-Mathf.RoundToInt(sr.bounds.min.y*100);}
    void LateUpdate(){Ordenar(donDinero);Ordenar(diegoByte);}
}
