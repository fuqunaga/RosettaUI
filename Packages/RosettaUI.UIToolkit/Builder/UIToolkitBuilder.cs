using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using FloatField = RosettaUI.UIToolkit.UnityInternalAccess.FloatField;

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
                [typeof(RowElement)] = Build_Row,
                [typeof(ColumnElement)] = Build_Column,
                [typeof(BoxElement)] = Build_Box,
                [typeof(ScrollViewElement)] = Build_ScrollView,
                [typeof(IndentElement)] = Build_Indent,
                [typeof(IndentElement)] = Build_Indent,
                
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_Field<int, IntegerField>,
                [typeof(UIntFieldElement)] = Build_Field<uint, UIntField>,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(TextFieldElement)] = Build_TextField,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                [typeof(ColorFieldElement)] = Build_ColorField,

                [typeof(DropdownElement)] = Build_Dropdown,
                [typeof(IntSliderElement)] = Build_Slider<int, ClampFreeSliderInt>,
                [typeof(FloatSliderElement)] = Build_Slider<float, ClampFreeSlider>,

                [typeof(IntMinMaxSliderElement)] = Build_MinMaxSlider_Int,
                [typeof(FloatMinMaxSliderElement)] = Build_MinMaxSlider_Float,
                [typeof(SpaceElement)] = Build_Space,
                [typeof(ImageElement)] = Build_Image,
                [typeof(ButtonElement)] = Build_Button,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(DynamicElement)] = Build_DynamicElement,
                [typeof(PopupMenuElement)] = Build_PopupElement
            };
        }


        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> BuildFuncTable => _buildFuncTable;

        protected override void SetTreeViewIndent(Element element, VisualElement uiObj, int indentLevel)
        {
            uiObj.schedule.Execute(() =>
            {
                // Foldout自体でインデントしてしまうと子要素すべてがインデントしてしまうので、FoldoutのインデントはToggle部分で行う
                // インデントは親でまとめて行わず各要素ごとに行う設計
                if (uiObj is Foldout fold)
                {
                    uiObj = fold.Q<Toggle>();
                }

                var indentSize = indentLevel * LayoutSettings.IndentSize;
                var marginLeft = uiObj.resolvedStyle.marginLeft;
                uiObj.style.marginLeft = marginLeft + indentSize;


                var (label, uiLeftWidth) = element switch
                {
                    RowElement or WindowLauncherElement => (null,0),
                    FoldElement f => (f.bar.FirstFieldLabel(), LayoutSettings.IndentSize),
                    _ => (element.FirstFieldLabel(),0)
                };
                
                if ( label != null)
                {
                    var labelObj = GetUIObj(label);
                    Assert.IsNotNull(labelObj, $"UIObj is not found. {element.GetType()} > Label[{label.Value}]");
                    labelObj.style.minWidth = Mathf.Max(0f, LayoutSettings.LabelWidth - indentSize - uiLeftWidth);

                    // Foldout直下のラベルはmarginRight、paddingRightがUnityDefaultCommon*.uss で書き換わるので上書きしておく
                    // セレクタ例： .unity-foldout--depth-1 > .unity-base-field > .unity-base-field__label
                    labelObj.style.marginRight = LayoutSettings.LabelMarginRight;
                    labelObj.style.paddingRight = LayoutSettings.LabelPaddingRight;
                }
            });
        }


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
            var isFixedSize = false;

            if (style.Width is { } width)
            {
                ve.style.width = width;
                isFixedSize = true;
            }

            if (style.Height is { } height)
            {
                ve.style.height = height;
                isFixedSize = true;
            }
            if (style.MinWidth is { } minWidth) ve.style.minWidth = minWidth;
            if (style.MinHeight is { } minHeight) ve.style.minHeight = minHeight;
            if (style.MaxWidth is { } maxWidth) ve.style.maxWidth = maxWidth;
            if (style.MaxHeight is { } maxHeight) ve.style.maxHeight = maxHeight;
            if (style.Color is { } color) ve.style.color = color;
   
            if (isFixedSize)
            {
                ve.style.flexGrow = 0;
                ve.style.flexShrink = 1;
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
            
            public static readonly string Space = RosettaUI + "-space";
        }
    }
}