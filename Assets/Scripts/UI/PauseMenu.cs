using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    // Yksi instanssi koko peliin
    public static PauseMenu Instance { get; private set; }
    public static bool IsPaused => Instance != null && Instance._isPaused;

    [Header("References")]
    [SerializeField] GameObject menuRoot;          // Canvas tai Panel, joka näytetään pause-tilassa
    [SerializeField] GameObject firstSelected;     // Ensimmäinen valittu UI-nappi kun menu avataan
    [SerializeField] PlayerInput playerInput;      // (valinnainen) jos haluat vaihtaa action mapin
    [SerializeField] string gameplayMap = "Player";
    [SerializeField] string uiMap = "UI";

    [Header("Behavior")]
    [SerializeField] bool pauseAudio = false;      // pysäytä äänet pausen aikana
    [SerializeField] bool switchToUIActionMap = false; // vaihda action map "UI":hin pausen ajaksi

    bool _isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (menuRoot != null) menuRoot.SetActive(false);
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        // Varmista että peli ei jää pysyvään pausetilaan editorissa
        if (_isPaused) SetPaused(false);
    }

    public static void Toggle()
    {
        if (Instance == null) { Debug.LogWarning("PauseMenu.Toggle() kutsuttiin, mutta PauseMenua ei ole scenessä."); return; }
        Instance.SetPaused(!Instance._isPaused);
    }

    public static void Pause()
    {
        if (Instance == null) return;
        Instance.SetPaused(true);
    }

    public static void Resume()
    {
        if (Instance == null) return;
        Instance.SetPaused(false);
    }

    void SetPaused(bool pause)
    {
        if (_isPaused == pause) return;
        _isPaused = pause;

        // Aika ja audio
        Time.timeScale = pause ? 0f : 1f;
        if (pauseAudio) AudioListener.pause = pause;

        // UI näkyviin/piiloon
        if (menuRoot != null) menuRoot.SetActive(pause);

        // Valitse ensimmäinen nappi kun avataan
        if (pause && firstSelected != null)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            EventSystem.current?.SetSelectedGameObject(firstSelected);
        }

        // Halutessa vaihdetaan action map UI:hin
        if (switchToUIActionMap && playerInput != null)
        {
            var targetMap = pause ? uiMap : gameplayMap;
            if (!string.IsNullOrEmpty(targetMap) && playerInput.currentActionMap?.name != targetMap)
                playerInput.SwitchCurrentActionMap(targetMap);
        }
    }

    // Nämä voi sitoa UI-nappeihin
    public void UI_Resume() => Resume();
    public void UI_QuitToDesktop()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

