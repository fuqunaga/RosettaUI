using RosettaUI.Test;
using UnityEngine;


namespace RosettaUI.UIToolkit.Test
{
    [RequireComponent(typeof(RosettaUI_UIToolkitRoot))]
    public class RosettaUI_UIToolkitTest : RosettaUI_Test
    {
        RosettaUI_UIToolkitRoot _root;

        protected override void BuildElement(Element rootElement)
        {
            if (_root == null)
            {
                _root = GetComponent<RosettaUI_UIToolkitRoot>();
            }

            _root.Build(rootElement);
        }
    }
}