using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class ActorBase : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected virtual void Awake() => rb = GetComponent<Rigidbody2D>();
}

