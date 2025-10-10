using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RosettaUI.Example;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RosettaUI.Test
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    public class TestUndoRedo : MonoBehaviour
    {
        public RosettaUIRoot root;

        public float floatValue;
        public string stringValue = "Hello";
        public MyEnum enumValue;
        public Vector2 vector2Value;
        
        private SimpleClass[] _classArray = {
            new() {stringValue = "Item 0", floatValue = 0f},
            null,
            new() {stringValue = "Item 2", floatValue = 2f},
        };
        
        private List<SimpleClass> _classList = new()
        {
            new SimpleClass {stringValue = "Item 0", floatValue = 0f},
            null,
            new SimpleClass {stringValue = "Item 2", floatValue = 2f},
        };
        
        
        private WindowElement _window;
        
        
        private void Start()
        {
            root.Build(CreateElement());
        }

        private void Update()
        {
            if (Keyboard.current[Key.U].wasPressedThisFrame)
            {
                _window.SetOpenFlag(!_window.IsOpen);
            }
        }

        private Element CreateElement()
        {
            return _window = UI.Window(nameof(TestUndoRedo),
                UI.Page(
                    UI.HelpBox("Change values, then Undo/Redo with Ctrl+Z / Ctrl+Y."),
                    UI.Field(() => floatValue),
                    UI.Field(() => stringValue),
                    UI.Field(() => enumValue),
                    UI.Field(() => vector2Value),
                    UI.Space(),
                    
                    UI.HelpBox("Test class or array containing null elements."),
                    UI.Field(() => _classArray),
                    UI.Button("Add null element", () => _classArray = _classArray.Append(null).ToArray()),
                    UI.Field(() => _classList),
                    UI.Button("Add null element", () => _classList.Add(null)),
                    
                    UI.Slider(() => floatValue),
                    UI.Slider(() => vector2Value),
                    UI.Space(),
                    UI.MinMaxSlider(() => vector2Value)
                )
            );
        }
    }
}