#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>Rebuilds the neighborhood from one shared, reference-traced layout.</summary>
public static class TechnopolisReconstruirReferenciaNivel1
{
    const string Root = "Assets/Technopolis/Nivel1/";
    const string ScenePath = "Assets/Scenes/EscenaNivel1.unity";
    const string Generated = "Barrio_Referencia_08";
    const float Left = -40f, Top = 20f;
    [Serializable] public class Plan { public int version; public Item[] placements; public Region[] regions; public Lane[] lanes; }
    [Serializable] public class Item
    {
        public string name, sprite, kind;
        public float c, r, width, height, angle;
        public bool solid;
        public int left, top, right, bottom;
    }
    [Serializable] public class Region { public string material; public int c, r, w, h; }
    [Serializable] public class Lane { public float c1, r1, c2, r2, width; }

    [MenuItem("Tools/Technopolis/Nivel 1/08 Reconstruir diseño de la referencia")]
    public static void Reconstruir()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorUtility.DisplayDialog("Sal de Play", "Detén el juego antes de reconstruir el barrio.", "Aceptar"); return; }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath && scene.path != "Assets/Scenes/EscenaNivel1_Referencia.unity")
        { EditorUtility.DisplayDialog("Abre Nivel 1", "Abre " + ScenePath, "Aceptar"); return; }
        Transform grid = Find(scene, "Grid"), player = Find(scene, "Jugador");
        if (!grid || !player || !grid.GetComponent<Grid>())
        { EditorUtility.DisplayDialog("Falta el mapa", "Se necesitan Grid y Jugador.", "Aceptar"); return; }
        if (grid.position != Vector3.zero || grid.rotation != Quaternion.identity || grid.lossyScale != Vector3.one ||
            !Mathf.Approximately(grid.GetComponent<Grid>().cellSize.x,1f) || !Mathf.Approximately(grid.GetComponent<Grid>().cellSize.y,1f))
        { EditorUtility.DisplayDialog("Revisa Grid", "Esta distribución utiliza Grid en (0,0,0), escala 1 y casillas de 1 unidad.", "Aceptar"); return; }
        var text = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "Editor/PlanoReferenciaNivel1.json");
        Plan plan = text ? JsonUtility.FromJson<Plan>(text.text) : null;
        if (plan == null || plan.version != 1 || plan.placements == null || plan.regions == null || plan.lanes == null)
        { EditorUtility.DisplayDialog("Falta el plano", "No se pudo leer PlanoReferenciaNivel1.json.", "Aceptar"); return; }
        var sprites = new Dictionary<string, Sprite>();
        var patterns = new Dictionary<string, TileBase[]>();
        // Complete preflight before touching scene content.
        foreach (Item p in plan.placements)
        {
            if (p.width <= 0 || p.right <= p.left || p.bottom <= p.top) throw new InvalidOperationException("Geometría inválida: " + p.name);
            if (sprites.ContainsKey(p.sprite)) continue;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + p.sprite);
            if (!sprite) { EditorUtility.DisplayDialog("Falta un sprite", Root + p.sprite, "Aceptar"); return; }
            sprites.Add(p.sprite, sprite);
        }
        foreach (string kind in new[] { "tierra", "pasto", "asfalto", "grava", "concreto", "empedrado" })
        {
            var tiles = new TileBase[16];
            string folder = kind == "concreto" || kind == "empedrado" ? "02_Caminos" : "01_Terreno";
            for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++)
            {
                string path = Root + "Tiles/" + folder + "/" + kind + "_bloque_f" + r + "_c" + c + ".asset";
                tiles[r * 4 + c] = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (!tiles[r * 4 + c]) { EditorUtility.DisplayDialog("Falta un tile", path, "Aceptar"); return; }
            }
            patterns.Add(kind, tiles);
        }
        if (!EditorSceneManager.SaveScene(scene)) return;
        const string backups = "Assets/Scenes/Respaldos";
        if (!AssetDatabase.IsValidFolder(backups)) AssetDatabase.CreateFolder("Assets/Scenes", "Respaldos");
        string backup = backups + "/EscenaNivel1_AntesReferencia08_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity";
        if (!AssetDatabase.CopyAsset(scene.path, backup))
        { EditorUtility.DisplayDialog("No se pudo respaldar", "No se modificó la escena.", "Aceptar"); return; }

        Undo.IncrementCurrentGroup(); int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Reconstruir barrio desde referencia");
        try
        {
            Transform previous = grid.Find(Generated);
            if (previous) Undo.DestroyObjectImmediate(previous.gameObject);
            // Original content remains in the scene, inactive, plus a full scene backup.
            foreach (Transform child in grid)
            {
                string n = child.name;
                if (n == "suelo_base" || n == "Caminos" || n == "Edificios" || n == "Objetos" ||
                    n == "Detalles" || n == "Limites_Nivel1" || n.StartsWith("Ambientacion_", StringComparison.Ordinal))
                { Undo.RecordObject(child.gameObject, "Archivar distribución anterior"); child.gameObject.SetActive(false); }
            }
            Transform root = NewGroup(grid, Generated);
            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            int layer = playerRenderer ? playerRenderer.sortingLayerID : 0;
            Tilemap terrain = NewMap(root, "Suelo_Referencia", -100, layer);
            Tilemap roads = NewMap(root, "Senderos_Referencia", -90, layer);
            roads.color = new Color(1.20f, 1.13f, 1.02f, 1f);
            var ground = new TileBase[4096]; var paths = new TileBase[4096];
            for (int row = 0; row < 64; row++) for (int col = 0; col < 64; col++)
            {
                string material = "tierra";
                foreach (Region z in plan.regions)
                    if (col >= z.c && col < z.c + z.w && row >= z.r && row < z.r + z.h) material = z.material;
                if (material == "tierra" && Garden(plan.placements,col+.5f,row+.5f) &&
                    !OnLane(plan.lanes,col+.5f,row+.5f) && (col*73+row*29)%10<7) material="pasto";
                int index = (63 - row) * 64 + col;
                ground[index] = patterns[material][(row % 4) * 4 + col % 4];
                if (material == "tierra" && OnLane(plan.lanes, col + .5f, row + .5f))
                    paths[index] = patterns["tierra"][(row % 4) * 4 + col % 4];
            }
            var bounds = new BoundsInt(-40, -44, 0, 64, 64, 1);
            terrain.SetTilesBlock(bounds, ground); roads.SetTilesBlock(bounds, paths);
            Transform buildings = NewGroup(root, "Edificios_Referencia"), objects = NewGroup(root, "Objetos_Referencia");
            foreach (Item p in plan.placements)
                Place(p, sprites[p.sprite], p.kind == "building" ? buildings : objects, layer);
            Perimeter(root, layer);
            Transform limits = NewGroup(root, "Limites_Referencia");
            Wall(limits, "Oeste", -40.2f, -12, .4f, 64);
            Wall(limits, "Este", 24.2f, -12, .4f, 64);
            Wall(limits, "Norte", -8, 20.2f, 64, .4f);
            Wall(limits, "Sur", -8, -44.2f, 64, .4f);

            Undo.RecordObject(player, "Inicio en patio de Alex");
            player.position = new Vector3(Left + 19.3f, Top - 34.4f, player.position.z);
            CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
            if (capsule)
            {
                Undo.RecordObject(capsule, "Conservar colisión pequeña del jugador");
                capsule.size = new Vector2(.72f / Mathf.Abs(player.lossyScale.x), .9f / Mathf.Abs(player.lossyScale.y));
                capsule.offset = new Vector2(0, .45f / Mathf.Abs(player.lossyScale.y));
                capsule.isTrigger = false;
            }
            var depth = Undo.AddComponent<TechnopolisOrdenBarrio>(root.gameObject);
            depth.jugador = playerRenderer;
            if(playerRenderer)Undo.RecordObject(playerRenderer,"Orden del jugador");
            depth.Actualizar();
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Barrio reconstruido",
                plan.placements.Length + " elementos colocados con el plano de referencia.\n" +
                "La distribución anterior quedó inactiva. No vuelvas a ejecutar 02–07 sobre esta versión.\n" +
                "Prueba los pasos en Play y comprueba el mapa en Scene.\n" +
                (saved ? "Escena guardada." : "Guarda con Ctrl+S.") + "\nRespaldo: " + backup, "Aceptar");
        }
        catch (Exception ex)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Reconstrucción cancelada", "Se revirtieron los cambios. Respaldo: " + backup, "Aceptar");
        }
    }

    static bool OnLane(Lane[] lanes, float c, float r)
    {
        foreach (Lane l in lanes)
            if (c >= Mathf.Min(l.c1,l.c2)-l.width/2 && c <= Mathf.Max(l.c1,l.c2)+l.width/2 &&
                r >= Mathf.Min(l.r1,l.r2)-l.width/2 && r <= Mathf.Max(l.r1,l.r2)+l.width/2) return true;
        return false;
    }
    static bool Garden(Item[] items,float c,float r)
    {
        foreach(Item p in items)
        {
            if(p.kind!="building")continue;
            float h=p.height>0?p.height:p.width*(p.bottom-p.top)/(p.right-p.left);
            float l=p.c-p.width/2,rr=p.c+p.width/2,t=p.r-h;
            bool near=c>l-1.2f&&c<rr+1.2f&&r>t-.4f&&r<p.r+1.4f;
            bool inside=c>l+.65f&&c<rr-.65f&&r>t+.65f&&r<p.r-.25f;
            if(near&&!inside)return true;
        }
        return c<1.7f||c>62.3f||r<1.5f||r>62.5f;
    }
    static Transform NewGroup(Transform parent, string name)
    {
        var go = new GameObject(name); Undo.RegisterCreatedObjectUndo(go, "Crear " + name);
        Undo.SetTransformParent(go.transform, parent, "Agrupar " + name);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }
    static Tilemap NewMap(Transform parent, string name, int order, int layer)
    {
        Transform go = NewGroup(parent, name);
        Tilemap map = Undo.AddComponent<Tilemap>(go.gameObject);
        TilemapRenderer renderer = Undo.AddComponent<TilemapRenderer>(go.gameObject);
        renderer.sortingOrder = order; renderer.sortingLayerID = layer;
        return map;
    }
    static void Place(Item p, Sprite sprite, Transform parent, int layer)
    {
        Transform anchor = NewGroup(parent, p.name);
        anchor.position = new Vector3(Left + p.c, Top - p.r, 0);
        Transform visual = NewGroup(anchor, "Sprite");
        float ppu = sprite.pixelsPerUnit;
        float sx = p.width * ppu / (p.right-p.left);
        float sy = p.height > 0 ? p.height * ppu / (p.bottom-p.top) : sx;
        Quaternion q = Quaternion.Euler(0,0,p.angle);
        Vector2 pivot = sprite.pivot;
        // Visible alpha bounds, rather than PNG canvas bounds, determine placement.
        Vector3[] corners = {
            new Vector3((p.left-pivot.x)/ppu*sx,(sprite.rect.height-p.bottom-pivot.y)/ppu*sy),
            new Vector3((p.right-pivot.x)/ppu*sx,(sprite.rect.height-p.bottom-pivot.y)/ppu*sy),
            new Vector3((p.left-pivot.x)/ppu*sx,(sprite.rect.height-p.top-pivot.y)/ppu*sy),
            new Vector3((p.right-pivot.x)/ppu*sx,(sprite.rect.height-p.top-pivot.y)/ppu*sy)
        };
        float minX=float.MaxValue,maxX=float.MinValue,minY=float.MaxValue,maxY=float.MinValue;
        foreach(Vector3 v in corners) { Vector3 a=q*v; minX=Mathf.Min(minX,a.x); maxX=Mathf.Max(maxX,a.x); minY=Mathf.Min(minY,a.y); maxY=Mathf.Max(maxY,a.y); }
        visual.localPosition = new Vector3(-(minX+maxX)/2,-minY,0);
        visual.localScale = new Vector3(sx,sy,1); visual.localRotation=q;
        SpriteRenderer sr=Undo.AddComponent<SpriteRenderer>(visual.gameObject);
        sr.sprite=sprite;sr.sortingLayerID=layer;sr.sortingOrder=p.kind=="floor"?-80:TechnopolisOrdenBarrio.Orden(anchor.position.y);
        sr.spriteSortPoint=SpriteSortPoint.Pivot;
        if (!p.solid) return;
        float w=maxX-minX,h=maxY-minY;
        if(p.kind=="building") { Box(anchor,w*.86f,h*.64f,h*.32f+.1f); return; }
        if(p.name.StartsWith("Arbol_",StringComparison.Ordinal))
        { var circle=Undo.AddComponent<CircleCollider2D>(anchor.gameObject);circle.radius=.85f;circle.offset=new Vector2(0,.8f);return; }
        if(p.name=="Columpios")
        {
            for(int side=-1;side<=1;side+=2)
            { var box=Undo.AddComponent<BoxCollider2D>(anchor.gameObject);box.size=new Vector2(.25f,.35f);box.offset=new Vector2(side*w*.42f,.18f); }
            return;
        }
        float fh=p.name=="Fuente_Parque"?1.35f:p.name=="Auto_Taller"?h*.7f:.45f;
        Box(anchor,w*.78f,fh,fh/2);
    }
    static void Box(Transform t,float w,float h,float y)
    {var b=Undo.AddComponent<BoxCollider2D>(t.gameObject);b.size=new Vector2(w,h);b.offset=new Vector2(0,y);}
    static void Wall(Transform parent,string name,float x,float y,float w,float h)
    {Transform t=NewGroup(parent,name);t.position=new Vector3(x,y);Box(t,w,h,0);}
    static void Perimeter(Transform parent,int layer)
    {
        Sprite s=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Sprites/04_Objetos/muro_perimetral.png");
        if(!s)throw new InvalidOperationException("Falta muro_perimetral.png");
        // Put the outer visible edge exactly on the 64x64 terrain edge.
        for(int i=0;i<8;i++)
        {
            var p=new Item{sprite="",name="Muro_Norte_"+i,c=4+i*8,r=.95f,width=8,height=.95f,left=0,top=11,right=128,bottom=37};Place(p,s,parent,layer);
            p.name="Muro_Sur_"+i;p.r=64;Place(p,s,parent,layer);
            p.name="Muro_Oeste_"+i;p.c=.475f;p.r=8+i*8;p.angle=90;Place(p,s,parent,layer);
            p.name="Muro_Este_"+i;p.c=63.525f;Place(p,s,parent,layer);
        }
    }
    static Transform Find(Scene scene,string name)
    {foreach(GameObject root in scene.GetRootGameObjects())foreach(Transform t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;return null;}
}
#endif
