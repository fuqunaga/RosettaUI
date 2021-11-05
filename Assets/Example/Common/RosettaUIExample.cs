using UnityEngine;

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class RosettaUIExample : MonoBehaviour, IElementCreator
    {
        public KeyCode toggleRootElementKey = KeyCode.U;
        private RosettaUIRoot _root;
        Element _rootElement;

        private void Start()
        {
            _root = GetComponent<RosettaUIRoot>();
            _root.Build(CreateElement());
        }

        public Element CreateElement()
        {
            _rootElement = UI.Window(
                UI.ElementCreatorWindowLauncher<FieldExample>()
                , UI.ElementCreatorWindowLauncher<SliderExample>()
                , UI.ElementCreatorWindowLauncher<MinMaxSliderExample>()
                , UI.ElementCreatorWindowLauncher<MiscExample>()
            );

            return _rootElement;
        }

        void Update()
        {
            if (!RosettaUIRoot.WillUseKeyInputAny() && Input.GetKeyDown(toggleRootElementKey))
            {
                _root.enabled = !_root.enabled;
            }
        }
    }
}