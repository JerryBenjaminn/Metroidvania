using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityController : MonoBehaviour, IAbilityUser
{
    [SerializeField] List<Ability> abilities = new();
    [SerializeField] bool[] unlocked; // sama pituus kuin abilities
    bool busy;
    CharacterMotor motor;
    Rigidbody2D rb;

    public Rigidbody2D Rigidbody => rb;
    public CharacterMotor Motor => motor;
    public bool IsGrounded => motor.IsGrounded;

    void Awake() {
        motor = GetComponent<CharacterMotor>();
        rb = GetComponent<Rigidbody2D>();
        if (unlocked == null || unlocked.Length != abilities.Count)
            unlocked = new bool[abilities.Count];
    }

    public void TriggerAbility(int index) {
        if (busy || index < 0 || index >= abilities.Count) return;
        if (!unlocked[index]) return;
        var ability = abilities[index];
        if (ability && ability.CanUse(this)) StartCoroutine(Run(ability));
    }

    IEnumerator Run(Ability a) {
        busy = true;
        yield return a.Execute(this);
        busy = false;
    }

    public void Unlock(string abilityName) {
        for (int i = 0; i < abilities.Count; i++)
            if (abilities[i] && abilities[i].abilityName == abilityName)
                unlocked[i] = true;
    }
    // AbilityController.cs (lisäykset)
    public event System.Action<Ability, int> OnAbilityUnlocked;

    public int IndexOf(Ability ability) => abilities != null ? abilities.IndexOf(ability) : -1;

    public bool IsUnlocked(Ability ability)
    {
        int i = IndexOf(ability);
        return i >= 0 && i < unlocked.Length && unlocked[i];
    }

    public bool Unlock(Ability ability)
    {
        int i = IndexOf(ability);
        if (i < 0) { Debug.LogWarning($"[{name}] Ability {ability?.name} ei ole listassa."); return false; }
        if (unlocked[i]) return false;
        unlocked[i] = true;
        OnAbilityUnlocked?.Invoke(ability, i);
        Debug.Log($"Unlocked ability: {ability.name} (index {i})");
        return true;
    }
    public List<string> GetUnlockedAbilityNames()
    {
        var list = new List<string>();
        for (int i = 0; i < abilities.Count; i++)
            if (i < unlocked.Length && unlocked[i] && abilities[i])
                list.Add(abilities[i].name);
        return list;
    }

    public void SetUnlockedByNames(List<string> names)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            bool on = abilities[i] && names.Contains(abilities[i].name);
            if (i < unlocked.Length) unlocked[i] = on;
        }
    }

    public bool UnlockByName(string abilityName)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            var a = abilities[i];
            if (a && a.name == abilityName)
                return Unlock(a);
        }
        return false;
    }
}

