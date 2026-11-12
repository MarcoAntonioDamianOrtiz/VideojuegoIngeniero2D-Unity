using UnityEngine;
using UnityEngine.UI;

/// <summary>Ambiente del menú independiente del tiempo de la partida pausada.</summary>
public sealed class TechnopolisMenuAnimacion : MonoBehaviour
{
    public RectTransform escenario;
    public Image seleccion;
    public Image flecha;
    public Texture2D caricias;
    public Rect[] recortesCaricia;
    public Image alexSentado, bitSentado;
    public RectTransform personajes;
    Sprite[] fotogramas;
    Image pareja;
    Vector2 origenPersonajes;
    float inicio;
    Vector2 posicionInicial;
    Vector3 escalaInicial;
    Color colorSeleccion;
    MenuAmbientePixel ambiente;

    void Awake()
    {
        if (!escenario) { enabled=false; return; }
        posicionInicial=escenario.anchoredPosition;
        escalaInicial=escenario.localScale;
        colorSeleccion=seleccion?seleccion.color:Color.white;
        inicio=Time.unscaledTime;
        if(personajes)origenPersonajes=personajes.anchoredPosition;
        if(caricias&&recortesCaricia!=null&&recortesCaricia.Length==4&&personajes){
            fotogramas=new Sprite[4];
            for(int i=0;i<4;i++)fotogramas[i]=Sprite.Create(caricias,recortesCaricia[i],new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
            var go=new GameObject("Alex y Bit · caricia",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            pareja=go.GetComponent<Image>();pareja.raycastTarget=false;pareja.preserveAspect=true;
            var rt=pareja.rectTransform;rt.SetParent(personajes,false);rt.anchorMin=rt.anchorMax=rt.pivot=Vector2.zero;
            rt.anchoredPosition=new Vector2(375,98);rt.sizeDelta=new Vector2(257*recortesCaricia[0].width/recortesCaricia[0].height,257);
            pareja.sprite=fotogramas[0];if(alexSentado)alexSentado.enabled=false;if(bitSentado)bitSentado.enabled=false;
        }
    }
    void OnEnable()
    {
        if(!escenario)return;
        if(ambiente){ambiente.gameObject.SetActive(true);return;}
        var go=new GameObject("Ambiente animado · humo brasas y luces",typeof(RectTransform));
        var r=go.GetComponent<RectTransform>();r.SetParent(escenario,false);
        r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
        ambiente=go.AddComponent<MenuAmbientePixel>();ambiente.raycastTarget=false;
    }
    void LateUpdate()
    {
        float t=Time.unscaledTime;
        Vector2 viaje=new Vector2(Mathf.Round(Mathf.Sin(t*.18f)*14),Mathf.Round(Mathf.Sin(t*.13f)*6));
        escenario.localScale=escalaInicial*1.035f;
        escenario.anchoredPosition=posicionInicial+viaje;
        if(personajes)personajes.anchoredPosition=origenPersonajes+viaje*.6f;
        if(pareja&&fotogramas!=null){
            // Reposa, levanta la mano, acaricia dos veces y vuelve a su pose.
            float ciclo=Mathf.Repeat(t-inicio,16);
            int frame=0;
            if(ciclo>=9&&ciclo<9.4f)frame=1;
            else if(ciclo>=9.4f&&ciclo<12.2f)frame=2+(Mathf.FloorToInt((ciclo-9.4f)*4)%2);
            else if(ciclo>=12.2f&&ciclo<12.6f)frame=1;
            pareja.sprite=fotogramas[frame];
        }
        if(seleccion){Color c=colorSeleccion;c.a*=.9f+.1f*Mathf.Sin(t*2);seleccion.color=c;}
        if(flecha)flecha.rectTransform.anchoredPosition=new Vector2(10+Mathf.Round(Mathf.Sin(t*3)*3),-18);
    }
    void OnDisable()
    {
        if(escenario){escenario.anchoredPosition=posicionInicial;escenario.localScale=escalaInicial;}
        if(personajes)personajes.anchoredPosition=origenPersonajes;
        if(seleccion)seleccion.color=colorSeleccion;
        if(ambiente)ambiente.gameObject.SetActive(false);
    }
    void OnDestroy(){if(fotogramas!=null)foreach(var s in fotogramas)if(s)Destroy(s);}
}

/// <summary>Partículas de pocos píxeles y luces suaves; no intercepta la interfaz.</summary>
public sealed class MenuAmbientePixel : MaskableGraphic
{
    readonly UIVertex[] quad=new UIVertex[4];
    int anterior=-1;
    void Update(){int frame=Mathf.FloorToInt(Time.unscaledTime*12);if(frame==anterior)return;anterior=frame;SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();Rect r=rectTransform.rect;float w=r.width,h=r.height,t=Mathf.Floor(Time.unscaledTime*12)/12;
        Vector2 fire=new Vector2(.604f*w,.255f*h);
        for(int i=0;i<15;i++){
            float age=Mathf.Repeat(t*.22f+i*.137f,1);
            float x=fire.x+Mathf.Sin(i*7.1f+age*5)*12+age*16;
            float y=fire.y+age*110;
            float alpha=Mathf.Sin(age*Mathf.PI)*.6f;
            Draw(vh,r,new Vector2(x,y),new Vector2(i%3==0?4:2,3),new Color(1,.66f,.19f,alpha));
        }
        for(int i=0;i<8;i++){
            float age=Mathf.Repeat(t*.09f+i*.173f,1);
            Vector2 pos=fire+new Vector2(age*35+Mathf.Sin(age*8+i)*9,24+age*150);
            Draw(vh,r,pos,Vector2.one*(15+age*26),new Color(.34f,.27f,.32f,Mathf.Sin(age*Mathf.PI)*.075f));
        }
        // La luz de las farolas y los rótulos respira sin destellos bruscos.
        Light(vh,r,new Vector2(.05f*w,.553f*h),32,new Color(1,.65f,.18f,.08f),t);
        Light(vh,r,new Vector2(.132f*w,.38f*h),28,new Color(1,.65f,.18f,.08f),t+2);
        Light(vh,r,fire,55,new Color(1,.42f,.08f,.06f),t*1.7f);
        Light(vh,r,new Vector2(.297f*w,.452f*h),28,new Color(.1f,1,.85f,.035f),t*.7f);
    }
    void Light(VertexHelper vh,Rect r,Vector2 center,float size,Color color,float time)
    {
        float wave=.75f+.25f*Mathf.Sin(time*2);
        for(int k=3;k>0;k--){Color c=color;c.a*=wave*(4-k)/3;Draw(vh,r,center,Vector2.one*(size*k/2),c);}
    }
    void Draw(VertexHelper vh,Rect r,Vector2 center,Vector2 size,Color color)
    {
        center+=r.min;center.x=Mathf.Round(center.x);center.y=Mathf.Round(center.y);
        Vector2 lo=center-size*.5f,hi=center+size*.5f;
        for(int i=0;i<4;i++){quad[i]=UIVertex.simpleVert;quad[i].color=color;quad[i].uv0=Vector2.zero;}
        quad[0].position=new Vector3(lo.x,lo.y);quad[1].position=new Vector3(lo.x,hi.y);quad[2].position=new Vector3(hi.x,hi.y);quad[3].position=new Vector3(hi.x,lo.y);vh.AddUIVertexQuad(quad);
    }
}
