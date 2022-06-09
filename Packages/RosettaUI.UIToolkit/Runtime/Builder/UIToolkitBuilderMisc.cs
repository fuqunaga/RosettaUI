using System.Linq;
using RosettaUI.UIToolkit.UnityInternalAccess;
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
        
        private static VisualElement Build_Image(Element element)
        {
            var imageElement = (ImageElement) element;
            var ve = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    alignSelf = Align.FlexStart
                }
            };

            imageElement.SubscribeValueOnUpdateCallOnce(tex => ve.image = tex);
            
            return ve;
        }

        
        private static Button Build_Button(Element element)
        {
            var buttonElement = (ButtonElement) element;

            var button = new Button(buttonElement.OnClick);

            button.ListenValue(buttonElement);
            // buttonElement.SubscribeValueOnUpdate(str => button.text = str);

            return button;
        }

        private VisualElement Build_Dropdown(Element element)
        {
            var dropdownElement = (DropdownElement) element;
            var options = dropdownElement.options.ToList();

            var field = new PopupFieldCustomMenu<string>(
                options,
                dropdownElement.Value
            );
            field.onMenuCreated += menu => menu.AddBoxShadow(); 
      
            SetupFieldLabel(field, dropdownElement);

            field.Bind(dropdownElement,
                elementValueToFieldValue: i => (0 <= i && i < options.Count) ? options[i] : default,
                fieldValueToElementValue: str => options.IndexOf(str)
            );

            return field;
        }
        
        private VisualElement Build_PopupElement(Element element)
        {
            var contextMenuElement = (PopupMenuElement) element;

            var ve = new VisualElement();
            Build_ElementGroupContents(ve, contextMenuElement);

            ve.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    var menuItems = contextMenuElement.CreateMenuItems?.Invoke();
                    if (menuItems != null)
                    {

                        var menu = DropDownMenuGenerator.Generate(menuItems, new Rect() {position = evt.position}, ve);
                        if (menu is GenericDropdownMenu gdm)
                        {
                            gdm.AddBoxShadow();
                        }

                        evt.StopPropagation();
                        evt.PreventDefault();
                    }
                }
            });

            return ve;
        }
    }
}