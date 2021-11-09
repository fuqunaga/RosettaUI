using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    public abstract class RosettaUIRoot : MonoBehaviour
    {
        public ElementUpdater Updater { get; } = new ElementUpdater();

        #region Unity

        protected virtual void OnEnable()
        {
            Register(this);
        }

        protected virtual void OnDisable()
        {
            Unregister(this);
        }

        protected virtual void Update()
        {
            Updater.Update();
        }

        #endregion

        
        protected abstract void BuildInternal(Element element);

        public abstract bool WillUseKeyInput();

        public void Build(Element element)
        {
            BuildInternal(element);
            
            Updater.Register(element);
            Updater.RegisterWindowRecursive(element);
        }

        
        #region static

        private static readonly HashSet<RosettaUIRoot> Roots = new HashSet<RosettaUIRoot>();

        private static void Register(RosettaUIRoot root)
        {
            Roots.Add(root);
        }

        private static void Unregister(RosettaUIRoot root)
        {
            Roots.Remove(root);
        }

        public static bool WillUseKeyInputAny()
        {
            return Roots.Any(r => r.WillUseKeyInput());
        }

        #endregion
    }
}