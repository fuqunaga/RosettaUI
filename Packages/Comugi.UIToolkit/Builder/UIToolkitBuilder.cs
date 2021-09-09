using RosettaUI.Builder;
using RosettaUI.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public class UIToolkitBuilder : BuildFramework<VisualElement>
    {
        static class FieldClassName
        {
            public static readonly string BaseField = "unity-base-field";
            public static readonly string BaseFieldLabel = BaseField + "__label";
            public static readonly string BaseFieldInput = BaseField + "__input";

            public static readonly string Row = "rosettaui-row";
            public static readonly string RowContents = Row + "__contents";
            public static readonly string RowContentsFirst = RowContents + "--first";
        }


        static class Layout
        {
            public static readonly float LabelWidth = 150f;
            public static readonly float IndentSize = 15f;

            public static readonly float LabelMarginRight = 2f;
            public static readonly float LabelPaddingRight = 2f;
        }


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
                [typeof(Column)] = Build_Column,
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_IntField,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(StringFieldElement)] = Build_Field<string, TextField>,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                [typeof(DropdownElement)] = Build_Dropdown,
                //[typeof(IntSlider)] = Build_IntSlider,
                [typeof(FloatSliderElement)] = Build_FloatSlider,
                /*
                [typeof(LogSlider)] = Build_LogSlider,
                */
                [typeof(ButtonElement)] = Build_Button,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(DynamicElement)] = (e) => Build_ElementGroup(new VisualElement() { name = nameof(DynamicElement) }, e),
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

        VisualElement Build_Row(Element element)
        {
            var row = CreateRowVisualElement();

            return Build_ElementGroup(row, element, (ve, i) =>
            {
                ve.AddToClassList(FieldClassName.RowContents);
                if (i == 0)
                {
                    ve.AddToClassList(FieldClassName.RowContentsFirst);
                }
            });
        }

        static VisualElement CreateRowVisualElement()
        {
            var row = new VisualElement();
            row.AddToClassList(FieldClassName.Row);
            return row;
        }

        VisualElement Build_Column(Element element)
        {
            var column = new VisualElement();
            //column.AddToClassList(FieldClassName.Column);

            return Build_ElementGroup(column, element);
        }


        VisualElement Build_CompositeField(Element element)
        {
            var compositeFieldElement = (CompositeFieldElement)element;

            var ve = new VisualElement();
            ve.AddToClassList(FieldClassName.BaseField);

            var labelElement = compositeFieldElement.label;
            if (labelElement != null)
            {
                var label = Build(labelElement);
                label.AddToClassList(FieldClassName.BaseFieldLabel);
                ve.Add(label);
            }

            var contentsElement = compositeFieldElement.contents;
            if (contentsElement != null)
            {
                var contents = Build(contentsElement);
                contents.AddToClassList(FieldClassName.BaseFieldInput);
                ve.Add(contents);
            }

            return ve;
        }

        VisualElement Build_ElementGroup(VisualElement container, Element element, Action<VisualElement, int> setupContentsVe = null)
        {
            var elementGroup = (ElementGroup)element;
            var elements = elementGroup.Elements;

            for (var i = 0; i < elements.Count; ++i)
            {
                var e = elements[i];
                var ve = Build(e);
                if (ve != null)
                {
                    setupContentsVe?.Invoke(ve, i);
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
            if (labelElement.IsLeftMost())
            {
                label.style.minWidth = Mathf.Max(0f, Layout.LabelWidth - labelElement.GetIndent() * Layout.IndentSize);

                // Foldout直下のラベルはmarginRight、paddingRightがUnityDefaultCommon*.uss で書き換わるので上書きしておく
                // セレクタ例： .unity - foldout--depth - 1 > .unity - base - field > .unity - base - field__label
                label.style.marginRight = Layout.LabelMarginRight;
                label.style.paddingRight = Layout.LabelPaddingRight;
            }

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
            var labelElement = fieldElement.label;

            var field = new TField();

            if (labelElement != null)
            {
                field.label = labelElement.GetInitialValue();
                SetupLabelCallback(field.labelElement, labelElement);
            }

            fieldElement.setValueToView += (v) => field.value = v;
            field.RegisterValueChangedCallback(ev => fieldElement.OnViewValueChanged(ev.newValue));

            return field;
        }


        
        VisualElement Build_FloatSlider(Element element)
        {
            var sliderElement = (FloatSliderElement)element;

            var slider = Build_Field<float, Slider>(element);
            slider.AddToClassList(FieldClassName.RowContentsFirst);

            var field = new FloatField();
            field.AddToClassList(FieldClassName.RowContents);

            slider.RegisterValueChangedCallback((ev) => field.SetValueWithoutNotify(ev.newValue));
            field.RegisterValueChangedCallback((ev) => slider.value = ev.newValue);

            var row = CreateRowVisualElement();
            row.Add(slider);
            row.Add(field);

            return row;
        }



        static VisualElement Build_Button(Element element)
        {
            var buttonElement = (ButtonElement)element;

            var button = new Button(buttonElement.onClick)
            {
                text = buttonElement.GetInitialValue(),
            };

            buttonElement.setValueToView += (str) => button.text = str;

            return button;
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