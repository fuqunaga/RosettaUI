using UnityEngine;

namespace RosettaUI.Test
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class DisableInputUI : MonoBehaviour
    {
        public string stringValue;
        public float floatValue;

        private void Start()
        {
            var root = GetComponent<RosettaUIRoot>();
            root.Build(CreateElement());
        }

        private Element CreateElement()
        {
            return UI.Window(nameof(stringValue),
                UI.Page(
                    UI.Field(() => stringValue),
                    UI.Slider(() => floatValue)
                )
            ).SetClosable(false);
        }
    }
}