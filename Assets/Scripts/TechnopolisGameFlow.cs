using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Loads the visual menu over the level and keeps the small level-one save.</summary>
public sealed class TechnopolisGameFlow : MonoBehaviour
{
    public const string LevelScene = "EscenaNivel1_FielReferencia";
    public const string MenuScene = "ESCENA NUEVA";

    const string Nivel2 = "EscenaNivel2Taller";
    const string Nivel3 = "EscenaNivel3Silicio";
    const string NivelGuardado = "Technopolis.Campaign.Scene";
    static string currentLevel = LevelScene;

    public static void AvanzarCampana(int numero)
    {
        string siguiente = numero == 2 ? Nivel2 : Nivel3;
        startWithoutMenu = true;
        restoreOnLoad = false;
        currentLevel = siguiente;
        SceneManager.LoadScene(siguiente, LoadSceneMode.Single);
    }
    static string SavePrefix => currentLevel == Nivel3 ? "Technopolis.Level3." : currentLevel == Nivel2 ? "Technopolis.Level2." : "Technopolis.Level1.";
    static string SaveVersion => SavePrefix + "Version";
    static string SaveX => SavePrefix + "PlayerX";
    static string SaveY => SavePrefix + "PlayerY";
    const float SaveInterval = 10f;

    static TechnopolisGameFlow instance;
    static Transform player;
    static Vector3 startingPosition;
    static bool sessionStarted;
    static bool menuOpen;
    static bool firstLevelLoad;
    static bool startWithoutMenu;
    static bool restoreOnLoad;

    float saveElapsed;

    public static bool HasSavedGame => PlayerPrefs.GetInt(SaveVersion, 0) == 1;
    public static bool CanContinue => sessionStarted || HasSavedGame || PlayerPrefs.HasKey(NivelGuardado);
    public static bool IsMenuOpen => menuOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        currentLevel = LevelScene;
        player = null;
        instance = null;
        sessionStarted = false;
        menuOpen = false;
        firstLevelLoad = true;
        startWithoutMenu = false;
        restoreOnLoad = false;
        Time.timeScale = 1f;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == LevelScene || scene.name == Nivel2 || scene.name == Nivel3)
        {
            currentLevel = scene.name;
            GameObject found = GameObject.Find("Jugador");
            player = found ? found.transform : null;
            if (player) startingPosition = player.position;
            EnsureHost();
            if (scene.name != LevelScene && found && found.TryGetComponent<MovimientoJugador>(out var movimiento))
            {
                var campana = new GameObject("Objetivos de campaña");
                SceneManager.MoveGameObjectToScene(campana, scene);
                campana.AddComponent<TechnopolisCampana>().Configurar(scene.name == Nivel3 ? 3 : 2, movimiento);
                MusicaBarrio.IniciarSiFalta();
            }

            if (restoreOnLoad)
            {
                RestorePosition();
                restoreOnLoad = false;
            }

            if (startWithoutMenu)
            {
                startWithoutMenu = false;
                firstLevelLoad = false;
                sessionStarted = true;
                menuOpen = false;
                Time.timeScale = 1f;
                SavePosition();
            }
            else if (firstLevelLoad)
            {
                firstLevelLoad = false;
                ShowMenu();
            }
        }
        else if (scene.name == MenuScene)
        {
            menuOpen = true;
            Time.timeScale = 0f;
        }
    }

    static void EnsureHost()
    {
        if (instance) return;
        GameObject host = new GameObject("TechnopolisGameFlow");
        instance = host.AddComponent<TechnopolisGameFlow>();
        DontDestroyOnLoad(host);
    }

    public static void StartNewGame()
    {
        EconomiaBarrio.ReiniciarPartida();
        PlayerPrefs.DeleteKey(NivelGuardado);
        ClearSave();
        if (sessionStarted || !SceneManager.GetSceneByName(currentLevel).isLoaded)
        {
            startWithoutMenu = true;
            restoreOnLoad = false;
            menuOpen = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(currentLevel, LoadSceneMode.Single);
            return;
        }

        MovePlayer(startingPosition);
        sessionStarted = true;
        CloseMenu();
    }

    public static void ContinueGame()
    {
        if (sessionStarted)
        {
            CloseMenu();
            return;
        }
        string guardado = PlayerPrefs.GetString(NivelGuardado, currentLevel);
        if (guardado == LevelScene || guardado == Nivel2 || guardado == Nivel3)
            currentLevel = guardado;
        if (!HasSavedGame) return;

        if (SceneManager.GetSceneByName(currentLevel).isLoaded)
        {
            RestorePosition();
            sessionStarted = true;
            CloseMenu();
        }
        else
        {
            restoreOnLoad = true;
            startWithoutMenu = true;
            menuOpen = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(currentLevel, LoadSceneMode.Single);
        }
    }

    public static void ReturnToMenu()
    {
        if (sessionStarted) SavePosition();
        ShowMenu();
    }

    static void ShowMenu()
    {
        if (menuOpen) return;
        menuOpen = true;
        Time.timeScale = 0f;
        Scene menu = SceneManager.GetSceneByName(MenuScene);
        if (!menu.isLoaded) SceneManager.LoadSceneAsync(MenuScene, LoadSceneMode.Additive);
    }

    static void CloseMenu()
    {
        menuOpen = false;
        Time.timeScale = 1f;
        Scene level = SceneManager.GetSceneByName(currentLevel);
        if (level.isLoaded) SceneManager.SetActiveScene(level);
        Scene menu = SceneManager.GetSceneByName(MenuScene);
        if (menu.isLoaded) SceneManager.UnloadSceneAsync(menu);
    }

    static void RestorePosition()
    {
        if (!HasSavedGame || !player) return;
        float x = PlayerPrefs.GetFloat(SaveX, startingPosition.x);
        float y = PlayerPrefs.GetFloat(SaveY, startingPosition.y);
        if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y)) return;
        MovePlayer(new Vector3(x, y, player.position.z));
    }

    static void MovePlayer(Vector3 position)
    {
        if (!player) return;
        player.position = position;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body) body.position = position;
        Physics2D.SyncTransforms();
    }

    static void SavePosition()
    {
        if (!sessionStarted || !player) return;
        PlayerPrefs.SetFloat(SaveX, player.position.x);
        PlayerPrefs.SetFloat(SaveY, player.position.y);
        PlayerPrefs.SetInt(SaveVersion, 1);
        PlayerPrefs.SetString(NivelGuardado, currentLevel);
        PlayerPrefs.Save();
    }

    static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveVersion);
        PlayerPrefs.DeleteKey(SaveX);
        PlayerPrefs.DeleteKey(SaveY);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (!sessionStarted || menuOpen) return;

        bool open = Input.GetKeyDown(KeyCode.Escape);
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        open |= pad != null && pad.startButton.wasPressedThisFrame;
#endif
        if (open)
        {
            ReturnToMenu();
            return;
        }

        saveElapsed += Time.unscaledDeltaTime;
        if (saveElapsed < SaveInterval) return;
        saveElapsed = 0f;
        SavePosition();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SavePosition();
    }

    void OnApplicationQuit()
    {
        SavePosition();
        Time.timeScale = 1f;
    }
}
