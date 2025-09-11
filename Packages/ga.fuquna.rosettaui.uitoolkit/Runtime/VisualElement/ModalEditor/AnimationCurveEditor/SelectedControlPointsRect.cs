using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
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
        #region RectSide enum and helper methods

        private enum RectSide
        {
            Top,
            Bottom,
            Left,
            Right
        }

        private static float GetSideValue(Rect rect, RectSide side)
        {
            return side switch
            {
                RectSide.Top => rect.yMin,
                RectSide.Bottom => rect.yMax,
                RectSide.Left => rect.xMin,
                RectSide.Right => rect.xMax,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        private static Rect SetSideValue(Rect rect, RectSide side, float value)
        {
            return side switch
            {
                RectSide.Top => Rect.MinMaxRect(rect.xMin, value, rect.xMax, rect.yMax),
                RectSide.Bottom => Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, value),
                RectSide.Left => Rect.MinMaxRect(value, rect.yMin, rect.xMax, rect.yMax),
                RectSide.Right => Rect.MinMaxRect(rect.xMin, rect.yMin, value, rect.yMax),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        // 1辺を移動して形を変えられるRect
        private class EdgeMovableRect
        {
            private Rect _originalRect;
            private float _sideValue;

            public RectSide Side { get; private set; }

            public void Setup(RectSide side, Rect rect)
            {
                Side = side;
                _originalRect = rect;
                _sideValue = GetSideValue(rect, side);
            }

            public Rect MoveEdge(float delta)
            {
                _sideValue += delta;
                return SetSideValue(_originalRect, Side, _sideValue);
            }
        }

        #endregion

       
        public static float ThicknessMin { get; set; } = 15f;
        
        private const string UssClassName = "rosettaui-animation-curve-editor__selected-control-points-rect";
        private const string HandleClassName = UssClassName + "__handle";
        private const string HandleHorizontalClassName = HandleClassName + "--horizontal";
        private const string HandleVerticalClassName = HandleClassName + "--vertical";
        private const string HandleTopClassName = HandleClassName + "--top";
        private const string HandleBottomClassName = HandleClassName + "--bottom";
        private const string HandleLeftClassName = HandleClassName + "--left";
        private const string HandleRightClassName = HandleClassName + "--right";
        private const string HandleLineClassName = HandleClassName + "__center-line";
        
        private readonly SelectedControlPointsEditor _selectedControlPointsEditor;
        private readonly PreviewTransform _previewTransform;
        private readonly EdgeMovableRect _edgeMovableRect = new();
        private readonly Dictionary<ControlPoint, Vector2> _normalizedPositionsOnDragStart = new();
        
        private readonly VisualElement _topHandle;
        private readonly VisualElement _bottomHandle;
        private readonly VisualElement _leftHandle;
        private readonly VisualElement _rightHandle;
        
        
        private Rect ControlPointsRect
        {
            get
            {
                using var _ = ListPool<ControlPoint>.Get(out var list);
                list.AddRange(_selectedControlPointsEditor.ControlPoints);
                
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

        public SelectedControlPointsRect(SelectedControlPointsEditor selectedControlPointsEditor, PreviewTransform previewTransform)
        {
            _selectedControlPointsEditor = selectedControlPointsEditor;
            _previewTransform = previewTransform;
            
            AddToClassList(UssClassName);
            
            _topHandle = CreateHandle(RectSide.Top, HandleTopClassName, HandleHorizontalClassName, CursorType.ResizeVertical);
            Add(_topHandle);
            
            _bottomHandle = CreateHandle(RectSide.Bottom, HandleBottomClassName, HandleHorizontalClassName, CursorType.ResizeVertical);
            Add(_bottomHandle);
            
            _leftHandle = CreateHandle(RectSide.Left, HandleLeftClassName, HandleVerticalClassName, CursorType.ResizeHorizontal);
            Add(_leftHandle);
            
            _rightHandle = CreateHandle(RectSide.Right, HandleRightClassName, HandleVerticalClassName, CursorType.ResizeHorizontal);
            Add(_rightHandle);
            
            return;
            
            
            VisualElement CreateHandle(RectSide side, string classNameDir, string classNameHv, CursorType cursorType)
            {
                var handle = new VisualElement();
                handle.AddToClassList(HandleClassName);
                handle.AddToClassList(classNameDir);
                handle.AddToClassList(classNameHv);
                handle.style.cursor = CursorHolder.Get(cursorType);
                
                var dragManipulator = new DragManipulator();
                dragManipulator.onDragStarting += (_, evt) => OnDragStarting(evt, side);
                dragManipulator.onDrag += (_, evt) => OnDrag(evt);
                handle.AddManipulator(dragManipulator);
                
                var line = new VisualElement
                {
                    pickingMode = PickingMode.Ignore
                };
                line.AddToClassList(HandleLineClassName);

                handle.Add(line);
                
                return handle;
            }
        }

        public void UpdateView()
        {
            var rect = ControlPointsRect;
            if (rect is { width: 0, height: 0 })
            {
                this.Hide();
                return;
            }
            
            // ハンドルの表示/非表示を設定
            var topBottomEnable = rect.height >= float.Epsilon;
            var leftRightEnable = rect.width >= float.Epsilon;
            _topHandle.SetShow(topBottomEnable);
            _bottomHandle.SetShow(topBottomEnable);
            _leftHandle.SetShow(leftRightEnable);
            _rightHandle.SetShow(leftRightEnable);
            
            // 厚みがThicknessMin以下にならないように制限（中心位置は変えない）
            var center = rect.center;
            rect.width = Mathf.Max(rect.width, ThicknessMin);
            rect.height = Mathf.Max(rect.height, ThicknessMin);
            rect.center = center;
            
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
 
            this.Show();
        }

        
        #region Drag callbacks
        
        private bool OnDragStarting(PointerDownEvent evt, RectSide side)
        {
            if (evt.button != 0) return false;
            
            var rectOnParent = ControlPointsRect;
            var rectOnCurve = new Rect()
            {
                min = _previewTransform.GetCurvePosFromScreenPos(rectOnParent.min),
                max = _previewTransform.GetCurvePosFromScreenPos(rectOnParent.max)
            };
            
            _edgeMovableRect.Setup(side, rectOnCurve);
            
            _normalizedPositionsOnDragStart.Clear();
            foreach (var cp in _selectedControlPointsEditor.ControlPoints)
            {
                var normalized = Rect.PointToNormalized(rectOnCurve, cp.KeyframePosition);
                _normalizedPositionsOnDragStart[cp] = normalized;
            }
            
            evt.StopPropagation();
            return true;
        }

        // 表示されるRectとは別に論理的にControlPointsのBoundingRectを計算する
        // 表示されるRectは厚みがThicknessMin以下にならないように制限しているが、論理Rectは制限なし
        // ドラッグ中にZoomされてもいいように論理RectはCurve座標系
        private void OnDrag(PointerMoveEvent evt)
        {
            var delta = _previewTransform.GetCurvePosFromScreenPos(evt.deltaPosition) - _previewTransform.GetCurvePosFromScreenPos(Vector2.zero);
            var deltaValue = _edgeMovableRect.Side is RectSide.Left or RectSide.Right ? delta.x : delta.y;
            
            var rect = _edgeMovableRect.MoveEdge(deltaValue);
            
            _selectedControlPointsEditor.UpdateControlPointKeyframes(cp =>
            {
                if (!_normalizedPositionsOnDragStart.TryGetValue(cp, out var normalized))
                {
                    return cp.Keyframe;
                }
                
                var keyframePosition = Rect.NormalizedToPoint(rect, normalized);
                
                keyframePosition = _previewTransform.SnapCurvePositionIfEnabled(keyframePosition);
                
                var keyframe = cp.Keyframe;
                keyframe.SetPosition(keyframePosition);
                return keyframe;
            });

            evt.StopPropagation();
        }
        
        #endregion
    }
}