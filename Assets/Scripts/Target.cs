using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{
    public static List<Target> All = new();
    
    public UnityEvent<bool> OnSetHighlight;
    
    public UnityEvent<Target, Arrow, Vector3> OnHit;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    public void SetHighlight(bool active)
    {
        OnSetHighlight?.Invoke(active);
    }

    public float testHitForce = 5;
    public Vector3 testHitDirection;
    
    [ContextMenu("Test Hit")]
    public void TestHit()
    {
        Hit(null, testHitDirection.normalized * testHitForce);
    }
    
    public void Hit(Arrow arrow, Vector3 force)
    {
        OnHit.Invoke(this, arrow, force);
        _rb?.AddForce(force, ForceMode.Impulse);
    }
}
