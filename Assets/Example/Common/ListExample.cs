using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Element CreateElement()
        {
            return UI.Row(
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
                ),
                ExampleTemplate.TitlePage("<b>Tips</b>",
                    UI.List("CustomItemElement",
                        () => intArray,
                        (itemBinder, idx) => UI.Row(
                            UI.Field("Item " + idx, itemBinder),
                            UI.Button("+", () => intArray[idx]++),
                            UI.Button("-", () => intArray[idx]--)
                        )
                    ),
                    UI.List(
                        "NonReorderable",
                        () => intList,
                        new ListViewOption(reorderable:false, fixedSize:false)),
                    UI.List(
                        "FixedSize",
                        () => classArray,
                        new ListViewOption(reorderable:true, fixedSize:true))
                )
            );
        }
    }
}