#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Ampliación aditiva: conserva íntegro el centro aprobado y los objetos ajenos al mapa.
public static class TechnopolisNivel3CiudadAlta
{
    const string Root="Assets/Technopolis/nivel3";
    const string ScenePath="Assets/Scenes/EscenaNivel3Silicio.unity";
    const string Name="N3_EXPANSION_CIUDAD_ALTA";
    static Transform root, houses, greenery, furniture;
    static Material material;

    [MenuItem("Tools/Technopolis Nivel 3/5 - Ampliar ciudad con casas modernas")]
    public static void Ampliar()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Abre el nivel 3 fuera de Play.");
        var backup=Root+"/Documentacion/Respaldo/EscenaNivel3_antes_ciudad_alta.unity.bak";
        if(!File.Exists(backup)) File.Copy(ScenePath,backup);
        const string editorBackup=Root+"/Documentacion/Respaldo/EscenaNivel3_antes_ciudad_alta_editor.unity";
        if(!File.Exists(editorBackup) && !EditorSceneManager.SaveScene(scene,editorBackup,true))
            throw new Exception("No se pudo respaldar el estado del editor.");
        var previous=scene.GetRootGameObjects().FirstOrDefault(o=>o.name==Name);
        if(previous!=null) Undo.DestroyObjectImmediate(previous);
        root=Group(Name,null);
        Undo.RegisterCreatedObjectUndo(root.gameObject,"Ampliar mapa Nivel 3");
        houses=Group("01_Casas_Modernas_12",root);
        greenery=Group("02_Jardines_y_Arbolado",root);
        furniture=Group("03_Alumbrado_y_Servicios",root);
        material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Generado/MapaReferencia_Sprites.mat");
        var grid=Group("04_Grid_Ampliacion",root);grid.gameObject.AddComponent<Grid>();
        var ground=Layer("Terreno",grid,0);
        var pavement=Layer("Banquetas",grid,1);
        var streets=Layer("Avenidas_Conectadas",grid,2);
        Fill(ground,"Pasto_04",80,0,131,59);
        Fill(pavement,"Banquetas_01",80,5,131,59);
        foreach(int x in new[]{80,104,128}) Fill(streets,"Asfalto_01",x,6,x+3,59);
        foreach(int y in new[]{6,25,44}) Fill(streets,"Asfalto_01",80,y,131,y+3);
        // Jardines interiores de manzana en lugar de grandes superficies vacías de concreto.
        foreach(int x0 in new[]{85,109}) foreach(int y0 in new[]{19,38,56})
        {
            int top=Mathf.Min(y0+5,59);
            for(int y=y0;y<=top;y++) for(int x=x0;x<=x0+17;x++)
                pavement.SetTile(new Vector3Int(x,y,0),null);
            if(y0<56) Fill(pavement,"Plaza_Piedra_02",x0,y0+3,x0+17,y0+3);
        }
        // Dos manzanas con tres filas de viviendas. Ocho fachadas diferentes.
        string[] models={"Casa_Ciudad_Balcon","Casa_Ciudad_Con_Garage","Casa_Ciudad_DosPisos",
            "Casa_Ciudad_Fachada_Cristal","Casa_Ciudad_Lamas_Madera","Casa_Ciudad_Residencial_Azul",
            "Casa_Ciudad_Techo_Lila","Casa_Ciudad_Techo_Teja"};
        int index=0;
        foreach(float y in new[]{11f,30f,49f}) foreach(float x in new[]{89f,99f,113f,123f})
        {
            var go=Sprite("Expansiones/CiudadAlta/Sprites/CasasModernas/"+models[index%models.Length],x,y,7.7f,houses);
            go.name="Vivienda_"+(index+1).ToString("00")+"_"+models[index%models.Length];
            var sr=go.GetComponent<SpriteRenderer>();
            var box=go.AddComponent<BoxCollider2D>();
            float scale=go.transform.localScale.x;
            box.size=new Vector2(sr.sprite.bounds.size.x*.85f,.6f/scale);
            box.offset=new Vector2(sr.sprite.bounds.center.x,sr.sprite.bounds.min.y+.3f/scale);
            // Entrada y jardín trasero; sin personajes ni lógica de vivienda.
            Fill(pavement,"Plaza_Piedra_02",(int)x-1,(int)y-1,(int)x+1,(int)y);
            foreach(float offset in new[]{-3.7f,3.7f})
                Base("Jardinera_Rosa",x+offset,y,1.2f,greenery);
            index++;
        }
        foreach(float y in new[]{21f,40f,56f}) foreach(float x in new[]{86f,94f,102f,110f,118f,126f})
        {
            Base(((int)x%3==0)?"Arbol_Rosa_Florido":"Arbol_Verde_Grande",x,y,2.8f,greenery);
            Base("Seto_Corto",x+2,y,1.6f,greenery);
        }
        foreach(float y in new[]{10f,29f,48f}) foreach(float x in new[]{84.5f,103f,108.5f,127f})
            Base("Farola_Moderna",x,y,1.1f,furniture);
        foreach(float y in new[]{23f,42f}) foreach(float x in new[]{90f,99f,114f,123f})
        {
            Base("Banco_Madera_01",x,y,2.1f,furniture);
            Base("Papelera_Reciclaje_Azul",x+2,y,.7f,furniture);
        }
        foreach(float x in new[]{87f,93f,99f,111f,117f,123f,129f})
        {
            Base("Valla_Metal_Piedra",x,1,5.5f,furniture);
            Base("Arbusto_Flores_Blancas",x,3,2.8f,greenery);
        }
        // Cruces y flechas en las avenidas; continúan las calles del centro.
        foreach(float x in new[]{81.5f,105.5f,129.5f}) foreach(float y in new[]{11f,30f,49f})
        {
            var go=Base("Marca_Paso_Peatonal",x,y,3.5f,furniture);
            go.GetComponent<SpriteRenderer>().sortingOrder=4;
        }
        foreach(float y in new[]{7f,26f,45f}) foreach(float x in new[]{91f,116f})
        {
            var go=Base("Marca_Flechas_Derecha",x,y,1.5f,furniture);
            go.GetComponent<SpriteRenderer>().sortingOrder=4;
        }
        foreach(float x in new[]{81.5f,105.5f,129.5f}) foreach(float y in new[]{18f,37f,55f})
        {
            var go=Base("Marca_Flechas_Arriba",x,y,1.5f,furniture);
            go.GetComponent<SpriteRenderer>().sortingOrder=4;
        }
        var dashed=Tile("Asfalto_06");
        foreach(int y in new[]{7,26,45}) for(int x=84;x<128;x++)
        {
            if(x>=102 && x<=109) continue;
            var cell=new Vector3Int(x,y,0);streets.SetTile(cell,dashed);
            streets.SetTileFlags(cell,TileFlags.None);
            streets.SetTransformMatrix(cell,Matrix4x4.Rotate(Quaternion.Euler(0,0,90)));
        }
        foreach(int x in new[]{81,105,129}) for(int y=12;y<60;y++)
            if(!(y>=23 && y<=30 || y>=42 && y<=49)) streets.SetTile(new Vector3Int(x,y,0),dashed);

