using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RosettaUI.Builder
{
    public abstract class BuildFramework<TUIObj>
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


        public TUIObj Build(Element element)
        {
            TUIObj uiObj = default;
            if (element != null)
            {
                if (BuildFuncTable.TryGetValue(element.GetType(), out var func))
                {
                    uiObj = func(element);
                    RegisterUIObj(element, uiObj);

                    CalcTreeViewIndent(element, uiObj);
                    SetDefaultCallbacks(element, uiObj);
                }
                else
                {
                    Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                }
            }

            return uiObj;
        }

        protected virtual void CalcTreeViewIndent(Element element, TUIObj uiObj)
        {
            var isTarget = element is not ElementGroup or FoldElement;
            if (isTarget && element.IsLeftMost())
            {
                SetTreeViewIndent(element, uiObj, element.GetIndentLevel());
            }
        }

        protected virtual void SetDefaultCallbacks(Element element, TUIObj uiObj)
        {
            element.enableRx.SubscribeAndCallOnce((enable) => OnElementEnableChanged(element, uiObj, enable));
            
            // foldElement always can be opened and closed
            if (element is FoldElement foldElement)
            {
                foldElement.interactableRx.SubscribeAndCallOnce(interactable =>
                {
                    foldElement.bar.Interactable &= interactable;
                    foreach (var c in foldElement.Contents)
                    {
                        c.Interactable &= interactable;
                    }
                });
            }
            else
            {
                element.interactableRx.SubscribeAndCallOnce((interactable) => OnElementInteractableChanged(element, uiObj, interactable));
            }

            element.Style.SubscribeAndCallOnce((style) => OnElementStyleChanged(element, uiObj, style));
            element.onDestroy += OnDestroyElement;

            if (element is ElementGroup elementGroup)
            {
                elementGroup.onRebuildChildren += OnRebuildElementGroupChildren;
            }
        }

        protected abstract void SetTreeViewIndent(Element element, TUIObj uiObj, int indentLevel);
        
        protected abstract void OnElementEnableChanged(Element element, TUIObj uiObj, bool enable);
        protected abstract void OnElementInteractableChanged(Element element, TUIObj uiObj, bool interactable);
        protected abstract void OnElementStyleChanged(Element element, TUIObj uiObj, Style style);
        protected abstract void OnRebuildElementGroupChildren(ElementGroup elementGroup);
        protected abstract void OnDestroyElement(Element element);


        protected IEnumerable<TUIObj> Build_ElementGroupContents(ElementGroup elementGroup)
        {
            return elementGroup.Contents.Select(Build).Where(ve => ve != null);
        }
    }
}