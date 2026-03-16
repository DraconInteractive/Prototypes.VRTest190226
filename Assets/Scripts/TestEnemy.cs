using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public float MaxHealth;
    // Exposed for testing
    public float CurrentHealth;

    public bool startAlive = true;
    public float deathDestructionTimer = 10;
    
    public List<Target> targets;

    private RagdollController _ragdollController;

    private bool _alive;
    
    private void Awake()
    {
        _ragdollController = GetComponent<RagdollController>();
    }

    private void Start()
    {
        if (startAlive)
        {
            _alive = true;
        }
    }

    private void OnEnable()
    {
        foreach (var target in targets)
        {
            target.OnHit.AddListener(OnTargetHit);
        }
    }

    private void OnDisable()
    {
        foreach (var target in targets)
        {
            target.OnHit.RemoveListener(OnTargetHit);
        }
    }

    private void OnValidate()
    {
        if (targets == null || targets.Count == 0)
        {
            targets = GetComponentsInChildren<Target>(includeInactive: true).ToList();
        }
    }
    
    private void Damage(int damage, Vector3 force = default)
    {
        if (!_alive) return;
        
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    [ContextMenu("Die")]
    private void Die()
    {
        if (!_alive) return;

        CurrentHealth = 0;
        
        _ragdollController?.ActivateRagdoll();
        if (deathDestructionTimer > 0)
        {
            Destroy(gameObject, deathDestructionTimer);
        }
    }
    
    private void OnTargetHit(Target target, Arrow arrow, Vector3 force)
    {
        // No alive check here since I might want to add post-mortem hit handling
        // Forwarding force in case we want to do a multiplier kind of thing later
        
        // Setting backup damage to 10 for testing
        Damage(Mathf.RoundToInt(arrow?.baseDamage ?? 10), force);
    }
    
}
