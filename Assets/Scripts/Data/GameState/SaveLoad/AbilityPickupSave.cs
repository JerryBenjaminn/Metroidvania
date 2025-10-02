using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class AbilityPickupSave : MonoBehaviour, ISaveable
{
    [System.Serializable] public class State { public bool collected; }

    public object CaptureState()
    {
        // jos olet inaktiivinen, olet kerätty
        return new State { collected = !gameObject.activeSelf };
    }

    public void RestoreState(object obj)
    {
        var s = obj as State; if (s == null) return;
        gameObject.SetActive(!s.collected);
    }
}
