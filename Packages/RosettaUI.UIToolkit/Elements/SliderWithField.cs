using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class SliderWithField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SliderWithField, UxmlTraits>
        {
        }

        private static readonly string UssClassName = "rosettaui-colorpicker-slider";
            
        public Slider Slider { get; protected set; }

        public string Label
        {
            get => Slider.label;
            set => Slider.label = value;
        }

        public float Value
        {
            get => Slider.value;
            set => Slider.value = value;
        }

        public SliderWithField()
        {
            AddToClassList(UssClassName);

            Slider = new Slider();
            var field = new FloatField();

            Slider.highValue = 1f;
            
            Add(Slider);
            Add(field);

            Slider.RegisterValueChangedCallback((evt) => field.SetValueWithoutNotify(evt.newValue));
            field.RegisterValueChangedCallback((evt) => Slider.SetValueWithoutNotify(evt.newValue));
        }
    }
}