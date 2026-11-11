#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Constructor limitado a esta escena. No importa ni modifica recursos compartidos.
public static class TechnopolisNivel3MapaReferencia
{
    const string Root = "Assets/Technopolis/nivel3";
    const string ScenePath = "Assets/Scenes/EscenaNivel3Silicio.unity";
    const string MapName = "N3_MAPA_REFERENCIA";
    static Transform map, buildings, plants, props;
    static Material material;
    static int count;

    [MenuItem("Tools/Technopolis Nivel 3/4 - Construir mapa de referencia con casa")]
    public static void Construir()
    {
        // Nunca cambiar de escena automáticamente en una sesión interactiva.
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Salir de Play antes de construir el mapa.");
        if (Application.isBatchMode) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new InvalidOperationException("Abre exclusivamente " + ScenePath);
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string backup = Root + "/Documentacion/Respaldo/EscenaNivel3Silicio_antes_referencia.unity.bak";
        if (!File.Exists(backup)) File.Copy(ScenePath, backup);
        count = 0;
        // Estos grupos pertenecen al mapa anterior. Otros objetos quedan intactos.
        foreach (GameObject obj in scene.GetRootGameObjects())
            if (obj.name == "N3_MAPA_GENERADO" || obj.name == "Grid_Nivel3" || obj.name == MapName)
                UnityEngine.Object.DestroyImmediate(obj);

        string matPath = Root + "/Generado/MapaReferencia_Sprites.mat";
        material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, matPath);
        }
        map = Group(MapName, null);
        buildings = Group("03_Edificios_y_Casa_Alex", map);
        plants = Group("04_Arboles_y_Jardines", map);
        props = Group("05_Mobiliario_y_Senales", map);
        Transform grid = Group("00_Grid_Mapa_80x60", map);
        grid.gameObject.AddComponent<Grid>();
        Tilemap ground = Layer("00_Terreno", grid, 0);
        Tilemap sidewalk = Layer("01_Banquetas_y_Plazas", grid, 1);
        Tilemap roads = Layer("02_Calles", grid, 2);
        // Las variantes con grietas se reservan para detalles: evitar costuras excesivas.
        Rect(ground, "Pasto_04", 0, 0, 79, 59);
        Rect(sidewalk, "Banquetas_01", 0, 5, 79, 42);
        Rect(sidewalk, "Banquetas_01", 19, 0, 65, 59);
        Rect(sidewalk, "Banquetas_01", 0, 40, 18, 44);
        Rect(sidewalk, "Banquetas_01", 0, 43, 11, 58);
        Rect(sidewalk, "Banquetas_01", 64, 40, 79, 55);
        Rect(roads, "Asfalto_01", 0, 6, 79, 9);
        Rect(roads, "Asfalto_01", 21, 6, 24, 59);
        Rect(roads, "Asfalto_01", 59, 6, 62, 59);
        Rect(roads, "Asfalto_01", 0, 37, 79, 40);
        Rect(roads, "Asfalto_01", 0, 27, 24, 29);
        Rect(roads, "Asfalto_01", 24, 21, 59, 24);
        Rect(roads, "Asfalto_01", 62, 26, 79, 29);
        Rect(sidewalk, "Plaza_Piedra_02", 27, 25, 56, 36);
        Rect(ground, "Pasto_04", 65, 11, 78, 24);
        for(int y=11;y<=24;y++) for(int x=65;x<=78;x++) sidewalk.SetTile(new Vector3Int(x,y,0),null);
        Rect(sidewalk, "Tierra_Caminos_01", 66, 17, 77, 18);
        Rect(sidewalk, "Tierra_Caminos_01", 71, 12, 72, 23);

        // Edificios situados como en la imagen; todos conservan sprites editables.
        Building("Edificio_Cybernetics_LTD", 32, 41.5f, 12);
        Building("Edificio_Global_Corp", 52, 41.5f, 9.5f);
        Building("Edificio_Bitforce", 11, 10.5f, 11.5f);
        Building("Edificio_Byteforce_Corp", 34, 10.5f, 10);
        Building("Edificio_Silicon_Alley", 49.5f, 10.5f, 14);
        Building("Edificio_Conveniencia_24_7", 6.5f, 30.5f, 10);
        Building("Edificio_Servicio_Tecnico", 17, 30.5f, 6);
        Building("Edificio_Clinica_Distrito", 72, 41.5f, 12);
        Building("Edificio_Hardware_Electronica", 71.5f, 30.5f, 12);
        Building("Negocio_Fideos", 42, 53, 6);
        Building("Casa_Techo_Rojo", 16, 44, 6);
        Building("Casa_Techo_Azul", 68, 21, 5);
        Building("Casa_Techo_Gris", 75, 21, 5);
        Building("Edificio_Comunicaciones", 73, 54, 9);
        // Casa visible, acceso peatonal libre. No añadir scripts o personajes.
        Transform house = Group("Casa_Alex_Noroeste", buildings);
        Place("Edificios", "Casa_Principal_Fachada_Ciudad", 6.5f, 43, 8.5f, house, true);
        Rect(sidewalk, "Banquetas_01", 5, 41, 8, 43);

        Prop("Fuente_Globo_Holografico", 41, 28, 7);
        Prop("Parada_Autobus", 40.5f, 36, 8);
        Prop("Pantalla_Info", 30, 27, 2.4f);
        Prop("Tablon_Anuncios", 53, 28, 3);
        Prop("Senal_Direccional_Grande", 56, 34, 4);
        Prop("Banco_Madera_01", 33, 32, 3);
        Prop("Banco_Madera_02", 48.5f, 32, 3);
        Prop("Banco_Madera_01", 33, 26, 3);
        Prop("Banco_Madera_02", 48.5f, 26, 3);
        Prop("Papelera_Reciclaje_Azul", 28, 29, 1);
        Prop("Papelera_Gris", 55, 29, 1);
        Prop("Estatua_Robot", 74, 18.5f, 2.4f);
        Prop("Banco_Madera_01", 68, 13.5f, 3);
        Prop("Banco_Madera_02", 76, 13.5f, 3);
        Prop("Maquina_Expendedora", 1.5f, 28, 1.5f);
        Prop("Bicicletero", 18, 32, 2);
        Prop("Rotulo_Distrito_Silicio", 40, 1, 9);
        Prop("Bandera_Circuito_01", 33.5f, 1, 1.7f);
        Prop("Bandera_Circuito_02", 46.5f, 1, 1.7f);

        // Jardineras simétricas alrededor de la plaza y arbolado en cada manzana.
        foreach (float x in new[] {28.5f, 54.5f})
            foreach (float y in new[] {26f, 31f, 35f})
                Plant("Jardinera_Rosa", x, y, 2.2f);
        foreach (Vector2 p in new[] {
            new Vector2(2,54), new Vector2(15,54), new Vector2(18,49),
            new Vector2(2,39), new Vector2(19,41), new Vector2(26,53),
            new Vector2(40,44), new Vector2(57,52), new Vector2(65,54),
            new Vector2(65,31), new Vector2(78,33), new Vector2(1,19),
            new Vector2(19,18), new Vector2(27,13), new Vector2(56,12),
            new Vector2(66,22), new Vector2(78,21), new Vector2(65,12), new Vector2(78,11)})
            Plant("Arbol_Verde_Grande", p.x, p.y, 3.6f);
        foreach (Vector2 p in new[] {new Vector2(12,52),new Vector2(27,30),
            new Vector2(57,30),new Vector2(40,12),new Vector2(65,44),new Vector2(77,23)})
            Plant("Arbol_Rosa_Florido", p.x, p.y, 3.4f);
        foreach (float y in new[] {12f,16f,20f,24f})
        {
            Plant("Seto_Corto", 64.5f, y, 1.6f);
            Plant("Seto_Corto", 78.5f, y, 1.6f);
        }
        foreach (float x in new[] {66f,69f,72f,75f,78f})
            Plant("Jardinera_Amarilla", x, 11, 2);
        foreach (float x in new[] {2f,7f,12f,17f,27f,32f,51f,56f,66f,71f,76f})
        {
            Plant("Arbusto_Flores_Blancas", x, 3.2f, 2.8f);
            Prop("Valla_Metal_Piedra", x, 1, 4.8f);
        }
        foreach (float x in new[] {2f,19.5f,26f,57.5f,64f,78f})
            foreach (float y in new[] {11f,27f,41f})
                Prop("Farola_Clasica_01", x, y, 1.1f);
        foreach (float x in new[] {30f,52f})
            foreach (float y in new[] {25.5f,34f})
                Prop("Farola_Clasica_01", x, y, 1.1f);
        foreach (float x in new[] {29f,32f,50f,53f})
            Prop("Bolardo_Iluminado", x, 25, .6f);
        foreach(float y in new[]{43f,48f,53f,57f})
        {
            Plant("Arbol_Verde_Grande",12.5f,y,3);
            Plant("Arbusto_Rosa",19,y,1.5f);
            Plant("Arbol_Joven",64.5f,y,2);
            Plant("Jardinera_Amarilla",40,y,1.7f);
        }
        foreach(float x in new[]{29f,33f,37f,45f,49f,53f})
        {
            Plant("Arbusto_Flores_Blancas",x,35,1.6f);
            Plant("Jardinera_Rosa",x,25.2f,1.6f);
        }
        foreach(float y in new[]{13f,17f,21f})
        {
            Plant("Arbol_Verde_Grande",2.5f,y,3.3f);
            Plant("Jardinera_Amarilla",19,y,1.6f);
            Plant("Arbol_Joven",27,y,2.2f);
        }
        foreach(float x in new[]{66.5f,69.5f,73.5f,77f})
            Plant("Arbol_Joven",x,23.5f,2.2f);
        LaneLines(roads);
        Markings();

        Camera camera = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Camera>(true)).FirstOrDefault();
        if (camera != null)
        {
            camera.transform.position = new Vector3(40,30,-10);
            camera.orthographic = true;
            camera.orthographicSize = 32;
        }
        AssetDatabase.SaveAssets();
        Validate(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        RenderPreview();
        Debug.Log("N3_REFERENCIA_OK: " + count + " sprites; mapa 80x60; casa noroeste; escena guardada.");
    }

    static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
    static Tilemap Layer(string name, Transform parent, int order)
    {
        var t = Group(name, parent);
        var tm = t.gameObject.AddComponent<Tilemap>();
        var renderer = t.gameObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = order;
        renderer.sharedMaterial = material;
        return tm;
    }
    static void Rect(Tilemap tm, string name, int x0,int y0,int x1,int y1)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>(Root + "/Generado/Tiles/" + name + ".asset");
        if (tile == null) throw new Exception("Falta Tile " + name);
        for(int y=y0;y<=y1;y++) for(int x=x0;x<=x1;x++) tm.SetTile(new Vector3Int(x,y,0),tile);
    }
    static void Building(string name,float x,float y,float width) { Place("Edificios",name,x,y,width,buildings,true); }
    static void Prop(string name,float x,float y,float width) { Place("Decoracion",name,x,y,width,props,false); }
    static void Plant(string name,float x,float y,float width) { Place("Decoracion",name,x,y,width,plants,false); }
    static GameObject Place(string category,string name,float x,float y,float width,Transform parent,bool solid)
    {
        string path = Root + "/Sprites/" + category + "/" + name + ".png";
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if(sprite == null) throw new Exception("Falta sprite " + path);
        var go = new GameObject(name);
        go.transform.SetParent(parent,false);
        float scale = width/sprite.bounds.size.x;
        go.transform.localScale = Vector3.one*scale;
        // Ajustar a los límites reales incluso si el sprite tiene pivote central.
        go.transform.position = new Vector3(x-sprite.bounds.center.x*scale,y-sprite.bounds.min.y*scale,0);
        var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=sprite; sr.sharedMaterial=material;
        sr.sortingOrder=1000-Mathf.RoundToInt(y*10);
        if(solid)
        {
            var collider=go.AddComponent<BoxCollider2D>();
            collider.size=new Vector2(sprite.bounds.size.x*.85f,Mathf.Min(.65f/scale,sprite.bounds.size.y*.15f));
            collider.offset=new Vector2(sprite.bounds.center.x,sprite.bounds.min.y+collider.size.y/2);
        }
        count++;
        return go;
    }
    static void Markings()
    {
        // Marcas planas por debajo de edificios y mobiliario.
        foreach(float x in new[]{22f,60f}) foreach(float y in new[]{14f,31f,46f,55f})
        {
            var go=Place("Decoracion","Marca_Flechas_Arriba",x,y,1.5f,props,false);
            go.GetComponent<SpriteRenderer>().sortingOrder=4;
        }
        foreach(float y in new[]{7f,38f}) foreach(float x in new[]{8f,33f,47f,71f})
        {
            var go=Place("Decoracion","Marca_Flechas_Derecha",x,y,1.6f,props,false);
            go.GetComponent<SpriteRenderer>().sortingOrder=4;
        }
        foreach(float x in new[]{22.5f,60.5f}) foreach(float y in new[]{11f,34f,42f})
        {
            var go=Place("Decoracion","Marca_Paso_Peatonal",x,y,3.6f,props,false);
            go.GetComponent<SpriteRenderer>().sortingOrder=4;
        }
    }
    static void LaneLines(Tilemap roads)
    {
        TileBase dashed=AssetDatabase.LoadAssetAtPath<TileBase>(Root+"/Generado/Tiles/Asfalto_06.asset");
        foreach(int x in new[]{22,60}) for(int y=10;y<60;y++)
        {
            if(y>=35 && y<=42 || y>=20 && y<=26) continue;
            roads.SetTile(new Vector3Int(x,y,0),dashed);
        }
        foreach(int y in new[]{7,38}) for(int x=0;x<80;x++)
        {
            if(x>=19 && x<=26 || x>=57 && x<=64) continue;
            var cell=new Vector3Int(x,y,0);roads.SetTile(cell,dashed);
            roads.SetTileFlags(cell,TileFlags.None);
            roads.SetTransformMatrix(cell,Matrix4x4.Rotate(Quaternion.Euler(0,0,90)));
        }
    }
    static void Validate(Scene scene)
    {
        if(scene.path!=ScenePath) throw new Exception("Escena equivocada");
        if(map.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.sprite==null)) throw new Exception("Sprite nulo");
        if(map.Find("03_Edificios_y_Casa_Alex/Casa_Alex_Noroeste/Casa_Principal_Fachada_Ciudad")==null)
            throw new Exception("Falta casa de Alex");
        if(map.GetComponentsInChildren<Tilemap>().Length!=3) throw new Exception("Faltan Tilemaps");
    }
    public static void RenderPreview()
    {
        var go=new GameObject("N3_CAMARA_PREVIA_TEMPORAL");
        var cam=go.AddComponent<Camera>();
        cam.orthographic=true; cam.orthographicSize=31;
        go.transform.position=new Vector3(40,30,-100);
        cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.08f,.10f,.12f);
        var target=new RenderTexture(1600,1200,24);
        cam.targetTexture=target;cam.Render();
        RenderTexture previous=RenderTexture.active;RenderTexture.active=target;
        var tex=new Texture2D(1600,1200,TextureFormat.RGB24,false);
        tex.ReadPixels(new Rect(0,0,1600,1200),0,0);tex.Apply();
        File.WriteAllBytes(Root+"/Documentacion/Vista_Mapa_Referencia.png",tex.EncodeToPNG());
        RenderTexture.active=previous;cam.targetTexture=null;
        UnityEngine.Object.DestroyImmediate(tex);target.Release();UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(go);
    }
}
#endif
