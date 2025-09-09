using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// 選択中のすべてのControlPointを囲むRect
    /// </summary>
    public class SelectedControlPointsRect : VisualElement
    {
        private const string UssClassName = "rosettaui-animation-curve-editor__selected-control-points-rect";
        private const string HandleClassName = UssClassName + "__handle";
        private const string HandleHorizontalClassName = HandleClassName + "--horizontal";
        private const string HandleVerticalClassName = HandleClassName + "--vertical";
        private const string HandleTopClassName = HandleClassName + "--top";
        private const string HandleBottomClassName = HandleClassName + "--bottom";
        private const string HandleLeftClassName = HandleClassName + "--left";
        private const string HandleRightClassName = HandleClassName + "--right";
        private const string HandleLineClassName = HandleClassName + "__center-line";
        
        
        private readonly Func<IEnumerable<ControlPoint>> _getSelectedControlPoints;

        public Rect Rect
        {
            get
            {
                using var _ = ListPool<ControlPoint>.Get(out var list);
                list.AddRange(_getSelectedControlPoints?.Invoke() ?? Enumerable.Empty<ControlPoint>());
                
                if (list.Count == 0)
                {
                    return default;
                }

                var firstPos = list[0].LocalPosition;

                var min = firstPos;
                var max = firstPos;
                foreach (var pos in list.Skip(1).Select(cp => cp.LocalPosition))
                {
                    min = Vector2.Min(min, pos);
                    max = Vector2.Max(max, pos);
                }
                
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }


        public SelectedControlPointsRect(Func<IEnumerable<ControlPoint>> getSelectedControlPoints)
        {
            _getSelectedControlPoints = getSelectedControlPoints;
            AddToClassList(UssClassName);

          
            // 4辺のハンドル
            var topHandle = CreateHandle(HandleTopClassName, HandleHorizontalClassName, CursorType.ResizeVertical);
            Add(topHandle);
            
            var bottomHandle = CreateHandle(HandleBottomClassName, HandleHorizontalClassName, CursorType.ResizeVertical);
            Add(bottomHandle);
            
            var leftHandle = CreateHandle(HandleLeftClassName, HandleVerticalClassName, CursorType.ResizeHorizontal);
            Add(leftHandle);
            
            var rightHandle = CreateHandle(HandleRightClassName, HandleVerticalClassName, CursorType.ResizeHorizontal);
            Add(rightHandle);
            
            return;
            
            
            VisualElement CreateHandle(string classNameDir, string classNameHv, CursorType cursorType)
            {
                var handle = new VisualElement();
                handle.AddToClassList(HandleClassName);
                handle.AddToClassList(classNameDir);
                handle.AddToClassList(classNameHv);
                handle.style.cursor = CursorHolder.Get(cursorType);
                
                var line = new VisualElement();
                line.AddToClassList(HandleLineClassName);
                handle.Add(line);
                
                return handle;
            }
        }
        
        public void UpdateView()
        {
            var rect = Rect;
            if (rect == default)
            {
                this.Hide();
                return;
            }
            
            this.Show();
            
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
        }
    }
}