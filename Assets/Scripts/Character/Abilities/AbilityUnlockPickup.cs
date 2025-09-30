using UnityEngine;

public class AbilityUnlockPickup : MonoBehaviour
{
    public string abilityName;
    void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<AbilityController>(out var ac)) {
            ac.Unlock(abilityName);
            Destroy(gameObject);
        }
    }
}

