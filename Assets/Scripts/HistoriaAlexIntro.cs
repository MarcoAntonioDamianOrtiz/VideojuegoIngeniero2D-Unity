using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Prólogo ilustrado; se abre únicamente al elegir Nueva partida.</summary>
public sealed class HistoriaAlexIntro : MonoBehaviour
{
    static HistoriaAlexIntro instancia;
    public static bool IsPlaying => instancia;
    public static float ReanudarEn { get; private set; }
    static readonly string[] Titulos = { "Una ciudad, una oportunidad", "Todo lo que tenía", "Una promesa a Bit", "El primer encargo" };
    static readonly string[] Relato = {
        "Alex Null acababa de terminar sus estudios de programación. Llegó a Technopolis con una mochila, muchas ideas y la esperanza de encontrar su primera oportunidad.",
        "Su computadora era antigua y sus ahorros apenas alcanzaban para empezar. Entre buscar trabajo y pagar lo necesario, Alex tenía que aprender a salir adelante un día a la vez.",
        "Pero Alex no había llegado solo. Bit, su compañero de cuatro patas, confiaba en él. «Vamos a estar bien, Bit. Primero conseguiremos trabajo… y algún día levantaremos NullForce.»",
        "El olor a pan recién hecho lo llevó hasta el negocio de Doña Ramona. Su terminal de inventario había dejado de funcionar. Para Alex, aquel pequeño problema podía ser el comienzo de algo grande."
    };
    HistoriaAlexRecursos recursos;
    Action terminar;
    RawImage ilustracion;
    UnityEngine.UI.Image alex, bit, ramona;
    TMP_Text titulo, narracion, pagina, siguienteTexto;
    UnityEngine.UI.Button atras;
    CanvasGroup entrada;
    int indice;
    float inicioPagina;
    bool textoCompleto, cerrado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReiniciarEstado() { instancia = null; ReanudarEn = 0; }

    public static void Mostrar(Action alTerminar)
    {
        if (instancia) return;
        var go = new GameObject("Prólogo · La llegada de Alex");
        instancia = go.AddComponent<HistoriaAlexIntro>();
        instancia.terminar = alTerminar;
        instancia.recursos = Resources.Load<HistoriaAlexRecursos>("HistoriaAlex");
        instancia.Construir();
        Time.timeScale = 0;
        instancia.MostrarPagina(0);
    }

