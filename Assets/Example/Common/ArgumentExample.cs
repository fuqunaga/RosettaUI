using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Example
{
    public class ArgumentExample : MonoBehaviour, IElementCreator
    {
        public float floatValue;
        public string stringValue = "this is string.";
        public Vector2 vector2Value;
        public List<int> intList = new(new[] {1, 2, 3});
        
        public int dropDownIndex;
        public string[] dropDownOptions = new[] {"One", "Two", "Three"};


        public Element CreateElement(LabelElement _)
        {
            return UI.Tabs(
                CreateCustomLabel(),
                CreateNoLabel(),
                CreateExpression(),
                CreateExpressionTreeLimitation()
            );
        }

        int GetDropdownIndex() => dropDownIndex;
        void SetDropdownIndex(int idx) => dropDownIndex = idx;
        
        List<int> GetIntList() => intList;
        void SetIntList(List<int> list) => intList = list;

        private (string, Element) CreateCustomLabel()
        {
            return (ExampleTemplate.TabTitle("Custom Label"),
                    ExampleTemplate.CodeElementSets("<b>Custom Label</b>",
                        ("UI.Field(\"CustomLabel\", () => floatValue);", UI.Field("CustomLabel", () => floatValue)),
                        ("UI.Slider(\"CustomLabel\", () => floatValue);", UI.Slider("CustomLabel", () => floatValue)),
                        ("UI.MinMaxSlider(\"CustomLabel\", () => vector2Value);",
                            UI.MinMaxSlider("CustomLabel", () => vector2Value)),
                        ("UI.Dropdown(\"CustomLabel\", () => dropDownIndex, dropDownOptions);",
                            UI.Dropdown("CustomLabel", () => dropDownIndex, dropDownOptions)),
                        ("UI.TextArea(\"CustomLabel\", () => stringValue);",
                            UI.TextArea("CustomLabel", () => stringValue)),
                        ("UI.List(\"CustomLabel\", () => intList);", UI.List("CustomLabel", () => intList))
                    )
                );
        }

        private (string, Element) CreateNoLabel()
        {
            return (ExampleTemplate.TabTitle("No Label"),
                    ExampleTemplate.CodeElementSets("<b>No Label</b>",
                        ("UI.Field(null, () => floatValue);", UI.Field(null, () => floatValue)),
                        ("UI.Slider(null, () => floatValue);", UI.Slider(null, () => floatValue)),
                        ("UI.MinMaxSlider(null, () => vector2Value);", UI.MinMaxSlider(null, () => vector2Value)),
                        ("UI.Dropdown(null, () => dropDownIndex, dropDownOptions);",
                            UI.Dropdown(null, () => dropDownIndex, dropDownOptions)),
                        ("UI.TextArea(null, () => stringValue);", UI.TextArea(null, () => stringValue)),
                        ("UI.List(null, () => intList);", UI.List(null, () => intList))
                    )
                );
        }

        private (string, Element) CreateExpression()
        {
            return (ExampleTemplate.TabTitle("Expression"),
                    ExampleTemplate.CodeElementSets("<b>Expression</b>",
                        ("UI.Field(() => floatValue / 2f);", UI.Field(() => floatValue / 2f)),
                        ("UI.Field(() => floatValue / 2f, f => floatValue = f * 2f);",
                            UI.Field(() => floatValue / 2f, f => floatValue = f * 2f)),
                        ("UI.Slider(() => floatValue / 2f);", UI.Slider(() => floatValue / 2f)),
                        ("UI.Slider(() => floatValue / 2f, f => floatValue = f * 2f);",
                            UI.Slider(() => floatValue / 2f, f => floatValue = f * 2f)),
                        ("UI.MinMaxSlider(() => vector2Value / 2f);", UI.MinMaxSlider(() => vector2Value / 2f)),
                        ("UI.MinMaxSlider(() => vector2Value / 2f, f => vector2Value = f * 2f);",
                            UI.MinMaxSlider(() => vector2Value / 2f, f => vector2Value = f * 2f)),
                        ("UI.Dropdown(() => GetDropdownIndex(),dropDownOptions);",
                            UI.Dropdown(() => GetDropdownIndex(), dropDownOptions)),
                        ("UI.Dropdown(() => GetDropdownIndex(),SetDropdownIndex,  dropDownOptions);",
                            UI.Dropdown(() => GetDropdownIndex(), SetDropdownIndex, dropDownOptions)),
                        ("UI.TextArea(() => stringValue.ToUpper());", UI.TextArea(() => stringValue.ToUpper())),
                        ("UI.TextArea(() => stringValue.ToUpper(),s => stringValue = s.ToLower());",
                            UI.TextArea(() => stringValue.ToUpper(), s => stringValue = s.ToLower())),
                        ("UI.List(() => GetIntList());", UI.List(() => GetIntList())),
                        ("UI.List(() => GetIntList(),SetIntList);", UI.List(() => GetIntList(), SetIntList))
                    )
                );
        }

        private (string, Element) CreateExpressionTreeLimitation()
        {
            return (ExampleTemplate.TabTitle("ExpressionTree limitation"),
                    ExampleTemplate.CodeElementSets("<b>ExpressionTree limitation</b>",
                        "targetExpression args cannot use \"?.\"(null-conditional operator), \"{}\"(blocks) or local function because of ExpressionTree limitation.\n" +
                        "but it can be used in styles such as UI.FieldReadOnly(label, readValue) and UI.Field(label, readValue, writeValue).",
                        ("// UI.Field(() => stringValue?.Length); // compile error", null),
                        ("// UI.Field(() => { LocalFunction(); return intValue;}); // compile error", null),
                        ("UI.FieldReadOnly(\"null-conditional operator\", () => stringValue?.Length);",
                            UI.FieldReadOnly("null-conditional operator", () => stringValue?.Length)
                        ),
                        (@"UI.Field(""Block and localFunction"",
    readValue: () =>
    {
        LocalFunction();
        return floatValue;
    },
    writeValue: f => floatValue = f
);",
                            UI.Field("Block and localFunction",
                                readValue: () =>
                                {
                                    LocalFunction();
                                    return floatValue;
                                },
                                writeValue: f => floatValue = f
                            )
                        )
                    )
                );

            float LocalFunction() => floatValue;
        }
    }
}