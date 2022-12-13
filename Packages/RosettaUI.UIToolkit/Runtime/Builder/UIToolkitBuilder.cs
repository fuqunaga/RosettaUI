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
                [typeof(CompositeFieldElement)] = BuildSimple<CompositeField>,
                [typeof(DynamicElement)] = BuildSimple<VisualElement>,
                [typeof(FoldElement)] = BuildSimple<FoldoutCustom>,
                [typeof(HelpBoxElement)] = BuildSimple<HelpBox>,
                [typeof(RowElement)] = BuildSimple<Row>,
                [typeof(ColumnElement)] = BuildSimple<Column>,
                [typeof(BoxElement)] = BuildSimple<Box>,
                [typeof(IndentElement)] = BuildSimple<Indent>,
                [typeof(PageElement)] = BuildSimple<Column>,
                
                [typeof(ScrollViewElement)] = BuildSimple<ScrollView>,
                [typeof(TabsElement)] = BuildSimple<Tabs>,
                [typeof(WindowElement)] = BuildSimple<Window>,
                [typeof(WindowLauncherElement)] = BuildSimple<WindowLauncher>,

                [typeof(IntFieldElement)] = BuildSimple<IntegerField>,
                [typeof(UIntFieldElement)] = BuildSimple<UIntField>,
                [typeof(FloatFieldElement)] = BuildSimple<FloatField>,
                [typeof(TextFieldElement)] = BuildSimple<TextField>,
                [typeof(ColorFieldElement)] = BuildSimple<ColorField>,

                [typeof(LabelElement)] = BuildSimple<Label>,
                [typeof(ToggleElement)] = BuildSimple<Toggle>,
                
                [typeof(IntSliderElement)] = BuildSimple<ClampFreeSliderInt>,
                [typeof(FloatSliderElement)] = BuildSimple<ClampFreeSlider>,
                [typeof(IntMinMaxSliderElement)] = BuildSimple<MinMaxSliderWithField<int, IntegerField>>,
                [typeof(FloatMinMaxSliderElement)] = BuildSimple<MinMaxSliderWithField<float, FloatField>>,
                
                [typeof(DropdownElement)] = BuildSimple<PopupFieldCustomMenu<string>>,
                [typeof(SpaceElement)] = BuildSimple<Space>,
                [typeof(ImageElement)] = BuildSimple<Image>,
                [typeof(ButtonElement)] = BuildSimple<Button>,
                [typeof(PopupMenuElement)] = BuildSimple<PopupMenu>,
                [typeof(ListViewItemContainerElement)] = BuildSimple<ListViewCustom>,
            };
            
            
            BindFuncTable = new()
            {
                [typeof(CompositeFieldElement)] = Bind_CompositeField,
                [typeof(DynamicElement)] = Bind_DynamicElement,
                [typeof(FoldElement)] = Bind_Fold,
                [typeof(HelpBoxElement)] = Bind_HelpBox,
                [typeof(RowElement)] = Bind_ElementGroup<RowElement, Row>,
                [typeof(ColumnElement)] = Bind_ElementGroup<ColumnElement, Column>,
                [typeof(BoxElement)] = Bind_ElementGroup<BoxElement, Box>,
                [typeof(IndentElement)] = Bind_Indent,
                [typeof(PageElement)] = Bind_ElementGroup<PageElement, Column>,
                
                [typeof(ScrollViewElement)] = Bind_ScrollView,
                [typeof(TabsElement)] = Bind_Tabs,
                [typeof(WindowElement)] = Bind_Window,
                [typeof(WindowLauncherElement)] = Bind_WindowLauncher,
                
                [typeof(IntFieldElement)] = Bind_Field<int, IntegerField>,
                [typeof(UIntFieldElement)] = Bind_Field<uint, UIntField>,
                [typeof(FloatFieldElement)] = Bind_Field<float, FloatField>,
                [typeof(TextFieldElement)] =  Bind_TextField,
                [typeof(ColorFieldElement)] = Bind_ColorField,
                
                [typeof(LabelElement)] = Bind_Label,
                [typeof(ToggleElement)] = Bind_Toggle,
                
                [typeof(IntSliderElement)] = Bind_Slider<int, ClampFreeSliderInt>,
                [typeof(FloatSliderElement)] = Bind_Slider<float, ClampFreeSlider>,
                [typeof(IntMinMaxSliderElement)] = Bind_MinMaxSlider<int, IntegerField>,
                [typeof(FloatMinMaxSliderElement)] = Bind_MinMaxSlider<float, FloatField>,
                
                [typeof(DropdownElement)] = Bind_Dropdown,
                [typeof(SpaceElement)] = BindSimple<Space>,
                [typeof(ImageElement)] = Bind_Image,
                [typeof(ButtonElement)] = Bind_Button,
                [typeof(PopupMenuElement)] = Bind_PopupMenu,
                [typeof(ListViewItemContainerElement)] = Bind_ListViewItemContainer
            };
        }

        public void RegisterBuildFunc(Type type, Func<Element, VisualElement> func) => _buildFuncTable[type] = func;
        public void UnregisterBuildFunc(Type type) => _buildFuncTable.Remove(type);
        

        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> BuildFuncTable => _buildFuncTable;

        private Dictionary<Type, Func<Element, VisualElement, bool>> BindFuncTable { get; }

        /// <summary>
        /// Build時の特殊処理がないBindするだけのBuild
        /// </summary>
        private VisualElement BuildSimple<TVisualElement>(Element element)
            where TVisualElement : VisualElement, new()
        {
            var ve = new TVisualElement();
            var success = Bind(element, ve);
            Assert.IsTrue(success);
            return ve;
        }

        private bool BindSimple<TVisualElement>(Element element, VisualElement visualElement)
            where TVisualElement : VisualElement
        {
            return visualElement is TVisualElement;
        }

        /// <summary>
        /// 既存のVisualElementを新たなElementと紐づける
        /// VisualElementの構成が一致していなければ return false
        /// </summary>
        /// <returns>success flag</returns>
        public bool Bind(Element element, VisualElement ve)
        {
            if (element == null || ve == null) return false;
            
            // 親なしはとりあえず禁止
            // 新Elementを旧Elementのヒエラルキー上に入れ忘れ防止
            // PrefixLabelの幅計算で正しい親が必要
            // TODO: あとで削除する。Windowなしで各Elementがヒエラルキー上のルートになることはありえる
            if (element is not WindowElement)
            {
                Assert.IsNotNull(element.Parent, $"{element.GetType()} FirstLabel[{element.FirstLabel()?.Value}]");
            }

            Unbind(element);
            var prevElement = GetElement(ve);
            if (prevElement != null)
            {
                Unbind(prevElement);
            }

            // BindFunc内でGetUIObj()をしたいので先に登録しておく
            // プレフィックスラベルの幅を求める計算がBuild時はコールバックで呼ばれるのでBuild後で済むが、
            // Bind時はデフォルトの幅になったままレイアウト処理が行われるとレイアウトが変更され重たいので、
            // すぐにBind前の幅に戻してしておきたい
            // したがってBindFunc内でGetUIObj()できるように先にSetupUIObj()を呼んでおく
            SetupUIObj(element, ve);
            
            if (!BindFuncTable.TryGetValue(element.GetType(), out var func))
            {
                Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                return false;
            }

            if (!func.Invoke(element, ve))
            {
                TeardownUIObj(element);
                return false;
            }

            
            return true;
        }

        public VisualElement Unbind(Element element)
        {
            foreach (var child in element.Children)
            {
                Unbind(child);
            }
            return TeardownUIObj(element);
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

        protected override void OnDetachView(Element element, bool destroyView)
        {
            var ve = GetUIObj(element);
            Unbind(element);   
            
            if (destroyView)
            {
                ve?.RemoveFromHierarchy();
            }
        }
    }
}