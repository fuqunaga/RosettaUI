using RosettaUI.Test;
using UnityEngine;

namespace RosettaUI.UGUI.Test
{
    public class RosettaUI_UGUITest : RosettaUI_Test
    {
        [SerializeField]
        public RosettaUI_UGUIRoot root;

        protected override void BuildElement(Element rootElement)
        {
            root.Build(rootElement);
        }
    }
}