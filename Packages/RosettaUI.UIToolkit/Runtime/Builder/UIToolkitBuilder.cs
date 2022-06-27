using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder : BuilderBase<VisualElement>
    {
        #region Static

        public static readonly UIToolkitBuilder Instance = new();

        public static VisualElement Build(Element element)
        {
            return Instance.BuildInternal(element);
        }
        
        #endregion


        private readonly Dictionary<Type, Func<Element, VisualElement>> _buildFuncTable;

        protected UIToolkitBuilder()
        {
            _buildFuncTable = new Dictionary<Type, Func<Element, VisualElement>>
            {
                [typeof(CompositeFieldElement)] = Build_CompositeField,
                [typeof(DynamicElement)] = Build_DynamicElement,
                [typeof(FoldElement)] = Build_Fold,
                [typeof(HelpBoxElement)] = Build_HelpBox,
                [typeof(IndentElement)] = Build_Indent,
                [typeof(RowElement)] = Build_Row,
                [typeof(ColumnElement)] = Build_Column,
                [typeof(PageElement)] = Build_Column,
                [typeof(BoxElement)] = Build_Box,
                [typeof(ScrollViewElement)] = Build_ScrollView,
                [typeof(TabsElement)] = Build_Tabs,
                [typeof(WindowElement)] = Build_Window,
                [typeof(WindowLauncherElement)] = Build_WindowLauncher,

                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_Field<int, IntegerField>,
                [typeof(UIntFieldElement)] = Build_Field<uint, UIntField>,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(TextFieldElement)] = Build_TextField,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                [typeof(ColorFieldElement)] = Build_ColorField,
                
                [typeof(IntSliderElement)] = Build_Slider<int, ClampFreeSliderInt>,
                [typeof(FloatSliderElement)] = Build_Slider<float, ClampFreeSlider>,
                [typeof(IntMinMaxSliderElement)] = Build_MinMaxSlider_Int,
                [typeof(FloatMinMaxSliderElement)] = Build_MinMaxSlider_Float,
                
                [typeof(DropdownElement)] = Build_Dropdown,
                [typeof(SpaceElement)] = Build_Space,
                [typeof(ImageElement)] = Build_Image,
                [typeof(ButtonElement)] = Build_Button,
                [typeof(PopupMenuElement)] = Build_PopupElement,
                [typeof(ListViewItemContainerElement)] = Build_ListViewItemContainer
            };
        }

        public void RegisterBuildFunc(Type type, Func<Element, VisualElement> func) => _buildFuncTable[type] = func;
        public void UnregisterBuildFunc(Type type) => _buildFuncTable.Remove(type);
        

        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> BuildFuncTable => _buildFuncTable;


        protected override void CalcPrefixLabelWidthWithIndent(LabelElement label, VisualElement ve)
        {
            // 表示前にラベルの幅を計算する
            // ve.schedule.Execute()は表示後に呼ばれるらしく、サイズが変更されるのが見えてしまう
            ve.ScheduleToUseResolvedLayoutBeforeRendering(() =>
            {
                var marginLeft = ve.worldBound.xMin;
                UpdateLabelWidth(marginLeft);

                // 初回生成時にve.worldBound.xMinがおかしい値の事があるのであとでもう一度チェックする
                // 特にEditorWindowでWindow内におさまっていないエレメントが怪しい
                ve.schedule.Execute(() =>
                {
                    var marginLeftAfter = ve.worldBound.xMin;
                    if (Math.Abs(marginLeft - marginLeftAfter) > 1f)
                    {
                        UpdateLabelWidth(marginLeftAfter);
                    }
                });
            });

            void UpdateLabelWidth(float marginLeft)
            {
                for (var element = label.Parent;
                     element != null;
                     element = element.Parent)
                {
                    if (LayoutHint.IsIndentOrigin(element))
                    {
                        marginLeft -= GetUIObj(element).worldBound.xMin;
                        break;
                    }
                }


                marginLeft /= ve.worldTransform.lossyScale.x; // ignore rotation

                ve.style.minWidth = LayoutSettings.LabelWidth - marginLeft;
            }
        }

        protected override void OnElementEnableChanged(Element _, VisualElement ve, bool enable)
        {
            var display = enable ? DisplayStyle.Flex : DisplayStyle.None;
            if (ve.resolvedStyle.display != display)
            {
                ve.style.display = display;
            }
        }

        protected override void OnElementInteractableChanged(Element _, VisualElement ve, bool interactable)
        {
            ve.SetEnabled(interactable);
        }


        protected override void OnElementStyleChanged(Element element, VisualElement ve, Style style)
        {
            var veStyle = ve.style;

            veStyle.width = ToStyleLength(style.Width);
            veStyle.height = ToStyleLength(style.Height);
            veStyle.minWidth = ToStyleLength(style.MinWidth);
            veStyle.minHeight = ToStyleLength(style.MinHeight);
            veStyle.maxWidth = ToStyleLength(style.MaxWidth);
            veStyle.maxHeight = ToStyleLength(style.MaxHeight);
            veStyle.color = ToStyleColor(style.Color);
            veStyle.backgroundColor = ToStyleColor(style.BackgroundColor);
   
            // width or height has value
            var isFixedSize = (veStyle.width.keyword == StyleKeyword.Undefined ||
                               veStyle.height.keyword == StyleKeyword.Undefined);
            if (isFixedSize)
            {
                 veStyle.flexGrow = 0;
                 veStyle.flexShrink = 1;
                 veStyle.minWidth = StyleKeyword.Auto;
                 veStyle.maxWidth = StyleKeyword.Auto;
                 veStyle.minHeight = StyleKeyword.Auto;
                 veStyle.maxHeight = StyleKeyword.Auto;
            }
            else
            {
                veStyle.flexGrow = StyleKeyword.Null;
                veStyle.flexShrink = StyleKeyword.Null;
            }
            
            static StyleLength ToStyleLength(float? nullable)
                => (nullable is { } value) ? value : StyleKeyword.Null;
            
            static StyleColor ToStyleColor(Color? nullable)
                => (nullable is { } value) ? value : StyleKeyword.Null;
        }



        protected override void OnRebuildElementGroupChildren(ElementGroup elementGroup)
        {
            var groupVe = GetUIObj(elementGroup);
            Build_ElementGroupContents(groupVe, elementGroup);
        }

        protected override void OnDestroyElement(Element element, bool isDestroyRoot)
        {
            var ve = GetUIObj(element);
            
            if (isDestroyRoot)
            {
                ve?.RemoveFromHierarchy();
            }

            UnregisterUIObj(element);
        }

        private static class UssClassName
        {
            public static readonly string UnityBaseField = "unity-base-field";
            public static readonly string UnityBaseFieldLabel = UnityBaseField + "__label";

            private static readonly string RosettaUI = "rosettaui";
            
            public static readonly string CompositeField = RosettaUI + "-composite-field";
            public static readonly string CompositeFieldContents = CompositeField + "__contents";
            public static readonly string Column = RosettaUI + "-column";
            public static readonly string Row = RosettaUI + "-row";
            public static readonly string WindowLauncher = RosettaUI + "-window-launcher";
            public static readonly string MinMaxSlider = RosettaUI + "-min-max-slider";
            public static readonly string Space = RosettaUI + "-space";
            public static readonly string DynamicElement = RosettaUI + "-dynamic-element";
            public static readonly string IndentElement = RosettaUI + "-indent";
        }
    }
}