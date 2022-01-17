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
                UI.Row(
                    UI.Page(
                        UI.Label("<b>UI.Slider(() => target)</b>"),
                        UI.Indent(
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
                        )
                    ),
                    UI.Page(
                        UI.Label("<b>UI.SliderReadOnly(() => intValue)</b>"),
                        UI.Indent(
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
                        )
                    )
                ),
                UI.Space().SetHeight(10f),
                UI.Page(
                    UI.Label("<b>Tips</b>"),
                    UI.Indent(
                        UI.Label("<b>UI.Slider(\"CustomLabel\", () => floatValue)</b>"),
                        UI.Slider("CustomLabel", () => floatValue),
                        UI.Space().SetHeight(10f),
                    
                        UI.Label("<b>UI.Slider(\"Custom min max\", () => floatValue, -1f, 2f)</b>"),
                        UI.Slider("Custom min max", () => floatValue, -1f, 2f),
                        UI.Label("<b>UI.Slider(\"Custom min max\", () => vector2Value, Vector2.one * -1, Vector2.one)"),
                        UI.Slider("Custom min max", () => vector2Value, Vector2.one * -1, Vector2.one),
                        UI.Space().SetHeight(10f),
            
                        UI.Label("<b>UI.Slider(() => vector2Value.x)</b>"),
                        UI.Label("Supports public field/property"),
                        UI.Slider(() => vector2Value.x),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.Slider(() => floatValue + 0.1f),</b>"),
                        UI.Label("Non-interactable if the expression is not assignable"),
                        UI.Slider(() => floatValue + 0.1f),
                        UI.Label("Interactable if set label and writeValue func"),
                        UI.Slider($"{nameof(floatValue)} + 0.1f",
                            () => floatValue + 0.1f,
                            f => floatValue = f - 0.1f
                        ),
                        UI.Space().SetHeight(10f),

                        UI.Label("<b>Null safe</b>"),
                        UI.Slider(() => nullClass),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.Slider(() => stringValue)</b>"),
                        UI.Label("Unsupported types will fall back to UI.Field()"),
                        UI.Slider(() => stringValue),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>Min and max will be set automatically if there is a range attribute</b>"),
                        UI.HelpBox("[Range(-1f, 1f)] public float rangeValue;"),
                        UI.Slider(() => rangeValue)
                    )
                )

            );
        }
    }
}