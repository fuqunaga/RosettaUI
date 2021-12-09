using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
  /// <summary>
  ///        <para>
  /// Makes a text field for entering a float.
  /// </para>
  ///      </summary>
  /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=FloatField">`FloatField` on docs.unity3d.com</a></footer>
  public class FloatField : TextValueField<float>
  {
    /// <summary>
    ///        <para>
    /// USS class name of elements of this type.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField.ussClassName">`FloatField.ussClassName` on docs.unity3d.com</a></footer>
    public new static readonly string ussClassName = "unity-float-field";
    /// <summary>
    ///        <para>
    /// USS class name of labels in elements of this type.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField.labelUssClassName">`FloatField.labelUssClassName` on docs.unity3d.com</a></footer>
    public new static readonly string labelUssClassName = ussClassName + "__label";
    /// <summary>
    ///        <para>
    /// USS class name of input elements in elements of this type.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField.inputUssClassName">`FloatField.inputUssClassName` on docs.unity3d.com</a></footer>
    public new static readonly string inputUssClassName = ussClassName + "__input";

    private FloatInput floatInput => (FloatInput) this.textInputBase;

    /// <summary>
    ///        <para>
    /// Converts the given float to a string.
    /// </para>
    ///      </summary>
    /// <param name="v">The float to be converted to string.</param>
    /// <returns>
    ///   <para>The float as string.</para>
    /// </returns>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField.ValueToString">`FloatField.ValueToString` on docs.unity3d.com</a></footer>
    protected override string ValueToString(float v) => v.ToString(this.formatString, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);

    /// <summary>
    ///        <para>
    /// Converts a string to a float.
    /// </para>
    ///      </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>
    ///   <para>The float parsed from the string.</para>
    /// </returns>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField.StringToValue">`FloatField.StringToValue` on docs.unity3d.com</a></footer>
    protected override float StringToValue(string str)
    {
      double num;
      EditorGUI.StringToDouble(str, out num);
      return MathUtils.ClampToFloat(num);
    }

    /// <summary>
    ///        <para>
    /// Constructor.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField">`FloatField` on docs.unity3d.com</a></footer>
    public FloatField()
      : this((string) null)
    {
    }

    /// <summary>
    ///        <para>
    /// Constructor.
    /// </para>
    ///      </summary>
    /// <param name="maxLength">Maximum number of characters the field can take.</param>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField">`FloatField` on docs.unity3d.com</a></footer>
    public FloatField(int maxLength)
      : this((string) null, maxLength)
    {
    }

    /// <summary>
    ///        <para>
    /// Constructor.
    /// </para>
    ///      </summary>
    /// <param name="maxLength">Maximum number of characters the field can take.</param>
    /// <param name="label"></param>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField">`FloatField` on docs.unity3d.com</a></footer>
    public FloatField(string label, int maxLength = -1)
      : base(label, maxLength, (TextValueInput) new FloatInput())
    {
      this.AddToClassList(ussClassName);
      this.labelElement.AddToClassList(labelUssClassName);
      this.visualInput.AddToClassList(inputUssClassName);
      this.AddLabelDragger<float>();
    }

    internal override bool CanTryParse(string textString) => float.TryParse(textString, out float _);

    /// <summary>
    ///        <para>
    /// Modify the value using a 3D delta and a speed, typically coming from an input device.
    /// </para>
    ///      </summary>
    /// <param name="delta">A vector used to compute the value change.</param>
    /// <param name="speed">A multiplier for the value change.</param>
    /// <param name="startValue">The start value.</param>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.FloatField.ApplyInputDeviceDelta">`FloatField.ApplyInputDeviceDelta` on docs.unity3d.com</a></footer>
    public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue) => this.floatInput.ApplyInputDeviceDelta(delta, speed, startValue);

    /// <summary>
    ///        <para>
    /// Instantiates a FloatField using the data read from a UXML file.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UxmlFactory">`FloatField.UxmlFactory` on docs.unity3d.com</a></footer>
    public new class UxmlFactory : UxmlFactory<FloatField, UxmlTraits>
    {
    }

    /*
    /// <summary>
    ///        <para>
    /// Defines UxmlTraits for the FloatField.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UxmlTraits">`FloatField.UxmlTraits` on docs.unity3d.com</a></footer>
    public new class UxmlTraits : TextValueFieldTraits<float, UxmlFloatAttributeDescription>
    {
    }
    */

    private class FloatInput : TextValueInput
    {
      private FloatField parentFloatField => (FloatField) this.parent;

      internal FloatInput() => this.formatString = EditorGUI.kFloatFieldFormatString;

      protected override string allowedCharacters => EditorGUI.s_AllowedCharactersForFloat;

      public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue)
      {
        double floatDragSensitivity = NumericFieldDraggerUtility.CalculateFloatDragSensitivity((double) startValue);
        float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
        double num = MathUtils.RoundBasedOnMinimumDifference((double) this.StringToValue(this.text) + (double) NumericFieldDraggerUtility.NiceDelta((Vector2) delta, acceleration) * floatDragSensitivity, floatDragSensitivity);
        if (this.parentFloatField.isDelayed)
          this.text = this.ValueToString(MathUtils.ClampToFloat(num));
        else
          this.parentFloatField.value = MathUtils.ClampToFloat(num);
      }

      protected override string ValueToString(float v) => v.ToString(this.formatString);

      protected override float StringToValue(string str)
      {
        double num;
        EditorGUI.StringToDouble(str, out num);
        return MathUtils.ClampToFloat(num);
      }
    }
  }
}
