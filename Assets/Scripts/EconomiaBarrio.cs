using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>Único saldo del Nivel 1 y vínculo con el indicador existente.</summary>
public sealed class EconomiaBarrio : MonoBehaviour
{
    const string ClaveDinero = "Technopolis.Level1.Dinero";
    const string ClavePagoPanaderia = "Technopolis.Level1.PanaderiaPagada";
    const int PagoPanaderia = 10000;

    public TMP_Text textoCantidad;

    static EconomiaBarrio instancia;
    int dinero;
    bool panaderiaPagada;

    public int Dinero => dinero;
    public bool PanaderiaPagada => panaderiaPagada;

    void Awake()
    {
        instancia = this;
        dinero = Mathf.Max(0, PlayerPrefs.GetInt(ClaveDinero, 0));
        panaderiaPagada = PlayerPrefs.GetInt(ClavePagoPanaderia, 0) == 1;
        ActualizarTexto();
    }

    void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    public void AgregarDinero(int cantidad)
    {
        if (cantidad <= 0) return;
        dinero = dinero > int.MaxValue - cantidad ? int.MaxValue : dinero + cantidad;
        Guardar();
    }

    public static bool RecibirPagoPanaderia()
    {
        if (!instancia)
            instancia = FindFirstObjectByType<EconomiaBarrio>();
        if (!instancia || instancia.panaderiaPagada) return false;
        instancia.panaderiaPagada = true;
        instancia.AgregarDinero(PagoPanaderia);
        return true;
    }

    public static void ReiniciarPartida()
    {
        PlayerPrefs.DeleteKey(ClaveDinero);
        PlayerPrefs.DeleteKey(ClavePagoPanaderia);
        PlayerPrefs.Save();
        if (!instancia) return;
        instancia.dinero = 0;
        instancia.panaderiaPagada = false;
        instancia.ActualizarTexto();
    }

    void Guardar()
    {
        PlayerPrefs.SetInt(ClaveDinero, dinero);
        PlayerPrefs.SetInt(ClavePagoPanaderia, panaderiaPagada ? 1 : 0);
        PlayerPrefs.Save();
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (textoCantidad)
            textoCantidad.text = "$" + dinero.ToString("N0", CultureInfo.InvariantCulture);
    }
}
