using UnityEngine;


namespace RosettaUI.Example
{
    public class ElementCreatorSimple : MonoBehaviour, IElementCreator
    {
        public int intValue;

        public Element CreateElement()
        {
            return UI.Column(
                UI.Label("This is " + nameof(ElementCreatorSimple)),
                UI.Field(() => intValue)
            );
        }
    }
}