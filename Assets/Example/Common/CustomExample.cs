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

            public Element CreateElement()
            {
                return UI.Slider(nameof(MyFloat), () => value);
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
            public float publicValue;
            private float _privateValue;
            [NonSerialized] public float publicValueNonSerialized;
            public float PropertyValue { get; set; }
        }

        
        public MyFloat myFloatValue;
        public MyInt myInt;
        public MyClass myClass;

        
        public Element CreateElement()
        {
            UICustom.RegisterElementCreationFunc<MyInt>(instance =>
            {
                return UI.Row(
                    UI.Field(nameof(MyInt), () => instance.value),
                    UI.Button("+", () => instance.value++),
                    UI.Button("-", () => instance.value--)
                );
            });
            
            UICustom.RegisterPropertyOrFields<MyClass>(
                "_privateValue", 
                "publicValueNonSerialized",
                "PropertyValue"
            );

            UICustom.UnregisterPropertyOrFields<MyClass>(
                "publicValue"
            );



            return UI.Column(
                ExampleTemplate.TitleIndent("<b>Custom default UI</b>",
                    ExampleTemplate.CodeElementSets("<b>IElementCreator</b>",
                        (@"public class MyFloat : IElementCreator
{
    public float value;

    public Element CreateElement()
    {
        return UI.Slider(nameof(MyFloat), () => value);
    }
}

public MyFloat myFloatValue;

UI.Field(() => myFloatValue));",
                            UI.Field(() => myFloatValue)
                        )
                    ),
                    ExampleTemplate.CodeElementSets("<b>CreationFunc</b>",
                        (@"public class MyInt
{
    public int value;
}

public MyInt myInt;

UICustom.RegisterElementCreationFunc<MyInt>(instance =>
{
    return UI.Row(
        UI.Field(nameof(MyInt), () => instance.value),
        UI.Button(""+"", () => instance.value++),
        UI.Button(""-"", () => instance.value--)
    );
});",
                            UI.Field(() => myInt))
                    ),
                    ExampleTemplate.CodeElementSets("<b>Property/Field</b>",
                        "RosettaUI is intended for members serialized in Unity. Otherwise, it can be explicitly specified.",
                        (@"public class MyClass
{
    public float publicValue;
    private float privateValue;
    [NonSerialized] public float publicValueNonSerialized;
    public float PropertyValue { get; set; }
}

public MyClass myClass;

UICustom.RegisterPropertyOrFields<MyClass>(
    ""privateValue"", 
    ""publicValueNonSerialized"",
    ""PropertyValue""
);

UICustom.UnregisterPropertyOrFields<MyClass>(
    ""publicValue""
);

UI.Field(() => myClass);",
                            UI.Field(() => myClass)))
                )
            );
        }
    }
}