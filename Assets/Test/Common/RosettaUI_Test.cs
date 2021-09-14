using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Test
{
    public abstract class RosettaUI_Test : MonoBehaviour
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


        public KeyCode toggleRootElementKey = KeyCode.U;

        public int intValue;
        public uint uintValue;
        public float floatValue;
        public string stringValue;
        public bool boolValue;
        public MyEnum enumValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public int dropDownIndex;
        public SimpleClass simpleClass;
        public ElementCreator elementCreator;
        public ComplexClass complexClass;
        public List<int> intList = new List<int>(new[] { 1, 2, 3 });
        public List<SimpleClass> classList = new List<SimpleClass>(new[] { new SimpleClass() { floatValue = 1f, stringValue = "First" } });


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

            SimpleClass nullTestValue = null;
            var fold = UI.Fold("Fold", UI.Fold("Fold2", UI.Field("hoge", () => intValue)));

            var list = Enumerable.Range(0, 3).Select(_ => new ComplexClass()).ToList();

            var window = UI.Window(
#if false
                UI.Row(
                    UI.Field("Test Null", () => nullTestValue),
                    UI.Button("Totggle null", () =>
                        {
                            if (nullTestValue != null) nullTestValue = null;
                            else nullTestValue = new SimpleClass();
                        })
                )
#else
                UI.Label("Label")
                , UI.Field(() => intValue)
                , UI.Fold("Field allows any type"
                    , UI.Field(() => intValue)
                    , UI.Field(() => uintValue)
                    , UI.Field(() => floatValue)
                    , UI.Field(() => stringValue)
                    , UI.Field(() => boolValue)
                    , UI.Field(() => enumValue)
                    , UI.Field(() => vector2Value)
                    , UI.Field(() => vector3Value)
                    , UI.Field(() => vector4Value)
                    , UI.Field(() => intList)
                    , UI.Field(() => simpleClass)
                    , UI.Field(() => elementCreator)
                    , UI.Field(() => complexClass)
                    
                )
                , UI.Fold("Field usage"
                    , UI.Field("CustomLabel", () => floatValue)
                    , UI.Field("onValueChanged", () => floatValue, onValueChanged: (f) => print($"{nameof(floatValue)} changed."))
                    , UI.Field(() => vector3Value.x) // public member
                    , UI.Field(() => floatValue + 1) // non-interactable if the expression is read-only, 
                    , UI.Field("Expression with onValueChanged", () => floatValue + 1, onValueChanged: (f) => floatValue = f - 1)      // interactable If onValuedChanged callback is present
                )

                , UI.Fold("Slider"
                    , UI.Slider(() => intValue)
                    , UI.Slider(() => floatValue)
                    , UI.Slider("custom min max", () => floatValue, min:-1f, max:2f)
                    , UI.Slider(() => vector2Value)
                /*
                , UI.Slider(() => vector3Value,
                    min: Vector3.zero,
                    max: Vector3.one
                    )
                */
                /*
                , UI.Row(
                    UI.Label("LogSlider[WIP]"),
                    new LogSlider(
                        Binder.Create(() => transform.localScale.x, (v) => transform.localScale = Vector3.one * v),
                        ConstGetter.Create((0.1f, 100f))
                    )
                )
                */
                )

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
                , UI.Dropdown("Dropdown",
                    () => dropDownIndex,
                    options: new[] { "One", "Two", "Three" }
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
                , UI.Row(
                    UI.Field("Test Null", () => nullTestValue),
                    UI.Button("Totggle null", () =>
                    {
                        if (nullTestValue != null) nullTestValue = null;
                        else nullTestValue = new SimpleClass();
                    })
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
                , fold.Close()
                , UI.Button("Totggle fold", () =>
                {
                    fold.isOpen = !fold.isOpen;
                })
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
                , UI.FindObjectObserverElement<ElementCreatorTest>()
                , UI.Fold("Custom Width"
                    , UI.Row(
                        UI.Label("Buttons")
                        , UI.Button("1", null).SetWidth(30)
                        , UI.Button("2", null).SetWidth(30)
                        , UI.Button("3", null).SetWidth(30)
                        , UI.Button("4", null).SetWidth(30)
                        , UI.Button("5", null).SetWidth(30)
                        )
                    , UI.Row(
                        UI.Label(nameof(intValue))
                        , UI.Field(() => intValue).SetWidth(30)
                    )
                    , UI.Row(
                        UI.Label(nameof(simpleClass))
                        , UI.Field(() => simpleClass).SetWidth(300)
                    )
                )
#endif
            );


            BuildElement(window);

            rootElement = window;
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleRootElementKey))
            {
                rootElement.enable = !rootElement.enable;
            }
        }


        protected abstract void BuildElement(Element rootElement);
    }
}