using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace RosettaUI.Example
{
    public class LayoutExample : MonoBehaviour, IElementCreator
    {
        public int intValue;
        public float floatValue;
        public string stringValue;


        public Element CreateElement(LabelElement _)
        {
            return UI.Tabs(
                CreateMethods0(),
                CreateMethods1(),
                CreateCodes()
            );
        }

        private (string, Element) CreateMethods0()
        {
            return (ExampleTemplate.TabTitle("Methods0"),
                    UI.Page(
                        ExampleTemplate.UIFunctionColumnBox(nameof(UI.Column),
                            UI.Column(
                                UI.Label("Element0"),
                                UI.Label("Element1"),
                                UI.Label("Element2")
                            )
                        ),
                        ExampleTemplate.UIFunctionColumnBox(nameof(UI.Row),
                            UI.Row(
                                UI.Label("Element0"),
                                UI.Label("Element1"),
                                UI.Label("Element2")
                            )
                        ),
                        ExampleTemplate.UIFunctionColumnBox(nameof(UI.Fold),
                            UI.Fold("Fold0",
                                UI.Fold("Fold1",
                                    UI.Fold("Fold2",
                                        UI.Label("Element")
                                    )
                                )
                            )
                        ),
                        ExampleTemplate.UIFunctionColumnBox(nameof(UI.Indent),
                            UI.Label("No indent"),
                            UI.Indent(
                                UI.Label("Indent1"),
                                UI.Indent(
                                    UI.Label("Indent2")
                                )
                            )
                        ),
                        ExampleTemplate.UIFunctionColumnBox(nameof(UI.Box),
                            UI.Label("box style frame")
                        ),
                        ExampleTemplate.UIFunctionColumn(nameof(UI.Page),
                            UI.Label("Adjust the width of the prefix labels."),
                            UI.Column(
                                UI.Row(
                                    UI.Label("Page").SetWidth(80f),
                                    UI.Box(
                                        UI.Page(
                                            UI.Field(() => intValue),
                                            UI.Fold("Fold0",
                                                UI.Field(() => floatValue),
                                                UI.Fold("Fold1",
                                                    UI.Field(() => stringValue)
                                                ).Open()
                                            ).Open()
                                        ))
                                ),
                                UI.Row(
                                    UI.Label("Column").SetWidth(80f),
                                    UI.Box(
                                        UI.Column(
                                            UI.Field(() => intValue),
                                            UI.Fold("Fold0",
                                                UI.Field(() => floatValue),
                                                UI.Fold("Fold",
                                                    UI.Field(() => stringValue)
                                                ).Open()
                                            ).Open()
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
        }


        private (string, Element) CreateMethods1()
        {
            return (ExampleTemplate.TabTitle("Methods1"),
                    UI.Page(
                        CreateElement_Tabs(),
                        CreateElement_ScrollView()
                    )
                );
        }

        private (string, Element) CreateCodes()
        {
            return (ExampleTemplate.TabTitle("Codes"),
                    UI.Page(
                        CreateElement_FoldArgument(),
                        CreateElement_IgnoreIndentRule()
                    )
                );
        }

        Element CreateElement_Tabs()
        {
            var tab0Str = @"UI.Tabs(
    (""Title0"", Element), 
    (""Title1"", Element)
);";
            var tab1Str = @"UI.Tabs(
    (""Title0"", Func<Element>), 
    (""Title1"", Func<Element>)
);";

            return ExampleTemplate.UIFunctionColumn(nameof(UI.Tabs),
                UI.Box(
                    UI.Tabs(
                        ("Tab0", UI.Column(
                                UI.TextArea(null, () => tab0Str)
                            )
                        ),
                        ("Tab1", UI.Column(
                                UI.TextArea(null, () => tab1Str),
                                UI.HelpBox("Passing Func<Element> will delay the build of the UI until it is displayed",
                                    HelpBoxType.Info)
                            )
                        )
                    )
                )
            );
        }

        Element CreateElement_ScrollView()
        {
            // ReSharper disable once ConvertToConstant.Local
            var scrollViewItemCount = 50;
            const float width = 700f;
            const float height = 300f;

            return ExampleTemplate.UIFunctionColumn(nameof(UI.ScrollView),
                UI.Slider(() => scrollViewItemCount),
                ExampleTemplate.BlankLine(),
                UI.Box(
                    UI.Tabs(
                        ("Vertical",
                            () => UI.ScrollViewVertical(height,
                                UI.DynamicElementOnStatusChanged(
                                    () => scrollViewItemCount,
                                    count => UI.Column(
                                        Enumerable.Range(0, count)
                                            .Select(i =>
                                            {
                                                var str = i.ToString();
                                                return UI.Field("Item" + str, () => str);
                                            })
                                    )
                                )
                            )
                        ),
                        ("Horizontal",
                            () => UI.ScrollViewHorizontal(null,
                                UI.DynamicElementOnStatusChanged(
                                    () => scrollViewItemCount,
                                    count => UI.Row(
                                        Enumerable.Range(0, count).Select(i =>
                                            {
                                                var str = i.ToString();
                                                return UI.Column(
                                                    UI.Label("Item" + str),
                                                    UI.Field(null, () => str)
                                                );
                                            }
                                        )
                                    )
                                )
                            )
                        ),
                        ("VerticalAndHorizontal",
                            () => UI.ScrollViewVerticalAndHorizontal(null, height,
                                UI.DynamicElementOnStatusChanged(
                                    () => scrollViewItemCount,
                                    count =>
                                    {
                                        using (CollectionPool<List<Element>, Element>.Get(out var rows))
                                        {
                                            const int chunkSize = 5;
                                            var i = 0;
                                            for (var remain = count; remain > 0; remain -= chunkSize)
                                            {
                                                var size = Mathf.Min(chunkSize, remain);
                                                rows.Add(
                                                    UI.Row(
                                                        Enumerable.Range(0, size).Select(_ =>
                                                        {
                                                            var idx = i++;
                                                            var str = idx.ToString();
                                                            return UI.Field(
                                                                UI.Label("Item" + idx, LabelType.Standard),
                                                                () => str);
                                                        })
                                                    )
                                                );
                                            }

                                            return UI.Column(rows);
                                        }
                                    }
                                )
                            )
                        )
                    )
                ).SetWidth(width)
            );
        }
        
        Element CreateElement_FoldArgument()
        {
            return ExampleTemplate.CodeElementSets("<b>Fold argument</b>",
                (@"UI.Fold(
    UI.Field(""CustomBar"", () => intValue), 
    new[]
    {
        UI.Label(""Element"")
    }
);",
                    UI.Fold(
                        UI.Field("CustomBar", () => intValue),
                        new[]
                        {
                            UI.Label("Element")
                        }
                    )
                ),
                (@"UI.Fold(
    UI.Label(""Left""), UI.Label(""Right""), 
    new[]
    {
        UI.Label(""Element"")
    }
);",
                    UI.Fold(
                        UI.Label("Left"), UI.Label("Right"),
                        new[]
                        {
                            UI.Label("Element")
                        }
                    )
                )
            );
        }

        Element CreateElement_IgnoreIndentRule()
        {
            return ExampleTemplate.TitleIndent(
                "Fold/WindowLauncher ignores one level of indentation for label alignment",
                UI.Box(
                    UI.Label("No indent"),
                    UI.Fold(nameof(UI.Fold) + 0),
                    UI.WindowLauncher(nameof(UI.WindowLauncher) + 0, UI.Window(nameof(UI.Window))),
                    UI.Indent(
                        UI.Label("Indent1"),
                        UI.Fold(nameof(UI.Fold) + 1),
                        UI.WindowLauncher(nameof(UI.WindowLauncher) + 1, UI.Window(nameof(UI.Window))),
                        UI.Indent(
                            UI.Label("Indent2"),
                            UI.Fold(nameof(UI.Fold) + 2),
                            UI.WindowLauncher(nameof(UI.WindowLauncher) + 2, UI.Window(nameof(UI.Window)))
                        ).SetBackgroundColor(new Color(0.5f, 0.5f, 1f, 0.2f))
                    ).SetBackgroundColor(new Color(0.5f, 0.5f, 1f, 0.2f))
                )
            );
        }
    }
}