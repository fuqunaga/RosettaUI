using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Example
{
    public class SafetyExample : MonoBehaviour, IElementCreator
    {
        public class CircularReferenceClass
        {
            public CircularReferenceClass other;
        }

        public Element CreateElement()
        {
            string nullString = null;
            List<float> nullList = null;
            SimpleClass nullOneLineClass = null;
            ComplexClass nullMultiLineClass = null;
            ElementCreator nullElementCreator = null;

            CircularReferenceClass circularReferenceClass = new();
            CircularReferenceClass circularReferenceClassOther = new();

            circularReferenceClass.other = circularReferenceClassOther;
            circularReferenceClassOther.other = circularReferenceClass;


            return UI.Column(
                ExampleTemplate.TitleIndent("<b>Null</b>",
                    ExampleTemplate.UIFunctionColumnBox(nameof(UI.Field),
                        UI.Field(() => nullString),
                        UI.Field(() => nullList),
                        UI.Field(() => nullOneLineClass),
                        UI.Field(() => nullMultiLineClass),
                        UI.Field(() => nullElementCreator)
                    ),
                    ExampleTemplate.UIFunctionColumnBox(nameof(UI.Slider),
                        UI.Slider(() => nullList),
                        UI.Slider(() => nullOneLineClass),
                        UI.Slider(() => nullMultiLineClass),
                        UI.Slider(() => nullElementCreator)
                    ),
                    ExampleTemplate.UIFunctionColumnBox(nameof(UI.MinMaxSlider),
                        UI.List(() => nullList)
                    )
                ),
                ExampleTemplate.TitleIndent("<b>Circular reference</b>",
                    ExampleTemplate.UIFunctionColumn(nameof(UI.Field),
                    UI.Field(() => circularReferenceClass)
                    ),
                    ExampleTemplate.UIFunctionColumn(nameof(UI.Slider),
                        UI.Slider(() => circularReferenceClass)
                    )
                )
            );
        }
    }
}