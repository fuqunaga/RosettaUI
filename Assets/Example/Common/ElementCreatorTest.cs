using UnityEngine;


namespace RosettaUI.Test
{
    public class ElementCreatorTest : MonoBehaviour, IElementCreator
    {
        public int intValue;

        public Element CreateElement()
        {
            return UI.Field(nameof(ElementCreatorTest) + nameof(intValue), () => intValue);
        }
    }
}