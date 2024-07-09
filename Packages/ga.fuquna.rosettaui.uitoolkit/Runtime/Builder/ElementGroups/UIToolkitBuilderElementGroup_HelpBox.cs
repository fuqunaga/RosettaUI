using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_HelpBox(Element element, VisualElement visualElement)
        {
            if (element is not HelpBoxElement helpBoxElement || visualElement is not HelpBox helpBox) return false;

            helpBox.messageType = ToHelpBoxMessageType(helpBoxElement.helpBoxType);
            Bind_ExistingLabel(helpBoxElement.label, null, str => helpBox.text = str);

            return true;
            
            static HelpBoxMessageType ToHelpBoxMessageType(HelpBoxType helpBoxType)
            {
                return helpBoxType switch
                {
                    HelpBoxType.None => HelpBoxMessageType.None,
                    HelpBoxType.Info => HelpBoxMessageType.Info,
                    HelpBoxType.Warning => HelpBoxMessageType.Warning,
                    HelpBoxType.Error => HelpBoxMessageType.Error,
                    _ => throw new ArgumentOutOfRangeException(nameof(helpBoxType), helpBoxType, null)
                };
            }
        }
    }
}