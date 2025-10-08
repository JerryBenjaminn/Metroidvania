using UnityEngine;

public class ChargerChargeState : IState
{
    private ChargerEnemy charger;

    public ChargerChargeState(ChargerEnemy charger)
    {
        this.charger = charger;
    }
    public void Enter()
    {
        Debug.Log("Charger: Entering Charge State");
        charger.StartCharge();
    }

    public void Execute()
    {
        if(!charger.IsCharging())
        {
            charger.StateMachine.ChangeState(new ChargerIdleState(charger));
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Charge State");
    }
}
