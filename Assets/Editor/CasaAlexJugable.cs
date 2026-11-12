#if UNITY_EDITOR
using System;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class CasaAlexJugable
{
 [MenuItem("Technopolis/Nivel 1/Corregir interior jugable de Alex")]
 public static void Aplicar(){
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/EscenaNivel1_FielReferencia.unity"||EditorApplication.isPlaying)throw new Exception("Abre el nivel 1 fuera de Play.");
 var casa=GameObject.Find("Casa_Alex");var floor=casa.GetComponentInChildren<SpriteRenderer>();floor.sortingOrder=-810;floor.color=Color.white;
 var top=casa.GetComponents<BoxCollider2D>().OrderByDescending(c=>c.bounds.center.y).First();var center=top.bounds.center;center.y=-7.12f;top.offset=casa.transform.InverseTransformPoint(center);
 if(!casa.transform.Find("Muebles_Solidos")){
 var group=new GameObject("Muebles_Solidos");group.transform.SetParent(casa.transform,false);
 Box(group.transform,"Cama",new Vector2(-24.0f,-9.55f),new Vector2(1.95f,2.75f));
 Box(group.transform,"Escritorio",new Vector2(-20.0f,-9.2f),new Vector2(2.4f,1.6f));
 Box(group.transform,"Baul",new Vector2(-19.35f,-12.7f),new Vector2(1.25f,.85f));
 }
 if(!casa.transform.Find("Muebles_Solidos/Techo_y_pared_posterior"))Box(casa.transform.Find("Muebles_Solidos"),"Techo_y_pared_posterior",new Vector2(-21.9f,-5.5f),new Vector2(7.4f,3.3f));
 Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 static void Box(Transform parent,string name,Vector2 pos,Vector2 size){var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=pos;go.transform.localScale=new Vector3(1/go.transform.parent.lossyScale.x,1/go.transform.parent.lossyScale.y,1);go.AddComponent<BoxCollider2D>().size=size;}
}
#endif
