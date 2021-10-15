using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Test;
using UnityEngine;

namespace RosettaUI.Example
{
    #region Type Define

    public enum MyEnum
    {
        One,
        Two,
        Three
    }

    [Serializable]
    public class SimpleClass
    {
        public float floatValue;
        public string stringValue;
        private int privateValue; // will be ignored
    }


    [Serializable]
    public class ElementCreator : IElementCreator
    {
        public float floatValue;

        public Element CreateElement() => UI.Slider("By CreteElement()", () => floatValue);
    }

    [Serializable]
    public class UICustomClass
    {
        public float floatValue;
    }


    [Serializable]
    public class ComplexClass
    {
        [Range(0f, 10f)]
        public float floatWithRangeValue; // Fields with a range attribute are automatically displayed with a slider

        public Vector3 vector3Value;
        public SimpleClass simpleClass;
        public ElementCreator elementCreatorClass;
        public UICustomClass uiCustomClass;
    }

    #endregion

    public abstract class RosettaUIExample : MonoBehaviour
    {
        public KeyCode toggleRootElementKey = KeyCode.U;

        public int intValue;
        public uint uintValue;
        public float floatValue;
        public string stringValue;
        public bool boolValue;
        public MyEnum enumValue;
        public Color colorValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public int dropDownIndex;
        public SimpleClass simpleClass;
        public ElementCreator elementCreator;
        public ComplexClass complexClass;
        public List<int> intList = new List<int>(new[] {1, 2, 3});
        public float[] floatArray = new[] {1f, 2f, 3f};

        public MinMax<int> intMinMax;
        public MinMax<float> floatMinMax;
        public MinMax<Vector2> vector2MinMax;


        Element rootElement;

        private int privateValue;
        private int privateProperty { get; set; }


        void Start()
        {
            UICustom.RegisterElementCreationFunc<UICustomClass>((uiCustomClass) =>
            {
                return UI.Column(
                    UI.Label("UICustomized!"),
                    UI.Field(() => uiCustomClass.floatValue)
                );
            });

            string nullString = null;
            SimpleClass nullOneLineClass = null;
            ComplexClass nullMultiLineClass = null;
            ElementCreator nullElementCreator = null;
            var fold = UI.Fold("Fold", UI.Fold("Fold2", UI.Field("hoge", () => intValue)));

            var list = Enumerable.Range(0, 3).Select(_ => new ComplexClass()).ToList();
            var smallWindow = UI.Window("SmallWindow",
                UI.Field(() => intValue)
            );

            var window = UI.Window(
#if true
                UI.ElementCreatorWindowLauncher<FieldExample>()
                , UI.ElementCreatorWindowLauncher<SliderExample>()
                , UI.ElementCreatorWindowLauncher<MinMaxSliderExample>()
                /*
                , UI.Row(
                    UI.Label("LogSlider[WIP]"),
                    new LogSlider(
                        Binder.Create(() => transform.localScale.x, (v) => transform.localScale = Vector3.one * v),
                        ConstGetter.Create((0.1f, 100f))
                    )
                )
                */
                , UI.Fold("ElementGroup"
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
                                UI.Field("contents", () => floatValue)
                            )
                        )
                    )
                )
                , UI.Fold("Null"
                    , UI.Box(
                        UI.Label("Field")
                        , UI.Field(nameof(String), () => nullString)
                        , UI.Field(nameof(List<float>), () => (List<float>) null)
                        , UI.Row(
                            UI.Field(nameof(nullOneLineClass), () => nullOneLineClass),
                            UI.Button("Totggle null", () =>
                            {
                                if (nullOneLineClass != null) nullOneLineClass = null;
                                else nullOneLineClass = new SimpleClass();
                            })
                        )
                        , UI.Field(nameof(nullMultiLineClass), () => nullMultiLineClass)
                        , UI.Field(nameof(nullElementCreator), () => nullElementCreator)
                    )
                    , UI.Box(
                        UI.Label("Slider")
                        , UI.Slider(nameof(List<float>), () => (List<float>) null)
                        , UI.Slider(nameof(nullOneLineClass), () => nullOneLineClass)
                        , UI.Slider(nameof(nullMultiLineClass), () => nullMultiLineClass)
                        , UI.Slider(nameof(nullElementCreator), () => nullElementCreator)
                    )
                )
                , UI.Dropdown("Dropdown",
                    () => dropDownIndex,
                    options: new[] {"One", "Two", "Three"}
                )
                , UI.Row(
                    UI.Label("DynamicElement Test\nchange ui when < 0"),
                    DynamicElement.Create(
                        readStatus: () => intValue > 0,
                        buildWithStatus: (status) =>
                        {
                            return status
                                ? UI.Field(nameof(intValue), () => intValue, (v) => intValue = v)
                                : UI.Label("Under Zero");
                        }
                    )
                )
                /*
                , UI.Fold("Interabletable",
                    fields.Concat(new[] {
                            UI.Button("Totggle", () =>
                            {
                                foreach(var f in fields) f.interactableSelf = !f.interactableSelf;
                            })
                        }
                    )
                ).Close()
                */
                , UI.Button("Totggle fold", () => { fold.IsOpen = !fold.IsOpen; })
                /*
                , UI.Fold("ObjectUI"
                    , objectUI.Field(nameof(privateValue)) // also use private member if you specify it explicitly.
                    , objectUI.Field(nameof(privateProperty)) // also use private member if you specify it explicitly.
                    , objectUI.Field("CustomLabel", nameof(privateValue)) // also use private member if you specify it explicitly.
                    , objectUI.FieldNoLabel(nameof(privateValue))
                    , objectUI.Slider(nameof(intValue), min: -100)
                    , objectUI.Slider(nameof(floatValue), max: 10f, onValueChanged: () => Debug.Log(floatValue))
                    , objectUI.Slider("CustomLabel", nameof(intValue), max:100f)
                    , objectUI.SliderNoLabel(nameof(intValue), max:100f)
                )
                */
                , UI.ElementCreatorWindowLauncher<ElementCreatorTest>()
                , UI.ElementCreatorInline<ElementCreatorTest>()
                , UI.Fold("Custom Width"
                    , UI.Row(
                        UI.Label("Buttons")
                        , UI.Button("1", null).SetMinWidth(30)
                        , UI.Button("2", null).SetMinWidth(30)
                        , UI.Button("3", null).SetMinWidth(30)
                        , UI.Button("4", null).SetMinWidth(30)
                        , UI.Button("5", null).SetMinWidth(30)
                    )
                    , UI.Row(
                        UI.Label(nameof(intValue))
                        , UI.Field(() => intValue).SetMinWidth(30)
                    )
                    , UI.Row(
                        UI.Label(nameof(simpleClass))
                        , UI.Field(() => simpleClass).SetMinWidth(300)
                    )
                )
#endif
            );

            window.SetMinWidth(400);

            BuildElement(window);

            rootElement = window;
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleRootElementKey))
            {
                rootElement.Enable = !rootElement.Enable;
            }
        }


        protected abstract void BuildElement(Element rootElement);
    }
}