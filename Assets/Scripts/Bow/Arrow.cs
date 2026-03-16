using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers.Markers;

public class Arrow : MonoBehaviour
{
    // Public
    [Header("Core")]
    [Tooltip("What is the FOV of the cone used to limit what targets can be assisted to?")]
    public float aimFOV = 60;
    [Range(0,1), Tooltip("How much should the aim assist correct the trajectory of the arrow? 1 = 100% assist, 0.5 = 50% assist, 0 = no assist")]
    public float aimAssist = 0.2f;
    
    [Space]
    public float fireSpeed = 10;
    public float lifeTime = 3;
    public float raycastDist = 0.5f;
    public float digLength = 0.05f;
    public float forceMultiplier = 1;

    [Space]
    public float baseDamage = 10;
    
    [Space]
    public Transform arrowTip;

    public List<ArrowDebug_Core> debugObjects;
    

    public LayerMask physicsLayer;
    
    // Private
    
    private XRPullInteractable _pullInteractable;
    private XRGrabInteractable _bowInteractable;

    private bool _released;
    private bool _hit;
    private Vector3 lastPhysicsPosition;
    private float _adjustedFireSpeed;
    
    public float AimRange => fireSpeed * lifeTime * 1.15f;

    private Transform _camera;
    
    // Target management
    private struct TargetData
    {
        public bool valid;
        public float dot;
        public Vector3 releaseVector;
    }
    
    private Dictionary<Target, TargetData> targets = new();
    
    [Space, ReadOnly]
    public Target CurrentTarget;
    
    public struct AimData
    {
        public bool valid;
        public Vector3 arrowVector;
        public Quaternion adjustedAssistRotation;
        public Quaternion fullAssistRotation;
    }
    
    // Lifetime
    public void Init(XRPullInteractable pullInteractable, XRGrabInteractable bowInteractable)
    {
        _pullInteractable = pullInteractable;
        _bowInteractable = bowInteractable;
        _pullInteractable.PullUpdated += OnPullUpdated;
        _pullInteractable.PullActionReleased += OnRelease;
        
        _camera = Camera.main.transform;

        foreach (var obj in debugObjects)
        {
            obj.Init(this);
        }
    }

    private void OnDestroy()
    {
        if (!_released)
        {
            _pullInteractable.PullUpdated -= OnPullUpdated;
            _pullInteractable.PullActionReleased -= OnRelease;
        }

        foreach (var obj in debugObjects)
        {
            obj?.OnArrowDestroyed();
        }
    }

    // Updates
    private void Update()
    {
        if (_released && !_hit)
        {
            FlightUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!_released)
        {
            PullFixedUpdate();
            return;
        }

        
        if (!_hit)
        {
            FlightFixedUpdate();
        }
    }

    private void PullFixedUpdate()
    {
        UpdateTargets();
        
        var foundValidTarget = TryGetBestTarget(out var bestTarget, out var bestTargetData);
        
        if (CurrentTarget != bestTarget)
        {
            CurrentTarget?.SetHighlight(false);
            CurrentTarget = bestTarget;
            CurrentTarget?.SetHighlight(true);
        }

        TryGetAssistedRotation(out var adjustedAssistRotation, out var fullAssistRotation);

        AimData aimData = new AimData()
        {
            valid = foundValidTarget,
            arrowVector = transform.forward,
            adjustedAssistRotation = adjustedAssistRotation,
            fullAssistRotation = fullAssistRotation
        };

        foreach (var obj in debugObjects)
        {
            obj.OnAimUpdate(aimData);
        }
        
        // No race condition with FlightFixedUpate since the methods are never run sequentially
        lastPhysicsPosition = transform.position;
    }

    private void FlightUpdate()
    {
        transform.position += transform.forward * _adjustedFireSpeed * Time.deltaTime;
    }
    
