using System.Linq;
using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static Button Build_Button(Element element)
        {
            var buttonElement = (ButtonElement) element;

            var button = new Button(buttonElement.OnClick);

            buttonElement.valueRx.SubscribeAndCallOnce(str => button.text = str);

            return button;
        }

        private static VisualElement Build_Dropdown(Element element)
        {
            var dropdownElement = (DropdownElement) element;
            var options = dropdownElement.options.ToList();

            var field = new PopupField<string>(
                options,
                dropdownElement.Value
            )
            {
                label = dropdownElement.label.Value
            };

            SetupLabelCallback(field.labelElement, dropdownElement.label);

            dropdownElement.valueRx.SubscribeAndCallOnce(v => field.index = v);
            field.RegisterValueChangedCallback(ev => dropdownElement.OnViewValueChanged(field.index));

            return field;
        }

    }
}