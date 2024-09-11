using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder : BuilderBase<VisualElement>
    {
        #region Static

        public static readonly UIToolkitBuilder Instance = new();

        static UIToolkitBuilder()
        {
            RosettaUI.PopupMenu.Implement ??= new PopupMenuUIToolkit();
        }
        
        public static VisualElement Build(Element element)
        {
            return Instance.BuildInternal(element);
        }

        #endregion

        
        protected UIToolkitBuilder()
        {
            FuncTable = new()
            {
                [typeof(BoxElement)] = BuildBindFunc<Box>.Create(Bind_ElementGroup<BoxElement, Box>),
                [typeof(ClickableElement)] = BuildBindFunc<WrapElement>.Create(Bind_ClickableElement),
                [typeof(ColumnElement)] = BuildBindFunc<Column>.Create(Bind_ElementGroup<ColumnElement, Column>),
                [typeof(CompositeFieldElement)] = BuildBindFunc<CompositeField>.Create(Bind_CompositeField),
                [typeof(DynamicElement)] = BuildBindFunc<WrapElement>.Create(Bind_DynamicElement),
                [typeof(FoldElement)] = BuildBindFunc<FoldoutCustom>.Create(Bind_Fold),
                [typeof(HelpBoxElement)] = BuildBindFunc<HelpBox>.Create(Bind_HelpBox),
                [typeof(IndentElement)] = BuildBindFunc<Indent>.Create(Bind_Indent),
                [typeof(ListViewItemContainerElement)] = BuildBindFunc<ListView>.Create(Bind_ListViewItemContainer),
                [typeof(PageElement)] = BuildBindFunc<Column>.Create(Bind_ElementGroup<PageElement, Column>),
                [typeof(PopupMenuElement)] = BuildBindFunc<PopupMenu>.Create(Bind_PopupMenu),
                [typeof(RowElement)] = BuildBindFunc<Row>.Create(Bind_ElementGroup<RowElement, Row>),
                [typeof(ScrollViewElement)] = BuildBindFunc<ScrollView>.Create(Bind_ScrollView),
                [typeof(TabsElement)] = BuildBindFunc<Tabs>.Create(Bind_Tabs),
                [typeof(WindowElement)] = BuildBindFunc<Window>.Create(Bind_Window),
                [typeof(WindowLauncherElement)] = BuildBindFunc<WindowLauncher>.Create(Bind_WindowLauncher),
                
                [typeof(FloatFieldElement)] = BuildBindFunc<FloatField>.Create(Bind_Field<float, FloatField>),
                [typeof(IntFieldElement)] = BuildBindFunc<IntegerField>.Create(Bind_Field<int, IntegerField>),
                [typeof(UIntFieldElement)] = BuildBindFunc<UnsignedIntegerField>.Create(Bind_Field<uint, UnsignedIntegerField>),
                [typeof(TextFieldElement)] =  BuildBindFunc<TextField>.Create(Bind_TextField),
                [typeof(ColorFieldElement)] = BuildBindFunc<ColorField>.Create(Bind_Field<Color, ColorField>),
                [typeof(GradientFieldElement)] = BuildBindFunc<GradientField>.Create(Bind_GradientField),

                [typeof(IntSliderElement)] = BuildBindFunc<ClampFreeSliderInteger>.Create(Bind_Slider<int, ClampFreeSliderInteger>),
                [typeof(FloatSliderElement)] = BuildBindFunc<ClampFreeSlider>.Create(Bind_Slider<float, ClampFreeSlider>),
                [typeof(IntMinMaxSliderElement)] = BuildBindFunc<MinMaxSliderWithField<int, IntegerField>>.Create(Bind_MinMaxSlider<int, IntegerField>),
                [typeof(FloatMinMaxSliderElement)] = BuildBindFunc<MinMaxSliderWithField<float, FloatField>>.Create(Bind_MinMaxSlider<float, FloatField>),
                
                [typeof(LabelElement)] = BuildBindFunc<Label>.Create(Bind_Label),
                [typeof(ButtonElement)] = BuildBindFunc<Button>.Create(Bind_Button),
                [typeof(ToggleElement)] = BuildBindFunc<Toggle>.Create(Bind_Toggle),
                [typeof(DropdownElement)] = BuildBindFunc<PopupFieldCustom<string>>.Create(Bind_Dropdown),
                [typeof(SpaceElement)] = BuildBindFunc<Space>.Create(BindSimple<Space>),
                [typeof(ImageElement)] = BuildBindFunc<Image>.Create(Bind_Image),
            };
        }

        public void RegisterBuildBindFunc(Type type, IBuildBindFunc buildBindFunc) => FuncTable[type] = buildBindFunc;
        public void UnregisterBuildFunc(Type type) => FuncTable.Remove(type);

        private Dictionary<Type, IBuildBindFunc> FuncTable { get; }

        protected override VisualElement DispatchBuild(Element element)
        {
            var type = element.GetType();
            return FuncTable.TryGetValue(type, out var buildBindFunc)
                ? buildBindFunc.Build(element) 
                : null;
        }

        private bool BindSimple<TVisualElement>(Element element, VisualElement visualElement)
            where TVisualElement : VisualElement
        {
            return visualElement is TVisualElement;
        }

        /// <summary>
        /// 既存のVisualElementを新たなElementと紐づける
        /// VisualElementの構成が一致していなければ return false
        /// ElementGroupの場合、子供の構成が違ったらBuildしなおす
        /// </summary>
        /// <returns>success flag</returns>
        public bool Bind(Element element, VisualElement ve)
        {
            if (element == null || ve == null) return false;
            if (GetUIObj(element) == ve) return true;
            
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
            
            if (!FuncTable.TryGetValue(element.GetType(), out var buildBindFunc))
            {
                Debug.LogError($"{GetType()}: Unknown Type[{element.GetType()}].");
                return false;
            }

            if (!buildBindFunc.Bind(element, ve))
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
            => PrefixLabelWidthCalculator.Register(label, ve);

        protected override void OnElementEnableChanged(Element _, VisualElement ve, bool enable)
        {
            var display = enable ? DisplayStyle.Flex : DisplayStyle.None;
            ve.style.display = display;
            
            RequestResizeWindowEvent.Send(ve);
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
                veStyle.flexGrow = ToStyleFloat(style.FlexGrow);
                veStyle.flexShrink = ToStyleFloat(style.FlexShrink);
            }
            
            return;
            
            static StyleLength ToStyleLength(float? nullable)
                => nullable ?? (StyleLength)StyleKeyword.Null;

            static StyleFloat ToStyleFloat(float? nullable)
                => nullable ?? (StyleFloat)StyleKeyword.Null;
            
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