using UnityEngine;

namespace RosettaUI.Example
{
    public class MinMaxSliderExample : MonoBehaviour, IElementCreator
    {
        public MinMax<int> intValue;
        public MinMax<uint> uintValue;
        public MinMax<float> floatValue;
        public MinMax<Vector2> vector2Value;
        public MinMax<Vector3> vector3Value;
        public MinMax<Vector4> vector4Value;
        public MinMax<Vector2Int> vector2IntValue;
        public MinMax<Vector3Int> vector3IntValue;
        public MinMax<Rect> rectValue;
        public MinMax<RectInt> rectIntValue;
        public MinMax<RectOffset> rectOffsetValue;
        public MinMax<Bounds> boundsValue;
        public MinMax<BoundsInt> boundsIntValue;
        

        public Element CreateElement()
        {
            return UI.Column(
                UI.Fold("MinMaxSlider"
                    , UI.MinMaxSlider(() => intValue)
                    , UI.MinMaxSlider(() => uintValue)
                    , UI.MinMaxSlider(() => floatValue)
                    , UI.MinMaxSlider(() => vector2Value)
                    , UI.MinMaxSlider(() => vector3Value)
                    , UI.MinMaxSlider(() => vector4Value)
                    , UI.MinMaxSlider(() => vector2IntValue)
                    , UI.MinMaxSlider(() => vector3IntValue)
                    , UI.MinMaxSlider(() => rectValue)
                    , UI.MinMaxSlider(() => rectIntValue)
                    , UI.MinMaxSlider(() => rectOffsetValue)
                    , UI.MinMaxSlider(() => boundsValue)
                    , UI.MinMaxSlider(() => boundsIntValue)
                )
                /*
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
                */
            );
        }
    }
}