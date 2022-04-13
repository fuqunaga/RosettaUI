using System;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class RosettaUIExample : MonoBehaviour
    {
        public static readonly Type[] ExampleTypes = {
            typeof(FieldExample),
            typeof(SliderExample),
            typeof(MinMaxSliderExample),
            typeof(ListExample),
            typeof(LayoutExample),
            typeof(WindowAndDynamicExample),
            typeof(MiscExample),
            typeof(ArgumentExample),
            typeof(MethodExample),
            typeof(CustomExample),
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