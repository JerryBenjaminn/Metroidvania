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
        public int abilityPower;
    }

    Health hp;
    AbilityController ac;
    AbilityPower ap;

    void Awake()
    {
        hp = GetComponent<Health>();
        ac = GetComponent<AbilityController>();
        ap = GetComponent<AbilityPower>();
        GameManager.Instance?.RegisterPlayer(transform);
    }

    public object CaptureState()
    {
        return new State
        {
            pos = transform.position,
            health = hp ? hp.Current : 0,
            abilities = ac ? ac.GetUnlockedAbilityNames() : new System.Collections.Generic.List<string>(),
            abilityPower = ap ? ap.Current : 0
        };
    }

    // 10/7/2025 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

    public void RestoreState(object o)
    {
        var s = o as State;
        if (s == null) return;

        transform.position = s.pos;

        if (hp)
        {
            hp.SetHealthFromSave(s.health); // Use SetHealthFromSave to ensure OnHealthChanged is invoked
        }
        if (ac) ac.SetUnlockedByNames(s.abilities);
        if (ap) ap.Set(s.abilityPower);
    }

    public void SaveState()
    {
        //Tallenna pelaajan data
        Debug.Log("Player state saved.");
    }

    public void RestoreStateFromCheckpoint(Checkpoint checkpoint)
    {
        //Palauta pelaajan positio checkpointille
        transform.position = checkpoint.SpawnPoint.position;

        //Palauta muu data jos tarvitsee
        Debug.Log("Player state restored to checkpoint.");
    }
}
