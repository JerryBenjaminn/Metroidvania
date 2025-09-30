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
}

