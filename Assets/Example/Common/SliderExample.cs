using System.Collections.Generic;
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

        public string stringValue;
        [Range(-1f, 1f)] public float rangeValue;

        public Element CreateElement()
        {
            SimpleClass nullClass = null;
            return UI.Column(
#if true
                UI.Slider(() => vector2Value)
#else
                UI.Fold("Slider",
                    UI.Slider(() => intValue),
                    UI.Slider(() => uintValue),
                    UI.Slider(() => floatValue),
                    UI.Slider(() => vector2Value),
                    UI.Slider(() => vector3Value),
                    UI.Slider(() => vector4Value),
                    UI.Slider(() => vector2IntValue),
                    UI.Slider(() => vector3IntValue),
                    UI.Slider(() => rectValue),
                    UI.Slider(() => rectIntValue),
                    UI.Slider(() => rectOffsetValue),
                    UI.Slider(() => boundsValue),
                    UI.Slider(() => boundsIntValue),
                    UI.Slider(() => simpleClass)
                ),
                UI.Fold("ReadOnly",
                    UI.SliderReadOnly(() => intValue),
                    UI.SliderReadOnly(() => uintValue),
                    UI.SliderReadOnly(() => floatValue),
                    UI.SliderReadOnly(() => vector2Value),
                    UI.SliderReadOnly(() => vector3Value),
                    UI.SliderReadOnly(() => vector4Value),
                    UI.SliderReadOnly(() => vector2IntValue),
                    UI.SliderReadOnly(() => vector3IntValue),
                    UI.SliderReadOnly(() => rectValue),
                    UI.SliderReadOnly(() => rectIntValue),
                    UI.SliderReadOnly(() => rectOffsetValue),
                    UI.SliderReadOnly(() => boundsValue),
                    UI.SliderReadOnly(() => boundsIntValue),
                    UI.SliderReadOnly(() => simpleClass)
                ),
                UI.Fold("Usage"
                    , UI.Slider("CustomLabel", () => floatValue)
                    , UI.Slider("Custom min max", () => floatValue, -1f, 2f)
                    , UI.Slider("Custom min max", () => vector2Value, Vector2.one * -1, Vector2.one) 
                    , UI.Slider("ValueChangedCallback", targetExpression: () => floatValue)
                        .RegisterValueChangeCallback(() =>  print($"{nameof(floatValue)} changed."))

                    // non-interactable if the expression is read-only,
                    , UI.Slider(() => floatValue + 0.1f)

                    // interactable if (label,readValue,writeValue) style
                    , UI.Slider($"{nameof(floatValue)} + 0.1f",
                        () => floatValue + 0.1f,
                        f => floatValue = f - 0.1f
                    )

                    // Supports public member
                    , UI.Slider(() => vector2Value.x)

                    // Null safe
                    , UI.Slider(() => nullClass)

                    // unsupported type will fallback to UI.Field()
                    , UI.Slider("Unsupported type (string)", () => stringValue)

                    // Min and max will be set automatically if there is a range attribute, 
                    , UI.Slider(() => rangeValue)
                )
#endif
            );
        }
    }
}