using UnityEngine;

namespace RosettaUI.Example
{
    public class BehaviourAnotherExample : MonoBehaviour, IElementCreator
    {
        public Element CreateElement(LabelElement _)
            => UI.Column(
                UI.Label(nameof(BehaviourAnotherExample)),
                UI.FieldReadOnly(() => name),
                UI.FieldReadOnly(() => isActiveAndEnabled)
            );
    }
}