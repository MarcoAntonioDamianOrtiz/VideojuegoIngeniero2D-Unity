#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
static class AlinearBibliotecaEnEditor
{
    static AlinearBibliotecaEnEditor()
    {
        EditorApplication.delayCall += Revisar;
    }

    static void Revisar()
    {
        if (Application.isPlaying) return;
        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != "Assets/Scenes/EscenaNivel1_FielReferencia.unity") return;
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (Transform objeto in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (objeto.name != "Biblioteca_Barrio") continue;
                Transform fachada = objeto.Find("Fachada_Sprite");
                if (fachada && fachada.localPosition.sqrMagnitude > .0001f)
                    InstalarBibliotecaBarrio.AjustarColision();
                return;
            }
        }
    }
}
#endif
