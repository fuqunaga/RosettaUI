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
        
        
        private List<int> _intList = new() {1};
        
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
            SimpleClass simpleClass = new();
            var floatValueNo = 0;
            var floatValue0 = 0f;
            var floatValue1 = 1f;
            var intValue = 0;
            var intArray = Enumerable.Range(0, 100).ToArray();
            
            return _window = UI.Window(nameof(TestUndoRedo),
                UI.Page(
                    UI.Fold("Change values, then Undo/Redo with Ctrl+Z / Ctrl+Y.",
                        UI.Field(() => floatValue),
                        UI.Field(() => stringValue),
                        UI.Field(() => enumValue),
                        UI.Field(() => vector2Value)
                    ),
                    UI.Fold("Removed Element Undo test",
                        UI.HelpBox("Expired if the Element to be Undo is deleted."),
                        UI.Field(() => simpleClass),
                        UI.Button("Toggle simpleClass null",
                            () => simpleClass = simpleClass == null ? new SimpleClass() : null),
                        Space(),
                        UI.HelpBox(
                            "Undo is possible if there is an Element of the same type at the same position in the hierarchical structure\neven if the Undo target has been deleted."),
                        UI.DynamicElementOnStatusChanged(
                            () => floatValueNo,
                            number => number switch
                            {
                                0 => UI.Field(() => floatValue0),
                                1 => UI.Field(() => floatValue1),
                                2 => UI.Field(() => intValue),
                                _ => UI.Label("No field")
                            }
                        ),
                        UI.Row(
                            UI.FieldReadOnly(() => floatValueNo),
                            UI.Button("Cycle field", () => floatValueNo = (floatValueNo + 1) % 4)
                        )
                    ),
                    UI.HelpBox("Test class or array containing null elements."),
                    UI.Field(() => _classArray),
                    UI.Button("Add null element", () => _classArray = _classArray.Append(null).ToArray()),
                    UI.Field(() => _classList),
                    UI.Button("Add null element", () => _classList.Add(null)),
                    UI.Slider(() => floatValue),
                    UI.Slider(() => vector2Value),
                    Space(),
                    UI.MinMaxSlider(() => vector2Value),
                    Space(),
                    UI.HelpBox("リストの要素Elementは動的に子供のIndexが変わるためElementHierarchyPathで特殊処理が必要"),
                    UI.Field(() => intArray).SetHeight(500f)
                )
            );

            static Element Space() => UI.Space().SetHeight(20f);
        }
    }
}