        if(houses.childCount!=12 || root.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.sprite==null))
            throw new Exception("Validación de casas o sprites falló.");
        var camera=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Camera>(true)).FirstOrDefault();
        if(camera!=null && camera.orthographic)
        {
            Undo.RecordObject(camera.transform,"Encuadrar ciudad ampliada Nivel 3");
            Undo.RecordObject(camera,"Encuadrar ciudad ampliada Nivel 3");
            camera.transform.position=new Vector3(66,30,-10);
            camera.orthographicSize=40;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new Exception("No se guardó la escena.");
        Preview();
        Debug.Log("N3_CIUDAD_ALTA_OK: 12 casas modernas; mapa 132x60; centro conservado.");
    }
    static Transform Group(string name,Transform parent)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
    static TileBase Tile(string name)
    {
        var tile=AssetDatabase.LoadAssetAtPath<TileBase>(Root+"/Generado/Tiles/"+name+".asset");
        if(tile==null) throw new Exception("Falta tile "+name);return tile;
    }
    static Tilemap Layer(string name,Transform parent,int order)
    {
        var go=Group(name,parent).gameObject;var tm=go.AddComponent<Tilemap>();
        var sr=go.AddComponent<TilemapRenderer>();sr.sharedMaterial=material;sr.sortingOrder=order;return tm;
    }
    static void Fill(Tilemap tm,string name,int x0,int y0,int x1,int y1)
    {var tile=Tile(name);for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)tm.SetTile(new Vector3Int(x,y,0),tile);}
    static GameObject Base(string name,float x,float y,float width,Transform parent)
    {return Sprite("Sprites/Decoracion/"+name,x,y,width,parent);}
    static GameObject Sprite(string path,float x,float y,float width,Transform parent)
    {
        var sprite=AssetDatabase.LoadAllAssetsAtPath(Root+"/"+path+".png").OfType<Sprite>().FirstOrDefault();
        if(sprite==null) throw new Exception("Falta sprite "+path);
        var go=new GameObject(Path.GetFileName(path));go.transform.SetParent(parent,false);
        float scale=width/sprite.bounds.size.x;go.transform.localScale=Vector3.one*scale;
        go.transform.position=new Vector3(x-sprite.bounds.center.x*scale,y-sprite.bounds.min.y*scale,0);
        var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;
        sr.sortingOrder=1000-Mathf.RoundToInt(y*10);return go;
    }
    public static void Preview()
    {
        var go=new GameObject("N3_PREVIA_CIUDAD_TEMP");var cam=go.AddComponent<Camera>();
        cam.orthographic=true;cam.orthographicSize=31;
        go.transform.position=new Vector3(66,30,-100);cam.clearFlags=CameraClearFlags.SolidColor;
        cam.backgroundColor=new Color(.08f,.1f,.12f);
        var rt=new RenderTexture(2640,1240,24);cam.targetTexture=rt;cam.Render();
        var previous=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(2640,1240,TextureFormat.RGB24,false);
        texture.ReadPixels(new Rect(0,0,2640,1240),0,0);texture.Apply();
        File.WriteAllBytes(Root+"/Documentacion/Vista_Ciudad_Ampliada.png",texture.EncodeToPNG());
        RenderTexture.active=previous;cam.targetTexture=null;
        UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(go);
        SceneView.RepaintAll();
    }
}
#endif
