using UnityEngine;

public class PatrolState : IState
{
    public void Enter()
    {
        Debug.Log("Entering Patrol State");
    }

    public void Execute()
    {
        Debug.Log("Executing Patrol State");
    }

    public void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }
}
