using UnityEngine;

namespace RosettaUI.Example
{
    public class BehaviourAnotherExample : MonoBehaviour, IElementCreator
    {
        public string stringValue;
        public float floatValue;
        
        public Element CreateElement(LabelElement _)
            => UI.Fold(nameof(BehaviourAnotherExample),
                UI.FieldReadOnly(() => name),
                UI.FieldReadOnly(() => isActiveAndEnabled),
                UI.Field(() => stringValue),
                UI.Field(() => floatValue)
            ).Open();
    }
}