#if UNITY_EDITOR
using System;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class ColocarHabitantesSilicio
{
 public static void Aplicar(){
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/EscenaNivel3Silicio.unity"||EditorApplication.isPlaying)throw new Exception("Abre Silicio fuera de Play.");
 var p=GameObject.Find("Jugador");var office=UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None).Where(c=>c.name=="Edificio_Silicon_Alley").OrderBy(c=>Vector2.Distance(p.transform.position,c.bounds.center)).First();Vector2 front=new Vector2(office.bounds.center.x,office.bounds.min.y-.65f);
 var host=GameObject.Find("Habitantes_Oficina_Silicio");if(!host)host=new GameObject("Habitantes_Oficina_Silicio");var comp=host.GetComponent<HabitantesSilicio>();if(!comp)comp=host.AddComponent<HabitantesSilicio>();comp.donDinero=Persona(host.transform,"DonDinero_Npc","DonDineroCuerpoCompleto",front+Vector2.left*1.7f,2.35f);comp.diegoByte=Persona(host.transform,"DiegoByte_Npc","DiegoByteCuerpoCompleto",front+Vector2.right*1.7f,2.25f);
 Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 static Transform Persona(Transform parent,string name,string file,Vector2 pos,float height){
 var path="Assets/Resources/Personajes/"+file+".png";var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.SaveAndReimport();var sp=AssetDatabase.LoadAssetAtPath<Sprite>(path);var existing=parent.Find(name);var t=existing?existing:new GameObject(name).transform;t.SetParent(parent,false);var sr=t.GetComponent<SpriteRenderer>();if(!sr)sr=t.gameObject.AddComponent<SpriteRenderer>();sr.sprite=sp;t.localScale=Vector3.one*(height/sp.bounds.size.y);t.position+=new Vector3(pos.x-sr.bounds.center.x,pos.y-sr.bounds.min.y,0);sr.sortingOrder=15000-Mathf.RoundToInt(pos.y*100);var box=t.GetComponent<BoxCollider2D>();if(!box)box=t.gameObject.AddComponent<BoxCollider2D>();box.size=new Vector2(.5f/t.lossyScale.x,.28f/t.lossyScale.y);box.offset=t.InverseTransformPoint(new Vector3(pos.x,pos.y+.14f,0));return t;
 }
}
#endif
