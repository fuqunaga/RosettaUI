using System.Linq;
using RosettaUI.Reactive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static VisualElement Build_Space(Element element)
        {
            var ve = new VisualElement();
            ve.AddToClassList(UssClassName.Space);
            return ve;
        }
        
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

        private VisualElement Build_PopupElement(Element element)
        {
            var contextMenuElement = (PopupMenuElement) element;

            var ve = new VisualElement();
            Build_ElementGroupContents(ve, contextMenuElement);

            ve.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    var menuItems = contextMenuElement.CreateMenuItems?.Invoke();
                    if (menuItems != null)
                    {
                        var menu = new GenericDropdownMenu();

                        foreach (var item in menuItems)
                        {
                            if (item.isEnable) menu.AddItem(item.name, item.isChecked, item.action);
                            else menu.AddDisabledItem(item.name, item.isChecked);
                        }

                        menu.DropDown(new Rect(){position = evt.mousePosition}, ve);
                        evt.StopPropagation();
                    }
                }
            });


            return ve;
        }
    }
}