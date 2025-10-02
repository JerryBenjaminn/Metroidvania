using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotView : MonoBehaviour
{
    public int slotIndex = 0;
    public string startScene = "StartScene";
    public Vector3 startPosition = Vector3.zero;

    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text subText;
    [SerializeField] Button primaryButton; // Continue / New
    [SerializeField] Button deleteButton;

    void OnEnable() { Refresh(); }

    public void Refresh()
    {
        var sm = SaveManager.Instance;
        if (!sm) return;

        var s = sm.GetSummary(slotIndex);
        bool has = s.valid;

        if (has)
        {
            if (titleText) titleText.text = $"Slot {slotIndex + 1} — Continue";
            if (subText)
            {
                var local = s.savedAtUtc == System.DateTime.MinValue ? "unknown" :
                    s.savedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                subText.text = $"{s.scene} | HP {s.hp:0} | Abilities {s.abilityCount} | {local}";
            }
            primaryButton.GetComponentInChildren<TMP_Text>().text = "Continue";
            primaryButton.onClick.RemoveAllListeners();
            primaryButton.onClick.AddListener(() => OnContinue());
            deleteButton.interactable = true;
        }
        else
        {
            if (titleText) titleText.text = $"Slot {slotIndex + 1} — New Game";
            if (subText) subText.text = s.exists ? "Corrupted/Unknown save" : "Empty";
            primaryButton.GetComponentInChildren<TMP_Text>().text = "New Game";
            primaryButton.onClick.RemoveAllListeners();
            primaryButton.onClick.AddListener(() => OnNewGame());
            deleteButton.interactable = s.exists; // voi poistaa korruptoidunkin
        }

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => OnDelete());
    }

    void OnNewGame()
    {
        var sm = SaveManager.Instance; if (!sm) return;
        primaryButton.interactable = false;
        sm.SavesDisabled = true; // ollaan valikossa
        GameManager.Instance.NewGame(slotIndex, startScene, startPosition);
    }

    void OnContinue()
    {
        var sm = SaveManager.Instance; if (!sm) return;
        primaryButton.interactable = false;
        sm.SavesDisabled = true; // valikossa, estä tallennus
        GameManager.Instance.LoadGame(slotIndex);
    }

    void OnDelete()
    {
        var sm = SaveManager.Instance; if (!sm) return;
        sm.DeleteSlot(slotIndex);
        Refresh();
    }
}

