using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class ClampFreeSlider : Slider
    {
        public ClampFreeSlider()
        {
            clamped = false;
            SliderPatchUtility.CreateTextInputFieldAndBlockSliderKeyDownEvent(this);
        }

        internal override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
            => Mathf.Clamp01(base.SliderNormalizeValue(currentValue, lowerValue, higherValue));
    }


    public static class SliderPatchUtility
    {
        /// <summary>
        /// BaseSlider.OnKeyDown()で左右キーなどの入力でスライダーが反応してしまう
        /// テキスト入力時はスライダーの反応を止めたいのでStopPropagation()するイベントをinputTextFieldに仕込んでおく
        /// </summary>
        public static void CreateTextInputFieldAndBlockSliderKeyDownEvent<T>(BaseSlider<T> slider) where T : IComparable<T>
        {
            slider.showInputField = true;
            var textField = slider.inputTextField;
            textField.RegisterCallback<KeyDownEvent>(e => e.StopPropagation());
        }
    }
}
