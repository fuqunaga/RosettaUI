using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public static class ExampleTemplate
    {
        public static SpaceElement BlankLine() => UI.Space().SetHeight(10f);
        
        public static string UIFunctionStr(string functionName) => $"<b>{nameof(UI)}.{functionName}()</b>";

        public static Element TitleIndent(string title, params Element[] elements) =>
            UI.Column(
                UI.Label(title),
                UI.Indent(elements),
                BlankLine()
            );

        public static Element TitlePage(string title, params Element[] elements) =>
            UI.Column(
                UI.Label(title),
                UI.Page(elements),
                BlankLine()
            );

        public static Element UIFunctionColumn(string functionName, params Element[] elements) =>
            TitleIndent(UIFunctionStr(functionName), elements);
        
        public static Element UIFunctionPage(string functionName, params Element[] elements) =>
            TitlePage(UIFunctionStr(functionName), elements);
        
        public static Element UIFunctionRow(string functionName, params Element[] elements) =>
            UI.Column(
                UI.Row(
                    new Element[]{UI.Label(UIFunctionStr(functionName), LabelType.Prefix)}.Concat(elements)
                ),
                BlankLine()
            );

        public static Element UIFunctionColumnBox(string functionName, params Element[] elements) =>
            UIFunctionColumn(functionName, UI.Box(elements));


        public static Element CodeElementSets((string, Element)[] pairs)
        {
            var texts = pairs.Select(pair => pair.Item1);
            var elements = pairs.Select(pair => pair.Item2);

            var code = string.Join("\n", texts);
            
            return UI.Row(
                    UI.TextArea(null, () => code)
                // UI.Column(
                //     texts.Select(t => UI.Field(null, () => t).SetHeight(32f))
                // )
                    //.SetBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                    .SetWidth(700f),
                UI.Space().SetWidth(30f),
                UI.Box(
                    UI.Page(
                        elements
                    )
                )
            );
        }
    }
}