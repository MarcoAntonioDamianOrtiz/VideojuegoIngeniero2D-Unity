#if UNITY_EDITOR
using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
public static class SombrasDistrito
{
 static Sprite Mancha(string name,bool light){string path="Assets/Resources/"+name+".png";var t=new Texture2D(64,64,TextureFormat.RGBA32,false);var colors=new Color32[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32))/32;float a=Mathf.Pow(Mathf.Clamp01(1-d),light?1.7f:.7f);a=Mathf.Round(a*12)/12;colors[y*64+x]=light?new Color(1,.72f,.27f,a*.18f):new Color(.16f,.18f,.12f,a*.26f);}t.SetPixels32(colors);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
 public static void Aplicar(Transform root){if(root.Find("06_Sombras_y_Luz"))return;var shadow=Mancha("SombraDistrito",false);var glow=Mancha("ResplandorDistrito",true);var group=new GameObject("06_Sombras_y_Luz").transform;group.SetParent(root,false);
 foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>().ToArray()){
 bool tree=sr.name.StartsWith("Arbol_");bool lamp=sr.name.StartsWith("Farola_");if(!tree&&!lamp)continue;var go=new GameObject((tree?"Sombra_":"Luz_")+sr.name);go.transform.SetParent(group,false);var image=go.AddComponent<SpriteRenderer>();image.sprite=tree?shadow:glow;image.sortingOrder=tree?-900:-880;float width=tree?sr.bounds.size.x*.85f:2.4f,height=tree?width*.32f:1.4f;go.transform.localScale=new Vector3(width/image.sprite.bounds.size.x,height/image.sprite.bounds.size.y,1);go.transform.position=new Vector3(sr.bounds.center.x+(tree?.25f:0),sr.bounds.min.y+.1f,0);
 }
 }
}
#endif
