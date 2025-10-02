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
            SetPrimary("Continue", OnContinue);
            deleteButton.interactable = true;
        }
        else
        {
            if (titleText) titleText.text = $"Slot {slotIndex + 1} — New Game";
            if (subText) subText.text = s.exists ? "Corrupted/Unknown save" : "Empty";
            SetPrimary("New Game", OnNewGame);
            deleteButton.interactable = s.exists;
        }

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnDelete);
    }

    void SetPrimary(string label, System.Action action)
    {
        var txt = primaryButton.GetComponentInChildren<TMP_Text>();
        if (txt) txt.text = label;
        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(() => action());
    }

    void OnNewGame()
    {
        var sm = SaveManager.Instance; if (!sm) return;
        primaryButton.interactable = false;
        sm.SavesDisabled = true;
        GameManager.Instance.NewGame(slotIndex, startScene, startPosition);
    }

    void OnContinue()
    {
        var sm = SaveManager.Instance; if (!sm) return;
        primaryButton.interactable = false;
        sm.SavesDisabled = true;
        GameManager.Instance.LoadGame(slotIndex);
    }

    void OnDelete()
    {
        var sm = SaveManager.Instance; if (!sm) return;
        sm.DeleteSlot(slotIndex);
        Refresh();
    }
}
