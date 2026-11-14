using UnityEngine;
public sealed class ProfundidadDistrito : MonoBehaviour
{
 void Start(){foreach(var sr in GetComponentsInChildren<SpriteRenderer>()){if(sr.name=="Suelo_Distrito"||sr.name.StartsWith("Sombra_")||sr.name.StartsWith("Luz_"))continue;var col=sr.GetComponent<Collider2D>();float pie=col?col.bounds.min.y-.08f:sr.bounds.min.y;sr.sortingOrder=15000-Mathf.RoundToInt(pie*100);}}
}
