using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RosettaUI.Builder;
using RosettaUI.UndoSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// CurveControllerのUndo関連部分
    /// </summary>
    public partial class CurveController
    {
        /// <summary>
        /// Undoできるコマンド
        /// </summary>
        public readonly struct UndoableCommand
        {
            private readonly CurveController _curveController;
            private AnimationCurve Curve => _curveController.Curve;
            
            public UndoableCommand(CurveController curveController) => _curveController = curveController;

            
            public void SetPreWrapMode(WrapMode mode)
            {
                if (Curve == null) return;
                var before = Curve.preWrapMode;
                if (before == mode) return;
                
                _curveController.SetPreWrapMode(mode);
            
                Undo.RecordValueChange($"{nameof(CurveController)} SetPreWrapMode", before, mode, _curveController.SetPreWrapMode);
            }
        
            public void SetPostWrapMode(WrapMode mode)
            {
                if (Curve == null) return;
                var before = Curve.postWrapMode;
                if (before == mode) return;

                _curveController.SetPostWrapMode(mode);
            
                Undo.RecordValueChange($"{nameof(CurveController)} SetPostWrapMode", before, mode, _curveController.SetPostWrapMode);
            }

            public void AddKey(Vector2 keyFramePosition)
            {
                using var _ = _curveController.RecordUndoSnapshot();
                _curveController.AddKey(keyFramePosition);
            }
            
            public void SetCurve(AnimationCurve newCurve)
            {
                if (Curve.Equals(newCurve)) return;
                
                using var _ = _curveController.RecordUndoSnapshot();
                _curveController.SetCurve(newCurve);
            }
        }

        
        public readonly struct UndoableCommandForSelection
        {
            private readonly CurveController _curveController;
            private IEnumerable<ControlPoint> SelectedControlPoints => _curveController.SelectedControlPoints;

            public UndoableCommandForSelection(CurveController curveController) => _curveController = curveController;
            
                    
            public void RemoveAllControlPoints()
            {
                using var undoScope = _curveController.RecordUndoSnapshot();
                
                using var _ = ListPool<ControlPoint>.Get(out var list);
                list.AddRange(_curveController.SelectedControlPoints);

                foreach (var cp in list)
                {
                    _curveController.DoRemoveControlPoint(cp);
                }
            
                _curveController.OnCurveChanged();
            }

            public void SetFreeSmooth()
            {
                using var _ = _curveController.RecordUndoSnapshot();
                SetTangentModeInternal(true, true, TangentMode.Free);
                SetKeyBrokenInternal(false);
            }

            public void SetFlat()
            {
                using var _ = _curveController.RecordUndoSnapshot();
                SetTangentModeInternal(true, true, TangentMode.Free);
                SetKeyBrokenInternal(false);
                UpdateKeyframes(keyframe =>
                {
                    keyframe.inTangent = 0;
                    keyframe.outTangent = 0;
                    return keyframe;
                });
            }
            
            public void SetKeyBroken(bool isBroken)
            {
                using var _ = _curveController.RecordUndoSnapshot();
                SetKeyBrokenInternal(isBroken);
            }

            public void SetTangentMode(bool enableIn, bool enableOut, TangentMode mode)
            {
                using var _ = _curveController.RecordUndoSnapshot();
                SetTangentModeInternal(enableIn, enableOut, mode);
                SetKeyBrokenInternal(true);
            }

            public void SetWeightedMode(WeightedMode weightedMode, bool weighted)
            {
                using var _ = _curveController.RecordUndoSnapshot();

                // weightedになったらTangentMode.Freeにする
                // https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Editor/Mono/Animation/AnimationWindow/CurveMenuManager.cs#L188-L202
                if (weighted)
                {
                    var (enableIn, enableOut) = weightedMode switch
                    {
                        WeightedMode.In => (true, false),
                        WeightedMode.Out => (false, true),
                        WeightedMode.Both => (true, true),
                        _ => (false, false)
                    };
                
                    SetTangentModeInternal(enableIn, enableOut, TangentMode.Free);
                }
            
                UpdateKeyframes(keyframe =>
                {
                    keyframe.SetWeightedFrag(weightedMode, weighted);
                    return keyframe;
                });
            }
            
            public void SetKeyframePosition(float? xOrNull, float? yOrNull)
            {
                using var _ = _curveController.RecordUndoSnapshot();
                
                UpdateKeyframes(keyframe =>
                {
                    if (xOrNull is { } x) keyframe.time = x;
                    if (yOrNull is { } y) keyframe.value = y;
                    return keyframe;
                });
            }



            private void SetTangentModeInternal(bool enableIn, bool enableOut, TangentMode tangentMode)
            {
                EditControlPoints(cp =>
                {
                    if (enableIn)
                    {
                        cp.InTangentMode = tangentMode;
                    }

                    if (enableOut)
                    {
                        cp.OutTangentMode = tangentMode;
                    }
                });
            }
            
            private void SetKeyBrokenInternal(bool isBroken) => EditControlPoints(cp => cp.IsKeyBroken = isBroken);

            private void UpdateKeyframes(Func<Keyframe, Keyframe> editKeyframeFunc)
            {
                UpdateControlPointKeyframes(cp => editKeyframeFunc(cp.Keyframe));
            } 
            
            private void UpdateControlPointKeyframes(Func<ControlPoint, Keyframe> controlPointToNewKeyframeFunc)
            {
                _curveController.UpdateKeyframes(SelectedControlPoints.Select(cp => (cp, controlPointToNewKeyframeFunc(cp))));
            }
            
            private void EditControlPoints(Action<ControlPoint> editAction)
            {
                foreach (var cp in SelectedControlPoints)
                {
                    editAction(cp);
                }
            }
        }


        private readonly struct RecordUndoSnapshotScope : IDisposable
        {
            private readonly string _commandName;
            private readonly CurveController _curveController;
            private readonly AnimationCurveUndoSnapshot _beforeSnapshot;
            
            public RecordUndoSnapshotScope(string commandName, CurveController curveController)
            {
                _commandName = commandName;
                _curveController = curveController;
                _beforeSnapshot = curveController.RentSnapshot();
            }
            
            public void Dispose()
            {
                var afterSnapshot = _curveController.RentSnapshot();
                
                if (_beforeSnapshot.Equals(afterSnapshot))
                {
                    _beforeSnapshot.Dispose();
                    afterSnapshot.Dispose();
                    return;
                }
                
                Undo.RecordValueChange(
                    $"{nameof(CurveController)} {_commandName}",
                    _beforeSnapshot,
                    afterSnapshot,
                    _curveController.ApplySnapshot);
            }
        }
        
        
        public UndoableCommand Command => new(this);
        public UndoableCommandForSelection CommandForSelection => new(this);
        
        private RecordUndoSnapshotScope RecordUndoSnapshot([CallerMemberName]string callerMethodName = "") => new(callerMethodName, this);
        
        
        public AnimationCurveUndoSnapshot RentSnapshot()
        {
            var snapshot = AnimationCurveUndoSnapshot.GetPooled();
            snapshot.Initialize(Curve, ControlPoints);
            return snapshot;
        }
        
        
        public void ApplySnapshot(AnimationCurveUndoSnapshot snapshot)
        {
            Curve = snapshot.curve;
            ResetControlPoints(snapshot.extraDataList);
            onApplySnapshot?.Invoke();
        }
    }
}