using UnityEngine;



public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}

public class StateMachineSystem : MonoBehaviour
{
    private IState currentState;

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;

        if (currentState != null )
        {
            currentState.Enter();
        }
    }

    public void Update()
    {
        if ( currentState != null )
        {
            currentState.Execute();
        }
    }

}
