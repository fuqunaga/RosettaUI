using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Example
{
    public class LayoutExample : MonoBehaviour, IElementCreator
    {
        public List<int> list;
        public Element CreateElement()
        {
            return UI.Page(
                #if false
                UI.Field(() => list)
#else
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
                    UI.Fold("Fold",
                        UI.Fold("Fold2",
                            UI.Fold("Fold3",
                                UI.Label("Element")
                            )
                        )
                    )
                ),
                UI.Space().SetHeight(10f),
                
                UI.Label("<b>UI.Space()</b>"),
                UI.Row(
                    UI.Label($"{nameof(UI.Space)} >"),
                    UI.Space().SetBackgroundColor(Color.gray),
                    UI.Label($"< {nameof(UI.Space)}")
                )
#endif
            );
        }
    }
}