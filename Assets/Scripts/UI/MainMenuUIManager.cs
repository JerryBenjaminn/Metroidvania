using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Manages the main menu UI and its interactions.
/// </summary>
public class MainMenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject saveSlotsPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button continueButton;

    [Header("First Selected")]
    [SerializeField] private GameObject mainFirstSelected;
    [SerializeField] private GameObject slotsFirstSelected;
    [SerializeField] private GameObject optionsFirstSelected;

    [Header("Slots")]
    [SerializeField] private SaveSlotView[] slots;

    void Start()
    {
        if (SaveManager.Instance) SaveManager.Instance.SavesDisabled = true;

        startButton.onClick.AddListener(OpenSlots);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);

        if (continueButton)
        {
            continueButton.onClick.AddListener(ContinueLatest);
            RefreshContinueButton();
        }

        OpenMain();
    }

    private void OpenMain()
    {
        SetActivePanel(mainMenuPanel);
        RefreshContinueButton();
    }

    private void OpenSlots()
    {
        foreach (var slot in slots) slot?.Refresh();
        SetActivePanel(saveSlotsPanel);
    }

    public void CloseSlots() => OpenMain();

    private void OpenOptions() => SetActivePanel(optionsPanel);

    public void CloseOptions() => OpenMain();

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetActivePanel(GameObject panel)
    {
        mainMenuPanel.SetActive(panel == mainMenuPanel);
        saveSlotsPanel.SetActive(panel == saveSlotsPanel);
        optionsPanel.SetActive(panel == optionsPanel);
        SelectFirst(panel == mainMenuPanel ? mainFirstSelected : panel == saveSlotsPanel ? slotsFirstSelected : optionsFirstSelected);
    }

    private void SelectFirst(GameObject go)
    {
        if (!go) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }

    private void RefreshContinueButton()
    {
        if (!continueButton || SaveManager.Instance == null) return;

        int latest = -1;
        long latestTicks = long.MinValue;
        for (int i = 0; i < SaveManager.Instance.slotCount; i++)
        {
            var summary = SaveManager.Instance.GetSummary(i);
            if (summary.valid && summary.savedAtUtc.Ticks > latestTicks)
            {
                latest = i;
                latestTicks = summary.savedAtUtc.Ticks;
            }
        }

        continueButton.interactable = latest >= 0;
        continueButton.GetComponentInChildren<TMP_Text>().text = latest >= 0 ? "Continue" : "Continue (no save)";
    }

    private void ContinueLatest()
    {
        if (!SaveManager.Instance) return;

        int latest = -1;
        long latestTicks = long.MinValue;
        for (int i = 0; i < SaveManager.Instance.slotCount; i++)
        {
            var summary = SaveManager.Instance.GetSummary(i);
            if (summary.valid && summary.savedAtUtc.Ticks > latestTicks)
            {
                latest = i;
                latestTicks = summary.savedAtUtc.Ticks;
            }
        }

        if (latest >= 0)
        {
            SaveManager.Instance.SavesDisabled = true;
            GameManager.Instance.LoadGame(latest);
        }
    }
}