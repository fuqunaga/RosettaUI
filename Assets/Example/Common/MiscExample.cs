using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public Element CreateElement()
        {
            string nullString = null;
            List<float> nullList = null;
            SimpleClass nullOneLineClass = null;
            ComplexClass nullMultiLineClass = null;
            ElementCreator nullElementCreator = null;


            UICustom.RegisterElementCreationFunc<UICustomClass>((uiCustomClass) =>
            {
                return UI.Column(
                    UI.Label("UICustomized!"),
                    UI.Slider(() => uiCustomClass.floatValue)
                );
            });

            return UI.Column(
                UI.Row(
                    UI.Label($"{nameof(UI.Space)} >")
                    , UI.Space()
                    , UI.Label($"< {nameof(UI.Space)}")
                )
                
                , UI.Button(nameof(UI.Button), () => print("On button clicked"))
                
                , UI.Dropdown(nameof(UI.Dropdown),
                    () => dropDownIndex,
                    options: new[] {"One", "Two", "Three"}
                )
                , UI.DropdownReadOnly($"{nameof(UI.DropdownReadOnly)}(selection index will not change)",
                    () => dropDownIndex,
                    options: new[] {"One", "Two", "Three"}
                ),
                
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
                )
                , UI.Popup(
                    UI.Box(UI.Label($"{nameof(UI.Popup)}(Right click)")),
                    () => new[]
                    {
                        new MenuItem("Menu0", () => Debug.Log("Menu0")),
                        new MenuItem("Menu1", () => Debug.Log("Menu1")),
                        new MenuItem("Menu2", () => Debug.Log("Menu2"))
                    }
                )
                , UI.Fold("Row/Column/Box/Fold/ScrollView/Indent"
                    , UI.Row(
                        UI.Label("Row0"),
                        UI.Label("Row1"),
                        UI.Label("Row2")
                    )
                    , UI.Column(
                        UI.Label("Column0"),
                        UI.Label("Column1"),
                        UI.Label("Column2")
                    )
                    , UI.Box(
                        UI.Label("Box0"),
                        UI.Label("Box1"),
                        UI.Label("Box2")
                    )
                    , UI.Fold("Fold",
                        UI.Fold("Fold2",
                            UI.Fold("Fold3",
                                UI.Label("contents")
                            )
                        )
                    )
                    , UI.Fold(UI.Field("CustomBar", () => boolValue), null)
                    , UI.Fold(UI.Button("LeftBar"), UI.Button("RightBar"), null)
                    , UI.Label(nameof(UI.ScrollView))
                    , UI.ScrollView(
                        Enumerable.Range(0, 100).Select(i => UI.Field("Count", () => i.ToString()))
                    ).SetHeight(100f)
                    , UI.Indent(
                        UI.Field(() => "Indent0")
                        , UI.Indent(
                            UI.Field(() => "Indent2")
                            , UI.Indent(
                                UI.Field(() => "Indent3")
                            )
                        )
                    )
                )
                , UI.Fold("ChildValueChangedCallback",
                    UI.Field(() => intValue),
                    UI.Field(() => floatValue)
                ).RegisterValueChangeCallback(() => Debug.Log($"OnChildValueChanged"))
                , UI.Fold("FindObject"
                    , UI.WindowLauncher<BehaviourExample>()
                    , UI.FieldIfObjectFound<BehaviourExample>()
                )
                , UI.Fold("DynamicElement"
                    , UI.Field(() => dynamicElementIf)
                    , UI.DynamicElementIf(
                        () => dynamicElementIf,
                        () => UI.Label(nameof(UI.DynamicElementIf))
                        )
                    , UI.Slider("Button count", () => intValue, max: 10)
                    , UI.DynamicElementOnStatusChanged(
                        readStatus: () => intValue,
                        build: (status) =>
                        {
                            var buttons = Enumerable.Range(0, intValue).Select(i => UI.Button(i.ToString()));
                            var label = UI.Label(nameof(UI.DynamicElementOnStatusChanged));
                            return UI.Row(
                                new Element[] {label}.Concat(buttons)
                            );
                        })
                    /*
                    , UI.Box(
                        UI.Slider("DynamicElementOnTrigger if > 0.5f", () => floatValue),
                        UI.DynamicElementOnTrigger(
                            build: () => UI.Label("> 0.5f"),
                            rebuildIf: (e) => floatValue > 0.5f
                        )
                    )
                    */
                )
                , UI.Fold("Methods"
                    , UI.Label(nameof(ElementExtensionsMethodChain.SetEnable)).SetEnable(false) // disappear
                    , UI.Field(nameof(ElementExtensionsMethodChain.SetInteractable), () => floatValue).SetInteractable(false)
                    , UI.Label(nameof(ElementExtensionsMethodChain.SetColor)).SetColor(Color.red)
#if true
                    , UI.Button($"{nameof(ElementExtensionsMethodChain.SetWidth)}(100f)").SetWidth(100f)
                    , UI.Button($"{nameof(ElementExtensionsMethodChain.SetHeight)}(50f)").SetHeight(50f)
#else
                    , UI.Button($"{nameof(ElementExtensionsMethodChain.SetWidth)}(100f)", null).SetWidth(100f)
                    , UI.Button($"{nameof(ElementExtensionsMethodChain.SetMinWidth)}(200f)", null).SetMinWidth(50f)
                    , UI.Button($"{nameof(ElementExtensionsMethodChain.SetMaxWidth)}(200f)", null).SetMaxWidth(200f)
                    , UI.Column(
                        UI.Button($"{nameof(ElementExtensionsMethodChain.SetHeight)}(50f)", null).SetHeight(50f)
                        , UI.Button($"{nameof(ElementExtensionsMethodChain.SetMinHeight)}(50f)", null).SetMinHeight(50f)
                        , UI.Button($"{nameof(ElementExtensionsMethodChain.SetMaxHeight)}(50f)", null).SetMaxHeight(50f)
                    ).SetHeight(200f)
#endif
                    , UI.Row(
                        UI.Button($"{nameof(ElementExtensionsMethodChain.SetJustify)}(Style.Justify.End)")
                    ).SetJustify(Style.Justify.End)
                    , UI.Fold(nameof(ElementExtensionsMethodChain.Open), UI.Label("Open")).Open()
                    , UI.Fold(nameof(ElementExtensionsMethodChain.Close), UI.Label("Close")).Close()
                )
                , UI.Fold("UI Customize"
                    , UI.Field(() => uiCustomClass)
                    // Element Creator in class/Array
                )
                , UI.Fold("Null"
                    , UI.Box(
                        UI.Label("Field")
                        , UI.Field(() => nullString)
                        , UI.Field(() => nullList)
                        , UI.Row(
                            UI.Field(nameof(nullOneLineClass), () => nullOneLineClass),
                            UI.Button("Toggle null",
                                () => { nullOneLineClass = nullOneLineClass != null ? null : new SimpleClass(); })
                        )
                        , UI.Field(nameof(nullMultiLineClass), () => nullMultiLineClass)
                        , UI.Field(nameof(nullElementCreator), () => nullElementCreator)
                    )
                    , UI.Box(
                        UI.Label("Slider")
                        , UI.Slider(() => nullList)
                        , UI.Slider(() => nullOneLineClass)
                        , UI.Slider(() => nullMultiLineClass)
                        , UI.Slider(() => nullElementCreator)
                    )
                )
            );
        }
    }
}