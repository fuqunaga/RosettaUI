using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class MinMaxSliderWithField<TValue, TField> : MinMaxSlider
        where TField : TextValueField<TValue>, new()
    {
        private const string UssClassName = "rosettaui-min-max-slider";

        #region ToTValue/ToFloat
        
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

            return;

            static float FloatToFloat(float f) => f;
            static int FloatToInt(float f) => (int)f;
            static float IntToFloat(int i) => i;
        }
        
        #endregion


        private bool _showInputField = true;

        public TField MinField { get; }
        public TField MaxField { get; }

        public bool ShowInputField
        {
            get => _showInputField;
            set
            {
                if (_showInputField == value) return;
                
                _showInputField = value;
                if (_showInputField)
                {
                    MinField.style.display = DisplayStyle.Flex;
                    MaxField.style.display = DisplayStyle.Flex;
                    
                    SetMinMaxToField(this.value);
                }
                else
                {
                    MinField.style.display = DisplayStyle.None;
                    MaxField.style.display = DisplayStyle.None;
                }
            }
        }
        
        public MinMaxSliderWithField()
        {
            AddToClassList(UssClassName);

#if !UNITY_6000_0_OR_NEWER
            // Unity6以前はdraggerと左右のminmaxDraggerThumbが重なっている
            // draggerの角が見えてしまうのでthumb同様に丸める
            var dragger = this.Q("unity-dragger");
            var draggerStyle = dragger.style;
            draggerStyle.borderTopRightRadius = 5f;
            draggerStyle.borderTopLeftRadius = 5f;
            draggerStyle.borderBottomRightRadius = 5f;
            draggerStyle.borderBottomLeftRadius = 5f;
#endif

            MinField = new TField();
            MaxField = new TField();

            Add(MinField);
            Add(MaxField);
            
            // MinMaxSliderのFocusInEventでスライダーがアクティブになってしまうのをFieldへのフォーカス時は防ぐ
            MinField.RegisterCallback<FocusInEvent>(evt => evt.StopPropagation());
            MaxField.RegisterCallback<FocusInEvent>(evt => evt.StopPropagation());
            
            // MinFieldからShift+Tabでスライダーへフォーカスする場合、
            // MinMaxSliderのイベント
            // - FocusInEventでスライダーがDragState.MinThumbになるが、そのあと
            // - BlurEventでスライダーがDragState.NoThumbになってしまう
            // 通常フォーカス時同様にDragState.MinThumbになってほしい
            // ややこしいのがMinFieldのBlurEvent内でthis.Focus()しても、上記同様MinMaxSliderがBlurイベントを受け取ってしまいやはりうまくいかない
            // 一度Blur()してからFocus()するようにしている
            // ReflectionでMinMaxSlider.SetNavigationState()を呼び出した方が副作用の恐れが少なくていいかもしれない
            var visualInput = this.Q(className: inputUssClassName);
            MinField.RegisterCallback<BlurEvent>(evt =>
            {
                if (evt.relatedTarget == visualInput)
                {
                    schedule.Execute(() =>
                    {
                        Blur();
                        Focus();
                    });
                }
            });
            
            MinField.RegisterValueChangedCallback(evt => minValue = ToFloat(evt.newValue));
            MaxField.RegisterValueChangedCallback(evt => maxValue = ToFloat(evt.newValue));
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
            MinField?.SetValueWithoutNotifyIfNotEqual(ToTValue(minMax.x));
            MaxField?.SetValueWithoutNotifyIfNotEqual(ToTValue(minMax.y));
        }
    }
}