using UnityEngine;
using System;

public class AbilityPower : MonoBehaviour, ISaveable
{
    [Header("Stats")]
    [SerializeField] int max = 99;          // esim. 99 “soul” pykälää
    [SerializeField] int start = 0;
    public int Max => max;
    public int Current { get; private set; }

    public event Action<int, int> OnChanged;

    void Awake()
    {
        Current = Mathf.Clamp(start, 0, max);
        OnChanged?.Invoke(Current, Max);
    }

    public void Set(int value)
    {
        int v = Mathf.Clamp(value, 0, Max);
        if (v == Current) return;
        Current = v;
        OnChanged?.Invoke(Current, Max);
    }

    public void Gain(int amount)
    {
        if (amount <= 0) return;
        Set(Current + amount);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (Current < amount) return false;
        Set(Current - amount);
        return true;
    }

    // --- Save/Load PlayerSave kautta (ISaveable) ---
    [System.Serializable] public class State { public int ap; }

    public object CaptureState() => new State { ap = Current };

    public void RestoreState(object o)
    {
        var s = o as State; if (s == null) return;
        Set(s.ap);
    }
}
