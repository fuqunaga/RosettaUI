using UnityEngine;

namespace RosettaUI.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class ExampleSimple : MonoBehaviour
    {
        public string stringValue;
        public float floatValue;
        public int intValue;
        public Color colorValue;

        
        void Start()
        {
            var root = GetComponent<RosettaUIRoot>();
            root.Build(CreateElement());
        }

        Element CreateElement()
        {
            return UI.Window(nameof(ExampleSimple),
                UI.Page(
                    UI.Field(() => stringValue),
                    UI.Slider(() => floatValue),
                    UI.Row(
                        UI.Field(() => intValue),
                        UI.Button("+", () => intValue++),
                        UI.Button("-", () => intValue--)
                    ),
                    UI.Field(() => colorValue)
                )
            );
        }
    }
}