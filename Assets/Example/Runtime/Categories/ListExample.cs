using System;
using System.Collections.Generic;
using RosettaUI.UndoSystem;
using UnityEngine;

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
            float Value { get; set; }
        }

        public class IntItem : IListItem
        {
            public int intValue;

            public float Value
            {
                get => intValue; 
                set => intValue = (int)value;
            }
            
            public Element CreateElement(LabelElement label)
            {
                return UI.Field(label, () => intValue);
            }
        }
        
        public class FloatItem : IListItem
        {
            public float Value { get; set; }
            
            public Element CreateElement(LabelElement label)
            {
                return UI.Slider(label, () => Value);
            }
        }

        public class StringOrSliderItem : IElementCreator
        {
            public string stringValue;
            public float floatValue;
            public bool useSlider;
            
            public Element CreateElement(LabelElement label)
            {
                return UI.Row(
                    UI.DynamicElementOnStatusChanged(
                        () => useSlider,
                        _ => useSlider
                            ? UI.Slider(() => floatValue)
                            : UI.Field(() => stringValue)
                    ),
                    UI.Button("Change Field",
                        () => useSlider = !useSlider)
                );
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
                CustomInstance(),
                CustomUndo()
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
            var list = new IListItem[]
            {
                new IntItem { Value = 1 },
                new FloatItem { Value = 2f },
                new IntItem { Value = 3 },
            };

            ListViewOption.OfType(list);

            return ExampleTemplate.Tab(nameof(CustomInstance),
                ExampleTemplate.CodeElementSets("Custom instance",
                    (@"UI.List(
    () => list,
    ListViewOption.OfType(list).SetCreateItemInstanceFunc(CreateNewItem)
)

IListItem CreateNewItem(IReadOnlyList<IListItem> _, int index)
{
    var previousValue = index > 0 
        ? list[index - 1].Value
        : 0f;

    return index % 2 == 0
        ? new IntItem() { Value = previousValue }
        : new FloatItem() { Value = previousValue };
}
",
                        UI.List(
                            () => list,
                            ListViewOption.OfType(list).SetCreateItemInstanceFunc(CreateNewItem)
                        )
                    )
                )
            );

            IListItem CreateNewItem(IReadOnlyList<IListItem> _, int index)
            {
                var previousValue = index > 0 
                    ? list[index - 1].Value
                    : 0f;

                return index % 2 == 0
                    ? new IntItem() { Value = previousValue }
                    : new FloatItem() { Value = previousValue };
            }
        }

        private static (string label, Element element) CustomUndo()
        {
            var list0 = CreateList();
            var list1 = CreateList();
            
            return ExampleTemplate.Tab(nameof(CustomUndo),
                ExampleTemplate.CodeElementSets("Custom Undo",
                    "When restoring a list item with Undo/Redo, only the value of the existing element is recreated; other parameters are not restored.\n" +
                    "With the Restore function, even if you edit floatValue using the \"Change Field\" button and then delete the item, it will be restored with Undo.",
                    (@"UI.List(""Without Restore function"",
    () => list0
),
UI.List(""With Restore function"",
    () => list1,
    ListViewOption.OfType(list1).SetItemRestoreFunc(CreateRestoreFunc)
)

            
Func<StringOrSliderItem> CreateRestoreFunc(StringOrSliderItem itemBeforeRemoval)
{
    return () => new StringOrSliderItem
    {
        stringValue = itemBeforeRemoval.stringValue,
        floatValue = itemBeforeRemoval.floatValue,
        useSlider = itemBeforeRemoval.useSlider
    };
}
",
                        UI.Column(
                            UI.List("Without Restore function",
                                () => list0
                            ),
                            UI.List("With Restore function",
                                () => list1,
                                ListViewOption.OfType(list1).SetItemRestoreFunc(CreateRestoreFunc)
                            )
                        )
                    )
                )
            );
            
            Func<StringOrSliderItem> CreateRestoreFunc(StringOrSliderItem itemBeforeRemoval)
            {
                return () => new StringOrSliderItem
                {
                    stringValue = itemBeforeRemoval.stringValue,
                    floatValue = itemBeforeRemoval.floatValue,
                    useSlider = itemBeforeRemoval.useSlider
                };
            }

            static List<StringOrSliderItem> CreateList()
            {
                return new List<StringOrSliderItem>
                {
                    new() { stringValue = "1st", floatValue = 0.1f },
                    new() { stringValue = "2nd", floatValue = 0.2f },
                    new() { stringValue = "3rd", floatValue = 0.3f },
                };
            }
        }
    }
}