    private void FlightFixedUpdate()
    {
        // Check space between where we were last physics update, and where we are now
        var stepDist = (transform.position - lastPhysicsPosition).magnitude;
        
        var ray = new Ray(lastPhysicsPosition, transform.forward);
        if (Physics.Raycast(ray, out var hit, stepDist + raycastDist, physicsLayer.value))
        {
            hit.collider.TryGetComponent<Target>(out var target);
            OnHit(hit.collider, target);
        }
        
        lastPhysicsPosition = transform.position;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var rToPoint = transform.position + transform.forward * raycastDist;
        Gizmos.DrawLine(transform.position, rToPoint);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rToPoint, rToPoint + transform.forward * digLength);
    }

    // Events
    private void OnPullUpdated(float obj)
    {
        // Get direction (and possible target)
        // Update arrow direction
        var bowGripPos = _bowInteractable.attachTransform.position;
        var notchHandPos = _pullInteractable.handPos;
        var fireDir = (bowGripPos - notchHandPos).normalized;

        if (fireDir != Vector3.zero)
        {
            var upDir = -_bowInteractable.interactorsSelecting[0].GetAttachTransform(_bowInteractable).right;
            var targetRot = Quaternion.LookRotation(fireDir, upDir);
            transform.rotation = targetRot;
        }
    }
    
    private void OnRelease(float draw)
    {
        _released = true;
        _adjustedFireSpeed = Mathf.Lerp(0, fireSpeed, draw);
        
        _pullInteractable.PullUpdated -= OnPullUpdated;
        _pullInteractable.PullActionReleased -= OnRelease;

        transform.SetParent(null);

        if (CurrentTarget != null)
        {
            if (TryGetAssistedRotation(out var adjustedAssistRotation, out var fullAssistRotation))
            {
                transform.rotation = adjustedAssistRotation;
            }
            CurrentTarget.SetHighlight(false);
        }
        
        Destroy(gameObject, lifeTime);
        
        foreach (var obj in debugObjects)
        {
            obj?.OnRelease();
        }
    }

    private void OnHit(Collider col, Target target)
    {
        transform.position += transform.forward * digLength;
        _hit = true;
        
        transform.SetParent(col.transform);
        foreach (var obj in debugObjects)
        {
            obj.OnHit(target);
        }

        if (target != null)
        {
            target.Hit(this, transform.forward * _adjustedFireSpeed * forceMultiplier);
        }
    }
    
    private void UpdateTargets()
    {
        targets.Clear();
        foreach (var target in Target.All)
        {
            TryGetTargetData(target, out var data);
            targets[target] = data;
        }
    }

    // Helpers
    private bool TryGetAssistedRotation(out Quaternion adjustedAssistRotation, out Quaternion fullAssistRotation)
    {
        if (CurrentTarget == null || !targets.TryGetValue(CurrentTarget, out var data) || !data.valid)
        {
            adjustedAssistRotation = Quaternion.identity;
            fullAssistRotation = Quaternion.identity;
            return false;
        }
        var targetRot = Quaternion.LookRotation(data.releaseVector);
        adjustedAssistRotation = Quaternion.Slerp(transform.rotation, targetRot, aimAssist);
        fullAssistRotation = targetRot;
        return true;
    }
    
    private bool TryGetBestTarget(out Target bestTarget, out TargetData bestData)
    {
        bestTarget = null;
        bestData = default;

        foreach (var kvp in targets)
        {
            if (!kvp.Value.valid) continue;
            if (kvp.Value.dot > bestData.dot)
            {
                bestTarget = kvp.Key;
                bestData = kvp.Value;
            }
        }

        return bestTarget != null;
    }

    private bool TryGetTargetData(Target target, out TargetData data)
    {
        if (TryGetTargetData(target, out var dot, out var vector))
        {
            data = new TargetData()
            {
                dot = dot,
                valid = true,
                releaseVector = vector
            };
            return true;
        }
        else
        {
            data = new TargetData()
            {
                valid = false
            };
            return false;
        }
    }
    
    private bool TryGetTargetData(Target target, out float dot, out Vector3 releaseVector)
    {
        releaseVector = target.transform.position - transform.position;
        
        dot = Vector3.Dot(transform.forward, (target.transform.position - transform.position).normalized);
        
        return dot > Mathf.Cos(aimFOV * 0.5f * Mathf.Deg2Rad);
    }
}