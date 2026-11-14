#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class CorregirCallesBarrio
{
 static readonly Rect[] Calles={new Rect(10.2f,2,3.6f,60),new Rect(25.2f,2,3.6f,60),new Rect(47.4f,2,3.6f,60),new Rect(16,9,47,3),new Rect(1,20.5f,62,3),new Rect(1,40.7f,62,3.2f),new Rect(1,52.4f,62,3.4f)};
 static readonly Rect[] Reservas={new Rect(12.8f,23,11.3f,9.6f),new Rect(28.8f,30,19.3f,10.9f),new Rect(29,43,19,8.9f)};
 static IEnumerable<Rect> Restar(Rect r,Rect c){if(!r.Overlaps(c)){yield return r;yield break;}float x0=Mathf.Max(r.xMin,c.xMin),x1=Mathf.Min(r.xMax,c.xMax),y0=Mathf.Max(r.yMin,c.yMin),y1=Mathf.Min(r.yMax,c.yMax);if(x0>r.xMin)yield return Rect.MinMaxRect(r.xMin,r.yMin,x0,r.yMax);if(x1<r.xMax)yield return Rect.MinMaxRect(x1,r.yMin,r.xMax,r.yMax);if(y0>r.yMin)yield return Rect.MinMaxRect(x0,r.yMin,x1,y0);if(y1<r.yMax)yield return Rect.MinMaxRect(x0,y1,x1,r.yMax);}
 static Rect Area(Bounds b)=>Rect.MinMaxRect(b.min.x+40,(25-b.max.y)/1.25f,b.max.x+40,(25-b.min.y)/1.25f);
 static void Colocar(BoxCollider2D c,Rect r){Vector3 center=new Vector3(r.center.x-40,25-r.center.y*1.25f,0);c.offset=c.transform.InverseTransformPoint(center);c.size=new Vector2(r.width/Mathf.Abs(c.transform.lossyScale.x),r.height*1.25f/Mathf.Abs(c.transform.lossyScale.y));}
 public static string Aplicar(){
 var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(s.path!="Assets/Scenes/EscenaNivel1_FielReferencia.unity"||EditorApplication.isPlaying)throw new Exception("Abre nivel1 fuera de Play");var group=GameObject.Find("Grid/Barrio_Fiel_Referencia/Bordes_Desnivel_Colision");
 var cuts=new List<Rect>();foreach(var road in Calles){var segments=new List<Rect>{road};foreach(var keep in Reservas)segments=segments.SelectMany(r=>Restar(r,keep)).ToList();cuts.AddRange(segments);}
 int changed=0;foreach(var c in group.GetComponentsInChildren<BoxCollider2D>().ToArray()){
 if(!c.enabled)continue;Rect source=Area(c.bounds);var pieces=new List<Rect>{source};foreach(var cut in cuts)pieces=pieces.SelectMany(r=>Restar(r,cut)).Where(r=>r.width>.02f&&r.height>.02f).ToList();if(pieces.Count==1&&Mathf.Abs(pieces[0].width-source.width)<.001f&&Mathf.Abs(pieces[0].height-source.height)<.001f)continue;
 changed++;if(pieces.Count==0){c.enabled=false;continue;}Colocar(c,pieces[0]);for(int i=1;i<pieces.Count;i++){var go=new GameObject(c.name+"_Paso_"+i);go.transform.SetParent(c.transform.parent,false);var other=go.AddComponent<BoxCollider2D>();Colocar(other,pieces[i]);}
 }
 Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);return changed+" bordes ajustados a las calles visibles; plataformas y accesos conservados";
 }
}
#endif
