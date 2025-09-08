using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// ControlPointとSelectedControlPointsRectにAddしてポインター操作を受け付けるマニュピレーター生成クラス
    /// </summary>
    public class ControlPointsManipulatorSource
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
        private readonly ControlPointsEditPositionPopup _editPositionPopup;
        private readonly Func<(bool, bool)> _getSnapSettings;
        private readonly Func<Vector2, Vector2> _worldToLocalPosition;
        private readonly Dictionary<ControlPoint, Vector2> _keyframePositionsOnDragStart = new();

        private Vector2 _cursorPositionOnDragStart;
        private MoveAxis _moveAxis = MoveAxis.Both;

        private SelectedControlPointsEditor SelectedControlPointsEditor => _curveController.SelectedControlPointsEditor;
        
        public ControlPointsManipulatorSource(
            CurveController curveController,
            PreviewTransform previewTransform,
            ControlPointDisplayPositionPopup positionPopup,
            ControlPointsEditPositionPopup editPositionPopup,
            Func<(bool, bool)> getSnapSettings,
            Func<Vector2, Vector2> worldToLocalPosition)
        {
            _curveController = curveController;
            _previewTransform = previewTransform;
            _positionPopup = positionPopup;
            _editPositionPopup = editPositionPopup;
            _getSnapSettings = getSnapSettings;
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
            var targetCp = manipulator.target as ControlPoint;

            switch (evt.button)
            {
                // Left click
                // - subKey: select/unselect
                // - !subKey: select only this. start drag.
                case 0:
                    var subKey = evt.shiftKey || evt.ctrlKey || evt.commandKey;

                    if (targetCp != null)
                    {
                        if (subKey)
                        {
                            if (targetCp.IsActive)
                            {
                                _curveController.UnselectControlPoint(targetCp);
                            }
                            else
                            {
                                _curveController.SelectControlPoint(targetCp, keepOtherSelection: true);
                            }
                            
                            // do not start drag
                            evt.StopPropagation();
                            return false;
                        }

                        // 未選択のコントロールポイントの場合はそれのみ選択する
                        // すでに選択済みの場合は複数選択のケースもあるのでそのまま
                        if (!targetCp.IsActive)
                        {
                            _curveController.SelectControlPoint(targetCp);
                        }
                    }
                    
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


                // Right click
                //  Show popup menu
                case 1:
                    if (targetCp != null)
                    {
                        // IsActive(=選択中）の場合はそのまま（複数選択のとき複数選択状態をキープ）
                        // IsActiveでない場合は他の選択を外して自身のみ選択してからメニュー表示
                        if (targetCp is { IsActive: false })
                        {
                            _curveController.SelectControlPoint(targetCp, keepOtherSelection: false);
                        }

                        // Show menu
                        // manipulator.targetがSelectedControlPointsRectのときでも出して良い気がするが、
                        // - UnityのAnimationCurveEditorの挙動では出ない
                        // - DropdownMenuが、画面下部にスペースがなく上部にある場合targetエレメントの上にメニューが表示される
                        //   SelectedControlPointsRectの上部に出てしまいカーソルから離れすぎて不自然なので一旦なし
                        ControlPointsPopupMenu.Show(evt.position, SelectedControlPointsEditor, _editPositionPopup, targetCp);
                        
                        evt.StopPropagation();
                    }

                    break;
            }


            return false;
        }

        private void OnDrag(DragManipulator manipulator, PointerMoveEvent evt)
        {
            if (_moveAxis == MoveAxis.Both && evt.shiftKey)
            {
                var delta = evt.deltaPosition;
                _moveAxis = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? MoveAxis.Horizontal : MoveAxis.Vertical;
            }
            
            var cursorPositionOnCurve = _previewTransform.GetCurvePosFromScreenPos(evt.position);
            var movedCursorPosition = cursorPositionOnCurve - _cursorPositionOnDragStart;
            
            var (snapX, snapY) = _getSnapSettings();
            SelectedControlPointsEditor.UpdateKeyframes(cp =>
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
                
                
                if (snapX || snapY)
                {
                    var gridViewport = _previewTransform.PreviewGridViewport;
                    if (snapX) keyframePosition.x = gridViewport.RoundX(keyframePosition.x, 0.05f);
                    if (snapY) keyframePosition.y = gridViewport.RoundY(keyframePosition.y, 0.05f);
                }
                
                var keyframe = cp.Keyframe;
                keyframe.time = keyframePosition.x;
                keyframe.value = keyframePosition.y;
                return keyframe;
            });
            
            UpdatePositionPopup(evt.position);
        }

        private void OnDragEnd(DragManipulator manipulator, EventBase evt)
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