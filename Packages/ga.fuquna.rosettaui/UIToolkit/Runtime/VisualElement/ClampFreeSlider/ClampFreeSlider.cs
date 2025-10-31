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
    /// - Slider.clampedを（internalだけど無理やり）falseにするとSliderのUI上のつまみが範囲外に行ってしまう
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
            
            // Slider上でNavigationMoveEventは無効化しデフォルトの挙動に任せる
            // 
            // FloatField（の祖先のTextInputBaseField<TValueType>）はHandleEventBubbleUp()で
            // NavigationMoveEventのときにevt.target != this.textInputBase.textElement だと
            // this.textInputBase.textElementをcurrentFocusとしてSwitchFocusOnEvent()を呼んでしまう
            // これはSliderにフォーカスがあるときにNavigationMoveEventが起こるとtextElementからのフォーカス移動扱いになり、
            // Shift+Tab時は再度SliderにTab時はtextElementを飛ばして次のエレメントにフォーカスが移ってしまう
            Slider.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                evt.StopPropagation();
            });
        }
    
        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Slider.SetValueWithoutNotify(newValue);
        }
    }
}