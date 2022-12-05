using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

#if UNITY_2022_1_OR_NEWER
using IntegerField = UnityEngine.UIElements.IntegerField;
using FloatField = UnityEngine.UIElements.FloatField;
#endif


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

                [typeof(IntFieldElement)] = Build_Field<int, IntegerField>,
                [typeof(UIntFieldElement)] = Build_Field<uint, UIntField>,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(TextFieldElement)] = (e) => Build_Field<string, TextField>(e, Bind_TextField),
                [typeof(ColorFieldElement)] = (e) => Build_Field<Color, ColorField>(e, Bind_ColorField),

                [typeof(LabelElement)] = Build_Label,
                [typeof(ToggleElement)] = (e) => Build_Field<bool, Toggle>(e, Bind_Toggle),
                
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
            
            
            BindFuncTable = new()
            {
                [typeof(IntFieldElement)] = Bind_Field<int, IntegerField>,
                [typeof(UIntFieldElement)] = Bind_Field<uint, UIntField>,
                [typeof(FloatFieldElement)] = Bind_Field<float, FloatField>,
                [typeof(TextFieldElement)] =  Bind_TextField,
                [typeof(ColorFieldElement)] = Bind_ColorField,
                
                [typeof(LabelElement)] = Bind_Label,
                [typeof(ToggleElement)] = Bind_Toggle,
            };
        }

        public void RegisterBuildFunc(Type type, Func<Element, VisualElement> func) => _buildFuncTable[type] = func;
        public void UnregisterBuildFunc(Type type) => _buildFuncTable.Remove(type);
        

        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> BuildFuncTable => _buildFuncTable;

        private Dictionary<Type, Func<Element, VisualElement, bool>> BindFuncTable { get; }

        /// <summary>
        /// 既存のVisualElementを新たなElementと紐づける
        /// VisualElementの構成が一致していなければ return false
        /// </summary>
        /// <returns>success flag</returns>
        public bool Bind(Element element, VisualElement ve)
        {
            if (element == null) return false;
            
            // 親なしはとりあえず禁止
            // 新Elementを旧Elementのヒエラルキー上に入れ忘れ防止
            // PrefixLabelの幅計算で正しい親が必要
            Assert.IsNotNull(element.Parent);
                
            Unbind(element);
            var prevElement = GetElement(ve);
            if (prevElement != null)
            {
                Unbind(prevElement);
            }

            if (!BindFuncTable.TryGetValue(element.GetType(), out var func))
            {
                Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                return false;
            }

            if (!func.Invoke(element, ve)) return false;

            SetupUIObj(element, ve);
            
            return true;
        }

        public void Unbind(Element element)
        {
            foreach (var child in element.Children)
            {
                Unbind(child);
            }
            TeardownUIObj(element);
        }


        // プレフィックスラベルの幅を計算する
        protected override void CalcPrefixLabelWidthWithIndent(LabelElement label, VisualElement ve)
        {
            // すでにパネルにアタッチされている＝Build時ではなくBind時
            // レイアウト計算が終わってるはずなので即計算する
            if (ve.panel != null)
            {
                CalcMinWidth();
            }
            // Build時はまだレイアウト計算が終わっていないのでGeometryChangedを待つ
            else
            {
                ve.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }

            // リストの要素など１回目のあとにレイアウト変更がされるので落ち着くまで複数回呼ばれるようにする
            void OnGeometryChanged(GeometryChangedEvent evt)
            {
                // 移動してなければ落ち着いたと見てコールバック解除
                // UpdateLabelWidth()でスタイルを変えているので少なくとも一度は再度呼ばれる
                if (Mathf.Approximately(evt.oldRect.xMin, evt.newRect.xMin))
                {
                    ve.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    return;
                }

                CalcMinWidth();
            }

            void CalcMinWidth()
            {
                if (label.Parent == null)
                {
                    Debug.LogWarning($"Label parent is null. [{label.Value}]");
                }

                var marginLeft = ve.worldBound.xMin;
          
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
                 veStyle.flexShrink = 0;
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
                => nullable ?? (StyleLength)StyleKeyword.Null;
            
            static StyleColor ToStyleColor(Color? nullable)
                => nullable ?? (StyleColor)StyleKeyword.Null;
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
            private static readonly string RosettaUI = "rosettaui";
            
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