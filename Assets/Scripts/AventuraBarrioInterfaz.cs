using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class AventuraBarrio
{
    readonly List<CanvasGroup> gruposHUD=new List<CanvasGroup>();
    MapaBarrioExpandido mapaGrande;
    MinijuegoAleatorioPanaderia[] terminales;
    InteraccionDonaRamona ramonaInteraccion;
    Vector2? puntoUsuario;
    Marca marcaUsuario;
    TMP_Text destinoPantalla;
    RectTransform destinoContenedor;
    TextMesh destinoMundo;
    bool ocupado;

    void Colocar(RectTransform r,Vector2 pos,Vector2 size){r.anchoredPosition=pos;r.sizeDelta=size;}
    void EstilizarHUD()
    {
        var panel=seguro.Find("Estado de Alex y Bit") as RectTransform;
        panel.anchorMin=panel.anchorMax=panel.pivot=Vector2.zero;Colocar(panel,new Vector2(18,66),new Vector2(520,96));
        var fondo=panel.Find("Madera") as RectTransform;fondo.name="Terminal de estado";Colocar(fondo,new Vector2(3,-3),new Vector2(514,90));
        Colocar(retratoBit.rectTransform,new Vector2(12,-12),new Vector2(56,56));
        Colocar(animo.rectTransform,new Vector2(82,-9),new Vector2(170,25));animo.fontSize=21;
        var canal=barraBit.transform.parent as RectTransform;Colocar(canal,new Vector2(82,-46),new Vector2(150,10));barraBit.rectTransform.sizeDelta=new Vector2(150,10);
        fondo.Find("Billete ajolote").gameObject.SetActive(false);
        Colocar(saldo.rectTransform,new Vector2(260,-9),new Vector2(242,30));saldo.fontSize=25;
        Colocar(reputacion.rectTransform,new Vector2(260,-40),new Vector2(240,25));reputacion.fontSize=21;
        Colocar(barraReputacion.transform.parent as RectTransform,new Vector2(260,-73),new Vector2(210,8));
        var food=alimentar.GetComponent<RectTransform>();food.anchorMin=food.anchorMax=food.pivot=Vector2.zero;Colocar(food,new Vector2(18,18),new Vector2(270,36));alimentar.GetComponentInChildren<TMP_Text>().fontSize=20;
        var mini=seguro.Find("Minimapa") as RectTransform;mini.localScale=Vector3.one*.72f;
        var objetivos=seguro.Find("Objetivo activo") as RectTransform;objetivos.localScale=Vector3.one*.72f;objetivos.anchoredPosition=new Vector2(-24,-250);objetivos.sizeDelta=new Vector2(350,120);
        mision.fontSize=21;mision.rectTransform.sizeDelta=new Vector2(322,100);
        destinoContenedor=Caja("Destino personal",seguro,Vector2.zero,new Vector2(160,66),new Color32(8,23,31,240));
        destinoContenedor.anchorMin=destinoContenedor.anchorMax=Vector2.zero;destinoContenedor.pivot=new Vector2(.5f,.5f);
        destinoContenedor.GetComponent<Image>().raycastTarget=false;
        destinoPantalla=Texto("Distancia",destinoContenedor,new Vector2(4,-3),new Vector2(152,60),24);destinoPantalla.alignment=TextAlignmentOptions.Center;destinoPantalla.color=Oro;
        destinoContenedor.gameObject.SetActive(false);
        foreach(Transform child in seguro)if(child.gameObject!=modal){var group=child.gameObject.AddComponent<CanvasGroup>();gruposHUD.Add(group);}
        terminales=FindObjectsByType<MinijuegoAleatorioPanaderia>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        ramonaInteraccion=FindFirstObjectByType<InteraccionDonaRamona>();
        tituloModal.text="// MISIÓN";
    }
    void ActualizarVisibilidad()
    {
        ocupado=modal.activeSelf||!movimiento.enabled;
        if(terminales!=null)foreach(var t in terminales)ocupado|=t&&t.gameObject.activeInHierarchy;
        ocupado|=ramonaInteraccion&&ramonaInteraccion.panelDialogo&&ramonaInteraccion.panelDialogo.activeInHierarchy;
        foreach(var g in gruposHUD){g.alpha=ocupado?0:1;g.interactable=!ocupado;g.blocksRaycasts=!ocupado;}
        if(mapaGrande)mapaGrande.PermitirAbrir=!ocupado;
        if(destinoMundo)destinoMundo.gameObject.SetActive(!ocupado&&puntoUsuario.HasValue);
    }
    void RecibirMarcador(Vector2? punto)
    {
        puntoUsuario=punto;
        if(marcaUsuario!=null){marcas.Remove(marcaUsuario);Destroy(marcaUsuario.icono.gameObject);marcaUsuario=null;}
        if(!punto.HasValue){destinoContenedor.gameObject.SetActive(false);if(destinoMundo)destinoMundo.gameObject.SetActive(false);return;}
        var text=Texto("X",mapa,Vector2.zero,new Vector2(28,28),25);text.color=Oro;text.alignment=TextAlignmentOptions.Center;text.rectTransform.pivot=new Vector2(.5f,.5f);
        marcaUsuario=new Marca{punto=punto.Value,icono=text.rectTransform,texto=text};marcas.Add(marcaUsuario);
        if(!destinoMundo){var go=new GameObject("Marcador personal · mundo");destinoMundo=go.AddComponent<TextMesh>();destinoMundo.text="v";destinoMundo.color=Oro;destinoMundo.fontSize=60;destinoMundo.characterSize=.12f;destinoMundo.anchor=TextAnchor.MiddleCenter;go.GetComponent<MeshRenderer>().sortingOrder=32740;}
        destinoMundo.transform.position=(Vector3)punto.Value+Vector3.up*.5f;
        destinoContenedor.gameObject.SetActive(true);
    }
    void ActualizarMarcador()
    {
        if(!puntoUsuario.HasValue||!destinoPantalla||!Camera.main)return;
        float metros=Vector2.Distance(jugador.position,puntoUsuario.Value); // Escala del mundo: 1 unidad = 1 metro.
        var vista=Camera.main.WorldToViewportPoint(puntoUsuario.Value);
        float x=Mathf.Clamp(vista.x,.12f,.88f),y=Mathf.Clamp(vista.y,.24f,.82f);
        destinoContenedor.anchoredPosition=new Vector2(x*seguro.rect.width,y*seguro.rect.height);
        destinoPantalla.text=$"v\n{Mathf.CeilToInt(metros)} m";
        if(metros<1.2f)destinoPantalla.text="DESTINO\nLlegaste";
    }
}
