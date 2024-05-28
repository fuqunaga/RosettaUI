#if !UNITY_2022_1_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    /// <summary>
    ///        <para>
    /// Generic popup selection field.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=PopupField">`PopupField` on docs.unity3d.com</a></footer>
    public class PopupField<T> : BasePopupField<T, T>
    {
        internal const int kPopupFieldDefaultIndex = -1;
        private int m_Index = -1;
        public new static readonly string ussClassName = "unity-popup-field";
        public new static readonly string labelUssClassName = PopupField<T>.ussClassName + "__label";
        public new static readonly string inputUssClassName = PopupField<T>.ussClassName + "__input";

        public virtual Func<T, string> formatSelectedValueCallback
        {
            get => this.m_FormatSelectedValueCallback;
            set
            {
                this.m_FormatSelectedValueCallback = value;
                this.textElement.text = this.GetValueToDisplay();
            }
        }

        public virtual Func<T, string> formatListItemCallback
        {
            get => this.m_FormatListItemCallback;
            set => this.m_FormatListItemCallback = value;
        }

        internal override string GetValueToDisplay() => this.m_FormatSelectedValueCallback != null
            ? this.m_FormatSelectedValueCallback(this.value)
            : ((object) this.value != null ? this.value.ToString() : string.Empty);

        internal override string GetListItemToDisplay(T value) => this.m_FormatListItemCallback != null
            ? this.m_FormatListItemCallback(value)
            : ((object) value == null || !this.m_Choices.Contains(value) ? string.Empty : value.ToString());

        public override T value
        {
            get => base.value;
            set
            {
                this.m_Index = this.m_Choices.IndexOf(value);
                base.value = value;
            }
        }

        public override void SetValueWithoutNotify(T newValue)
        {
            this.m_Index = this.m_Choices.IndexOf(newValue);
            base.SetValueWithoutNotify(newValue);
        }

        public int index
        {
            get => this.m_Index;
            set
            {
                if (value == this.m_Index)
                    return;
                this.m_Index = value;
                if (this.m_Index >= 0 && this.m_Index < this.m_Choices.Count)
                    this.value = this.m_Choices[this.m_Index];
                else
                    this.value = default(T);
            }
        }

        public PopupField()
            : this((string) null)
        {
        }

        public PopupField(string label = null)
            : base(label)
        {
            this.AddToClassList(PopupField<T>.ussClassName);
            this.labelElement.AddToClassList(PopupField<T>.labelUssClassName);
            this.visualInput.AddToClassList(PopupField<T>.inputUssClassName);
        }

        public PopupField(
            List<T> choices,
            T defaultValue,
            Func<T, string> formatSelectedValueCallback = null,
            Func<T, string> formatListItemCallback = null)
            : this((string) null, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(
            string label,
            List<T> choices,
            T defaultValue,
            Func<T, string> formatSelectedValueCallback = null,
            Func<T, string> formatListItemCallback = null)
            : this(label)
        {
            if ((object) defaultValue == null)
                throw new ArgumentNullException(nameof(defaultValue));
            this.choices = choices;
            if (!this.m_Choices.Contains(defaultValue))
                throw new ArgumentException(string.Format(
                    "Default value {0} is not present in the list of possible values", (object) defaultValue));
            this.SetValueWithoutNotify(defaultValue);
            this.formatListItemCallback = formatListItemCallback;
            this.formatSelectedValueCallback = formatSelectedValueCallback;
        }

        public PopupField(
            List<T> choices,
            int defaultIndex,
            Func<T, string> formatSelectedValueCallback = null,
            Func<T, string> formatListItemCallback = null)
            : this((string) null, choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
        {
        }

        public PopupField(
            string label,
            List<T> choices,
            int defaultIndex,
            Func<T, string> formatSelectedValueCallback = null,
            Func<T, string> formatListItemCallback = null)
            : this(label)
        {
            this.choices = choices;
            this.index = defaultIndex;
            this.formatListItemCallback = formatListItemCallback;
            this.formatSelectedValueCallback = formatSelectedValueCallback;
        }

        internal override void AddMenuItems(IGenericMenu menu)
        {
            if (menu == null)
                throw new ArgumentNullException(nameof(menu));
            foreach (T choice in this.m_Choices)
            {
                T item = choice;
                bool isChecked = EqualityComparer<T>.Default.Equals(item, this.value);
                menu.AddItem(this.GetListItemToDisplay(item), isChecked,
                    (Action) (() => this.ChangeValueFromMenu(item)));
            }
        }

        private void ChangeValueFromMenu(T menuItem) => this.value = menuItem;
    }
}

#endif