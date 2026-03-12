using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Arrow : MonoBehaviour
{
    [Tooltip("What is the FOV of the cone used to limit what targets can be assisted to?")]
    public float aimFOV = 60;
    [Range(0,1), Tooltip("How much should the aim assist correct the trajectory of the arrow? 1 = 100% assist, 0.5 = 50% assist, 0 = no assist")]
    public float aimAssist = 0.2f;
    [Range(0,1), Tooltip("How far towards the centre of the object should the arrow target when assisting? 1 = direct centre, 0 = closest edge")]
    public float centreAssist = 0.2f;
    
    public float fireSpeed = 10;
    public float lifeTime = 3;
    public float raycastDist = 0.5f;
    public float digLength = 0.05f;

    public Transform arrowTip;
    
    public GameObject aimMarker;
    public float markerDownInitialOffset = 0.25f;
    public float markerDownDistanceOffset = 0.05f;
    
    public GameObject targetRingUI;
    public float targetRingOffset;

    public GameObject assistDebugObject;
    
    public float markerSmoothing = 25f;

    public LayerMask physicsLayer;
    
    private XRPullInteractable _pullInteractable;
    private XRGrabInteractable _bowInteractable;

    private bool _released;
    private bool _hit;

    private Transform _camera;
    
    private float AimRange => fireSpeed * lifeTime * 1.15f;

    private struct TargetData
    {
        public bool valid;
        public float dot;
        public Vector3 closestPoint;
        public Vector3 releaseVector;
    }

    private Dictionary<Target, TargetData> targets = new();
    private Target lastBestTarget;
    
    public void Init(XRPullInteractable pullInteractable, XRGrabInteractable bowInteractable)
    {
        _pullInteractable = pullInteractable;
        _bowInteractable = bowInteractable;
        _pullInteractable.PullUpdated += OnPullUpdated;
        _pullInteractable.PullActionReleased += OnRelease;
        
        aimMarker.transform.SetParent(null);
        aimMarker?.SetActive(false);

        targetRingUI.transform.SetParent(null);
        assistDebugObject.transform.SetParent(null);
        _camera = Camera.main.transform;
    }

    private void OnDestroy()
    {
        if (!_released)
        {
            _pullInteractable.PullUpdated -= OnPullUpdated;
            _pullInteractable.PullActionReleased -= OnRelease;
        }

        if (aimMarker != null)
        {
            Destroy(aimMarker);
        }

        if (targetRingUI != null)
        {
            Destroy(targetRingUI);
        }

        if (assistDebugObject != null)
        {
            Destroy(assistDebugObject);
        }
    }

    private void FixedUpdate()
    {
        if (!_released)
        {
            UpdateTargets();

            var best = TryGetBestTarget(out var bestTarget, out var bestTargetData);

            if (lastBestTarget != best)
            {
                if (lastBestTarget != null)
                {
                    lastBestTarget.SetHighlight(false);
                    targetRingUI.SetActive(false);
                    assistDebugObject.SetActive(false);
                }

                lastBestTarget = bestTarget;
                
                if (best)
                {
                    bestTarget.SetHighlight(true);
                    targetRingUI.SetActive(true);
                    assistDebugObject.SetActive(true);
                }
            }

            if (best)
            {
                targetRingUI.transform.position = arrowTip.position + bestTargetData.releaseVector * targetRingOffset;
                targetRingUI.transform.rotation = Quaternion.LookRotation(bestTargetData.releaseVector);

                if (TryGetAssistedRotation(out var assistedRotation))
                {
                    assistDebugObject.transform.position = targetRingUI.transform.position;
                    assistDebugObject.transform.rotation = assistedRotation;
                }
            }
            
            // Marker showing unmodified arrow trajectory
            var markerRay = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(markerRay, out var hit, (AimRange), physicsLayer.value))
            {
                if (!aimMarker.activeSelf)
                {
                    aimMarker.SetActive(true);
                }
                
                var down = markerDownInitialOffset +
                           Vector3.Distance(hit.point, transform.position) * markerDownDistanceOffset;
                
                aimMarker.transform.position = Vector3.Lerp(
                    aimMarker.transform.position,
                    hit.point + Vector3.down * down,
                    markerSmoothing * Time.fixedDeltaTime);

                // Point marker at camera
                aimMarker.transform.rotation =
                    Quaternion.LookRotation((_camera.transform.position - aimMarker.transform.position).normalized); 
            }
            else if (aimMarker.activeSelf)
            {
                aimMarker.SetActive(false);
            }
            return;
        }

        
        if (!_hit)
        {
            // TODO: Separate physics and render / 'true' position
            // So in fixed update update the 'physics' position. This is used for collisions etc
            // In normal update, move the arrows actual body towards the physics position smoothly
            // If we want, we can even move the arrow slightly forward of the current physics position (if we wanted predictive rendering). Not required for now
            
            Vector3 startPos = transform.position;
            float stepDist = fireSpeed * Time.fixedDeltaTime;

            transform.position += transform.forward * stepDist;
            
            // Using cached pos so that it checks along the move trajectory already covered, not just distance in front of the arrow
            // If the item hit is in front of the arrow it will jump forward, forcing the contact instead of receiving it
            var ray = new Ray(startPos, transform.forward);
            if (Physics.Raycast(ray, out var hit, stepDist + raycastDist, physicsLayer.value))
            {
                transform.position += transform.forward * digLength;
                _hit = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var rToPoint = transform.position + transform.forward * raycastDist;
        Gizmos.DrawLine(transform.position, rToPoint);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rToPoint, rToPoint + transform.forward * digLength);
    }

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
        _pullInteractable.PullUpdated -= OnPullUpdated;
        _pullInteractable.PullActionReleased -= OnRelease;

        transform.SetParent(null);

        if (lastBestTarget != null)
        {
            if (TryGetAssistedRotation(out var assistedRotation))
            {
                transform.rotation = assistedRotation;
            }
        }
        Destroy(gameObject, lifeTime);
        Destroy(aimMarker);
        Destroy(targetRingUI);
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

    private bool TryGetAssistedRotation(out Quaternion assistedRotation)
    {
        if (lastBestTarget == null || !targets.ContainsKey(lastBestTarget) || !targets[lastBestTarget].valid)
        {
            assistedRotation = Quaternion.identity;
            return false;
        }
        var targetRot = Quaternion.LookRotation(targets[lastBestTarget].releaseVector);
        assistedRotation = Quaternion.Slerp(transform.rotation, targetRot, aimAssist);
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
        if (TryGetTargetData(target, out var dot, out var closest, out var vector))
        {
            data = new TargetData()
            {
                closestPoint = closest,
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
    
    private bool TryGetTargetData(Target target, out float dot, out Vector3 closestPoint, out Vector3 releaseVector)
    {
        if (target.collider == null)
        {
            dot = 0;
            closestPoint = Vector3.zero;
            releaseVector = Vector3.zero;
            return false;
        }
        
        var ray = new Ray(transform.position, transform.forward);
        if (target.collider.Raycast(ray, out var hit, AimRange))
        {
            closestPoint = hit.point;
            releaseVector = transform.forward;
        }
        else
        {
            var toCenter = target.collider.bounds.center - transform.position;
            var t = Mathf.Max(0f, Vector3.Dot(toCenter, transform.forward));
            var pointOnRay = transform.position + transform.forward * t;
            closestPoint = target.collider.ClosestPoint(pointOnRay);

            var edgeVector = (closestPoint - transform.position).normalized;

            releaseVector = Vector3.Lerp(edgeVector, toCenter, centreAssist);
        }
        dot = Vector3.Dot(transform.forward, (closestPoint -
                                              transform.position).normalized);
        
        return dot > Mathf.Cos(aimFOV * 0.5f * Mathf.Deg2Rad);
    }
}