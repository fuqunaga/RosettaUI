using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RosettaUI.Builder
{
    public abstract class BuilderBase<TUIObj>
    {
        protected abstract IReadOnlyDictionary<Type, Func<Element, TUIObj>> BuildFuncTable { get; }
        readonly Dictionary<Element, TUIObj> _elementToUIObj = new Dictionary<Element, TUIObj>();

        protected TUIObj GetUIObj(Element element)
        {
            _elementToUIObj.TryGetValue(element, out var uiObj);
            return uiObj;
        }

        protected void RegisterUIObj(Element element, TUIObj uiObj) => _elementToUIObj[element] = uiObj;
        protected void UnregisterUIObj(Element element) => _elementToUIObj.Remove(element);


        protected TUIObj BuildInternal(Element element)
        {
            TUIObj uiObj = default;
            if (element != null)
            {
                if (BuildFuncTable.TryGetValue(element.GetType(), out var func))
                {
                    uiObj = func(element);
                    SetupUIObj(element, uiObj);
                }
                else
                {
                    Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                }
            }

            return uiObj;
        }

        protected virtual void SetupUIObj(Element element, TUIObj uiObj)
        {
            RegisterUIObj(element, uiObj);

            SetDefaultCallbacks(element, uiObj);

            if (element is LabelElement {labelType: LabelType.Prefix} label && label.IsLeftMost())
            {
                CalcPrefixLabelWidthWithIndent(label, uiObj);
            }
        }


        protected abstract void CalcPrefixLabelWidthWithIndent(LabelElement label, TUIObj uiObj);
        
        protected virtual void SetDefaultCallbacks(Element element, TUIObj uiObj)
        {
            element.enableRx.SubscribeAndCallOnce((enable) => OnElementEnableChanged(element, uiObj, enable));
            element.interactableRx.SubscribeAndCallOnce((interactable) =>
                OnElementInteractableChanged(element, uiObj, interactable));
            element.Style.SubscribeAndCallOnce((style) => OnElementStyleChanged(element, uiObj, style));
            element.onDestroy += OnDestroyElement;

            if (element is DynamicElement dynamicElement)
            {
                dynamicElement.onRebuildChildren += OnRebuildElementGroupChildren;
            }
        }

        protected abstract void OnElementEnableChanged(Element element, TUIObj uiObj, bool enable);
        protected abstract void OnElementInteractableChanged(Element element, TUIObj uiObj, bool interactable);
        protected abstract void OnElementStyleChanged(Element element, TUIObj uiObj, Style style);
        protected abstract void OnRebuildElementGroupChildren(ElementGroup elementGroup);
        protected abstract void OnDestroyElement(Element element, bool isDestroyRoot);


        protected IEnumerable<TUIObj> Build_ElementGroupContents(ElementGroup elementGroup)
        {
            return elementGroup.Contents.Select(BuildInternal).Where(ve => ve != null);
        }
    }
}