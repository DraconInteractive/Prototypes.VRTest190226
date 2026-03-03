using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Arrow : MonoBehaviour
{
    public float aimFOV = 60;
    
    public float fireSpeed = 10;
    public float lifeTime = 3;
    public float raycastDist = 0.5f;
    public float digLength = 0.05f;

    public Transform arrowTip;
    
    public GameObject aimMarker;

    public GameObject targetRingUI;
    public float targetRingOffset;
    public LineRenderer targetLine;
    
    public float markerSmoothing = 25f;

    public LayerMask physicsLayer;
    
    private XRPullInteractable _pullInteractable;
    private XRGrabInteractable _bowInteractable;

    private bool _released;
    private bool _hit;

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
        targetLine.transform.SetParent(null);
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

        if (targetLine != null)
        {
            Destroy(targetLine.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (!_released)
        {
            UpdateTargets();

            var best = TryGetBestTarget(out Target bestTarget, out TargetData bestTargetData);

            if (lastBestTarget != best)
            {
                if (lastBestTarget != null)
                {
                    lastBestTarget.SetHighlight(false);
                    targetRingUI.SetActive(false);
                    targetLine.gameObject.SetActive(false);
                }

                lastBestTarget = bestTarget;
                if (best)
                {
                    bestTarget.SetHighlight(true);
                    targetRingUI.SetActive(true);
                    targetLine.gameObject.SetActive(true);
                }
            }

            if (best)
            {
                targetRingUI.transform.position = arrowTip.position + bestTargetData.releaseVector * targetRingOffset;
                targetRingUI.transform.rotation = Quaternion.LookRotation(bestTargetData.releaseVector);
                // TODO set target line positions here
                // Start position is same calc as targetRingUI position, but * 1.1f so it sits slightly in front
                // then bezier curve from start to bestTarget.closestPoint
            }
            
            // Marker showing unmodified arrow trajectory
            var markerRay = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(markerRay, out var hit, (AimRange), physicsLayer.value))
            {
                if (!aimMarker.activeSelf)
                {
                    aimMarker.SetActive(true);
                }
                aimMarker.transform.position = Vector3.Lerp(
                    aimMarker.transform.position,
                    hit.point,
                    markerSmoothing * Time.fixedDeltaTime);
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
        Destroy(gameObject, lifeTime);
        Destroy(aimMarker);
        Destroy(targetRingUI);
        Destroy(targetLine.gameObject);

    }

    private void UpdateTargets()
    {
        targets.Clear();
        foreach (var target in Target.All)
        {
            var data = TryGetTargetData(target, out float dot, out Vector3 point, out Vector3 vector);
            targets[target] = new TargetData()
            {
                valid = data,
                dot = dot,
                closestPoint = point,
                releaseVector = vector
            };
        }
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
            Vector3 toCenter = target.collider.bounds.center - transform.position;
            float t = Mathf.Max(0f, Vector3.Dot(toCenter, transform.forward));
            Vector3 pointOnRay = transform.position + transform.forward * t;
            closestPoint = target.collider.ClosestPoint(pointOnRay);

            releaseVector = (closestPoint - transform.position).normalized;
        }
        dot = Vector3.Dot(transform.forward, (closestPoint -
                                              transform.position).normalized);
        
        return dot > Mathf.Cos(aimFOV * 0.5f * Mathf.Deg2Rad);
    }
}