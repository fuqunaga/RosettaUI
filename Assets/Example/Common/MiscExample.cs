using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

namespace RosettaUI.Example
{
    public class MiscExample : MonoBehaviour, IElementCreator
    {
        public int dropDownIndex;
        public List<int> listValue = new[] {1, 2, 3}.ToList();

        public float floatValue;
        public int intValue;

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
                UI.Button("Button", () => print("On button clicked"))
                , UI.Dropdown("Dropdown",
                    () => dropDownIndex,
                    options: new[] {"One", "Two", "Three"}
                )
                , UI.Fold("List"
                    /*
                    , UI.List("List",
                        () => listValue,
                        (itemBinder, idx) => UI.Row(
                            UI.Field("Item " + idx, itemBinder)
                            , UI.Button("+", () => listValue[idx]++)
                            , UI.Button("-", () => listValue[idx]--)
                        )
                    )
                    , UI.List("ReadOnlyList",
                        () => listValue.AsReadOnly(),
                        (itemBinder, idx) => UI.Row(
                            UI.Field("Item " + idx, itemBinder)
                            , UI.Button("+", () => listValue[idx]++)
                            , UI.Button("-", () => listValue[idx]--)
                        )
                    )
                    */
                )
                , UI.Fold("Row/Column/Box/Fold/ScrollView"
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
                    , UI.Label(nameof(UI.ScrollView))
                    , UI.ScrollView(
                        Enumerable.Range(0, 100).Select(i => UI.Field("Count", () => i.ToString()))
                    ).SetHeight(100f)
                )
                , UI.Fold("ElementCreator"
                    , UI.ElementCreatorWindowLauncher<ElementCreatorSimple>()
                    , UI.ElementCreatorInline<ElementCreatorSimple>()
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
                            var buttons = Enumerable.Range(0, intValue).Select(i => UI.Button(i.ToString(), null));
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
                    , UI.Row(
                        UI.Field(nameof(ElementExtensionsMethodChain.SetMinWidth), () => floatValue).SetMinWidth(0f)
                    )
                    , UI.Row
                    (
                        UI.Field(nameof(ElementExtensionsMethodChain.SetMinHeight), () => floatValue).SetMinHeight(50f)
                    )
                    , UI.Row(
                        UI.Label(nameof(ElementExtensionsMethodChain.SetJustify)).SetMinWidth(0)
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