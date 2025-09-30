using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LeverVisuals : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string pulledBool = "Pulled";
    bool pulled;

    void Reset() { animator = GetComponent<Animator>(); }

    // Sido nämä UnityEventeihin:
    public void TogglePulled() { SetPulled(!pulled); }
    public void SetPulledTrue() { SetPulled(true); }
    public void SetPulledFalse() { SetPulled(false); }

    public void SetPulled(bool v)
    {
        pulled = v;
        if (animator && !string.IsNullOrEmpty(pulledBool))
            animator.SetBool(pulledBool, pulled);
    }
}

