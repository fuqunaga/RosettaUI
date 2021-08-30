using System;
using System.Collections.Generic;
using UnityEngine;


namespace RosettaUI.Builder
{
    public abstract class BuildFramework<UIObj>
    {
        protected abstract IReadOnlyDictionary<Type, Func<Element, UIObj>> buildFuncTable { get; }
        protected readonly Dictionary<Element, UIObj> elementToUIObj = new Dictionary<Element, UIObj>();

        public UIObj Build(Element element)
        {
            UIObj uiObj = default;
            if (element != null)
            {
                if (buildFuncTable.TryGetValue(element.GetType(), out var func))
                {
                    uiObj = func(element);
                    elementToUIObj[element] = uiObj;

                    Initialize(uiObj, element);
                    NotifyInitializeProperties(element);
                }
                else
                {
                    Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                }
            }

            return uiObj;
        }

        protected virtual void Initialize(UIObj obj, Element element) { }



        void NotifyInitializeProperties(Element element)
        {
            element.enableRx.NotifyPropertyValue();
            element.interactableRx.NotifyPropertyValue();

            if (element is FoldElement fold)
            {
                fold.isOpenRx.NotifyPropertyValue();
            }

            element.layoutRx.NotifyPropertyValue();
        }
    }
}