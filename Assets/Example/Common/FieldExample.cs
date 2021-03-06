using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class FieldExample : MonoBehaviour, IElementCreator
    {
        [Serializable]
        public class AttributeTestClass
        {
            [Range(0f,100f)]
            public float rangeFloat;

            [Multiline]
            public string multiLineString;

            [NonReorderable]
            public List<int> nonReorderableList;
        }

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

        public List<SimpleClass> classList = new[]
        {
            new SimpleClass {floatValue = 1f, stringValue = "First"}
        }.ToList();
        
        public AttributeTestClass attributeTestClass;
        
        
        public Element CreateElement(LabelElement _)
        {
            return UI.Column(
                UI.Row(
                    ExampleTemplate.UIFunctionPage(nameof(UI.Field),
                        UI.Field(() => intValue),
                        UI.Field(() => uintValue),
                        UI.Field(() => floatValue),
                        UI.Field(() => stringValue),
                        UI.Field(() => boolValue),
                        UI.Field(() => enumValue),
                        UI.Field(() => colorValue),
                        UI.Field(() => vector2Value),
                        UI.Field(() => vector3Value),
                        UI.Field(() => vector4Value),
                        UI.Field(() => vector2IntValue),
                        UI.Field(() => vector3IntValue),
                        UI.Field(() => rectValue),
                        UI.Field(() => rectIntValue),
                        UI.Field(() => rectOffsetValue),
                        UI.Field(() => boundsValue),
                        UI.Field(() => boundsIntValue),
                        UI.Field(() => intList),
                        UI.Field(() => floatArray),
                        UI.Field(() => simpleClass),
                        UI.Field(() => classList)
                    ),
                    ExampleTemplate.UIFunctionPage(nameof(UI.FieldReadOnly),
                        UI.FieldReadOnly(() => intValue),
                        UI.FieldReadOnly(() => uintValue),
                        UI.FieldReadOnly(() => floatValue),
                        UI.FieldReadOnly(() => stringValue),
                        UI.FieldReadOnly(() => boolValue),
                        UI.FieldReadOnly(() => enumValue),
                        UI.FieldReadOnly(() => colorValue),
                        UI.FieldReadOnly(() => vector2Value),
                        UI.FieldReadOnly(() => vector3Value),
                        UI.FieldReadOnly(() => vector4Value),
                        UI.FieldReadOnly(() => vector2IntValue),
                        UI.FieldReadOnly(() => vector3IntValue),
                        UI.FieldReadOnly(() => rectValue),
                        UI.FieldReadOnly(() => rectIntValue),
                        UI.FieldReadOnly(() => rectOffsetValue),
                        UI.FieldReadOnly(() => boundsValue),
                        UI.FieldReadOnly(() => boundsIntValue),
                        UI.FieldReadOnly(() => intList),
                        UI.FieldReadOnly(() => floatArray),
                        UI.FieldReadOnly(() => simpleClass),
                        UI.FieldReadOnly(() => classList)
                    )
                ),
                ExampleTemplate.CodeElementSets("<b>Attribute</b>",
                    (@"public class AttributeTextClass
{
    [Range(0f,100f)]
    public float rangeFloat;

    [Multiline]
    public float multiLineString;

    [NonReorderable]
    public List<int> nonReorderableList;
}

UI.Field(() => attributeTestClass);
",
                        UI.Field(() => attributeTestClass)
                    )
                )
            );
        }
    }
}