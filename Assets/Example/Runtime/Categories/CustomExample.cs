using System;
using System.Linq;
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
        public class MyClass
        {
            private float _privateValue;
            [NonSerialized] public float publicValueNonSerialized;
            public float PropertyValue { get; set; }
        }

        public class MySuffixAttribute : PropertyAttribute
        {
            public string suffix;
        }

        public class MyDropDownAttribute : PropertyAttribute { }

        [Serializable]
        public class MyAttributeClass
        {
            public float value;
            [MySuffix(suffix = "suffix")] public float suffixValue;
            public string stringValue;
            [MyDropDown] public string myDropDownStringValue = "A";
        }

        public MyFloat myFloatValue;
        public int intValue;
        public MyClass myClass;
        public Vector2 vector2Value;
        public MyAttributeClass myAttributeClass;

        public Element CreateElement(LabelElement _)
        {
            SyntaxHighlighter.AddPattern("type", nameof(MyFloat));
            SyntaxHighlighter.AddPattern("type", nameof(MyClass));
            SyntaxHighlighter.AddPattern("type", nameof(MyAttributeClass));
            SyntaxHighlighter.AddPattern("type", nameof(PropertyAttribute));
            SyntaxHighlighter.AddPattern("type", nameof(MySuffixAttribute));
            SyntaxHighlighter.AddPattern("type", nameof(MyDropDownAttribute));

            return UI.Tabs(
                    CreateTabIElementCreator(),
                    CreateTabCreationFunc(),
                    CreateTabPropertyField(),
                    CreateTabOverrideByAttributeFunc()
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

UI.Field(() => myFloatValue));
",
                    UI.Field(() => myFloatValue)
                )
            );
        }

        private (string, Element) CreateTabCreationFunc()
        {
            using var scope = UICustom.ElementCreationFuncScope.Create<int>((label, readValue, writeValue) =>
            {
                return UI.Row(
                    UI.Field(label, readValue, writeValue), // UI.Field() in CreationFunc calls default UI.Field()
                    UI.Button("+", () => writeValue(readValue()+1)),
                    UI.Button("-", () => writeValue(readValue()-1))
                );
            });

            return ExampleTemplate.CodeElementSetsWithDescriptionTab("ElementCreationFunc",
                "Default UI can be defined with UICustom.",
                (@"using var scope = UICustom.ElementCreationFuncScope.Create<int>((label, readValue, writeValue) =>
{
    return UI.Row(
        UI.Field(label, readValue, writeValue), // UI.Field() in CreationFunc calls default UI.Field()
        UI.Button(""+"", () => writeValue(readValue()+1)),
        UI.Button(""-"", () => writeValue(readValue()-1))
    );
});

UI.Field(() => intValue);
",
                    UI.Field(() => intValue)
                )
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
                        ExampleTemplate.CodeElementSets("Property/Field",
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
    ""_privateValue"", 
    ""publicValueNonSerialized"",
    ""PropertyValue""
);

UI.Field(() => myClass).Open();
",
                                UI.Field(() => myClass).Open())
                        ),
                        ExampleTemplate.CodeElementSets("Property/Field Label",
                            (@"using var labelModifierScope = new UICustom.PropertyOrFieldLabelModifierScope<Vector2>(
    (""x"", ""horizontal""),
    (""y"", ""vertical"")
);

UI.Field(() => vector2Value);
",
                                UI.Field(() => vector2Value)
                                )
                        )
                    )
                );
        }

        private (string, Element) CreateTabOverrideByAttributeFunc()
        {
            using var modifyScope = UICustom.ElementModifyAttributeScope.Create<MySuffixAttribute>((attribute, element) =>
            {
                return UI.Row(element, UI.Label(attribute.suffix), UI.Space().SetWidth(4f));
            });

            using var overrideScope = UICustom.ElementOverrideAttributeScope.Create<MyDropDownAttribute, string>((attribute, label, readValue, writeValue) =>
            {
                return UI.Dropdown(label, readValue, writeValue, new[] { "A", "B", "C" });
            });

            return ExampleTemplate.CodeElementSetsWithDescriptionTab("ElementOverrideAttribute",
                "Default field UI can be overridden / modified by custom element.",
                (@"public class MySuffixAttribute : PropertyAttribute
{
    public string suffix;
}

public class MyDropDownAttribute : PropertyAttribute { }

[Serializable]
public class MyAttributeClass
{
    public float value;
    [MySuffix(suffix = ""suffix"")] public float suffixValue;
    public string stringValue;
    [MyDropDown] public string myDropDownStringValue = ""A"";
}

public MyAttributeClass myAttributeClass;

using var modifyScope = UICustom.ElementModifyAttributeScope.Create<MySuffixAttribute>((attribute, element) =>
{
    return UI.Row(element, UI.Label(attribute.suffix), UI.Space().SetWidth(4f));
});

using var overrideScope = UICustom.ElementOverrideAttributeScope.Create<MyDropDownAttribute, string>((attribute, label, readValue, writeValue) =>
{
    return UI.Dropdown(label, readValue, writeValue, new[] { ""A"", ""B"", ""C"" });
});

UI.Field(() => myAttributeClass);
",
                    UI.Field(() => myAttributeClass).Open()
                )
            );
        }
    }
}