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

        public Element CreateElement(LabelElement _)
        {
            int? nullableInt = null;
            List<float> nullList = null;
            SimpleClass nullClass = null;
            Gradient nullGradient = null;

            CircularReferenceClass circularReferenceClass = new();
            CircularReferenceClass circularReferenceClassOther = new();

            circularReferenceClass.other = circularReferenceClassOther;
            circularReferenceClassOther.other = circularReferenceClass;

            return UI.Page(
                ExampleTemplate.UIFunctionPage(nameof(UI.Field),
                    UI.Field(() => nullableInt),
                    UI.Field(() => nullClass),
                    UI.Field(() => nullList),
                    UI.Field(() => nullGradient),
                    UI.Field(() => circularReferenceClass)
                ),
                ExampleTemplate.UIFunctionPage(nameof(UI.Slider),
                    UI.Slider(() => nullableInt),
                    UI.Slider(() => nullClass),
                    UI.Slider(() => nullList),
                    UI.Slider(() => circularReferenceClass)
                ),
                ExampleTemplate.UIFunctionPage(nameof(UI.List),
                    UI.List(() => nullList)
                )
            );
        }
    }
}