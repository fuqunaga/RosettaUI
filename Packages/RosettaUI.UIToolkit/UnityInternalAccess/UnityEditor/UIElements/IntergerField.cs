using System;
using System.Globalization;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    /// <summary>
    ///        <para>
    /// Makes a text field for entering an integer.
    /// </para>
    ///      </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=IntegerField">`IntegerField` on docs.unity3d.com</a></footer>
    public class IntegerField : TextValueField<int>
    {
        /// <summary>
        ///        <para>
        /// USS class name of elements of this type.
        /// </para>
        ///      </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField.ussClassName">`IntegerField.ussClassName` on docs.unity3d.com</a></footer>
        public new static readonly string ussClassName = "unity-integer-field";

        /// <summary>
        ///        <para>
        /// USS class name of labels in elements of this type.
        /// </para>
        ///      </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField.labelUssClassName">`IntegerField.labelUssClassName` on docs.unity3d.com</a></footer>
        public new static readonly string labelUssClassName = IntegerField.ussClassName + "__label";

        /// <summary>
        ///        <para>
        /// USS class name of input elements in elements of this type.
        /// </para>
        ///      </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField.inputUssClassName">`IntegerField.inputUssClassName` on docs.unity3d.com</a></footer>
        public new static readonly string inputUssClassName = IntegerField.ussClassName + "__input";

        private IntegerField.IntegerInput integerInput => (IntegerField.IntegerInput) this.textInputBase;

        /// <summary>
        ///        <para>
        /// Converts the given integer to a string.
        /// </para>
        ///      </summary>
        /// <param name="v">The integer to be converted to string.</param>
        /// <returns>
        ///   <para>The integer as string.</para>
        /// </returns>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField.ValueToString">`IntegerField.ValueToString` on docs.unity3d.com</a></footer>
        protected override string ValueToString(int v) => v.ToString(this.formatString,
            (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);

        /// <summary>
        ///        <para>
        /// Converts a string to an integer.
        /// </para>
        ///      </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>
        ///   <para>The integer parsed from the string.</para>
        /// </returns>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField.StringToValue">`IntegerField.StringToValue` on docs.unity3d.com</a></footer>
        protected override int StringToValue(string str)
        {
            long num;
            EditorGUI.StringToLong(str, out num);
            return MathUtils.ClampToInt(num);
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField">`IntegerField` on docs.unity3d.com</a></footer>
        public IntegerField()
            : this((string) null)
        {
        }

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField">`IntegerField` on docs.unity3d.com</a></footer>
        public IntegerField(int maxLength)
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
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField">`IntegerField` on docs.unity3d.com</a></footer>
        public IntegerField(string label, int maxLength = -1)
            : base(label, maxLength, (TextValueField<int>.TextValueInput) new IntegerField.IntegerInput())
        {
            this.AddToClassList(IntegerField.ussClassName);
            this.labelElement.AddToClassList(IntegerField.labelUssClassName);
            this.visualInput.AddToClassList(IntegerField.inputUssClassName);
            this.AddLabelDragger<int>();
        }

        internal override bool CanTryParse(string textString) => int.TryParse(textString, out int _);

        /// <summary>
        ///        <para>
        /// Modify the value using a 3D delta and a speed, typically coming from an input device.
        /// </para>
        ///      </summary>
        /// <param name="delta">A vector used to compute the value change.</param>
        /// <param name="speed">A multiplier for the value change.</param>
        /// <param name="startValue">The start value.</param>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UIElements.IntegerField.ApplyInputDeviceDelta">`IntegerField.ApplyInputDeviceDelta` on docs.unity3d.com</a></footer>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue) =>
            this.integerInput.ApplyInputDeviceDelta(delta, speed, startValue);

        /// <summary>
        ///        <para>
        /// Instantiates an IntegerField using the data read from a UXML file.
        /// </para>
        ///      </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UxmlFactory">`IntegerField.UxmlFactory` on docs.unity3d.com</a></footer>
        public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<IntegerField, IntegerField.UxmlTraits>
        {
        }

        /// <summary>
        ///        <para>
        /// Defines UxmlTraits for the IntegerField.
        /// </para>
        ///      </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=UxmlTraits">`IntegerField.UxmlTraits` on docs.unity3d.com</a></footer>
        public new class UxmlTraits : TextValueFieldTraits<int, UxmlIntAttributeDescription>
        {
        }

        private class IntegerInput : TextValueField<int>.TextValueInput
        {
            private IntegerField parentIntegerField => (IntegerField) this.parent;

            internal IntegerInput() => this.formatString = EditorGUI.kIntFieldFormatString;

            protected override string allowedCharacters => EditorGUI.s_AllowedCharactersForInt;

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
            {
                double intDragSensitivity =
                    (double) NumericFieldDraggerUtility.CalculateIntDragSensitivity((long) startValue);
                float acceleration =
                    NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                long num = (long) this.StringToValue(this.text) + (long) Math.Round(
                    (double) NumericFieldDraggerUtility.NiceDelta((Vector2) delta, acceleration) * intDragSensitivity);
                if (this.parentIntegerField.isDelayed)
                    this.text = this.ValueToString(MathUtils.ClampToInt(num));
                else
                    this.parentIntegerField.value = MathUtils.ClampToInt(num);
            }

            protected override string ValueToString(int v) => v.ToString(this.formatString);

            protected override int StringToValue(string str)
            {
                long num;
                EditorGUI.StringToLong(str, out num);
                return MathUtils.ClampToInt(num);
            }
        }
    }
}