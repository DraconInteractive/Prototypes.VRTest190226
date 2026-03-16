using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BowController : MonoBehaviour
{
    public XRGrabInteractable BowInteractable;
    public XRPullInteractable NotchInteractable;

    public Arrow arrowPrefab;

    private Arrow spawnedArrow;
    private void OnEnable()
    {
        NotchInteractable.PullStarted += OnPullStart;
    }

    private void OnDisable()
    {
        NotchInteractable.PullStarted -= OnPullStart;
    }

    private void OnPullStart()
    {
        // TODO Set pos / rot from notch
        spawnedArrow = Instantiate(arrowPrefab, NotchInteractable.NotchPoint) as Arrow;
        spawnedArrow.Init(NotchInteractable, BowInteractable);
    }
}
