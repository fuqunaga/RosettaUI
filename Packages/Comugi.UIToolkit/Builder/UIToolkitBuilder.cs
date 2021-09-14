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
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                [typeof(BoxElement)] = Build_Box,
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_IntField,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(StringFieldElement)] = Build_Field<string, TextField>,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                [typeof(DropdownElement)] = Build_Dropdown,
                [typeof(IntSliderElement)] = Build_Slider<int, SliderInt, IntegerField>,
                [typeof(FloatSliderElement)] = Build_Slider<float, Slider, FloatField>,
                /*
                [typeof(LogSlider)] = Build_LogSlider,
                */
                [typeof(ButtonElement)] = Build_Button,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(DynamicElement)] = (e) => Build_ElementGroupChildren(new VisualElement() { name = nameof(DynamicElement) }, e),
            };
        }


        protected override void OnElemnetEnableChanged(Element _, VisualElement ve, bool enable)
        {
            ve.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void OnElemnetInteractableChanged(Element _, VisualElement ve, bool interactable)
        {
            ve.SetEnabled(interactable);
        }

        protected override void OnRebuildElementGroupChildren(ElementGroup elementGroup)
        {
            var groupVe = GetUIObj(elementGroup);
            Build_ElementGroupChildren(groupVe, elementGroup);
   
        }

        protected override void OnDestroyElement(Element element)
        {
            var ve = GetUIObj(element);
            ve?.RemoveFromHierarchy();
            UnregisterUIObj(element);
        }



        VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement)element;
            var window = new Window();
            window.closeButton.clicked += () => windowElement.enable = !windowElement.enable;

            return Build_ElementGroupChildren(window, element);
        }

        VisualElement Build_Row(Element element)
        {
            var row = CreateRowVisualElement();

            return Build_ElementGroupChildren(row, element, (ve, i) =>
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

            return Build_ElementGroupChildren(column, element);
        }

        VisualElement Build_Box(Element element)
        {
            var box = new Box();
            return Build_ElementGroupChildren(box, element);
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

        VisualElement Build_ElementGroupChildren(VisualElement container, Element element, Action<VisualElement, int> setupContentsVe = null)
        {
            var i = 0;
            foreach(var ve in Build_ElementGroupChildren((ElementGroup)element))
            {
                setupContentsVe?.Invoke(ve, i);
                container.Add(ve);
                i++;
            }
            return container;
        }


        static VisualElement Build_Label(Element element)
        {
            var labelElement = (LabelElement)element;
            var label = new Label(labelElement.value);
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
                labelElement.valueRx.Subscribe((text) => label.text = text);
            }
        }

        VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement)element;
            var fold = new Foldout();

            var title = foldElement.title;
            fold.text = title.value;

            foreach (var content in foldElement.Contents)
            {
                fold.Add(Build(content));
            }

            foldElement.isOpenRx.SubscribeAndCallOnce((isOpen) => fold.value = isOpen);

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
                field.label = labelElement.value;
                SetupLabelCallback(field.labelElement, labelElement);
            }

            fieldElement.valueRx.SubscribeAndCallOnce((v) => field.value = v);
            field.RegisterValueChangedCallback(ev => fieldElement.OnViewValueChanged(ev.newValue));

            return field;
        }

        static VisualElement Build_Slider<T, TSlider, TField>(Element element)
            where T : IComparable<T>
            where TSlider : BaseSlider<T>, new()
            where TField : BaseField<T>, new()
        {
            var sliderElement = (Slider<T>)element;

            var slider = Build_Field<T, TSlider>(element);
            slider.AddToClassList(FieldClassName.RowContentsFirst);

            var (min, max) = sliderElement.minMax;
            slider.lowValue = min;
            slider.highValue = max;

            if ( !sliderElement.IsMinMaxConst)
            {
                sliderElement.minMaxRx.Subscribe((pair) =>
                {
                    var (min,max) = pair;
                    slider.lowValue = min;
                    slider.highValue = max;
                });
            }

            var field = new TField();
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

            var button = new Button(buttonElement.onClick);

            buttonElement.valueRx.SubscribeAndCallOnce((str) => button.text = str);

            return button;
        }

        static VisualElement Build_Dropdown(Element element)
        {
            var dropdownElement = (DropdownElement)element;
            var options = dropdownElement.options.ToList();

            var field = new PopupField<string>(
                options,
                dropdownElement.value
                );

            field.label = dropdownElement.label.value;
            SetupLabelCallback(field.labelElement, dropdownElement.label);

            dropdownElement.valueRx.SubscribeAndCallOnce((v) => field.index = v);
            field.RegisterValueChangedCallback(ev => dropdownElement.OnViewValueChanged(field.index));

            return field;
        }
    }
}