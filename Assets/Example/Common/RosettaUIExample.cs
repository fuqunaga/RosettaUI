using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class RosettaUIExample : MonoBehaviour
    {
        public KeyCode toggleRootElementKey = KeyCode.U;
        private RosettaUIRoot _root;
        Element _rootElement;

        private void Start()
        {
            _root = GetComponent<RosettaUIRoot>();
            _root.Build(CreateElement());
        }

        private Element CreateElement()
        {
            _rootElement = UI.Window(
                UI.WindowLauncher<FieldExample>()
#if false
                , UI.WindowLauncher<SliderExample>()
                , UI.WindowLauncher<MinMaxSliderExample>()
                , UI.WindowLauncher<MiscExample>()
                , UI.WindowLauncher<UICustomExample>()
#endif
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