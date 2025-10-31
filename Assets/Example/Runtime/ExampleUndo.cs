using System.Collections.Generic;
using System.Linq;
using RosettaUI.UndoSystem;
using UnityEngine;

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class ExampleUndo : MonoBehaviour
    {
        private void Start()
        {
            var root = GetComponent<RosettaUIRoot>();
            root.Build(CreateElement());
        }

        private Element CreateElement()
        {
            return UI.Window(nameof(ExampleUndo),
                UI.Tabs(
                    BasicRules(),
                    Custom()
                )
            );
        }

        private (string, Element) BasicRules()
        {
            var stringValue = "some text";
            var floatValue = 0f;
            var colorValue = Color.cyan;

            var floatValueExist = true;
            var floatValueInDynamic = 0f;
            var floatValueInWindow = 0f;

            return ExampleTemplate.Tab(nameof(BasicRules),
                ExampleTemplate.TitleDescriptionElement("Undo/Redo",
                    "• Undo/Redo can be performed using keyboard shortcuts (Ctrl+Z / Ctrl+Y or Cmd+Z / Cmd+Shift+Z)\n" +
                    "• Shortcuts can be customized in the RosettaUIRoot component\n" +
                    "• Undo history can be viewed in the Undo History window(<b>Window > RosettaUI > Undo History</b>)",
                    UI.Field(() => stringValue),
                    UI.Slider(() => floatValue),
                    UI.Field(() => colorValue)
                ),
                ExampleTemplate.TitleDescriptionElement("Non-existent Element",
                    "Undo for non-existent Element will be skipped.\n" +
                    "After changing the following floatValue,\n\n" +
                    "if you delete the element with the Toggle Field Existence button and then press Ctrl+Z,\n" +
                    "the previous Undo will be executed.\n\n" +
                    "The same applies to Elements inside the Window.",
                    UI.Row(
                        UI.DynamicElementIf(
                            () => floatValueExist,
                            () => UI.Field(() => floatValueInDynamic)
                        ),
                        UI.Button("Toggle Field Existence", () => floatValueExist = !floatValueExist
                        )
                    ),
                    UI.WindowLauncher("Example Window",
                        UI.Window("Example Window",
                            UI.Page(
                                UI.Field(() => floatValueInWindow
                                )
                            )
                        )
                    )
                )
            );
        }

        private (string, Element) Custom()
        {
            return (
                ExampleTemplate.TabTitle(nameof(Custom)),
                UI.Column(
                    ExampleTemplate.BlankLine(),
                    UI.HelpBox("You can register custom Undo actions in your application.\n" +
                               "RosettaUI only undoes changes to UI library values,\n" +
                               "but adding your own Undo actions enables more natural behavior.",
                        HelpBoxType.Info),
                    ExampleTemplate.BlankLine(),
                    UI.Tabs(
                        UndoRecordValueChangeTab(),
                        UndoRecordActionTab()
                    )
                )
            );
        }

        private static (string, Element) UndoRecordValueChangeTab()
        {
            var intValue = 0;
            SyntaxHighlighter.AddPattern("method", nameof(AddIntValue));

            return ExampleTemplate.CodeElementSetsTab(
                ExampleTemplate.FunctionStr(nameof(Undo), nameof(Undo.RecordValueChange)),
                (@"UI.Row(
    UI.Field(() => intValue),
    UI.Button(""+"", () => AddIntValue(1)),
    UI.Button(""-"", () => AddIntValue(-1))
)

void AddIntValue(int delta)
{
    var before = intValue;
    intValue += delta;
    var after = intValue;

    Undo.RecordValueChange(
        nameof(AddIntValue),
        before,
        after,
        applyValue: v => intValue = v
    );
}",
                    UI.Row(
                        UI.Field(() => intValue),
                        UI.Button("+", () => AddIntValue(1)),
                        UI.Button("-", () => AddIntValue(-1))
                    )
                )
            );

            void AddIntValue(int delta)
            {
                var before = intValue;
                intValue += delta;
                var after = intValue;

                Undo.RecordValueChange(
                    nameof(AddIntValue),
                    before,
                    after,
                    applyValue: v => intValue = v
                );
            }
        }


        private static (string label, Element element) UndoRecordActionTab()
        {
            var objectName = "MyGameObject";
            var primitiveType = PrimitiveType.Cube;
            List<GameObject> createdObjects = new();

            return ExampleTemplate.CodeElementSetsTab(
                ExampleTemplate.FunctionStr(nameof(Undo), nameof(Undo.RecordAction)),
                (@"UI.Column(
    UI.Field(() => objectName),
    UI.Field(() => primitiveType),
    UI.Button(""Create GameObject"", () =>
        {
            var position = Vector3.right * createdObjects.Count * 2f;
            var createData = (objectName, primitiveType, position);
            var go = Create(createData);
            
            Undo.RecordCommon(
                ""Create GameObject"",
                undoAction: () => Remove(go),
                redoAction: () => Create(createData)
            );
        }
    )
)

GameObject Create((string, PrimitiveType, Vector3) createData)
{
    var (goName, type, position) = createData;
    var go = GameObject.CreatePrimitive(type);
    go.name = goName;
    go.transform.position = position;
    
    createdObjects.Add(go);
    return go;
}

void Remove(GameObject go)
{
    createdObjects.Remove(go);
    Destroy(go);
}",
                    UI.Column(
                        UI.Field(() => objectName),
                        UI.Field(() => primitiveType),
                        UI.Button("Create GameObject", () =>
                            {
                                var position = Vector3.right * createdObjects.Count * 2f;
                                var createData = (objectName, primitiveType, position);
                                var go = Create(createData);
                                
                                Undo.RecordAction(
                                    "Create GameObject",
                                    undoAction: () => Remove(go),
                                    redoAction: () => Create(createData)
                                );
                            }
                        )
                    )
                )
            );

            GameObject Create((string, PrimitiveType, Vector3) createData)
            {
                var (goName, type, position) = createData;
                var go = GameObject.CreatePrimitive(type);
                go.name = goName;
                go.transform.position = position;
                
                createdObjects.Add(go);
                return go;
            }

            void Remove(GameObject go)
            {
                createdObjects.Remove(go);
                Destroy(go);
            }
        }
    }
}