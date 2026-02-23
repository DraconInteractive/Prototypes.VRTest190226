using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class XRDualBowGrabTransformer : XRBaseGrabTransformer
{
    Vector3 m_LastUp;

    public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
    {
        base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
        if (grabInteractable.interactorsSelecting.Count == 2)
            m_LastUp = grabInteractable.transform.up;
    }
    
    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        switch (updatePhase)
        {
            case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
            case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
            {
                if (grabInteractable.interactorsSelecting.Count == 1)
                    return;
                
                UpdateTargetMulti(grabInteractable, ref targetPose);

                break;
            }
        }
    }

    void UpdateTargetMulti(XRGrabInteractable grabInteractable, ref Pose targetPose)
    {
        var primaryAttachPose = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).GetWorldPose();
        var secondaryAttachPose = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable).GetWorldPose();
        var interactorAttachPose = primaryAttachPose;
        interactorAttachPose.position = primaryAttachPose.position;
        var forward = (secondaryAttachPose.position - primaryAttachPose.position).normalized;
        var up = primaryAttachPose.up;
        var right = primaryAttachPose.right;
        interactorAttachPose.rotation = Quaternion.LookRotation(forward, up);

        targetPose.position = interactorAttachPose.position;
        targetPose.rotation = interactorAttachPose.rotation;
    }
    
    
}
