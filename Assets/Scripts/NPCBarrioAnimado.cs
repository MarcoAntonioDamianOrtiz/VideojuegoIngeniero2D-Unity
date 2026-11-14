using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public sealed class NPCBarrioAnimado : MonoBehaviour
{
 SpriteRenderer dibujo;Sprite original;Sprite[] poses;float inicio;bool diego;
 void Start(){
 dibujo=GetComponent<SpriteRenderer>();original=dibujo.sprite;if(!original)return;
 var data=Resources.Load<NPCBarrioRecursos>("Personajes/AnimacionesHabitantes");if(!data)return;diego=name.Contains("Diego");var image=diego?data.diegoByte:data.donDinero;var rects=diego?data.posesDiegoByte:data.posesDonDinero;if(!image||rects==null||rects.Length!=8)return;
 float ppu=rects[0].height/original.bounds.size.y;Vector2 pivot=new Vector2(original.pivot.x/original.rect.width,original.pivot.y/original.rect.height);poses=new Sprite[8];for(int i=0;i<8;i++)poses[i]=Sprite.Create(image,rects[i],pivot,ppu,0,SpriteMeshType.FullRect);inicio=Time.time;dibujo.sprite=poses[0];
 }
 void LateUpdate(){if(poses==null)return;float t=Mathf.Repeat(Time.time-inicio+(diego?.8f:0),5.6f);int frame=t<1.2f?0:t<1.9f?1:t<2.05f?2:t<3.5f?0:Mathf.Min(7,3+Mathf.FloorToInt((t-3.5f)/.24f));dibujo.sprite=poses[frame];dibujo.sortingOrder=15000-Mathf.RoundToInt(dibujo.bounds.min.y*100);}
 void OnDestroy(){if(poses!=null)foreach(var p in poses)if(p)Destroy(p);}
}
