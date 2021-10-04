using System.Collections.Generic;
using System.Linq;
using UnityEngine.VFX;

namespace RosettaUI
{
    public class WindowElement : OpenCloseBaseElement
    {
        public WindowElement(LabelElement title, IEnumerable<Element> contents) : base(title, contents)
        {}
    }
}