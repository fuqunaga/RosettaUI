using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Comugi.UGUI.Builder
{
    /// <summary>
    /// Copy LayoutElement Value from target
    /// </summary>
    public class CopyLayoutElement : UIBehaviour, ILayoutElement
    {
        public RectTransform target;
        public bool horizontal;
        public bool vertical;

        public Vector2 minOffset;
        public Vector2 preferredOffset;
        public Vector2 flexibleOffset;


        protected Vector2 min = Vector2.one * -1f;
        protected Vector2 preferred = Vector2.one * -1f;
        protected Vector2 flexible = Vector2.one * -1f;


        public int _layoutPriority = 0;


        #region ILayoutElement

        public float minWidth => min.x;

        public float preferredWidth => preferred.x;

        public float flexibleWidth => flexible.x;

        public float minHeight => min.y;

        public float preferredHeight => preferred.y;

        public float flexibleHeight => flexible.y;

        public int layoutPriority => _layoutPriority;

        public void CalculateLayoutInputHorizontal()
        {
            if (horizontal)
            {
                CalculateLayoutInput(0);
            }
        }

        public void CalculateLayoutInputVertical()
        {
            if (vertical)
            {
                CalculateLayoutInput(1);
            }
        }

        void CalculateLayoutInput(int axis)
        {
            min[axis] = LayoutUtility.GetMinSize(target, axis) + minOffset[axis];
            preferred[axis] = LayoutUtility.GetPreferredSize(target, axis) + preferredOffset[axis];
            flexible[axis] = LayoutUtility.GetFlexibleSize(target, axis) + flexibleOffset[axis];
        }

        #endregion
    }
}