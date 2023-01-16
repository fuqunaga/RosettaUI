using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class ListExample : MonoBehaviour, IElementCreator
    {
        public int[] intArray = {1, 2, 3};
        public List<int> intList = new[] {1, 2, 3}.ToList();
        public SimpleClass[] classArray = {new() {stringValue = "1"}, new() {stringValue = "2"}, new() {stringValue = "2"}};
        public List<SimpleClass> classList = new SimpleClass[]{new() {stringValue = "1"}, new() {stringValue = "2"}, new() {stringValue = "2"}}.ToList();

        [NonReorderable]
        public int[] nonReorderableArray = {1,2,3};
        
        public Element CreateElement(LabelElement _)
        {
            return UI.Column(
                UI.HelpBox("Right-click on an item to open the menu", HelpBoxType.Info),
                UI.Row(
                    ExampleTemplate.UIFunctionPage(nameof(UI.List),
                        UI.List(() => intArray),
                        UI.List(() => intList),
                        UI.List(() => classArray),
                        UI.List(() => classList)
                    ),
                    ExampleTemplate.UIFunctionPage(nameof(UI.ListReadOnly),
                        UI.ListReadOnly(() => intArray),
                        UI.ListReadOnly(() => intList),
                        UI.ListReadOnly(() => classArray),
                        UI.ListReadOnly(() => classList)
                    )
                ),
                ExampleTemplate.CodeElementSets("<b>CustomItemElement</b>",
                    new[]
                    {
                        (@"UI.List(() => intArray,
    (itemBinder, idx) => UI.Row(
        UI.Field($""Item {idx}"", itemBinder),
        UI.Button(""+"", () => intArray[idx]++),
        UI.Button(""-"", () => intArray[idx]--)
);",
                            (Element) UI.List(() => intArray,
                                (itemBinder, idx) => UI.Row(
                                    UI.Field($"Item {idx}", itemBinder),
                                    UI.Button("+", () => intArray[idx]++),
                                    UI.Button("-", () => intArray[idx]--)
                                )
                            )
                        )
                    }
                ),
                ExampleTemplate.CodeElementSets("<b>Options</b>",
                    new[]
                    {
                        ("UI.List(() => intArray, new ListViewOption(reorderable: false, fixedSize: false));", UI.List(() => intArray, new ListViewOption(reorderable: false, fixedSize: false)) as Element),
                        ("UI.List(() => intArray, new ListViewOption(reorderable: true,  fixedSize: true));", UI.List(() => intArray, new ListViewOption(reorderable: true, fixedSize: true)))
                    }
                ),
                ExampleTemplate.CodeElementSets("<b>Attribute</b>",
                    new[]
                    {
                        (@"[NonReorderable]
public int[] nonReorderableArray;

UI.List(() => nonReorderableArray);",
                            UI.List(() => nonReorderableArray) as Element)
                    }
                )
            );
        }
    }
}