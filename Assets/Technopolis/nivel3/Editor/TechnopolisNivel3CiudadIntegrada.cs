#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Reutiliza las viviendas y edificios existentes. No trabaja sobre personajes ni otros niveles.
public static class TechnopolisNivel3CiudadIntegrada
{
    const string Root="Assets/Technopolis/nivel3";
    const string ScenePath="Assets/Scenes/EscenaNivel3Silicio.unity";
    const string MapName="N3_CIUDAD_INTEGRADA";
    static Material mat;
    static Transform map, structures, decor;
    static readonly List<SpriteRenderer> occupied=new List<SpriteRenderer>();

    [MenuItem("Tools/Technopolis Nivel 3/7 - Integrar casas y edificios sin superposiciones")]
    public static void Aplicar()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Abre únicamente el nivel 3 fuera de Play.");
        string backup=Root+"/Documentacion/Respaldo/EscenaNivel3_antes_ciudad_integrada.unity";
        if(!File.Exists(backup) && !EditorSceneManager.SaveScene(scene,backup,true))
            throw new Exception("No se pudo respaldar el estado actual.");
        var sources=scene.GetRootGameObjects().Where(g=>g.name=="N3_MAPA_REFERENCIA" ||
            g.name=="N3_EXPANSION_CIUDAD_ALTA" || g.name==MapName).ToArray();
        var sprites=sources.SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
        var homes=sprites.Where(s=>s.name.StartsWith("Vivienda_",StringComparison.Ordinal)).OrderBy(s=>s.name).ToArray();
        if(homes.Length!=12) throw new Exception("Se requieren las doce casas existentes.");
        mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Generado/MapaReferencia_Sprites.mat");
        occupied.Clear();
        map=Group(MapName+"_Nueva",null);structures=Group("01_Manzanas_Mixtas",map);
        decor=Group("02_Jardines_y_Mobiliario",map);
        Undo.RegisterCreatedObjectUndo(map.gameObject,"Integrar ciudad nivel 3");

        // Una misma trama para viviendas, torres y comercios. Calle de cuatro unidades,
        // separación mínima entre fachadas y zonas de acceso al frente de cada lote.
        var grid=Group("03_Grid_Ciudad_82x88",map);grid.gameObject.AddComponent<Grid>();
        var ground=Layer("Terreno",grid,0);Fill(ground,"Pasto_04",0,0,93,91);
        var pavement=Layer("Banquetas_Continuas",grid,1);Fill(pavement,"Banquetas_01",0,4,93,91);
        var roads=Layer("Calles_Conectadas",grid,2);
        foreach(int x in new[]{0,30,60,90})Fill(roads,"Asfalto_01",x,6,x+3,91);
        foreach(int y in new[]{6,34,62,88})Fill(roads,"Asfalto_01",0,y,93,y+3);
        var plaza=Layer("Plaza_Central",grid,3);Fill(plaza,"Plaza_Piedra_02",35,39,58,60);

        // Los ocho lotes alrededor de la plaza mezclan usos; no queda un barrio aparte.
        float[,] homePositions={
            {25,11},{55,11},{55,23},{85,11},{85,23},
            {25,39},{25,51},{85,39},{85,52},
            {25,67},{55,67},{85,67}};
        for(int i=0;i<homes.Length;i++) Position(homes[i],homePositions[i,0],homePositions[i,1],7.5f);
        Move(sprites,"Edificio_Bitforce",12,11,13);
        Move(sprites,"Edificio_Byteforce_Corp",42,11,11);
        Move(sprites,"Edificio_Silicon_Alley",72,11,12);
        Move(sprites,"Edificio_Conveniencia_24_7",12,39,12);
        Move(sprites,"Edificio_Servicio_Tecnico",12,51,10);
        Move(sprites,"Edificio_Clinica_Distrito",72,39,11);
        Move(sprites,"Edificio_Hardware_Electronica",72,52,11);
        Move(sprites,"Edificio_Cybernetics_LTD",12,67,11);
        Move(sprites,"Edificio_Global_Corp",42,67,10);
        Move(sprites,"Casa_Principal_Fachada_Ciudad",25,77,6.5f);
        Move(sprites,"Edificio_Comunicaciones",55,77,7);
        Move(sprites,"Casa_Techo_Azul",55,83.5f,5);
        Move(sprites,"Casa_Techo_Rojo",85,78,6);
        Move(sprites,"Casa_Techo_Gris",85,84,4.5f);
        Move(sprites,"Negocio_Fideos",72,79,9);
        Move(sprites,"Edificio_Departamentos",72,67,10);
        var cafe=Add("Expansiones/CiudadAlta/Sprites/Negocios/Negocio_Cafeteria",42,25,7,structures);
        occupied.Add(cafe);

