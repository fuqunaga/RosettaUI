using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [UxmlElement]
    public partial class ToggleButton : Button
    {
        public bool IsToggled { get; private set; }
        public event Action<bool> toggledStateChanged; 
        
        private const string ToggledClass = "rosettaui-toggle-button__toggled";
        
        public ToggleButton()
        {
            IsToggled = false;
            clicked += ToggleState;
        }
        
        public void SetValueWithoutNotify(bool value)
        {
            IsToggled = value;
            RemoveFromClassList(ToggledClass);
            if (IsToggled)
            {
                AddToClassList(ToggledClass);
            }
        }
        
        public void SetValue(bool value)
        {
            SetValueWithoutNotify(value);
            toggledStateChanged?.Invoke(IsToggled);
        }

        private void ToggleState()
        {
            SetValue(!IsToggled);
        }
    }
}