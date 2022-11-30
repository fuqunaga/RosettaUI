using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_HelpBox(Element element)
        {
            var helpBoxElement = (HelpBoxElement) element;
        
            var helpBox = new HelpBox(null, GetHelpBoxMessageType(helpBoxElement.helpBoxType));
            helpBoxElement.label.GetViewBridge().SubscribeValueOnUpdateCallOnce(str => helpBox.text = str);

            return helpBox;

            static HelpBoxMessageType GetHelpBoxMessageType(HelpBoxType helpBoxType)
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