        // Comprobar antes de desactivar el mapa anterior o guardar el nuevo.
        var conflicts=new List<string>();
        for(int i=0;i<occupied.Count;i++)for(int j=i+1;j<occupied.Count;j++)
            if(Intersects(occupied[i].bounds,occupied[j].bounds,0))
                conflicts.Add(occupied[i].name+" / "+occupied[j].name);
        if(conflicts.Count>0) throw new Exception("Construcciones superpuestas: "+string.Join("; ",conflicts));
        foreach(var s in occupied)
            if(s.bounds.min.x<4 || s.bounds.max.x>78 || s.bounds.min.y<10 || s.bounds.max.y>84)
                throw new Exception("Construcción fuera del lote: "+s.name);

        // Parterres estrechos en los bordes de cada manzana, sin ocultar fachadas.
        foreach(int y0 in new[]{10,38,66})foreach(int x0 in new[]{4,34,64})
        {
            if(x0==34 && y0==38)continue;
            Garden(ground,pavement,x0,y0,x0+1,y0+22);
            foreach(float yy in new[]{y0+2f,y0+11f,y0+20f})
                SafeDecor("Arbol_Joven",x0+1,yy,1.8f);
            foreach(float xx in new[]{x0+4f,x0+12f,x0+22f})
                SafeDecor("Jardinera_Rosa",xx,y0+.2f,1.6f);
        }
        // Plaza compartida; las casas miran hacia las mismas calles que los comercios.
        D("Fuente_Globo_Holografico",47,46,7);
        D("Parada_Autobus",47,60,7);
        D("Pantalla_Info",38,44,2.2f);D("Tablon_Anuncios",56,44,2.4f);
        D("Senal_Direccional_Grande",57,56,3.4f);
        foreach(float x in new[]{39f,54f})foreach(float y in new[]{42f,54f})
        {D("Banco_Madera_01",x,y,2.7f);D("Jardinera_Rosa",x,y+2,2.3f);}
        foreach(float x in new[]{36.5f,58f})foreach(float y in new[]{42f,49f,57f})
        {D("Arbol_Joven",x,y,2.1f);D("Bolardo_Iluminado",x+1,y,.6f);}
        D("Papelera_Doble",37,47,.9f);D("Papelera_Doble",57,47,.9f);
        // Servicios y jardines pequeños repartidos por toda la ciudad.
        foreach(float y in new[]{10f,38f,66f})foreach(float x in new[]{4.6f,29f,34.6f,59f,64.6f,89f})
            SafeDecor("Farola_Moderna",x,y,1);
        foreach(float y in new[]{31f,59f,86f})foreach(float x in new[]{7f,20f,37f,50f,67f,80f})
        {
            SafeDecor("Arbusto_Flores_Blancas",x,y,2.2f);
            SafeDecor("Banco_Madera_01",x+3,y,2.1f);
        }
        foreach(float x in new[]{7f,13f,19f,25f,37f,43f,49f,55f,67f,73f,79f,85f})
        {if(x==43 || x==49)continue;D("Seto_Largo_Flores",x,3,4.2f);D("Valla_Metal_Piedra",x,1,5.5f);}
        D("Rotulo_Distrito_Silicio",47,1,9);D("Bandera_Circuito_01",40,1,1.7f);D("Bandera_Circuito_02",54,1,1.7f);
        LaneLines(roads);
        foreach(float x in new[]{31.5f,61.5f})foreach(float y in new[]{11f,39f,67f})
        {var s=D("Marca_Paso_Peatonal",x,y,3.4f);s.sortingOrder=4;}
        foreach(float y in new[]{7f,35f,63f})foreach(float x in new[]{18f,47f,78f})
        {var s=D("Marca_Flechas_Derecha",x,y,1.5f);s.sortingOrder=4;}

