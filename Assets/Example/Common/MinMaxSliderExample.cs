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
                    UI.Row(
                        UI.Page(
                            UI.Label("<b>UI.MinMaxSlider(() => target)</b>"),
                            UI.Indent(
                                UI.MinMaxSlider(() => intMinMax),
                                UI.MinMaxSlider(() => uintMinMax),
                                UI.MinMaxSlider(() => floatMinMax),
                                UI.MinMaxSlider(() => vector2MinMax),
                                UI.MinMaxSlider(() => vector3MinMax),
                                UI.MinMaxSlider(() => vector4MinMax),
                                UI.MinMaxSlider(() => vector2IntMinMax),
                                UI.MinMaxSlider(() => vector3IntMinMax),
                                UI.MinMaxSlider(() => rectMinMax),
                                UI.MinMaxSlider(() => rectIntMinMax),
                                UI.MinMaxSlider(() => rectOffsetMinMax),
                                UI.MinMaxSlider(() => boundsMinMax),
                                UI.MinMaxSlider(() => boundsIntMinMax)
                            )
                        ),
                        UI.Page(
                            UI.Label("<b>UI.MinMaxSliderReadOnly(() => target)</b>"),
                            UI.Indent(
                                UI.MinMaxSliderReadOnly(() => intMinMax),
                                UI.MinMaxSliderReadOnly(() => uintMinMax),
                                UI.MinMaxSliderReadOnly(() => floatMinMax),
                                UI.MinMaxSliderReadOnly(() => vector2MinMax),
                                UI.MinMaxSliderReadOnly(() => vector3MinMax),
                                UI.MinMaxSliderReadOnly(() => vector4MinMax),
                                UI.MinMaxSliderReadOnly(() => vector2IntMinMax),
                                UI.MinMaxSliderReadOnly(() => vector3IntMinMax),
                                UI.MinMaxSliderReadOnly(() => rectMinMax),
                                UI.MinMaxSliderReadOnly(() => rectIntMinMax),
                                UI.MinMaxSliderReadOnly(() => rectOffsetMinMax),
                                UI.MinMaxSliderReadOnly(() => boundsMinMax),
                                UI.MinMaxSliderReadOnly(() => boundsIntMinMax)
                            )
                        )
                    ),
                    UI.Space().SetHeight(10f),
                    UI.Page(
                        UI.Label("<b>Tips</b>"),
                        UI.Indent(
                            UI.Label("<b>UI.MinMaxSlider(\"CustomLabel\", () => floatMinMax)</b>"),
                            UI.MinMaxSlider("CustomLabel", () => floatMinMax),
                            UI.Space().SetHeight(10f),

                            UI.Label("<b>UI.MinMaxSlider(\"Custom min max\", () => floatMinMax, -1f, 1f)</b>"),
                            UI.MinMaxSlider("Custom min max", () => floatMinMax, -1f, 1f),
                            UI.Label(
                                "<b>UI.MinMaxSlider(\"Custom min max\", () => vector2MinMax, Vector2.one * -1f, Vector2.one)</b>"),
                            UI.MinMaxSlider("Custom min max", () => vector2MinMax, Vector2.one * -1f, Vector2.one)

#if false
                            //TODO: type free minmax
                            //TODO: MinMax unsupported type will fall back to UI.Slider()

                            // non-interactable if the expression is read-only,
                            , UI.MinMaxSlider("min,max + 0.1f",
                                () => MinMax.Create(floatMinMax.min + 0.1f, floatMinMax.max + 0.1f))

                            // interactable if (label,readValue,writeValue) style
                            , UI.MinMaxSlider("min,max + 0.1f",
                                () => MinMax.Create(floatMinMax.min + 0.1f, floatMinMax.max + 0.1f),
                                f => floatMinMax = MinMax.Create(f.min - 0.1f, f.max - 0.1f))
#endif
                        )
                    )
            );
        }
    }
}