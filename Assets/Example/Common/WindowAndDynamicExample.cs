using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class WindowAndDynamicExample : MonoBehaviour, IElementCreator
    {
        public struct FindObjectOption
        {
            public bool supportMultiple;
            public bool includeInactive;
        }
        
        
        [Multiline] public string multiLineStringValue = "this is\nmultiline\nstring";
        public Texture texture;
        public int dropDownIndex;
        public FindObjectOption windowLauncherOptions;
        public FindObjectOption fieldIfObjectFoundOptions;

        
        
        public List<int> listValue = new[] {1, 2, 3}.ToList();
        public int[] arrayValue = new[] {1, 2, 3};

        public float floatValue;
        public int intValue;
        public bool boolValue;

        public UICustomClass uiCustomClass;

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
                ),

                /*,
                UI.Box(
                    UI.Slider("DynamicElementOnTrigger if > 0.5f", () => floatValue),
                    UI.DynamicElementOnTrigger(
                        build: () => UI.Label("> 0.5f"),
                        rebuildIf: (e) => floatValue > 0.5f
                    )
                )
                */
                UI.Fold("ChildValueChangedCallback",
                    UI.Field(() => intValue),
                    UI.Field(() => floatValue)
                ).RegisterValueChangeCallback(() => Debug.Log($"OnChildValueChanged")),
                UI.Fold("Methods",
                    UI.Label(nameof(ElementExtensionsMethodChain.SetEnable)).SetEnable(false), // disappear,
                    UI.Field(nameof(ElementExtensionsMethodChain.SetInteractable), () => floatValue)
                        .SetInteractable(false),
                    UI.Label(nameof(ElementExtensionsMethodChain.SetColor)).SetColor(Color.red),
#if true
                    UI.Button($"{nameof(ElementExtensionsMethodChain.SetWidth)}(300f)").SetWidth(300f),
                    UI.Button($"{nameof(ElementExtensionsMethodChain.SetHeight)}(50f)").SetHeight(50f),
#else
                    UI.Button($"{nameof(ElementExtensionsMethodChain.SetWidth)}(100f)", null).SetWidth(100f),
                    UI.Button($"{nameof(ElementExtensionsMethodChain.SetMinWidth)}(200f)", null).SetMinWidth(50f),
                    UI.Button($"{nameof(ElementExtensionsMethodChain.SetMaxWidth)}(200f)", null).SetMaxWidth(200f),
                    UI.Column(
                        UI.Button($"{nameof(ElementExtensionsMethodChain.SetHeight)}(50f)", null).SetHeight(50f),
                        UI.Button($"{nameof(ElementExtensionsMethodChain.SetMinHeight)}(50f)", null).SetMinHeight(50f),
                        UI.Button($"{nameof(ElementExtensionsMethodChain.SetMaxHeight)}(50f)", null).SetMaxHeight(50f)
                    ).SetHeight(200f),
#endif
                    UI.Fold(nameof(ElementExtensionsMethodChain.Open), UI.Label("Open")).Open(),
                    UI.Fold(nameof(ElementExtensionsMethodChain.Close), UI.Label("Close")).Close()
                ),
                UI.Fold("UI Customize",
                    UI.Field(() => uiCustomClass)
                    // Element Creator in class/Array
                )
            );
        }
    }
}