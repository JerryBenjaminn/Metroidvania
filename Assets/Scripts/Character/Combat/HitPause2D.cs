using UnityEngine;
using System.Collections;

public class HitPause2D : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;

    Vector2 savedVel;
    bool paused;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Pause(float seconds)
    {
        if (paused || seconds <= 0) return;
        StartCoroutine(CoPause(seconds));
    }

    IEnumerator CoPause(float t)
    {
        paused = true;

        float prevAnimSpeed = 1f;
        if (animator) { prevAnimSpeed = animator.speed; animator.speed = 0f; }

        bool hadRb = rb && rb.simulated;
        if (hadRb) { savedVel = rb.linearVelocity; rb.simulated = false; }

        yield return new WaitForSecondsRealtime(t);

        if (animator) animator.speed = prevAnimSpeed;
        if (hadRb) { rb.simulated = true; rb.linearVelocity = savedVel; }

        paused = false;
    }
}
