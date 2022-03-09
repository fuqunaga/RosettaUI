using System;
using System.Globalization;
using UnityEngine;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class UIntField : TextValueField<uint>
    {
        /*
        public new static readonly string ussClassName = "unity-integer-field";
        public new static readonly string labelUssClassName = IntegerField.ussClassName + "__label";
        public new static readonly string inputUssClassName = IntegerField.ussClassName + "__input";
        */

        private UIntInput uintInput => (UIntInput) this.textInputBase;

        protected override string ValueToString(uint v) =>
            v.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);

        protected override uint StringToValue(string str)
        {
            uint.TryParse(str, out var ret);
            return ret;
        }

        public UIntField() : this(null)
        {
        }

        public UIntField(int maxLength) : this(null, maxLength)
        {
        }

        public UIntField(string label, int maxLength = -1) : base(label, maxLength, new UIntInput())
        {
            /*
             AddToClassList(IntegerField.ussClassName);
            labelElement.AddToClassList(IntegerField.labelUssClassName);
            visualInput.AddToClassList(IntegerField.inputUssClassName);
            */
            AddLabelDragger<uint>();
        }

        internal override bool CanTryParse(string textString) => uint.TryParse(textString, out uint _);

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, uint startValue) =>
            uintInput.ApplyInputDeviceDelta(delta, speed, startValue);

        protected class UIntInput : TextValueInput
        {
            private UIntField parentIntegerField => (UIntField) this.parent;

            public UIntInput() => this.formatString = EditorGUI.kIntFieldFormatString;

            protected override string allowedCharacters => EditorGUI.s_AllowedCharactersForInt;

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, uint startValue)
            {
                double intDragSensitivity =
                    (double) NumericFieldDraggerUtility.CalculateIntDragSensitivity((long) startValue);
                float acceleration =
                    NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                long num = (long) this.StringToValue(this.text) + (long) Math.Round(
                    (double) NumericFieldDraggerUtility.NiceDelta((Vector2) delta, acceleration) * intDragSensitivity);

                var value = (uint)Math.Max(num, 0);

                if (this.parentIntegerField.isDelayed)
                    this.text = this.ValueToString(value);
                else
                    this.parentIntegerField.value = value;
            }

            protected override string ValueToString(uint v) => v.ToString(formatString);

            protected override uint StringToValue(string str)
            {
                uint.TryParse(str, out var ret);
                return ret;
            }
        }
    }
}