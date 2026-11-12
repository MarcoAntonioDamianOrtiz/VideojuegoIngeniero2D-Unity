#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.Tilemaps;
public static class DisenoComercialNivel2
{
 const string Dir="Assets/CiudadNivel2/Extras/";
 const string Ground="Assets/Resources/SueloDistritoComercial.png";
 static Transform root,buildings,props;
 static readonly float[] X={-64,-36,-8,20},Y={-50,-30,-10,10};
 [MenuItem("Technopolis/Nivel 2/Diseñar manzanas comerciales")]
 public static void Aplicar(){
 if(EditorApplication.isPlaying)throw new Exception("Detén Play.");var scene=SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Nivel2.unity")throw new Exception("Abre Nivel2.");if(GameObject.Find("N2_Distrito_Comercial"))throw new Exception("El diseño ya existe.");
 File.Copy(scene.path,Path.Combine(Path.GetTempPath(),"Nivel2_antes_diseno_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity"));
 var cfg=UnityEngine.Object.FindFirstObjectByType<DistritoNivel2>();
 foreach(var map in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Tilemap>()))map.gameObject.SetActive(false);
 cfg.transform.Find("Colisiones del distrito").gameObject.SetActive(false);
 root=Group("N2_Distrito_Comercial",null);buildings=Group("01_Comercios_y_Viviendas",root);props=Group("02_Jardines_y_Mobiliario",root);
 Suelo();
 // Tres filas de manzanas; el centro permanece abierto como plaza del distrito.
 Add("Edificio_Banco",Dir+"Edificios/Negocios/Banco_Ciudad.png",-55,-4,9,buildings,true);
 Add("Edificio_Correos",Dir+"Edificios/Negocios/Servicio_Correos.png",-44,-4,8,buildings,true);
 Add("Edificio_Tecnologia",Dir+"Edificios/Negocios/Tienda_Tecnologia.png",-27,-4,8.5f,buildings,true);
 Add("Edificio_Papeleria",Dir+"Edificios/Negocios/Papeleria_Local.png",-16,-4,8,buildings,true);
 Add("Taller", "Assets/CiudadNivel2/Edificios_Completos/Almacen.png",1,-4,9,buildings,true);
 Add("Edificio_Ferreteria","Assets/CiudadNivel2/Edificios_Completos/Ferreteria.png",12,-4,8,buildings,true);
 Add("Edificio_Cafeteria",Dir+"Edificios/Negocios/Cafeteria_Acampanada.png",-55,-24,8,buildings,true);
 Add("Edificio_Restaurante",Dir+"Edificios/Negocios/Restaurante_Ciudad.png",-44,-24,9,buildings,true);
 Add("Edificio_Farmacia",Dir+"Edificios/Negocios/Farmacia_Local.png",1,-24,8,buildings,true);
 Add("Edificio_Gimnasio",Dir+"Edificios/Negocios/Gimnasio_Ciudad.png",12,-24,8,buildings,true);
 Add("Vivienda_Jardin",Dir+"Casas/Casa_Ciudad_Piedra_Jardin.png",-55,-44,8,buildings,true);
 Add("Vivienda_Teja",Dir+"Casas/Casa_Residencial_Teja_Roja.png",-44,-44,7.5f,buildings,true);
 Add("Edificio_Departamentos",Dir+"Edificios/Departamentos/Departamentos_Cristal.png",-27,-45,6.5f,buildings,true);
 Add("Vivienda_Balcon",Dir+"Casas/Casa_Moderna_Balcon.png",-16,-44,7.5f,buildings,true);
 Add("Vivienda_Azul",Dir+"Casas/Casa_Contemporanea_Azul.png",1,-44,7.5f,buildings,true);
 Add("Edificio_Clinica","Assets/CiudadNivel2/Edificios_Completos/Clinica.png",12,-44,8,buildings,true);
 // Árboles de copa compacta y parterres sobre las banquetas, fuera de los corredores.
 foreach(float yy in new[]{-45f,-25f,-5f})foreach(float xx in new[]{-60.5f,-39.5f,-32.5f,-11.5f,-4.5f,16.5f}){
 Add("Arbol_Distrito",Dir+"Arboles/Arboles/Arbol_Frondoso_02.png",xx,yy+9,2.1f,props,false,.28f);
 Add("Jardinera",Dir+"Arboles/Arbustos/Jardinera_Madera.png",xx,yy-.5f,1.25f,props,false,.3f);
 }
 foreach(float xx in new[]{-58f,-43f,-30f,-15f,-2f,13f})foreach(float yy in new[]{-46.5f,-26.5f,-6.5f})
 Add("Farola_Distrito","Assets/Technopolis/nivel3/Sprites/Decoracion/Farola_Moderna.png",xx,yy,.7f,props,false,.18f);
 foreach(float xx in new[]{-29f,-15f})foreach(float yy in new[]{-24.5f,-15.5f}){
 Add("Banco_Plaza","Assets/Technopolis/nivel3/Sprites/Decoracion/Banco_Madera_01.png",xx,yy,2.5f,props,false,.35f);
 if(yy<-20)Add("Arbol_Plaza",Dir+"Arboles/Arboles/Arbol_Frondoso_01.png",xx,yy+2,2.8f,props,false,.3f);
 }
 Add("Arbol_Plaza_Central",Dir+"Arboles/Arboles/Arbol_Frondoso_03.png",-22,-21.5f,5.5f,props,false,.65f);
 foreach(float xx in new[]{-26f,-18f})Add("Jardin_Plaza",Dir+"Arboles/Arbustos/Arbusto_Flores_Rosas.png",xx,-20,1.7f,props,false,.25f);
 foreach(float xx in new[]{-30f,-14f})Add("Papelera","Assets/Technopolis/nivel3/Sprites/Decoracion/Papelera_Doble.png",xx,-20,.7f,props,false,.3f);
 // Accesos y personajes conservan los mismos sistemas de campaña.
 cfg.donDinero.position=new Vector2(-23,-6.2f);cfg.taller.position=new Vector2(1,-6.2f);cfg.salida.position=new Vector2(17,-49);
 cfg.limites=new Bounds(new Vector3(-22,-20,0),new Vector3(88,64,1));
 cfg.transform.Find("Porton_Salida").position=new Vector2(17,-50.5f);cfg.transform.Find("Señal Ciudad de Silicio").position=new Vector2(16,-47);
 var p=GameObject.Find("Jugador");p.transform.position=new Vector2(-28,-6.4f);p.transform.localScale=new Vector3(1.8f,1.8f,1);GameObject.Find("Bit").transform.position=p.transform.position+Vector3.left;GameObject.Find("Bit").transform.localScale=Vector3.one*.14f;
 Box("Límite_norte",new Vector2(-22,12),new Vector2(88,.4f));Box("Límite_sur",new Vector2(-22,-52),new Vector2(88,.4f));Box("Límite_oeste",new Vector2(-66,-20),new Vector2(.4f,64));Box("Límite_este",new Vector2(22,-20),new Vector2(.4f,64));
 ArbolesJardinesDistrito.Aplicar();
 Physics2D.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 static Transform Group(string name,Transform parent){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
 static SpriteRenderer Add(string name,string path,float x,float y,float width,Transform parent,bool building,float solid=0){
 var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer&&path.StartsWith(Dir)){if(importer.filterMode!=FilterMode.Point||importer.mipmapEnabled){importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.SaveAndReimport();}}
 var sp=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderByDescending(s=>s.rect.width*s.rect.height).FirstOrDefault();if(!sp)throw new Exception("Sprite faltante: "+path);
 var t=Group(name,parent);var sr=t.gameObject.AddComponent<SpriteRenderer>();sr.sprite=sp;sr.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Technopolis/nivel3/Generado/MapaReferencia_Sprites.mat");t.localScale=Vector3.one*(width/sp.bounds.size.x);t.position+=new Vector3(x-sr.bounds.center.x,y-sr.bounds.min.y,0);sr.sortingOrder=TechnopolisOrdenBarrio.Orden(y,1.25f);
 var box=t.gameObject.AddComponent<BoxCollider2D>();var b=sr.bounds;Vector2 sz=building?new Vector2(width*.85f,b.size.y*.48f):new Vector2(solid,Mathf.Min(.4f,solid));box.size=new Vector2(sz.x/t.lossyScale.x,sz.y/t.lossyScale.y);box.offset=t.InverseTransformPoint(new Vector3(x,y+sz.y*.5f+.08f,0));return sr;
 }
 static void Box(string name,Vector2 center,Vector2 size){var t=Group(name,root);t.position=center;t.gameObject.AddComponent<BoxCollider2D>().size=size;}
 static uint Hash(int x,int y){unchecked{uint h=(uint)(x*374761393+y*668265263);h=(h^(h>>13))*1274126177;return h^(h>>16);}}
 static float Near(float p,float[] roads){float d=1000;foreach(float v in roads)d=Mathf.Min(d,Mathf.Abs(p-v));return d;}
 static void Suelo(){
 const int w=1408,h=1024;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var colors=new Color32[w*h];
 for(int iy=0;iy<h;iy++)for(int ix=0;ix<w;ix++){
 float x=-66+(ix+.5f)/16,y=-52+(iy+.5f)/16;float dx=Near(x,X),dy=Near(y,Y),d=Mathf.Min(dx,dy);int n=(int)(Hash(ix,iy)%7)-3;Color32 c;
 bool plaza=x>-33&&x<-11&&y>-27&&y<-13;
 if(d<2){int wear=(int)(Mathf.PerlinNoise(ix*.035f,iy*.035f)*9);c=new Color32((byte)(56+n+wear),(byte)(65+n+wear),(byte)(68+n+wear),255);bool stripe=(dx<.065f&&Mathf.Repeat(y+52,3.5f)<1.8f&&dy>3)||(dy<.065f&&Mathf.Repeat(x+66,3.5f)<1.8f&&dx>3);if(stripe)c=new Color32(206,191,133,255);
 // Pasos peatonales a los lados de cada cruce.
 bool cross=(dx<1.65f&&dy>2.7f&&dy<4.5f&&Mathf.Repeat(x+66,.6f)<.3f)||(dy<1.65f&&dx>2.7f&&dx<4.5f&&Mathf.Repeat(y+52,.6f)<.3f);if(cross)c=new Color32(204,205,183,255);
 }else if(d<4||plaza){bool seam=(ix%16==0||iy%16==0);int a=plaza?0:12;c=seam?new Color32(133,128,112,255):new Color32((byte)(181+n+a),(byte)(170+n+a),(byte)(145+n+a),255);if(d<2.15f)c=new Color32(213,204,175,255);
 // Roseta de ladrillo en la plaza, sin texto horneado.
 if(plaza){float r=Vector2.Distance(new Vector2(x,y),new Vector2(-22,-20));if(Mathf.Abs(r-5)<.15f||Mathf.Abs(r-2.8f)<.12f)c=new Color32(133,104,83,255);}
 }else{int g=(int)(Mathf.PerlinNoise(ix*.025f,iy*.025f)*13);c=new Color32((byte)(105+n+g),(byte)(132+n+g),(byte)(77+n+g),255);if((Hash(ix,iy)&1023)==0)c=new Color32(156,162,100,255);}
 colors[iy*w+ix]=c;
 }
 tex.SetPixels32(colors);tex.Apply();File.WriteAllBytes(Ground,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(Ground);var imp=(TextureImporter)AssetImporter.GetAtPath(Ground);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=16;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.userData="Diseño original: calles, banquetas y plaza del distrito comercial. Generado localmente, sin fuentes externas.";imp.SaveAndReimport();var sp=AssetDatabase.LoadAssetAtPath<Sprite>(Ground);var t=Group("Suelo_Distrito",root);var sr=t.gameObject.AddComponent<SpriteRenderer>();sr.sprite=sp;t.localScale=new Vector3(88/sp.bounds.size.x,64/sp.bounds.size.y,1);t.position+=new Vector3(-22,-20,0)-sr.bounds.center;sr.sortingOrder=-1000;
 }
}
#endif
