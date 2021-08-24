using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using UILabel = UnityEngine.UIElements.Label;

namespace Comugi.UIToolkit
{
    public static class UIToolkitBuilder
    {
        static readonly Dictionary<Type, Func<Element, VisualElement>> buildFuncs = new Dictionary<Type, Func<Element, VisualElement>>()
        {
            [typeof(WindowElement)] = Build_Window,
            /*
            [typeof(Panel)] = (e) => Build_ElementGroup(e, resource.panel),
            [typeof(Row)] = Build_Row,
            [typeof(Column)] = Build_Column,
            */
            [typeof(LabelElement)] = Build_Label,
            /*
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
            */
        };

        static UIToolkitBuilder()
        {
            ViewBridge.Init(SetActive, null, null, null, null, null /*SetInteractive, SetFoldOpen, SetLayout, Rebuild, Destroy*/);
        }


        static readonly Dictionary<Element, VisualElement> elementToVisualElement = new Dictionary<Element, VisualElement>();
        static VisualElement ElementToVisualElement(Element element)
        {
            elementToVisualElement.TryGetValue(element, out var ve);
            return ve;
        }

        private static void SetActive(Element element, bool active)
        {
            var ve = ElementToVisualElement(element);
            ve.visible = active;
        }

        public static VisualElement Build(Element element)
        {
            VisualElement ret = null;

            if (buildFuncs.TryGetValue(element.GetType(), out var func))
            {
                var ve = func(element);
                elementToVisualElement[element] = ve;

                ret = ve;
            }

            return ret;
        }



        static VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement)element;
            var window = new Window();
            window.closeButton.clicked += () => windowElement.enable = !windowElement.enable;

            return Build_ElementGroup(window, element);
        }

        private static VisualElement Build_ElementGroup(VisualElement container, Element element)
        {
            var elementGroup = (ElementGroup)element;

            foreach(var e in elementGroup.Elements)
            {
                container.Add(Build(e));
            }

            return container;
        }


        static VisualElement Build_Label(Element element)
        {
            var label = (LabelElement)element;
            return new UILabel(label.GetInitialValue());
        }
    }
}