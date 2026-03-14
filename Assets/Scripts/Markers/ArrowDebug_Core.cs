namespace UnityEngine.XR.Interaction.Toolkit.Transformers.Markers
{
    public abstract class ArrowDebug_Core : MonoBehaviour
    {
        protected Arrow _arrow;
        protected bool _initialized;
        
        public virtual void Init(Arrow arrow)
        {
            _arrow = arrow;
            _initialized = true;
        }
        
        public abstract void OnAimUpdate(Arrow.AimData aimData);

        public abstract void OnRelease();

        public abstract void OnHit();

        public abstract void OnArrowDestroyed();
    }
}