#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Plano medido sobre la referencia de 1448x1086. Los edificios y detalles siguen siendo
// sprites separados. Solo el pavimento se compone en una textura a partir de tiles locales.
public static class TechnopolisNivel3PlanoFotografico
{
    const string Root="Assets/Technopolis/nivel3";
    const string ScenePath="Assets/Scenes/EscenaNivel3Silicio.unity";
    const string Name="N3_DISTRITO_TRAZADO_FOTO";
    const int W=1448,H=1086;
    const float PPU=24;
    static Transform root, buildings, greenery, props;
    static Material material;
    static Color32[] pixels;
    static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
    static readonly List<RectInt> roadRects=new List<RectInt>();
    static int count;

    [MenuItem("Tools/Technopolis Nivel 3/8 - Reconstruir siguiendo la foto")]
    public static void Construir()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new Exception("Abrir únicamente EscenaNivel3Silicio fuera de Play.");
        string backup=Root+"/Documentacion/Respaldo/EscenaNivel3_antes_plano_fotografico.unity";
        if(!File.Exists(backup) && !EditorSceneManager.SaveScene(scene,backup,true))throw new Exception("No se pudo respaldar.");
        count=0;
        material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Generado/MapaReferencia_Sprites.mat");
        root=Group(Name+"_Construccion",null);
        Undo.RegisterCreatedObjectUndo(root.gameObject,"Trazar nivel 3 como foto");
        buildings=Group("01_Edificios_Posiciones_Foto",root);
        greenery=Group("02_Vegetacion",root);props=Group("03_Mobiliario",root);
        CrearSuelo();

        // Rectángulos medidos en píxeles de la foto: x, y superior, ancho y alto.
        B("Edificio_Cybernetics_LTD",315,24,220,328);
        B("Edificio_Global_Corp",878,15,236,338);
        B("Edificio_Bitforce",78,548,284,367);
        B("Edificio_Byteforce_Corp",455,647,241,270);
        B("Edificio_Silicon_Alley",755,570,338,353);
        B("Edificio_Conveniencia_24_7",13,296,208,213);
        B("Edificio_Servicio_Tecnico",248,380,134,127);
        B("Edificio_Clinica_Distrito",1231,248,204,187);
        B("Edificio_Hardware_Electronica",1095,405,214,153);
        B("Edificio_Comunicaciones",1231,10,211,150);
        B("Negocio_Fideos",673,0,143,126);
        B("Edificio_Departamentos",776,157,110,179);
        B("Casa_Techo_Azul",17,190,122,116);
        B("Casa_Techo_Rojo",130,186,126,120);
        B("Casa_Techo_Rojo",1354,469,91,91);
        B("Casa_Techo_Azul",1134,620,143,133);
        B("Casa_Techo_Azul",1270,626,151,136);

        D("Fuente_Globo_Holografico",609,441,155,145);
        D("Parada_Autobus",661,332,152,95);
        D("Pantalla_Info",493,514,69,82);
        D("Tablon_Anuncios",794,506,77,80);
        D("Senal_Direccional_Grande",907,375,146,111);
        D("Banco_Madera_01",535,457,57,42);D("Banco_Madera_02",779,456,54,42);
        D("Banco_Madera_01",1172,895,76,43);
        D("Estatua_Robot",1328,810,53,76);
        D("Papelera_Reciclaje_Azul",570,581,26,39);
        D("Papelera_Gris",844,585,25,39);
        D("Paneles_Solares",182,85,53,45);
        D("Paneles_Solares",1234,181,67,48);D("Paneles_Solares",1311,181,67,48);
        D("Antena_Satelital",1380,1,62,90);
        D("Mesa_Picnic",38,17,80,53);
        Letrero();
        D("Bandera_Circuito_01",588,997,36,73);D("Bandera_Circuito_02",844,997,36,73);
        // Pilares y reja de entrada con los recursos disponibles, sin rótulos añadidos.
        D("Puerta_Reja",574,942,74,139);D("Puerta_Reja",798,942,74,139);

