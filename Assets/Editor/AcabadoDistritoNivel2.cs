#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
public static class AcabadoDistritoNivel2
{
 const string Ground="Assets/Resources/SueloDistritoComercial.png";
 static readonly float[] X={-64,-36,-8,20},Y={-50,-30,-10,10};
 static float Near(float p,float[] list){float d=1000;foreach(var v in list)d=Mathf.Min(d,Mathf.Abs(v-p));return d;}
 static uint Hash(int x,int y){unchecked{uint v=(uint)(x*374761393+y*668265263);v=(v^(v>>13))*1274126177;return v^(v>>16);}}
 [MenuItem("Technopolis/Nivel 2/Aplicar acabado urbano y acceso")]
 public static void Aplicar(){
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Nivel2.unity"||EditorApplication.isPlaying)throw new Exception("Abre Nivel2 fuera de Play.");
 Suelo();Jardines();var player=GameObject.Find("Jugador");player.transform.localScale=new Vector3(3,3,1);GameObject.Find("Bit").transform.localScale=Vector3.one*.2f;
 var cfg=UnityEngine.Object.FindFirstObjectByType<DistritoNivel2>();var old=cfg.transform.Find("Señal Ciudad de Silicio");if(old)old.gameObject.SetActive(false);
 var obj=GameObject.Find("Acceso_Permanente_Silicio");if(!obj)obj=new GameObject("Acceso_Permanente_Silicio");obj.transform.position=new Vector3(17,-51,0);var gate=obj.GetComponent<AccesoSilicioVisual>();if(!gate)gate=obj.AddComponent<AccesoSilicioVisual>();gate.piedraMetal=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Technopolis/nivel3/Sprites/Decoracion/Valla_Metal_Piedra.png");gate.fuente=AssetDatabase.LoadAssetAtPath<Font>("Assets/Sprites/Menu/Fonts/PixelifySans.ttf");gate.Refrescar();
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 static void Jardines(){
 var root=GameObject.Find("N2_Distrito_Comercial").transform;if(root.Find("04_Parterres_y_Bordes"))return;var group=new GameObject("04_Parterres_y_Bordes").transform;group.SetParent(root,false);
 const string dir="Assets/Technopolis/nivel3/Sprites/Decoracion/";
 foreach(float cx in new[]{-50f,-22f,6f})foreach(float cy in new[]{3f,-17f,-37f}){
 if(cx==-22&&cy==-17)continue;
 for(int i=0;i<5;i++){
 Adorno(group,dir+(i%2==0?"Jardinera_Piedra.png":"Jardinera_Rosa.png"),cx-8+i*4,cy+1.65f,2.4f,true);
 }
 foreach(float dx in new[]{-9f,9f})Adorno(group,dir+"Maceta_Arbusto_Redondo.png",cx+dx,cy-1.3f,1.4f,true);
 }
 }
 static void Adorno(Transform parent,string path,float x,float y,float width,bool solid){
 var sp=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderByDescending(s=>s.rect.width*s.rect.height).FirstOrDefault();if(!sp)return;
 var go=new GameObject(Path.GetFileNameWithoutExtension(path));go.transform.SetParent(parent,false);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sp;go.transform.localScale=Vector3.one*(width/sp.bounds.size.x);go.transform.position+=new Vector3(x-sr.bounds.center.x,y-sr.bounds.min.y,0);sr.sortingOrder=TechnopolisOrdenBarrio.Orden(y,1.25f);
 if(solid){var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(width*.8f/go.transform.lossyScale.x,.3f/go.transform.lossyScale.y);box.offset=go.transform.InverseTransformPoint(new Vector3(x,y+.15f,0));}
 }
 public static void Suelo(){
 const int ppu=32,w=88*ppu,h=64*ppu;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var pixels=new Color32[w*h];
 for(int y=0;y<h;y++)for(int x=0;x<w;x++){
 float wx=-66+(x+.5f)/ppu,wy=-52+(y+.5f)/ppu;float dx=Near(wx,X),dy=Near(wy,Y),d=Mathf.Min(dx,dy);uint hash=Hash(x,y);int n=(int)(hash%5)-2;float patch=Mathf.PerlinNoise(x*.006f,y*.006f);Color c;
 bool plaza=wx>-33&&wx<-11&&wy>-27&&wy<-13;
 bool forecourt=d>=4&&(Mathf.Abs(wy+5)<1.45f||Mathf.Abs(wy+25)<1.45f||Mathf.Abs(wy+45)<1.45f);
 bool exit=wx>13&&wx<21&&wy<-46;
 if(d<2||exit){
 c=Color.Lerp(new Color32(73,70,79,255),new Color32(102,93,97,255),patch*.8f);c+=(Color)new Color(n/255f,n/255f,n/255f,0);
 bool stripe=(dx<.055f&&Mathf.Repeat(wy+52,3.5f)<1.6f&&dy>4)||(dy<.055f&&Mathf.Repeat(wx+66,3.5f)<1.6f&&dx>4);
 bool cross=(dx<1.65f&&dy>2.7f&&dy<4.5f&&Mathf.Repeat(wx+66,.6f)<.28f)||(dy<1.65f&&dx>2.7f&&dx<4.5f&&Mathf.Repeat(wy+52,.6f)<.28f);
 if(stripe)c=new Color32(208,168,65,255);if(cross)c=new Color32(207,202,175,255);
 int bx=x/60,by=y/60;bool crack=(Hash(bx,by)&15)==0&&Mathf.Abs((x%60)-(int)(20+8*Mathf.Sin(y*.13f)))<1.5f;if(crack&&!stripe&&!cross)c*=.7f;
 if(exit&&Mathf.Abs(wx-17)<1.7f&&Mathf.Abs(Mathf.Repeat(wy+52,2)-(Mathf.Abs(wx-17)*.7f+.25f))<.09f)c=new Color32(100,186,181,255);
 }else if(d<4||plaza||forecourt){
 int row=y/12,col=(x+(row%2)*8)/16;
 bool seam=(x+(row%2)*8)%16==0||y%12==0;int variation=(int)(Hash(col,row)%15)-7;
 c=seam?new Color32(128,108,101,255):new Color32((byte)(183+variation+n),(byte)(161+variation+n),(byte)(144+variation+n),255);
 if(!seam&&y%12==1)c+=new Color(.035f,.03f,.025f,0);
 if(d<2.12f)c=new Color32(216,199,175,255);
 if(plaza){float ring=Vector2.Distance(new Vector2(wx,wy),new Vector2(-22,-20));if(Mathf.Abs(ring-5)<.18f||Mathf.Abs(ring-2.8f)<.14f)c=new Color32(137,112,101,255);}
 }else{
 float blot=Mathf.PerlinNoise(x*.021f+13,y*.021f+21);c=Color.Lerp(new Color32(104,119,60,255),new Color32(133,147,71,255),patch);
 if(blot>.66f)c=Color.Lerp(c,new Color32(151,130,81,255),(blot-.66f)*2);
 c+=new Color(n/180f,n/180f,n/220f,0);
 uint tuft=Hash(x/5,y/7);if((tuft&15)<3&&(x%5==2||x%5==3)&&y%7<3)c=Color.Lerp(c,new Color32(168,171,88,255),.6f);
 if((hash&255)<4)c=Color.Lerp(c,new Color32(91,87,59,255),.7f);
 }
 c.a=1;pixels[y*w+x]=c;
 }
 tex.SetPixels32(pixels);tex.Apply();File.WriteAllBytes(Ground,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(Ground);var importer=(TextureImporter)AssetImporter.GetAtPath(Ground);importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();var sr=GameObject.Find("N2_Distrito_Comercial/Suelo_Distrito").GetComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Ground);sr.transform.localScale=new Vector3(88/sr.sprite.bounds.size.x,64/sr.sprite.bounds.size.y,1);sr.transform.position=new Vector3(-22,-20,0);sr.transform.position+=new Vector3(-22,-20,0)-sr.bounds.center;
 }
}
#endif
