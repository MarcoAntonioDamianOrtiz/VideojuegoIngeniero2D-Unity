#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Genera una ciudad editable DENTRO de la escena abierta.
/// No crea otra escena y no modifica los sistemas de los niveles anteriores.
/// Unity 6 LTS. Debe guardarse dentro de una carpeta Editor.
/// </summary>
public static class TechnopolisNivel3GeneradorCiudad
{
    private const string NombreRaiz = "N3_MAPA_GENERADO";
    private const int Ancho = 64;
    private const int Alto = 44;

    private static string carpeta;
    private static readonly Dictionary<string, TileBase> cacheTiles = new Dictionary<string, TileBase>();
    private static readonly HashSet<string> avisos = new HashSet<string>();

    [MenuItem("Tools/Technopolis Nivel 3/3 - Generar ciudad en escena actual")]
    public static void GenerarCiudad()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Nivel 3", "Sal del modo Play antes de generar el mapa.", "Aceptar");
            return;
        }

        Scene escena = SceneManager.GetActiveScene();
        if (!escena.IsValid() || !escena.isLoaded || string.IsNullOrEmpty(escena.path))
        {
            EditorUtility.DisplayDialog("Nivel 3", "Guarda primero tu escena vacia dentro de Assets/Scenes (Ctrl + S).", "Aceptar");
            return;
        }

        carpeta = BuscarCarpetaDelPaquete();
        if (carpeta == null || !ExistenRecursos())
        {
            EditorUtility.DisplayDialog("Faltan paletas o prefabs",
                "Antes de generar el mapa, usa:\nTools > Technopolis Nivel 3 > 1 - Crear Tile Palettes y prefabs.\n\n" +
                "Si ya lo hiciste, comprueba que los recursos esten dentro de Assets/Technopolis/nivel3/Generado.",
                "Aceptar");
            return;
        }

        GameObject anterior = BuscarRaizGenerada(escena);
        if (anterior != null)
        {
            bool reemplazar = EditorUtility.DisplayDialog(
                "Regenerar ciudad en esta escena",
                "Ya existe N3_MAPA_GENERADO. Solo se eliminara y creara otra vez ESE grupo.\n\n" +
                "Si moviste edificios o pintaste dentro de ese grupo, perderas esos cambios. " +
                "Tus otros objetos, Grid_Nivel3, Alex, Bit y escenas no se tocaran.\n\n" +
                "Puedes cancelar y duplicar tu escena antes de continuar.",
                "Reemplazar grupo generado", "Cancelar");
            if (!reemplazar) return;
        }
        else
        {
            bool confirmar = EditorUtility.DisplayDialog(
                "Generar Nivel 3 en escena actual",
                "Se agregara N3_MAPA_GENERADO a la escena que tienes abierta:\n" + escena.name +
                "\n\nIncluira terreno 64 x 44, calles, banquetas, plaza, edificios, casas, negocios y senales." +
                "\n\nLos objetos que ya creaste se conservaran.",
                "Generar mapa", "Cancelar");
            if (!confirmar) return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        cacheTiles.Clear();
        avisos.Clear();
        Undo.IncrementCurrentGroup();
        int grupoUndo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Generar mapa Technopolis Nivel 3");

        try
        {
            if (anterior != null) Undo.DestroyObjectImmediate(anterior);

            GameObject raiz = new GameObject(NombreRaiz);
            Undo.RegisterCreatedObjectUndo(raiz, "Generar ciudad Nivel 3");
            GameObject contenedorGrid = Nuevo("Grid_Nivel3_Auto", raiz.transform);
            Grid grid = contenedorGrid.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            grid.cellGap = Vector3.zero;

            Tilemap baseSuelo = CrearTilemap("00_TerrenoBase_Generado", contenedorGrid.transform, 0);
            Tilemap banquetas = CrearTilemap("01_Banquetas_Generadas", contenedorGrid.transform, 1);
            Tilemap calles = CrearTilemap("02_Calles_Generadas", contenedorGrid.transform, 2);
            Tilemap marcas = CrearTilemap("03_MarcasViales_Generadas", contenedorGrid.transform, 3);
            Tilemap plazas = CrearTilemap("04_PlazasYAndadores_Generados", contenedorGrid.transform, 4);

            PintarCiudad(baseSuelo, banquetas, calles, marcas, plazas);

            Transform edificios = Nuevo("05_EdificiosYNegocios", raiz.transform).transform;
            Transform vegetacion = Nuevo("06_ArbolesYJardines", raiz.transform).transform;
            Transform decoracion = Nuevo("07_MobiliarioYServicios", raiz.transform).transform;
            Transform senales = Nuevo("08_SenalesYGuias", raiz.transform).transform;
            Transform casa = Nuevo("09_CasaPrincipal_Alex", raiz.transform).transform;
            Transform puntos = Nuevo("10_PuntosParaJuego", raiz.transform).transform;

            ColocarEdificios(edificios);
            ColocarPlantas(vegetacion);
            ColocarMobiliario(decoracion);
            ColocarSenales(senales);
            ColocarCasa(casa);

            NuevoEnPosicion("SPAWN_ALEX_Y_BIT", puntos, 24f, 3f);
            NuevoEnPosicion("OBJETIVO_PLAZA_CENTRAL", puntos, 33f, 23f);
            NuevoEnPosicion("OBJETIVO_GLOBAL_CORP", puntos, 34f, 34f);
            NuevoEnPosicion("OBJETIVO_SILICON_ALLEY", puntos, 32f, 4f);

            PrepararCamaraSiEstaPorDefecto();
            EditorSceneManager.MarkSceneDirty(escena);
            Undo.CollapseUndoOperations(grupoUndo);
            Selection.activeGameObject = raiz;
            EditorGUIUtility.PingObject(raiz);

            Debug.Log("[Technopolis Nivel 3] Mapa 64 x 44 generado en la escena '" + escena.name +
                "'. Objetos anteriores conservados. Guarda con Ctrl + S.");
            EditorUtility.DisplayDialog("Ciudad del Nivel 3 creada",
                "Listo. Se agrego N3_MAPA_GENERADO a esta escena.\n\n" +
                "Edita sus Tilemaps con Window > 2D > Tile Palette.\n" +
                "Agrega a Alex y Bit desde el Nivel 1.\n" +
                "Revisa las colisiones, la escala de los edificios y la puerta de la casa.\n\n" +
                "IMPORTANTE: Ctrl + S para guardar la escena.", "Aceptar");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("No se pudo completar la ciudad",
                "Revisa el primer error de Console. El script no modifica ningun archivo de escena de forma automatica.\n\n" + e.Message,
                "Aceptar");
        }
    }

    private static string BuscarCarpetaDelPaquete()
    {
        string[] carpetas = { "Assets/Technopolis/nivel3", "Assets/Technopolis_Nivel3" };
        foreach (string ruta in carpetas)
        {
            if (AssetDatabase.IsValidFolder(ruta + "/Generado/Tiles") &&
                AssetDatabase.IsValidFolder(ruta + "/Generado/Prefabs"))
                return ruta;
        }
        return null;
    }

    private static bool ExistenRecursos()
    {
        return Tile("Pasto_01") != null && Tile("Asfalto_01") != null &&
            PrefabExiste("Edificio_Cybernetics_LTD", true) &&
            PrefabExiste("Casa_Principal_Fachada_Ciudad", true);
    }

    private static TileBase Tile(string nombre)
    {
        if (string.IsNullOrEmpty(carpeta)) return null;
        TileBase valor;
        if (cacheTiles.TryGetValue(nombre, out valor)) return valor;
        valor = AssetDatabase.LoadAssetAtPath<TileBase>(carpeta + "/Generado/Tiles/" + nombre + ".asset");
        cacheTiles[nombre] = valor;
        if (valor == null && avisos.Add(nombre))
            Debug.LogWarning("[Technopolis Nivel 3] Falta Tile: " + nombre);
        return valor;
    }

    private static bool PrefabExiste(string nombre, bool edificio)
    {
        string ruta = carpeta + "/Generado/Prefabs/" +
            (edificio ? "Edificios/" : "Decoracion/") + nombre + ".prefab";
        return AssetDatabase.LoadAssetAtPath<GameObject>(ruta) != null;
    }

    private static GameObject BuscarRaizGenerada(Scene escena)
    {
        foreach (GameObject o in escena.GetRootGameObjects())
            if (o.name == NombreRaiz) return o;
        return null;
    }

    private static GameObject Nuevo(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        return objeto;
    }

    private static Transform NuevoEnPosicion(string nombre, Transform padre, float x, float y)
    {
        GameObject objeto = Nuevo(nombre, padre);
        objeto.transform.position = new Vector3(x, y, 0f);
        return objeto.transform;
    }

    private static Tilemap CrearTilemap(string nombre, Transform padre, int orden)
    {
        GameObject go = Nuevo(nombre, padre);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer render = go.AddComponent<TilemapRenderer>();
        render.sortingOrder = orden;
        return tm;
    }

    private static bool Entre(int valor, int min, int max)
    {
        return valor >= min && valor <= max;
    }

    private static bool EsCalle(int x, int y)
    {
        return Entre(x, 17, 21) || Entre(x, 44, 48) ||
               Entre(y, 14, 18) || Entre(y, 27, 31);
    }

    private static bool EsBanqueta(int x, int y)
    {
        return Entre(x, 14, 24) || Entre(x, 41, 51) ||
               Entre(y, 11, 21) || Entre(y, 24, 34);
    }

    private static void PintarCiudad(Tilemap suelo, Tilemap banqueta, Tilemap calle,
        Tilemap marca, Tilemap plaza)
    {
        TileBase[] pastos = { Tile("Pasto_01"), Tile("Pasto_02"), Tile("Pasto_03"), Tile("Pasto_04") };
        TileBase[] asfalto = { Tile("Asfalto_01"), Tile("Asfalto_02"), Tile("Asfalto_03") };
        TileBase[] losetas = { Tile("Banquetas_01"), Tile("Banquetas_02"), Tile("Banquetas_03") };
        TileBase[] piedra = { Tile("Plaza_Piedra_01"), Tile("Plaza_Piedra_02"), Tile("Plaza_Piedra_03") };

        for (int y = 0; y < Alto; y++)
        {
            for (int x = 0; x < Ancho; x++)
            {
                int ruido = Mathf.Abs(x * 79 + y * 139 + (x * y * 7));
                Vector3Int p = new Vector3Int(x, y, 0);
                suelo.SetTile(p, pastos[(ruido / 11) % pastos.Length]);

                if (EsBanqueta(x, y))
                    banqueta.SetTile(p, losetas[(ruido / 29) % losetas.Length]);
                if (EsCalle(x, y))
                    calle.SetTile(p, asfalto[(ruido / 37) % asfalto.Length]);
            }
        }

        // Plaza peatonal principal y corredores hacia ambos lados.
        PintarRectangulo(plaza, "Plaza_Piedra_01", 26, 20, 39, 25);
        PintarRectangulo(plaza, "Banquetas_01", 22, 22, 25, 23);
        PintarRectangulo(plaza, "Banquetas_01", 40, 22, 43, 23);
        // Variantes sutiles de piedra en el centro.
        for (int y = 20; y <= 25; y++)
            for (int x = 26; x <= 39; x++)
                if ((x * 7 + y * 13) % 9 == 0)
                    plaza.SetTile(new Vector3Int(x, y, 0), piedra[(x + y) % piedra.Length]);

        // Andadores de acceso a comercios, casa del protagonista y el parque.
        PintarRectangulo(plaza, "Banquetas_01", 23, 1, 25, 13);
        PintarRectangulo(plaza, "Banquetas_01", 52, 2, 63, 3);
        PintarRectangulo(plaza, "Tierra_Caminos_01", 52, 9, 63, 10);
        PintarRectangulo(plaza, "Banquetas_01", 1, 33, 12, 34);
        PintarRectangulo(plaza, "Banquetas_01", 52, 32, 63, 33);

        // Pasos de peatones en las dos avenidas.
        TileBase cruce = Tile("Cruces_Peatonales_01");
        if (cruce != null)
        {
            foreach (int yy in new int[] { 14, 18, 27, 31 })
            {
                for (int x = 17; x <= 21; x++) marca.SetTile(new Vector3Int(x, yy, 0), cruce);
                for (int x = 44; x <= 48; x++) marca.SetTile(new Vector3Int(x, yy, 0), cruce);
            }
            foreach (int xx in new int[] { 17, 21, 44, 48 })
            {
                for (int y = 14; y <= 18; y++) marca.SetTile(new Vector3Int(xx, y, 0), cruce);
                for (int y = 27; y <= 31; y++) marca.SetTile(new Vector3Int(xx, y, 0), cruce);
            }
        }
        // Algunos tramos con flechas/neon de orientacion.
        TileBase flecha = Tile("Senales_Viales_11");
        if (flecha != null)
        {
            foreach (int x in new int[] { 19, 46 })
                foreach (int y in new int[] { 4, 10, 22, 36, 40 })
                    marca.SetTile(new Vector3Int(x, y, 0), flecha);
        }
    }

    private static void PintarRectangulo(Tilemap tm, string tile, int x0, int y0, int x1, int y1)
    {
        TileBase t = Tile(tile);
        if (t == null) return;
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                tm.SetTile(new Vector3Int(x, y, 0), t);
    }

    private static GameObject Colocar(string nombre, float x, float y, Transform padre,
        bool edificio = false, float escala = -1f)
    {
        string ruta = carpeta + "/Generado/Prefabs/" +
            (edificio ? "Edificios/" : "Decoracion/") + nombre + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
        if (prefab == null)
        {
            if (avisos.Add(nombre)) Debug.LogWarning("[Technopolis Nivel 3] No se encontro prefab: " + ruta);
            return null;
        }
        GameObject instancia = PrefabUtility.InstantiatePrefab(prefab, padre) as GameObject;
        if (instancia == null) return null;
        instancia.name = nombre;
        instancia.transform.position = new Vector3(x, y, 0f);
        if (escala > 0f) instancia.transform.localScale = Vector3.one * escala;
        SpriteRenderer sr = instancia.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = edificio ? 10 : 12;
        return instancia;
    }

    private static void ColocarEdificios(Transform padre)
    {
        // Cuatro edificios iconicos: norte y sur, sin ocupar las avenidas.
        Colocar("Edificio_Cybernetics_LTD", 7, 34, padre, true);
        Colocar("Edificio_Global_Corp", 33, 34, padre, true);
        Colocar("Edificio_Bitforce", 7, 3, padre, true);
        Colocar("Edificio_Silicon_Alley", 33, 4, padre, true);

        // Oficinas, tiendas y establecimientos urbanos.
        Colocar("Edificio_Byteforce_Corp", 39, 34, padre, true);
        Colocar("Edificio_Clinica_Distrito", 55, 34, padre, true);
        Colocar("Edificio_Hardware_Electronica", 61, 34, padre, true);
        Colocar("Edificio_Conveniencia_24_7", 5, 20, padre, true);
        Colocar("Edificio_Servicio_Tecnico", 10, 20, padre, true);
        Colocar("Edificio_Departamentos", 3, 34, padre, true, 1.15f);
        Colocar("Negocio_Fideos", 57, 20, padre, true);
        Colocar("Edificio_Comunicaciones", 61, 20, padre, true);
        Colocar("Casa_Techo_Azul", 3, 3, padre, true);
        Colocar("Casa_Techo_Rojo", 11, 4, padre, true);
        Colocar("Casa_Techo_Gris", 61, 4, padre, true);
    }

    private static void ColocarPlantas(Transform padre)
    {
        foreach (Vector2 p in new Vector2[] {
            new Vector2(2, 29), new Vector2(10, 30), new Vector2(2, 12),
            new Vector2(11, 12), new Vector2(25, 38), new Vector2(41, 39),
            new Vector2(53, 12), new Vector2(61, 12), new Vector2(54, 7),
            new Vector2(52, 39), new Vector2(63, 40) })
            Colocar("Arbol_Verde_Grande", p.x, p.y, padre);

        foreach (Vector2 p in new Vector2[] {
            new Vector2(2, 25), new Vector2(12, 25), new Vector2(24, 25),
            new Vector2(41, 25), new Vector2(51, 22), new Vector2(63, 23) })
            Colocar("Arbol_Rosa_Florido", p.x, p.y, padre);

        foreach (Vector2 p in new Vector2[] {
            new Vector2(27, 20), new Vector2(39, 20), new Vector2(27, 25),
            new Vector2(39, 25), new Vector2(52, 8), new Vector2(62, 8) })
            Colocar("Jardinera_Rosa", p.x, p.y, padre);

        foreach (Vector2 p in new Vector2[] {
            new Vector2(25, 19), new Vector2(42, 20), new Vector2(52, 17),
            new Vector2(12, 35), new Vector2(52, 35) })
            Colocar("Seto_Corto", p.x, p.y, padre);
    }

    private static void ColocarMobiliario(Transform padre)
    {
        Colocar("Fuente_Globo_Holografico", 33, 22, padre);
        Colocar("Parada_Autobus", 23, 31, padre);
        Colocar("Pantalla_Info", 27, 24, padre);
        Colocar("Tablon_Anuncios", 40, 24, padre);
        Colocar("Puesto_Comida", 4, 10, padre);
        Colocar("Estatua_Robot", 58, 9, padre);
        Colocar("Maquina_Expendedora", 52, 20, padre);
        foreach (Vector2 p in new Vector2[] {
            new Vector2(29, 25), new Vector2(37, 25),
            new Vector2(29, 20), new Vector2(37, 20),
            new Vector2(55, 8), new Vector2(61, 8) })
            Colocar("Banco_Madera_01", p.x, p.y, padre);
        foreach (Vector2 p in new Vector2[] {
            new Vector2(14, 21), new Vector2(24, 19),
            new Vector2(41, 21), new Vector2(51, 19),
            new Vector2(24, 33), new Vector2(41, 33) })
            Colocar("Farola_Clasica_01", p.x, p.y, padre);
        Colocar("Papelera_Reciclaje_Azul", 26, 22, padre);
        Colocar("Papelera_Gris", 39, 22, padre);
        Colocar("Paneles_Solares", 58, 39, padre);
        Colocar("Antena_Satelital", 61, 39, padre);
    }

    private static void ColocarSenales(Transform padre)
    {
        Colocar("Senal_Direccional_Grande", 42, 23, padre);
        Colocar("Senal_Azul_Flechas", 25, 17, padre);
        Colocar("Senal_Madera", 52, 11, padre);
        Colocar("Rotulo_Distrito_Silicio", 24, 1, padre);
        Colocar("Rotulo_Nivel_3", 26, 1, padre);
        Colocar("Bandera_Circuito_01", 22, 2, padre);
        Colocar("Bandera_Circuito_02", 27, 2, padre);
    }

    private static void ColocarCasa(Transform padre)
    {
        const float x = 57f;
        const float y = 4f;
        GameObject fachada = Colocar("Casa_Principal_Fachada_Ciudad", x, y, padre, true, 1f);
        GameObject interiorDia = Colocar("Casa_Principal_Interior_Dia", x, y, padre, true, 1f);
        GameObject interiorNoche = Colocar("Casa_Principal_Interior_Noche", x, y, padre, true, 1f);
        if (fachada == null) return;

        // El interior y la fachada son una sola ilustracion; sus colliders de base se
        // desactivan para que el jugador pueda atravesar la puerta. Las paredes
        // interiores deben recibir colisiones ajustadas manualmente al final.
        foreach (GameObject go in new GameObject[] { fachada, interiorDia, interiorNoche })
        {
            if (go == null) continue;
            foreach (Collider2D col in go.GetComponents<Collider2D>()) col.enabled = false;
        }
        if (interiorDia != null) interiorDia.SetActive(false);
        if (interiorNoche != null) interiorNoche.SetActive(false);

        Transform fuera = NuevoEnPosicion("Punto_Fuera", padre, x, y - 1.2f);
        Transform dentro = NuevoEnPosicion("Punto_Dentro", padre, x, y + 1.4f);
        Transform umbralExterior = NuevoEnPosicion("Umbral_Exterior", padre, x, y + 0.2f);
        Transform umbralInterior = NuevoEnPosicion("Umbral_Interior", padre, x, y + 0.7f);
        GameObject zona = Nuevo("Zona_Puerta_Presiona_E", padre);
        zona.transform.position = umbralExterior.position;
        BoxCollider2D trigger = zona.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.2f, 0.9f);

        // Si el script de entrada incluido en el paquete ya esta instalado,
        // enlazamos automaticamente las referencias privadas desde el Editor.
        Type tipoCasa = null;
        foreach (Type t in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
            if (t.Name == "CasaPrincipalInteractiva" && !t.IsAbstract) { tipoCasa = t; break; }
        if (tipoCasa == null)
        {
            Debug.LogWarning("[Nivel 3] Se colocaron los umbrales de la casa, " +
                "pero falta CasaPrincipalInteractiva.cs. Puedes agregarlo despues.");
            return;
        }
        Component controlador = zona.AddComponent(tipoCasa);
        SerializedObject campos = new SerializedObject(controlador);
        Asignar(campos, "fachada", fachada);
        Asignar(campos, "interiorDia", interiorDia);
        Asignar(campos, "interiorNoche", interiorNoche);
        Asignar(campos, "puntoDentro", dentro);
        Asignar(campos, "puntoFuera", fuera);
        Asignar(campos, "umbralExterior", umbralExterior);
        Asignar(campos, "umbralInterior", umbralInterior);
        GameObject bit = GameObject.Find("Bit");
        if (bit != null) Asignar(campos, "bit", bit.transform);
        campos.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Asignar(SerializedObject so, string nombre, UnityEngine.Object obj)
    {
        SerializedProperty p = so.FindProperty(nombre);
        if (p != null) p.objectReferenceValue = obj;
    }

    private static void PrepararCamaraSiEstaPorDefecto()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        // Ajusta solo la camara ortografica recien creada, no camaras configuradas.
        if (cam.orthographic && Mathf.Abs(cam.orthographicSize - 5f) < 0.01f &&
            Mathf.Abs(cam.transform.position.x) < 0.01f &&
            Mathf.Abs(cam.transform.position.y) < 0.01f)
        {
            Undo.RecordObject(cam.transform, "Encuadrar ciudad del Nivel 3");
            Undo.RecordObject(cam, "Encuadrar ciudad del Nivel 3");
            cam.transform.position = new Vector3(32f, 22f, -10f);
            cam.orthographicSize = 25f;
        }
    }
}
#endif
