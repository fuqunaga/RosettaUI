namespace RosettaUI
{
    public abstract class SliderElement<T> : SliderBaseElement<T, T>
    {
        protected SliderElement(LabelElement label, IBinder<T> binder, in SliderElementOption<T> option) 
            : base(label, binder, option)
        {
        }
    }
    
    public abstract class SliderBaseElement<T, TRange> : RangeFieldElement<T, TRange>
    {
        public readonly bool showInputField;

        protected SliderBaseElement(LabelElement label, IBinder<T> binder, in SliderElementOption<TRange> option) 
            : base(label, binder, option.minGetter, option.maxGetter, option.FieldOption)
        {
            showInputField = option.ShowInputField;
        }
    }


    public readonly struct SliderElementOption
    {
        public readonly IGetter minGetter;
        public readonly IGetter maxGetter;
        public readonly SliderOption? sliderOption;
        
        public SliderElementOption(IGetter minGetter, IGetter maxGetter, in SliderOption? sliderOption)
        {
            this.minGetter = minGetter;
            this.maxGetter = maxGetter;
            this.sliderOption = sliderOption;
        }

        public SliderElementOption(in SliderElementOption baseOption, IGetter newMinGetter, IGetter newMaxGetter)
        {
            minGetter = newMinGetter;
            maxGetter = newMaxGetter;
            sliderOption = baseOption.sliderOption;
        }

        public SliderElementOption<T> Cast<T>()
        {
            return new SliderElementOption<T>
            (
                (IGetter<T>) minGetter,
                (IGetter<T>) maxGetter,
                sliderOption
            );
        }
    }

    public readonly struct SliderElementOption<T>
    {
        public readonly IGetter<T> minGetter;
        public readonly IGetter<T> maxGetter;
        public readonly SliderOption? sliderOption;
        
        public bool ShowInputField => sliderOption?.showInputField ?? true;
        public FieldOption FieldOption => sliderOption?.fieldOption ?? FieldOption.Default;
        
        public SliderElementOption(IGetter<T> minGetter, IGetter<T> maxGetter, SliderOption? sliderOption)
        {
            this.minGetter = minGetter;
            this.maxGetter = maxGetter;
            this.sliderOption = sliderOption;
        }
    }

    public static class SliderElementOptionExtension
    {
        public static SliderElementOption<T> SetMinMaxGetterIfNotExist<T>(this SliderElementOption<T> elementOption, IGetter<T> minGetter, IGetter<T> maxGetter)
        {
            return new SliderElementOption<T>(
                elementOption.minGetter ?? minGetter,
                elementOption.maxGetter ?? maxGetter,
                elementOption.sliderOption
            );
        }
    }
}