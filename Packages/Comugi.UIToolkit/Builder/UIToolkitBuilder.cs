using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Comugi.UIToolkit
{
    public static class UIToolkitBuilder
    {
        static readonly Dictionary<Type, Func<Element, VisualElement>> buildFuncs = new Dictionary<Type, Func<Element, VisualElement>>()
        {
            [typeof(Comugi.Window)] = (e) => Build_ElementGroup(new Window(), e)
            /*
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
            */
        };

        public static VisualElement Build(Element element)
        {
            if (buildFuncs.TryGetValue(element.GetType(), out var func))
            {
                return func(element);
            }

            return null;
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

    }
}