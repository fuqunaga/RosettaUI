using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
    public partial class ToggleButton : Button, INotifyValueChanged<bool>
    {
#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<ToggleButton, UxmlTraits>
        {
        }
#endif
        
        public bool value
        {
            get => _isToggled;
            set
            {
                SetValueWithoutNotify(value);
                toggledStateChanged?.Invoke(_isToggled);
            }
        }

        public event Action<bool> toggledStateChanged; 
        
        private bool _isToggled;
        private const string ToggledClass = "rosettaui-toggle-button__toggled";
        
        public ToggleButton()
        {
            _isToggled = false;
            clicked += ToggleState;
        }
        
        public void SetValueWithoutNotify(bool val)
        {
            _isToggled = val;
            RemoveFromClassList(ToggledClass);
            if (_isToggled)
            {
                AddToClassList(ToggledClass);
            }
        }

        private void ToggleState()
        {
           value = !value;
        }
    }
}