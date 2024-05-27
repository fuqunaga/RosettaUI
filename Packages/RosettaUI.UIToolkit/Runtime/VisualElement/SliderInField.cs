using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// TextFieldにSliderを内包したもの
    ///
    /// SliderもshowInputFieldでTextFieldを表示できるが値がClampされる
    /// 範囲外の値がセットされた場合でもクランプせずに正確な値を表示したい場合に使用する
    /// </summary>
    public class SliderInField : FloatField
    {
        private const string UssClassName = "rosettaui-slider-in-field";
        
        public Slider Slider { get; }
        
        public SliderInField()
        {
            AddToClassList(UssClassName);
            
            Slider = new Slider();
            Insert(0, Slider);
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Slider.SetValueWithoutNotify(newValue);
        }
    }
}