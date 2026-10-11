using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Input and actions for the editable menu Canvas.</summary>
public sealed class TechnopolisMenuRuntime : MonoBehaviour
{
    enum Page { Main, Options, Credits }

    const string VolumeKey = "Technopolis.Menu.Volume";
    const string FullscreenKey = "Technopolis.Menu.Fullscreen";

    public GameObject mainPanel;
    public RectTransform[] mainRows;
    public Text[] mainLabels;
    public Image selectionBackground;
    public Image selectionArrow;

    public GameObject optionsPanel;
    public RectTransform[] optionsRows;
    public Text[] optionsLabels;
    public Text volumeLabel;
    public Text fullscreenLabel;

    public GameObject creditsPanel;
    public RectTransform creditsBackRow;
    public Text creditsBackLabel;

    Page page;
    int selected;
    float nextStickMove;
    Vector2 lastMousePosition;

    void Awake()
    {
        if (mainRows == null || mainRows.Length != 5 || mainLabels == null || mainLabels.Length != 5 ||
            optionsRows == null || optionsRows.Length != 3 || optionsLabels == null || optionsLabels.Length != 3 ||
            !mainPanel || !optionsPanel || !creditsPanel || !selectionBackground || !selectionArrow)
        {
            Debug.LogError("La escena del menú no tiene todas sus referencias visuales.", this);
            enabled = false;
            return;
        }

        int volume = Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 100), 0, 100);
        AudioListener.volume = volume / 100f;
        if (PlayerPrefs.HasKey(FullscreenKey))
            Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        lastMousePosition = Input.mousePosition;
        OpenMain();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace) || ControllerBack())
        {
            if (page != Page.Main) OpenMain();
            return;
        }

        int movement = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) movement = -1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) movement = 1;
        movement = ControllerVertical(movement);
        if (movement != 0) MoveSelection(movement);

        if (page == Page.Options)
        {
            int horizontal = 0;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) horizontal = -1;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) horizontal = 1;
            horizontal = ControllerHorizontal(horizontal);
            if (horizontal != 0) ChangeOption(horizontal);
        }

        HandlePointer();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space) || ControllerConfirm())
            Activate();
    }

    void OpenMain()
    {
        page = Page.Main;
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        selected = 0;
        RefreshMainSelection();
    }

    void OpenOptions()
    {
        page = Page.Options;
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        selected = 0;
        RefreshOptions();
    }

    void OpenCredits()
    {
        page = Page.Credits;
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        selected = 0;
        RefreshCredits();
    }

    void MoveSelection(int direction)
    {
        if (page == Page.Credits) return;
        int count = page == Page.Main ? mainRows.Length : optionsRows.Length;
        for (int attempt = 0; attempt < count; attempt++)
        {
            selected = (selected + direction + count) % count;
            if (page != Page.Main || selected != 1 || TechnopolisGameFlow.CanContinue) break;
        }
        RefreshSelection();
    }

    void RefreshSelection()
    {
        if (page == Page.Main) RefreshMainSelection();
        else if (page == Page.Options) RefreshOptions();
        else RefreshCredits();
    }

    void RefreshMainSelection()
    {
        bool canContinue = TechnopolisGameFlow.CanContinue;
        if (!canContinue && selected == 1) selected = 2;
        for (int i = 0; i < mainLabels.Length; i++)
            mainLabels[i].color = i == 1 && !canContinue
                ? new Color32(111, 122, 132, 255)
                : i == selected ? Color.white : new Color32(216, 222, 224, 255);

        RectTransform highlight = selectionBackground.rectTransform;
        highlight.SetParent(mainRows[selected], false);
        highlight.anchoredPosition = Vector2.zero;
        highlight.SetAsFirstSibling();
        RectTransform arrow = selectionArrow.rectTransform;
        arrow.SetParent(mainRows[selected], false);
        arrow.anchoredPosition = new Vector2(10, -18);
        arrow.SetSiblingIndex(1);
    }

    void RefreshOptions()
    {
        int volume = Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 100), 0, 100);
        volumeLabel.text = "VOLUMEN  " + volume + "%";
        fullscreenLabel.text = "PANTALLA COMPLETA  " + (Screen.fullScreen ? "SÍ" : "NO");
        for (int i = 0; i < optionsLabels.Length; i++)
            optionsLabels[i].color = i == selected ? new Color32(127, 236, 255, 255) : Color.white;
    }

    void RefreshCredits()
    {
        creditsBackLabel.color = new Color32(127, 236, 255, 255);
    }

    void ChangeOption(int direction)
    {
        if (selected == 0)
        {
            int current = Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 100), 0, 100);
            int next = Mathf.Clamp(current + direction * 10, 0, 100);
            PlayerPrefs.SetInt(VolumeKey, next);
            PlayerPrefs.Save();
            AudioListener.volume = next / 100f;
        }
        else if (selected == 1)
        {
            Screen.fullScreen = !Screen.fullScreen;
            PlayerPrefs.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
        }
        RefreshOptions();
    }

    void Activate()
    {
        if (page == Page.Options)
        {
            if (selected == 2) OpenMain();
            else ChangeOption(1);
            return;
        }
        if (page == Page.Credits)
        {
            OpenMain();
            return;
        }

        switch (selected)
        {
            case 0: TechnopolisGameFlow.StartNewGame(); break;
            case 1: if (TechnopolisGameFlow.CanContinue) TechnopolisGameFlow.ContinueGame(); break;
            case 2: OpenOptions(); break;
            case 3: OpenCredits(); break;
            case 4:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    void HandlePointer()
    {
        Vector2 pointer = Input.mousePosition;
        bool moved = (pointer - lastMousePosition).sqrMagnitude > 4f;
        lastMousePosition = pointer;
        if (!moved && !Input.GetMouseButtonDown(0)) return;

        RectTransform[] rows = page == Page.Main ? mainRows :
            page == Page.Options ? optionsRows : new[] { creditsBackRow };
        for (int i = 0; i < rows.Length; i++)
        {
            if (!rows[i] || !RectTransformUtility.RectangleContainsScreenPoint(rows[i], pointer)) continue;
            if (page == Page.Main && i == 1 && !TechnopolisGameFlow.CanContinue) return;
            if (selected != i)
            {
                selected = i;
                RefreshSelection();
            }
            if (Input.GetMouseButtonDown(0)) Activate();
            return;
        }
    }

    static bool ControllerConfirm()
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        return pad != null && pad.buttonSouth.wasPressedThisFrame;
#else
        return false;
#endif
    }

    static bool ControllerBack()
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        return pad != null && pad.buttonEast.wasPressedThisFrame;
#else
        return false;
#endif
    }

    int ControllerVertical(int current)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        if (pad == null) return current;
        if (pad.dpad.up.wasPressedThisFrame) return -1;
        if (pad.dpad.down.wasPressedThisFrame) return 1;
        float y = pad.leftStick.ReadValue().y;
        if (Mathf.Abs(y) > .65f && Time.unscaledTime >= nextStickMove)
        {
            nextStickMove = Time.unscaledTime + .2f;
            return y > 0 ? -1 : 1;
        }
#endif
        return current;
    }

    int ControllerHorizontal(int current)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        if (pad == null) return current;
        if (pad.dpad.left.wasPressedThisFrame) return -1;
        if (pad.dpad.right.wasPressedThisFrame) return 1;
        float x = pad.leftStick.ReadValue().x;
        if (Mathf.Abs(x) > .65f && Time.unscaledTime >= nextStickMove)
        {
            nextStickMove = Time.unscaledTime + .2f;
            return x > 0 ? 1 : -1;
        }
#endif
        return current;
    }
}
