using System.Linq;

namespace Comugi.UGUI.Builder
{
    public static class PaddingHint
    {
        // Top Level Row may needs LR padding
        public static bool IsTopLevelRow(this Element element)
        {
            var ret = true;
            var parent = element.parentGroup;
            while (parent != null)
            {
                if (parent is Row)
                {
                    ret = false;
                    break;
                }

                parent = parent.parentGroup;
            }

            return ret;
        }


        public static int GetIndent(this Element element)
        {
            var indent = 0;
            var parent = element.parentGroup;
            while(parent != null)
            {
                if (parent is FoldElement) indent++;
                parent = parent.parentGroup;
            }

            return indent;
        }

        public static bool IsPrefix(this Element label)
        {
            Element current = label;
            var parent = current.parentGroup;
            if (parent is FoldElement) return false;

            while (parent != null)
            {
                if (parent is Row row)
                {
                    if (row.Elements.FirstOrDefault() != current)
                    {
                        return false;
                    }
                }

                current = parent;
                parent = current.parentGroup;
            }

            return true;
        }
    }
}