using UnityEngine;

namespace RosettaUI.Example
{
    public class BehaviourExample : MonoBehaviour, IElementCreator
    {
        public string stringValue;
        
        public Element CreateElement(LabelElement _)
            => UI.Fold(nameof(BehaviourExample),
                UI.FieldReadOnly(() => name),
                UI.FieldReadOnly(() => isActiveAndEnabled),
                UI.Field(() => stringValue)
            ).Open();
    }
}