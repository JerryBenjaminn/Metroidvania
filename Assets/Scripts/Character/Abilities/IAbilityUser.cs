using UnityEngine;

public interface IAbilityUser
{
    Transform transform { get; }
    Rigidbody2D Rigidbody { get; }
    CharacterMotor Motor { get; }
    bool IsGrounded { get; }
}
