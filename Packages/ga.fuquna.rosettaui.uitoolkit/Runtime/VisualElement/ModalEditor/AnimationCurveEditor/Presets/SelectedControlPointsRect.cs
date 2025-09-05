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
                
                var firstPos = list[0].GetLocalPosition();

                var min = firstPos;
                var max = firstPos;
                foreach (var pos in list.Skip(1).Select(cp => cp.GetLocalPosition()))
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