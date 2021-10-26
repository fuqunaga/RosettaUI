using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Reactive;
using RosettaUI.UIToolkit.PackageInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public class UIToolkitBuilder : BuildFramework<VisualElement>
    {
        private readonly Dictionary<Type, Func<Element, VisualElement>> _buildFuncs;


        public UIToolkitBuilder()
        {
            _buildFuncs = new Dictionary<Type, Func<Element, VisualElement>>
            {
                [typeof(WindowElement)] = Build_Window,
                [typeof(WindowLauncherElement)] = Build_WindowLauncher,
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                [typeof(BoxElement)] = Build_Box,
                [typeof(ScrollViewElement)] = Build_ScrollView,
                
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_IntField,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(StringFieldElement)] = Build_Field<string, TextField>,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                [typeof(ColorFieldElement)] = Build_ColorField,

                [typeof(DropdownElement)] = Build_Dropdown,
                [typeof(IntSliderElement)] = Build_Slider<int, ClampFreeSliderInt>,
                [typeof(FloatSliderElement)] = Build_Slider<float, ClampFreeSlider>,

                [typeof(IntMinMaxSliderElement)] = Build_MinMaxSlider_Int,
                [typeof(FloatMinMaxSliderElement)] = Build_MinMaxSlider_Float,
                /*
                [typeof(LogSlider)] = Build_LogSlider,
                */
                [typeof(ButtonElement)] = Build_Button,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(DynamicElement)] = Build_DynamicElement
            };
        }


        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> buildFuncTable => _buildFuncs;


        protected override void OnElementEnableChanged(Element _, VisualElement ve, bool enable)
        {
            ve.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void OnElementInteractableChanged(Element _, VisualElement ve, bool interactable)
        {
            ve.SetEnabled(interactable);
        }


        protected override void OnElementStyleChanged(Element element, VisualElement ve, Style style)
        {
            if (!style.HasValue) return;

            if (style.width is { } width) ve.style.width = width;
            if (style.height is { } height) ve.style.height = height;
            if (style.minWidth is { } minWidth) ve.style.minWidth = minWidth;
            if (style.minHeight is { } minnHeight) ve.style.minHeight = minnHeight;
            if (style.color is { } color) ve.style.color = color;
            if (style.justify is { } justify)
            {
                ve.style.justifyContent = justify == Style.Justify.Start ? Justify.FlexStart : Justify.FlexEnd;
            }
        }

        protected override void OnRebuildElementGroupChildren(ElementGroup elementGroup)
        {
            var groupVe = GetUIObj(elementGroup);
            Build_ElementGroupContents(groupVe, elementGroup);
        }

        protected override void OnDestroyElement(Element element)
        {
            var ve = GetUIObj(element);
            ve?.RemoveFromHierarchy();
            UnregisterUIObj(element);
        }

        private VisualElement Build_DynamicElement(Element element)
        {
            var ve = new VisualElement();
            ve.AddToClassList(UssClassName.DynamicElement);
            return Build_ElementGroupContents(ve, element);
        }

        private VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement) element;
            var window = new Window();
            window.TitleBarContainerLeft.Add(Build(windowElement.title));
            window.closeButton.clicked += () => windowElement.Enable = !windowElement.Enable;

            windowElement.isOpenRx.SubscribeAndCallOnce(isOpen =>
            {
                if (isOpen) window.Show();
                else window.Hide();
            });

            return Build_ElementGroupContents(window, element);
        }

        private VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement) element;
            var fold = new Foldout();

            var title = foldElement.title;
            fold.text = title.Value;

            foldElement.isOpenRx.SubscribeAndCallOnce(isOpen => fold.value = isOpen);

            return Build_ElementGroupContents(fold, foldElement);
        }

        private VisualElement Build_WindowLauncher(Element element)
        {
            var launcherElement = (WindowLauncherElement) element;
            var windowElement = launcherElement.Window;
            var window = (Window) Build(windowElement);

            //var toggle = Build_Field<bool, Toggle>(element, false);
            var toggle = CreateField<bool, Toggle>(launcherElement);
            toggle.AddToClassList(UssClassName.WindowLauncher);
            toggle.RegisterCallback<PointerUpEvent>(evt =>
            {
                // panel==null（初回）はクリックした場所に出る
                // 以降は以前の位置に出る
                // Toggleの値が変わるのはこのイベントの後
                if (!windowElement.Enable && window.panel == null) window.Show(evt.originalMousePosition, toggle);
            });

            launcherElement.label.valueRx.SubscribeAndCallOnce((v) => toggle.text = v);

            return toggle;
        }

        private VisualElement Build_Row(Element element)
        {
            var row = CreateRowVisualElement();

            return Build_ElementGroupContents(row, element, (ve, i) =>
            {
                //ve.AddToClassList(UssClassName.RowContents);
                if (i == 0) ve.AddToClassList(UssClassName.RowContentsFirst);
            });
        }

        private static VisualElement CreateRowVisualElement()
        {
            var row = new VisualElement();
            row.AddToClassList(UssClassName.Row);
            return row;
        }

        private VisualElement Build_Column(Element element)
        {
            var column = new VisualElement();
            //column.AddToClassList(FieldClassName.Column);

            return Build_ElementGroupContents(column, element);
        }

        private VisualElement Build_Box(Element element)
        {
            var box = new Box();
            return Build_ElementGroupContents(box, element);
        }

        VisualElement Build_ScrollView(Element element)
        {
            var scrollView = new ScrollView(); // TODO: support horizontal. ScrollViewMode.VerticalAndHorizontal may not work correctly 
            return Build_ElementGroupContents(scrollView, element);
        }

        private VisualElement Build_CompositeField(Element element)
        {
            var compositeFieldElement = (CompositeFieldElement) element;

            var field = new VisualElement();
            field.AddToClassList(UssClassName.UnityBaseField);
            field.AddToClassList(UssClassName.CompositeField);

            var labelElement = compositeFieldElement.title;
            if (labelElement != null)
            {
                var label = Build(labelElement);
                label.AddToClassList(UssClassName.UnityBaseFieldLabel);
                field.Add(label);
            }

            var contentContainer = new VisualElement();
            contentContainer.AddToClassList(UssClassName.CompositeFieldContents);
            field.Add(contentContainer);
            Build_ElementGroupContents(contentContainer, element, (ve, idx) =>
            {
                if (idx == 0)
                {
                    ve.AddToClassList(UssClassName.CompositeFieldFirstChild);
                }
            });

            return field;
        }

        private VisualElement Build_ElementGroupContents(VisualElement container, Element element,
            Action<VisualElement, int> setupContentsVe = null)
        {
            var i = 0;
            foreach (var ve in Build_ElementGroupContents((ElementGroup) element))
            {
                setupContentsVe?.Invoke(ve, i);
                container.Add(ve);
                i++;
            }

            return container;
        }


        private static VisualElement Build_Label(Element element)
        {
            var labelElement = (LabelElement) element;
            var label = new Label(labelElement.Value);
            SetupLabelCallback(label, labelElement);

            return label;
        }


        private static void SetupLabelCallback(Label label, LabelElement labelElement)
        {
            if (labelElement.IsLeftMost())
            {
                label.style.minWidth = Mathf.Max(0f,
                    LayoutSettings.LabelWidth - labelElement.GetIndent() * LayoutSettings.IndentSize);

                // Foldout直下のラベルはmarginRight、paddingRightがUnityDefaultCommon*.uss で書き換わるので上書きしておく
                // セレクタ例： .unity-foldout--depth-1 > .unity-base-field > .unity-base-field__label
                label.style.marginRight = LayoutSettings.LabelMarginRight;
                label.style.paddingRight = LayoutSettings.LabelPaddingRight;
            }

            if (!labelElement.IsConst) labelElement.valueRx.Subscribe(text => label.text = text);
        }


        private static VisualElement Build_IntField(Element element)
        {
            var intField = Build_Field<int, IntegerField>(element);
            intField.isUnsigned = ((IntFieldElement) element).isUnsigned;

            return intField;
        }


        private static VisualElement Build_ColorField(Element element)
        {
            var colorField = Build_Field<Color, ColorField>(element);

            colorField.showColorPickerFunc += (pos, target) =>
            {
                ColorPicker.Show(pos, target, colorField.value, color => colorField.value = color);
            };

            return colorField;
        }

        private static TField Build_Field<T, TField>(Element element)
            where TField : BaseField<T>, new()
        {
            return Build_Field<T, T, TField>(element,
                (field, v) => field.SetValueWithoutNotify(v),
                (v) => v);
        }

        private static TField Build_Field<TElementValue, TFieldValue, TField>(
            Element element,
            Action<TField, TElementValue> onElementValueChanged,
            Func<TFieldValue, TElementValue> fieldToElement
        )
            where TField : BaseField<TFieldValue>, new()
        {
            var fieldElement = (FieldBaseElement<TElementValue>) element;
            var field = CreateField<TElementValue, TFieldValue, TField>(fieldElement, onElementValueChanged,
                fieldToElement);

            var labelElement = fieldElement.label;
            if (labelElement != null)
            {
                field.label = labelElement.Value;
                SetupLabelCallback(field.labelElement, labelElement);
            }

            return field;
        }

        static TField CreateField<T, TField>(FieldBaseElement<T> fieldBaseElement)
            where TField : BaseField<T>, new()
        {
            return CreateField<T, T, TField>(fieldBaseElement, (field, v) => field.SetValueWithoutNotify(v), (v) => v);
        }

        private static TField CreateField<TElementValue, TFieldValue, TField>(
            FieldBaseElement<TElementValue> fieldBaseElement,
            //Func<TElementValue, TFieldValue> elementToFieldValue,
            Action<TField, TElementValue> onElementValueChanged,
            Func<TFieldValue, TElementValue> fieldToElement
        )
            where TField : BaseField<TFieldValue>, new()
        {
            var field = new TField();
            fieldBaseElement.valueRx.SubscribeAndCallOnce(v => onElementValueChanged(field, v));
            field.RegisterValueChangedCallback(ev => fieldBaseElement.OnViewValueChanged(fieldToElement(ev.newValue)));

            return field;
        }


        private static VisualElement Build_Slider<T, TSlider>(Element element)
            where T : IComparable<T>
            where TSlider : BaseSlider<T>, new()
        {
            var sliderElement = (SliderElement<T>) element;
            var slider = Build_Field<T, TSlider>(sliderElement);

            InitRangeFieldElement(sliderElement,
                (min) => slider.lowValue = min,
                (max) => slider.highValue = max
            );

            slider.showInputField = true;
            return slider;
        }

        static VisualElement Build_MinMaxSlider_Int(Element element) =>
            Build_MinMaxSlider<int, IntegerField>(element, (i) => i, (f) => (int) f);

        static VisualElement Build_MinMaxSlider_Float(Element element) =>
            Build_MinMaxSlider<float, FloatField>(element, (f) => f, (f) => f);

        private static VisualElement Build_MinMaxSlider<T, TTextField>(Element element, Func<T, float> toFloat,
            Func<float, T> toValue)
            where TTextField : TextInputBaseField<T>, new()
        {
            if (toValue == null) throw new ArgumentNullException(nameof(toValue));

            var minTextField = new TTextField();
            var maxTextField = new TTextField();

            var sliderElement = (MinMaxSliderElement<T>) element;
            var slider = Build_Field<MinMax<T>, Vector2, MinMaxSlider>(
                sliderElement,
                onElementValueChanged: (field, minMax) =>
                {
                    var vec2 = new Vector2(toFloat(minMax.min), toFloat(minMax.max));

                    field.SetValueWithoutNotify(vec2);
                    minTextField.SetValueWithoutNotify(minMax.min);
                    maxTextField.SetValueWithoutNotify(minMax.max);
                },
                (vec2) => MinMax.Create(toValue(vec2.x), toValue(vec2.y))
            );

            InitRangeFieldElement(sliderElement,
                (min) => slider.lowLimit = toFloat(min),
                (max) => slider.highLimit = toFloat(max)
            );

            minTextField.RegisterValueChangedCallback((evt) => slider.minValue = toFloat(evt.newValue));
            maxTextField.RegisterValueChangedCallback((evt) => slider.maxValue = toFloat(evt.newValue));


            var row = CreateRowVisualElement();

            row.AddToClassList(UssClassName.MinMaxSlider);
            minTextField.AddToClassList(UssClassName.MinMaxSliderTextField);
            maxTextField.AddToClassList(UssClassName.MinMaxSliderTextField);

            row.Add(slider);
            row.Add(minTextField);
            row.Add(maxTextField);

            return row;
        }


        static void InitRangeFieldElement<T, TRange>(
            RangeFieldElement<T, TRange> rangeFieldElement,
            Action<TRange> updateMin,
            Action<TRange> updateMax
        )
        {
            if (rangeFieldElement.IsMinConst)
                updateMin(rangeFieldElement.Min);
            else
                rangeFieldElement.minRx.SubscribeAndCallOnce(updateMin);

            if (rangeFieldElement.IsMaxConst)
                updateMax(rangeFieldElement.Max);
            else
                rangeFieldElement.maxRx.SubscribeAndCallOnce(updateMax);
        }

        private static Button Build_Button(Element element)
        {
            var buttonElement = (ButtonElement) element;

            var button = new Button(buttonElement.OnClick);

            buttonElement.valueRx.SubscribeAndCallOnce(str => button.text = str);

            return button;
        }

        private static VisualElement Build_Dropdown(Element element)
        {
            var dropdownElement = (DropdownElement) element;
            var options = dropdownElement.options.ToList();

            var field = new PopupField<string>(
                options,
                dropdownElement.Value
            )
            {
                label = dropdownElement.label.Value
            };

            SetupLabelCallback(field.labelElement, dropdownElement.label);

            dropdownElement.valueRx.SubscribeAndCallOnce(v => field.index = v);
            field.RegisterValueChangedCallback(ev => dropdownElement.OnViewValueChanged(field.index));

            return field;
        }

        private static class UssClassName
        {
            public static readonly string UnityBaseField = "unity-base-field";
            public static readonly string UnityBaseFieldLabel = UnityBaseField + "__label";
            public static readonly string UnityBaseFieldInput = UnityBaseField + "__input";

            public static readonly string CompositeField = "rosettaui-composite-field";
            public static readonly string CompositeFieldContents = CompositeField + "__contents";
            public static readonly string CompositeFieldFirstChild = CompositeField + "__first-child";

            public static readonly string Row = "rosettaui-row";
            public static readonly string RowContents = Row + "__contents";
            public static readonly string RowContentsFirst = RowContents + "--first";

            public static readonly string WindowLauncher = "rosettaui-window-launcher";

            public static readonly string DynamicElement = "rosettaui-dynamic-element";

            public static readonly string MinMaxSlider = "rosettaui-min-max-slider";
            public static readonly string MinMaxSliderTextField = MinMaxSlider + "__text-field";
        }


        private static class LayoutSettings
        {
            public static readonly float LabelWidth = 150f;
            public static readonly float IndentSize = 15f;

            public static readonly float LabelMarginRight = 2f;
            public static readonly float LabelPaddingRight = 2f;
        }
    }
}