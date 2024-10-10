using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // Dropdown()の第3引数を無視するGenericDropdownMenu
    // BasePopupFieldがDropdownMenuでanchor == trueで呼び出しているが、
    // falseにしたいために作成
    // IGenericMenuがinternalなのが辛い
    public class GenericDropdownMenuIgnoreAnchored : GenericDropdownMenu, IGenericMenu
    {
        void IGenericMenu.DropDown(Rect position, VisualElement targetElement, bool anchored)
            => base.DropDown(position, targetElement, false);
    }
}