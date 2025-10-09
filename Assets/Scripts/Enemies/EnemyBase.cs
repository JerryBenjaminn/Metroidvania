using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public abstract class EnemyBase : ActorBase
{
    protected Health health;

    [Header("Death")]
    public Animator animator;
    [SerializeField] string dieTrigger = "Die";
    [SerializeField] float destroyDelay = 0.8f;       // jos ei käytä animaatioeventtiä
    [SerializeField] Collider2D[] collidersToDisable; // jos tyhjä  haetaan automaattisesti
    [SerializeField] ContactDamage contactDamage;     // optional, disabloidaan kuolemassa

    protected virtual void Start()
    {
        health = GetComponent<Health>();
        health.OnDeath += HandleDeath;

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!contactDamage) contactDamage = GetComponent<ContactDamage>();
        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider2D>(true);
    }

    protected virtual void OnDestroy()
    {
        if (health) health.OnDeath -= HandleDeath;
    }

    protected virtual void HandleDeath()
    {
        // lopeta vahinko ja fysiikka
        if (contactDamage) contactDamage.enabled = false;
        if (rb) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }

        foreach (var c in collidersToDisable)
            if (c) c.enabled = false;

        if (animator && !string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);

        // Jos et käytä animaatioeventtiä, poistutaan viiveellä
        StartCoroutine(CoDestroyAfterDelay());
    }

    IEnumerator CoDestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    // Voit kutsua tätä suoraan animaatioeventistä viimeisessä framessa
    public void Animation_DestroySelf() => Destroy(gameObject);
}
