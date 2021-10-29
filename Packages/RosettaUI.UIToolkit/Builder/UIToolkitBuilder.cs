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
    public partial class UIToolkitBuilder : BuildFramework<VisualElement>
    {
        private readonly Dictionary<Type, Func<Element, VisualElement>> _buildFuncTable;


        public UIToolkitBuilder()
        {
            _buildFuncTable = new Dictionary<Type, Func<Element, VisualElement>>
            {
                [typeof(WindowElement)] = Build_Window,
                [typeof(WindowLauncherElement)] = Build_WindowLauncher,
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                [typeof(BoxElement)] = Build_Box,
                [typeof(ScrollViewElement)] = Build_ScrollView,
                [typeof(IndentElement)] = Build_Indent,
                
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


        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> BuildFuncTable => _buildFuncTable;


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

        private static class UssClassName
        {
            public static readonly string UnityBaseField = "unity-base-field";
            public static readonly string UnityBaseFieldLabel = UnityBaseField + "__label";
            public static readonly string UnityBaseFieldInput = UnityBaseField + "__input";

            private static readonly string RosettaUI = "rosettaui";
            
            public static readonly string CompositeField = RosettaUI + "-composite-field";
            public static readonly string CompositeFieldContents = CompositeField + "__contents";
            public static readonly string CompositeFieldFirstChild = CompositeField + "__first-child";

            public static readonly string Row = RosettaUI + "-row";
            public static readonly string RowContents = Row + "__contents";
            public static readonly string RowContentsFirst = RowContents + "--first";

            public static readonly string Indent = RosettaUI + "-indent";

            public static readonly string WindowLauncher = RosettaUI + "-window-launcher";

            public static readonly string DynamicElement = RosettaUI + "-dynamic-element";

            public static readonly string MinMaxSlider = RosettaUI + "-min-max-slider";
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