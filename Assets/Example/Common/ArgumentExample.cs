using System;
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
                ExampleTemplate.CodeElementSets("<b>Custom Label</b>",
                    new[]
                    {
                        ("UI.Field(\"CustomLabel\", () => floatValue);", UI.Field("CustomLabel", () => floatValue)),
                        ("UI.Slider(\"CustomLabel\", () => floatValue);", UI.Slider("CustomLabel", () => floatValue)),
                        ("UI.MinMaxSlider(\"CustomLabel\", () => vector2Value);",
                            UI.MinMaxSlider("CustomLabel", () => vector2Value)),
                        ("UI.Dropdown(\"CustomLabel\", () => dropDownIndex, dropDownOptions);",
                            UI.Dropdown("CustomLabel", () => dropDownIndex, dropDownOptions)),
                        ("UI.TextArea(\"CustomLabel\", () => stringValue);",
                            UI.TextArea("CustomLabel", () => stringValue)),
                        ("UI.List(\"CustomLabel\", () => intList);", UI.List("CustomLabel", () => intList)),
                    }
                ),
                ExampleTemplate.CodeElementSets("<b>No Label</b>",
                    new[]
                    {
                        ("UI.Field(null, () => floatValue);", UI.Field(null, () => floatValue)),
                        ("UI.Slider(null, () => floatValue);", UI.Slider(null, () => floatValue)),
                        ("UI.MinMaxSlider(null, () => vector2Value);", UI.MinMaxSlider(null, () => vector2Value)),
                        ("UI.Dropdown(null, () => dropDownIndex, dropDownOptions);",
                            UI.Dropdown(null, () => dropDownIndex, dropDownOptions)),
                        ("UI.TextArea(null, () => stringValue);", UI.TextArea(null, () => stringValue)),
                        ("UI.List(null, () => intList);", UI.List(null, () => intList)),
                    }
                ),
                ExampleTemplate.CodeElementSets("<b>Expression</b>",
                    new[]
                    {
                        ("UI.Field(() => floatValue / 2f);", UI.Field(() => floatValue / 2f)),
                        ("UI.Field(() => floatValue / 2f, f => floatValue = f * 2f);",
                            UI.Field(() => floatValue / 2f, f => floatValue = f * 2f)),
                        ("UI.Slider(() => floatValue / 2f);", UI.Slider(() => floatValue / 2f)),
                        ("UI.Slider(() => floatValue / 2f, f => floatValue = f * 2f);",
                            UI.Slider(() => floatValue / 2f, f => floatValue = f * 2f)),
                        ("UI.MinMaxSlider(() => vector2Value / 2f);", UI.MinMaxSlider(() => vector2Value / 2f)),
                        ("UI.MinMaxSlider(() => vector2Value / 2f, f => vector2Value = f * 2f);",
                            UI.MinMaxSlider(() => vector2Value / 2f, f => vector2Value = f * 2f)),
                        ("UI.Dropdown(() => GetDropdownIndex(), dropDownOptions);",
                            UI.Dropdown(() => GetDropdownIndex(), dropDownOptions)),
                        ("UI.Dropdown(() => GetDropdownIndex(), SetDropdownIndex,  dropDownOptions);",
                            UI.Dropdown(() => GetDropdownIndex(), SetDropdownIndex, dropDownOptions)),
                        ("UI.TextArea(() => stringValue.ToUpper());", UI.TextArea(() => stringValue.ToUpper())),
                        ("UI.TextArea(() => stringValue.ToUpper(), s => stringValue = s.ToLower());",
                            UI.TextArea(() => stringValue.ToUpper(), s => stringValue = s.ToLower())),
                        ("UI.List(() => GetIntList());", UI.List(() => GetIntList())),
                        ("UI.List(() => GetIntList(), SetIntList);", UI.List(() => GetIntList(), SetIntList)),
                    }
                )
            );
        }

        int GetDropdownIndex() => dropDownIndex;
        void SetDropdownIndex(int idx) => dropDownIndex = idx;
        
        List<int> GetIntList() => intList;
        void SetIntList(List<int> list) => intList = list;
    }
}