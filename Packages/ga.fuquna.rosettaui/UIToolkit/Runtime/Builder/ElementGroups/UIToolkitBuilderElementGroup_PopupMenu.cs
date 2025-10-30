using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_PopupMenu(Element element, VisualElement visualElement)
        {
            if (element is not PopupMenuElement popupMenuElement || visualElement is not PopupMenu popupMenu) return false;

            popupMenu.CreateMenuItems = popupMenuElement.CreateMenuItems;
            popupMenu.MouseButton = popupMenuElement.MouseButton;
            
            popupMenuElement.GetViewBridge().onUnsubscribe += () => popupMenu.CreateMenuItems = null;
            
            return Bind_ElementGroupContents(popupMenuElement, popupMenu);
        }
    }
}