using UnityEngine;

public interface IHealth
{
    float Max { get; }
    float Current { get; }
    void Heal(float amount);
    void Kill();
}
