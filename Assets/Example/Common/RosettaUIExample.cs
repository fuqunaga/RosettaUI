using UnityEngine;

namespace RosettaUI.Example
{
    public class RosettaUIExample : MonoBehaviour, IElementCreator
    {
        public KeyCode toggleRootElementKey = KeyCode.U;
        Element _rootElement;

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
            if (Input.GetKeyDown(toggleRootElementKey))
            {
                _rootElement.Enable = !_rootElement.Enable;
            }
        }
    }
}