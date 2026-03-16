using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public List<Rigidbody> physicsObjects;

    public bool setKinematicOnAwake;
    private void Awake()
    {
        if (setKinematicOnAwake)
        {
            ToggleRagdoll(false);
        }
    }

    [ContextMenu("Activate")]
    public void ActivateRagdoll()
    {
        ToggleRagdoll(true);
    }

    [ContextMenu("Deactivate")]
    public void DeactivateRagdoll()
    {
        ToggleRagdoll(false);
    }
    
    public void ToggleRagdoll(bool state)
    {
        foreach (var obj in physicsObjects)
        {
            obj.isKinematic = !state;
        }
    }

    private void OnValidate()
    {
        if (physicsObjects == null || physicsObjects.Count == 0)
        {
            var objs = GetComponentsInChildren<Rigidbody>(includeInactive: true);
            physicsObjects = objs.ToList();
        }
    }
}
