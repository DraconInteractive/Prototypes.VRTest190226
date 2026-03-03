using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{
    public static List<Target> All = new();

    public Collider collider;

    public UnityEvent<bool> OnSetHighlight;
    private void Awake()
    {
        if (collider == null)
        {
            collider = GetComponentInChildren<Collider>();
        }
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
}
