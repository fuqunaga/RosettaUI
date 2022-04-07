using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Example
{
    public class MethodExample : MonoBehaviour, IElementCreator
    {
        public float floatValue;
        public int intValue;
        public List<int> intList = new(new[] {1, 2, 3});
        
        public Element CreateElement()
        {
            return UI.Column(
                ExampleTemplate.CodeElementSets("<b>Styles</b>",
                    ("UI.Field(() => intValue).SetEnable(false); // disappear",
                        UI.Field(() => intValue).SetEnable(false)),
                    ("UI.Field(() => intValue).SetInteractable(false);",
                        UI.Field(() => intValue).SetInteractable(false)),
                    ("UI.Button(\"Width\").SetWidth(300f);", UI.Button("Width").SetWidth(300f)),
                    ("UI.Button(\"Height\").SetHeight(300f);", UI.Button("Height").SetHeight(50f)),
                    ("UI.Button(\"Color\").SetColor(Color.red);", UI.Button("Color").SetColor(Color.red)),
                    ("UI.Button(\"BackgroundColor\").SetBackgroundColor(Color.blue);", UI.Button("BackgroundColor").SetBackgroundColor(Color.blue))
                ),
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
                ),

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
    }
}