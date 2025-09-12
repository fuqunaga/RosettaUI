using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// 選択中のControlPointをまとめて編集するクラス
    /// </summary>
    public class SelectedControlPointsEditor
    {
        private readonly CurveController _curveController;
        
        public bool IsEmpty => !_curveController.SelectedControlPoints.Any();

        public bool IsMultiSelection => _curveController.SelectedControlPoints.Skip(1).Any();
        
        public IEnumerable<ControlPoint> ControlPoints => _curveController.SelectedControlPoints;
        


        public SelectedControlPointsEditor(CurveController curveController)
        {
            _curveController = curveController;
        }
        
        
        public void RemoveAll() => _curveController.RemoveAllSelectedControlPoints();

        public void SetTangentMode(InOrOut inOrOut, TangentMode tangentMode, bool notifyOnChanged = true)
        {
            Action<ControlPoint> setFunc = inOrOut switch
            {
                InOrOut.In => SetIn,
                InOrOut.Out => SetOut,
                _ => throw new ArgumentOutOfRangeException(nameof(inOrOut), inOrOut, null)
            };
            
            EditControlPoints(setFunc, notifyOnChanged);
            return;

            void SetIn(ControlPoint cp)
            {
                cp.InTangentMode = tangentMode;
            }
            
            void SetOut(ControlPoint cp)
            {
                cp.OutTangentMode = tangentMode;
            }
        }
        
        public void SetBothTangentMode(TangentMode tangentMode, bool notifyOnChanged = true)
        {
            EditControlPoints(cp =>
            {
                cp.InTangentMode = tangentMode;
                cp.OutTangentMode = tangentMode;
            }, notifyOnChanged);
        }

        public void SetKeyBroken(bool isBroken, bool notifyOnChanged = true)
            => EditControlPoints(cp => cp.IsKeyBroken = isBroken, notifyOnChanged);

        public void SetWeightedMode(WeightedMode weightedMode, bool weighted, bool notifyOnChanged = true)
        {
            // weightedになったらTangentMode.Freeにする
            // https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Editor/Mono/Animation/AnimationWindow/CurveMenuManager.cs#L188-L202
            if (weighted)
            {
                var (inEnable, outEnable) = weightedMode switch
                {
                    WeightedMode.In => (true, false),
                    WeightedMode.Out => (false, true),
                    WeightedMode.Both => (true, true),
                    _ => (false, false)
                };
                
                EditControlPoints(cp =>
                {
                    if (inEnable)
                    {
                        cp.InTangentMode = TangentMode.Free;
                    }

                    if (outEnable)
                    {
                        cp.OutTangentMode = TangentMode.Free;
                    }
                }, false);
            }
            
            UpdateKeyframes(keyframe =>
            {
                keyframe.SetWeightedFrag(weightedMode, weighted);
                return keyframe;
            }, notifyOnChanged);
        }
        
        public void UpdateKeyframePosition(float? xOrNull, float? yOrNull, bool notifyOnChanged = true){
            UpdateKeyframes(keyframe =>
            {
                if (xOrNull is { } x) keyframe.time = x;
                if (yOrNull is { } y) keyframe.value = y;
                return keyframe;
            }, notifyOnChanged);
        }
        
        public void UpdateKeyframes(Func<Keyframe, Keyframe> editKeyframeFunc, bool notifyOnChanged = true)
        {
            UpdateControlPointKeyframes(
                cp => editKeyframeFunc(cp.Keyframe),
                notifyOnChanged
            );
        } 
        
        public void UpdateControlPointKeyframes(Func<ControlPoint, Keyframe> controlPointToNewKeyframeFunc, bool notifyOnChanged = true)
        {
            _curveController.UpdateKeyframes(
                ControlPoints.Select(cp => (cp, controlPointToNewKeyframeFunc(cp))),
                notifyOnChanged
            );
        } 
        
        public void EditControlPoints(Action<ControlPoint> editAction, bool notifyOnChanged = true)
        {
            foreach (var cp in ControlPoints)
            {
                editAction(cp);
            }

            if (notifyOnChanged)
            {
                _curveController.OnCurveChanged();
            }
        }
    }
}