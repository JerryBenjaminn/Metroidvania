using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class Interactor : MonoBehaviour
{
    [Header("Detection")]
    public Transform origin;                 // esim. rintakehän kohdalle
    public float radius = 0.9f;
    public LayerMask interactableMask;
    public bool preferFacingDirection = true;

    [Header("Behavior")]
    public float cooldown = 0.15f;           // spam-suoja
    public Animator animator;                // sama kuin pelaajan Animator
    public string interactTrigger = "Interact";

    IInteractable current;
    Collider2D[] hits = new Collider2D[8];
    bool busy;
    Coroutine holdCo;

    void Reset() { origin = transform; }

    [System.Obsolete]
    void Update()
    {
        // Etsi lähin IInteractable säteeltä
        int n = Physics2D.OverlapCircleNonAlloc(origin.position, radius, hits, interactableMask);
        IInteractable best = null;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < n; i++)
        {
            var col = hits[i];
            if (!col) continue;

            // hae sekä tältä että vanhemmalta
            var cand = col.GetComponent<IInteractable>() ?? col.GetComponentInParent<IInteractable>();
            if (cand == null) continue;

            // suuntafiltteri (valinnainen): älä tarjoudu selän takaa
            if (preferFacingDirection)
            {
                float dx = col.transform.position.x - origin.position.x;
                bool facingRight = transform.lossyScale.x >= 0f;
                if (Mathf.Abs(dx) > 0.15f && Mathf.Sign(dx) != (facingRight ? 1f : -1f)) continue;
            }

            float d = (col.transform.position - origin.position).sqrMagnitude;
            if (d < bestDist) { best = cand; bestDist = d; }
        }

        if (!ReferenceEquals(best, current))
        {
            current?.OnFocusExit(gameObject);
            current = best;
            current?.OnFocusEnter(gameObject);
            // tänne voi päivittää prompt-UI:n jos haluat
        }
    }

    public void StartInteract()
    {
        if (busy || current == null || !current.CanInteract(gameObject)) return;

        if (current.Kind == InteractionKind.Tap)
        {
            Perform();
        }
        else
        {
            CancelHoldInternal();
            holdCo = StartCoroutine(HoldRoutine(current));
        }
    }

    public void CancelInteract()
    {
        CancelHoldInternal();
    }

    IEnumerator HoldRoutine(IInteractable target)
    {
        busy = true;
        float t = 0f, need = Mathf.Max(0.05f, target.HoldTime);
        // tähän voi syöttää UI:lle prosenttia t/need
        while (t < need && target == current)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (t >= need && target == current) Perform();
        busy = false;
        holdCo = null;
    }

    void Perform()
    {
        // Laukaise animaatio (jos on)
        if (animator && !string.IsNullOrEmpty(interactTrigger))
            animator.SetTrigger(interactTrigger);

        current?.Interact(gameObject);
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        busy = true;
        yield return new WaitForSeconds(cooldown);
        busy = false;
    }

    void CancelHoldInternal()
    {
        if (holdCo != null) StopCoroutine(holdCo);
        holdCo = null;
        busy = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!origin) origin = transform;
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(origin.position, radius);
    }
}

