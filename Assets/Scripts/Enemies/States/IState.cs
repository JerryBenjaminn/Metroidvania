/*using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnExit();
    void Tick();
}

public class StateMachine
{
    IState current;
    public void Set(IState next) {
        current?.OnExit();
        current = next;
        current?.OnEnter();
    }
    public void Tick() => current?.Tick();
}

*/