using UnityEngine;

[RequireComponent(typeof(DoorController))]
public class DoorControllerSave : MonoBehaviour, ISaveable
{
    [System.Serializable] public class State { public bool open; }

    DoorController door;

    void Awake() { door = GetComponent<DoorController>(); }

    public object CaptureState() => new State { open = door.IsOpen };

    public void RestoreState(object o)
    {
        var s = o as State; if (s == null) return;
        if (s.open) door.Open(); else door.Close();
    }
}
