using System.Linq;

namespace RosettaUI.Example
{
    public class ExampleTemplate
    {
        public static SpaceElement BlankLine() => UI.Space().SetHeight(10f);
        
        public static string UIFunctionStr(string functionName) => $"<b>{nameof(UI)}.{functionName}()</b>";

        public static Element TitleIndent(string title, params Element[] elements) =>
            UI.Column(
                UI.Label(title),
                UI.Indent(elements),
                BlankLine()
            );
        
        public static Element UIFunctionColumn(string functionName, params Element[] elements) =>
            TitleIndent(UIFunctionStr(functionName), elements);
        
        public static Element UIFunctionRow(string functionName, params Element[] elements) =>
            UI.Column(
                UI.Row(
                    new Element[]{UI.Label(UIFunctionStr(functionName), LabelType.Prefix)}.Concat(elements)
                ),
                BlankLine()
            );

        public static Element UIFunctionColumnBox(string functionName, params Element[] elements) =>
            UIFunctionColumn(functionName, UI.Box(elements));
    }
}