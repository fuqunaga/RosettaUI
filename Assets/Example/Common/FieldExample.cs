using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class FieldExample : MonoBehaviour, IElementCreator
    {
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

        public ElementCreator elementCreator;

        public List<SimpleClass> classList = new[]
        {
            new SimpleClass {floatValue = 1f, stringValue = "First"}
        }.ToList();
        
        [Range(-100f, 100f)]
        public float rangeValue;

        //public ComplexClass complexClass;


        public Element CreateElement()
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
                UI.Column(
                    UI.Label("<b>Tips</b>"),
                    UI.Indent(
                        UI.Label("<b>UI.Field(\"CustomLabel\", () => floatValue)</b>"),
                        UI.Field("CustomLabel", () => floatValue),
                        ExampleTemplate.BlankLine(),
                        
                        UI.Label("<b>UI.Field(() => vector2Value.x)</b>"),
                        UI.Label("Supports public field/property"),
                        UI.Field(() => vector2Value.x),
                        ExampleTemplate.BlankLine(),

                        UI.Label("<b>Element.RegisterValueChangeCallback()</b>"),
                        UI.Field("ValueChangedCallback", () => floatValue)
                            .RegisterValueChangeCallback(() => print($"{nameof(floatValue)} changed.")),
                        ExampleTemplate.BlankLine(),

                        UI.Label("<b>UI.Field(() => floatValue + 1f),</b>"),
                        UI.Label("Non-interactable if the expression is not assignable"),
                        UI.Field(() => floatValue + 1f),
                        UI.Label("Interactable if set label and writeValue func"),
                        UI.Field($"({nameof(floatValue)}  + 1f)",
                            () => floatValue + 1f,
                            f => floatValue = f - 1f
                        ),
                        ExampleTemplate.BlankLine(),

                        // TODO
                        // Field with range attribute will become Slider
                        //, UI.Field(() => rangeValue)

                        UI.Label("<b>If the target is IElementCreator, use CreateElement()</b>"),
                        UI.Field(() => elementCreator),
                        ExampleTemplate.BlankLine(),
                        
                        UI.Label(
                            "<b>ExpressionTree limitation</b>\n" +
                            "UI.Field()'s targetExpressions cannot use ?.(null-conditional operator), {}(blocks) or local functions.\n" +
                            "but UI.FieldReadOnly(label, readValue) and UI.Field(label, readValue, writeValue) can."
                            ),
                        UI.HelpBox(
                            "// UI.Field(() => stringValue?.Length), // compile error\n" +
                            "// UI.Field(() => { LocalFunction(); return intValue;}) // compile error"
                        ),
                        UI.FieldReadOnly("UI.FieldReadOnly(label, readValue)", () =>
                        {
                            LocalFunction();
                            return intValue;
                        }),
                        UI.Field("UI.Field(label, readValue, writeValue)",
                            () =>
                            {
                                LocalFunction();
                                return intValue;
                            },
                            i => intValue = i)
                    )
                )
            );

            static void LocalFunction()
            {
            }
        }
    }
}