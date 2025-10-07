using UnityEngine;

/// <summary>
/// Handles saving and restoring the player's state.
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerSave : MonoBehaviour, ISaveable
{
    [System.Serializable]
    public class State
    {
        public Vector3 pos;
        public float health;
        public System.Collections.Generic.List<string> abilities;
        public int abilityPower;
    }

    private Health _health;
    private AbilityController _abilityController;
    private AbilityPower _abilityPower;

    void Awake()
    {
        _health = GetComponent<Health>();
        _abilityController = GetComponent<AbilityController>();
        _abilityPower = GetComponent<AbilityPower>();
        GameManager.Instance?.RegisterPlayer(transform);
    }

    public object CaptureState()
    {
        return new State
        {
            pos = transform.position,
            health = _health ? _health.Current : 0,
            abilities = _abilityController ? _abilityController.GetUnlockedAbilityNames() : new System.Collections.Generic.List<string>(),
            abilityPower = _abilityPower ? _abilityPower.Current : 0
        };
    }

    public void RestoreState(object state)
    {
        if (state is not State s) return;

        transform.position = s.pos;

        if (_health) _health.SetHealthFromSave(s.health);
        if (_abilityController) _abilityController.SetUnlockedByNames(s.abilities);
        if (_abilityPower) _abilityPower.Set(s.abilityPower);
    }
}