using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class ArgumentExample : MonoBehaviour, IElementCreator
    {
        public int intValue;
        public uint uintValue;
        public float floatValue;
        [Multiline]
        public string stringValue = "this is string.";
        public bool boolValue;
        public MyEnum enumValue;
        public Color colorValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Vector2Int vector2IntValue;
        public Vector3Int vector3IntValue;
        public Rect rectValue;
        public RectInt rectIntValue;
        public RectOffset rectOffsetValue;
        public Bounds boundsValue;
        public BoundsInt boundsIntValue;
        public List<int> intList = new(new[] {1, 2, 3});
        public float[] floatArray = {1f, 2f, 3f};
        public SimpleClass simpleClass;

        public ElementCreator elementCreator;

        public List<SimpleClass> classList = new[]
        {
            new SimpleClass {floatValue = 1f, stringValue = "First"}
        }.ToList();
        
        [Range(-100f, 100f)]
        public float rangeValue;

        //public ComplexClass complexClass;

        public int dropDownIndex;
        public string[] dropDownOptions = new[] {"One", "Two", "Three"};
        

        public Element CreateElement()
        {
            return UI.Column(
                ExampleTemplate.TitleIndent("<b>Custom Label</b>",
                    ExampleTemplate.CodeElementSets(new[]
                        {
                            ("UI.Field(\"CustomLabel\", () => floatValue);", UI.Field("CustomLabel", () => floatValue)),
                            ("UI.Slider(\"CustomLabel\", () => floatValue);", UI.Slider("CustomLabel", () => floatValue)),
                            ("UI.MinMaxSlider(\"CustomLabel\", () => vector2Value);", UI.MinMaxSlider("CustomLabel", () => vector2Value)),
                            ("UI.List(\"CustomLabel\", () => intList);", UI.List("CustomLabel", () => intList)),
                            ("UI.TextArea(\"CustomLabel\", () => stringValue);", UI.TextArea("CustomLabel", () => stringValue)),
                            ("UI.Dropdown(\"CustomLabel\", () => dropDownIndex, dropDownOptions);", UI.Dropdown("CustomLabel", () => dropDownIndex, dropDownOptions))
                        }
                    )
                ),
                ExampleTemplate.TitleIndent("<b>No Label</b>",
                    ExampleTemplate.CodeElementSets(new[]
                        {
                            ("UI.Field(null, () => floatValue);", UI.Field(null, () => floatValue)),
                            ("UI.Slider(null, () => floatValue);", UI.Slider(null, () => floatValue)),
                            ("UI.MinMaxSlider(null, () => vector2Value);", UI.MinMaxSlider(null, () => vector2Value)),
                            ("UI.List(null, () => intList);", UI.List(null, () => intList)),
                            ("UI.TextArea(null, () => stringValue);", UI.TextArea(null, () => stringValue)),
                            ("UI.Dropdown(null, () => dropDownIndex, dropDownOptions);", UI.Dropdown(null, () => dropDownIndex, dropDownOptions))
                        }
                    )
                ),
                ExampleTemplate.TitleIndent("<b>Public field/property</b>",
                    ExampleTemplate.CodeElementSets(new[]
                        {
                            ("UI.Field(() => vector2Value.x);", UI.Field(() => vector2Value.x)),
                            ("UI.Slider(() => vector2Value.x);", UI.Slider(() => vector2Value.x)),
                        }
                    )
                ),
                ExampleTemplate.TitleIndent("<b>Expression</b>",
                    ExampleTemplate.CodeElementSets(new[]
                        {
                            ("UI.Field(() => floatValue / 2f);",  UI.Field(() => floatValue / 2f)),
                            ("UI.Field(() => floatValue / 2f, f => floatValue = f * 2f);", UI.Field(() => floatValue / 2f, f => floatValue = f * 2f)),
                            ("UI.Slider(() => floatValue / 2f);",  UI.Slider(() => floatValue / 2f)),
                            ("UI.Slider(() => floatValue / 2f, f => floatValue = f * 2f);", UI.Slider(() => floatValue / 2f, f => floatValue = f * 2f)),
                            ("UI.MinMaxSlider(() => vector2Value / 2f);",  UI.MinMaxSlider(() => vector2Value / 2f)),
                            ("UI.MinMaxSlider(() => vector2Value / 2f, f => vector2Value = f * 2f);", UI.MinMaxSlider(() => vector2Value / 2f, f => vector2Value = f * 2f)),
                        }
                    )
                )
            );
        }
    }
}