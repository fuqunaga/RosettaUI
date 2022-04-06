using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class MiscExample : MonoBehaviour, IElementCreator
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
            UICustom.RegisterElementCreationFunc<UICustomClass>((uiCustomClass) =>
            {
                return UI.Column(
                    UI.Label("UICustomized!"),
                    UI.Slider(() => uiCustomClass.floatValue)
                );
            });


            return UI.Column(
                UI.Row(
                    UI.Column(
                        ExampleTemplate.UIFunctionColumn(nameof(UI.TextArea),
                            UI.TextArea(nameof(UI.TextArea), () => multiLineStringValue),
                            UI.TextAreaReadOnly(nameof(UI.TextAreaReadOnly), () => multiLineStringValue)
                        ),
                        ExampleTemplate.UIFunctionColumn(nameof(UI.Dropdown),
                            UI.Dropdown(nameof(UI.Dropdown),
                                () => dropDownIndex,
                                options: new[] {"One", "Two", "Three"}
                            ),
                            UI.DropdownReadOnly($"{nameof(UI.DropdownReadOnly)}(selection index will not change)",
                                () => dropDownIndex,
                                options: new[] {"One", "Two", "Three"}
                            )
                        ),
                        ExampleTemplate.UIFunctionRow(nameof(UI.Image),
                            UI.Image(() => texture).SetMaxWidth(200f).SetMaxHeight(200f)
                        ),
                        ExampleTemplate.UIFunctionRow(nameof(UI.Button),
                            UI.Button(nameof(UI.Button), () => print("On button clicked"))
                        ),
              
                        ExampleTemplate.UIFunctionRow(nameof(UI.HelpBox),
                            UI.Column(
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.None)}", HelpBoxType.None),
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Info)}", HelpBoxType.Info),
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Warning)}",
                                    HelpBoxType.Warning),
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Error)}", HelpBoxType.Error)
                            )
                        ),
                        ExampleTemplate.UIFunctionRow(nameof(UI.Space),
                            UI.Space().SetBackgroundColor(Color.gray)
                        ),
                        ExampleTemplate.UIFunctionRow(nameof(UI.Popup),
                            UI.Popup(
                                UI.Box(UI.Label($"{nameof(UI.Popup)}(Right click)")),
                                () => new[]
                                {
                                    new MenuItem("Menu0", () => Debug.Log("Menu0")),
                                    new MenuItem("Menu1", () => Debug.Log("Menu1")),
                                    new MenuItem("Menu2", () => Debug.Log("Menu2"))
                                }
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