using UnityEngine;

namespace RosettaUI.Example
{
    public class SliderExample : MonoBehaviour, IElementCreator
    {
        public int intValue;
        public uint uintValue;
        public float floatValue;
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
        public SimpleClass simpleClass;

        [Range(-100f, 100f)] public int rangeIntValue;
        [Range(-100f, 100f)] public float rangeValue;
        public string stringValue;

        public Element CreateElement()
        {
            SimpleClass nullClass = null;
            return UI.Column(
                UI.Fold("Slider"
                    , UI.Slider(() => intValue)
                    , UI.Slider(() => uintValue)
                    , UI.Slider(() => floatValue)
                    , UI.Slider(() => vector2Value)
                    , UI.Slider(() => vector3Value)
                    , UI.Slider(() => vector4Value)
                    , UI.Slider(() => vector2IntValue)
                    , UI.Slider(() => vector3IntValue)
                    , UI.Slider(() => rectValue)
                    , UI.Slider(() => rectIntValue)
                    , UI.Slider(() => rectOffsetValue)
                    , UI.Slider(() => boundsValue)
                    , UI.Slider(() => boundsIntValue)
                    , UI.Slider(() => simpleClass)
                )
                , UI.Fold("Usage"
                    , UI.Slider("CustomLabel", () => floatValue)
                    , UI.Slider("custom min max", () => floatValue, -1f, 2f)
                    , UI.Slider("onValueChanged",
                        targetExpression: () => floatValue,
                        onValueChanged: f => print($"{nameof(floatValue)} changed."))

                    // non-interactable if the expression is read-only,
                    , UI.Slider(() => floatValue + 0.1f)

                    // interactable If onValuedChanged callback is present
                    , UI.Slider(
                        targetExpression: () => floatValue + 0.1f,
                        onValueChanged: f => floatValue = f - 0.1f
                    )

                    // Supports public member
                    , UI.Slider(() => vector2Value.x)

                    // Null safe
                    , UI.Slider(() => nullClass)

                    // unsupported type will fallback to UI.Field()
                    , UI.Slider("Unsupported type (string)", () => stringValue)

                    // Min and max will be set automatically if the range attribute is present, 
                    , UI.Slider(() => rangeIntValue)
                    , UI.Slider(() => uintValue, (uint)10, (uint)100)
                )
            );
        }
    }
}