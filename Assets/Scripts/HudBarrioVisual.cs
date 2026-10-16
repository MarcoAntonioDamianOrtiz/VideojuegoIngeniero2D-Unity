using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class HudBarrioVisual
{
    static readonly Color32 Sombra = new Color32(13, 15, 18, 145);
    static readonly Color32 Madera = new Color32(48, 34, 29, 245);
    static readonly Color32 Bronce = new Color32(174, 122, 69, 255);
    static readonly Color32 Luz = new Color32(226, 177, 102, 255);
    static readonly Color32 Marfil = new Color32(250, 226, 177, 255);

    public static RectTransform ConfigurarPerro(Transform canvas, Image barra, Image rostro, TMP_Text porcentaje)
    {
        var escalador = canvas.GetComponent<CanvasScaler>();
        if (escalador)
        {
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920, 1080);
            escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escalador.matchWidthOrHeight = .5f;
        }

        var fondoAnterior = canvas.Find("Barra_Fondo");
        if (fondoAnterior) fondoAnterior.gameObject.SetActive(false);

        var sombra = Panel("Sombra_Bit", canvas, Sombra, new Vector2(29, -29), new Vector2(392, 100));
        sombra.SetAsFirstSibling();
        var marco = Panel("HUD_Bit", canvas, Bronce, new Vector2(24, -24), new Vector2(388, 96));
        marco.SetSiblingIndex(1);
        var interior = Panel("Madera_Bit", marco, Madera, new Vector2(4, -4), new Vector2(380, 88));
        Panel("Luz_Marco", interior, Luz, new Vector2(2, -2), new Vector2(374, 3));

        Panel("Recuadro_Rostro", interior, Bronce, new Vector2(11, -12), new Vector2(72, 68));
        Colocar((RectTransform)rostro.transform, interior, new Vector2(17, -17), new Vector2(60, 56));
        rostro.preserveAspect = true;
        rostro.raycastTarget = false;

        var titulo = Texto("BIT  ·  ÁNIMO", interior, new Vector2(96, -9), new Vector2(200, 29), 21);
        titulo.alignment = TextAlignmentOptions.MidlineLeft;

        Colocar((RectTransform)porcentaje.transform, interior, new Vector2(298, -9), new Vector2(74, 29));
        porcentaje.fontSize = 22;
        porcentaje.fontStyle = FontStyles.Bold;
        porcentaje.alignment = TextAlignmentOptions.MidlineRight;
        porcentaje.color = Marfil;
        porcentaje.raycastTarget = false;

        var borde = Panel("Borde_Animo", interior, Bronce, new Vector2(96, -50), new Vector2(276, 25));
        var canal = Panel("Canal_Animo", borde, new Color32(26, 29, 29, 255), new Vector2(4, -4), new Vector2(268, 17));
        var relleno = (RectTransform)barra.transform;
        relleno.SetParent(canal, false);
        relleno.anchorMin = Vector2.zero;
        relleno.anchorMax = Vector2.one;
        relleno.pivot = new Vector2(0, .5f);
        relleno.offsetMin = relleno.offsetMax = Vector2.zero;
        relleno.localScale = Vector3.one;
        barra.sprite = null;
        barra.type = Image.Type.Simple;
        barra.raycastTarget = false;

        for (int i = 1; i < 4; i++)
        {
            var marca = Panel("Marca_" + i, canal, new Color(0, 0, 0, .22f), Vector2.zero, new Vector2(2, 17));
            marca.anchorMin = marca.anchorMax = new Vector2(i / 4f, .5f);
            marca.pivot = new Vector2(.5f, .5f);
            marca.anchoredPosition = Vector2.zero;
        }
        return relleno;
    }

    public static void ConfigurarDinero()
    {
        var canvasObjeto = GameObject.Find("Canvas_Dinero");
        if (!canvasObjeto) return;
        var canvas = canvasObjeto.transform;
        var billete = canvas.Find("Icono_billete") as RectTransform;
        var titulo = canvas.Find("Texto_Dinero") as RectTransform;
        RectTransform cantidad = null;
        foreach (Transform hijo in canvas)
            if (hijo.name.Trim().StartsWith("Texto_Cantidad")) cantidad = hijo as RectTransform;
        if (!billete || !titulo || !cantidad) return;

        bool vertical = Screen.height > Screen.width;
        float y = vertical ? -126 : -24;
        var sombra = Panel("Sombra_Dinero", canvas, Sombra, new Vector2(-25, y - 5), new Vector2(316, 98));
        sombra.anchorMin = sombra.anchorMax = sombra.pivot = Vector2.one;
        sombra.anchoredPosition = new Vector2(-25, y - 5);
        sombra.SetAsFirstSibling();
        var marco = Panel("HUD_Dinero", canvas, Bronce, new Vector2(-29, y), new Vector2(310, 92));
        marco.anchorMin = marco.anchorMax = marco.pivot = Vector2.one;
        marco.anchoredPosition = new Vector2(-29, y);
        marco.SetSiblingIndex(1);
        var interior = Panel("Madera_Dinero", marco, Madera, new Vector2(4, -4), new Vector2(302, 84));
        Panel("Luz_Marco", interior, Luz, new Vector2(2, -2), new Vector2(296, 3));

        Colocar(billete, interior, new Vector2(10, -10), new Vector2(101, 64));
        var imagenBillete = billete.GetComponent<Image>();
        if (imagenBillete)
        {
            imagenBillete.preserveAspect = true;
            imagenBillete.raycastTarget = false;
        }
        Colocar(titulo, interior, new Vector2(122, -9), new Vector2(167, 29));
        var textoTitulo = titulo.GetComponent<TMP_Text>();
        if (textoTitulo)
        {
            textoTitulo.text = "DINERO";
            textoTitulo.fontSize = 20;
            textoTitulo.fontStyle = FontStyles.Bold;
            textoTitulo.alignment = TextAlignmentOptions.MidlineLeft;
            textoTitulo.color = Marfil;
            textoTitulo.raycastTarget = false;
        }
        Colocar(cantidad, interior, new Vector2(122, -38), new Vector2(167, 37));
        var textoCantidad = cantidad.GetComponent<TMP_Text>();
        if (textoCantidad)
        {
            textoCantidad.fontSize = 31;
            textoCantidad.fontStyle = FontStyles.Bold;
            textoCantidad.alignment = TextAlignmentOptions.MidlineLeft;
            textoCantidad.color = Luz;
            textoCantidad.raycastTarget = false;
        }
    }

    static RectTransform Panel(string nombre, Transform padre, Color color, Vector2 posicion, Vector2 tamano)
    {
        var objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        var rect = objeto.GetComponent<RectTransform>();
        Colocar(rect, padre, posicion, tamano);
        var imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return rect;
    }

    static TextMeshProUGUI Texto(string contenido, Transform padre, Vector2 posicion, Vector2 tamano, float fuente)
    {
        var objeto = new GameObject("Etiqueta_Bit", typeof(RectTransform), typeof(TextMeshProUGUI));
        var texto = objeto.GetComponent<TextMeshProUGUI>();
        Colocar(texto.rectTransform, padre, posicion, tamano);
        texto.text = contenido;
        texto.fontSize = fuente;
        texto.fontStyle = FontStyles.Bold;
        texto.color = Marfil;
        texto.raycastTarget = false;
        return texto;
    }

    static void Colocar(RectTransform rect, Transform padre, Vector2 posicion, Vector2 tamano)
    {
        rect.SetParent(padre, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
        rect.localScale = Vector3.one;
    }
}
