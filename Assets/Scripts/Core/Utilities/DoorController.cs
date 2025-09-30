using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DoorController : MonoBehaviour
{
    [Header("State")]
    [SerializeField] bool startsOpen = false;
    [SerializeField] bool togglable = true;

    [Header("Visuals / Collision")]
    [SerializeField] Animator animator;
    [SerializeField] string animBool = "Open";
    [SerializeField] Collider2D[] blockers;
    [SerializeField] GameObject[] hideWhenOpen;

    [Header("Audio (optional)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openSfx;
    [SerializeField] AudioClip closeSfx;

    [Header("Editor Comfort")]
    [SerializeField] bool autoDeselectBeforeHide = true; // vähentää Editorin NRE:tä

    public UnityEvent OnOpened;
    public UnityEvent OnClosed;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (blockers == null || blockers.Length == 0)
            blockers = GetComponentsInChildren<Collider2D>(true);
    }

    void Start() => ApplyState(startsOpen, instant: true);

    public void Open() => ApplyState(true, false);
    public void Close() => ApplyState(false, false);
    public void Toggle()
    {
        if (!togglable && IsOpen) return;
        ApplyState(!IsOpen, false);
    }

    void ApplyState(bool open, bool instant)
    {
        if (IsOpen == open && !instant) return;
        IsOpen = open;

        if (animator && !string.IsNullOrEmpty(animBool))
            animator.SetBool(animBool, IsOpen);

        // Colliders blokkaa vain kun kiinni
        if (blockers != null)
            foreach (var c in blockers) if (c) c.enabled = !IsOpen;

        // Piilota/tuo näkyviin visuaalit
        if (hideWhenOpen != null)
        {
            foreach (var go in hideWhenOpen)
            {
                if (!go) continue;
                if (go == gameObject)
                {
                    Debug.LogWarning($"[Door] {name}: Älä laita itse ovea hideWhenOpen-listaan. Käytä lapsi-objektia (esim. 'Visual').");
                    continue;
                }

#if UNITY_EDITOR
                if (autoDeselectBeforeHide && Selection.activeGameObject == go)
                    Selection.activeGameObject = null;
#endif

                go.SetActive(!IsOpen);
            }
        }

        if (!instant && audioSource)
        {
            var clip = IsOpen ? openSfx : closeSfx;
            if (clip) audioSource.PlayOneShot(clip);
        }

        if (IsOpen) OnOpened?.Invoke(); else OnClosed?.Invoke();
    }

    void OnValidate()
    {
        // Siivoa nullit ja komponentit
        if (hideWhenOpen != null)
        {
            for (int i = 0; i < hideWhenOpen.Length; i++)
            {
                var go = hideWhenOpen[i];
                if (!go) continue;

                // jos joku raahasi vahingossa komponentin
                if (go.GetComponent<Transform>() == null)
                {
                    Debug.LogWarning($"[Door] {name}: hideWhenOpen[{i}] ei ole GameObject. Poista ja lisää GameObject.");
                    hideWhenOpen[i] = null;
                }
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Toggle Now")]
    void EditorToggleNow() => Toggle();
#endif
}
