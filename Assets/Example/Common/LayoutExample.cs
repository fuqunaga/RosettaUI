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


        public Element CreateExampleElement(string title, params Element[] contents)
        {
            return UI.Column(
                UI.Label($"<b>{title}</b>"),
                UI.Page(
                    UI.Box(contents)
                ),
                UI.Space().SetHeight(10f)
            );
        }
        

        public Element CreateElement()
        {
            var scrollViewItemCount = 50;

            return UI.Column(
                UI.Row(
                    UI.Column(
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
                    ),
                    ExampleTemplate.UIFunctionColumn(nameof(UI.ScrollView),
                        UI.Slider(() => scrollViewItemCount),
                        ExampleTemplate.BlankLine(),
                        ExampleTemplate.TitleIndent("Vertical",
                            UI.Box(
                                UI.ScrollViewVertical(300f,
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
                            )
                        ),
                        ExampleTemplate.TitleIndent("Horizontal",
                            UI.Box(
                                UI.ScrollViewHorizontal(700f,
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
                            )
                        ),

                        ExampleTemplate.TitleIndent("VerticalAndHorizontal",
                            UI.Box(
                                UI.ScrollViewVerticalAndHorizontal(700f, 300f,
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
                    )
                ),
                ExampleTemplate.BlankLine(),

                ExampleTemplate.CodeElementSets("<b>Fold argument</b>",
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
                ),
                ExampleTemplate.TitleIndent("Fold ignores one level of indentation for label alignment",
                    UI.Box(
                        UI.Label("No indent"),
                        UI.Fold("Fold0"),
                        UI.Indent(
                            UI.Label("Indent1"),
                            UI.Fold("Fold1"),
                            UI.Indent(
                                UI.Label("Indent2"),
                                UI.Fold("Fold1")
                            ).SetBackgroundColor(new Color(0.5f, 0.5f, 1f, 0.2f))
                        ).SetBackgroundColor(new Color(0.5f, 0.5f, 1f, 0.2f))
                    )
                )
            );
        }
    }
}