using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Example
{
    public class MethodExample : MonoBehaviour, IElementCreator
    {
        public float floatValue;
        public int intValue;
        public List<int> intList = new(new[] {1, 2, 3});
        
        public Element CreateElement(LabelElement _)
        {
            return UI.Tabs(
                CreateTabStyles(),
                CreateTabValueChangeCallback(),
                CreateTabOpenClose(),
                CreateTabWindowPosition()
            );
        }

        private (string, Element) CreateTabStyles()
        {
            return (ExampleTemplate.TabTitle("Styles"),
                    ExampleTemplate.CodeElementSets("<b>Styles</b>",
                        ("UI.Field(() => intValue).SetEnable(false); // disappear",
                            UI.Field(() => intValue).SetEnable(false)),
                        ("UI.Field(() => intValue).SetInteractable(false);",
                            UI.Field(() => intValue).SetInteractable(false)),
                        ("UI.Button(\"Width\").SetWidth(300f);", UI.Button("Width").SetWidth(300f)),
                        ("UI.Button(\"Height\").SetHeight(300f);", UI.Button("Height").SetHeight(50f)),
                        ("UI.Button(\"Color\").SetColor(Color.red);", UI.Button("Color").SetColor(Color.red)),
                        ("UI.Button(\"BackgroundColor\").SetBackgroundColor(Color.blue);",
                            UI.Button("BackgroundColor").SetBackgroundColor(Color.blue))
                    )
                );
        }

        private (string, Element) CreateTabValueChangeCallback()
        {
            return (ExampleTemplate.TabTitle("ValueChangeCallback"),
                    ExampleTemplate.CodeElementSets(
                        ExampleTemplate.ElementFunctionStr(nameof(ElementExtensionsMethodChain
                            .RegisterValueChangeCallback)),
                        (@"UI.Field(() => intValue)
    .RegisterValueChangeCallback(
        () => Debug.Log(""On field value changed."")
    );
",
                            UI.Field(() => intValue)
                                .RegisterValueChangeCallback(() => Debug.Log("On field value changed"))),
                        (@"UI.Box(
  UI.Field(() => intValue),
  UI.Field(() => floatValue)
).RegisterValueChangeCallback(
    () => Debug.Log(""On any of the fields value changed."")
);",
                            UI.Box(
                                UI.Field(() => intValue),
                                UI.Field(() => floatValue)
                            ).RegisterValueChangeCallback(() => Debug.Log("On any of the fields value changed."))
                        )
                    )
                );
        }

        private (string, Element) CreateTabOpenClose()
        {
            return (ExampleTemplate.TabTitle("Open/Close"),
                    ExampleTemplate.CodeElementSets("<b>Open/Close</b>",
                        (@"UI.Fold(""SetOpenFlag"", 
    UI.Label(""Element"")
).SetOpenFlag(true);
",
                            UI.Fold("SetOpenFlag", UI.Label("Element")).SetOpenFlag(true)
                        ),
                        (@"UI.Fold(""Open"",
    UI.Label(""Element"")
).Open();
",
                            UI.Fold("Open", UI.Label("Element")).Open()
                        ),
                        ("UI.List(() => intList).Close();", UI.List(() => intList).Close())
                    )
                );
        }

        private (string, Element) CreateTabWindowPosition()
        {
            return (ExampleTemplate.TabTitle("WindowPosition"),
                    ExampleTemplate.CodeElementSets("<b>WindowPosition</b>",
                        (@"var window = UI.Window(""Window"");
var position = Vector2.zero;

UI.Column(
    UI.WindowLauncher(""Window"", window),
    UI.Slider(
        () => position,
        max: new Vector2(Screen.width, Screen.height)
    ).RegisterUpdateCallback(_ => window.Position = position)
);
",
                            WindowPosition()
                        )
                    )
                );
            
            
            Element WindowPosition()
            {
                var window = UI.Window("Window");
                var position = Vector2.zero;

                return UI.Column(
                    UI.WindowLauncher("Window", window),
                    UI.Slider(
                        () => position,
                        max: new Vector2(Screen.width, Screen.height)
                    ).RegisterUpdateCallback(_ => window.Position = position)
                );
            }
        }
    }
}