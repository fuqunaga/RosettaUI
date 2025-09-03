using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// 矩形選択範囲
    /// </summary>
    public class SelectionRect : VisualElement
    {
        private const string UssClassName = "rosettaui-animation-curve-editor__selection-rect";

        private Vector2 _pinnedPosition;
        private Vector2 _movablePosition;

        public bool IsVisible => style.display == DisplayStyle.Flex;
        
        public SelectionRect()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(UssClassName);
        }
        
        public Rect Rect => new(
            Mathf.Min(_pinnedPosition.x, _movablePosition.x),
            Mathf.Min(_pinnedPosition.y, _movablePosition.y),
            Mathf.Abs(_pinnedPosition.x - _movablePosition.x),
            Mathf.Abs(_pinnedPosition.y - _movablePosition.y)
        );
        
        public void SetPinnedPosition(Vector2 pinnedPosition)
        {
            _pinnedPosition = pinnedPosition;
            _movablePosition = pinnedPosition;
        }
        
        public void SetMovablePosition(Vector2 movablePosition)
        {
            _movablePosition = movablePosition;
            UpdateView();
        }

        private void UpdateView()
        {
            var rect = Rect;
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
        }

        public void Show(Vector2 pinnedPosition)
        {
            SetPinnedPosition(pinnedPosition);
            UpdateView();
            this.Show();
        }
    }
}