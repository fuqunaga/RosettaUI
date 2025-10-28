using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.Example
{
    public class ListExample : MonoBehaviour, IElementCreator
    {
        public class NoCopyClass
        {
            public int intValue;
        }

        public class CloneableClass : ICloneable
        {
            public int intValue;
            public object Clone() => new CloneableClass { intValue = this.intValue };
        }

        public class CopyConstructorClass
        {
            public int intValue;

            public CopyConstructorClass()
            {
            }

            public CopyConstructorClass(CopyConstructorClass other)
            {
                intValue = other.intValue;
            }
        }
        
        public interface IListItem : IElementCreator
        {
        }

        public class IntItem : IListItem
        {
            public int intValue;
            
            public Element CreateElement(LabelElement label)
            {
                return UI.Field(label, () => intValue);
            }
        }
        
        public class FloatItem : IListItem
        {
            public float floatValue;
            
            public Element CreateElement(LabelElement label)
            {
                return UI.Slider(label, () => floatValue);
            }
        }
       


        public int[] intArray = { 1, 2, 3 };
        public List<int> intList = new() { 1, 2, 3 };

        public SimpleClass[] classArray =
        {
            new() { stringValue = "1" },
            new() { stringValue = "2" },
            new() { stringValue = "3" }
        };

        public List<SimpleClass> classList = new()
        {
            new SimpleClass { stringValue = "1" },
            new SimpleClass { stringValue = "2" },
            new SimpleClass { stringValue = "3" }
        };

        [NonReorderable] public int[] nonReorderableArray = { 1, 2, 3 };

        public Element CreateElement(LabelElement _)
        {
            return UI.Tabs(
                ExampleTemplate.UIFunctionTab(nameof(UI.List),
                    UI.List(() => intArray),
                    UI.List(() => intList),
                    UI.List(() => classArray),
                    UI.List(() => classList),
                    UI.HelpBox("Right-click on an item to open the menu", HelpBoxType.Info)
                ),
                ExampleTemplate.UIFunctionTab(nameof(UI.ListReadOnly),
                    UI.ListReadOnly(() => intArray),
                    UI.ListReadOnly(() => intList),
                    UI.ListReadOnly(() => classArray),
                    UI.ListReadOnly(() => classList)
                ),
                Code0(),
                Code1(),
                CustomInstance()
            );
        }


        private (string, Element) Code0()
        {
            var listViewOption = ListViewOption.Default;

            return ExampleTemplate.Tab(nameof(Code0),
                ExampleTemplate.CodeElementSets("CustomItemElement",
                    (@"UI.List(
    () => intArray,
    new ListViewOption
    {
        createItemElementFunc = (itemBinder, idx) => UI.Row(
            UI.Field($""Item {idx}"", itemBinder),
            UI.Button(""+"", () => intArray[idx]++),
            UI.Button(""-"", () => intArray[idx]--)
        )
    }
);",
                        UI.List(
                            () => intArray,
                            new ListViewOption
                            {
                                createItemElementFunc = (itemBinder, idx) => UI.Row(
                                    UI.Field($"Item {idx}", itemBinder),
                                    UI.Button("+", () => intArray[idx]++),
                                    UI.Button("-", () => intArray[idx]--)
                                )
                            }
                        )
                    )
                ),
                ExampleTemplate.CodeElementSets("Options",
                    (@"var listViewOption = ListViewOption.Default;

UI.Field(() => listViewOption).Open(),

UI.DynamicElementOnStatusChanged(
    () => listViewOption,
    _ => UI.List(() => intArray, listViewOption)
)",
                        UI.Column(
                            UI.Field(() => listViewOption).Open(),
                            UI.DynamicElementOnStatusChanged(
                                () => listViewOption,
                                _ => UI.List(() => intArray, listViewOption)
                            )
                        )
                    )
                )
            );
        }

        private (string, Element) Code1()
        {
            List<NoCopyClass> noCopyClassList = new()
            {
                new NoCopyClass { intValue = 1 },
            };

            List<CloneableClass> cloneableClassList = new()
            {
                new CloneableClass { intValue = 1 },
            };

            List<CopyConstructorClass> copyConstructorClassList = new()
            {
                new CopyConstructorClass { intValue = 1 },
            };

            return ExampleTemplate.Tab(nameof(Code1),
                ExampleTemplate.CodeElementSets("<b>Duplicate previous item</b>",
                    "If the item implements ICloneable or has a copy constructor, it will copy its previous item when added.",
                    (@"public class NoCopyClass
{
    public int intValue;
}

public class CloneableClass : ICloneable
{
    public int intValue;
    public object Clone() => new CloneableClass { intValue = this.intValue };
}

public class CopyConstructorClass
{
    public int intValue;

    public CopyConstructorClass() { }
    
    public CopyConstructorClass(CopyConstructorClass other)
    {
        intValue = other.intValue;
    }
}

UI.List(() => noCopyClassList);
UI.List(() => cloneableClassList);
UI.List(() => copyConstructorClassList);
",
                        UI.Column(
                            UI.List(() => noCopyClassList),
                            UI.List(() => cloneableClassList),
                            UI.List(() => copyConstructorClassList)
                        )
                    )
                ),
                ExampleTemplate.CodeElementSets("<b>Attribute</b>",
                    (@"[NonReorderable]
public int[] nonReorderableArray;

UI.List(() => nonReorderableArray);
",
                        UI.List(() => nonReorderableArray)))
            );
        }
        
        
        private static (string label, Element element) CustomInstance()
        {
            var list = new List<IListItem>
            {
                new IntItem { intValue = 1 },
                new FloatItem() { floatValue = 2f },
                new IntItem { intValue = 3 },
            };
            
            return ExampleTemplate.Tab("CustomInstance",
                ExampleTemplate.CodeElementSets("Custom instance with interface",
                    (@"UI.List(
    () => list,
    ListViewOption.OfType(list).SetCreateItemInstanceFunc(CreateNewItem)
)

IListItem CreateNewItem(List<IListItem> _, int index)
{
    return index % 2 == 0 
        ? new IntItem() 
        : new FloatItem();
}
",
                        UI.List(
                            () => list,
                            ListViewOption.OfType(list).SetCreateItemInstanceFunc(CreateNewItem)
                        )
                    )
                )
            );

            IListItem CreateNewItem(List<IListItem> _, int index)
            {
                return index % 2 == 0 
                    ? new IntItem() 
                    : new FloatItem();
            }
        }
    }
}