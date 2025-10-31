using System.Linq;
using UnityEngine;

namespace RosettaUI.Example
{
    public static class ExampleTemplate
    {
        public static float codeBoxMaxHeight = 250f;
        
        public static SpaceElement BlankLine() => UI.Space().SetHeight(10f);

        public static string Bold(string str) => $"<b>{str}</b>";
        public static string FunctionStr(string className, string functionName) => $"{className}.{functionName}()";
        public static string UIFunctionStr(string functionName) => FunctionStr(nameof(UI), functionName);
        public static string TabTitle(string title) => $"● {title} ";

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

        public static (string, Element) Tab(string title, params Element[] elements) =>
            (TabTitle(title), UI.Page(elements));

        public static Element FunctionColumn(string className, string functionName, params Element[] elements) =>
            TitleIndent(FunctionStr(className, functionName), elements);

        public static Element UIFunctionColumn(string functionName, params Element[] elements) =>
            TitleIndent(UIFunctionStr(functionName), elements);

        public static Element UIFunctionPage(string functionName, params Element[] elements) =>
            TitlePage(UIFunctionStr(functionName), elements);

        public static Element UIFunctionRow(string functionName, params Element[] elements) =>
            UI.Column(
                UI.Row(
                    new Element[] { UI.Label(UIFunctionStr(functionName), LabelType.Prefix) }.Concat(elements)
                ),
                BlankLine()
            );

        public static Element UIFunctionColumnBox(string functionName, params Element[] elements) =>
            UIFunctionColumn(functionName, UI.Box(elements));

        public static (string, Element) UIFunctionTab(string functionName, params Element[] elements) =>
            Tab(UIFunctionStr(functionName), elements);


        public static Element CodeBox(string code, float? maxHeight = null)
        {
            var highlightedCode = SyntaxHighlighter.Highlight(code);
            Element innerElement = maxHeight.HasValue
                ? UI.ScrollViewVertical(
                    maxHeight.Value,
                    UI.Label(highlightedCode)
                )
                : UI.Label(highlightedCode);
            
            return UI.Box(innerElement).SetBackgroundColor(new Color(0f, 0f, 0f, 0.7f));
        }

        public static Element CodeElementSets(string title, params (string, Element)[] pairs)
            => CodeElementSets(title, null, pairs);

        public static Element CodeElementSets(string title, string description, params (string, Element)[] pairs)
        {
            var texts = pairs.Select(pair => pair.Item1);
            var elements = pairs.Select(pair => pair.Item2);

            var code = string.Join("\n", texts);

            return TitleIndent(Bold(title),
                string.IsNullOrEmpty(description) ? null : UI.Label(description),
                UI.Column(
                    CodeBox(code, codeBoxMaxHeight),
                    UI.Space().SetWidth(30f),
                    UI.Box(
                        UI.Page(
                            elements
                        )
                    )
                )
            );
        }

        public static (string, Element) CodeElementSetsTab(string title, params (string, Element)[] pairs) =>
            CodeElementSetsTab(title, title, pairs);

        public static (string, Element) CodeElementSetsWithDescriptionTab(string title, string description,
            params (string, Element)[] pairs) =>
            CodeElementSetsTab(title, title, description, pairs);

        public static (string, Element) CodeElementSetsTab(string tabTitle, string codeTitle,
            params (string, Element)[] pairs) =>
            CodeElementSetsTab(tabTitle, codeTitle, null, pairs);

        public static (string, Element) CodeElementSetsTab(string tabTitle, string codeTitle, string description,
            params (string, Element)[] pairs) =>
            (TabTitle(tabTitle), UI.Page(CodeElementSets(codeTitle, description, pairs)));


        public static Element TitleDescriptionElement(string title, string description, params Element[] elements)
        {
            return TitleIndent(Bold(title),
                UI.Label(description),
                BlankLine(),
                UI.Box(
                    UI.Page(
                        elements
                    )
                )
            );
        }
    }
}