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
            SimpleClass nullClass = null;

            CircularReferenceClass circularReferenceClass = new();
            CircularReferenceClass circularReferenceClassOther = new();

            circularReferenceClass.other = circularReferenceClassOther;
            circularReferenceClassOther.other = circularReferenceClass;
            
            return UI.Row(
                ExampleTemplate.UIFunctionPage(nameof(UI.Field),
                    UI.Field(() => nullList),
                    UI.Field(() => nullClass),
                    UI.Field(() => nullString),
                    UI.Field(() => circularReferenceClass)
                ),
                ExampleTemplate.UIFunctionPage(nameof(UI.Slider),
                    UI.Slider(() => nullList),
                    UI.Slider(() => nullClass),
                    UI.Space().SetHeight(32),
                    UI.Slider(() => circularReferenceClass)
                ),
                ExampleTemplate.UIFunctionPage(nameof(UI.List),
                    UI.List(() => nullList)
                )
            );
        }
    }
}