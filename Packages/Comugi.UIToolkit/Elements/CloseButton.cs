using System;
using UnityEngine.UIElements;


namespace Comugi.UIToolkit
{
    public class CloseButton : Button
    {
        public static new readonly string ussClassName = "comugi-closebutton";

        public CloseButton() : this(null) { }
        public CloseButton(Action clickEvent) : base(clickEvent)
        {
            AddToClassList(ussClassName);
            text = "x";
        }
    }
}