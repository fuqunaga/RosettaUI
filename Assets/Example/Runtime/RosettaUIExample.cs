using System;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class RosettaUIExample : MonoBehaviour
    {
        private static readonly Type[] ExampleTypes = {
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
        
#if ENABLE_INPUT_SYSTEM
        public Key toggleUIKey = Key.U;
#else
        public KeyCode toggleUIKey = KeyCode.U;
#endif
        private RosettaUIRoot _root;
        
        private void Start()
        {
            _root = GetComponent<RosettaUIRoot>();
            _root.Build(CreateElement());
        }
        
        private static Element CreateElement()
        {
            return UI.Window(
                ExampleTypes.Select(type => UI.WindowLauncher(null, false, false, false, type))
            ).SetClosable(false);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if ( Keyboard.current[toggleUIKey].wasPressedThisFrame)
#else
            if (Input.GetKeyDown(toggleUIKey))
#endif
            {
                _root.enabled = !_root.enabled;
            }
        }
    }
}