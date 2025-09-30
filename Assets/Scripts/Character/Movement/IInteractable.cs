using UnityEngine;

public enum InteractionKind { Tap, Hold }

public interface IInteractable
{
    string GetPrompt();                  // esim. "Pick up"
    InteractionKind Kind { get; }        // Tap tai Hold
    float HoldTime { get; }              // käytössä jos Kind==Hold
    bool CanInteract(GameObject who);    // saako tehdä nyt
    void Interact(GameObject who);       // tee itse asia
    void OnFocusEnter(GameObject who);   // optionaalinen highlight
    void OnFocusExit(GameObject who);
}
