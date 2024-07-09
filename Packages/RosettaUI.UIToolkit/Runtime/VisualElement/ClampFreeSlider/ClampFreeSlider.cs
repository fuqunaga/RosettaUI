using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// TextFieldにSliderを内包したもの
    ///
    /// 
    /// SliderもshowInputFieldでTextFieldを表示できるが値がクランプされる
    /// 範囲外の値がセットされた場合でもクランプせずに正確な値を表示したい場合に使用する
    ///
    /// SliderにTextFieldを入れる作りにはしない？
    /// - this.value = newValue でクランプされてしまう
    ///
    /// 親VisualElementを用意してSliderとTextFieldを並列に並べるのは？
    /// - ラベルのドラッグで値が変わって欲しい
    /// - SliderもTextFieldもフォーカスが合ったときラベルの色を変えたい
    /// </summary>
    public class ClampFreeSlider : FloatField, IClampFreeSlider<float>
    {
        public BaseSlider<float> Slider { get; }
        public VisualElement InputField { get; }
        
        public ClampFreeSlider()
        {
            InputField = this.Q<TextValueInput>();
            Slider = new Slider();
            
            this.Initialize<ClampFreeSlider, float>();
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Slider.SetValueWithoutNotify(newValue);
        }
    }
}