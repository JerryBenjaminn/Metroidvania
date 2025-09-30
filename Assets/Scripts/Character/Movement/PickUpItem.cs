using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public string itemName = "Shiny Thing";
    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldTime => 0f;

    public bool CanInteract(GameObject who) => true;

    public string GetPrompt() => $"Pick up {itemName}";

    public void Interact(GameObject who)
    {
        Debug.Log($"Picked up: {itemName}");
        // TODO: lis‰‰ inventaariin
        Destroy(gameObject);
    }

    SpriteRenderer sr;
    void Awake() { sr = GetComponentInChildren<SpriteRenderer>() ?? GetComponent<SpriteRenderer>(); }

    public void OnFocusEnter(GameObject who) { /* highlight p‰‰lle tms. */ }
    public void OnFocusExit(GameObject who) { /* highlight pois */ }
}

