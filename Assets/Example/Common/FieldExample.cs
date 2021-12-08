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
        public List<int> intList = new List<int>(new[] {1, 2, 3});
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

        [Multiline]
        public string multiLineStringValue = "this is\nmultiline\nstring";


        public Element CreateElement()
        {
            SimpleClass nullClass = null;
            return UI.Column(
                UI.Field(() => stringValue)
                #if false
                UI.Fold("Allows any type",
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
                UI.Fold("ReadOnly",
                    UI.FieldReadOnly(nameof(intValue), () => intValue),
                    UI.FieldReadOnly(nameof(uintValue), () => uintValue),
                    UI.FieldReadOnly(nameof(floatValue), () => floatValue),
                    UI.FieldReadOnly(nameof(stringValue), () => stringValue),
                    UI.FieldReadOnly(nameof(boolValue), () => boolValue),
                    UI.FieldReadOnly(nameof(enumValue), () => enumValue),
                    UI.FieldReadOnly(nameof(colorValue), () => colorValue),
                    UI.FieldReadOnly(nameof(vector2Value), () => vector2Value),
                    UI.FieldReadOnly(nameof(vector3Value), () => vector3Value),
                    UI.FieldReadOnly(nameof(vector4Value), () => vector4Value),
                    UI.FieldReadOnly(nameof(vector2IntValue), () => vector2IntValue),
                    UI.FieldReadOnly(nameof(vector3IntValue), () => vector3IntValue),
                    UI.FieldReadOnly(nameof(rectValue), () => rectValue),
                    UI.FieldReadOnly(nameof(rectIntValue), () => rectIntValue),
                    UI.FieldReadOnly(nameof(rectOffsetValue), () => rectOffsetValue),
                    UI.FieldReadOnly(nameof(boundsValue), () => boundsValue),
                    UI.FieldReadOnly(nameof(boundsIntValue), () => boundsIntValue),
                    UI.FieldReadOnly(nameof(intList), () => intList),
                    UI.FieldReadOnly(nameof(floatArray), () => floatArray),
                    UI.FieldReadOnly(nameof(simpleClass), () => simpleClass),
                    UI.FieldReadOnly(nameof(classList), () => classList)
                ),
                UI.Fold("TextArea",
                    UI.TextArea(() => multiLineStringValue),
                    UI.TextAreaReadOnly(() => multiLineStringValue)
                ),
                UI.Fold("Usage",
                    UI.Field("CustomLabel", () => floatValue),
                    UI.Field("ValueChangedCallback", () => floatValue)
                        .RegisterValueChangeCallback(() => print($"{nameof(floatValue)} changed.")),

                    // non-interactable if the expression is not assignable,
                    UI.Field(() => floatValue + 1f),

                    // interactable if (label,readValue,writeValue) style
                    UI.Field($"{nameof(floatValue)}  + 1f",
                        () => floatValue + 1f,
                        f => floatValue = f - 1f
                    ),

                    // Supports public member
                    UI.Field(() => vector2Value.x),
                    
                    // Null safe
                    UI.Field(() => nullClass),
                    
                    // TODO
                    // Field with range attribute will become Slider
                    //, UI.Field(() => rangeValue)

                    // IElementCreator will use CreateElement() method
                    UI.Field(() => elementCreator),
                    
                    
                    UI.Fold("ReadOnly",
                        UI.Label("ReadOnly: only display the value"),
                        UI.FieldReadOnly(() => intValue),

                        // UI.Field()'s targetExpressions cannot use ?.(null-conditional operator), {}(blocks) or local functions(ExpressionTree limitations)
                        // but UI.FieldReadOnly(label, readValue) and UI.Field(label, readValue, writeValue) can
                        //
                        // UI.Field(() => stringValue?.Length), // compile error
                        // UI.Field(() => { LocalFunction(); return intValue;}) // compile error
                        
                        UI.FieldReadOnly(nameof(UI.FieldReadOnly), () =>
                        {
                            LocalFunction();
                            return intValue;
                        })
                    )
                )
                /*
                , UI.Fold("Complex types"
                    , UI.Field(() => complexClass)
                )
                */
                #endif
            );

            static void LocalFunction()
            {
            }
        }
    }
}