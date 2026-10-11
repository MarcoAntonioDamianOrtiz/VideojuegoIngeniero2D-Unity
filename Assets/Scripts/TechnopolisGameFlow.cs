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

    const string SavePrefix = "Technopolis.Level1.";
    const string SaveVersion = SavePrefix + "Version";
    const string SaveX = SavePrefix + "PlayerX";
    const string SaveY = SavePrefix + "PlayerY";
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
    public static bool CanContinue => sessionStarted || HasSavedGame;
    public static bool IsMenuOpen => menuOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (scene.name == LevelScene)
        {
            GameObject found = GameObject.Find("Jugador");
            player = found ? found.transform : null;
            if (player) startingPosition = player.position;
            EnsureHost();

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
        ClearSave();
        if (sessionStarted || !SceneManager.GetSceneByName(LevelScene).isLoaded)
        {
            startWithoutMenu = true;
            restoreOnLoad = false;
            menuOpen = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(LevelScene, LoadSceneMode.Single);
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
        if (!HasSavedGame) return;

        if (SceneManager.GetSceneByName(LevelScene).isLoaded)
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
            SceneManager.LoadScene(LevelScene, LoadSceneMode.Single);
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
        Scene level = SceneManager.GetSceneByName(LevelScene);
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
