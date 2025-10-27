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
        private readonly CurveSnapshot _curveSnapshot;
        private readonly Dictionary<ControlPoint, Vector2> _pointerToKeyframePositionOffsetsOnDragStart = new();
        
        private Manipulator _currentManipulator;
        private Vector2 _pointerPositionOnDragStart;
        private MoveAxis _moveAxis = MoveAxis.Both;
        private CurveController.RecordUndoSnapshotScope _undoScopeOnDragStart;

        private IEnumerable<ControlPoint> SelectedControlPoints => _curveController.SelectedControlPoints;
        
        public ControlPointsDragManipulatorSource(
            CurveController curveController,
            PreviewTransform previewTransform,
            ControlPointDisplayPositionPopup positionPopup
        )
        {
            _curveController = curveController;
            _previewTransform = previewTransform;
            _positionPopup = positionPopup;
            _curveSnapshot = new CurveSnapshot(curveController);
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
            if(_currentManipulator != null) return false; // すでにドラッグ中
            _currentManipulator = manipulator;
            
            _undoScopeOnDragStart = _curveController.RecordUndoSnapshot();
            
            // start drag
            _pointerPositionOnDragStart = _previewTransform.GetCurvePosFromUIWorldPos(evt.position);
            
            _pointerToKeyframePositionOffsetsOnDragStart.Clear();
            foreach (var cp in SelectedControlPoints)
            {
                _pointerToKeyframePositionOffsetsOnDragStart[cp] = cp.KeyframePosition - _pointerPositionOnDragStart;
            }

            // ControlPointとKeyframeのスナップショットを保存
            // ドラッグ中に選択中のKeyframeが同一timeに来ると上書きされて消えるが、
            // さらにドラッグしてtimeがずれたら復活したい
            _curveSnapshot.TakeSnapshot();
            
            _moveAxis = MoveAxis.Both;

            _positionPopup.Show();
            UpdatePositionPopup(evt.position);

            evt.StopPropagation();

            return true;
        }

        private void OnDrag(DragManipulator _, PointerMoveEvent evt)
        {
            // スナップショットに保存されたKeyframeのうち選択中ではないものを復元
            // ドラッグ中に選択中のKeyframeが同一timeに来ると上書きされて消えるがさらにドラッグしてtimeがずれたら復活させる
            // すでに同一timeにKeyframeがある場合はAddKeyframeで無視されるので問題ない
            _curveSnapshot.ApplySnapshotWithoutSelection();
            
            if (_moveAxis == MoveAxis.Both && evt.shiftKey)
            {
                var delta = evt.deltaPosition;
                _moveAxis = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? MoveAxis.Horizontal : MoveAxis.Vertical;
            }
            
            var pointerPositionOnCurve = _previewTransform.GetCurvePosFromUIWorldPos(evt.position);

            _curveController.UpdateSelectedControlPointKeyframes(cp =>
            {
                if (!_pointerToKeyframePositionOffsetsOnDragStart.TryGetValue(cp,
                        out var pointerToKeyframeOffsetOnDragStart))
                {
                    return cp.Keyframe;
                }

                switch (_moveAxis)
                {
                    case MoveAxis.Both:
                        break;
                    case MoveAxis.Horizontal:
                        pointerPositionOnCurve.y = _pointerPositionOnDragStart.y;
                        break;
                    case MoveAxis.Vertical:
                        pointerPositionOnCurve.x = _pointerPositionOnDragStart.x;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var keyframePosition = pointerToKeyframeOffsetOnDragStart + pointerPositionOnCurve;

                keyframePosition = _previewTransform.SnapCurvePositionIfEnabled(keyframePosition);

                var keyframe = cp.Keyframe;
                keyframe.SetPosition(keyframePosition);
                return keyframe;
            });
            
            UpdatePositionPopup(evt.position);
        }

        private void OnDragEnd(DragManipulator _, EventBase evt)
        {
            _undoScopeOnDragStart.Dispose();
            _undoScopeOnDragStart = default;
            
            _currentManipulator = null;
            _positionPopup.Hide();
        }

        // cursorPosition: 画面上の座標。PreviewTransformのScreenPosではない(ので注意
        private void UpdatePositionPopup(Vector2 pointerPosition)
        {
            Vector2 elementPosition;
            Vector2 keyframePosition;
            
            var isMultiSelection = _curveController.IsMultiSelection;
            if (isMultiSelection)
            {
                elementPosition = _previewTransform.GetScreenPosFromUIWorldPos(pointerPosition);
                keyframePosition = _previewTransform.GetCurvePosFromScreenPos(elementPosition);
            }
            else
            {
                var cp = SelectedControlPoints.FirstOrDefault();
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