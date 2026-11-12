using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MapaBarrioExpandido : MonoBehaviour
{
    sealed class Punto { public Transform destino; public Vector2 pos; public string id; public RectTransform etiqueta; }
    readonly List<Punto> puntos=new List<Punto>();
    Transform jugador;
    Action<Vector2?> cambiado;
    Canvas canvas;
    Camera camara;
    RenderTexture textura;
    RectTransform superficie;
    TMP_Text informacion,seleccion;
    Vector2? marcador;
    float tiempoAnterior;
    string clave;
    static MapaBarrioExpandido abierto;
    public static bool Abierto=>abierto;
    public static float ReanudarEn {get;private set;}
    public bool PermitirAbrir=true;
    static readonly Color Cian=new Color32(76,202,198,255);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset(){abierto=null;ReanudarEn=0;}

    RectTransform Rect(string nombre,Transform parent,Vector2 pos,Vector2 size)
    {
        var r=new GameObject(nombre,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    TMP_Text Texto(string contenido,Transform parent,Vector2 pos,Vector2 size,int tam=25)
    {
        var r=Rect(contenido,parent,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.text=contenido;t.fontSize=tam;t.color=new Color32(215,239,233,255);t.raycastTarget=false;var f=Resources.Load<TMP_FontAsset>("FuenteTerminal");if(f)t.font=f;return t;
    }
    void Boton(string text,Transform parent,Vector2 pos,Action action)
    {
        var r=Rect(text,parent,pos,new Vector2(560,60));var i=r.gameObject.AddComponent<Image>();i.color=new Color32(24,55,65,255);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=i;b.onClick.AddListener(()=>action());Texto(text,r,new Vector2(15,-13),new Vector2(530,40));
    }
    public void Configurar(Transform player,Scene scene,Action<Vector2?> onChange)
    {
        jugador=player;cambiado=onChange;clave="Technopolis.Waypoint."+scene.name;
        SceneManager.MoveGameObjectToScene(gameObject,scene);
        canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1800;
        var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;gameObject.AddComponent<GraphicRaycaster>();
        var dim=gameObject.AddComponent<Image>();dim.color=new Color32(3,8,15,248);
        var panel=Rect("Terminal cartográfica",transform,Vector2.zero,new Vector2(1540,960));panel.anchorMin=panel.anchorMax=panel.pivot=new Vector2(.5f,.5f);panel.gameObject.AddComponent<Image>().color=new Color32(12,21,31,255);
        Texto("// MAPA DEL DISTRITO",panel,new Vector2(30,-18),new Vector2(1400,45),32);
        superficie=Rect("Mapa interactivo",panel,new Vector2(30,-85),new Vector2(800,800));var raw=superficie.gameObject.AddComponent<RawImage>();raw.color=Color.white;
        textura=new RenderTexture(1024,1024,16);textura.filterMode=FilterMode.Point;raw.texture=textura;
        superficie.gameObject.AddComponent<RectMask2D>();superficie.gameObject.AddComponent<MapaPulsacion>().Inicializar(this);
        var obj=new GameObject("Cámara cartográfica");SceneManager.MoveGameObjectToScene(obj,scene);camara=obj.AddComponent<Camera>();camara.orthographic=true;camara.targetTexture=textura;camara.cullingMask=Camera.main?Camera.main.cullingMask&~(1<<5):~(1<<5);camara.clearFlags=CameraClearFlags.SolidColor;camara.backgroundColor=new Color(.03f,.06f,.08f);camara.allowHDR=false;
        Bounds bounds=new Bounds(jugador.position,Vector3.one*10);var terreno=GameObject.Find("Terreno_Organico");
        if(terreno&&terreno.GetComponentInChildren<SpriteRenderer>())bounds=terreno.GetComponentInChildren<SpriteRenderer>().bounds;
        else{var grid=GameObject.Find("Grid");if(grid)foreach(var r in grid.GetComponentsInChildren<Renderer>())if(r.bounds.size.x>0)bounds.Encapsulate(r.bounds);}
        camara.transform.position=new Vector3(bounds.center.x,bounds.center.y,-10);camara.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.04f;
        Texto("> COLOCA UN DESTINO",panel,new Vector2(875,-95),new Vector2(600,45),29);
        Texto("Haz clic o toca cualquier punto del mapa.\n\nEl marcador aparecerá en el barrio y en el minimapa.\n\nLa distancia se actualiza al caminar.\n1 unidad del mundo = 1 metro.\nDistancia en línea recta.",panel,new Vector2(875,-165),new Vector2(600,275),25);
        informacion=Texto("Sin marcador personal",panel,new Vector2(875,-470),new Vector2(600,90),28);informacion.color=Cian;
        Boton("Quitar marcador",panel,new Vector2(875,-650),Quitar);
        Boton("Volver al juego (M / Escape)",panel,new Vector2(875,-740),Cerrar);
        Texto("▲ Alex   ! misión   ● edificio   X destino",panel,new Vector2(30,-905),new Vector2(1450,35),22);
        seleccion=Texto("X",superficie,Vector2.zero,new Vector2(40,40),36);seleccion.color=Cian;seleccion.rectTransform.pivot=new Vector2(.5f,.5f);seleccion.gameObject.SetActive(false);
        canvas.enabled=false;camara.enabled=false;
        if(PlayerPrefs.GetInt(clave+".Set",0)==1){marcador=new Vector2(PlayerPrefs.GetFloat(clave+".X"),PlayerPrefs.GetFloat(clave+".Y"));cambiado(marcador);}
    }
    public void Agregar(Transform target,Vector2 point,string label,Color color,string id)
    {
        var t=Texto(label,superficie,Vector2.zero,new Vector2(label.Length>2?130:30,30),label.Length>2?18:26);t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.pivot=new Vector2(.5f,.5f);
        puntos.Add(new Punto{destino=target,pos=point,id=id,etiqueta=t.rectTransform});seleccion.transform.SetAsLastSibling();
    }
    public void Abrir()
    {
        if(!PermitirAbrir||Abierto||TechnopolisGameFlow.IsMenuOpen)return;
        abierto=this;tiempoAnterior=Time.timeScale;Time.timeScale=0;canvas.enabled=true;camara.enabled=true;
    }
    public void Cerrar()
    {
        if(abierto!=this)return;abierto=null;ReanudarEn=Time.unscaledTime+.25f;canvas.enabled=false;camara.enabled=false;Time.timeScale=tiempoAnterior;
    }
    public void Pulsar(Vector2 pantalla,Camera eventCamera)
    {
        if(!Abierto||!RectTransformUtility.ScreenPointToLocalPointInRectangle(superficie,pantalla,eventCamera,out var local))return;
        float u=local.x/superficie.rect.width,v=1+local.y/superficie.rect.height;
        if(u<0||u>1||v<0||v>1)return;
        Vector3 point=camara.ViewportToWorldPoint(new Vector3(u,v,10));ColocarMarcador(point);
    }
    public void ColocarMarcador(Vector2 point)
    {
        marcador=point;PlayerPrefs.SetInt(clave+".Set",1);PlayerPrefs.SetFloat(clave+".X",point.x);PlayerPrefs.SetFloat(clave+".Y",point.y);PlayerPrefs.Save();cambiado(marcador);
    }
    public void Quitar(){marcador=null;PlayerPrefs.DeleteKey(clave+".Set");PlayerPrefs.DeleteKey(clave+".X");PlayerPrefs.DeleteKey(clave+".Y");PlayerPrefs.Save();cambiado(null);}
    void Posicionar(RectTransform r,Vector2 world){Vector3 v=camara.WorldToViewportPoint(world);r.anchorMin=r.anchorMax=Vector2.zero;r.anchoredPosition=new Vector2(v.x*800,v.y*800);}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){if(Abierto)Cerrar();else Abrir();}
        if(abierto!=this)return;
        if(Input.GetKeyDown(KeyCode.Escape)){Cerrar();return;}
        foreach(var p in puntos){p.etiqueta.gameObject.SetActive(p.id==""||!ProgresoTechnopolis.Completada(p.id));Posicionar(p.etiqueta,p.destino?(Vector2)p.destino.position:p.pos);}
        seleccion.gameObject.SetActive(marcador.HasValue);
        if(marcador.HasValue){Posicionar(seleccion.rectTransform,marcador.Value);informacion.text=$"DESTINO PERSONAL\n{Mathf.CeilToInt(Vector2.Distance(jugador.position,marcador.Value))} metros";}else informacion.text="Sin marcador personal";
    }
    void OnDestroy(){if(abierto==this){abierto=null;Time.timeScale=tiempoAnterior;}if(camara)Destroy(camara.gameObject);if(textura){textura.Release();Destroy(textura);}}
}
public sealed class MapaPulsacion : MonoBehaviour,IPointerClickHandler
{
    MapaBarrioExpandido mapa;
    public void Inicializar(MapaBarrioExpandido owner){mapa=owner;}
    public void OnPointerClick(PointerEventData e){mapa.Pulsar(e.position,e.pressEventCamera);}
}
