#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class ArbolesJardinesDistrito
{
 public const string Tree="Assets/Resources/ArbolDistritoCompleto.png";
 static Rect pixels;static Sprite sprite;static Transform garden;
 static void Preparar(){
 AssetDatabase.ImportAsset(Tree);var imp=(TextureImporter)AssetImporter.GetAtPath(Tree);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.alphaIsTransparency=true;imp.userData="Árbol original completo generado para TECHNOPOLIS. Copa entera, sin recortes, fondo transparente.";imp.SaveAndReimport();sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Tree);
 var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(Tree));var data=image.GetPixels32();int minx=image.width,miny=image.height,maxx=0,maxy=0;for(int y=0;y<image.height;y++)for(int x=0;x<image.width;x++)if(data[y*image.width+x].a>20){minx=Mathf.Min(minx,x);maxx=Mathf.Max(maxx,x);miny=Mathf.Min(miny,y);maxy=Mathf.Max(maxy,y);}pixels=Rect.MinMaxRect(minx,miny,maxx+1,maxy+1);UnityEngine.Object.DestroyImmediate(image);
 }
 [MenuItem("Technopolis/Nivel 2/Completar árboles y jardines")]
 public static void Aplicar(){
 if(EditorApplication.isPlaying)throw new Exception("Detén Play.");var root=GameObject.Find("N2_Distrito_Comercial");if(!root)throw new Exception("Abre Nivel2.");Preparar();
 foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>(true).Where(s=>s.name.StartsWith("Arbol_"))){var box=sr.GetComponent<BoxCollider2D>();Vector2 pos=new Vector2(sr.bounds.center.x,box?box.bounds.min.y-.08f:sr.bounds.min.y);float width=sr.bounds.size.x;if(AssetDatabase.GetAssetPath(sr.sprite)==Tree)width*=pixels.width/sprite.rect.width;var old=box?box.bounds:new Bounds();Colocar(sr,pos,width);if(box){box.size=new Vector2(old.size.x/sr.transform.lossyScale.x,old.size.y/sr.transform.lossyScale.y);box.offset=sr.transform.InverseTransformPoint(old.center);}}
 if(!root.transform.Find("03_Jardines_Comerciales")){
 garden=new GameObject("03_Jardines_Comerciales").transform;garden.SetParent(root.transform,false);
 foreach(float cx in new[]{-50f,-22f,6f})foreach(float cy in new[]{3f,-17f,-37f}){
 if(cx==-22&&cy==-17)continue;
 NuevoArbol(cx,cy,cy==3?2.8f:2.3f);
 foreach(float dx in new[]{-7.5f,-4.5f,4.5f,7.5f})Adorno("Flores_Jardin","Assets/CiudadNivel2/Extras/Arboles/Arbustos/Arbusto_Blanco_Flores.png",cx+dx,cy+.6f,1.35f);
 Adorno("Banco_Jardin","Assets/Technopolis/nivel3/Sprites/Decoracion/Banco_Madera_01.png",cx+4.5f,cy-1,2,true);
 }
 }
 Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);
 }
 static bool Cabe(Bounds b){var root=GameObject.Find("N2_Distrito_Comercial/01_Comercios_y_Viviendas");return !root.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.bounds.Intersects(b));}
 static void NuevoArbol(float x,float y,float width){var go=new GameObject("Arbol_Jardin");go.transform.SetParent(garden,false);var sr=go.AddComponent<SpriteRenderer>();Colocar(sr,new Vector2(x,y),width);if(!Cabe(sr.bounds)){UnityEngine.Object.DestroyImmediate(go);return;}var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(.3f/go.transform.lossyScale.x,.25f/go.transform.lossyScale.y);box.offset=go.transform.InverseTransformPoint(new Vector3(x,y+.2f,0));}
 static void Colocar(SpriteRenderer sr,Vector2 pos,float width){sr.sprite=sprite;sr.transform.localScale=Vector3.one*(width/(pixels.width/sprite.pixelsPerUnit));Vector3 local=new Vector3((pixels.center.x-sprite.pivot.x)/sprite.pixelsPerUnit,(pixels.yMin-sprite.pivot.y)/sprite.pixelsPerUnit,0);sr.transform.position+=(Vector3)pos-sr.transform.TransformPoint(local);sr.sortingOrder=TechnopolisOrdenBarrio.Orden(pos.y,1.25f);}
 static void Adorno(string name,string path,float x,float y,float width,bool solid=false){var imp=AssetImporter.GetAtPath(path) as TextureImporter;if(imp&&path.StartsWith("Assets/CiudadNivel2/")&&imp.filterMode!=FilterMode.Point){imp.filterMode=FilterMode.Point;imp.SaveAndReimport();}var sp=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderByDescending(s=>s.rect.width*s.rect.height).First();var go=new GameObject(name);go.transform.SetParent(garden,false);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sp;go.transform.localScale=Vector3.one*(width/sp.bounds.size.x);go.transform.position+=new Vector3(x-sr.bounds.center.x,y-sr.bounds.min.y,0);sr.sortingOrder=TechnopolisOrdenBarrio.Orden(y,1.25f);if(!Cabe(sr.bounds)){UnityEngine.Object.DestroyImmediate(go);return;}if(solid){var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(width*.8f/go.transform.lossyScale.x,.3f/go.transform.lossyScale.y);box.offset=go.transform.InverseTransformPoint(new Vector3(x,y+.2f,0));}}
}
#endif