        // Jardines y árboles alrededor de las mismas manzanas, no en parcelas separadas.
        G("Arbol_Verde_Grande",9,-22,88,116);
        G("Arbol_Rosa_Florido",50,42,124,120);
        G("Arbol_Verde_Grande",166,-18,102,136);
        G("Arbol_Verde_Grande",130,111,79,88);
        G("Arbol_Verde_Grande",203,77,63,88);
        G("Arbol_Verde_Grande",560,133,89,105);
        G("Arbol_Verde_Grande",580,223,85,99);
        G("Arbol_Verde_Grande",716,137,81,114);
        G("Seto_Largo_Flores",620,140,60,28);
        G("Seto_Largo_Flores",570,80,60,40);
        G("Arbusto_Flores_Blancas",552,53,50,38);
        G("Arbusto_Flores_Blancas",635,1,60,34);
        G("Arbusto_Flores_Blancas",818,0,58,40);
        G("Arbusto_Rosa",716,270,48,40);
        G("Arbusto_Rosa",1120,340,40,44);
        G("Arbol_Verde_Grande",1112,14,70,103);
        G("Arbol_Verde_Grande",1109,112,67,102);
        G("Arbol_Verde_Grande",1113,241,65,95);
        G("Arbol_Rosa_Florido",1202,329,80,91);
        G("Arbol_Verde_Grande",1016,433,64,90);
        G("Arbol_Verde_Grande",467,412,79,102);
        G("Arbol_Verde_Grande",806,421,80,102);
        G("Arbol_Verde_Grande",459,507,43,75);
        G("Arbol_Verde_Grande",811,524,70,96);
        G("Arbol_Rosa_Florido",641,787,101,116);
        G("Arbol_Verde_Grande",35,567,65,100);
        G("Arbol_Verde_Grande",30,710,67,99);
        G("Arbol_Verde_Grande",332,607,67,103);
        G("Arbol_Verde_Grande",333,760,65,100);
        G("Arbol_Verde_Grande",24,925,98,115);
        G("Arbol_Verde_Grande",1118,602,93,114);
        G("Arbol_Verde_Grande",1351,586,85,104);
        G("Arbol_Verde_Grande",1357,744,87,132);
        G("Arbol_Verde_Grande",1125,774,68,103);
        G("Arbol_Verde_Grande",1366,908,75,107);
        foreach(int y in new[]{763,809,855,901,947})
        {G("Seto_Corto",1118,y,33,48);G("Seto_Corto",1409,y,33,48);}
        foreach(int x in new[]{1144,1185,1226,1267,1308,1349,1390})
        {G("Seto_Largo_Flores",x,750,49,29);G("Seto_Largo_Flores",x,954,49,29);}
        foreach(int y in new[]{77,171,265})
        {
            G("Arbusto_Flores_Blancas",293,y,33,57);
            G("Arbusto_Rosa",540,y,35,54);
            G("Arbusto_Flores_Blancas",858,y,30,55);
            G("Arbusto_Rosa",1118,y,30,54);
        }
        foreach(int y in new[]{442,498,554})
        {
            G("Jardinera_Rosa",476,y,48,39);
            G("Jardinera_Amarilla",827,y,48,39);
        }
        G("Jardinera_Rosa",598,581,53,40);G("Jardinera_Rosa",748,581,53,40);
        foreach(int x in new[]{5,65,125,185,245,305,365,425,485,545,905,965,1025,1085,1145,1205,1265,1325,1385})
        {
            G("Seto_Largo_Flores",x,988,66,46);
            D("Valla_Metal_Piedra",x,1024,68,61);
        }
        foreach(Vector2 p in new[]{
            new Vector2(263,100),new Vector2(274,276),new Vector2(575,277),
            new Vector2(658,243),new Vector2(854,246),new Vector2(1143,240),
            new Vector2(388,419),new Vector2(444,441),new Vector2(590,438),
            new Vector2(867,441),new Vector2(1003,444),new Vector2(1010,535),
            new Vector2(479,545),new Vector2(576,546),new Vector2(871,546),
            new Vector2(397,646),new Vector2(399,810),new Vector2(738,648),
            new Vector2(1109,653),new Vector2(1106,812),new Vector2(52,842),
            new Vector2(270,978),new Vector2(971,983)})
            D("Farola_Clasica_01",p.x,p.y,21,76);
        foreach(int x in new[]{472,499,573,800,831,863})
        {D("Bolardo_Iluminado",x,600,13,29);D("Bolardo_Iluminado",x,409,13,29);}
        D("Marca_Flechas_Arriba",419,521,30,41,5);
        D("Marca_Flechas_Arriba",947,521,30,41,5);
        D("Marca_Flechas_Derecha",510,381,44,25,5);
        D("Marca_Flechas_Derecha",657,642,47,25,5);
        D("Marca_Flechas_Arriba",9,682,25,37,5);

