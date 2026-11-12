using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Exploración, encargos y mapa compartidos por las tres escenas de campaña.</summary>
public sealed partial class AventuraBarrio : MonoBehaviour
{
    public sealed class Encargo
    {
        public string id, nombre, personaje;
        public int pago, reputacion;
        public Vector2 punto;
        public string[] preguntas;
        public string[][] opciones;
        public int[] respuestas;
    }
    sealed class Marca { public Transform destino; public Vector2 punto; public RectTransform icono; public TMP_Text texto; public string id; }
    readonly List<Marca> marcas = new List<Marca>();
    readonly List<Encargo> encargos = new List<Encargo>();
    Transform jugador, bit;
    MovimientoJugador movimiento;
    BarraFelicidadPerro felicidad;
    Canvas canvas;
    RectTransform seguro, mapa;
    Camera camaraMapa;
    RenderTexture texturaMapa;
    GameObject modal;
    TMP_Text saldo, animo, reputacion, mision, aviso, bienvenida, textoModal, tituloModal;
    Image barraBit, barraReputacion, retratoBit;
    Button accion, alimentar;
    readonly List<Button> respuestas = new List<Button>();
    readonly List<GameObject> barreras = new List<GameObject>();
    Encargo actual;
    Vector2 salida, hueso;
    int nivel, pregunta, vidas;
    float inicio, ultimoAnimo, mensajeHasta;
    bool jugando, movimientoAnterior, puertaAbierta;
    bool rastroAvisado;
    TextMesh alerta;
    GameObject objetoHueso;
    static readonly Color Oro = new Color32(76,202,198,255), Papel = new Color32(215,239,233,255);
    static readonly Color Fondo = new Color32(12,21,31,248), Verde = new Color32(112,218,148,255);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Registrar()
    {
        SceneManager.sceneLoaded -= Cargar;
        SceneManager.sceneLoaded += Cargar;
    }
    static void Cargar(Scene escena, LoadSceneMode modo)
    {
        if (!escena.name.StartsWith("EscenaNivel") || (!escena.name.Contains("FielReferencia") && !escena.name.Contains("Taller") && !escena.name.Contains("Silicio"))) return;
        var go = new GameObject("Aventura · mapa y misiones");
        SceneManager.MoveGameObjectToScene(go, escena);
        go.AddComponent<AventuraBarrio>();
    }
    IEnumerator Start()
    {
        yield return null;
        jugador = GameObject.Find("Jugador")?.transform;
        bit = GameObject.Find("Bit")?.transform;
        if (!jugador) { enabled = false; yield break; }
        movimiento = jugador.GetComponent<MovimientoJugador>();
        if(!jugador.GetComponent<TransparenciaBarrio>())jugador.gameObject.AddComponent<TransparenciaBarrio>();
        nivel = gameObject.scene.name.Contains("Silicio") ? 3 : gameObject.scene.name.Contains("Taller") ? 2 : 1;
        felicidad = FindFirstObjectByType<BarraFelicidadPerro>();
        if (EconomiaBarrio.Obtener().PanaderiaPagada) ProgresoTechnopolis.Premiar("panaderia", 0, 20);
        ConstruirUI();
        EstilizarHUD();
        ConfigurarMapa();
        ConfigurarEncargos();
        ConfigurarSalida();
        if (bit)
        {
            var go = new GameObject("Bit · aviso de hallazgo");
            go.transform.SetParent(bit, false);
            go.transform.localScale = new Vector3(1f/bit.lossyScale.x,1f/bit.lossyScale.y,1f);
            alerta = go.AddComponent<TextMesh>();
            alerta.text = "!"; alerta.anchor = TextAnchor.MiddleCenter; alerta.fontSize = 80; alerta.characterSize = .08f;
            alerta.color = Oro;
            alerta.GetComponent<MeshRenderer>().sortingOrder = 32760;
        }
        hueso = nivel == 1 ? new Vector2(-4,-34) : (Vector2)jugador.position + Vector2.right * 3;
        if (nivel == 1 && !ProgresoTechnopolis.Completada("hueso")) CrearHueso();
        inicio = Time.unscaledTime;
        ultimoAnimo = Time.time;
        foreach (string nombre in new[] {"Canvas_AnimoPerro", "Canvas_Dinero"})
        { var go = GameObject.Find(nombre); if (go && go.TryGetComponent<Canvas>(out var viejo)) viejo.enabled = false; }
    }
    RectTransform Caja(string nombre, Transform padre, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        var r = go.GetComponent<RectTransform>(); r.SetParent(padre,false);
        r.anchorMin = r.anchorMax = new Vector2(0,1); r.pivot = new Vector2(0,1); r.anchoredPosition=pos; r.sizeDelta=size;
        var image=go.GetComponent<Image>(); image.color=color; image.raycastTarget=false;
        return r;
    }
    TMP_Text Texto(string texto, Transform padre, Vector2 pos, Vector2 size, int font=24)
    {
        var go=new GameObject(texto,typeof(RectTransform),typeof(TextMeshProUGUI));
        var r=go.GetComponent<RectTransform>();r.SetParent(padre,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();t.text=texto;t.fontSize=font;t.color=Papel;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;
        var terminalFont=Resources.Load<TMP_FontAsset>("FuenteTerminal");if(terminalFont)t.font=terminalFont;
        return t;
    }
    Button Boton(string texto, Transform padre, Vector2 pos, Vector2 size, Action click)
    {
        var r=Caja(texto,padre,pos,size,new Color32(24,55,65,255));var b=r.gameObject.AddComponent<Button>();
        b.targetGraphic=r.GetComponent<Image>();b.targetGraphic.raycastTarget=true;
        var colors=b.colors;colors.highlightedColor=new Color(1.25f,1.2f,1.1f);colors.pressedColor=new Color(.7f,.8f,.7f);b.colors=colors;
        var t=Texto(texto,r,new Vector2(12,-5),size-new Vector2(24,10),24);t.alignment=TextAlignmentOptions.Midline;
        b.onClick.AddListener(()=>click());return b;
    }
    void ConstruirUI()
    {
        var go=new GameObject("HUD · Exploración",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(go,gameObject.scene);canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=65;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        seguro=Caja("Área segura",go.transform,Vector2.zero,Vector2.zero,Color.clear);seguro.anchorMin=Vector2.zero;seguro.anchorMax=Vector2.one;seguro.offsetMin=seguro.offsetMax=Vector2.zero;
        var marco=Caja("Estado de Alex y Bit",seguro,new Vector2(24,-24),new Vector2(440,236),Oro);
        var fondo=Caja("Madera",marco,new Vector2(3,-3),new Vector2(434,230),Fondo);
        retratoBit=Caja("Retrato Bit",fondo,new Vector2(14,-16),new Vector2(64,64),Color.white).GetComponent<Image>();retratoBit.preserveAspect=true;
        var viejo=GameObject.Find("Canvas_AnimoPerro");
        if(viejo)foreach(var i in viejo.GetComponentsInChildren<Image>(true))if(i.name.ToLower().Contains("icono"))retrato(i.sprite);
        animo=Texto("BIT · ÁNIMO",fondo,new Vector2(94,-12),new Vector2(320,30),23);
        var canal=Caja("Canal Bit",fondo,new Vector2(94,-51),new Vector2(318,20),new Color(.1f,.1f,.1f));
        barraBit=Caja("Bienestar",canal,Vector2.zero,new Vector2(318,20),Verde).GetComponent<Image>();
        var billete=Caja("Billete ajolote",fondo,new Vector2(18,-95),new Vector2(64,38),Color.white).GetComponent<Image>();billete.preserveAspect=true;
        var anterior=GameObject.Find("Icono_billete");if(anterior&&anterior.TryGetComponent<Image>(out var icono))billete.sprite=icono.sprite;
        saldo=Texto("",fondo,new Vector2(94,-95),new Vector2(322,35),27);
        reputacion=Texto("",fondo,new Vector2(18,-141),new Vector2(398,30),22);
        var cr=Caja("Canal reputación",fondo,new Vector2(18,-182),new Vector2(394,13),new Color(.1f,.1f,.1f));
        barraReputacion=Caja("Reputación",cr,Vector2.zero,new Vector2(394,13),new Color32(107,180,203,255)).GetComponent<Image>();
        alimentar=Boton("Cuidar a Bit · $100",seguro,new Vector2(24,-270),new Vector2(440,48),Alimentar);
        var borde=Caja("Minimapa",seguro,new Vector2(-24,-24),new Vector2(350,300),Oro);borde.anchorMin=borde.anchorMax=new Vector2(1,1);borde.pivot=new Vector2(1,1);
        var back=Caja("Mapa interior",borde,new Vector2(4,-4),new Vector2(342,292),Fondo);
        Texto("",back,new Vector2(12,-6),new Vector2(320,34),23).text=nivel==3?"CIUDAD DE SILICIO":nivel==2?"BARRIO DE NEGOCIOS":"EL BARRIO";
        mapa=Caja("Vista del mapa",back,new Vector2(10,-46),new Vector2(322,222),Color.black);
        mapa.gameObject.AddComponent<RectMask2D>();
        var raw=new GameObject("Terreno en tiempo real",typeof(RectTransform),typeof(RawImage));raw.transform.SetParent(mapa,false);var rr=raw.GetComponent<RectTransform>();rr.anchorMin=Vector2.zero;rr.anchorMax=Vector2.one;rr.offsetMin=rr.offsetMax=Vector2.zero;raw.GetComponent<RawImage>().raycastTarget=false;
        texturaMapa=new RenderTexture(384,256,16);texturaMapa.filterMode=FilterMode.Point;raw.GetComponent<RawImage>().texture=texturaMapa;
        Texto("▲ ALEX   ! OBJETIVO   ● EDIFICIO",back,new Vector2(9,-270),new Vector2(326,20),15);
        var missionBox=Caja("Objetivo activo",seguro,new Vector2(-24,-336),new Vector2(350,142),Fondo);missionBox.anchorMin=missionBox.anchorMax=new Vector2(1,1);missionBox.pivot=new Vector2(1,1);
        mision=Texto("",missionBox,new Vector2(14,-12),new Vector2(322,122),21);
        accion=Boton("",seguro,new Vector2(0,80),new Vector2(700,60),Interactuar);var ar=accion.GetComponent<RectTransform>();ar.anchorMin=ar.anchorMax=new Vector2(.5f,0);ar.pivot=new Vector2(.5f,0);
        aviso=Texto("",seguro,new Vector2(0,155),new Vector2(1000,65),28);aviso.alignment=TextAlignmentOptions.Center;var av=aviso.rectTransform;av.anchorMin=av.anchorMax=new Vector2(.5f,0);av.pivot=new Vector2(.5f,0);
        bienvenida=Texto("",seguro,new Vector2(0,-180),new Vector2(1150,140),46);bienvenida.fontStyle=FontStyles.Bold;bienvenida.outlineWidth=.18f;bienvenida.outlineColor=Color.black;bienvenida.alignment=TextAlignmentOptions.Center;var br=bienvenida.rectTransform;br.anchorMin=br.anchorMax=new Vector2(.5f,1);br.pivot=new Vector2(.5f,1);
        modal=Caja("Libro de encargos",seguro,Vector2.zero,new Vector2(1100,680),Oro).gameObject;var mr=modal.GetComponent<RectTransform>();mr.anchorMin=mr.anchorMax=mr.pivot=new Vector2(.5f,.5f);
        var paper=Caja("Panel encargo",modal.transform,new Vector2(4,-4),new Vector2(1092,672),Fondo);
        tituloModal=Texto("",paper,new Vector2(38,-30),new Vector2(940,70),32);
        textoModal=Texto("",paper,new Vector2(38,-118),new Vector2(1000,200),29);
        for(int i=0;i<3;i++){int index=i;respuestas.Add(Boton("",paper,new Vector2(38,-340-i*84),new Vector2(1000,70),()=>Responder(index)));}
        Boton("Cerrar",paper,new Vector2(890,-600),new Vector2(150,48),Cerrar);
        modal.SetActive(false);
        if(!EventSystem.current)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
    }
    void retrato(Sprite s){if(s)retratoBit.sprite=s;}
    void ConfigurarMapa()
    {
        var go=new GameObject("Cámara minimapa");go.transform.position=new Vector3(jugador.position.x,jugador.position.y,-10);camaraMapa=go.AddComponent<Camera>();camaraMapa.orthographic=true;camaraMapa.orthographicSize=16;camaraMapa.targetTexture=texturaMapa;camaraMapa.cullingMask=Camera.main?Camera.main.cullingMask & ~(1<<5):~(1<<5);camaraMapa.clearFlags=CameraClearFlags.SolidColor;camaraMapa.backgroundColor=new Color(.12f,.14f,.12f);camaraMapa.allowHDR=false;
        SceneManager.MoveGameObjectToScene(go,gameObject.scene);
        mapaGrande=new GameObject("Mapa del distrito").AddComponent<MapaBarrioExpandido>();
        mapaGrande.Configurar(jugador,gameObject.scene,RecibirMarcador);
        var open=mapa.gameObject.AddComponent<Button>();open.targetGraphic=mapa.GetComponent<Image>();open.targetGraphic.raycastTarget=true;open.onClick.AddListener(mapaGrande.Abrir);
        foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            string n=t.name.ToLowerInvariant();
            if(n=="panaderia_ramona"||n=="taller"||n=="miscelanea"||n=="casa_alex"||n=="arbol_plaza"||n=="fuente_parque"||n=="biblioteca_barrio"||n.StartsWith("edificio_silicon_alley"))
                Marcar(t,t.position,"● "+(n.Contains("panaderia")?"PAN":n.Contains("biblioteca")?"LIB":n.Contains("silicon")?"OFICINA":n=="arbol_plaza"?"PLAZA":n=="fuente_parque"?"PARQUE":n=="casa_alex"?"CASA":n=="miscelanea"?"TIENDA":"TALLER"),new Color32(236,209,143,255));
        }
        Marcar(jugador,jugador.position,"▲",Color.white);
        if(bit)Marcar(bit,bit.position,"●",Verde);
    }
    void Marcar(Transform destino,Vector2 punto,string simbolo,Color color,string id="")
    {
        var t=Texto(simbolo,mapa,Vector2.zero,new Vector2(simbolo.Length>2?100:28,28),simbolo.Length>2?14:24);t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.pivot=new Vector2(.5f,.5f);
        marcas.Add(new Marca{destino=destino,punto=punto,icono=t.rectTransform,texto=t,id=id});
        mapaGrande?.Agregar(destino,punto,simbolo,color,id);
    }
    void ConfigurarEncargos()
    {
        if(nivel==3&&!ProgresoTechnopolis.Completada("auditoria"))
        {
            var oficina=FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None).Where(c=>c.name=="Edificio_Silicon_Alley").OrderBy(c=>Vector2.Distance(c.bounds.center,jugador.position)).FirstOrDefault();
            if(oficina){ProgresoTechnopolis.Activa="auditoria";Marcar(null,new Vector2(oficina.bounds.center.x,oficina.bounds.min.y-.65f),"!",Oro,"auditoria");}
        }
        if(nivel==2)
        {
            Vector2 punto=(Vector2)jugador.position+Vector2.right*2;
            foreach(var c in FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))if(c.name=="Taller")punto=new Vector2(c.bounds.max.x+1.5f,c.bounds.min.y-1);
            var npc=new GameObject("Don Dinero · encargos");npc.transform.position=punto;var sr=npc.AddComponent<SpriteRenderer>();var texture=Resources.Load<Texture2D>("Personajes/DonDineroTransparente");
            if(texture){sr.sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,0),texture.height/2f);sr.sortingOrder=TechnopolisOrdenBarrio.Orden(punto.y,1.25f);}
            var collider=npc.AddComponent<BoxCollider2D>();collider.size=new Vector2(.5f,.3f);collider.offset=new Vector2(0,.15f);
            encargos.Add(new Encargo{id="inventario",nombre="Inventario sin pérdidas",personaje="Don Dinero",pago=6000,reputacion=20,punto=punto,preguntas=new[]{"El inventario tiene 8 piezas y vendes 3. ¿Cuántas quedan?","¿Qué dato identifica cada producto sin confundirlo?","¿Qué validas antes de registrar una venta?"},opciones=new[]{new[]{"11","5","3"},new[]{"El identificador único","El color de la pantalla","La posición del botón"},new[]{"Que haya existencias","Que el sonido sea fuerte","Que el nombre sea largo"}},respuestas=new[]{1,0,0}});
            encargos.Add(new Encargo{id="respaldo",nombre="Respaldo del negocio",personaje="Don Dinero",pago=6000,reputacion=20,punto=punto,preguntas=new[]{"¿Dónde conservas una copia segura?","Antes de cambiar datos importantes…","¿Cómo verificas un respaldo?"},opciones=new[]{new[]{"Solo en la memoria RAM","En otro almacenamiento protegido","En la papelera"},new[]{"Borras todo","Desactivas controles","Guardas una copia"},new[]{"Pruebas restaurarlo","Cambias el icono","Cierras la ventana"}},respuestas=new[]{1,2,0}});
        }
        foreach(var e in encargos)if(!ProgresoTechnopolis.Completada(e.id))Marcar(null,e.punto,"!",Oro,e.id);
        if(nivel==1&&!ProgresoTechnopolis.Completada("panaderia")){ProgresoTechnopolis.Activa="panaderia";Marcar(GameObject.Find("Ramona_Npc")?.transform,new Vector2(4,-10.4f),"!",Oro,"panaderia");}
    }
    void ConfigurarSalida()
    {
        if(nivel==3)return;
        var gate=GameObject.Find("Porton_Salida");
        var solido=gate?gate.GetComponentInChildren<Collider2D>():null;
        salida=solido?(Vector2)solido.bounds.center:gate?(Vector2)gate.transform.position:new Vector2(18,-46);
        foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if(t.name=="Porton_Salida"||t.name=="Conos_Salida"||t.name=="Aviso_Salida_Cerrada"||t.name=="senal_bloqueo_301")barreras.Add(t.gameObject);
        Marcar(null,salida,">",new Color32(114,202,223,255));
    }
    void CrearHueso()
    {
        objetoHueso=new GameObject("Hueso de Bit");objetoHueso.transform.position=hueso;
        var texture=new Texture2D(24,12,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;
        for(int y=0;y<12;y++)for(int x=0;x<24;x++)texture.SetPixel(x,y,((x>=4&&x<20&&y>=4&&y<8)||((x<7||x>16)&&(y>=2&&y<10)))?Papel:Color.clear);
        texture.Apply();var sr=objetoHueso.AddComponent<SpriteRenderer>();sr.sprite=Sprite.Create(texture,new Rect(0,0,24,12),new Vector2(.5f,.5f),24);sr.sortingOrder=32700;
        Marcar(objetoHueso.transform,hueso,"!",Verde,"hueso");
    }
    void Alimentar()
    {
        if(modal.activeSelf||!movimiento.enabled)return;
        if(ProgresoTechnopolis.Bienestar>=100){Avisar("Bit está contento. ¡Gracias por cuidarlo!");return;}
        if(!EconomiaBarrio.Obtener().Gastar(100)){Avisar("Necesitas $100 para comida y descanso de Bit.");return;}
        ProgresoTechnopolis.CuidarBit(30);Avisar("Bit comió y descansó · +30 de bienestar");
    }
    void Avisar(string text){aviso.text=text;mensajeHasta=Time.unscaledTime+4;}
    void Update()
    {
        if(!canvas||!jugador)return;
        canvas.enabled=!TechnopolisGameFlow.IsMenuOpen;
        ActualizarVisibilidad();
        if(!canvas.enabled)return;
        var safe=Screen.safeArea;seguro.anchorMin=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);seguro.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
        int bienestar=ProgresoTechnopolis.Bienestar;
        if(Time.time-ultimoAnimo>=90 && movimiento.enabled&&!modal.activeSelf){ProgresoTechnopolis.CuidarBit(-2);ultimoAnimo=Time.time;}
        if(felicidad)felicidad.EstablecerFelicidad(bienestar);
        if(felicidad&&felicidad.IconoActual)retratoBit.sprite=felicidad.IconoActual;
        animo.text=$"BIT  {bienestar}%";barraBit.rectTransform.sizeDelta=new Vector2(150*bienestar/100f,10);barraBit.color=bienestar>=60?Verde:bienestar>20?Oro:new Color32(210,105,82,255);
        saldo.text="$ "+EconomiaBarrio.Obtener().Dinero.ToString("N0");reputacion.text=$"REP  {ProgresoTechnopolis.Reputacion}/100";barraReputacion.rectTransform.sizeDelta=new Vector2(210*ProgresoTechnopolis.Reputacion/100f,8);
        if(Time.unscaledTime>mensajeHasta)aviso.text="";
        float age=Time.unscaledTime-inicio;float a=Mathf.Min(Mathf.Clamp01(age),Mathf.Clamp01(5-age));bienvenida.alpha=a;
        bienvenida.rectTransform.anchoredPosition=new Vector2(0,Mathf.Lerp(-140,-180,Mathf.SmoothStep(0,1,Mathf.Clamp01(age))));
        bienvenida.text="BIENVENIDO A\n"+(nivel==3?"LA CIUDAD DE SILICIO":nivel==2?"EL BARRIO DE LOS NEGOCIOS":"EL SEMESTRE DEL HAMBRE");
        if(nivel<3&&!puertaAbierta&&ProgresoTechnopolis.PuedeAvanzar(nivel+1))
        {puertaAbierta=true;foreach(var g in barreras)if(g)g.SetActive(false);Avisar("¡Acceso desbloqueado! Sigue > en el minimapa.");}
        string active=ProgresoTechnopolis.Activa;
        mision.text="OBJETIVO\n"+(active=="hueso"?"Bit sigue un rastro. Recoge el hueso marcado en verde.":active=="panaderia"?"Habla con Doña Ramona en la panadería.":encargos.FirstOrDefault(e=>e.id==active)?.nombre ?? (nivel<3?"Acceso: "+ProgresoTechnopolis.Requisito(nivel+1):"Visita a Don Dinero en Silicon Alley."));
        bool bone=nivel==1&&objetoHueso&&Vector2.Distance(bit?bit.position:jugador.position,hueso)<5;
        if(bone&&!rastroAvisado){rastroAvisado=true;ProgresoTechnopolis.Activa="hueso";Avisar("¡Bit encontró un rastro! Busca la marca verde del mapa.");}
        bool missionNear=encargos.Any(e=>!ProgresoTechnopolis.Completada(e.id)&&Vector2.Distance(jugador.position,e.punto)<5);
        var ramona=GameObject.Find("Ramona_Npc");
        missionNear |= nivel==1&&!ProgresoTechnopolis.Completada("panaderia")&&ramona&&Vector2.Distance(jugador.position,ramona.transform.position)<5;
        if(alerta){alerta.gameObject.SetActive(bone||missionNear||bienestar<=20);alerta.transform.position=bit.GetComponent<SpriteRenderer>().bounds.max+new Vector3(-.25f,.35f+Mathf.Sin(Time.time*4)*.08f,0);}
        string prompt=Prompt();accion.gameObject.SetActive(!modal.activeSelf&&movimiento.enabled&&!string.IsNullOrEmpty(prompt));accion.GetComponentInChildren<TMP_Text>().text=prompt;
        if(accion.gameObject.activeSelf&&Input.GetKeyDown(KeyCode.E))Interactuar();
    }
    string Prompt()
    {
        if(objetoHueso&&Vector2.Distance(jugador.position,hueso)<1.6f)return "Recoger el hueso de Bit (E)";
        if(nivel<3&&Vector2.Distance(jugador.position,salida)<4)return puertaAbierta?"Cruzar al siguiente distrito (E)":"Revisar requisitos del acceso (E)";
        var e=encargos.FirstOrDefault(q=>!ProgresoTechnopolis.Completada(q.id)&&Vector2.Distance(jugador.position,q.punto)<2.4f);
        if(e!=null)return "Hablar con "+e.personaje+" (E)";
        return "";
    }
    void Interactuar()
    {
        if(modal.activeSelf||!movimiento.enabled)return;
        if(objetoHueso&&Vector2.Distance(jugador.position,hueso)<1.6f){ProgresoTechnopolis.Premiar("hueso",0,5);ProgresoTechnopolis.CuidarBit(25);if(!ProgresoTechnopolis.Completada("panaderia"))ProgresoTechnopolis.Activa="panaderia";Destroy(objetoHueso);Avisar("¡Bit encontró su hueso! +5 reputación y +25 bienestar");return;}
        if(nivel<3&&Vector2.Distance(jugador.position,salida)<4){if(puertaAbierta)TechnopolisGameFlow.AvanzarCampana(nivel+1);else Avisar("Para abrir el acceso necesitas "+ProgresoTechnopolis.Requisito(nivel+1));return;}
        actual=encargos.FirstOrDefault(e=>!ProgresoTechnopolis.Completada(e.id)&&Vector2.Distance(jugador.position,e.punto)<2.4f);
        if(actual==null)return;
        jugando=false;movimientoAnterior=movimiento.enabled;movimiento.enabled=false;modal.SetActive(true);
        tituloModal.text=actual.personaje+" · "+actual.nombre;textoModal.text=$"Necesito que revises el sistema de mi negocio.\n\nRecompensa: ${actual.pago:N0} y +{actual.reputacion} reputación.\nTres pruebas de lógica. Tienes tres vidas.";
        MostrarOpciones(new[]{"Aceptar y empezar","Más tarde"},i=>{if(i==0){ProgresoTechnopolis.Activa=actual.id;jugando=true;pregunta=0;vidas=3;MostrarPregunta();}else Cerrar();});
    }
    void MostrarOpciones(string[] textos,Action<int> callback)
    {
        for(int i=0;i<respuestas.Count;i++){int n=i;var b=respuestas[i];b.gameObject.SetActive(i<textos.Length);b.onClick.RemoveAllListeners();if(i<textos.Length){b.GetComponentInChildren<TMP_Text>().text=textos[i];b.onClick.AddListener(()=>callback(n));}}
    }
    void MostrarPregunta(){textoModal.text=$"PRUEBA {pregunta+1}/3  ·  VIDAS {vidas}\n\n{actual.preguntas[pregunta]}";MostrarOpciones(actual.opciones[pregunta],Responder);}
    void Responder(int index)
    {
        if(!jugando)return;
        var campana=FindFirstObjectByType<TechnopolisCampana>();if(campana)campana.Sonido(index==actual.respuestas[pregunta]);
        if(index!=actual.respuestas[pregunta]){vidas--;if(vidas>0){MostrarPregunta();return;}jugando=false;textoModal.text="Se agotaron las vidas. Revisa el problema y vuelve a intentarlo.";MostrarOpciones(new[]{"Reintentar"},i=>{vidas=3;pregunta=0;jugando=true;MostrarPregunta();});return;}
        pregunta++;if(pregunta<actual.preguntas.Length){MostrarPregunta();return;}
        jugando=false;bool pago=ProgresoTechnopolis.Premiar(actual.id,actual.pago,actual.reputacion);textoModal.text=pago?$"¡Trabajo completado!\n\n+${actual.pago:N0}   +{actual.reputacion} reputación\nEl negocio vuelve a funcionar.":"Este encargo ya fue recompensado.";MostrarOpciones(new[]{"Volver al barrio"},i=>Cerrar());
    }
    void Cerrar(){jugando=false;modal.SetActive(false);movimiento.enabled=movimientoAnterior;}
    void LateUpdate()
    {
        if(!camaraMapa||!jugador)return;
        camaraMapa.transform.position=new Vector3(jugador.position.x,jugador.position.y,-10);
        foreach(var m in marcas){m.icono.gameObject.SetActive(m.id==""||!ProgresoTechnopolis.Completada(m.id));Vector2 pos=m.destino?(Vector2)m.destino.position:m.punto;Vector2 d=pos-(Vector2)jugador.position;float x=d.x/(32*camaraMapa.aspect)*322,y=d.y/32*222;m.icono.anchoredPosition=new Vector2(161+Mathf.Clamp(x,-126,126),-111+Mathf.Clamp(y,-98,98));}
        ActualizarMarcador();
    }
    void OnDestroy(){if(destinoMundo)Destroy(destinoMundo.gameObject);if(mapaGrande)Destroy(mapaGrande.gameObject);if(camaraMapa)Destroy(camaraMapa.gameObject);if(texturaMapa){texturaMapa.Release();Destroy(texturaMapa);}if(canvas)Destroy(canvas.gameObject);}
}
