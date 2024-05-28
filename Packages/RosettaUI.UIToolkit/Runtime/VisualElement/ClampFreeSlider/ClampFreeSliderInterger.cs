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
    public class ClampFreeSliderInteger : IntegerField, IClampFreeSlider<int>
    {
        public BaseSlider<int> Slider { get; }
        public VisualElement InputField { get; }
        
        public ClampFreeSliderInteger()
        {
            InputField = this.Q<TextValueInput>();
            Slider = new SliderInt();
            
            this.Initialize<ClampFreeSliderInteger, int>();
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Slider.SetValueWithoutNotify(newValue);
        }
    }
}