        // Solo se sustituyen los grupos de mapa que creamos; no buscar ni tocar personajes.
        foreach(var previous in scene.GetRootGameObjects())
        {
            if(previous==root.gameObject)continue;
            if(previous.name.StartsWith("N3_",StringComparison.Ordinal) || previous.name=="Grid_Nivel3")
            {Undo.RecordObject(previous,"Archivar versión previa nivel 3");previous.SetActive(false);}
        }
        root.name=Name;
        if(root.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.sprite==null))throw new Exception("Falta un sprite");
        var camera=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Camera>()).FirstOrDefault();
        if(camera!=null)
        {
            Undo.RecordObject(camera,"Encuadrar plano de referencia");Undo.RecordObject(camera.transform,"Encuadrar plano de referencia");
            camera.transform.position=new Vector3(W/(2*PPU),H/(2*PPU),-10);
            camera.orthographic=true;camera.orthographicSize=H/(2*PPU);
            camera.backgroundColor=new Color(.07f,.09f,.08f);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new Exception("No se pudo guardar");
        Vista();
        File.WriteAllText(Root+"/Documentacion/Validacion_Plano_Foto.txt",
            "Plano 1448x1086 trazado sobre la referencia.\n17 fachadas en posiciones medidas.\n"+
            count+" sprites separados.\nPavimento compuesto exclusivamente con los tiles del nivel 3.\n"+
            "Sin usar la fotografía como fondo. Sin añadir ni modificar personajes.\n");
        Debug.Log("N3_FOTO_OK: "+count+" sprites; plano trazado 1448x1086.");
    }
    static Transform Group(string name,Transform parent)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static void B(string n,float x,float y,float w,float h)
    {
        var sr=Place("Edificios",n,x,y,w,h,buildings,1000+Mathf.RoundToInt(y+h));
        var box=sr.gameObject.AddComponent<BoxCollider2D>();var b=sr.sprite.bounds;
        box.size=new Vector2(b.size.x*.88f,b.size.y*.12f);
        box.offset=new Vector2(b.center.x,b.min.y+box.size.y*.5f);
    }
    static void G(string n,float x,float y,float w,float h) {Place("Decoracion",n,x,y,w,h,greenery,1000+Mathf.RoundToInt(y+h));}
    static void D(string n,float x,float y,float w,float h,int order=-1)
    {Place("Decoracion",n,x,y,w,h,props,order<0?1000+Mathf.RoundToInt(y+h):order);}
    static SpriteRenderer Place(string category,string n,float x,float y,float w,float h,Transform parent,int order)
    {
        var sprite=AssetDatabase.LoadAllAssetsAtPath(Root+"/Sprites/"+category+"/"+n+".png").OfType<Sprite>().FirstOrDefault();
        if(sprite==null)throw new Exception("Falta sprite "+n);
        var g=new GameObject(n);g.transform.SetParent(parent,false);var b=sprite.bounds;
        float sx=w/PPU/b.size.x,sy=h/PPU/b.size.y;
        g.transform.localScale=new Vector3(sx,sy,1);
        g.transform.position=new Vector3(x/PPU-b.min.x*sx,(H-y-h)/PPU-b.min.y*sy,0);
        var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;sr.sortingOrder=order;count++;return sr;
    }
    static Texture2D Texture(string n)
    {
        if(textures.TryGetValue(n,out var t))return t;
        t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(Root+"/Sprites/Terreno/"+n+".png"));
        textures[n]=t;return t;
    }
    static void Tile(string n,int x,int y,int w,int h,int period=48)
    {
        var t=Texture(n);var source=t.GetPixels32();
        for(int yy=Mathf.Max(0,y);yy<Mathf.Min(H,y+h);yy++)for(int xx=Mathf.Max(0,x);xx<Mathf.Min(W,x+w);xx++)
        {
            int tx=(xx%period)*t.width/period,ty=t.height-1-(yy%period)*t.height/period;
            pixels[(H-1-yy)*W+xx]=source[ty*t.width+tx];
        }
    }
    static void ColorRect(int x,int y,int w,int h,Color32 color)
    {for(int yy=Mathf.Max(0,y);yy<Mathf.Min(H,y+h);yy++)for(int xx=Mathf.Max(0,x);xx<Mathf.Min(W,x+w);xx++)pixels[(H-1-yy)*W+xx]=color;}
    static void Road(int x,int y,int w,int h)
    {
        roadRects.Add(new RectInt(x,y,w,h));
        ColorRect(x-5,y-5,w+10,h+10,new Color32(113,104,97,255));
        ColorRect(x-3,y-3,w+6,h+6,new Color32(194,173,144,255));Tile("Asfalto_01",x,y,w,h,96);
    }
    static void Cross(int x,int y,bool vertical)
    {
        var white=new Color32(222,213,194,255);
        for(int i=0;i<6;i++)if(vertical)ColorRect(x+i*7,y,4,34,white);else ColorRect(x,y+i*7,34,4,white);
    }
    static void CrearSuelo()
    {
        roadRects.Clear();pixels=new Color32[W*H];Tile("Banquetas_01",0,0,W,H,32);
        Tile("Pasto_04",15,0,230,190);Tile("Pasto_04",563,18,94,327);
        Tile("Pasto_04",1115,0,46,355);Tile("Pasto_04",1227,0,221,250);
        Tile("Pasto_04",1117,747,315,232);Tile("Pasto_04",0,987,W,99);
        Tile("Pasto_04",51,576,46,343);Tile("Pasto_04",334,610,59,306);
        Tile("Pasto_04",642,777,98,141);
        Road(246,0,45,365);Road(1159,0,55,367);
        Road(291,367,923,53);Road(393,367,54,562);
        Road(0,514,415,51);Road(447,634,649,47);
        Road(932,420,55,192);Road(987,562,461,50);
        Road(1069,612,51,321);Road(0,930,1120,49);
        Road(0,565,44,365);Road(679,978,60,108);
        foreach(var r in roadRects)Tile("Asfalto_01",r.x,r.y,r.width,r.height,96);
        Tile("Banquetas_01",467,423,424,206,32);
        Tile("Pasto_04",470,432,43,185);Tile("Pasto_04",846,431,42,185);
        Tile("Banquetas_01",574,0,45,324,32);
        Tile("Tierra_Caminos_01",1137,889,245,26,48);
        Tile("Tierra_Caminos_01",1299,787,24,170,48);
        // Marcas continuas coherentes con el trazado; los cruces quedan en accesos reales.
        var yellow=new Color32(220,182,67,255);
        for(int y=20;y<350;y+=48){ColorRect(267,y,3,17,yellow);ColorRect(1184,y,3,17,yellow);}
        for(int y=443;y<916;y+=48){if(y<510 || y>572)ColorRect(418,y,3,18,yellow);}
        for(int x=22;x<1100;x+=48)ColorRect(x,953,19,3,yellow);
        for(int x=1010;x<1440;x+=48)ColorRect(x,585,19,3,yellow);
        for(int x=21;x<373;x+=48)ColorRect(x,539,19,3,yellow);
        for(int x=300;x<1155;x+=48)if(x<655 || x>819)ColorRect(x,391,19,3,yellow);
        Cross(410,384,true);Cross(409,596,true);Cross(408,902,true);Cross(1044,583,false);
        Cross(322,520,false);Cross(1159,318,false);Cross(942,610,true);Cross(199,937,false);
        ColorRect(636,948,177,99,new Color32(57,59,76,255));
        ColorRect(640,952,169,91,new Color32(142,146,164,255));
        ColorRect(644,956,161,83,new Color32(33,52,67,255));
        string dir=Root+"/Generado/PlanoFoto";Directory.CreateDirectory(dir);
        string path=dir+"/Suelo_Distrito_Trazado.png";
        var texture=new Texture2D(W,H,TextureFormat.RGBA32,false);texture.SetPixels32(pixels);texture.Apply();
        File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        foreach(var t in textures.Values)UnityEngine.Object.DestroyImmediate(t);textures.Clear();pixels=null;
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=PPU;importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
        importer.SaveAndReimport();
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null)throw new Exception("Falta suelo generado");
        var g=Group("00_Suelo_Trazado_Desde_Tiles",root).gameObject;
        g.transform.position=new Vector3(W/(2*PPU),H/(2*PPU),0);
        var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;sr.sortingOrder=0;
    }
    static void Letrero()
    {
        var g=Group("Letrero_Distrito_de_Silicio",props).gameObject;
        var text=g.AddComponent<TextMeshPro>();
        text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if(text.font==null)throw new Exception("Falta fuente del letrero");
        text.text="<b>DISTRITO\nDE SILICIO</b>\n<size=65%><color=#C69CFF>[NIVEL 3]</color></size>";
        text.color=new Color(.4f,.9f,1);text.alignment=TextAlignmentOptions.Center;
        text.enableAutoSizing=true;text.fontSizeMin=1;text.fontSizeMax=8;
        text.rectTransform.sizeDelta=new Vector2(153/PPU,78/PPU);
        text.rectTransform.position=new Vector3(724.5f/PPU,(H-997.5f)/PPU,-.1f);
        text.GetComponent<MeshRenderer>().sortingOrder=3000;
    }
    public static void Vista()
    {
        var g=new GameObject("N3_Camara_Previa_Temporal");var c=g.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=H/(2*PPU);
        g.transform.position=new Vector3(W/(2*PPU),H/(2*PPU),-100);c.clearFlags=CameraClearFlags.SolidColor;
        var rt=new RenderTexture(W,H,24);var prev=RenderTexture.active;
        try
        {
            c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(W,H,TextureFormat.RGB24,false);
            t.ReadPixels(new Rect(0,0,W,H),0,0);t.Apply();
            File.WriteAllBytes(Root+"/Documentacion/Vista_Plano_Fotografico.png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
        }
        finally{RenderTexture.active=prev;c.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(g);}
    }
}
#endif
