using RosettaUI.Builder;
using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        public static UGUIResource resource;
        public static UGUISettings settings;


        class UGUIBuilderImpl : BuildFramework<GameObject>
        {
            readonly Dictionary<Type, Func<Element, GameObject>> buildFuncs;

            public UGUIBuilderImpl()
            {
                buildFuncs = new Dictionary<Type, Func<Element, GameObject>>()
                {
                    [typeof(WindowElement)] = (e) => Build_ElementGroup(e, resource.window),
                    [typeof(BoxElement)] = (e) => Build_ElementGroup(e, resource.panel),
                    [typeof(Row)] = Build_Row,
                    [typeof(Column)] = Build_Column,
                    [typeof(LabelElement)] = (e) => Build_Label((LabelElement)e),
                    [typeof(IntFieldElement)] = Build_IntField,
                    [typeof(FloatFieldElement)] = Build_FloatField,
                    [typeof(StringFieldElement)] = Build_StringField,
                    [typeof(BoolFieldElement)] = Build_BoolField,
                    [typeof(ButtonElement)] = Build_Button,
                    [typeof(DropdownElement)] = Build_Dropdown,
                    [typeof(IntSliderElement)] = Build_IntSlider,
                    [typeof(FloatSliderElement)] = Build_FloatSlider,
                    [typeof(LogSlider)] = Build_LogSlider,
                    [typeof(FoldElement)] = Build_Fold,
                    [typeof(DynamicElement)] = (e) => Build_ElementGroup(e, null, true, (go) => AddLayoutGroup<HorizontalLayoutGroup>(go))
                };
            }

            protected override IReadOnlyDictionary<Type, Func<Element, GameObject>> buildFuncTable => buildFuncs;

            int layer;

            public GameObject Build(Element element, int layer)
            {
                this.layer = layer;
                return Build(element);
            }

            protected override void Initialize(GameObject uiObj, Element element)
            {
                base.Initialize(uiObj, element);

                SetLayerRecursive(uiObj.transform, layer);

                static void SetLayerRecursive(Transform t, int layer)
                {
                    t.gameObject.layer = layer;
                    foreach (Transform child in t)
                    {
                        SetLayerRecursive(child, layer);
                    }
                }
            }



            protected override void OnElementEnableChanged(Element _, GameObject uiObj, bool enable)
            {
                uiObj.SetActive(enable);
            }

            protected override void OnElementInteractableChanged(Element element, GameObject uiObj, bool interactable)
            {
                /*
                if (element is ElementGroup elementGroup)
                {
                    element.interactable = element.interactable && interactable;
                }
                */
                throw new NotImplementedException();
            }


            readonly Dictionary<Element, LayoutElement> elementToLayoutElement = new Dictionary<Element, LayoutElement>();

            protected override void OnElementLayoutChanged(Element element, GameObject go, Layout layout)
            {
                if (!layout.HasValue) return;
                {
                    if (!elementToLayoutElement.TryGetValue(element, out var layoutElement))
                    {
                        if (go != null)
                        {
                            layoutElement = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
                        }
                    }

                    if (layoutElement)
                    {
                        if (layout.minWidth is { } minWidth)
                        {
                            layoutElement.minWidth = minWidth;
                            layoutElement.flexibleWidth = 0;
                        }

                        if (layout.minHeight is { } minHeight)
                        {
                            layoutElement.minHeight = minHeight;
                            layoutElement.flexibleHeight = 0;
                        }
                    }
                }
            }

            protected override void OnRebuildElementGroupChildren(ElementGroup elementGroup)
            {
                var parentGo = GetUIObj(elementGroup);
                var trans = parentGo.transform;

                foreach(var e in elementGroup.Elements)
                {
                    var go = impl.Build(e);
                    go.transform.SetParent(trans);
                }

            }


            protected override void OnDestroyElement(Element element)
            {
                var go = GetUIObj(element);

                Object.Destroy(go);

                UnregisterUIObj(element);

            }


            public GameObject Build_ElementGroup(Element element, GameObject prefab, bool useDisplayName = false, Action<GameObject> addComponentFunc = null)
            {
                var elementGroup = (ElementGroup)element;

                var name = useDisplayName ? elementGroup.displayName : element.GetType().Name;

                var go = Instantiate(name, prefab);
                addComponentFunc?.Invoke(go);

                var trans = go.transform;

                foreach (var childGo in Build_ElementGroupChildren(elementGroup))
                {
                    childGo.transform.SetParent(trans);
                }

                return go;
            }
        }


        static UGUIBuilderImpl impl;

        public static GameObject Build(Element element, Transform transform)
        {
            if (impl == null) impl = new UGUIBuilderImpl();

            var go = impl.Build(element, transform.gameObject.layer);
            go.transform.SetParent(transform, false);

            return go;
        }


        #region Utility Function

        static void SubscribeInteractable(Element element, Selectable selectable, TMP_Text text)
        {
            element.interactableRx.Subscribe((interactable) =>
            {
                selectable.interactable = interactable;
                if (text)
                {
                    SetTextColorWithInteractable(text, selectable.colors.fadeDuration, interactable);
                }
            });
        }

        static GameObject Instantiate(Element element, GameObject prefab = null)
        {
            return Instantiate(element.GetType().Name, prefab);
        }

        static GameObject Instantiate(string name, GameObject prefab = null)
        {
            var go = prefab != null ? Object.Instantiate(prefab) : new GameObject();
            go.name = name;

            return go;
        }


        static void SetTextColorWithInteractable(Graphic graphic, float duration, bool interactable)
        {
            if (graphic != null)
            {
                var theme = settings.theme;
                var alpha = interactable ? theme.textColor.a : theme.textAlphaOnDisable;

                graphic.CrossFadeAlpha(alpha, duration, true);
            }
        }

        #endregion
    }
}