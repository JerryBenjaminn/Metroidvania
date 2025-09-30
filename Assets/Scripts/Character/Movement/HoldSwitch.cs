using UnityEngine;
using UnityEngine.Events;

public class HoldSwitch : MonoBehaviour, IInteractable
{
    public float holdTime = 0.6f;
    public UnityEvent onActivated;
    public InteractionKind Kind => InteractionKind.Hold;
    public float HoldTime => holdTime;

    public bool CanInteract(GameObject who) => true;
    public string GetPrompt() => "Hold to activate";

    public void Interact(GameObject who) => onActivated?.Invoke();
    public void OnFocusEnter(GameObject who) { }
    public void OnFocusExit(GameObject who) { }
}

