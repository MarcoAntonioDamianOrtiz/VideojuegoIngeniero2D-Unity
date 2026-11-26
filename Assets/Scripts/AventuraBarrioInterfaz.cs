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
    Vector2 puntoRamona;
    Vector2? puntoDonDinero;
    string objetivoGuiado;
    GameObject senalPanaderia;
    BibliotecaInteractiva biblioteca;
    Collider2D entradaBiblioteca;
    bool alertaBitMostrada;
    bool alertaHambreMostrada;

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
        if (nivel == 2)
        {
            panel.anchoredPosition = new Vector2(18, 88);
            Colocar(food, new Vector2(18, 18), new Vector2(420, 58));
            var etiqueta = alimentar.GetComponentInChildren<TMP_Text>();
            Colocar(etiqueta.rectTransform, new Vector2(12, -5), new Vector2(396, 48));
            etiqueta.fontSize = 24;
            etiqueta.fontStyle = FontStyles.Bold;
        }
        var mini=seguro.Find("Minimapa") as RectTransform;mini.localScale=Vector3.one*.72f;
        var objetivos=seguro.Find("Objetivo activo") as RectTransform;objetivos.localScale=Vector3.one*.72f;objetivos.anchoredPosition=new Vector2(-24,-250);objetivos.sizeDelta=new Vector2(350,120);
        mision.fontSize=21;mision.rectTransform.sizeDelta=new Vector2(322,100);
        if (nivel < 3)
        {
            objetivos.sizeDelta = new Vector2(350, 242);
            mision.rectTransform.sizeDelta = new Vector2(322, 218);
        }
        if (nivel == 1)
        {
            biblioteca = FindFirstObjectByType<BibliotecaInteractiva>();
            if (biblioteca && biblioteca.TryGetComponent(out entradaBiblioteca))
                biblioteca.LibroCerrado += AlCerrarBiblioteca;
        }
        destinoContenedor=Caja("Destino",seguro,Vector2.zero,new Vector2(236,66),new Color32(8,23,31,240));
        destinoContenedor.anchorMin=destinoContenedor.anchorMax=Vector2.zero;destinoContenedor.pivot=new Vector2(.5f,.5f);
        destinoContenedor.GetComponent<Image>().raycastTarget=false;
        destinoPantalla=Texto("Distancia",destinoContenedor,new Vector2(4,-3),new Vector2(228,60),24);destinoPantalla.alignment=TextAlignmentOptions.Center;destinoPantalla.color=Oro;
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
        if (mapaGrande) mapaGrande.PermitirAbrir = !ocupado && !EnInteriorBit;
        mapa.parent.parent.gameObject.SetActive(!EnInteriorBit);
        mision.transform.parent.gameObject.SetActive(!EnInteriorBit);
        destinoContenedor.gameObject.SetActive(!EnInteriorBit && puntoUsuario.HasValue);
        if(destinoMundo)destinoMundo.gameObject.SetActive(!ocupado&&puntoUsuario.HasValue);
    }
    void CrearSenalPanaderia()
    {
        // La calle al este de la plaza conecta con la acera de Ramona.
        Vector2 cruce = puntoRamona + new Vector2(7f, -0.5f);
        senalPanaderia = new GameObject("Señal hacia la panadería");
        senalPanaderia.transform.SetParent(transform, false);
        senalPanaderia.transform.position = cruce;
        var texto = senalPanaderia.AddComponent<TextMesh>();
        texto.text = "← PANADERÍA";
        texto.anchor = TextAnchor.MiddleCenter;
        texto.fontSize = 48;
        texto.characterSize = .07f;
        texto.color = Oro;
        senalPanaderia.GetComponent<MeshRenderer>().sortingOrder = 32740;
        senalPanaderia.SetActive(!ProgresoTechnopolis.Completada("panaderia"));
        Marcar(null, cruce, "←", Oro, "panaderia");
    }

    void ActualizarGuiaObjetivo()
    {
        if (entradaBiblioteca && ProgresoTechnopolis.Completada("panaderia") &&
            ProgresoTechnopolis.VisitaBiblioteca == 0 && !ocupado)
        {
            ProgresoTechnopolis.VisitaBiblioteca = 2;
            movimientoAnterior = movimiento.enabled;
            movimiento.enabled = false;
            tituloModal.text = "Biblioteca del pueblo";
            textoModal.text = "Ya ayudaste a Doña Ramona. Puedes visitar la biblioteca del pueblo antes de ir al siguiente distrito.";
            modal.SetActive(true);
            MostrarOpciones(new[] { "Visitar la biblioteca", "Seguir adelante" }, opcion =>
            {
                if (opcion == 0) ProgresoTechnopolis.VisitaBiblioteca = 1;
                Cerrar();
            });
        }
        string nombre = null;
        Vector2 punto = Vector2.zero;
        if (nivel == 2 && ProgresoTechnopolis.BitEnfermo && hospitalBit != null)
        {
            if (!alertaBitMostrada && !ocupado)
            {
                alertaBitMostrada = true;
                MostrarAvisoBit("Bit está enfermo",
                    "Lleva a Bit a la veterinaria. Su entrada está marcada en el mapa.", "Ir a la veterinaria");
            }
            nombre = "Veterinaria";
            punto = hospitalBit.Entrada;
        }
        else if (nivel == 2 && ProgresoTechnopolis.BitCurado && !ProgresoTechnopolis.BitComioCroquetas && tiendaBit != null)
        {
            if (ProgresoTechnopolis.Croquetas == 0)
            {
                if (!alertaHambreMostrada && !ocupado)
                {
                    alertaHambreMostrada = true;
                    MostrarAvisoBit("Bit tiene hambre",
                        "Bit ya se recuperó. Compra una ración en la tienda marcada y pulsa «Dar croquetas a Bit».", "Ir a la tienda");
                }
                nombre = "Tienda de croquetas";
                punto = tiendaBit.Entrada;
            }
        }
        else if (entradaBiblioteca && ProgresoTechnopolis.VisitaBiblioteca == 1)
        {
            nombre = "Biblioteca";
            punto = entradaBiblioteca.bounds.center;
        }
        else if (puertaAbierta)
        {
            nombre = "Salida";
            punto = salida;
        }
        else if (nivel == 1 && !ProgresoTechnopolis.Completada("panaderia"))
        {
            nombre = "Doña Ramona";
            punto = puntoRamona;
        }
        else if (puntoDonDinero.HasValue &&
                 (nivel == 2 && (!ProgresoTechnopolis.Completada("inventario") || !ProgresoTechnopolis.Completada("respaldo"))
                  || nivel == 3 && !ProgresoTechnopolis.Completada("auditoria")))
        {
            nombre = "Don Dinero";
            punto = puntoDonDinero.Value;
        }

        if (objetivoGuiado == nombre) return;
        objetivoGuiado = nombre;
        if (senalPanaderia) senalPanaderia.SetActive(nombre == "Doña Ramona");
        if (nombre == null) mapaGrande.QuitarObjetivo();
        else mapaGrande.MarcarObjetivo(nombre, punto);
    }
    void MostrarAvisoBit(string titulo, string mensaje, string boton)
    {
        movimientoAnterior = movimiento.enabled;
        movimiento.enabled = false;
        tituloModal.text = titulo;
        textoModal.text = mensaje;
        modal.SetActive(true);
        MostrarOpciones(new[] { boton }, _ => Cerrar());
    }

    void AlCerrarBiblioteca()
    {
        if (ProgresoTechnopolis.VisitaBiblioteca == 1)
            ProgresoTechnopolis.VisitaBiblioteca = 2;
    }
    void RecibirMarcador(Vector2? punto)
    {
        puntoUsuario=punto;
        if(marcaUsuario!=null){marcas.Remove(marcaUsuario);Destroy(marcaUsuario.icono.gameObject);marcaUsuario=null;}
        if(!punto.HasValue){destinoContenedor.gameObject.SetActive(false);if(destinoMundo)destinoMundo.gameObject.SetActive(false);return;}
        if (mapaGrande.NombreObjetivo == null)
        {
            var text=Texto("X",mapa,Vector2.zero,new Vector2(28,28),25);text.color=Oro;text.alignment=TextAlignmentOptions.Center;text.rectTransform.pivot=new Vector2(.5f,.5f);
            marcaUsuario=new Marca{punto=punto.Value,icono=text.rectTransform,texto=text};marcas.Add(marcaUsuario);
        }
        if(!destinoMundo){var go=new GameObject("Marcador personal · mundo");destinoMundo=go.AddComponent<TextMesh>();destinoMundo.text="v";destinoMundo.color=Oro;destinoMundo.fontSize=60;destinoMundo.characterSize=.12f;destinoMundo.anchor=TextAnchor.MiddleCenter;go.GetComponent<MeshRenderer>().sortingOrder=32740;}
        destinoMundo.transform.position=(Vector3)punto.Value+Vector3.up*.5f;
        destinoContenedor.gameObject.SetActive(true);
    }
    void ActualizarMarcador()
    {
        if(!puntoUsuario.HasValue||!destinoPantalla||!Camera.main)return;
        float metros=Vector2.Distance(jugador.position,puntoUsuario.Value); // Escala del mundo: 1 unidad = 1 metro.
        var vista=Camera.main.WorldToViewportPoint(puntoUsuario.Value);
        float x=Mathf.Clamp(vista.x,.16f,.82f),y=Mathf.Clamp(vista.y,.24f,.77f);
        destinoContenedor.anchoredPosition=new Vector2(x*seguro.rect.width,y*seguro.rect.height);
        string etiqueta = mapaGrande.NombreObjetivo ?? "v";
        destinoPantalla.text = $"{etiqueta}\n{Mathf.CeilToInt(metros)} m";
        if (metros < 1.2f) destinoPantalla.text = $"{mapaGrande.NombreObjetivo ?? "Destino"}\nLlegaste";
    }
}
