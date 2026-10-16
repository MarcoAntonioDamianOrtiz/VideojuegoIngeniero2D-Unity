#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class InstalarBibliotecaBarrio
{
    const string Escena = "Assets/Scenes/EscenaNivel1_FielReferencia.unity";
    const string SpritePath = "Assets/Technopolis/Nivel1/Sprites/05_Edificios/biblioteca_barrio.png";
    const string AudioFolder = "Assets/Technopolis/Biblioteca/Audio/";

    static readonly string[] Datos =
    {
        "001|¿Kenin nonkatej axxan noyolikniwan?|¿Cómo están mis amigos?",
        "002|¿Kenin nonkatej axxan?|¿Cómo están?",
        "003|¿Mitspaktia?|¿Te gusta?",
        "004|¿Mitspaktia timijtotis?|¿Te gusta bailar?",
        "005|¿Mitspaktia yolkalmej?|¿Te gustan los animales?",
        "006|Axxan nikuikas|Hoy cantaré",
        "016|Ken moijtoa ika nawatlajtolikuali yowal|¿Cómo se dice buenas noches?",
        "017|Ikon moijtoa|Así se dice",
        "026|Xiuala nikan|Ven aquí",
        "032|Ma kuali xiualakan|Sean bienvenidos",
        "045|Yohualtzino|Buenas noches",
        "046|Kuali tiotlaquiltzin noyolicnihuan|Buenas tardes amigos",
        "047|Neh nimotoca|Me llamo",
        "048|Kuali tiotlaquiltzin noyolicnihuan neh nimotoca neh nitemachtic|Buenas tardes amigos, me llamo Daniel y soy maestro",
        "049|Quenon tomixtonaltitzino, kuali tlazohcamati|Buenos días, bien gracias.",
        "050|Tiotlactilzio|Buenas tardes",
        "051|Neh nitemachtic|Soy maestro",
        "052|Quenon tomixtonalti kuali, tlazohcamati|Buenos días, gracias.",
        "053|Kuali tiotlaquiltzin|Buenas tardes",
        "054|Kuali Yohualzintli|Buenas noches",
        "055|Quenon tomixtonaltitzino|Buenos días, ¿Cómo está usted?"
    };

    [MenuItem("Technopolis/Biblioteca/Ajustar colisión a fachada movida")]
    public static void AjustarColision()
    {
        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != Escena)
        {
            Debug.LogError("Abre primero EscenaNivel1_FielReferencia para ajustar la biblioteca.");
            return;
        }
        Transform biblioteca = Buscar(escena, "Biblioteca_Barrio");
        if (!biblioteca) { Debug.LogError("No se encontró Biblioteca_Barrio."); return; }
        Transform fachada = biblioteca.Find("Fachada_Sprite");
        if (!fachada) { Debug.LogError("No se encontró la fachada de la biblioteca."); return; }
        Vector3 lugarVisible = fachada.position;
        Undo.RecordObject(biblioteca, "Alinear colisión de biblioteca");
        Undo.RecordObject(fachada, "Centrar fachada de biblioteca");
        biblioteca.position = lugarVisible;
        fachada.localPosition = Vector3.zero;
        var render = fachada.GetComponent<SpriteRenderer>();
        if (render) render.sortingOrder = TechnopolisOrdenBarrio.Orden(biblioteca.position.y);
        Physics2D.SyncTransforms();
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = biblioteca.gameObject;
        Debug.Log("Biblioteca: fachada, colisión y acceso alineados en " + lugarVisible + ". Escena guardada.");
    }

    [MenuItem("Technopolis/Biblioteca/Instalar en Nivel 1")]
    public static void Instalar()
    {
        AssetDatabase.Refresh();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (!sprite) { Debug.LogError("Falta el sprite de la biblioteca: " + SpritePath); return; }
        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != Escena) escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);

        Transform edificios = Buscar(escena, "Edificios_Sprites");
        Transform casa = Buscar(escena, "Vivienda_36");
        if (!casa) casa = Buscar(escena, "Vivienda_36_Reemplazada_por_Biblioteca");
        Transform jugador = Buscar(escena, "Jugador");
        if (!edificios || !casa || !jugador)
        {
            Debug.LogError("No se encontraron los objetos base del nivel para instalar la biblioteca.");
            return;
        }
        Transform anterior = Buscar(escena, "Biblioteca_Barrio");
        if (anterior) Object.DestroyImmediate(anterior.gameObject);

        Vector3 lugar = casa.position;
        casa.gameObject.SetActive(false);
        casa.name = "Vivienda_36_Reemplazada_por_Biblioteca";

        var biblioteca = new GameObject("Biblioteca_Barrio");
        biblioteca.transform.SetParent(edificios, true);
        biblioteca.transform.position = lugar;

        var fachada = new GameObject("Fachada_Sprite");
        fachada.transform.SetParent(biblioteca.transform, false);
        SpriteRenderer render = fachada.AddComponent<SpriteRenderer>();
        render.sprite = sprite;
        render.spriteSortPoint = SpriteSortPoint.Pivot;
        render.sortingOrder = TechnopolisOrdenBarrio.Orden(lugar.y);
        float escala = 5.25f / sprite.bounds.size.x;
        fachada.transform.localScale = new Vector3(escala, escala, 1);

        var baseFisica = biblioteca.AddComponent<BoxCollider2D>();
        baseFisica.size = new Vector2(4.25f, .8f);
        baseFisica.offset = new Vector2(0, .36f);

        var puerta = new GameObject("Acceso_Lectura_E");
        puerta.transform.SetParent(biblioteca.transform, false);
        puerta.transform.localPosition = new Vector3(0, -1.02f, 0);
        var zona = puerta.AddComponent<BoxCollider2D>();
        zona.isTrigger = true;
        zona.size = new Vector2(3.2f, 1.9f);
        var audioSource = puerta.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0;
        var interaccion = puerta.AddComponent<BibliotecaInteractiva>();
        interaccion.movimientoJugador = jugador.GetComponent<MovimientoJugador>();
        interaccion.entradas = new BibliotecaInteractiva.Entrada[Datos.Length];
        for (int i = 0; i < Datos.Length; i++)
        {
            string[] p = Datos[i].Split('|');
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "Audio-" + p[0] + "-2026.mp3");
            if (!clip) Debug.LogError("Falta audio de biblioteca: " + p[0]);
            interaccion.entradas[i] = new BibliotecaInteractiva.Entrada
            {
                numero = p[0], nahuatl = p[1], espanol = p[2], audio = clip
            };
        }

        var letrero = new GameObject("Rotulo_BIBLIOTECA");
        letrero.transform.SetParent(biblioteca.transform, false);
        letrero.transform.localPosition = new Vector3(-.53f, 2.05f, 0);
        letrero.transform.localScale = new Vector3(.16f, .16f, 1);
        var texto = letrero.AddComponent<TextMeshPro>();
        texto.text = "BIBLIOTECA";
        texto.fontSize = 2.6f;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color32(249, 218, 157, 255);
        texto.rectTransform.sizeDelta = new Vector2(19f, 3f);
        texto.GetComponent<MeshRenderer>().sortingOrder = render.sortingOrder + 1;

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = biblioteca;
        Debug.Log("Biblioteca instalada junto al parque con " + Datos.Length + " entradas y audios. Escena guardada.");
    }

    static Transform Buscar(Scene escena, string nombre)
    {
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            Transform encontrado = Buscar(raiz.transform, nombre);
            if (encontrado) return encontrado;
        }
        return null;
    }

    static Transform Buscar(Transform actual, string nombre)
    {
        if (actual.name == nombre) return actual;
        foreach (Transform hijo in actual)
        {
            Transform encontrado = Buscar(hijo, nombre);
            if (encontrado) return encontrado;
        }
        return null;
    }
}
#endif
