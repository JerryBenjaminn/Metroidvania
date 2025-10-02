using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    public int slot = 0;
    public string startScene = "StartScene";
    public Vector3 startPosition = Vector3.zero;

    public Button newBtn, loadBtn, delBtn, quitBtn;
    public TextMeshProUGUI statusText; // tai TMP_Text

    void Start()
    {
        Refresh();
        newBtn.onClick.AddListener(() => GameManager.Instance.NewGame(slot, startScene, startPosition));
        loadBtn.onClick.AddListener(() => GameManager.Instance.LoadGame(slot));
        delBtn.onClick.AddListener(() => { GameManager.Instance.DeleteSave(slot); Refresh(); });
        if (quitBtn) quitBtn.onClick.AddListener(() => Application.Quit());
    }

    void Refresh()
    {
        bool has = SaveManager.Instance.HasValidSave(slot);
        if (statusText) statusText.text = $"Slot {slot}: " + (has ? "Save found" : "Empty");
        loadBtn.interactable = has;
        delBtn.interactable = has;
    }

    void OnEnable()
    {
        // Est‰ kaikki tallennukset p‰‰valikossa
        if (SaveManager.Instance) SaveManager.Instance.SavesDisabled = true;
    }
}
