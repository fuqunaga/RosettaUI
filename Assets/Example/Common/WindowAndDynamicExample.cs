using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class WindowAndDynamicExample : MonoBehaviour, IElementCreator
    {
        public int intValue;
        public bool dynamicElementIf;

        
        public Element CreateElement()
        {
            return UI.Column(
                ExampleTemplate.CodeElementSets(ExampleTemplate.UIFunctionStr(nameof(UI.WindowLauncher)),
                    (@"UI.WindowLauncher(
    UI.Window(
        UI.Label(""Element"")
    )
);
",
                        UI.WindowLauncher(
                            UI.Window(
                                UI.Label("Window")
                            )
                        )
                    ),
                    ("UI.WindowLauncher<BehaviourExample>();\n", UI.WindowLauncher<BehaviourExample>()),
                    (@"UI.WindowLauncher<BehaviourExample>(
    supportMultiple: true, 
    includeInactive: true
);", 
                        UI.WindowLauncher<BehaviourExample>(supportMultiple: true, includeInactive: true))
                ),
                ExampleTemplate.CodeElementSets(ExampleTemplate.UIFunctionStr(nameof(UI.FieldIfObjectFound)),
                    (@"UI.Box(
    UI.FieldIfObjectFound<BehaviourExample>()
);
",
                        UI.Box(UI.FieldIfObjectFound<BehaviourExample>())
                    ),
                    (@"UI.Box(
    UI.FieldIfObjectFound<BehaviourExample>(
        supportMultiple: true, 
        includeInactive: true
    )
);",
                        UI.Box(UI.FieldIfObjectFound<BehaviourExample>(supportMultiple: true, includeInactive: true))
                    )
                ),
                ExampleTemplate.CodeElementSets(ExampleTemplate.UIFunctionStr(nameof(UI.DynamicElementIf)),
                    "The element appears when the condition is true.",
                    (@"UI.Field(() => dynamicElementIf);

UI.DynamicElementIf(
    condition: () => dynamicElementIf,
    build: () => UI.Label(nameof(UI.DynamicElementIf))
);",
                        UI.Column(
                            UI.Field(() => dynamicElementIf),
                            UI.DynamicElementIf(
                                condition: () => dynamicElementIf,
                                build: () => UI.Label(nameof(UI.DynamicElementIf))
                            )
                        )
                    )
                ),
                ExampleTemplate.CodeElementSets(ExampleTemplate.UIFunctionStr(nameof(UI.DynamicElementOnStatusChanged)),
                    (@"UI.Slider(""Button count"", () => intValue, max: 10);

UI.DynamicElementOnStatusChanged(
    readStatus: () => intValue,
    build: (status) => UI.Row(
        Enumerable.Range(0, intValue).Select(i => UI.Button(i.ToString()))
    )
);",
                        UI.Column(
                            UI.Slider("Button count", () => intValue, max: 10),
                            UI.DynamicElementOnStatusChanged(
                                readStatus: () => intValue,
                                build: (status) => UI.Row(
                                    Enumerable.Range(0, intValue).Select(i => UI.Button(i.ToString()))
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}