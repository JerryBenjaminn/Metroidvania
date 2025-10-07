using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMotor))]
public class PlayerInputController : MonoBehaviour
{
    CharacterMotor motor;
    AbilityController abilities;
    Vector2 moveInput;
    Interactor interactor;
    AimInput aim;

    void Awake() {
        motor = GetComponent<CharacterMotor>();
        abilities = GetComponent<AbilityController>();
        interactor = GetComponent<Interactor>();
        aim = GetComponent<AimInput>();
    }

    // PlayerInput → Unity Events kutsuu näitä nimiä automaattisesti:
    public void OnMove(InputAction.CallbackContext ctx) {
        moveInput = ctx.ReadValue<Vector2>();
        motor.SetMove(moveInput.x);
        aim?.SetAim(moveInput);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started) motor.QueueJump();      // pyyntö hypylle (bufferoidaan)
        if (ctx.canceled) motor.ReleaseJump();   // nappi irti → low jump
    }

    public void OnAbility1(InputAction.CallbackContext ctx) {
        if (ctx.started) GetComponent<AbilityController>().TriggerAbility(2);
        Debug.Log("Pressed F");
    }

    public void OnAbility2(InputAction.CallbackContext ctx) {
        if (ctx.started) abilities?.TriggerAbility(1);
    }

    public void OnDash(InputAction.CallbackContext ctx) {
        if (ctx.started) abilities?.TriggerAbility(0); // jos dash on index 0
    }

    public void OnPause(InputAction.CallbackContext ctx) {
        if (ctx.started) PauseMenu.Toggle(); // toteuta yksinkertainen pause
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started) interactor?.StartInteract();
        if (ctx.canceled) interactor?.CancelInteract();
    }
    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started) GetComponent<AbilityController>()?.TriggerAbility( /* index */ 0);
    }


    void Update() {
        motor.SetMove(moveInput.x);
    }
}