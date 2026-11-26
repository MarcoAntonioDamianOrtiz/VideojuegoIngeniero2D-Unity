using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>Único saldo del Nivel 1 y vínculo con el indicador existente.</summary>
public sealed class EconomiaBarrio : MonoBehaviour
{
    const string ClaveDinero = "Technopolis.Level1.Dinero";
    const string ClavePagoPanaderia = "Technopolis.Level1.PanaderiaPagada";
    internal const int PagoPanaderia = 10000;
    const string ClaveGanancias = "Technopolis.Campaign.Ganancias";
    int ganancias;
    public int Ganancias => ganancias;
    public static bool PanaderiaPagadaEnGuardado => PlayerPrefs.GetInt(ClavePagoPanaderia, 0) == 1;

    public TMP_Text textoCantidad;

    static EconomiaBarrio instancia;
    int dinero;
    bool panaderiaPagada;

    public int Dinero => dinero;
    public bool PanaderiaPagada => panaderiaPagada;

    public static EconomiaBarrio Obtener()
    {
        if (!instancia) instancia = FindFirstObjectByType<EconomiaBarrio>();
        if (!instancia) instancia = new GameObject("EconomiaBarrio").AddComponent<EconomiaBarrio>();
        return instancia;
    }

    public bool Gastar(int cantidad)
    {
        if (cantidad <= 0 || dinero < cantidad) return false;
        dinero -= cantidad;
        Guardar();
        return true;
    }

    void Awake()
    {
        instancia = this;
        dinero = Mathf.Max(0, PlayerPrefs.GetInt(ClaveDinero, 0));
        panaderiaPagada = PanaderiaPagadaEnGuardado;
        ganancias = PlayerPrefs.GetInt(ClaveGanancias, Mathf.Max(dinero, ProgresoTechnopolis.GananciasPrevias));
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
        ganancias = (int)System.Math.Min((long)ganancias + cantidad, int.MaxValue);
        Guardar();
    }

    public static bool RecibirPagoPanaderia()
    {
        if (!instancia)
            instancia = FindFirstObjectByType<EconomiaBarrio>();
        if (!instancia || instancia.panaderiaPagada) return false;
        instancia.panaderiaPagada = true;
        instancia.AgregarDinero(PagoPanaderia);
        ProgresoTechnopolis.Premiar("panaderia", 0, 20);
        return true;
    }

    public static void ReiniciarPartida()
    {
        ProgresoTechnopolis.Reiniciar();
        PlayerPrefs.DeleteKey(ClaveDinero);
        PlayerPrefs.DeleteKey(ClavePagoPanaderia);
        PlayerPrefs.DeleteKey(ClaveGanancias);
        PlayerPrefs.Save();
        if (!instancia) return;
        instancia.dinero = 0;
        instancia.ganancias = 0;
        instancia.panaderiaPagada = false;
        instancia.ActualizarTexto();
    }

    void Guardar()
    {
        PlayerPrefs.SetInt(ClaveDinero, dinero);
        PlayerPrefs.SetInt(ClaveGanancias, ganancias);
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
