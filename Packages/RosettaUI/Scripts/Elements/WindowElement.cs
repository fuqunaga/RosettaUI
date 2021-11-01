using System.Collections.Generic;
using System.Linq;
using UnityEngine.VFX;

namespace RosettaUI
{
    public class WindowElement : OpenCloseBaseElement
    {
        public WindowElement(Element bar, IEnumerable<Element> contents) : base(bar, contents)
        {}
    }
}