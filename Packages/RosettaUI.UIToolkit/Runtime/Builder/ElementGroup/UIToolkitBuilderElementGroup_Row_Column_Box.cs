using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Row(Element element)
        {
            var row = CreateRowVisualElement();

            return Build_ElementGroupContents(row, element);
        }

        private static VisualElement CreateRowVisualElement()
        {
            var row = new VisualElement();
            row.AddToClassList(UssClassName.Row);
            return row;
        }

        private VisualElement Build_Column(Element element)
        {
            var column = new VisualElement();
            column.AddToClassList(UssClassName.Column);

            return Build_ElementGroupContents(column, element);
        }

        private VisualElement Build_Box(Element element)
        {
            var box = new Box();
            return Build_ElementGroupContents(box, element);
        }
    }
}