        foreach(var source in sources)
        {
            if(source.name==MapName) Undo.DestroyObjectImmediate(source);
            else {Undo.RecordObject(source,"Conservar respaldo de grupos previos");source.SetActive(false);}
        }
        map.name=MapName;
        var camera=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Camera>()).FirstOrDefault();
        if(camera!=null)
        {
            Undo.RecordObject(camera.transform,"Encuadrar ciudad integrada");Undo.RecordObject(camera,"Encuadrar ciudad integrada");
            camera.transform.position=new Vector3(41,44,-10);camera.orthographicSize=45;
        }
        // Todos los edificios conservan su objeto y collider; el mobiliario se adapta a los huecos.
        if(map.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.sprite==null))throw new Exception("Sprite nulo");
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new Exception("No se guardó la escena");
        Render();
        File.WriteAllText(Root+"/Documentacion/Validacion_Ciudad_Integrada.txt",
            "Escena nivel 3 guardada.\n12 casas modernas integradas en manzanas mixtas.\n"+
            occupied.Count+" construcciones comprobadas por limites visibles.\n0 pares de construcciones superpuestos.\n"+
            "Mapa 82x88; calles conectadas y plaza central.\nNo se modifican personajes ni nivel 1.\n");
        Debug.Log("N3_INTEGRADA_OK: "+occupied.Count+" construcciones; 12 casas; 0 superposiciones.");
    }
    static Transform Group(string name,Transform parent)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static void Position(SpriteRenderer sr,float x,float y,float width)
    {
        x=X(x);y=Y(y);
        Undo.SetTransformParent(sr.transform,structures,"Integrar construcción");
        Undo.RecordObject(sr.transform,"Ajustar lote");Undo.RecordObject(sr,"Ordenar fachada");
        float scale=width/sr.sprite.bounds.size.x;sr.transform.localScale=Vector3.one*scale;
        sr.transform.position=new Vector3(x-sr.sprite.bounds.center.x*scale,y-sr.sprite.bounds.min.y*scale,0);
        sr.sortingOrder=1000-Mathf.RoundToInt(y*10);sr.gameObject.SetActive(true);occupied.Add(sr);
    }
    static void Move(SpriteRenderer[] sprites,string name,float x,float y,float width)
    {
        var sr=sprites.FirstOrDefault(s=>s.name==name);
        if(sr==null)throw new Exception("Falta construcción "+name);Position(sr,x,y,width);
    }
    static Tilemap Layer(string name,Transform parent,int order)
    {
        var g=Group(name,parent).gameObject;var tm=g.AddComponent<Tilemap>();var r=g.AddComponent<TilemapRenderer>();
        r.sharedMaterial=mat;r.sortingOrder=order;return tm;
    }
    static void Fill(Tilemap tm,string name,int x0,int y0,int x1,int y1)
    {
        var tile=AssetDatabase.LoadAssetAtPath<TileBase>(Root+"/Generado/Tiles/"+name+".asset");
        if(tile==null)throw new Exception("Falta tile "+name);
        for(int y=YC(y0);y<=YC(y1);y++)for(int x=XC(x0);x<=XC(x1);x++)tm.SetTile(new Vector3Int(x,y,0),tile);
    }
    static void Garden(Tilemap ground,Tilemap pavement,int x0,int y0,int x1,int y1)
    {Fill(ground,"Pasto_04",x0,y0,x1,y1);for(int y=YC(y0);y<=YC(y1);y++)for(int x=XC(x0);x<=XC(x1);x++)pavement.SetTile(new Vector3Int(x,y,0),null);}
    static SpriteRenderer Add(string path,float x,float y,float width,Transform parent)
    {
        x=X(x);y=Y(y);
        var sprite=AssetDatabase.LoadAllAssetsAtPath(Root+"/"+path+".png").OfType<Sprite>().FirstOrDefault();
        if(sprite==null)throw new Exception("Falta sprite "+path);
        var g=new GameObject(Path.GetFileName(path));g.transform.SetParent(parent,false);
        float scale=width/sprite.bounds.size.x;g.transform.localScale=Vector3.one*scale;
        g.transform.position=new Vector3(x-sprite.bounds.center.x*scale,y-sprite.bounds.min.y*scale,0);
        var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=mat;r.sortingOrder=1000-Mathf.RoundToInt(y*10);return r;
    }
    static SpriteRenderer D(string name,float x,float y,float width)
    {return Add("Sprites/Decoracion/"+name,x,y,width,decor);}
    static bool Intersects(Bounds a,Bounds b,float tolerance)
    {return a.min.x<b.max.x-tolerance && a.max.x>b.min.x+tolerance && a.min.y<b.max.y-tolerance && a.max.y>b.min.y+tolerance;}
    static void SafeDecor(string name,float x,float y,float width)
    {
        var sr=D(name,x,y,width);
        if(occupied.Any(s=>Intersects(s.bounds,sr.bounds,.05f)))UnityEngine.Object.DestroyImmediate(sr.gameObject);
    }
    static void LaneLines(Tilemap roads)
    {
        var tile=AssetDatabase.LoadAssetAtPath<TileBase>(Root+"/Generado/Tiles/Asfalto_06.asset");
        foreach(int x in new[]{1,31,61,91})for(int y=10;y<88;y++)
            if(!(y>=32&&y<=39 || y>=60&&y<=67))roads.SetTile(new Vector3Int(XC(x),YC(y),0),tile);
        foreach(int y in new[]{7,35,63,89})for(int x=4;x<90;x++)
        {
            if(x>=28&&x<=35 || x>=58&&x<=65)continue;
            var cell=new Vector3Int(XC(x),YC(y),0);roads.SetTile(cell,tile);roads.SetTileFlags(cell,TileFlags.None);
            roads.SetTransformMatrix(cell,Matrix4x4.Rotate(Quaternion.Euler(0,0,90)));
        }
    }
    // Reducir los espacios de los lotes sin deformar sprites ni estrechar las calles.
    static float X(float x)
    {int block=Mathf.FloorToInt(x/30);float local=x-block*30;return block*26+(local<4?local:4+(local-4)*22/26);}
    static float Y(float y) {return y-(y>=62?4:y>=34?2:0);}
    static int XC(int x) {return Mathf.FloorToInt(X(x)+.0001f);}
    static int YC(int y) {return Mathf.RoundToInt(Y(y));}
    public static void Render()
    {
        var g=new GameObject("N3_CAMARA_PREVIA_TEMP");var c=g.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=45;
        g.transform.position=new Vector3(41,44,-100);c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.08f,.1f,.12f);
        var rt=new RenderTexture(1800,1800,24);var prev=RenderTexture.active;
        try
        {
            c.targetTexture=rt;c.Render();RenderTexture.active=rt;var tex=new Texture2D(1800,1800,TextureFormat.RGB24,false);
            tex.ReadPixels(new Rect(0,0,1800,1800),0,0);tex.Apply();
            File.WriteAllBytes(Root+"/Documentacion/Vista_Ciudad_Integrada.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
        }
        finally{RenderTexture.active=prev;c.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(g);}
    }
}
#endif
