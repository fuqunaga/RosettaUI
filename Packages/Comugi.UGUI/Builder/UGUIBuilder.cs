using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Comugi.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        public static UGUIResource resource;
        public static UGUISettings settings;

        static readonly Dictionary<Type, Func<Element, GameObject>> buildFuncs = new Dictionary<Type, Func<Element, GameObject>>()
        {
            [typeof(Window)] = (e) => Build_ElementGroup(e, resource.window),
            [typeof(Panel)] = (e) => Build_ElementGroup(e, resource.panel),
            [typeof(Row)] = Build_Row,
            [typeof(Column)] = Build_Column,
            [typeof(Label)] = (e) => Build_Label((Label)e),
            [typeof(IntField)] = Build_IntField,
            [typeof(FloatField)] = Build_FloatField,
            [typeof(StringField)] = Build_StringField,
            [typeof(BoolField)] = Build_BoolField,
            [typeof(ButtonElement)] = Build_Button,
            [typeof(Dropdown)] = Build_Dropdown,
            [typeof(IntSlider)] = Build_IntSlider,
            [typeof(FloatSlider)] = Build_FloatSlider,
            [typeof(LogSlider)] = Build_LogSlider,
            [typeof(FoldElement)] = Build_Fold,
            [typeof(DynamicElement)] = (e) => Build_ElementGroup(e, null, true, (go) => AddLayoutGroup<HorizontalLayoutGroup>(go))
        };

        static readonly Dictionary<Element, GameObject> elementToGameObject = new Dictionary<Element, GameObject>();



        static UGUIBuilder()
        {
            ViewBridge.Init(SetActive, SetInteractive, SetFoldOpen, SetLayout, Rebuild, Destroy);
        }

        public static GameObject Build(Transform transform, Element element)
        {
            GameObject go = null;
            if (element != null)
            {
                if (buildFuncs.TryGetValue(element.GetType(), out var func))
                {
                    go = func(element);
                    go.transform.SetParent(transform, false);

                    SetLayerRecursive(go.transform, transform.gameObject.layer);
                    elementToGameObject[element] = go;
                }
                else
                {
                    Debug.LogError($"DebugUI.UGUI Unknown Type[{element.GetType()}].");
                }

                ViewBridge.InitElementAfterBuild(element);
            }

            return go;

            static void SetLayerRecursive(Transform t, int layer)
            {
                t.gameObject.layer = layer;
                foreach (Transform child in t)
                {
                    SetLayerRecursive(child, layer);
                }
            }

        }

        static void SetActive(Element element, bool active)
        {
            if (elementToGameObject.TryGetValue(element, out var go))
            {
                go.SetActive(active);
            }
        }


        static readonly Dictionary<Element, Action<bool>> elementToSetInteractive = new Dictionary<Element, Action<bool>>();

        static void SetInteractive(Element element, bool interactive)
        {
            if (elementToSetInteractive.TryGetValue(element, out var action))
            {
                action(interactive);
            }
        }

        static void RegisterSetInteractable(Element element, Action<bool> setInteractive)
        {
            elementToSetInteractive[element] = setInteractive;
        }

        static void RegisterSetInteractable(Element element, Selectable selectable, TMP_Text text)
        {
            RegisterSetInteractable(element, (interactable) =>
            {
                selectable.interactable = interactable;
                if (text)
                {
                    SetTextColorWithInteractable(text, selectable.colors.fadeDuration, interactable);
                }
            });
        }


        static readonly Dictionary<FoldElement, Action<bool>> elementToSetFoldOpen = new Dictionary<FoldElement, Action<bool>>();

        static void SetFoldOpen(FoldElement element, bool isOpen)
        {
            if (elementToSetFoldOpen.TryGetValue(element, out var action))
            {
                action(isOpen);
            }
        }
        static void RegisterSetFoldOpen(FoldElement element, Action<bool> setFoldOpen)
        {
            elementToSetFoldOpen[element] = setFoldOpen;
        }


        static readonly Dictionary<Element, LayoutElement> elementToLayoutElement = new Dictionary<Element, LayoutElement>();

        static void SetLayout(Element element, Layout layout)
        {
            if (layout.HasValue)
            {
                if (!elementToLayoutElement.TryGetValue(element, out var layoutElement))
                {
                    if (elementToGameObject.TryGetValue(element, out var go))
                    {
                        layoutElement = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
                    }
                }

                if ( layoutElement )
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


        static void Rebuild(Element element)
        {
            if (elementToGameObject.TryGetValue(element, out var go))
            {
                Object.Destroy(go);
            }

            var parentGo = elementToGameObject[element.parentGroup];

            Build(parentGo.transform, element);

            //go = Build(parentGo.transform, element);
            //go.transform.SetSiblingIndex(element.SiblingIndex);
        }

        static void Destroy(Element element)
        {
            if (elementToGameObject.TryGetValue(element, out var go))
            {
                Object.Destroy(go);
                elementToGameObject.Remove(element);
            }

            elementToSetInteractive.Remove(element);
        }


        #region Utility Function

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