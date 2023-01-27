using System.Linq;

namespace RosettaUI.Builder
{
    public static class LayoutHint
    {
        public static bool IsIndentOrigin(Element element) => element is PageElement or WindowElement;

        /// <summary>
        /// 子のマイナスインデントを許容しないElement
        /// マイナスインデントは親の左側をはみ出るので Box や Fold では絵的にまずい
        /// </summary>
        public static bool IsIndentWall(Element element)
            => element is BoxElement or FoldElement or TabsElement or ScrollViewElement || IsIndentOrigin(element);

        /// <summary>
        /// Foldなど、ラベルの左側にあるElementは1つ目のインデントを無視することで通常のラベルと並びが揃う
        /// この1つ目のインデントがあるかどうかの判定
        /// </summary>
        public static bool CanMinusIndent(this Element element)
        {
            for (var e = element;
                 e.Parent != null && !IsIndentWall(e.Parent);
                 e = e.Parent)
            {
                switch (e.Parent)
                {
                    // Row内なら一番左のみ有効
                    case RowElement row when row.Children.FirstOrDefault() != e:
                        return false;
                    case IndentElement or ListViewItemContainerElement:
                        return true;
                }
            }

            return false;
        }

        public static bool IsPrefix(this LabelElement label)
        {
            return label.labelType == LabelType.Prefix && label.IsMostLeftLabel();
        }

        public static bool IsMostLeftLabel(this LabelElement element)
        {
            for (Element e = element;
                 e.Parent != null && !IsIndentWall(e.Parent);
                 e = e.Parent)
            {
                switch (e.Parent)
                {
                    case RowElement row when 
                        row.Children.FirstOrDefault() != e 
                        && row.FirstLabel() != element:
                        return false;
                    
                    case CompositeFieldElement c when c.Label != e:
                        return false;
                }
            }

            return true;
        }
    }
}