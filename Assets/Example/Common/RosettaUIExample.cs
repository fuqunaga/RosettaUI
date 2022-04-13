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
        
        public KeyCode toggleUIKey = KeyCode.U;
        private RosettaUIRoot _root;

        private void Start()
        {
            _root = GetComponent<RosettaUIRoot>();
            _root.Build(CreateElement());
        }

        private Element CreateElement()
        {
            return UI.Window(
                ExampleTypes.Select(type => UI.WindowLauncher(type))
            );
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleUIKey))
            {
                _root.enabled = !_root.enabled;
            }
        }
    }
}