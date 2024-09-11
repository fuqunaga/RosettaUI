using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static RowElement Row(params Element[] elements) => Row(elements.AsEnumerable());

        public static RowElement Row(IEnumerable<Element> elements) => new(elements);

        
        public static ColumnElement Column(params Element[] elements) => Column(elements.AsEnumerable());

        public static ColumnElement Column(IEnumerable<Element> elements) => new(elements);

        
        public static BoxElement Box(params Element[] elements) => Box(elements.AsEnumerable());

        public static BoxElement Box(IEnumerable<Element> elements) => new(elements);

        public static IndentElement Indent(params Element[] elements) => Indent(elements.AsEnumerable());

        public static IndentElement Indent(IEnumerable<Element> elements, int level = 1) => new(elements, level);

        
        public static PageElement Page(params Element[] elements) => Page(elements.AsEnumerable());

        public static PageElement Page(IEnumerable<Element> elements) => new(new[] {Indent(elements)});


        public static FoldElement Fold(LabelElement label, params Element[] elements) =>
            Fold((Element) label, elements.AsEnumerable());

        public static FoldElement Fold(LabelElement label, IEnumerable<Element> elements) =>
            Fold((Element) label, elements);

        public static FoldElement Fold(Element barLeft, Element barRight, IEnumerable<Element> elements) =>
            Fold(Row(barLeft, Space(), barRight), elements);

        public static FoldElement Fold(Element bar, IEnumerable<Element> elements) => new(bar, new[] {Indent(elements, 2).SetFlexShrink(1f)});
    }
}