using System;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers.Markers
{
    public class ArrowDebug_Assist : ArrowDebug_Core
    {
        public GameObject fullAssistObject;
        public GameObject adjustedAssistObject;

        public float ringOffset = 0.4f;
        public float adjustedRingAdditionalOffset = 0.05f;

        private bool _clean;
        
        private void Awake()
        {
            fullAssistObject.transform.SetParent(null);
            fullAssistObject.SetActive(false);
            
            adjustedAssistObject.transform.SetParent(null);
            adjustedAssistObject.SetActive(false);
        }

        public override void OnAimUpdate(Arrow.AimData aimData)
        {
            if (fullAssistObject.activeSelf != aimData.valid)
            {
                fullAssistObject.SetActive(aimData.valid);
                adjustedAssistObject.SetActive(aimData.valid);
            }

            if (!aimData.valid || _clean)
            {
                return;
            }
            
            fullAssistObject.transform.position = _arrow.arrowTip.position + aimData.arrowVector * ringOffset;
            fullAssistObject.transform.rotation = aimData.fullAssistRotation;
            
            adjustedAssistObject.transform.rotation = aimData.adjustedAssistRotation;
            adjustedAssistObject.transform.position = _arrow.arrowTip.position + aimData.arrowVector * (ringOffset + adjustedRingAdditionalOffset);
        }

        public override void OnRelease()
        {
            Cleanup();
        }

        public override void OnHit()
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
            Destroy(fullAssistObject);
            Destroy(adjustedAssistObject);
        }
    }
}