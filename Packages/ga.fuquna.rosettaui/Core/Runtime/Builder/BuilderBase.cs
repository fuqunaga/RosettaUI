using System.Collections.Generic;
using System.Linq;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI.Builder
{
    public abstract class BuilderBase<TUIObj>
    {
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

        protected TUIObj UnregisterUIObj(Element element)
        {
            var uiObj = GetUIObj(element);
            if (uiObj != null)
            {
                _uiObjToElement.Remove(uiObj);
            }
            
            _elementToUIObj.Remove(element);

            return uiObj;
        }

        protected TUIObj BuildInternal(Element element)
        {
            if (element == null) return default;

            TUIObj uiObj = DispatchBuild(element);
            if (uiObj != null)
            {
                SetupUIObj(element, uiObj);
            }
            else
            {
                Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
            }

            return uiObj;
        }

        protected abstract TUIObj DispatchBuild(Element element);

        protected virtual void SetupUIObj(Element element, TUIObj uiObj)
        {
            RegisterUIObj(element, uiObj);
            SetDefaultCallbacks(element, uiObj);
        }

        protected virtual TUIObj TeardownUIObj(Element element)
        {
            element.GetViewBridge().UnsubscribeAll();
            return UnregisterUIObj(element);
        }


        protected abstract void CalcPrefixLabelWidthWithIndent(LabelElement label, TUIObj uiObj);

       
        protected virtual void SetDefaultCallbacks(Element element, TUIObj uiObj)
        {
            var enableUnsubscribe = element.enableRx.SubscribeAndCallOnce((enable) => OnElementEnableChanged(element, uiObj, enable));
            var interactableUnsubscribe = element.interactableRx.SubscribeAndCallOnce((interactable) => OnElementInteractableChanged(element, uiObj, interactable));
            var styleUnsubscribe =  element.Style.SubscribeAndCallOnce((style) => OnElementStyleChanged(element, uiObj, style));
            
            element.onDetachView += OnDetachView;

            var viewBridge = element.GetViewBridge();
            viewBridge.onUnsubscribe += () =>
            {
                enableUnsubscribe.Dispose();
                interactableUnsubscribe.Dispose();
                styleUnsubscribe.Dispose();
                element.onDetachView -= OnDetachView; 
            };
            
                        
            if (element is LabelElement label && label.IsPrefix())
            {
                CalcPrefixLabelWidthWithIndent(label, uiObj);
            }
        }
        
        protected abstract void OnElementEnableChanged(Element element, TUIObj uiObj, bool enable);
        protected abstract void OnElementInteractableChanged(Element element, TUIObj uiObj, bool interactable);
        protected abstract void OnElementStyleChanged(Element element, TUIObj uiObj, Style style);
        protected abstract void OnDetachView(Element element, bool destroyView);

        protected IEnumerable<TUIObj> Build_ElementGroupContents(ElementGroup elementGroup)
        {
            return elementGroup.Contents.Select(BuildInternal).Where(ve => ve != null);
        }
    }
}