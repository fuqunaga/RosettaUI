using System.Linq;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
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

            imageElement.GetViewBridge().SubscribeValueOnUpdateCallOnce(tex => ve.image = tex);
            
            return ve;
        }

        
        private static Button Build_Button(Element element)
        {
            var buttonElement = (ButtonElement) element;

            var button = new Button(buttonElement.OnClick);

            buttonElement.SubscribeValueOnUpdateCallOnce(button);
            // buttonElement.SubscribeValueOnUpdate(str => button.text = str);

            return button;
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