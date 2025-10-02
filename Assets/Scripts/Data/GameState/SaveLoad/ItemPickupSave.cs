using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class ItemPickupSave : MonoBehaviour, ISaveable
{
    [System.Serializable] public class State { public bool collected; }

    public object CaptureState() => new State { collected = !gameObject.activeSelf };
    public void RestoreState(object s)
    {
        var st = s as State; if (st == null) return;
        gameObject.SetActive(!st.collected);
    }
}
