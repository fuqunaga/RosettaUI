using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RosettaUI.Example;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RosettaUI.Test
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    public class TestUndoRedo : MonoBehaviour
    {
        public class ListIncludeClass
        {
            public string stringValue = nameof(ListIncludeClass);
            public List<SimpleClass> classList = Enumerable.Range(0, 3).Select(i => new SimpleClass {stringValue = $"Item{i}"}).ToList();
        }
        
        public RosettaUIRoot root;

        public float floatValue;
        public string stringValue = "Hello";
        public MyEnum enumValue;
        public Vector2 vector2Value;
        public Color colorValue = Color.white;
        public Gradient gradientValue = new();
        public AnimationCurve animationCurveValue = new();
        
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


        private ListIncludeClass[] _arrayIncludeClassArray =
        {
            new() { stringValue = "Item 0" },
            null,
            new() { stringValue = "Item 2" },
        };
        
        private List<ListIncludeClass> _arrayIncludeClassList = new()
        {
            new() { stringValue = "Item 0" },
            null,
            new() { stringValue = "Item 2" },
        };
        
         
        
        private WindowElement _window;
        
        
        private void Start()
        {
            root.Build(CreateElement());
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current[Key.U].wasPressedThisFrame)
#else
            if (Input.GetKey(KeyCode.U))
#endif
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
                    UI.Fold("Modal Editors.",
                        UI.Field(() => colorValue),
                        UI.Field(() => gradientValue),
                        UI.Field(() => animationCurveValue)
                    ).Open(),
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
                    UI.Field(() => _arrayIncludeClassArray),
                    UI.Button("Add null element", () => _arrayIncludeClassArray = _arrayIncludeClassArray.Append(null).ToArray()),
                    UI.Field(() => _arrayIncludeClassList),
                    UI.Button("Add null element", () => _arrayIncludeClassList.Add(null))
                )
            );

            static Element Space() => UI.Space().SetHeight(20f);
        }
    }
}