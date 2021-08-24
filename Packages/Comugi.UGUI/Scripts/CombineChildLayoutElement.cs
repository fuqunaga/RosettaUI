using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Comugi.UGUI.Builder
{
    /// <summary>
    /// Child のRectTransformとLayoutElementのから自身のLayoutElementを求める
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CombineChildLayoutElement : UIBehaviour, ILayoutElement
    {
        public bool horizontal;
        public bool vertical;
        public float horizontalOffset;
        public float verticalOffset;
        public int _layoutPriority;

        protected Vector2 _min = Vector2.one * -1f;
        protected Vector2 _preferred = Vector2.one * -1f;
        protected Vector2 _flexible = Vector2.one * -1f;


        #region ILayoutElement

        public float minWidth => _min.x;

        public float preferredWidth => _preferred.x;

        public float flexibleWidth => _flexible.x;

        public float minHeight => _min.y;

        public float preferredHeight => _preferred.y;

        public float flexibleHeight => _flexible.y;

        public int layoutPriority => _layoutPriority;


        static List<RectTransform> rectTransformBuffer = new List<RectTransform>();

        public void CalculateLayoutInputHorizontal()
        {
            if (horizontal) CalculateLayoutInput(0, horizontalOffset);
        }

        public void CalculateLayoutInputVertical()
        {
            if (vertical) CalculateLayoutInput(1, verticalOffset);
        }


        void CalculateLayoutInput(int axis, float offset)
        {
            rectTransformBuffer.Clear();
            GetComponentsInChildren(rectTransformBuffer);

            var min = -1f;
            var preferred = -1f;
            var flexible = -1f;

            foreach (var rt in rectTransformBuffer.Where(r => r.transform != transform))
            {
                min = Mathf.Max(min, LayoutUtility.GetMinSize(rt, axis));
                preferred = Mathf.Max(preferred, LayoutUtility.GetPreferredSize(rt, axis));
                flexible = Mathf.Max(flexible, LayoutUtility.GetFlexibleSize(rt, axis));
            }

            if (min != -1f) min += offset;
            if (preferred != -1f) preferred += offset;
            // flexible does not apply offset because the size units are different

            _min[axis] = min;
            _preferred[axis] = preferred;
            _flexible[axis] = flexible;
        }

        #endregion
    }
}