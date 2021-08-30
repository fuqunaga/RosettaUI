using RosettaUI.Test;
using UnityEngine;


namespace RosettaUI.UIToolkit.Test
{
    [RequireComponent(typeof(RosettaUI_UIToolkitRoot))]
    public class RosettaUI_UIToolkitTest : RosettaUI_Test
    {
        RosettaUI_UIToolkitRoot root;

        protected override void BuildElement(Element rootElement)
        {
            if (root == null)
            {
                root = GetComponent<RosettaUI_UIToolkitRoot>();
            }

            root.Build(rootElement);
        }
    }
}