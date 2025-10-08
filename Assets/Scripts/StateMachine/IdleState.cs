using UnityEngine;

public class IdleState : IState
{
    public void Enter()
    {
        Debug.Log("Entering Idle State");
    }

    public void Execute()
    {
        Debug.Log("Executing Idle State");
    }

    public void Exit()
    {
        Debug.Log("Exiting Idle State");
    }
}
