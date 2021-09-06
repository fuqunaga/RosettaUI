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
            static readonly Dictionary<Type, Func<Element, GameObject>> buildFuncs = new Dictionary<Type, Func<Element, GameObject>>()
            {
                [typeof(WindowElement)] = (e) => Build_ElementGroup(e, resource.window),
                [typeof(Panel)] = (e) => Build_ElementGroup(e, resource.panel),
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                [typeof(LabelElement)] = (e) => Build_Label((LabelElement)e),
                [typeof(IntFieldElement)] = Build_IntField,
                [typeof(FloatFieldElement)] = Build_FloatField,
                [typeof(StringFieldElement)] = Build_StringField,
                [typeof(BoolFieldElement)] = Build_BoolField,
                [typeof(ButtonElement)] = Build_Button,
                [typeof(DropdownElement)] = Build_Dropdown,
                [typeof(IntSlider)] = Build_IntSlider,
                [typeof(FloatSlider)] = Build_FloatSlider,
                [typeof(LogSlider)] = Build_LogSlider,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(DynamicElement)] = (e) => Build_ElementGroup(e, null, true, (go) => AddLayoutGroup<HorizontalLayoutGroup>(go))
            };

            protected override IReadOnlyDictionary<Type, Func<Element, GameObject>> buildFuncTable => buildFuncs;

            int layer;

            public GameObject Build(Element element, int layer)
            {
                this.layer = layer;
                return Build(element);
            }

            protected override void Initialize(GameObject uiObj, Element element)
            {
                SetLayerRecursive(uiObj.transform, layer);

                element.enableRx.Subscribe((enable) => uiObj.SetActive(enable));
                element.layoutRx.Subscribe((layout) => SetLayout(element, layout));
                element.onRebuild += OnRebuild;
                element.onDestroy += OnDestroy;

                static void SetLayerRecursive(Transform t, int layer)
                {
                    t.gameObject.layer = layer;
                    foreach (Transform child in t)
                    {
                        SetLayerRecursive(child, layer);
                    }
                }
            }

            readonly Dictionary<Element, LayoutElement> elementToLayoutElement = new Dictionary<Element, LayoutElement>();


            void SetLayout(Element element, Layout layout)
            {
                if (layout.HasValue)
                {
                    if (!elementToLayoutElement.TryGetValue(element, out var layoutElement))
                    {
                        if (elementToUIObj.TryGetValue(element, out var go))
                        {
                            layoutElement = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
                        }
                    }

                    if (layoutElement)
                    {
                        if (layout.preferredWidth.HasValue)
                        {
                            layoutElement.preferredWidth = layout.preferredWidth.Value;
                            layoutElement.flexibleWidth = 0;
                        }

                        if (layout.preferredHeight.HasValue)
                        {
                            layoutElement.preferredHeight = layout.preferredHeight.Value;
                            layoutElement.flexibleHeight = 0;
                        }
                    }
                }
            }


            void OnRebuild(Element element)
            {
                if (elementToUIObj.TryGetValue(element, out var go))
                {
                    Object.Destroy(go);
                }

                var parentGo = elementToUIObj[element.parent];
                var newGo = impl.Build(element);
                newGo.transform.SetParent(parentGo.transform);
            }


            void OnDestroy(Element element)
            {
                if (elementToUIObj.TryGetValue(element, out var go))
                {
                    Object.Destroy(go);
                    elementToUIObj.Remove(element);
                }
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