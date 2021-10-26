using UnityEngine;

namespace RosettaUI
{
    
    public abstract class RosettaUIRoot : MonoBehaviour
    {
        protected Element rootElement;

        protected abstract void OnEnable();
        protected abstract void OnDisable();
        protected abstract void BuildInternal(Element element);

        protected virtual void Update()
        {
            rootElement?.Update();
        }

        public void Build(Element element)
        {
            rootElement = element;
            BuildInternal(element);
        }

        
    }
}