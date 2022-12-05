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
        private readonly Dictionary<Element, TUIObj> _elementToUIObj = new();
        private readonly Dictionary<TUIObj, Element> _uiObjToElement = new();
        
        public TUIObj GetUIObj(Element element)
        {
            _elementToUIObj.TryGetValue(element, out var uiObj);
            return uiObj;
        }

        public Element GetElement(TUIObj uiObj)
        {
            _uiObjToElement.TryGetValue(uiObj, out var element);
            return element;
        }

        protected void RegisterUIObj(Element element, TUIObj uiObj)
        {
            _elementToUIObj[element] = uiObj;
            _uiObjToElement[uiObj] = element;
        }

        protected void UnregisterUIObj(Element element)
        {
            _elementToUIObj.Remove(element);

            var uiObj = GetUIObj(element);
            if (uiObj != null)
            {
                _uiObjToElement.Remove(uiObj);
            }
        }

        protected TUIObj BuildInternal(Element element)
        {
            TUIObj uiObj = default;
            if (element != null)
            {
                if (BuildFuncTable.TryGetValue(element.GetType(), out var func))
                {
                    uiObj = func(element);
                    SetupUIObj(element, uiObj);
                    SetPersistantCallback(element);
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
        }

        protected virtual void TeardownUIObj(Element element)
        {
            UnregisterUIObj(element);
            element.GetViewBridge().UnsubscribeAll();
        }


        protected abstract void CalcPrefixLabelWidthWithIndent(LabelElement label, TUIObj uiObj);


        protected virtual void SetPersistantCallback(Element element)
        {
            element.onDestroyView += OnDestroyViewElement;

            if (element is DynamicElement dynamicElement)
            {
                dynamicElement.RegisterBuildUI(OnRebuildElementGroupChildren);
            } 
        }
        
        protected virtual void SetDefaultCallbacks(Element element, TUIObj uiObj)
        {
            var enableUnsubscribe = element.enableRx.SubscribeAndCallOnce((enable) => OnElementEnableChanged(element, uiObj, enable));
            var interactableUnsubscribe = element.interactableRx.SubscribeAndCallOnce((interactable) => OnElementInteractableChanged(element, uiObj, interactable));
            var styleUnsubscribe =  element.Style.SubscribeAndCallOnce((style) => OnElementStyleChanged(element, uiObj, style));

            var viewBridge = element.GetViewBridge();
            viewBridge.onUnsubscribe += () =>
            {
                enableUnsubscribe.Dispose();
                interactableUnsubscribe.Dispose();
                styleUnsubscribe.Dispose();
            };
            
                        
            if (element is LabelElement label && label.IsPrefix())
            {
                CalcPrefixLabelWidthWithIndent(label, uiObj);
            }
        }
        
        protected abstract void OnElementEnableChanged(Element element, TUIObj uiObj, bool enable);
        protected abstract void OnElementInteractableChanged(Element element, TUIObj uiObj, bool interactable);
        protected abstract void OnElementStyleChanged(Element element, TUIObj uiObj, Style style);
        protected abstract void OnRebuildElementGroupChildren(ElementGroup elementGroup);
        protected abstract void OnDestroyViewElement(Element element, bool isDestroyRoot);


        protected IEnumerable<TUIObj> Build_ElementGroupContents(ElementGroup elementGroup)
        {
            return elementGroup.Contents.Select(BuildInternal).Where(ve => ve != null);
        }
    }
}