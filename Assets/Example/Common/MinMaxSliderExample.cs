using System;
using System.Linq;
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

        [Serializable]
        public struct MyMinMax<T>
        {
            public T Min;
            public T Max;
        }

        public MyMinMax<float> myMinMax;

        public Vector2 vector2Value;
        
        public Element CreateElement(LabelElement _)
        {
            return UI.Column(
                UI.Row(
                    ExampleTemplate.UIFunctionPage(nameof(UI.MinMaxSlider),
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
                    ),
                    ExampleTemplate.UIFunctionPage(nameof(UI.MinMaxSliderReadOnly),
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
                ),
                ExampleTemplate.CodeElementSets("<b>Argument</b>",
                    new[]
                    {
                        ("UI.MinMaxSlider(() => floatMinMax, MinMax.Create(-1f, 1f));", UI.MinMaxSlider(() => floatMinMax, MinMax.Create(-1f, 1f))),
                        (@"UI.MinMaxSlider(() => vector2MinMax,
         MinMax.Create(-Vector2.one, Vector2.one);",
                            UI.MinMaxSlider(() => vector2MinMax, MinMax.Create(-Vector2.one, Vector2.one))),
                    }
                ),
                ExampleTemplate.CodeElementSets(
                    $"Supports any type that has a specific member pair [{string.Join(",", TypeUtility.MinMaxMemberNamePairs.Select(pair => $"({pair.Item1}, {pair.Item2})"))}]",
                    new[]
                    {
                        (@"public struct MyMinMax<T>
{
    public T Min;
    public T Max;
}

public MyMinMax<float> myMinMax;

UI.MinMaxSlider(() => myMinMax);",
                            UI.MinMaxSlider(() => myMinMax)),


                        ("UI.MinMaxSlider(() => vector2Value);", UI.MinMaxSlider(() => vector2Value))
                    }
                )
            );
        }
    }
}