    RectTransform Rectangulo(string nombre, Transform padre, Vector2 pos, Vector2 size)
    {
        var r = new GameObject(nombre, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(padre, false); r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1); r.anchoredPosition = pos; r.sizeDelta = size;
        return r;
    }
    TMP_Text Texto(string nombre, Transform padre, Vector2 pos, Vector2 size, int tamano)
    {
        var r=Rectangulo(nombre,padre,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();
        t.fontSize=tamano;t.color=new Color32(249,230,190,255);t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
    }
    UnityEngine.UI.Button Boton(string nombre,Transform padre,Vector2 pos,Vector2 size,Action accion)
    {
        var r=Rectangulo(nombre,padre,pos,size);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new Color32(91,63,41,255);
        var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;
        var c=b.colors;c.highlightedColor=new Color(1.25f,1.2f,1.1f);c.pressedColor=new Color(.7f,.8f,.8f);b.colors=c;
        var t=Texto("Etiqueta",r,new Vector2(8,-4),size-new Vector2(16,8),25);t.text=nombre;t.alignment=TextAlignmentOptions.Midline;
        b.onClick.AddListener(()=>accion());return b;
    }
    UnityEngine.UI.Image Actor(string nombre,Transform padre,Vector2 pos,Vector2 size,Sprite sprite)
    {
        var r=Rectangulo(nombre,padre,pos,size);var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;
    }
    void Construir()
    {
        var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=2000;
        var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        gameObject.AddComponent<GraphicRaycaster>();
        var black=gameObject.AddComponent<UnityEngine.UI.Image>();black.color=Color.black;black.raycastTarget=true;
        var area=Rectangulo("Composición 16 por 9",transform,Vector2.zero,new Vector2(1920,1080));area.anchorMin=area.anchorMax=area.pivot=new Vector2(.5f,.5f);
        entrada=area.gameObject.AddComponent<CanvasGroup>();
        ilustracion=Rectangulo("Ilustración",area,Vector2.zero,new Vector2(1920,1080)).gameObject.AddComponent<RawImage>();ilustracion.raycastTarget=true;
        var tocar=ilustracion.gameObject.AddComponent<UnityEngine.UI.Button>();tocar.targetGraphic=ilustracion;tocar.transition=Selectable.Transition.None;tocar.onClick.AddListener(Avanzar);
        alex=Actor("Alex original",area,new Vector2(390,-360),new Vector2(380,400),recursos?recursos.alex:null);
        bit=Actor("Bit original",area,new Vector2(760,-430),new Vector2(230,325),recursos?recursos.bit:null);
        ramona=Actor("Doña Ramona original",area,new Vector2(1120,-340),new Vector2(230,420),recursos?recursos.ramona:null);
        var badge=Rectangulo("Capítulo",area,new Vector2(40,-34),new Vector2(650,62));badge.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color32(24,22,28,235);
        Texto("Nombre del juego",badge,new Vector2(18,-13),new Vector2(620,40),25).text="TECHNOPOLIS · EL COMIENZO";
        var panel=Rectangulo("Narración y controles",area,new Vector2(40,-780),new Vector2(1840,270));panel.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color32(24,22,28,247);
        var linea=Rectangulo("Borde bronce",panel,Vector2.zero,new Vector2(1840,4));linea.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color32(205,164,95,255);
        titulo=Texto("Título de viñeta",panel,new Vector2(30,-18),new Vector2(1680,48),34);titulo.fontStyle=FontStyles.Bold;
        narracion=Texto("Relato",panel,new Vector2(30,-76),new Vector2(1780,110),31);
        atras=Boton("Anterior",panel,new Vector2(30,-200),new Vector2(205,52),()=>MostrarPagina(indice-1));
        pagina=Texto("Progreso",panel,new Vector2(270,-212),new Vector2(500,35),23);
        Boton("Omitir historia",panel,new Vector2(1170,-200),new Vector2(260,52),Finalizar);
        var next=Boton("Siguiente",panel,new Vector2(1460,-200),new Vector2(350,52),Avanzar);siguienteTexto=next.GetComponentInChildren<TMP_Text>();
    }
    void MostrarPagina(int numero)
    {
        indice=Mathf.Clamp(numero,0,Relato.Length-1);inicioPagina=Time.unscaledTime;textoCompleto=false;
        titulo.text=Titulos[indice];narracion.text=Relato[indice];narracion.maxVisibleCharacters=0;
        pagina.text=$"{indice+1} / {Relato.Length}   ·   ESPACIO / TOCA PARA AVANZAR";
        siguienteTexto.text=indice==Relato.Length-1?"Comenzar aventura":"Siguiente";atras.interactable=indice>0;
        if(recursos)ilustracion.texture=indice==1?recursos.cuarto:indice==2?recursos.parque:indice==3?recursos.panaderia:recursos.tejados;
        alex.gameObject.SetActive(recursos&&recursos.alex&&(indice==0||indice==2));bit.gameObject.SetActive(recursos&&recursos.bit&&(indice==0||indice==2));ramona.gameObject.SetActive(recursos&&recursos.ramona&&indice==3);
        entrada.alpha=0;
    }
    public void Avanzar()
    {
        if(cerrado||Time.unscaledTime-inicioPagina<.2f)return;
        if(!textoCompleto){textoCompleto=true;narracion.maxVisibleCharacters=int.MaxValue;return;}
        if(indice==Relato.Length-1)Finalizar();else MostrarPagina(indice+1);
    }
    void Update()
    {
        float edad=Time.unscaledTime-inicioPagina;entrada.alpha=Mathf.Clamp01(edad/.4f);
        if(!textoCompleto){narracion.maxVisibleCharacters=Mathf.FloorToInt(edad*48);if(narracion.maxVisibleCharacters>=Relato[indice].Length)textoCompleto=true;}
        bool avanzar=Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.RightArrow);
        bool omitir=Input.GetKeyDown(KeyCode.Escape);
        if(Input.GetKeyDown(KeyCode.LeftArrow)&&indice>0){MostrarPagina(indice-1);return;}
#if ENABLE_INPUT_SYSTEM
        if(Gamepad.current!=null){avanzar|=Gamepad.current.buttonSouth.wasPressedThisFrame;omitir|=Gamepad.current.buttonEast.wasPressedThisFrame;}
#endif
        if(omitir)Finalizar();else if(avanzar)Avanzar();
    }
    public void Finalizar()
    {
        if(cerrado)return;cerrado=true;instancia=null;ReanudarEn=Time.unscaledTime+.3f;Time.timeScale=1;
        var callback=terminar;terminar=null;Destroy(gameObject);callback?.Invoke();
    }
    void OnDestroy(){if(instancia==this){instancia=null;Time.timeScale=1;}}
}
