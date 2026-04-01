using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BowController : MonoBehaviour
{
    public static BowController Instance;
    
    public XRGrabInteractable BowInteractable;
    public XRPullInteractable NotchInteractable;

    public Arrow arrowPrefab;

    private Arrow spawnedArrow;

    public UnityEvent OnGrabbed;
    public UnityEvent OnReleased;

    public bool InHand { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        NotchInteractable.PullStarted += OnPullStart;
    }

    private void OnDisable()
    {
        NotchInteractable.PullStarted -= OnPullStart;
    }

    public void OnBowGrab()
    {
        InHand = true;
        OnGrabbed?.Invoke();
    }

    public void OnBowRelease()
    {
        InHand = false;
        OnReleased?.Invoke();
    }
    
    private void OnPullStart()
    {
        // TODO Set pos / rot from notch
        spawnedArrow = Instantiate(arrowPrefab, NotchInteractable.NotchPoint) as Arrow;
        spawnedArrow.Init(NotchInteractable, BowInteractable);
    }
}
