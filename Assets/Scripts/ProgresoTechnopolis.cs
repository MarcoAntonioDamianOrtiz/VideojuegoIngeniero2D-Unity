using UnityEngine;

public static class ProgresoTechnopolis
{
    const string Prefix = "Technopolis.Progress.";
    const string AccesoNivel2 = Prefix + "AccesoNivel2";
    public static int Reputacion => PlayerPrefs.GetInt(Prefix + "Reputacion", 0);
    public static int Bienestar => PlayerPrefs.GetInt(Prefix + "Bit", 100);
    public const int PrecioVeterinario = 1500;
    public const int PrecioCroquetas = 250;
    public const int MaxCroquetas = 3;
    public const int PagoTaller = 5000;
    public const int PagoEncargo = 6000;
    public static int Croquetas => PlayerPrefs.GetInt(Prefix + "Croquetas", 0);
    public static bool BitComioCroquetas => PlayerPrefs.GetInt(Prefix + "BitComioCroquetas", 0) == 1;
    public static bool BitEnfermo => PlayerPrefs.GetInt(Prefix + "SaludBit", 0) == 1;
    public static bool BitCurado => PlayerPrefs.GetInt(Prefix + "SaludBit", 0) == 2;
    public static int GananciasPrevias =>
        (EconomiaBarrio.PanaderiaPagadaEnGuardado ? EconomiaBarrio.PagoPanaderia : 0) +
        (Completada("taller") ? PagoTaller : 0) +
        (Completada("inventario") ? PagoEncargo : 0) +
        (Completada("respaldo") ? PagoEncargo : 0);

    public static bool EnfermarBitEnNivel2()
    {
        if (BitCurado || BitEnfermo) return false;
        PlayerPrefs.SetInt(Prefix + "SaludBit", 1);
        CuidarBit(0);
        return true;
    }

    public static bool CurarBit()
    {
        if (!BitEnfermo || !EconomiaBarrio.Obtener().Gastar(PrecioVeterinario)) return false;
        PlayerPrefs.SetInt(Prefix + "SaludBit", 2);
        CuidarBit(65 - Bienestar);
        return true;
    }

    public static bool ComprarCroquetas()
    {
        if (Croquetas >= MaxCroquetas || !EconomiaBarrio.Obtener().Gastar(PrecioCroquetas)) return false;
        PlayerPrefs.SetInt(Prefix + "Croquetas", Croquetas + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool DarCroquetas()
    {
        if (BitEnfermo || Croquetas <= 0 || Bienestar >= 100) return false;
        PlayerPrefs.SetInt(Prefix + "Croquetas", Croquetas - 1);
        PlayerPrefs.SetInt(Prefix + "BitComioCroquetas", 1);
        CuidarBit(30);
        return true;
    }

    // 0: sin invitacion, 1: visita elegida, 2: continuar al siguiente distrito.
    public static int VisitaBiblioteca
    {
        get => PlayerPrefs.GetInt(Prefix + "VisitaBiblioteca", 0);
        set
        {
            PlayerPrefs.SetInt(Prefix + "VisitaBiblioteca", value);
            PlayerPrefs.Save();
        }
    }

    public static bool Completada(string id) => PlayerPrefs.GetInt(Prefix + id, 0) == 1;
    public static string Activa { get => PlayerPrefs.GetString(Prefix + "Activa", ""); set { PlayerPrefs.SetString(Prefix + "Activa", value); PlayerPrefs.Save(); } }
    public static bool Premiar(string id, int dinero, int reputacion)
    {
        if (Completada(id)) return false;
        EconomiaBarrio.Obtener().AgregarDinero(dinero);
        PlayerPrefs.SetInt(Prefix + id, 1);
        PlayerPrefs.SetInt(Prefix + "Reputacion", Mathf.Clamp(Reputacion + reputacion, 0, 100));
        if (Activa == id) Activa = "";
        PlayerPrefs.Save();
        return true;
    }
    public static void CuidarBit(int delta)
    {
        PlayerPrefs.SetInt(Prefix + "Bit", Mathf.Clamp(Bienestar + delta, 0, BitEnfermo ? 20 : 100));
        PlayerPrefs.Save();
    }
    public static bool PuedeAvanzar(int siguiente)
    {
        if (siguiente != 2)
            return BitCurado && EconomiaBarrio.Obtener().Ganancias >= 20000 && Reputacion >= 60;

        if (PlayerPrefs.GetInt(AccesoNivel2, 0) == 1) return true;

        // En partidas anteriores, el encargo de la panadería ya otorgaba todo lo necesario.
        if (!Completada("panaderia") && (EconomiaBarrio.Obtener().Dinero < 10000 || Reputacion < 20))
            return false;

        PlayerPrefs.SetInt(AccesoNivel2, 1);
        PlayerPrefs.Save();
        return true;
    }
    public static string Requisito(int siguiente)
    {
        if (siguiente == 2) return "$10,000 y 20 de reputación";
        return BitCurado ? "$20,000 ganados y 60 de reputación" : "curar a Bit en la veterinaria";
    }
    public static void Reiniciar()
    {
        PlayerPrefs.DeleteKey(AccesoNivel2);
        PlayerPrefs.DeleteKey(Prefix + "VisitaBiblioteca");
        PlayerPrefs.DeleteKey(Prefix + "BitComioCroquetas");
        foreach (string key in new[] { "Reputacion", "Bit", "Activa", "panaderia", "taller", "inventario", "respaldo", "auditoria", "hueso", "Croquetas", "SaludBit" })
            PlayerPrefs.DeleteKey(Prefix + key);
        PlayerPrefs.Save();
    }
}
