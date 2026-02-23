using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRPullInteractable : XRBaseInteractable
{
    public event Action<float> PullActionReleased;
    public event Action<float> PullUpdated;
    public event Action PullStarted;
    public event Action PullEnded;
    
    [Header("Pull Settings")] 
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    public Transform NotchPoint;

    public float pullAmount { get; private set; } = 0.0f;
    public Vector3 handPos { get; private set; }
    public bool isPulling { get; private set; }
    
    private LineRenderer _lineRenderer;
    private IXRSelectInteractor _pullingInteractor;

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnDrawGizmos()
    {
        if (_pullingInteractor != null)
        {
            Gizmos.color = Color.cyan;
            var iPos = _pullingInteractor.GetAttachTransform(this).position;
            Gizmos.DrawWireSphere(iPos, 0.05f);
            Gizmos.DrawLine(iPos, NotchPoint.position);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(NotchPoint.position, 0.05f);
        }
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        _pullingInteractor = args.interactorObject;
        isPulling = true;
        PullStarted?.Invoke();
    }

    public void Release()
    {
        PullActionReleased?.Invoke(pullAmount);
        PullEnded?.Invoke();
        _pullingInteractor = null;
        pullAmount = 0f;
        NotchPoint.transform.localPosition = new Vector3(NotchPoint.transform.localPosition.x, NotchPoint.transform.localPosition.y, 0f);
        isPulling = false;
        UpdateStringAndNotch();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected && _pullingInteractor != null)
            {
                Vector3 pullPosition = _pullingInteractor.GetAttachTransform(this).position;
                handPos = pullPosition;
                float previousPull = pullAmount;
                pullAmount = CalculatePull(pullPosition);

                if (previousPull != pullAmount)
                {
                    PullUpdated?.Invoke(pullAmount);
                }

                UpdateStringAndNotch();
                HandleHaptics();
            }
        }
    }
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        SetPullInteractor(args);
    }

    private float CalculatePull(Vector3 pullPosition)
    {
        Vector3 pullDirection = pullPosition - _startPoint.position;
        Vector3 targetDirection = _endPoint.position - _startPoint.position;
        float maxLength = targetDirection.magnitude;
        targetDirection.Normalize();
        float pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
        return Mathf.Clamp01(pullValue);
    }

    private void UpdateStringAndNotch()
    {
        Vector3 linePosition = Vector3.Lerp(_startPoint.localPosition, _endPoint.localPosition, pullAmount);
        NotchPoint.transform.localPosition = linePosition;
        _lineRenderer.SetPosition(1, linePosition);
    }

    private void HandleHaptics()
    {
        if (_pullingInteractor != null && _pullingInteractor is XRBaseInputInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(pullAmount, 0.1f);
        }
    }
}
