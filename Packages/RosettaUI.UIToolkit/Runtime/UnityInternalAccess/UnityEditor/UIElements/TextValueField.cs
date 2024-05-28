#if !UNITY_2022_1_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
  /// <summary>
  ///        <para>
  /// Base class for text fields.
  /// </para>
  ///      </summary>
  /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=TextValueField">`TextValueField` on docs.unity3d.com</a></footer>
  public abstract class TextValueField<TValueType> : 
    TextInputBaseField<TValueType>,
    IValueField<TValueType>
  {
    private BaseFieldMouseDragger m_Dragger;

    private TextValueField<TValueType>.TextValueInput textValueInput => (TextValueField<TValueType>.TextValueInput) this.textInputBase;

    public string formatString
    {
      get => this.textValueInput.formatString;
      set
      {
        this.textValueInput.formatString = value;
        this.SetValueWithoutNotify(this.value);
      }
    }

    protected TextValueField(
      int maxLength,
      TextValueField<TValueType>.TextValueInput textValueInput)
      : this((string) null, maxLength, textValueInput)
    {
    }

    protected TextValueField(
      string label,
      int maxLength,
      TextValueField<TValueType>.TextValueInput textValueInput)
      : base(label, maxLength, char.MinValue, (TextInputBaseField<TValueType>.TextInputBase) textValueInput)
    {
      this.SetValueWithoutNotify(default (TValueType));
      this.onIsReadOnlyChanged += new Action<bool>(this.OnIsReadOnlyChanged);
    }

    public abstract void ApplyInputDeviceDelta(
      Vector3 delta,
      DeltaSpeed speed,
      TValueType startValue);

    public void StartDragging()
    {
      if (this.showMixedValue)
        this.value = default (TValueType);
      this.textValueInput.StartDragging();
    }

    public void StopDragging() => this.textValueInput.StopDragging();

    public override TValueType value
    {
      get => base.value;
      set
      {
        base.value = value;
        if (!this.textValueInput.m_UpdateTextFromValue)
          return;
        this.text = this.ValueToString(this.rawValue);
      }
    }

    private void OnIsReadOnlyChanged(bool newValue) => this.EnableLabelDragger(!newValue);

    internal virtual bool CanTryParse(string textString) => false;

    protected void AddLabelDragger<TDraggerType>()
    {
      this.m_Dragger = (BaseFieldMouseDragger) new FieldMouseDragger<TDraggerType>((IValueField<TDraggerType>) this);
      this.EnableLabelDragger(!this.isReadOnly);
    }

    private void EnableLabelDragger(bool enable)
    {
      if (this.m_Dragger == null)
        return;
      this.m_Dragger.SetDragZone(enable ? (VisualElement) this.labelElement : (VisualElement) null);
      this.labelElement.EnableInClassList(BaseField<TValueType>.labelDraggerVariantUssClassName, enable);
    }

    public override void SetValueWithoutNotify(TValueType newValue)
    {
      base.SetValueWithoutNotify(newValue);
      if (!this.textValueInput.m_UpdateTextFromValue)
        return;
      this.text = this.ValueToString(this.rawValue);
    }

    /// <summary>
    ///        <para>
    /// This is the inner representation of the Text input.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=TextValueInput">`TextValueField.TextValueInput` on docs.unity3d.com</a></footer>
    protected abstract class TextValueInput : TextInputBaseField<TValueType>.TextInputBase
    {
      internal bool m_UpdateTextFromValue;

      private TextValueField<TValueType> textValueFieldParent => (TextValueField<TValueType>) this.parent;

      protected TextValueInput() => this.m_UpdateTextFromValue = true;

      internal override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != char.MinValue && this.allowedCharacters.IndexOf(c) != -1;

      protected abstract string allowedCharacters { get; }

      public string formatString { get; set; }

      public abstract void ApplyInputDeviceDelta(
        Vector3 delta,
        DeltaSpeed speed,
        TValueType startValue);

      public void StartDragging()
      {
        this.isDragging = true;
        this.SelectNone();
        this.MarkDirtyRepaint();
      }

      public void StopDragging()
      {
        if (this.textValueFieldParent.isDelayed)
          this.UpdateValueFromText();
        this.isDragging = false;
        this.SelectAll();
        this.MarkDirtyRepaint();
      }

      protected abstract string ValueToString(TValueType value);

      protected override TValueType StringToValue(string str) => base.StringToValue(str);

      protected override void ExecuteDefaultActionAtTarget(EventBase evt)
      {
        base.ExecuteDefaultActionAtTarget(evt);
        bool flag = false;
        if (evt.eventTypeId == EventBase<KeyDownEvent>.TypeId())
        {
          // ISSUE: explicit non-virtual call
          // char? nullable1 = evt is KeyDownEvent keyDownEvent2 ? new char?(/*__nonvirtual*/ (keyDownEvent2.character)) : new char?();
          char? nullable1 = new char?();;
          if (evt is KeyDownEvent keyDownEvent2)
          {
            nullable1 = keyDownEvent2.character;
          }
          else
          {
            keyDownEvent2 = null;
          }
           
          int? nullable2 = nullable1.HasValue ? new int?((int) nullable1.GetValueOrDefault()) : new int?();
          int num1 = 3;
          int num2;
          if (!(nullable2.GetValueOrDefault() == num1 & nullable2.HasValue))
          {
            nullable1 = keyDownEvent2?.character;
            nullable2 = nullable1.HasValue ? new int?((int) nullable1.GetValueOrDefault()) : new int?();
            int num3 = 10;
            num2 = nullable2.GetValueOrDefault() == num3 & nullable2.HasValue ? 1 : 0;
          }
          else
            num2 = 1;
          if (num2 != 0)
          {
            this.parent.Focus();
            evt.StopPropagation();
            evt.PreventDefault();
          }
          else if (!this.isReadOnly)
            flag = true;
        }
        else if (!this.isReadOnly && evt.eventTypeId == EventBase<ExecuteCommandEvent>.TypeId())
        {
          string commandName = (evt as ExecuteCommandEvent).commandName;
          if (commandName == "Paste" || commandName == "Cut")
            flag = true;
        }
        if (!(!(this.textValueFieldParent?.isDelayed ?? false) & flag))
          return;
        this.m_UpdateTextFromValue = false;
        try
        {
          this.UpdateValueFromText();
        }
        finally
        {
          this.m_UpdateTextFromValue = true;
        }
      }

      protected override void ExecuteDefaultAction(EventBase evt)
      {
        base.ExecuteDefaultAction(evt);
        if (evt == null)
          return;
        if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
        {
          if (string.IsNullOrEmpty(this.text))
            this.textValueFieldParent.value = default (TValueType);
          else
            this.UpdateValueFromText();
        }
        else
        {
          if (evt.eventTypeId != EventBase<FocusEvent>.TypeId() || !this.textValueFieldParent.showMixedValue)
            return;
          this.textValueFieldParent.value = default (TValueType);
        }
      }
    }
  }
}


#endif