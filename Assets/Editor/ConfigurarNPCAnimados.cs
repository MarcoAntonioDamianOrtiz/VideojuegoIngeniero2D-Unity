#if UNITY_EDITOR
using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class ConfigurarNPCAnimados
{
 static Rect[] Rectangulos(string path,out Texture2D texture){AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(path));int cw=t.width/4,ch=t.height/2;var p=t.GetPixels32();int x0=cw,x1=0,height=0;var bases=new int[8];
 for(int i=0;i<8;i++){int col=i%4,row=1-i/4,bottom=ch,top=0;for(int y=0;y<ch;y++){int count=0;for(int x=0;x<cw;x++)if(p[(row*ch+y)*t.width+col*cw+x].a>150)count++;if(count>=6){bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}}for(int x=0;x<cw;x++){int count=0;for(int y=bottom;y<=top;y++)if(p[(row*ch+y)*t.width+col*cw+x].a>150)count++;if(count>=6){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);}}bases[i]=Mathf.Max(0,bottom-2);height=Mathf.Max(height,top-bases[i]+3);}
 var result=new Rect[8];for(int i=0;i<8;i++)result[i]=new Rect((i%4)*cw+Mathf.Max(0,x0-2),(1-i/4)*ch+bases[i],Mathf.Min(cw-x0,x1-x0+5),Mathf.Min(height,ch-bases[i]));UnityEngine.Object.DestroyImmediate(t);return result;
 }
 public static void Recursos(){const string path="Assets/Resources/Personajes/AnimacionesHabitantes.asset";var data=AssetDatabase.LoadAssetAtPath<NPCBarrioRecursos>(path);if(!data){data=ScriptableObject.CreateInstance<NPCBarrioRecursos>();AssetDatabase.CreateAsset(data,path);}data.posesDonDinero=Rectangulos("Assets/Resources/Personajes/DonDineroAnimado.png",out data.donDinero);data.posesDiegoByte=Rectangulos("Assets/Resources/Personajes/DiegoByteAnimado.png",out data.diegoByte);EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();}
 public static void Escena(){var h=UnityEngine.Object.FindFirstObjectByType<HabitantesSilicio>();if(!h)throw new Exception("Abre Silicio");foreach(var t in new[]{h.donDinero,h.diegoByte})if(!t.GetComponent<NPCBarrioAnimado>())t.gameObject.AddComponent<NPCBarrioAnimado>();EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveScene(h.gameObject.scene);}
}
#endif
