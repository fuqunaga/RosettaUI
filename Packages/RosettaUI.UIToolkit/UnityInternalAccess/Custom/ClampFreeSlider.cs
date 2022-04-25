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
            
            
            // Label の width が変わってスライダーの長さが変わってもdraggerの位置が再計算されないケース対策
            //
            //  // これで dragger がはみ出る
            //  var root = GetComponent<RosettaUIRoot>();
            //  root.Build(UI.Slider(() => 1);
            // 
            // BaseSlider では dragElement にしか UpdateDragElementPosition() がコールバック登録されていないので
            // dragContainer にも登録したい・・・が、private で呼べないので SetValueWithoutNotify() で間接的に呼ぶ
            
            dragContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (evt.oldRect == evt.newRect) return;
                SetValueWithoutNotify(value);
            });
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
