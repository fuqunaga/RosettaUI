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


        static class LayoutSettings
        {
            public static readonly float LabelWidth = 150f;
            public static readonly float IndentSize = 15f;

            public static readonly float LabelMarginRight = 2f;
            public static readonly float LabelPaddingRight = 2f;
        }


        readonly Dictionary<Type, Func<Element, VisualElement>> _buildFuncs;
        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> buildFuncTable => _buildFuncs;


        public UIToolkitBuilder()
        {
            _buildFuncs = new Dictionary<Type, Func<Element, VisualElement>>()
            {
                [typeof(WindowElement)] = Build_Window,
                [typeof(WindowLauncherElement)] = Build_WindowLauncher,
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                [typeof(BoxElement)] = Build_Box,
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_IntField,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(StringFieldElement)] = Build_Field<string, TextField>,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                [typeof(ColorFieldElement)] = Build_ColorField,

                [typeof(DropdownElement)] = Build_Dropdown,
                [typeof(IntSliderElement)] = Build_Slider<int, SliderInt>,
                [typeof(FloatSliderElement)] = Build_Slider<float, Slider>,
                /*
                [typeof(LogSlider)] = Build_LogSlider,
                */
                [typeof(ButtonElement)] = Build_Button,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(DynamicElement)] = (e) =>
                    Build_ElementGroupChildren(new VisualElement() {name = nameof(DynamicElement)}, e),
            };
        }


        protected override void OnElementEnableChanged(Element _, VisualElement ve, bool enable)
        {
            ve.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void OnElementInteractableChanged(Element _, VisualElement ve, bool interactable)
        {
            ve.SetEnabled(interactable);
        }


        protected override void OnElementLayoutChanged(Element element, VisualElement ve, Layout layout)
        {
            if (!layout.HasValue) return;

            if (layout.minWidth is { } minWidth) ve.style.minWidth = minWidth;
            if (layout.minHeight is { } minnHeight) ve.style.minHeight = minnHeight;
            if (layout.justify is { } justify)
            {
                ve.style.justifyContent = justify == Layout.Justify.Start ? Justify.FlexStart : Justify.FlexEnd;
            }
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
            var windowElement = (WindowElement) element;
            var window = new Window();
            window.TitleBarContainerLeft.Add(Build(windowElement.title));
            window.closeButton.clicked += () => windowElement.enable = !windowElement.enable;

            windowElement.isOpenRx.SubscribeAndCallOnce((isOpen) =>
            {
                if (isOpen) window.Show();
                else window.Hide();
            });
            
            return Build_ElementGroupChildren(window, element);
        }
        
        VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement) element;
            var fold = new Foldout();

            var title = foldElement.title;
            fold.text = title.Value;
            
            foldElement.isOpenRx.SubscribeAndCallOnce((isOpen) => fold.value = isOpen);
            
            return Build_ElementGroupChildren(fold, foldElement);
        }

        VisualElement Build_WindowLauncher(Element element)
        {
            var launcherElement = (WindowLauncherElement) element;
            var windowElement = launcherElement.Window;
            var window = (Window)Build(windowElement);

            var button = Build_Button(launcherElement);
            button.clickable.clickedWithEventInfo += (evt) =>
            {
                windowElement.enable = !windowElement.enable;
                // panel==null（初回）はクリックした場所に出る
                // 移行は以前の位置に出る
                if (windowElement.enable && window.panel == null)
                {
                    window.Show(evt.originalMousePosition, button);
                }
            };

            return button;
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
            var compositeFieldElement = (CompositeFieldElement) element;

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

        VisualElement Build_ElementGroupChildren(VisualElement container, Element element,
            Action<VisualElement, int> setupContentsVe = null)
        {
            var i = 0;
            foreach (var ve in Build_ElementGroupChildren((ElementGroup) element))
            {
                setupContentsVe?.Invoke(ve, i);
                container.Add(ve);
                i++;
            }

            return container;
        }


        static VisualElement Build_Label(Element element)
        {
            var labelElement = (LabelElement) element;
            var label = new Label(labelElement.Value);
            SetupLabelCallback(label, labelElement);

            return label;
        }


        static void SetupLabelCallback(Label label, LabelElement labelElement)
        {
            if (labelElement.IsLeftMost())
            {
                label.style.minWidth = Mathf.Max(0f,
                    LayoutSettings.LabelWidth - labelElement.GetIndent() * LayoutSettings.IndentSize);

                // Foldout直下のラベルはmarginRight、paddingRightがUnityDefaultCommon*.uss で書き換わるので上書きしておく
                // セレクタ例： .unity - foldout--depth - 1 > .unity - base - field > .unity - base - field__label
                label.style.marginRight = LayoutSettings.LabelMarginRight;
                label.style.paddingRight = LayoutSettings.LabelPaddingRight;
            }

            if (!labelElement.IsConst)
            {
                labelElement.valueRx.Subscribe((text) => label.text = text);
            }
        }


        static VisualElement Build_IntField(Element element)
        {
            var intField = Build_Field<int, IntegerField>(element);
            intField.isUnsigned = ((IntFieldElement) element).isUnsigned;

            return intField;
        }


        static VisualElement Build_ColorField(Element element)
        {
            var colorField = Build_Field<Color, ColorField>(element);

            colorField.showColorPickerFunc += (pos, target) =>
            {
                ColorPicker.Show(pos, target, colorField.value, (color) => colorField.value = color);
            };

            return colorField;
        }

        static TField Build_Field<T, TField>(Element element)
            where TField : BaseField<T>, new()
        {
            var fieldElement = (FieldBaseElement<T>) element;
            var labelElement = fieldElement.label;

            var field = new TField();

            if (labelElement != null)
            {
                field.label = labelElement.Value;
                SetupLabelCallback(field.labelElement, labelElement);
            }

            fieldElement.valueRx.SubscribeAndCallOnce((v) => field.value = v);
            field.RegisterValueChangedCallback(ev => fieldElement.OnViewValueChanged(ev.newValue));

            return field;
        }

        static VisualElement Build_Slider<T, TSlider>(Element element)
            where T : IComparable<T>
            where TSlider : BaseSlider<T>, new()
        {
            var sliderElement = (Slider<T>) element;

            var slider = Build_Field<T, TSlider>(element);
            slider.AddToClassList(FieldClassName.RowContentsFirst);

            var (min, max) = sliderElement.minMax;
            slider.lowValue = min;
            slider.highValue = max;

            if (!sliderElement.IsMinMaxConst)
            {
                sliderElement.minMaxRx.Subscribe((pair) =>
                {
                    var (min, max) = pair;
                    slider.lowValue = min;
                    slider.highValue = max;
                });
            }

            slider.showInputField = true;
            return slider;
        }


        static Button Build_Button(Element element)
        {
            var buttonElement = (ButtonElement) element;

            var button = new Button(buttonElement.OnClick);

            buttonElement.valueRx.SubscribeAndCallOnce((str) => button.text = str);

            return button;
        }

        static VisualElement Build_Dropdown(Element element)
        {
            var dropdownElement = (DropdownElement) element;
            var options = dropdownElement.options.ToList();

            var field = new PopupField<string>(
                options,
                dropdownElement.Value
            );

            field.label = dropdownElement.label.Value;
            SetupLabelCallback(field.labelElement, dropdownElement.label);

            dropdownElement.valueRx.SubscribeAndCallOnce((v) => field.index = v);
            field.RegisterValueChangedCallback(ev => dropdownElement.OnViewValueChanged(field.index));

            return field;
        }
    }
}