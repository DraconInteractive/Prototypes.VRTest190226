using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{
    public static List<Target> All = new();
    
    public UnityEvent<bool> OnSetHighlight;
    
    public UnityEvent<Arrow> OnHit;

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

    public void Hit(Arrow arrow)
    {
        OnHit.Invoke(arrow);
    }
}
