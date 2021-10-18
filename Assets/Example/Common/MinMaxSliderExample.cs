using UnityEngine;

namespace RosettaUI.Example
{
    public class MinMaxSliderExample : MonoBehaviour, IElementCreator
    {
        public MinMax<int> intMinMax;
        public MinMax<uint> uintMinMax;
        public MinMax<float> floatMinMax;
        public MinMax<Vector2> vector2MinMax;
        public MinMax<Vector3> vector3MinMax;
        public MinMax<Vector4> vector4MinMax;
        public MinMax<Vector2Int> vector2IntMinMax;
        public MinMax<Vector3Int> vector3IntMinMax;
        public MinMax<Rect> rectMinMax;
        public MinMax<RectInt> rectIntMinMax;
        public MinMax<RectOffset> rectOffsetMinMax;
        public MinMax<Bounds> boundsMinMax;
        public MinMax<BoundsInt> boundsIntMinMax;

        public Element CreateElement()
        {
            return UI.Column(
                UI.Fold("MinMaxSlider"
                    , UI.MinMaxSlider(() => intMinMax)
                    , UI.MinMaxSlider(() => uintMinMax)
                    , UI.MinMaxSlider(() => floatMinMax)
                    , UI.MinMaxSlider(() => vector2MinMax)
                    , UI.MinMaxSlider(() => vector3MinMax)
                    , UI.MinMaxSlider(() => vector4MinMax)
                    , UI.MinMaxSlider(() => vector2IntMinMax)
                    , UI.MinMaxSlider(() => vector3IntMinMax)
                    , UI.MinMaxSlider(() => rectMinMax)
                    , UI.MinMaxSlider(() => rectIntMinMax)
                    , UI.MinMaxSlider(() => rectOffsetMinMax)
                    , UI.MinMaxSlider(() => boundsMinMax)
                    , UI.MinMaxSlider(() => boundsIntMinMax)
                )
                , UI.Fold("Usage"
                    , UI.MinMaxSlider("CustomLabel", () => floatMinMax)
                    , UI.MinMaxSlider("Custom min max", () => floatMinMax, min: -1f, max: 2f)
                    , UI.MinMaxSlider("Custom min max", () => vector2MinMax, min: Vector2.zero, max: new Vector2(360f, 100f)) 
                    , UI.MinMaxSlider("onValueChanged",
                        targetExpression: () => floatMinMax,
                        onValueChanged: f => print($"{nameof(floatMinMax)} changed."))

                    // non-interactable if the expression is read-only,
                    , UI.MinMaxSlider(() => MinMax.Create(floatMinMax.min + 0.1f, floatMinMax.max + 0.1f))

                    // interactable If onValuedChanged callback is present
                    , UI.MinMaxSlider(
                        targetExpression: () => MinMax.Create(floatMinMax.min + 0.1f, floatMinMax.max + 0.1f),
                        onValueChanged: f => floatMinMax = MinMax.Create(f.min - 0.1f, f.max - 0.1f))
                    )
            );
        }
    }
}