#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class PulirDistritoComercial
{
 const string Garden="Assets/CiudadNivel2/Extras/Arboles/Arbustos/";
 static Sprite Load(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderByDescending(s=>s.rect.width*s.rect.height).First();
 static void Reemplazar(SpriteRenderer sr,string path,float width){
 var b=sr.bounds;Vector2 foot=new Vector2(b.center.x,b.min.y);var box=sr.GetComponent<BoxCollider2D>();Bounds old=box?box.bounds:new Bounds();sr.sprite=Load(path);
 var raw=new Texture2D(2,2);raw.LoadImage(File.ReadAllBytes(path));var data=raw.GetPixels32();int x0=raw.width,x1=0,y0=raw.height,y1=0;for(int y=0;y<raw.height;y++)for(int x=0;x<raw.width;x++)if(data[y*raw.width+x].a>40){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);}UnityEngine.Object.DestroyImmediate(raw);
 sr.transform.localScale=Vector3.one*(width/((x1-x0+1)/sr.sprite.pixelsPerUnit));Vector3 localFoot=new Vector3(((x0+x1)*.5f-sr.sprite.pivot.x)/sr.sprite.pixelsPerUnit,(y0-sr.sprite.pivot.y)/sr.sprite.pixelsPerUnit,0);sr.transform.position+=(Vector3)foot-sr.transform.TransformPoint(localFoot);
 if(box){box.size=new Vector2(old.size.x/Mathf.Abs(sr.transform.lossyScale.x),old.size.y/Mathf.Abs(sr.transform.lossyScale.y));box.offset=sr.transform.InverseTransformPoint(old.center);}
 }
 static void Adorno(Transform group,string path,Vector2 foot,float width,bool solid){var go=new GameObject(Path.GetFileNameWithoutExtension(path));go.transform.SetParent(group,false);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=Load(path);go.transform.localScale=Vector3.one*(width/sr.sprite.bounds.size.x);go.transform.position+=new Vector3(foot.x-sr.bounds.center.x,foot.y-sr.bounds.min.y,0);sr.sortingOrder=TechnopolisOrdenBarrio.Orden(foot.y,1.25f);if(solid){var c=go.AddComponent<BoxCollider2D>();c.size=new Vector2(width*.8f/go.transform.lossyScale.x,.25f/go.transform.lossyScale.y);c.offset=go.transform.InverseTransformPoint((Vector3)foot+Vector3.up*.125f);}}
 public static string Aplicar(){
 var root=GameObject.Find("N2_Distrito_Comercial");if(!root||EditorApplication.isPlaying)throw new Exception("Abre Nivel2 fuera de Play");int replacements=0;
 foreach(var path in new[]{"Assets/Resources/BancoDistritoCompleto.png","Assets/Resources/PapelerasDistritoCompletas.png"}){AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();}
 foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>()){
 string path=AssetDatabase.GetAssetPath(sr.sprite);
 if(sr.name=="Arbol_Plaza_Central"&&!path.EndsWith("arbol_plaza_monumental.png")){Reemplazar(sr,"Assets/Technopolis/Nivel1/Sprites/04_Objetos/arbol_plaza_monumental.png",5.8f);replacements++;}
 else if(path.EndsWith("Banco_Madera_01.png")||path.EndsWith("Banco_Madera_02.png")){Reemplazar(sr,"Assets/Resources/BancoDistritoCompleto.png",sr.name.Contains("Plaza")?2.5f:2f);replacements++;}
 else if(path.EndsWith("Papelera_Doble.png")){Reemplazar(sr,"Assets/Resources/PapelerasDistritoCompletas.png",1.2f);replacements++;}
 else if(path.EndsWith("Jardinera_Piedra.png")){Reemplazar(sr,Garden+"Seto_Largo_Flores.png",2.4f);replacements++;}
 else if(path.EndsWith("Jardinera_Rosa.png")){Reemplazar(sr,Garden+"Jardinera_Madera.png",1.6f);replacements++;}
 else if(path.EndsWith("Maceta_Arbusto_Redondo.png")){Reemplazar(sr,Garden+"Maceta_Conica_01.png",1.2f);replacements++;}
 }
 var benches=root.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.name.Contains("Banco_")).ToArray();
 foreach(var light in root.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.name.StartsWith("Farola_"))){foreach(var bench in benches)if(light.bounds.Intersects(bench.bounds)&&Mathf.Abs(light.bounds.center.x-bench.bounds.center.x)<bench.bounds.size.x*.5f+.7f){float dx=light.bounds.center.x-bench.bounds.center.x;light.transform.position+=Vector3.right*(bench.bounds.center.x+(dx<0?-1:1)*(bench.bounds.size.x*.5f+.85f)-light.bounds.center.x);break;}}
 if(!root.transform.Find("05_Plaza_y_Paseos")){
 var group=new GameObject("05_Plaza_y_Paseos").transform;group.SetParent(root.transform,false);
 foreach(float x in new[]{-31.8f,-12.2f})foreach(float y in new[]{-18f,-23f})Adorno(group,Garden+"Jardinera_Madera.png",new Vector2(x,y),1.7f,true);
 foreach(float cx in new[]{-50f,-22f,6f})foreach(float cy in new[]{3f,-17f,-37f}){if(cx==-22&&cy==-17)continue;foreach(float dx in new[]{-6f,6f})Adorno(group,Garden+"Arbusto_Flores_Azules.png",new Vector2(cx+dx,cy-1.8f),1.35f,false);}
 }
 AjustarArriate(root.transform);SombrasDistrito.Aplicar(root.transform);if(!root.GetComponent<ProfundidadDistrito>())root.AddComponent<ProfundidadDistrito>();
 AcabadoDistritoNivel2.Suelo();Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);return replacements+" piezas recortadas reemplazadas; plaza, jardines y pavimento pulidos";
 }
 public static void AjustarArriate(Transform root){var tree=root.GetComponentsInChildren<SpriteRenderer>().First(s=>s.name=="Arbol_Plaza_Central");var old=tree.GetComponent<BoxCollider2D>();if(!old)return;Vector2 foot=new Vector2(old.bounds.center.x,old.bounds.min.y-.08f);UnityEngine.Object.DestroyImmediate(old);var c=tree.gameObject.AddComponent<CapsuleCollider2D>();c.direction=CapsuleDirection2D.Horizontal;c.size=new Vector2(2.7f/Mathf.Abs(tree.transform.lossyScale.x),1f/Mathf.Abs(tree.transform.lossyScale.y));c.offset=tree.transform.InverseTransformPoint(foot+Vector2.up*.58f);}
}
#endif
