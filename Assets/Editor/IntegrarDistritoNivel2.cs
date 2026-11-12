#if UNITY_EDITOR
using System;using System.Linq;using UnityEngine;using UnityEngine.Tilemaps;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;
public static class IntegrarDistritoNivel2
{
 [MenuItem("Technopolis/Nivel 2/Integrar ciudad con campaña")]
 public static void Aplicar(){
 var scene=SceneManager.GetSceneByPath("Assets/Scenes/Nivel2.unity");if(!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel2.unity",OpenSceneMode.Additive);
 var roots=scene.GetRootGameObjects();if(roots.Any(r=>r.GetComponent<DistritoNivel2>()))throw new Exception("La integración ya existe.");
 SceneManager.SetActiveScene(scene);var host=new GameObject("Campaña del distrito comercial");var config=host.AddComponent<DistritoNivel2>();config.limites=new Bounds(new Vector3(-22,-19,0),new Vector3(88,66,1));
 config.donDinero=Punto(host.transform,"Don Dinero · punto de encuentro",new Vector2(-18,-16));config.taller=Punto(host.transform,"Taller técnico",new Vector2(.5f,-18));config.salida=Punto(host.transform,"Acceso a Ciudad de Silicio",new Vector2(17,-43));
 var collisions=new GameObject("Colisiones del distrito").transform;collisions.SetParent(host.transform);
 int count=0;foreach(var map in roots.SelectMany(r=>r.GetComponentsInChildren<Tilemap>())){
 if(map.name!="Edificios"&&map.name!="Arboles_escalados")continue;
 foreach(var cell in map.cellBounds.allPositionsWithin){var sp=map.GetSprite(cell);if(!sp)continue;string n=sp.name.ToLowerInvariant();if(n.Contains("techo"))continue;
 var matrix=map.transform.localToWorldMatrix*Matrix4x4.Translate(map.CellToLocalInterpolated((Vector3)cell+map.tileAnchor))*map.GetTransformMatrix(cell);var b=sp.bounds;Vector3 min=matrix.MultiplyPoint3x4(b.min),max=matrix.MultiplyPoint3x4(b.max);var size=max-min;bool tree=map.name=="Arboles_escalados";
 Vector2 sz=tree?new Vector2(Mathf.Min(.38f,size.x*.3f),.25f):new Vector2(size.x*.8f,size.y*.52f);Vector2 center=new Vector2((min.x+max.x)/2,min.y+sz.y*.5f+.08f);Caja(collisions,"Solido_"+count++,center,sz);
 }}
 Caja(collisions,"Límite norte",new Vector2(-22,14),new Vector2(89,.5f));Caja(collisions,"Límite sur",new Vector2(-22,-52),new Vector2(89,.5f));Caja(collisions,"Límite oeste",new Vector2(-66,-19),new Vector2(.5f,66));Caja(collisions,"Límite este",new Vector2(22,-19),new Vector2(.5f,66));
 var gate=new GameObject("Porton_Salida");gate.transform.SetParent(host.transform);gate.transform.position=new Vector3(17,-44,0);var box=gate.AddComponent<BoxCollider2D>();box.size=new Vector2(6,.35f);var tex=new Texture2D(48,5);tex.filterMode=FilterMode.Point;for(int y=0;y<5;y++)for(int x=0;x<48;x++)tex.SetPixel(x,y,((x+y)/5)%2==0?new Color32(217,155,53,255):new Color32(31,40,48,255));tex.Apply();const string path="Assets/Resources/BarreraDistrito.png";System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=8;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.SaveAndReimport();var sr=gate.AddComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);sr.sortingOrder=900;
 var label=Punto(host.transform,"Señal Ciudad de Silicio",new Vector2(17,-42));var text=label.gameObject.AddComponent<TextMesh>();text.text="CIUDAD DE SILICIO\nACCESO >";text.anchor=TextAnchor.MiddleCenter;text.fontSize=40;text.characterSize=.045f;text.color=new Color32(76,202,198,255);label.GetComponent<MeshRenderer>().sortingOrder=1000;
 var player=roots.First(r=>r.name=="Jugador");player.transform.position=new Vector3(-21.9f,-16.75f,0);player.transform.localScale=new Vector3(1.4f,1.4f,1);var order=player.GetComponent<TechnopolisOrdenBarrio>();if(!order)order=player.AddComponent<TechnopolisOrdenBarrio>();order.jugador=player.GetComponent<SpriteRenderer>();order.escalaVertical=1.25f;
 var dog=roots.First(r=>r.name=="Bit");dog.transform.localScale=Vector3.one*.1f;dog.transform.position=player.transform.position+Vector3.left;dog.GetComponent<BitSeguidor>().jugador=player.transform;
 foreach(var name in new[]{"Canvas","Stats","Ramona_Npc"}){var old=roots.FirstOrDefault(r=>r.name==name);if(old)old.SetActive(false);}
 // Se conservan los Canvas de estado: el HUD compartido utiliza sus retratos y saldo.
 var build=EditorBuildSettings.scenes.ToList();if(!build.Any(s=>s.path==scene.path))build.Add(new EditorBuildSettingsScene(scene.path,true));EditorBuildSettings.scenes=build.ToArray();
 Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("NIVEL2_INTEGRADO colisiones="+count);
 }
 static Transform Punto(Transform root,string name,Vector2 pos){var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=pos;return go.transform;}
 static void Caja(Transform parent,string name,Vector2 pos,Vector2 size){var t=Punto(parent,name,pos);t.gameObject.AddComponent<BoxCollider2D>().size=size;}
}
#endif
