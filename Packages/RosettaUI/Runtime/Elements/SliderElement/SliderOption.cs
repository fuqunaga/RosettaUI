using System;

namespace RosettaUI
{
    [Serializable]
    public struct SliderOption
    {
        public static SliderOption Default => new();
        
        public bool showInputField;
        public FieldOption? fieldOption;
        
        public SliderOption(bool showInputField = true, FieldOption? fieldOption = null)
        {
            this.showInputField = showInputField;
            this.fieldOption = fieldOption;
        }
    }
}