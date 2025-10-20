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
    public float respawnDelay = 1.2f;           // Wait for death animation
    public Transform respawnPoint;             // Used if mode == RespawnAtTransform

    [Header("Refs")]
    [SerializeField] Animator animator;
    [SerializeField] string dieTrigger = "Die";
    [SerializeField] string deadBool = "Dead";

    [SerializeField] PlayerInput playerInput;
    [SerializeField] MonoBehaviour[] disableOnDeath; // e.g., CharacterMotor, PlayerInputController, AbilityController
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D[] colliders;   // Usually left enabled, but can be disabled if needed

    [Header("Boss Arena")]
    [SerializeField] BossArena bossArena;      // Reference to the BossArena script

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
        if (dead) return;
        dead = true;

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

        Time.timeScale = 1.0f;

        // Reset the boss fight if a BossArena is assigned
        if (bossArena != null)
        {
            bossArena.ResetBossFight();
        }

        // Load the active slot via SaveManager to restore all ISaveable states
        int slot = SaveManager.Instance && SaveManager.Instance.HasActiveSlot
            ? SaveManager.Instance.CurrentSlot
            : 0;

        GameManager.Instance.LoadGame(slot);

        // Reset player position
        transform.position = respawnPoint.position;

        // Revive the player
        health.Heal(health.Max);
        if (rb) { rb.simulated = true; rb.linearVelocity = Vector2.zero; }
        foreach (var c in disableOnDeath) if (c) c.enabled = true;
        if (playerInput) playerInput.enabled = true;
        if (animator && !string.IsNullOrEmpty(deadBool)) animator.SetBool(deadBool, false);

        dead = false;
    }
}