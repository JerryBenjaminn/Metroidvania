using UnityEngine;
using UnityEngine.UI;

public class AbilityPowerUI : MonoBehaviour
{
    [SerializeField] Image fill; // Image type: Filled (Radial tai Horizontal)
    AbilityPower ap;

    void Start()
    {
        var player = GameManager.Instance?.Player;
        if (!player) { enabled = false; return; }
        ap = player.GetComponent<AbilityPower>();
        if (!ap) { enabled = false; return; }
        ap.OnChanged += HandleChanged;
        HandleChanged(ap.Current, ap.Max);
    }

    void OnDestroy()
    {
        if (ap != null) ap.OnChanged -= HandleChanged;
    }

    void HandleChanged(int cur, int max)
    {
        if (!fill) return;
        fill.fillAmount = max > 0 ? (float)cur / max : 0f;
    }
}
