using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Arrow : MonoBehaviour
{
    public float fireSpeed = 10;
    public float lifeTime = 3;
    public float raycastDist = 0.5f;
    public float digLength = 0.05f;

    public GameObject aimMarker;
    
    public float markerSmoothing = 25f;

    public LayerMask physicsLayer;
    
    private XRPullInteractable _pullInteractable;
    private XRGrabInteractable _bowInteractable;

    private bool _released;
    private bool _hit;
    
    public void Init(XRPullInteractable pullInteractable, XRGrabInteractable bowInteractable)
    {
        _pullInteractable = pullInteractable;
        _bowInteractable = bowInteractable;
        _pullInteractable.PullUpdated += OnPullUpdated;
        _pullInteractable.PullActionReleased += OnRelease;
        
        aimMarker.transform.SetParent(null);
        aimMarker?.SetActive(false);
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
    }

    private void FixedUpdate()
    {
        if (!_released)
        {
            // If valid target found in trajectory, show marker. Else hide.
            var markerRay = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(markerRay, out var hit, (fireSpeed * lifeTime * 1.15f), physicsLayer.value))
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
    }
}