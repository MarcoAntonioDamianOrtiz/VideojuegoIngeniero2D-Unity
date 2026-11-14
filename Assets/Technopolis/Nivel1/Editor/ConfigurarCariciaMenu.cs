#if UNITY_EDITOR
using System.IO;using UnityEditor;using UnityEngine;using UnityEngine.UI;
public static class ConfigurarCariciaMenu
{
 public static void Aplicar(TechnopolisMenuAnimacion a,RectTransform personajes){
 const string path="Assets/Sprites/Menu/Personajes/AlexBitCariciaFluida.png";AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.filterMode=FilterMode.Point;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();a.caricias=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(path));int cw=t.width/4,ch=t.height/3;var p=t.GetPixels32();int x0=cw,x1=0,height=0;var bases=new int[12];
 for(int i=0;i<12;i++){
 int col=i%4,row=2-i/4,bottom=ch,top=0;
 for(int y=0;y<ch;y++){int count=0;for(int x=0;x<cw;x++)if(p[(row*ch+y)*t.width+col*cw+x].a>160)count++;if(count>=10){bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}}
 for(int x=0;x<cw;x++){int count=0;for(int y=bottom;y<=top;y++)if(p[(row*ch+y)*t.width+col*cw+x].a>160)count++;if(count>=8){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);}}
 bases[i]=Mathf.Max(0,bottom-2);height=Mathf.Max(height,top-bases[i]+3);
 }
 a.recortesCaricia=new Rect[12];for(int i=0;i<12;i++)a.recortesCaricia[i]=new Rect((i%4)*cw+Mathf.Max(0,x0-2),(2-i/4)*ch+bases[i],Mathf.Min(cw-x0,x1-x0+5),Mathf.Min(height,ch-bases[i]));Object.DestroyImmediate(t);
 a.personajes=personajes;a.alexSentado=personajes.Find("JugadorSentado").GetComponent<Image>();a.bitSentado=personajes.Find("PerroBitSentado").GetComponent<Image>();EditorUtility.SetDirty(a);
 }
}
#endif
