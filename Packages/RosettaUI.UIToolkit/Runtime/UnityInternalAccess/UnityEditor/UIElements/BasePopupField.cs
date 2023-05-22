using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    /// <summary>
    ///        <para>
    /// This is the base class for all the popup field elements.
    /// TValue and TChoice can be different, see MaskField,
    ///   or the same, see PopupField
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=BasePopupField">`BasePopupField` on docs.unity3d.com</a></footer>
    public abstract class BasePopupField<TValueType, TValueChoice> : BaseField<TValueType>
    {
        internal List<TValueChoice> m_Choices;
        private TextElement m_TextElement;
        private VisualElement m_ArrowElement;
        internal Func<TValueChoice, string> m_FormatSelectedValueCallback;
        internal Func<TValueChoice, string> m_FormatListItemCallback;
        internal Func<IGenericMenu> createMenuCallback;
        public new static readonly string ussClassName = "unity-base-popup-field";

        public static readonly string textUssClassName =
            BasePopupField<TValueType, TValueChoice>.ussClassName + "__text";

        public static readonly string arrowUssClassName =
            BasePopupField<TValueType, TValueChoice>.ussClassName + "__arrow";

        public new static readonly string labelUssClassName =
            BasePopupField<TValueType, TValueChoice>.ussClassName + "__label";

        public new static readonly string inputUssClassName =
            BasePopupField<TValueType, TValueChoice>.ussClassName + "__input";

        protected TextElement textElement => this.m_TextElement;

        internal abstract string GetValueToDisplay();

        internal abstract string GetListItemToDisplay(TValueType item);

        internal abstract void AddMenuItems(IGenericMenu menu);

        public virtual List<TValueChoice> choices
        {
            get => this.m_Choices;
            set
            {
                this.m_Choices = value != null ? value : throw new ArgumentNullException(nameof(value));
                this.SetValueWithoutNotify(this.rawValue);
            }
        }

        public override void SetValueWithoutNotify(TValueType newValue)
        {
            base.SetValueWithoutNotify(newValue);
            ((INotifyValueChanged<string>) this.m_TextElement).SetValueWithoutNotify(this.GetValueToDisplay());
        }

        public string text => this.m_TextElement.text;

        internal BasePopupField()
            : this((string) null)
        {
        }

        internal BasePopupField(string label)
            : base(label, (VisualElement) null)
        {
            this.AddToClassList(BasePopupField<TValueType, TValueChoice>.ussClassName);
            this.labelElement.AddToClassList(BasePopupField<TValueType, TValueChoice>.labelUssClassName);
            BasePopupField<TValueType, TValueChoice>.PopupTextElement popupTextElement =
                new BasePopupField<TValueType, TValueChoice>.PopupTextElement();
            popupTextElement.pickingMode = PickingMode.Ignore;
            this.m_TextElement = (TextElement) popupTextElement;
            this.m_TextElement.AddToClassList(BasePopupField<TValueType, TValueChoice>.textUssClassName);
            this.visualInput.AddToClassList(BasePopupField<TValueType, TValueChoice>.inputUssClassName);
            this.visualInput.Add((VisualElement) this.m_TextElement);
            this.m_ArrowElement = new VisualElement();
            this.m_ArrowElement.AddToClassList(BasePopupField<TValueType, TValueChoice>.arrowUssClassName);
            this.m_ArrowElement.pickingMode = PickingMode.Ignore;
            this.visualInput.Add(this.m_ArrowElement);
            this.choices = new List<TValueChoice>();
            this.RegisterCallback<PointerDownEvent>(new EventCallback<PointerDownEvent>(this.OnPointerDownEvent));
            this.RegisterCallback<PointerMoveEvent>(new EventCallback<PointerMoveEvent>(this.OnPointerMoveEvent));
            this.RegisterCallback<MouseDownEvent>((EventCallback<MouseDownEvent>) (e =>
            {
                if (e.button != 0)
                    return;
                e.StopPropagation();
            }));
        }

        private void OnPointerDownEvent(PointerDownEvent evt) =>
            this.ProcessPointerDown<PointerDownEvent>((PointerEventBase<PointerDownEvent>) evt);

        private void OnPointerMoveEvent(PointerMoveEvent evt)
        {
            if (evt.button != 0 || (uint) (evt.pressedButtons & 1) <= 0U)
                return;
            this.ProcessPointerDown<PointerMoveEvent>((PointerEventBase<PointerMoveEvent>) evt);
        }

        private void ProcessPointerDown<T>(PointerEventBase<T> evt) where T : PointerEventBase<T>, new()
        {
            if (evt.button != 0 ||
                !this.visualInput.ContainsPoint(this.visualInput.WorldToLocal(evt.position)))
                return;
            this.ShowMenu();
            evt.StopPropagation();
        }

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt == null || !(evt is KeyDownEvent keyDownEvent) || keyDownEvent.keyCode != KeyCode.Space &&
                keyDownEvent.keyCode != KeyCode.KeypadEnter && keyDownEvent.keyCode != KeyCode.Return)
                return;
            this.ShowMenu();
            evt.StopPropagation();
        }

        private void ShowMenu()
        {
            IGenericMenu menu;
            if (this.createMenuCallback != null)
            {
                menu = this.createMenuCallback();
            }
            else
            {
                BaseVisualElementPanel elementPanel = this.elementPanel;
                menu = (elementPanel != null ? (elementPanel.contextType == ContextType.Player ? 1 : 0) : 0) != 0
                    ? (IGenericMenu) new GenericDropdownMenu()
                    : DropdownUtility.CreateDropdown();
            }

            this.AddMenuItems(menu);
            menu.DropDown(this.visualInput.worldBound, (VisualElement) this, true);
        }

        protected override void UpdateMixedValueContent()
        {
            if (this.showMixedValue)
                this.textElement.text = BaseField<TValueType>.mixedValueString;
            this.textElement.EnableInClassList(BaseField<TValueType>.mixedValueLabelUssClassName, this.showMixedValue);
        }

        private class PopupTextElement : TextElement
        {
            protected internal override Vector2 DoMeasure(
                float desiredWidth,
                VisualElement.MeasureMode widthMode,
                float desiredHeight,
                VisualElement.MeasureMode heightMode)
            {
                string textToMeasure = this.text;
                if (string.IsNullOrEmpty(textToMeasure))
                    textToMeasure = " ";
                return this.MeasureTextSize(textToMeasure, desiredWidth, widthMode, desiredHeight, heightMode);
            }
        }
    }
}