using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        
        #region Row/Column/Box/ScrollView/Indent

        public static RowElement Row(params Element[] elements) => Row(elements.AsEnumerable());

        public static RowElement Row(IEnumerable<Element> elements)
        {
            return new RowElement(elements);
        }

        public static ColumnElement Column(params Element[] elements) => Column(elements.AsEnumerable());

        public static ColumnElement Column(IEnumerable<Element> elements)
        {
            return new ColumnElement(elements);
        }

        public static BoxElement Box(params Element[] elements) => Box(elements.AsEnumerable());

        public static BoxElement Box(IEnumerable<Element> elements)
        {
            return new BoxElement(elements);
        }

        public static ScrollViewElement ScrollView(params Element[] elements) => ScrollView(elements.AsEnumerable());

        public static ScrollViewElement ScrollView(IEnumerable<Element> elements)
        {
            return new ScrollViewElement(elements);
        }

        public static IndentElement Indent(params Element[] elements) => Indent(elements.AsEnumerable());
        
        public static IndentElement Indent(IEnumerable<Element> elements)
        {
            return new IndentElement(elements);
        }
        #endregion


        #region Fold

        public static FoldElement Fold(LabelElement label, params Element[] elements) => Fold(label,  null, elements);

        public static FoldElement Fold(LabelElement label, IEnumerable<Element> elements) => Fold((Element)label, elements);

        public static FoldElement Fold(Element barLeft, Element barRight, IEnumerable<Element> elements)
        {
            var bar = (barLeft != null || barRight != null)
                ? Row(barLeft, Space(), barRight)
                : null;

            return Fold(bar, elements);
        }

        public static FoldElement Fold(Element bar, IEnumerable<Element> elements)
        {
            return new FoldElement(bar, elements);
        }

        #endregion
    }
}