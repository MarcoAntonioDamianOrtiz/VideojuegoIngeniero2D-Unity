using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the office conversation in the existing level-one dialogue panel.</summary>
public sealed class TechnopolisDialogoOficina : MonoBehaviour
{
    GameObject panel;
    TextMeshProUGUI nombre;
    TextMeshProUGUI texto;
    TextMeshProUGUI indicador;
    Button continuar;
    RawImage retrato;
    Texture2D retratoDonDinero;
    Texture2D retratoDiego;
    readonly Vector3[] esquinasPanel = new Vector3[4];
    Action aceptar;
    Action alternativa;
    bool eligiendo;
    public Canvas CanvasDialogo { get; private set; }

    public void Configurar(Transform dialogoOriginal, MinijuegoAleatorioPanaderia terminal, Texture2D donDinero, Texture2D diego)
    {
        panel = dialogoOriginal.gameObject;
        CanvasDialogo = dialogoOriginal.GetComponentInParent<Canvas>();
        nombre = dialogoOriginal.Find("TextoNombreNPC").GetComponent<TextMeshProUGUI>();
        texto = dialogoOriginal.Find("TextoDialogo").GetComponent<TextMeshProUGUI>();
        indicador = dialogoOriginal.Find("TextoIndicador").GetComponent<TextMeshProUGUI>();
        var imagenObjeto = new GameObject("RetratoPersonaje", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imagenObjeto.transform.SetParent(dialogoOriginal, false);
        retrato = imagenObjeto.GetComponent<RawImage>();
        retratoDonDinero = donDinero;
        retratoDiego = diego;
        retrato.texture = retratoDonDinero;
        retrato.raycastTarget = false;
        if (retratoDonDinero) retratoDonDinero.filterMode = FilterMode.Point;
        if (retratoDiego) retratoDiego.filterMode = FilterMode.Point;
        continuar = panel.GetComponent<Button>();
        if (!continuar) continuar = panel.AddComponent<Button>();
        continuar.transition = Selectable.Transition.None;
        continuar.onClick = new Button.ButtonClickedEvent();
        continuar.onClick.AddListener(ClickPanel);
    }

    public void Mostrar(bool diego, string mensaje, string accion, Action alAceptar, string otra, Action alVolver)
    {
        aceptar = alAceptar;
        alternativa = alVolver;
        eligiendo = !string.IsNullOrEmpty(otra);
        retrato.texture = diego ? retratoDiego : retratoDonDinero;
        retrato.uvRect = new Rect(0f, 0f, 1f, 1f);
        nombre.text = diego ? "Diego Byte" : "Don Dinero";
        texto.text = mensaje;
        indicador.text = eligiendo ? "[1] " + accion + "   /   [2] " + otra
            : "[E] " + accion + "  /  Toca para continuar";
        gameObject.SetActive(true);
        CanvasDialogo.enabled = true;
        panel.SetActive(true);
    }

    void LateUpdate()
    {
        if (!panel || !retrato || !retrato.texture) return;
        RectTransform dialogo = (RectTransform)panel.transform;
        RectTransform canvas = (RectTransform)CanvasDialogo.transform;
        dialogo.GetWorldCorners(esquinasPanel);
        float techo = canvas.InverseTransformPoint(esquinasPanel[1]).y;
        float disponible = Mathf.Max(0f, canvas.rect.yMax - techo - 24f);
        float proporcion = retrato.texture.width / (float)retrato.texture.height;
        float alto = Mathf.Min(260f, disponible, dialogo.rect.width * .32f / proporcion);
        RectTransform rect = retrato.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(20f, 12f);
        rect.sizeDelta = new Vector2(alto * proporcion, alto);
        rect.localScale = Vector3.one;
    }

    void Update()
    {
        if (!panel || !panel.activeInHierarchy || TechnopolisGameFlow.IsMenuOpen) return;
        if (eligiendo)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Elegir(false);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Elegir(true);
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            Elegir(false);
    }

    void ClickPanel()
    {
        if (TechnopolisGameFlow.IsMenuOpen) return;
        bool segunda = false;
        if (eligiendo)
        {
            RectTransform rect = (RectTransform)panel.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out Vector2 local);
            segunda = local.x > rect.rect.center.x;
        }
        Elegir(segunda);
    }

    void Elegir(bool segunda)
    {
        Action accion = segunda ? alternativa : aceptar;
        if (accion == null) return;
        panel.SetActive(false);
        accion();
    }

    void OnDisable()
    {
        if (panel) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (continuar) continuar.onClick.RemoveListener(ClickPanel);
    }
}
