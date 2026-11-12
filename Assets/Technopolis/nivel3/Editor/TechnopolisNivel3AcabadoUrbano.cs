#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Acabado de mapa solamente. Conserva personajes y objetos ajenos a los grupos N3.
public static class TechnopolisNivel3AcabadoUrbano
{
    const string Root="Assets/Technopolis/nivel3";
    const string ScenePath="Assets/Scenes/EscenaNivel3Silicio.unity";
    static Transform details;
    static Material mat;

    [MenuItem("Tools/Technopolis Nivel 3/6 - Acabado urbano fiel a referencia")]
    public static void Aplicar()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new Exception("Trabajar únicamente en el nivel 3 fuera de Play.");
        string backup=Root+"/Documentacion/Respaldo/EscenaNivel3_antes_acabado_urbano.unity";
        if(!File.Exists(backup) && !EditorSceneManager.SaveScene(scene,backup,true))
            throw new Exception("No se pudo respaldar la escena abierta.");
        var center=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="N3_MAPA_REFERENCIA");
        var expansion=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="N3_EXPANSION_CIUDAD_ALTA");
        if(center==null || expansion==null) throw new Exception("Falta mapa o expansión.");
        mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Generado/MapaReferencia_Sprites.mat");
        var old=center.transform.Find("06_Acabado_Urbano_Referencia");
        if(old!=null) Undo.DestroyObjectImmediate(old.gameObject);
        details=Group("06_Acabado_Urbano_Referencia",center.transform);
        Undo.RegisterCreatedObjectUndo(details.gameObject,"Acabado urbano Nivel 3");

        // Eliminar las franjas vacías alrededor de los edificios con jardines continuos.
        var baseMap=center.GetComponentsInChildren<Tilemap>().First(t=>t.name=="00_Terreno");
        var paving=center.GetComponentsInChildren<Tilemap>().First(t=>t.name=="01_Banquetas_y_Plazas");
        Garden(baseMap,paving,25,42,27,59);
        Garden(baseMap,paving,39,42,44,52);
        Garden(baseMap,paving,57,42,58,59);
        Garden(baseMap,paving,12,44,19,59);
        Garden(baseMap,paving,1,11,3,23);
        Garden(baseMap,paving,25,11,28,20);
        Garden(baseMap,paving,39,11,41,18);
        Garden(baseMap,paving,64,42,65,59);
        Garden(baseMap,paving,77,42,78,53);
        // Parterres verdes en la plaza: centro libre para circular.
        Garden(baseMap,paving,27,26,29,35);
        Garden(baseMap,paving,54,26,56,32);

        foreach(float y in new[]{43f,47f,51f,55f})
        {
            D("Arbol_Verde_Grande",26,y,3.4f);
            D("Arbusto_Rosa",39.5f,y,2.2f);
            D("Arbol_Joven",57.5f,y,2.3f);
            D("Seto_Largo_Flores",17.5f,y,3.2f);
            D("Arbusto_Flores_Blancas",64.5f,y,2.1f);
        }
        foreach(float y in new[]{12f,16f,20f})
        {
            D("Arbol_Verde_Grande",2,y,3.5f);
            D("Arbusto_Amarillo",27,y,2.5f);
            D("Arbol_Rosa_Florido",40,y,3);
            D("Seto_Corto",64.5f,y,2);
            D("Seto_Corto",78,y,2);
        }
        foreach(float y in new[]{27f,31f,34f})
        {
            D("Arbol_Joven",28,y,2.7f);
            D("Arbusto_Rosa",55,y,2.5f);
        }
        // Edificios secundarios entre las torres, como los callejones de la referencia.
        S("Sprites/Edificios/Edificio_Departamentos",42,42,5.5f,details);
        D("Paneles_Solares",43,50,2.1f);
        D("Bicicletero",40,41,1.7f);
        D("Gabinete_Tecnico",44,41,1.1f);
        D("Puesto_Comida",15.5f,53.5f,3.2f);
        D("Mesa_Picnic",15,50,2);

        // Jardineras y luminarias de borde: una trama más rica, sin invadir las calzadas.
        foreach(float x in new[]{2f,7f,12f,17f,27f,32f,37f,47f,52f,57f,66f,71f,76f})
        {
            D("Seto_Largo_Flores",x,4.2f,4.5f);
            if(x<20 || x>64) D("Valla_Metal_Piedra",x,2,4.8f);
        }
        foreach(float x in new[]{28f,33f,38f,44f,49f,54f})
        {
            D("Jardinera_Rosa",x,35.5f,2.8f);
            if(x<38 || x>44) D("Jardinera_Amarilla",x,25.5f,2.4f);
        }
        foreach(float y in new[]{13f,22f,32f,43f,52f}) foreach(float x in new[]{20f,25.5f,58f,63.5f})
        {
            D("Farola_Clasica_02",x,y,1.15f);
            D("Maceta_Flores",x,y+1.8f,1.1f);
        }
        foreach(float x in new[]{29f,31f,51f,53f})
        {
            D("Bolardo_Iluminado",x,25,.7f);
            D("Bolardo_Iluminado",x,34.5f,.7f);
        }
        foreach(float y in new[]{12f,15f,18f,21f})
        {
            D("Valla_Metal_Piedra",65,y,2);
            D("Valla_Metal_Piedra",78.5f,y,2);
        }
        D("Puerta_Reja",71.5f,11,3);
        D("Parada_Autobus",17.5f,30,3);
        D("Papelera_Doble",27,33,1);
        D("Papelera_Doble",55,33,1);
        D("Maquina_Expendedora",19,30,1.1f);

        // Unificar marcas de carril sin rotaciones residuales de los tiles compartidos.
        var streets=center.GetComponentsInChildren<Tilemap>().First(t=>t.name=="02_Calles");
        foreach(var cell in streets.cellBounds.allPositionsWithin)
        {
            var tile=streets.GetTile(cell);
            if(tile==null) continue;
            streets.SetTileFlags(cell,TileFlags.None);
            streets.SetTransformMatrix(cell,Matrix4x4.identity);
            if(tile.name=="Asfalto_06" && (cell.y==7 || cell.y==38))
                streets.SetTransformMatrix(cell,Matrix4x4.Rotate(Quaternion.Euler(0,0,90)));
        }
        BarrioNorte(expansion);
        var camera=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Camera>()).FirstOrDefault();
        if(camera!=null)
        {
            Undo.RecordObject(camera.transform,"Encuadrar distrito principal");
            Undo.RecordObject(camera,"Encuadrar distrito principal");
            camera.transform.position=new Vector3(40,30,-10);camera.orthographicSize=32;
        }
        if(center.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.sprite==null)) throw new Exception("Sprite nulo");
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new Exception("No se guardó la escena");
        Render(40,30,31,1600,1200,"Vista_Distrito_Detallado.png");
        Render(40,46,47,1200,1410,"Vista_Ciudad_Completa.png");
        Debug.Log("N3_ACABADO_OK: centro detallado, doce casas conservadas al norte, nivel 1 intacto.");
    }
    static void BarrioNorte(GameObject expansion)
    {
        var houses=expansion.transform.Find("01_Casas_Modernas_12");
        if(houses==null || houses.childCount!=12) throw new Exception("Se requieren las 12 casas existentes");
        // No reconstruir ni sustituir las casas; conservar sus componentes y referencias.
        int i=0;
        foreach(Transform house in houses)
        {
            var sr=house.GetComponent<SpriteRenderer>();float scale=9.5f/sr.sprite.bounds.size.x;
            float x=new[]{9f,21f,33f,47f,59f,71f}[i%6];float y=i<6?64.5f:78.5f;
            Undo.RecordObject(house,"Reubicar casa en barrio norte");
            house.localScale=Vector3.one*scale;
            house.position=new Vector3(x-sr.sprite.bounds.center.x*scale,y-sr.sprite.bounds.min.y*scale,0);
            sr.sortingOrder=1000-Mathf.RoundToInt(y*10);
            i++;
        }
        foreach(Transform child in expansion.transform.Cast<Transform>().ToArray())
            if(child!=houses) Undo.DestroyObjectImmediate(child.gameObject);
        var grid=Group("Grid_Barrio_Norte",expansion.transform);grid.gameObject.AddComponent<Grid>();
        var ground=Layer("Terreno_Norte",grid,0);Fill(ground,"Pasto_04",0,60,79,91);
        var pavement=Layer("Banquetas_Norte",grid,1);Fill(pavement,"Banquetas_01",0,60,79,91);
        var road=Layer("Calles_Norte",grid,2);
        foreach(int y in new[]{60,74,88}) Fill(road,"Asfalto_01",0,y,79,y+3);
        foreach(int x in new[]{0,38,76}) Fill(road,"Asfalto_01",x,60,x+3,91);
        var garden=Group("Jardines_Norte",expansion.transform);
        foreach(int y in new[]{71,85})
        {
            foreach(int x0 in new[]{4,42}) Garden(ground,pavement,x0,y,x0+32,y+2);
            foreach(float x in new[]{7f,19f,31f,45f,57f,69f})
            {
                S("Sprites/Decoracion/Arbol_Joven",x,y,2.3f,garden);
                S("Sprites/Decoracion/Jardinera_Rosa",x+3,y,2.2f,garden);
            }
        }
        foreach(float y in new[]{64f,78f}) foreach(float x in new[]{5f,17f,29f,43f,55f,67f,75f})
            S("Sprites/Decoracion/Farola_Moderna",x,y,1,garden);
        foreach(float x in new[]{9f,21f,33f,47f,59f,71f})
        {
            S("Sprites/Decoracion/Banco_Madera_01",x,72,2,garden);
            S("Sprites/Decoracion/Banco_Madera_01",x,86,2,garden);
        }
    }
    static Transform Group(string name,Transform parent)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static Tilemap Layer(string name,Transform parent,int order)
    {
        var g=Group(name,parent).gameObject;var tm=g.AddComponent<Tilemap>();
        var r=g.AddComponent<TilemapRenderer>();r.sharedMaterial=mat;r.sortingOrder=order;return tm;
    }
    static void Fill(Tilemap tm,string name,int x0,int y0,int x1,int y1)
    {
        var tile=AssetDatabase.LoadAssetAtPath<TileBase>(Root+"/Generado/Tiles/"+name+".asset");
        if(tile==null) throw new Exception("Falta tile "+name);
        for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)tm.SetTile(new Vector3Int(x,y,0),tile);
    }
    static void Garden(Tilemap ground,Tilemap pavement,int x0,int y0,int x1,int y1)
    {
        Fill(ground,"Pasto_04",x0,y0,x1,y1);
        for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)pavement.SetTile(new Vector3Int(x,y,0),null);
    }
    static void D(string name,float x,float y,float width) {S("Sprites/Decoracion/"+name,x,y,width,details);}
    static void S(string path,float x,float y,float width,Transform parent)
    {
        var sprite=AssetDatabase.LoadAllAssetsAtPath(Root+"/"+path+".png").OfType<Sprite>().FirstOrDefault();
        if(sprite==null)throw new Exception("Falta sprite "+path);
        var g=new GameObject(Path.GetFileName(path));g.transform.SetParent(parent,false);
        float scale=width/sprite.bounds.size.x;g.transform.localScale=Vector3.one*scale;
        g.transform.position=new Vector3(x-sprite.bounds.center.x*scale,y-sprite.bounds.min.y*scale,0);
        var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=mat;r.sortingOrder=1000-Mathf.RoundToInt(y*10);
    }
    static void Render(float x,float y,float size,int w,int h,string name)
    {
        var g=new GameObject("Camara_Previa_Temporal");var c=g.AddComponent<Camera>();c.orthographic=true;
        c.orthographicSize=size;g.transform.position=new Vector3(x,y,-100);
        c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.08f,.1f,.12f);
        var rt=new RenderTexture(w,h,24);var previous=RenderTexture.active;
        try
        {
            c.targetTexture=rt;c.Render();RenderTexture.active=rt;
            var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
            File.WriteAllBytes(Root+"/Documentacion/"+name,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
        }
        finally {RenderTexture.active=previous;c.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(g);}
    }
}
#endif
