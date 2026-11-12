#if UNITY_EDITOR
using System.IO;using UnityEditor;using UnityEngine;using UnityEngine.UI;
public static class ConfigurarCariciaMenu
{
 public static void Aplicar(TechnopolisMenuAnimacion a,RectTransform personajes){
 const string path="Assets/Sprites/Menu/Personajes/AlexBitCaricia.png";AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.filterMode=FilterMode.Point;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();a.caricias=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(path));int cw=t.width/2,ch=t.height/2;var p=t.GetPixels32();int x0=cw,y0=ch,x1=0,y1=0;
 for(int row=0;row<2;row++)for(int col=0;col<2;col++)for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(p[(row*ch+y)*t.width+col*cw+x].a>32){x0=Mathf.Min(x0,x);y0=Mathf.Min(y0,y);x1=Mathf.Max(x1,x);y1=Mathf.Max(y1,y);}
 a.recortesCaricia=new Rect[4];for(int i=0;i<4;i++)a.recortesCaricia[i]=new Rect((i%2)*cw+x0,(1-i/2)*ch+y0,x1-x0+1,y1-y0+1);Object.DestroyImmediate(t);
 a.personajes=personajes;a.alexSentado=personajes.Find("JugadorSentado").GetComponent<Image>();a.bitSentado=personajes.Find("PerroBitSentado").GetComponent<Image>();EditorUtility.SetDirty(a);
 }
}
#endif
