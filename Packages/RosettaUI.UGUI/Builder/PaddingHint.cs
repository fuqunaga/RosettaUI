using System.Linq;

namespace RosettaUI.UGUI.Builder
{
    public static class PaddingHint
    {
        // Top Level Row may needs LR padding
        public static bool IsTopLevelRow(this Element element)
        {
            var ret = true;
            var parent = element.parent;
            while (parent != null)
            {
                if (parent is Row)
                {
                    ret = false;
                    break;
                }

                parent = parent.parent;
            }

            return ret;
        }


        public static int GetIndent(this Element element)
        {
            var indent = 0;
            var parent = element.parent;
            while(parent != null)
            {
                if (parent is FoldElement) indent++;
                parent = parent.parent;
            }

            return indent;
        }
    }
}