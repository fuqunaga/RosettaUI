using UnityEngine;

namespace RosettaUI.Example
{
    public class BehaviourExample : MonoBehaviour, IElementCreator
    {
        public Element CreateElement(LabelElement _)
            => UI.Column(
                UI.FieldReadOnly(() => name),
                UI.FieldReadOnly(() => isActiveAndEnabled)
            );
    }
}