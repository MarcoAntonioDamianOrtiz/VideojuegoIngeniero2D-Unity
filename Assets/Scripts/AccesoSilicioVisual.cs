using UnityEngine;
[ExecuteAlways]
public sealed class AccesoSilicioVisual : MonoBehaviour
{
    public Texture2D piedraMetal;
    public Font fuente;
    Transform vista;Sprite columna,blanco;SpriteRenderer[] indicadores;TextMesh estado;
    void OnEnable(){Refrescar();}
    public void Refrescar(){
        Limpiar();if(!piedraMetal||!fuente)return;
        vista=new GameObject("Marco permanente de acceso").transform;vista.SetParent(transform,false);vista.gameObject.hideFlags=HideFlags.DontSave;
        columna=Sprite.Create(piedraMetal,new Rect(0,0,28,piedraMetal.height),new Vector2(.5f,0),piedraMetal.height/3.3f);columna.hideFlags=HideFlags.DontSave;
        blanco=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f),Texture2D.whiteTexture.width);blanco.hideFlags=HideFlags.DontSave;
        indicadores=new SpriteRenderer[2];int i=0;
        foreach(float x in new[]{-3.8f,3.8f}){
            var post=Pieza("Pilar de piedra",columna,new Vector2(x,0),Vector2.one,Color.white);var box=post.gameObject.AddComponent<BoxCollider2D>();box.size=new Vector2(.65f,.4f);box.offset=new Vector2(0,.2f);
            indicadores[i++]=Pieza("Indicador de acceso",blanco,new Vector2(x,2.9f),new Vector2(.3f,.2f),Color.red);
        }
        Pieza("Dintel",blanco,new Vector2(0,3.15f),new Vector2(7.8f,.2f),new Color32(81,81,88,255));
        Pieza("Placa industrial",blanco,new Vector2(0,3.3f),new Vector2(6.9f,1.05f),new Color32(29,43,55,255));
        Texto("CIUDAD DE SILICIO",new Vector2(0,3.52f),.105f,new Color32(105,225,235,255));
        estado=Texto("NIVEL 3",new Vector2(0,3.08f),.045f,Color.white);
    }
    SpriteRenderer Pieza(string name,Sprite sprite,Vector2 pos,Vector2 scale,Color color){var go=new GameObject(name);go.transform.SetParent(vista,false);go.transform.localPosition=pos;go.transform.localScale=new Vector3(scale.x,scale.y,1);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.color=color;sr.sortingOrder=700;return sr;}
    TextMesh Texto(string text,Vector2 pos,float size,Color color){var go=new GameObject(text);go.transform.SetParent(vista,false);go.transform.localPosition=pos;var t=go.AddComponent<TextMesh>();t.font=fuente;t.fontSize=72;t.characterSize=size;t.text=text;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;go.GetComponent<MeshRenderer>().sharedMaterial=fuente.material;go.GetComponent<MeshRenderer>().sortingOrder=701;return t;}
    void Update(){if(indicadores==null)return;bool open=Application.isPlaying&&ProgresoTechnopolis.PuedeAvanzar(3);foreach(var i in indicadores)if(i)i.color=open?new Color32(110,230,151,255):new Color32(239,164,65,255);if(estado)estado.text=open?"ACCESO ABIERTO  >":"NIVEL 3 · ACCESO CONTROLADO";}
    void Limpiar(){if(vista){if(Application.isPlaying)Destroy(vista.gameObject);else DestroyImmediate(vista.gameObject);}if(columna){if(Application.isPlaying)Destroy(columna);else DestroyImmediate(columna);}if(blanco){if(Application.isPlaying)Destroy(blanco);else DestroyImmediate(blanco);}}
    void OnDisable(){Limpiar();}
}
