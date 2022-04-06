using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class MethodExample : MonoBehaviour, IElementCreator
    {
        public struct FindObjectOption
        {
            public bool supportMultiple;
            public bool includeInactive;
        }
        
        
        [Multiline] public string multiLineStringValue = "this is\nmultiline\nstring";
        public Texture texture;
        public int dropDownIndex;
        public FindObjectOption windowLauncherOptions;
        public FindObjectOption fieldIfObjectFoundOptions;

        
        
        public List<int> listValue = new[] {1, 2, 3}.ToList();
        public int[] arrayValue = new[] {1, 2, 3};

        public float floatValue;
        public int intValue;
        public bool boolValue;
        public List<int> intList = new(new[] {1, 2, 3});
        
        public UICustomClass uiCustomClass;

        public bool dynamicElementIf;

        
        
        
        public Element CreateElement()
        {
            UICustom.RegisterElementCreationFunc<UICustomClass>((uiCustomClass) =>
            {
                return UI.Column(
                    UI.Label("UICustomized!"),
                    UI.Slider(() => uiCustomClass.floatValue)
                );
            });


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