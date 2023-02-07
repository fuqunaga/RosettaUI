using UnityEngine;

namespace RosettaUI.Example
{
    public class MiscExample : MonoBehaviour, IElementCreator
    {
        [Multiline]
        public string multiLineStringValue = "this is\nmultiline\nstring";
        public Texture texture;
        public int dropDownIndex;
        public bool boolValue;

        public Element CreateElement(LabelElement _)
        {
            return UI.Page(
                ExampleTemplate.UIFunctionColumn(nameof(UI.TextArea),
                    UI.TextArea(nameof(UI.TextArea), () => multiLineStringValue),
                    UI.TextAreaReadOnly(nameof(UI.TextAreaReadOnly), () => multiLineStringValue)
                ),
                ExampleTemplate.UIFunctionColumn(nameof(UI.Dropdown),
                    UI.Dropdown(nameof(UI.Dropdown),
                        () => dropDownIndex,
                        options: new[] { "One", "Two", "Three" }
                    ),
                    UI.DropdownReadOnly($"{nameof(UI.DropdownReadOnly)}",
                        () => dropDownIndex,
                        options: new[] { "One", "Two", "Three" }
                    )
                ),
                ExampleTemplate.UIFunctionColumn(nameof(UI.Toggle),
                    UI.Toggle(nameof(UI.Toggle), () => boolValue),
                    UI.ToggleReadOnly(nameof(UI.ToggleReadOnly), () => boolValue)
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.Image),
                    UI.Image(() => texture).SetMaxWidth(200f).SetMaxHeight(200f)
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.Button),
                    UI.Button(nameof(UI.Button), () => print("On button clicked"))
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.HelpBox),
                    UI.Column(
                        UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.None)}", HelpBoxType.None),
                        UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Info)}", HelpBoxType.Info),
                        UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Warning)}",
                            HelpBoxType.Warning),
                        UI.HelpBox($"{nameof(UI.HelpBox)} {nameof(HelpBoxType.Error)}", HelpBoxType.Error)
                    )
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.Space),
                    UI.Space().SetBackgroundColor(Color.gray)
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.Popup),
                    UI.Popup(
                        UI.Box(UI.Label($"{nameof(UI.Popup)}(Right click)")),
                        () => new[]
                        {
                            new MenuItem("Menu0", () => Debug.Log("Menu0")),
                            new MenuItem("Menu1", () => Debug.Log("Menu1")),
                            new MenuItem("Menu2", () => Debug.Log("Menu2"))
                        }
                    )
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.Clickable),
                    UI.Clickable(
                        UI.Box(UI.Label($"Set click callbacks for any element.")),
                        clickEvent => Debug.Log($"{nameof(clickEvent)} Button[{clickEvent.Button}] Position[{clickEvent.Position}")
                    )
                )
            );
        }
    }
}