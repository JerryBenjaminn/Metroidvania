using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public enum PlayerDeathMode { ReloadScene, RespawnAtTransform }

[RequireComponent(typeof(Health))]
public class PlayerDeathController : MonoBehaviour
{
    [Header("Behavior")]
    public PlayerDeathMode mode = PlayerDeathMode.ReloadScene;
    public float respawnDelay = 1.2f;           // odota animaation verran
    public Transform respawnPoint;              // k‰ytet‰‰n jos mode == RespawnAtTransform

    [Header("Refs")]
    [SerializeField] Animator animator;
    [SerializeField] string dieTrigger = "Die";
    [SerializeField] string deadBool = "Dead";

    [SerializeField] PlayerInput playerInput;
    [SerializeField] MonoBehaviour[] disableOnDeath; // esim. CharacterMotor, PlayerInputController, AbilityController
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D[] colliders;   // yleens‰ j‰tet‰‰n p‰‰lle, mutta voit disabloida jos haluat

    Health health;
    bool dead;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);

        var list = new System.Collections.Generic.List<MonoBehaviour>();
        var m = GetComponent<CharacterMotor>(); if (m) list.Add(m);
        var pic = GetComponent<PlayerInputController>(); if (pic) list.Add(pic);
        var ac = GetComponent<AbilityController>(); if (ac) list.Add(ac);
        disableOnDeath = list.ToArray();
    }

    void Awake() { health = GetComponent<Health>(); }
    void OnEnable() { health.OnDeath += HandleDeath; }
    void OnDisable() { if (health) health.OnDeath -= HandleDeath; }

    void HandleDeath()
    {
        if (dead) return; dead = true;

        if (playerInput) playerInput.enabled = false;
        foreach (var c in disableOnDeath) if (c) c.enabled = false;

        if (rb) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }

        if (animator)
        {
            if (!string.IsNullOrEmpty(deadBool)) animator.SetBool(deadBool, true);
            if (!string.IsNullOrEmpty(dieTrigger)) animator.SetTrigger(dieTrigger);
        }

        StartCoroutine(CoRespawn());
    }

    IEnumerator CoRespawn()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (mode == PlayerDeathMode.ReloadScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }

        // Respawn point
        if (!respawnPoint)
        {
            // jos respawnia ei ole m‰‰ritetty, fallback reloadiin
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }

        // Resetoi
        transform.position = respawnPoint.position;

        // her‰t‰ henkiin
        health.Heal(health.Max);
        if (rb) { rb.simulated = true; rb.linearVelocity = Vector2.zero; }
        foreach (var c in disableOnDeath) if (c) c.enabled = true;
        if (playerInput) playerInput.enabled = true;
        if (animator && !string.IsNullOrEmpty(deadBool)) animator.SetBool(deadBool, false);

        dead = false;
    }
}
