using System;
using UnityEngine.UIElements;


namespace RosettaUI.UIToolkit
{
    public class WindowTitleButton : Button
    {
        public new const string ussClassName = "rosettaui-window-title-button";
        public const string ussClassNameIcon = "rosettaui-window-title-button__icon";

        // UIToolkit can't scale 9slice, so separate a icon element
        public VisualElement IconElement { get; protected set; } = new VisualElement();

        public WindowTitleButton() : this(null) { }
        public WindowTitleButton(Action clickEvent) : base(clickEvent)
        {
            ClearClassList();
            AddToClassList(ussClassName);
            
            IconElement.AddToClassList(ussClassNameIcon);
            
            Add(IconElement);
        }
    }
}