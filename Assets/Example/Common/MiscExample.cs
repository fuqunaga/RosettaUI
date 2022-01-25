using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.Example
{
    public class MiscExample : MonoBehaviour, IElementCreator
    {
        public int dropDownIndex;
        public List<int> listValue = new[] {1, 2, 3}.ToList();
        public int[] arrayValue = new[] {1, 2, 3};

        public float floatValue;
        public int intValue;
        public bool boolValue;

        public UICustomClass uiCustomClass;

        public bool dynamicElementIf;

        public Texture texture;

        [Multiline] public string multiLineStringValue = "this is\nmultiline\nstring";


        public Element CreateElement()
        {
            string nullString = null;
            List<float> nullList = null;
            SimpleClass nullOneLineClass = null;
            ComplexClass nullMultiLineClass = null;
            ElementCreator nullElementCreator = null;
            var scrollViewItemCount = 10;


            UICustom.RegisterElementCreationFunc<UICustomClass>((uiCustomClass) =>
            {
                return UI.Column(
                    UI.Label("UICustomized!"),
                    UI.Slider(() => uiCustomClass.floatValue)
                );
            });


            return UI.Page(
                UI.Row(
                    UI.Page(
                        UI.Label("<b>UI.TextArea()</b>"),
                        UI.TextArea(nameof(UI.TextArea), () => multiLineStringValue),
                        UI.TextAreaReadOnly(nameof(UI.TextAreaReadOnly), () => multiLineStringValue),
                        UI.Space().SetHeight(10f),
                        UI.Row(
                            UI.Label("<b>UI.Image()</b>", true),
                            UI.Image(() => texture).SetMaxWidth(200f).SetMaxHeight(200f)
                        ),
                        UI.Space().SetHeight(10f),
                        UI.Row(
                            UI.Label("<b>UI.Button()</b>", true),
                            UI.Button(nameof(UI.Button), () => print("On button clicked"))
                        ),
                        UI.Space().SetHeight(10f),
                        UI.Label("<b>UI.Dropdown()</b>"),
                        UI.Dropdown(nameof(UI.Dropdown),
                            () => dropDownIndex,
                            options: new[] {"One", "Two", "Three"}
                        ),
                        UI.DropdownReadOnly($"{nameof(UI.DropdownReadOnly)}(selection index will not change)",
                            () => dropDownIndex,
                            options: new[] {"One", "Two", "Three"}
                        ),
                        UI.Space().SetHeight(10f),
                        UI.Row(
                            UI.Label("<b>UI.Popup()</b>", true),
                            UI.Popup(
                                UI.Box(UI.Label($"{nameof(UI.Popup)}(Right click)")),
                                () => new[]
                                {
                                    new MenuItem("Menu0", () => Debug.Log("Menu0")),
                                    new MenuItem("Menu1", () => Debug.Log("Menu1")),
                                    new MenuItem("Menu2", () => Debug.Log("Menu2"))
                                }
                            )
                        ),
                        UI.Space().SetHeight(10f),
                        UI.Row(
                            UI.Label("<b>UI.HelpBox()</b>", true),
                            UI.Column(
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.None)}", HelpBoxType.None),
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Info)}", HelpBoxType.Info),
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Warning)}",
                                    HelpBoxType.Warning),
                                UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Error)}", HelpBoxType.Error)
                            )
                        ),
                        UI.Space().SetHeight(10f),
                        UI.Row(
                            UI.Label("<b>UI.Space()</b>", true),
                            UI.Space().SetBackgroundColor(Color.gray)
                        ),
                        UI.Space().SetHeight(10f)
                    ),
                    UI.Page(
                        UI.Label("<b>UI.WindowLauncher()</b>"),
                        UI.WindowLauncher(
                            UI.Window(
                                UI.Label("This is window.")
                            )
                        ),
                        UI.WindowLauncher<BehaviourExample>(),
                        UI.Space().SetHeight(10f),
                        UI.Label("<b>UI.FieldIfObjectFound</b>"),
                        UI.Box(
                            UI.FieldIfObjectFound<BehaviourExample>()
                        ),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.DynamicElementIf()</b>"),
                        UI.Field(() => dynamicElementIf),
                        UI.DynamicElementIf(
                            () => dynamicElementIf,
                            () => UI.Label(nameof(UI.DynamicElementIf))
                        ),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.DynamicElementOnStatusChanged()</b>"),
                        UI.Slider("Button count", () => intValue, max: 10),
                        UI.DynamicElementOnStatusChanged(
                            readStatus: () => intValue,
                            build: (status) =>
                            {
                                var buttons = Enumerable.Range(0, intValue)
                                    .Select(i => UI.Button(i.ToString()));
                                var label = UI.Label(nameof(UI.DynamicElementOnStatusChanged));
                                return UI.Row(
                                    new Element[] {label}.Concat(buttons)
                                );
                            })
                    )
                ),
                UI.Space().SetHeight(10f),
                /*,
                UI.Box(
                    UI.Slider("DynamicElementOnTrigger if > 0.5f", () => floatValue),
                    UI.DynamicElementOnTrigger(
                        build: () => UI.Label("> 0.5f"),
                        rebuildIf: (e) => floatValue > 0.5f
                    )
                )
                */
                UI.Fold("List",
                    UI.List(
                        () => arrayValue,
                        (itemBinder, idx) => UI.Row(
                            UI.Field("Item " + idx, itemBinder),
                            UI.Button("+", () => arrayValue[idx]++),
                            UI.Button("-", () => arrayValue[idx]--)
                        )
                    ),
                    UI.List(
                        () => listValue,
                        (itemBinder, idx) => UI.Row(
                            UI.Field("Item " + idx, itemBinder),
                            UI.Button("+", () => listValue[idx]++),
                            UI.Button("-", () => listValue[idx]--)
                        )
                    ),
                    UI.List("ReadOnlyList",
                        () => listValue.AsReadOnly(),
                        (itemBinder, idx) => UI.Row(
                            UI.Field("Item " + idx, itemBinder),
                            UI.Button("+", () => listValue[idx]++),
                            UI.Button("-", () => listValue[idx]--)
                        )
                    ),
                    UI.ListReadOnly(() => listValue)
                ),
                UI.Fold(UI.Field("CustomBar", () => boolValue), null),
                UI.Fold(UI.Button("LeftBar"), UI.Button("RightBar"), null),
                
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
                    UI.Button($"{nameof(ElementExtensionsMethodChain.SetWidth)}(100f)").SetWidth(100f),
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
                ),
                UI.Fold("Null",
                    UI.Box(
                        UI.Label("Field"),
                        UI.Field(() => nullString),
                        UI.Field(() => nullList),
                        UI.Row(
                            UI.Field(nameof(nullOneLineClass), () => nullOneLineClass),
                            UI.Button("Toggle null",
                                () => { nullOneLineClass = nullOneLineClass != null ? null : new SimpleClass(); })
                        ),
                        UI.Field(nameof(nullMultiLineClass), () => nullMultiLineClass),
                        UI.Field(nameof(nullElementCreator), () => nullElementCreator)
                    ),
                    UI.Box(
                        UI.Label("Slider"),
                        UI.Slider(() => nullList),
                        UI.Slider(() => nullOneLineClass),
                        UI.Slider(() => nullMultiLineClass),
                        UI.Slider(() => nullElementCreator)
                    )
                )
            );
        }
    }
}