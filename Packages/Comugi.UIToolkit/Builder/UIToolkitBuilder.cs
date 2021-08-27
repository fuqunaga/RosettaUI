using Comugi.Builder;
using Comugi.Reactive;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using UILabel = UnityEngine.UIElements.Label;

namespace Comugi.UIToolkit
{
    public class UIToolkitBuilder : BuildFramework<VisualElement>
    {
        readonly Dictionary<Type, Func<Element, VisualElement>> buildFuncs;
        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> buildFuncTable => buildFuncs;


        public UIToolkitBuilder()
        {
            buildFuncs = new Dictionary<Type, Func<Element, VisualElement>>()
            {
                [typeof(WindowElement)] = Build_Window,
                /*
                [typeof(Panel)] = (e) => Build_ElementGroup(e, resource.panel),
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                */
                [typeof(LabelElement)] = Build_Label,
                //[typeof(IntField)] = Build_IntField,
                /*
                [typeof(FloatField)] = Build_FloatField,
                [typeof(StringField)] = Build_StringField,
                [typeof(BoolField)] = Build_BoolField,
                [typeof(ButtonElement)] = Build_Button,
                [typeof(Dropdown)] = Build_Dropdown,
                [typeof(IntSlider)] = Build_IntSlider,
                [typeof(FloatSlider)] = Build_FloatSlider,
                [typeof(LogSlider)] = Build_LogSlider,
                */
                [typeof(FoldElement)] = Build_Fold,
                /*
                [typeof(DynamicElement)] = (e) => Build_ElementGroup(e, null, true, (go) => AddLayoutGroup<HorizontalLayoutGroup>(go))
                */

            };
        }


        protected override void Initialize(VisualElement ve, Element element)
        {
            element.enableRx.Subscribe((enable) => ve.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None);
            element.interactableRx.Subscribe((interactable) => ve.SetEnabled(interactable));
        }


        VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement)element;
            var window = new Window();
            window.closeButton.clicked += () => windowElement.enable = !windowElement.enable;

            return Build_ElementGroup(window, element);
        }

        VisualElement Build_ElementGroup(VisualElement container, Element element)
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

        VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement)element;
            var fold = new Foldout();

            var title = foldElement.title;
            fold.text = title.GetInitialValue();

            fold.Add(Build(foldElement.contents));

            foldElement.isOpenRx.Subscribe((isOpen) => fold.value = isOpen);

            return fold;
        }

#if false
        private static VisualElement Build_IntField(Element arg)
        {
            var field = new IntField();
        }



        static VisualElement Build_InputField<T>(FieldBase<T> field, GameObject prefab, TMP_InputField.ContentType contentType, Func<string, (bool, T)> tryParse, out TMP_InputField inputFieldUI)
        {
            var go = Instantiate(field, prefab);

            inputFieldUI = go.GetComponentInChildren<TMP_InputField>();
            inputFieldUI.contentType = contentType;
            inputFieldUI.text = field.GetInitialValue()?.ToString();
            inputFieldUI.pointSize = settings.fontSize;

            inputFieldUI.colors = settings.theme.fieldColors;
            inputFieldUI.selectionColor = settings.theme.selectionColor;
            inputFieldUI.textComponent.color = settings.theme.textColor;

            inputFieldUI.onValueChanged.AddListener((str) =>
            {
                var (success, v) = tryParse(str);
                if (success)
                {
                    field.OnViewValueChanged(v);
                }
            }
            );

            var capturedInputFieldUI = inputFieldUI; // capture UI for lambda

            field.RegisterSetValueToView((v) =>
            {
                var (success, viewValue) = tryParse(capturedInputFieldUI.text);
                var isDifferent = !success || !(v?.Equals(viewValue) ?? (viewValue == null));

                if (isDifferent)
                {
                    capturedInputFieldUI.text = v?.ToString() ?? "";
                }
            });

            RegisterSetInteractable(field, capturedInputFieldUI, capturedInputFieldUI.textComponent);


            return BuildField_AddLabelIfHas(go, field);
        }
#endif
    }
}