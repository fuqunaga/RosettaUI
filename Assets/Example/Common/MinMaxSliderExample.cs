using UnityEngine;

namespace RosettaUI.Example
{
    public class MinMaxSliderExample : MonoBehaviour, IElementCreator
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
        public ComplexClass complexClass;


        public Element CreateElement()
        {
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
                    , UI.Slider(() => complexClass)
                )
                , UI.Fold("Usage"
                    , UI.Slider("CustomLabel", () => floatValue)
                    , UI.Slider("custom min max", () => floatValue, -1f, 2f)
                    , UI.Slider("onValueChanged", () => floatValue, f => print($"{nameof(floatValue)} changed."))
                    , UI.Slider(() => vector2Value.x) // public member
                    , UI.Slider(() => floatValue + 0.1f) // non-interactable if the expression is read-only, 
                    , UI.Slider("Expression with onValueChanged",
                        () => floatValue + 0.1f,
                        f => floatValue = f - 0.1f) // interactable If onValuedChanged callback is present
                )
            );
        }
    }
}