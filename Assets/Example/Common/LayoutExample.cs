using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public class LayoutExample : MonoBehaviour, IElementCreator
    {
        public int intValue;
        public float floatValue;
        public string stringValue;
        public List<int> list;

        public Element CreateElement()
        {
            var scrollViewItemCount = 100;
            
            return UI.Column(
                UI.Row(
                    UI.Page(
                        UI.Label("<b>UI.Box()</b>"),
                        UI.Box(
                            UI.Label("box style frame")
                        ),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.Row()</b>"),
                        UI.Box(
                            UI.Row(
                                UI.Label("Element0"),
                                UI.Label("Element1"),
                                UI.Label("Element2")
                            )
                        ),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.Column()</b>"),
                        UI.Box(
                            UI.Column(
                                UI.Label("Element0"),
                                UI.Label("Element1"),
                                UI.Label("Element2")
                            )
                        ),
                        UI.Space().SetHeight(10f),
                        UI.Label("<b>UI.Fold()</b>"),
                        UI.Box(
                            UI.Fold("Fold0",
                                UI.Fold("Fold1",
                                    UI.Fold("Fold2",
                                        UI.Label("Element")
                                    )
                                )
                            )
                        ),
                        UI.Space().SetHeight(10f),
                        
                        UI.Label("<b>UI.Indent()</b>"),
                        UI.Box(
                            UI.Label("No indent"),
                            UI.Indent(
                                UI.Label("Indent1"),
                                UI.Indent(
                                    UI.Label("Indent2")
                                )
                            )
                        )
                    ).SetMinWidth(500f),
                    
                    UI.Page(
                        UI.Label("<b>UI.Page()</b>"),
                        UI.Label("Adjust the width of the prefix labels."),
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
                    ),
                    
                    UI.Page(
                        UI.Label("<b>UI.ScrollView()</b>"),
                        UI.Slider(() => scrollViewItemCount),
                        UI.ScrollView(
                            UI.DynamicElementOnStatusChanged(
                                () => scrollViewItemCount,
                                count => UI.Column(Enumerable.Range(0, count)
                                    .Select(i => UI.Field("Count" + i, () => i.ToString())))
                            )
                        ).SetHeight(300f)
                    )
                ),
                UI.Space().SetHeight(10f),
                
                UI.Label("<b>Tips()</b>"),
                UI.Page(
                    UI.Label("Fold ignores one level of indentation for label alignment"),
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