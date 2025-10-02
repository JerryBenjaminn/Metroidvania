using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerSave : MonoBehaviour, ISaveable
{
    [System.Serializable]
    public class State
    {
        public Vector3 pos;
        public float health;
        public System.Collections.Generic.List<string> abilities;
    }

    Health hp;
    AbilityController ac;

    void Awake()
    {
        hp = GetComponent<Health>();
        ac = GetComponent<AbilityController>();
        GameManager.Instance?.RegisterPlayer(transform);
    }

    public object CaptureState()
    {
        return new State
        {
            pos = transform.position,
            health = hp ? hp.Current : 0,
            abilities = ac ? ac.GetUnlockedAbilityNames() : new System.Collections.Generic.List<string>()
        };
    }

    public void RestoreState(object o)
    {
        var s = o as State; if (s == null) return;
        transform.position = s.pos;
        if (hp) { hp.Heal(hp.Max); hp.ApplyDamage(hp.Max - s.health, Vector2.zero); }
        if (ac) ac.SetUnlockedByNames(s.abilities);
    }
}
