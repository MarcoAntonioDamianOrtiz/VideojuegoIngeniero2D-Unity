using UnityEngine;

public static class ProgresoTechnopolis
{
    const string Prefix = "Technopolis.Progress.";
    const string AccesoNivel2 = Prefix + "AccesoNivel2";
    public static int Reputacion => PlayerPrefs.GetInt(Prefix + "Reputacion", 0);
    public static int Bienestar => PlayerPrefs.GetInt(Prefix + "Bit", 100);
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
        PlayerPrefs.SetInt(Prefix + "Bit", Mathf.Clamp(Bienestar + delta, 0, 100));
        PlayerPrefs.Save();
    }
    public static bool PuedeAvanzar(int siguiente)
    {
        if (siguiente != 2)
            return EconomiaBarrio.Obtener().Dinero >= 20000 && Reputacion >= 60;

        if (PlayerPrefs.GetInt(AccesoNivel2, 0) == 1) return true;

        // En partidas anteriores, el encargo de la panadería ya otorgaba todo lo necesario.
        if (!Completada("panaderia") && (EconomiaBarrio.Obtener().Dinero < 10000 || Reputacion < 20))
            return false;

        PlayerPrefs.SetInt(AccesoNivel2, 1);
        PlayerPrefs.Save();
        return true;
    }
    public static string Requisito(int siguiente) => siguiente == 2 ? "$10,000 y 20 de reputación" : "$20,000 y 60 de reputación";
    public static void Reiniciar()
    {
        PlayerPrefs.DeleteKey(AccesoNivel2);
        foreach (string key in new[] { "Reputacion", "Bit", "Activa", "panaderia", "taller", "inventario", "respaldo", "auditoria", "hueso" })
            PlayerPrefs.DeleteKey(Prefix + key);
        PlayerPrefs.Save();
    }
}
