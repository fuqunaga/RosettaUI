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
            SyntaxHighlighter.AddPattern("type", "MyMinMax");
            
            var sliderOption = new SliderOption()
            {
                showInputField = false,
                fieldOption = FieldOption.Default
            };
            
            return UI.Tabs(
                ExampleTemplate.UIFunctionTab(nameof(UI.MinMaxSlider),
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
                ExampleTemplate.UIFunctionTab(nameof(UI.MinMaxSliderReadOnly),
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
                ),
                ExampleTemplate.Tab("Codes",
                    ExampleTemplate.CodeElementSets("Range",
                        (@"UI.MinMaxSlider(
    () => floatMinMax,
    range: MinMax.Create(-1f, 1f)
);
",
                            UI.MinMaxSlider(
                                () => floatMinMax,
                                range: MinMax.Create(-1f, 1f)
                            )
                        ),
                        (@"UI.MinMaxSlider(
    () => vector2MinMax,
    range: MinMax.Create(-Vector2.one, Vector2.one)
).Open();
",
                            UI.MinMaxSlider(
                                () => vector2MinMax,
                                range: MinMax.Create(-Vector2.one, Vector2.one)
                            ).Open()
                        )
                    ),
                    ExampleTemplate.CodeElementSets("Option",
                        (@"var sliderOption = new SliderOption()
{
    showInputField = false,
    fieldOption = FieldOption.Default
};

UI.Field(() => sliderOption).Open();
                            
UI.DynamicElementOnStatusChanged(
    () => sliderOption,
    _ => UI.Slider(() => intMinMax, sliderOption)
        .RegisterValueChangeCallback(() => Debug.Log($""OnValueChanged[{intMinMax}]""))
)",
                            UI.Column(
                                UI.Field(() => sliderOption).Open(),
                                UI.DynamicElementOnStatusChanged(
                                    () => sliderOption,
                                    _ => UI.MinMaxSlider(() => intMinMax, sliderOption)
                                        .RegisterValueChangeCallback(() => Debug.Log($"OnValueChanged[{intMinMax}]"))
                                )
                            )
                        )
                    ),
                    ExampleTemplate.CodeElementSets("Supported types",
                        $"Supports any type that has a specific member pair [{string.Join(",", TypeUtility.MinMaxMemberNamePairs.Select(pair => $"({pair.Item1}, {pair.Item2})"))}]", 
                        (@"public struct MyMinMax<T>
{
    public T Min;
    public T Max;
}

public MyMinMax<float> myMinMax;

UI.MinMaxSlider(() => myMinMax);",
                            UI.MinMaxSlider(() => myMinMax)
                            ),
                        ("UI.MinMaxSlider(() => vector2Value);\n",
                            UI.MinMaxSlider(() => vector2Value)
                        )
                    )
                )
            );
        }
    }
}