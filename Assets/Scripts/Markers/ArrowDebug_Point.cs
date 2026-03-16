using System;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers.Markers
{
    public class ArrowDebug_Point : ArrowDebug_Core
    {
        public GameObject markerObject;
        public float markerSmoothing = 25f;
    
        private bool _clean;

        private Vector3 targetMarkerPos;
        private Transform _camera;
        
        public override void Init(Arrow arrow)
        {
            base.Init(arrow);
            _camera = Camera.main.transform;
            targetMarkerPos = _arrow.arrowTip.position + _arrow.transform.forward * _arrow.AimRange;
        }

        public override void OnAimUpdate(Arrow.AimData aimData)
        {
            if (_clean || !_initialized)
            {
                return;
            }
            
            var markerRay = new Ray(_arrow.transform.position, _arrow.transform.forward);
            if (Physics.Raycast(markerRay, out var hit, _arrow.AimRange, _arrow.physicsLayer.value))
            {
                targetMarkerPos = hit.point;
            }
            else
            {
                targetMarkerPos = _arrow.arrowTip.position + aimData.arrowVector * _arrow.AimRange;
            }
        }

        private void Update()
        {
            if (_clean)
            {
                return;
            }
            
            markerObject.transform.position = Vector3.Lerp(
                markerObject.transform.position,
                targetMarkerPos,
                markerSmoothing * Time.deltaTime);
            
            // Point marker at camera
            markerObject.transform.rotation =
                Quaternion.LookRotation((_camera.position - markerObject.transform.position).normalized); 
        }

        public override void OnRelease()
        {
            Cleanup();
        }

        public override void OnHit(Target target)
        {
            
        }
        
        public override void OnArrowDestroyed()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_clean) return;
            _clean = true;
            Destroy(markerObject);
        }
    }
}