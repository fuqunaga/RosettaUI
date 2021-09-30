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
        public FloatField Field { get; protected set; }

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
            Field = new FloatField();

            Slider.highValue = 1f;
            
            Add(Slider);
            Add(Field);

            Slider.RegisterValueChangedCallback((evt) => Field.SetValueWithoutNotify(evt.newValue));
            Field.RegisterValueChangedCallback((evt) => Slider.SetValueWithoutNotify(evt.newValue));
        }

        public void SetValueWithoutNotify(float newValue)
        {
            Slider.SetValueWithoutNotify(newValue);
            Field.SetValueWithoutNotify(newValue);
        }
    }
}