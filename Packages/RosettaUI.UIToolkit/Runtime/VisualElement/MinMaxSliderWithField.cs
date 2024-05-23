using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class MinMaxSliderWithField<TValue, TField> : MinMaxSlider
        where TField : TextValueField<TValue>, new()
    {
        private const string UssClassName = "rosettaui-min-max-slider";

        #region ToTValue/TFloat
        
        public static readonly Func<float, TValue> ToTValue;
        public static readonly Func<TValue, float> ToFloat;
        
        static MinMaxSliderWithField()
        {
            // avoid boxing cast trick
            // https://www.appsloveworld.com/csharp/100/467/primitive-type-conversion-in-generic-method-without-boxing
            var type = typeof(TValue);
            if (type == typeof(float))
            {
                ToTValue = (Func<float, TValue>)(object)(Func<float, float>)FloatToFloat;
                ToFloat = (Func<TValue, float>)(object)(Func<float, float>)FloatToFloat;
            }
            else if (type == typeof(int))
            {
                ToTValue = (Func<float, TValue>)(object)(Func<float, int>)FloatToInt;
                ToFloat = (Func<TValue, float>)(object)(Func<int, float>)IntToFloat;
            }

            static float FloatToFloat(float f) => f;
            static int FloatToInt(float f) => (int)f;
            static float IntToFloat(int i) => i;
        }
        
        #endregion


        private bool _showInputField = true;
        private readonly TField _minField;
        private readonly TField _maxField;
        
        public TField MinField => _minField;
        public TField MaxField => _maxField;

        public bool ShowInputField
        {
            get => _showInputField;
            set
            {
                if (_showInputField == value) return;
                
                _showInputField = value;
                if (_showInputField)
                {
                    _minField.style.display = DisplayStyle.Flex;
                    _maxField.style.display = DisplayStyle.Flex;
                    
                    SetMinMaxToField(this.value);
                }
                else
                {
                    _minField.style.display = DisplayStyle.None;
                    _maxField.style.display = DisplayStyle.None;
                }
            }
        }
        
        public MinMaxSliderWithField()
        {
            AddToClassList(UssClassName);

            _minField = new();
            _maxField = new();

            Add(_minField);
            Add(_maxField);
            
            _minField.RegisterValueChangedCallback(evt => minValue = ToFloat(evt.newValue));
            _maxField.RegisterValueChangedCallback(evt => maxValue = ToFloat(evt.newValue));
        }

        
        // MinMaxSliderWithField.valueに値が代入されたときに_minField,_maxFieldにも値を代入する
        // BaseField<T>.valueのsetはパネルにアタッチされていないとイベントを起こさないので、
        // this.RegisterValueChangeCallback()ではダメ
        // MinMaxSliderのコンストラクタからも呼ばれるので注意
        public override void SetValueWithoutNotify(Vector2 newValue)
        {
            base.SetValueWithoutNotify(newValue);
            SetMinMaxToField(newValue);
        }

        private void SetMinMaxToField(Vector2 minMax)
        {
            _minField?.SetValueWithoutNotifyIfNotEqual(ToTValue(minMax.x));
            _maxField?.SetValueWithoutNotifyIfNotEqual(ToTValue(minMax.y));
        }
    }
}