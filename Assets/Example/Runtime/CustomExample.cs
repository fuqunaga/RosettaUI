using System;
using UnityEngine;

namespace RosettaUI.Example
{
    public class CustomExample : MonoBehaviour, IElementCreator
    {
        [Serializable]
        public class MyFloat : IElementCreator
        {
            public float value;

            public Element CreateElement(LabelElement label)
            {
                return UI.Slider(label, () => value);
            }
        }

        [Serializable]
        public class MyInt
        {
            public int value;
        }

        [Serializable]
        public class MyClass
        {
            private float _privateValue;
            [NonSerialized] public float publicValueNonSerialized;
            public float PropertyValue { get; set; }
        }

        public MyFloat myFloatValue;
        public MyInt myInt;
        public MyClass myClass;
        public Vector2 vector2Value;


        public Element CreateElement(LabelElement _)
        {
            return UI.Tabs(
                    CreateTabIElementCreator(),
                    CreateTabCreationFunc(),
                    CreateTabPropertyField()
                );
        }

        private (string, Element) CreateTabIElementCreator()
        {
            return ExampleTemplate.CodeElementSetsWithDescriptionTab("IElementCreate",
                "Default UI can be defined with CreateElement().",
                (@"public class MyFloat : IElementCreator
{
    public float value;

    public Element CreateElement(LabelElement label)
    {
        return UI.Slider(label, () => value);
    }
}

public MyFloat myFloatValue;

UI.Field(() => myFloatValue));",
                    UI.Field(() => myFloatValue)
                )
            );
        }

        private (string, Element) CreateTabCreationFunc()
        {
            using var creationFuncScope = new UICustom.ElementCreationFuncScope<MyInt>((label, getInstance) =>
            {
                return UI.Row(
                    UI.Field(label, () => getInstance().value),
                    UI.Button("+", () => getInstance().value++),
                    UI.Button("-", () => getInstance().value--)
                );
            });

            return ExampleTemplate.CodeElementSetsWithDescriptionTab("CreationFunc",
                "Default UI can be defined with UICustom.",
                (@"public class MyInt
{
    public int value;
}

public MyInt myInt;

using var creationFuncScope = 
    new UICustom.ElementCreationFuncScope<MyInt>((label, getInstance) =>
    {
        return UI.Row(
            UI.Field(label, () => getInstance().value),
            UI.Button(""+"", () => getInstance().value++),
            UI.Button(""-"", () => getInstance().value--)
        );
    });

UI.Field(() => myInt);",
                    UI.Field(() => myInt))
            );
        }

        private (string, Element) CreateTabPropertyField()
        {
            using var propertyOrFieldsScope = new UICustom.PropertyOrFieldsScope<MyClass>(
                "_privateValue",
                "publicValueNonSerialized",
                "PropertyValue"
            );
      
            using var labelModifierScope = new UICustom.PropertyOrFieldLabelModifierScope<Vector2>(
                ("x", "horizontal"),
                ("y", "vertical")
            );



            return (ExampleTemplate.TabTitle("Property/Field"),
                    UI.Page(
                        ExampleTemplate.CodeElementSets("<b>Property/Field</b>",
                            "RosettaUI is intended for members serialized in Unity. Otherwise, it can be explicitly specified.",
                            (@"public class MyClass
{
    private float _privateValue;
    [NonSerialized] 
    public float publicValueNonSerialized;
    public float PropertyValue { get; set; }
}

public MyClass myClass;

using var propertyOrFieldsScope = new UICustom.PropertyOrFieldsScope<MyClass>(
    ""privateValue"", 
    ""publicValueNonSerialized"",
    ""PropertyValue""
);

UI.Field(() => myClass);",
                                UI.Field(() => myClass))
                        ),
                        ExampleTemplate.CodeElementSets("<b>Property/Field Label</b>",
                            (@"using var labelModifierScope = new UICustom.PropertyOrFieldLabelModifierScope<Vector2>(
        (""x"", ""horizontal""),
        (""y"", ""vertical"")
    );

UI.Field(() => vector2Value).Open();",
                                UI.Field(() => vector2Value).Open()
                                )
                        )
                    )
                );
        }
    }
}