using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Example
{
    public class MiscExample : MonoBehaviour, IElementCreator
    {
        [Multiline]
        public string multiLineStringValue = "this is\nmultiline\nstring";
        public Texture texture;
        public int dropDownIndex;
        public string dropDownString = "One";
        public bool boolValue;

        public Element CreateElement(LabelElement _)
        {
            return UI.Page(
                ExampleTemplate.UIFunctionColumn(nameof(UI.TextArea),
                    UI.TextArea(nameof(UI.TextArea), () => multiLineStringValue),
                    UI.TextAreaReadOnly(nameof(UI.TextAreaReadOnly), () => multiLineStringValue)
                ),
                ExampleTemplate.UIFunctionColumn(nameof(UI.Dropdown),
                    UI.Dropdown("Index",
                        () => dropDownIndex,
                        options: new[] { "One", "Two", "Three" }
                    ),
                    UI.DropdownReadOnly("Index ReadOnly",
                        () => dropDownIndex,
                        options: new[] { "One", "Two", "Three" }
                    ),
                    UI.Dropdown("String",
                        () => dropDownString,
                        options: new[] { "One", "Two", "Three" }
                    ),
                    UI.DropdownReadOnly("String ReadOnly",
                        () => dropDownString,
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
                ExampleTemplate.UIFunctionRow(nameof(UI.PopupMenuButton),
                    UI.PopupMenuButton(nameof(UI.PopupMenuButton), CreateMenuItems)
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
                        CreateMenuItems
                    )
                ),
                ExampleTemplate.UIFunctionRow(nameof(UI.Clickable),
                    UI.Clickable(
                        UI.Box(UI.Label($"Set click callbacks for any element.")),
                        clickEvent => Debug.Log($"{nameof(clickEvent)} Button[{clickEvent.Button}] Position[{clickEvent.Position}")
                    )
                )
            );

            static IEnumerable<IMenuItem> CreateMenuItems()
            {
                return new IMenuItem[]
                {
                    new MenuItem("Item0", () => Debug.Log("Item0")),
                    new MenuItem("Item1(Checked)", () => Debug.Log("Item1(Checked)")) { isChecked = true },
                    new MenuItem("Item2(Disabled)"),
                    new MenuItemSeparator(""),
                    new MenuItem("Sub0/Item0", () => Debug.Log("Sub0/Item0")),
                    new MenuItem("Sub0/Item1(Checked)", () => Debug.Log("Sub0/Item1(Checked)")) { isChecked = true },
                    new MenuItem("Sub0/Item2(Disabled)"),
                    new MenuItemSeparator("Sub0/"),
                    new MenuItem("Sub0/Sub1/Item0", () => Debug.Log("Sub0/Sub1/Item0")),
                    new MenuItem("Sub0/Sub1/Item1(Checked)", () => Debug.Log("Sub0/Sub1/Item1(Checked)")) { isChecked = true },
                    new MenuItem("Sub0/Sub1/Item2(Disabled)")
                };
            }
        }
    }
}