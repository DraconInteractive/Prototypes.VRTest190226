using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Arrow : MonoBehaviour
{
    public float fireForce = 10;
    public float lifeTime = 3;
    public float raycastDist = 0.5f;
    public float digLength = 0.05f;
    
    public LayerMask physicsLayer;
    
    private XRPullInteractable _pullInteractable;
    private XRGrabInteractable _bowInteractable;

    private bool _released;
    private bool _hit;
    
    private Rigidbody rb;
    public void Init(XRPullInteractable pullInteractable, XRGrabInteractable bowInteractable)
    {
        _pullInteractable = pullInteractable;
        _bowInteractable = bowInteractable;
        _pullInteractable.PullUpdated += OnPullUpdated;
        _pullInteractable.PullActionReleased += OnRelease;
    }

    private void OnDestroy()
    {
        if (!_released)
        {
            _pullInteractable.PullUpdated -= OnPullUpdated;
            _pullInteractable.PullActionReleased -= OnRelease;
        }
    }

    private void Update()
    {
        if (!_released)
        {
            return;
        }

        if (!_hit)
        {
            var ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out var hit, raycastDist, LayerMask.GetMask()))
            {
                transform.position += -(hit.normal * digLength);
                Destroy(rb);
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
        rb = gameObject.AddComponent<Rigidbody>();
        rb.AddForce(transform.forward * fireForce, ForceMode.Impulse);
        Destroy(gameObject, lifeTime);
    }
}