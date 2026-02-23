using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BowController : MonoBehaviour
{
    public Transform BowRoot;
    public XRGrabInteractable BowInteractable;
    public XRPullInteractable NotchInteractable;

    public float rotationSmoothing = 90;

    public bool rotating = false;

    private Quaternion lastRotation;
    
    private void LateUpdate()
    {
        rotating = BowInteractable.interactorsSelecting.Count > 0 && NotchInteractable.isPulling;

        if (!rotating) return;
        
        var bowGripPos = BowInteractable.attachTransform.position;
        var notchHandPos = NotchInteractable.handPos;
            
        var fireDir = (bowGripPos - notchHandPos).normalized;
        var upDir = BowInteractable.interactorsSelecting[0].GetAttachTransform(BowInteractable).up;
        Quaternion targetRot = Quaternion.LookRotation(fireDir, upDir);

        BowRoot.rotation = Quaternion.Slerp(lastRotation, targetRot, rotationSmoothing * Time.deltaTime);

        lastRotation = targetRot;
    }
}
