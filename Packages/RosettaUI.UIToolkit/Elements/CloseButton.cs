using System;
using UnityEngine.UIElements;


namespace RosettaUI.UIToolkit
{
    public class CloseButton : Button
    {
        public static new readonly string ussClassName = "rosettaui-closebutton";

        public CloseButton() : this(null) { }
        public CloseButton(Action clickEvent) : base(clickEvent)
        {
            AddToClassList(ussClassName);
            text = "x";
        }
    }
}