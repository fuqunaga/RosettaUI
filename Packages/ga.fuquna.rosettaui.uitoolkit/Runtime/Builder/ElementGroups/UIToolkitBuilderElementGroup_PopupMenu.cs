using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_PopupMenu(Element element, VisualElement visualElement)
        {
            if (element is not PopupMenuElement popupMenuElement || visualElement is not PopupMenu popupMenu) return false;

            popupMenu.CreateMenuItems = popupMenuElement.CreateMenuItems;
            popupMenuElement.GetViewBridge().onUnsubscribe += () => popupMenu.CreateMenuItems = null;
            
            return Bind_ElementGroupContents(popupMenuElement, popupMenu);
        }
    }
}