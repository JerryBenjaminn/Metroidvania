using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject saveSlotsPanel;
    public GameObject optionsPanel;

    [Header("Main Menu Buttons")]
    public Button startBtn;
    public Button optionsBtn;
    public Button quitBtn;
    public Button continueBtn;           // valinnainen

    [Header("First Selected")]
    public GameObject mainFirstSelected; // esim. Start Game
    public GameObject slotsFirstSelected;// esim. 1. slotin primary-nappi
    public GameObject optionsFirstSelected;

    [Header("Slots")]
    public SaveSlotView[] slots;         // vedä 3 kpl inspectorissa

    void OnEnable()
    {
        if (SaveManager.Instance) SaveManager.Instance.SavesDisabled = true;
    }

    void Start()
    {
        if (SaveManager.Instance) SaveManager.Instance.SavesDisabled = true;

        // Hookit
        startBtn.onClick.AddListener(OpenSlots);
        optionsBtn.onClick.AddListener(OpenOptions);
        quitBtn.onClick.AddListener(QuitGame);

        // Continue-nappi: lataa tuorein validi slotti
        if (continueBtn)
        {
            continueBtn.onClick.AddListener(ContinueLatest);
            RefreshContinueButton();
        }

        OpenMain();
    }

    void OpenMain()
    {
        mainMenuPanel.SetActive(true);
        saveSlotsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        SelectFirst(mainFirstSelected);
        RefreshContinueButton();
    }

    void OpenSlots()
    {
        // päivitä jokainen slot-kortti
        foreach (var s in slots) if (s) s.Refresh();

        mainMenuPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);
        optionsPanel.SetActive(false);
        SelectFirst(slotsFirstSelected);
    }

    public void CloseSlots() => OpenMain();

    void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
        optionsPanel.SetActive(true);
        SelectFirst(optionsFirstSelected);
    }

    public void CloseOptions() => OpenMain();

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SelectFirst(GameObject go)
    {
        if (!go) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }
    public void BackFromSlots()
    {
        // sulje Slots, avaa Main
        saveSlotsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // aseta ohjainfokus takaisin Start Gameen (tai mihin haluat)
        SelectFirst(mainFirstSelected);
    }


    void RefreshContinueButton()
    {
        if (!continueBtn || SaveManager.Instance == null) return;

        // etsi uusin validi
        int latest = -1;
        long latestTicks = long.MinValue;
        for (int i = 0; i < SaveManager.Instance.slotCount; i++)
        {
            var sum = SaveManager.Instance.GetSummary(i);
            if (sum.valid && sum.savedAtUtc.Ticks > latestTicks)
            {
                latest = i; latestTicks = sum.savedAtUtc.Ticks;
            }
        }
        continueBtn.interactable = latest >= 0;
        continueBtn.gameObject.SetActive(true); // jos et halua näyttää lainkaan, kun ei löydy, piilota tässä
        continueBtn.GetComponentInChildren<TMP_Text>().text =
            latest >= 0 ? "Continue" : "Continue (no save)";
    }

    void ContinueLatest()
    {
        if (!SaveManager.Instance) return;

        int latest = -1;
        long latestTicks = long.MinValue;
        for (int i = 0; i < SaveManager.Instance.slotCount; i++)
        {
            var sum = SaveManager.Instance.GetSummary(i);
            if (sum.valid && sum.savedAtUtc.Ticks > latestTicks)
            {
                latest = i; latestTicks = sum.savedAtUtc.Ticks;
            }
        }
        if (latest >= 0)
        {
            // Ensure saving is disabled while loading the game
            SaveManager.Instance.SavesDisabled = true;
            GameManager.Instance.LoadGame(latest);
        }
    }
}
