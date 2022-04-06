using System;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class RosettaUIExample : MonoBehaviour
    {
        public static readonly Type[] ExampleTypes = new[]
        {
            typeof(FieldExample),
            typeof(SliderExample),
            typeof(MinMaxSliderExample),
            typeof(ListExample),
            typeof(ArgumentExample),
            typeof(LayoutExample),
            typeof(WindowAndDynamicExample),
            typeof(MiscExample),
            typeof(SafetyExample)
        };
        
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
                ExampleTypes.Select(type => UI.WindowLauncher(type))
#if false
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