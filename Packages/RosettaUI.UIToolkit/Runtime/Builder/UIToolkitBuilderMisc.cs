using System.Linq;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
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