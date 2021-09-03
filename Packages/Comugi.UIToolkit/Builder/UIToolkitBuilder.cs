using RosettaUI.Builder;
using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
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
                */
                [typeof(Row)] = Build_Row,
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                //[typeof(Column)] = Build_Column,

                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_IntField,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(StringFieldElement)] = Build_Field<string, TextField>,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                /*
                [typeof(ButtonElement)] = Build_Button,
                */
                [typeof(DropdownElement)] = Build_Dropdown,
                /*
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


        static class FieldClassName
        {
            public static readonly string BaseField = "unity-base-field";
            public static readonly string BaseFieldLabel = BaseField + "__label";
            public static readonly string BaseFieldInput = BaseField + "__input";

            public static readonly string Row = "rosettaui-row";
            public static readonly string RowContents = "rosettaui-row__contents";
            public static readonly string CompositeField = "unity-composite-field";
            
            
        }

        VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement)element;
            var window = new Window();
            window.closeButton.clicked += () => windowElement.enable = !windowElement.enable;

            return Build_ElementGroup(window, element);
        }

        VisualElement Build_Row(Element element)
        {
            var row = new VisualElement();

            row.style.flexDirection = FlexDirection.Row;
            row.AddToClassList(FieldClassName.Row);

            return Build_ElementGroup(row, element, (ve) => ve.AddToClassList(FieldClassName.RowContents));
        }

        VisualElement Build_CompositeField(Element element)
        {
            var compositeFieldElement = (CompositeFieldElement)element;

            var ve = new VisualElement();
            ve.AddToClassList(FieldClassName.BaseField);


            var label = Build(compositeFieldElement.label);
            label.AddToClassList(FieldClassName.BaseFieldLabel);
            ve.Add(label);

            var contents = Build(compositeFieldElement.contents);
            contents.AddToClassList(FieldClassName.BaseFieldInput);
            ve.Add(contents);

            return ve;
        }

        VisualElement Build_ElementGroup(VisualElement container, Element element, Action<VisualElement> setupContentsVe = null)
        {
            var elementGroup = (ElementGroup)element;

            foreach (var e in elementGroup.Elements)
            {
                var ve = Build(e);
                if (ve != null)
                {
                    setupContentsVe?.Invoke(ve);
                    container.Add(ve);
                }
            }

            return container;
        }


        static VisualElement Build_Label(Element element)
        {
            var labelElement = (LabelElement)element;
            var label = new Label(labelElement.GetInitialValue());
            SetupLabelCallback(label, labelElement);

            return label;
        }


        static void SetupLabelCallback(Label label, LabelElement labelElement)
        {
            if (!labelElement.IsConst)
            {
                labelElement.setValueToView += (text) => label.text = text;
            }
        }

        VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement)element;
            var fold = new Foldout();

            var title = foldElement.title;
            fold.text = title.GetInitialValue();

            foreach (var content in foldElement.Contents)
            {
                fold.Add(Build(content));
            }


            foldElement.isOpenRx.Subscribe((isOpen) => fold.value = isOpen);

            return fold;
        }


        static VisualElement Build_IntField(Element element)
        {
            var intField = Build_Field<int, IntegerField>(element);
            intField.isUnsigned = ((IntFieldElement)element).isUnsigned;

            return intField;
        }

        static TField Build_Field<T, TField>(Element element)
            where TField : BaseField<T>, new()
        {
            var fieldElement = (FieldBaseElement<T>)element;
            var field = new TField();
            field.label = fieldElement.label.GetInitialValue();
            SetupLabelCallback(field.labelElement, fieldElement.label);


            fieldElement.setValueToView += (v) => field.value = v;
            field.RegisterValueChangedCallback(ev => fieldElement.OnViewValueChanged(ev.newValue));

            return field;

        }

        static VisualElement Build_Dropdown(Element element)
        {
            var dropdownElement = (DropdownElement)element;
            var options = dropdownElement.options.ToList();

            var field = new PopupField<string>(
                options,
                dropdownElement.GetInitialValue()
                );

            field.label = dropdownElement.label.GetInitialValue();
            SetupLabelCallback(field.labelElement, dropdownElement.label);

            dropdownElement.setValueToView += (v) => field.index = v;
            field.RegisterValueChangedCallback(ev => dropdownElement.OnViewValueChanged(field.index));

            return field;
        }
    }
}