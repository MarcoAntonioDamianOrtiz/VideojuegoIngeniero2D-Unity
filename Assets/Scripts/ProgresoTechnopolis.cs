using UnityEngine;

public static class ProgresoTechnopolis
{
    const string Prefix = "Technopolis.Progress.";
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
    public static bool PuedeAvanzar(int siguiente) => EconomiaBarrio.Obtener().Dinero >= (siguiente == 2 ? 10000 : 20000) && Reputacion >= (siguiente == 2 ? 20 : 60);
    public static string Requisito(int siguiente) => siguiente == 2 ? "$10,000 y 20 de reputación" : "$20,000 y 60 de reputación";
    public static void Reiniciar()
    {
        foreach (string key in new[] { "Reputacion", "Bit", "Activa", "panaderia", "taller", "inventario", "respaldo", "auditoria", "hueso" })
            PlayerPrefs.DeleteKey(Prefix + key);
        PlayerPrefs.Save();
    }
}
