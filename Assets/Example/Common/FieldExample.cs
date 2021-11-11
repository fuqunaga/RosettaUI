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


        public Element CreateElement()
        {
            SimpleClass nullClass = null;
            return UI.Column(
                UI.Fold("Allows any type"
                    , UI.Field(() => intValue)
                    , UI.Field(() => uintValue)
                    , UI.Field(() => floatValue)
                    , UI.Field(() => stringValue)
                    , UI.Field(() => boolValue)
                    , UI.Field(() => enumValue)
                    , UI.Field(() => colorValue)
                    , UI.Field(() => vector2Value)
                    , UI.Field(() => vector3Value)
                    , UI.Field(() => vector4Value)
                    , UI.Field(() => vector2IntValue)
                    , UI.Field(() => vector3IntValue)
                    , UI.Field(() => rectValue)
                    , UI.Field(() => rectIntValue)
                    , UI.Field(() => rectOffsetValue)
                    , UI.Field(() => boundsValue)
                    , UI.Field(() => boundsIntValue)
                    , UI.Field(() => intList)
                    , UI.Field(() => floatArray)
                    , UI.Field(() => simpleClass)
                    , UI.Field(() => classList)
                )
                , UI.Fold("Usage"
                    , UI.Field("CustomLabel", () => floatValue)
                    , UI.Field("onValueChanged",
                        targetExpression: () => floatValue,
                        onValueChanged: f => print($"{nameof(floatValue)} changed.")
                    )

                    // non-interactable if the expression is read-only,
                    , UI.Field(() => floatValue + 1f)

                    // interactable If onValuedChanged callback is present
                    , UI.Field(
                        targetExpression: () => floatValue + 1f,
                        onValueChanged: f => floatValue = f - 1f
                    )

                    // Supports public member
                    , UI.Field(() => vector2Value.x)
                    
                    // Null safe
                    , UI.Field(() => nullClass)
                    
                    // Field with range attribute will become Slider
                    , UI.Field(() => rangeValue)

                    // IElementCreator will use CreateElement() method
                    , UI.Field(() => elementCreator)
                    
                    // UI.Field()'s targetExpressions cannot use blocks({}) or local functions(ExpressionTree limitations)
                    // but UI.FieldReadOnly() can.
                    // , UI.Field(() => { return intValue;}) // compile error
                    , UI.FieldReadOnly(nameof(UI.FieldReadOnly), () => { return intValue; })
                )
                /*
                , UI.Fold("Complex types"
                    , UI.Field(() => complexClass)
                )
                */
            );
        }
    }
}