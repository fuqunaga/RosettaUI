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
        public VisualElement InputField { get; }

        public bool ShowInputField
        {
            get => InputField.style.display == DisplayStyle.Flex;
            set => InputField.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public SliderInField()
        {
            AddToClassList(UssClassName);
            InputField = this.Q<TextValueInput>();
            
            Slider = new Slider();
            
            // 矢印キーでスライダーを動かしたときにフォーカスも左右のエレメントに移動するのを抑制
            Slider.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if (evt.direction is NavigationMoveEvent.Direction.Left or NavigationMoveEvent.Direction.Right)
                {
                    evt.StopPropagationAndFocusControllerIgnoreEvent();
                }
            });
            
            Insert(0, Slider);
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Slider.SetValueWithoutNotify(newValue);
        }
    }
}