using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// ControlPointとSelectedControlPointsRectにAddしてドラッグ操作を受け付けるマニュピレーター生成クラス
    /// </summary>
    public class ControlPointsDragManipulatorSource
    {
        private enum MoveAxis
        {
            Both,
            Horizontal,
            Vertical
        }
        
        private readonly CurveController _curveController;
        private readonly PreviewTransform _previewTransform;
        private readonly ControlPointDisplayPositionPopup _positionPopup;
        private readonly Func<Vector2, Vector2> _worldToLocalPosition;
        private readonly Dictionary<ControlPoint, Vector2> _keyframePositionsOnDragStart = new();

        private Vector2 _cursorPositionOnDragStart;
        private MoveAxis _moveAxis = MoveAxis.Both;

        private SelectedControlPointsEditor SelectedControlPointsEditor => _curveController.SelectedControlPointsEditor;
        
        public ControlPointsDragManipulatorSource(
            CurveController curveController,
            PreviewTransform previewTransform,
            ControlPointDisplayPositionPopup positionPopup,
            Func<Vector2, Vector2> worldToLocalPosition)
        {
            _curveController = curveController;
            _previewTransform = previewTransform;
            _positionPopup = positionPopup;
            _worldToLocalPosition = worldToLocalPosition;
        }
        
        public Manipulator CreateManipulator()
        {
            var manipulator = new DragManipulator();
            manipulator.onDragStarting += OnDragStarting;
            manipulator.onDrag += OnDrag;
            manipulator.onDragEnd += OnDragEnd;
            return manipulator;
        }

        private bool OnDragStarting(DragManipulator manipulator, PointerDownEvent evt)
        {
            if (evt.button != 0) return false; // left button only
            
            // start drag
            _cursorPositionOnDragStart = _previewTransform.GetCurvePosFromScreenPos(evt.position);
            _keyframePositionsOnDragStart.Clear();
            foreach (var cp in SelectedControlPointsEditor.ControlPoints)
            {
                _keyframePositionsOnDragStart[cp] = cp.KeyframePosition;
            }

            _moveAxis = MoveAxis.Both;

            _positionPopup.Show();
            UpdatePositionPopup(evt.position);

            evt.StopPropagation();

            return true;
        }

        private void OnDrag(DragManipulator _, PointerMoveEvent evt)
        {
            if (_moveAxis == MoveAxis.Both && evt.shiftKey)
            {
                var delta = evt.deltaPosition;
                _moveAxis = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? MoveAxis.Horizontal : MoveAxis.Vertical;
            }
            
            var cursorPositionOnCurve = _previewTransform.GetCurvePosFromScreenPos(evt.position);
            var movedCursorPosition = cursorPositionOnCurve - _cursorPositionOnDragStart;
            
            SelectedControlPointsEditor.UpdateControlPointKeyframes(cp =>
            {
                if (!_keyframePositionsOnDragStart.TryGetValue(cp, out var keyframePositionOnDragStart))
                {
                    return cp.Keyframe;
                }

                var keyframePosition = keyframePositionOnDragStart + movedCursorPosition;
                
                switch (_moveAxis)
                {
                    case MoveAxis.Both:
                        break;
                    case MoveAxis.Horizontal:
                        keyframePosition.y = keyframePositionOnDragStart.y;
                        break;
                    case MoveAxis.Vertical:
                        keyframePosition.x = keyframePositionOnDragStart.x;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                keyframePosition = _previewTransform.SnapCurvePositionIfEnabled(keyframePosition);
                
                var keyframe = cp.Keyframe;
                keyframe.SetPosition(keyframePosition);
                return keyframe;
            });
            
            UpdatePositionPopup(evt.position);
        }

        private void OnDragEnd(DragManipulator _, EventBase evt)
        {
            _positionPopup.Hide();
        }

        // cursorPosition: 画面上の座標。PreviewTransformのScreenPosではない(ので注意
        private void UpdatePositionPopup(Vector2 cursorPosition)
        {
            Vector2 elementPosition;
            Vector2 keyframePosition;
            
            var isMultiSelection = SelectedControlPointsEditor.IsMultiSelection;
            if (isMultiSelection)
            {
                elementPosition = _worldToLocalPosition(cursorPosition);
                keyframePosition = _previewTransform.GetCurvePosFromScreenPos(elementPosition);
            }
            else
            {
                var cp = SelectedControlPointsEditor.ControlPoints.FirstOrDefault();
                if (cp == null)
                {
                    return;
                }
                
                elementPosition = cp.GetLocalPosition();
                keyframePosition = cp.KeyframePosition;
            }

            _positionPopup.Update(elementPosition, keyframePosition);
        }
    }
}