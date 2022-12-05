using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Row(Element element)
        {
            var row = new Row();
            Bind_Row(element, row);
            return row;
        }

        private bool Bind_Row(Element element, VisualElement visualElement)
        {
            if (element is not RowElement rowElement || visualElement is not Row row) return false;

            return Bind_ElementGroupContents(element, visualElement);
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