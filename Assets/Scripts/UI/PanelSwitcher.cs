using UnityEngine;
using UnityEngine.EventSystems;

public class PanelSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class Panel
    {
        public string key;                 // "Main", "Slots", "Options"
        public CanvasGroup group;          // vedä CanvasGroup
        public GameObject firstSelected;   // ohjainfokus
    }

    public Panel[] panels;
    public float fadeTime = 0.15f;

    Panel current;

    void Awake()
    {
        foreach (var p in panels) SetVisible(p, false, instant: true);
    }

    public void Show(string key)
    {
        var next = System.Array.Find(panels, p => p.key == key);
        if (next == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeTo(next));
    }

    System.Collections.IEnumerator FadeTo(Panel next)
    {
        if (current != null) yield return Fade(current, false);
        yield return Fade(next, true);
        current = next;

        if (next.firstSelected)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(next.firstSelected);
        }
    }

    System.Collections.IEnumerator Fade(Panel p, bool on)
    {
        if (!p.group) yield break;
        float t = 0f, from = p.group.alpha, to = on ? 1f : 0f;
        p.group.blocksRaycasts = true; // estä klikkispämmi faden aikana
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            p.group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        p.group.alpha = to;
        p.group.interactable = on;
        p.group.blocksRaycasts = on;
    }

    void SetVisible(Panel p, bool on, bool instant = false)
    {
        if (!p.group) return;
        p.group.alpha = on ? 1 : 0;
        p.group.interactable = on;
        p.group.blocksRaycasts = on;
    }
}
