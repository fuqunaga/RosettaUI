using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace RosettaUI.Builder
{
    public abstract class BuildFramework<UIObj>
    {
        protected abstract IReadOnlyDictionary<Type, Func<Element, UIObj>> buildFuncTable { get; }
        readonly Dictionary<Element, UIObj> elementToUIObj = new Dictionary<Element, UIObj>();

        protected UIObj GetUIObj(Element element)
        {
            elementToUIObj.TryGetValue(element, out var uiObj);
            return uiObj;
        }

        protected void RegisterUIObj(Element element, UIObj uiObj) => elementToUIObj[element] = uiObj;
        protected void UnregisterUIObj(Element element) => elementToUIObj.Remove(element);


        public UIObj Build(Element element)
        {
            UIObj uiObj = default;
            if (element != null)
            {
                if (buildFuncTable.TryGetValue(element.GetType(), out var func))
                {
                    uiObj = func(element);
                    RegisterUIObj(element, uiObj);

                    Initialize(uiObj, element);
                }
                else
                {
                    Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                }
            }

            return uiObj;
        }

        protected virtual void Initialize(UIObj uiObj, Element element)
        {
            element.enableRx.SubscribeAndCallOnce((enable) => OnElementEnableChanged(element, uiObj, enable));
            element.interactableRx.SubscribeAndCallOnce((interactable) => OnElementInteractableChanged(element, uiObj, interactable));
            element.layoutRx.SubscribeAndCallOnce((layout) => OnElementLayoutChanged(element, uiObj, layout));
            element.onDestroy += OnDestroyElement;

            if (element is ElementGroup elementGroup)
            {
                elementGroup.onRebuildChildern += OnRebuildElementGroupChildren;
            }
        }

        protected abstract void OnElementEnableChanged(Element element, UIObj uiObj, bool enable);
        protected abstract void OnElementInteractableChanged(Element element, UIObj uiObj, bool interactable);
        protected abstract void OnElementLayoutChanged(Element element, UIObj uiObj, Layout layout);
        protected abstract void OnRebuildElementGroupChildren(ElementGroup elementGroup);
        protected abstract void OnDestroyElement(Element element);


        protected IEnumerable<UIObj> Build_ElementGroupChildren(ElementGroup elementGroup)
        {
            var elements = elementGroup.Elements;

            for (var i = 0; i < elements.Count; ++i)
            {
                var e = elements[i];
                var ve = Build(e);
                if (ve != null)
                {
                    yield return ve;
                }
            }
        }
    }
}