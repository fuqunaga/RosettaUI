using RosettaUI.Example;
using UnityEngine;

namespace RosettaUI.UGUI.Test
{
    public class RosettaUI_UGUITest : RosettaUIExample
    {
        [SerializeField]
        public RosettaUI_UGUIRoot root;

        protected override void BuildElement(Element rootElement)
        {
            root.Build(rootElement);
        }
    }
}