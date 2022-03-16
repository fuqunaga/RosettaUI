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
            return
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
                    ),
                    ExampleTemplate.TitlePage(ExampleTemplate.UIFunctionStr(nameof(UI.List)) + " Custom Item Element", 
                        UI.List(() => intArray, CreateItemElementIntArray),
                        UI.List(() => intList, CreateItemElementIntList),
                        UI.List(() => classArray, CreateItemElementSimpleClass),
                        UI.List(() => classList, CreateItemElementSimpleClass)
                    ),
                    ExampleTemplate.TitlePage(ExampleTemplate.UIFunctionStr(nameof(UI.ListReadOnly)) + " Custom Item Element",
                        UI.ListReadOnly(() => intArray, CreateItemElementIntArray),
                        UI.ListReadOnly(() => intList, CreateItemElementIntList),
                        UI.ListReadOnly(() => classArray, CreateItemElementSimpleClass),
                        UI.ListReadOnly(() => classList, CreateItemElementSimpleClass)
                    )
                );
            

            Element CreateItemElementIntArray(IBinder itemBinder, int idx)
            {
                return UI.Row(
                    UI.Field("Item " + idx, itemBinder),
                    UI.Button("+", () => intArray[idx]++),
                    UI.Button("-", () => intArray[idx]--)
                );
            }
            
            Element CreateItemElementIntList(IBinder itemBinder, int idx)
            {
                return UI.Row(
                    UI.Field("Item " + idx, itemBinder),
                    UI.Button("+", () => intList[idx]++),
                    UI.Button("-", () => intList[idx]--)
                );
            }

            Element CreateItemElementSimpleClass(IBinder itemBinder, int idx)
            {
                return UI.Slider("Item " + idx, itemBinder, new SliderOption());
            }
        }
    }
}