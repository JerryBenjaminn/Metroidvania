using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Image iconPrefab;
    [SerializeField] Sprite spriteFull;
    [SerializeField] Sprite spriteEmpty;

    Image[] icons;

    void Awake()
    {
        if (!health) health = FindFirstObjectByType<Health>();
        Rebuild();
        health.OnHealthChanged += UpdateUI;
        UpdateUI(health.Current, health.Max);
    }

    void OnDestroy() { if (health) health.OnHealthChanged -= UpdateUI; }

    void Rebuild()
    {
        foreach (Transform c in transform) Destroy(c.gameObject);
        icons = new Image[Mathf.Max(1, Mathf.RoundToInt(health.Max))];
        for (int i = 0; i < icons.Length; i++) icons[i] = Instantiate(iconPrefab, transform);
    }

    void UpdateUI(float cur, float max)
    {
        int m = Mathf.RoundToInt(max);
        int c = Mathf.RoundToInt(cur);
        if (icons == null || icons.Length != m) Rebuild();
        for (int i = 0; i < m; i++) icons[i].sprite = i < c ? spriteFull : spriteEmpty;
    }
}
