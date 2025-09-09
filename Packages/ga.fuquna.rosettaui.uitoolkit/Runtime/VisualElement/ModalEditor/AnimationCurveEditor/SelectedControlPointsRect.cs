using System;
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
        
        private static RectSide OppositeSide(RectSide side)
        {
            return side switch
            {
                RectSide.Top => RectSide.Bottom,
                RectSide.Bottom => RectSide.Top,
                RectSide.Left => RectSide.Right,
                RectSide.Right => RectSide.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
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
        
        private static float GetSideThickness(Rect rect, RectSide side)
        {
            return side switch
            {
                RectSide.Top => rect.height,
                RectSide.Bottom => rect.height,
                RectSide.Left => rect.width,
                RectSide.Right => rect.width,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
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
        
        private readonly VisualElement _topHandle;
        private readonly VisualElement _bottomHandle;
        private readonly VisualElement _leftHandle;
        private readonly VisualElement _rightHandle;

        private RectSide _draggingRectSide;
        private Vector2 _pointerPositionOnDragStart;
        private float _rectSideValueOnDragStart;


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
            
            _draggingRectSide = side;
            _pointerPositionOnDragStart = evt.position;
            _rectSideValueOnDragStart = GetSideValue(ControlPointsRect, side);

            evt.StopPropagation();
            return true;
        }
        
        private void OnDrag(PointerMoveEvent evt)
        {
            var delta = (Vector2)evt.position - _pointerPositionOnDragStart;
            var deltaValue = _draggingRectSide is RectSide.Left or RectSide.Right ? delta.x : delta.y;
            var newSideValue = _rectSideValueOnDragStart + deltaValue;
            
            var rect = ControlPointsRect;
            var newRect = SetSideValue(rect, _draggingRectSide, newSideValue);
            var newThickness = GetSideThickness(newRect, _draggingRectSide);
            
            const float epsilon = 1f;

            switch (newThickness)
            {
                // width/heightが-epsilon以下になったら反転
                // OppositeSideをドラッグしている状態にする
                // width/heightが負になっていても-epsilonを越えるまでは反転せず下記の厚みepsilon制限を適用
                case < -epsilon:
                    _rectSideValueOnDragStart = GetSideValue(newRect, _draggingRectSide);
                    _pointerPositionOnDragStart = evt.position;
                    _draggingRectSide = OppositeSide(_draggingRectSide);
                    break;
                
                // 厚みがepsilon以下にならないように制限
                case < epsilon:
                {
                    var sideValue = GetSideValue(rect, _draggingRectSide);
                    var oppositeSideValue = GetSideValue(rect, OppositeSide(_draggingRectSide));
                    newSideValue = oppositeSideValue + Mathf.Sign(sideValue - oppositeSideValue) * epsilon;
                    newRect = SetSideValue(rect, _draggingRectSide, newSideValue);
                    break;
                }
            }
            
            _selectedControlPointsEditor.UpdateControlPointKeyframes(cp =>
            {
                var normalized = Rect.PointToNormalized(rect, cp.LocalPosition);
                var newLocalPosition = Rect.NormalizedToPoint(newRect, normalized);
                
                var keyframePosition = _previewTransform.GetCurvePosFromScreenPos(newLocalPosition);

                var keyframe = cp.Keyframe;
                keyframe.SetPosition(keyframePosition);
                return keyframe;
            });

            evt.StopPropagation();
        }
        
        #endregion
    }
}