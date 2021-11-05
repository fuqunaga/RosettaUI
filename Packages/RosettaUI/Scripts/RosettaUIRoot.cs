using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    public abstract class RosettaUIRoot : MonoBehaviour
    {
        #region static

        private static readonly HashSet<RosettaUIRoot> Roots = new HashSet<RosettaUIRoot>();
        static void Register(RosettaUIRoot root) => Roots.Add(root);
        static void Unregister(RosettaUIRoot root) => Roots.Remove(root);

        public static bool WillUseKeyInputAny() => Roots.Any(r => r.WillUseKeyInput());

        #endregion
        
        
        protected Element rootElement;

        protected virtual void OnEnable() => Register(this);
        protected virtual void OnDisable() => Unregister(this);
        protected abstract void BuildInternal(Element element);

        public abstract bool WillUseKeyInput();

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