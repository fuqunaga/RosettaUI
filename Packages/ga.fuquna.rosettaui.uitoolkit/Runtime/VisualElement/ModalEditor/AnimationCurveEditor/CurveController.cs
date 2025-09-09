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
    /// Manage AnimationCurve and ControlPoints
    /// </summary>
    public class CurveController
    {
        public event Action onCurveChanged;
        
        // Invoked when control point selection or addition/removal happens
        public event Action onControlPointSelectionChanged;
        
        private readonly VisualElement _parent;
        private readonly Func<CurveController, ControlPoint> _getNewControlPoint;

        public AnimationCurve Curve { get; private set; } = new();
        
        public SelectedControlPointsEditor SelectedControlPointsEditor { get; }
        
        public IEnumerable<ControlPoint> SelectedControlPoints => ControlPoints.Where(t => t.IsActive);


        private List<ControlPoint> ControlPoints { get; } = new();

        public CurveController(VisualElement parent, Func<CurveController, ControlPoint> getNewControlPoint)
        {
            _parent = parent;
            _getNewControlPoint = getNewControlPoint;
            
            SelectedControlPointsEditor = new SelectedControlPointsEditor(this);
        }
        
        public void SetCurve(AnimationCurve curve)
        {
            Curve = curve;
            ResetControlPoints();
        }
        
        public Keyframe GetKeyframe(ControlPoint controlPoint)
        {
            var index = ControlPoints.IndexOf(controlPoint);
            if (index < 0 || index >= Curve.keys.Length)
            {
                return default;
            }
            return Curve[index];
        }
        
        public ControlPoint GetControlPointLeft(ControlPoint controlPoint)
        {
            if (controlPoint == null) return null;
            var index = ControlPoints.IndexOf(controlPoint);
            return index <= 0 
                ? null 
                : GetControlPoint(index - 1);
        }
        
        public ControlPoint GetControlPointRight(ControlPoint controlPoint)
        {
            if (controlPoint == null) return null;
            var index = ControlPoints.IndexOf(controlPoint);
            return index < 0 || index >= ControlPoints.Count - 1 
                ? null 
                : GetControlPoint(index + 1);
        }

        public int AddKey(Vector2 keyFramePosition)
        {
            var key = new Keyframe(keyFramePosition.x, keyFramePosition.y);
            
            var index = Curve.AddKey(key);
            if (index < 0) return index;
            
            // Add control point
            var controlPoint = CreateControlPoint(index);
            controlPoint.IsActive = true;
            
            // Match the tangent mode to neighborhood point one
            if (GetControlPointLeft(controlPoint)?.OutTangentMode is {} inTangentMode)
            {
                controlPoint.InTangentMode = inTangentMode;
            }

            if (GetControlPointRight(controlPoint)?.InTangentMode is { } outTangentMode)
            {
                controlPoint.OutTangentMode = outTangentMode;
            }
            
            OnCurveChanged();
            
            return index;
        }
        
        public void RemoveAllSelectedControlPoints()
        {
            using var _ = ListPool<ControlPoint>.Get(out var list);
            list.AddRange(ControlPoints.Where(cp => cp.IsActive));
            
            foreach (var cp in list)
            {
                DoRemoveControlPoint(cp);
            }
            
            OnCurveChanged();
        }
        
        private void DoRemoveControlPoint(ControlPoint controlPoint)
        {
            controlPoint.RemoveFromHierarchy();
            
            var index = ControlPoints.IndexOf(controlPoint);
            Curve.RemoveKey(index);
            ControlPoints.RemoveAt(index);
        }

        public void SelectControlPointsInRect(Rect selectionRect, bool preserveOtherSelection = false)
        {
            if (!preserveOtherSelection)
            {
                DoUnselectAllControlPoints();
            }
            
            foreach (var controlPoint in ControlPoints)
            {
                var resolvedStyle = controlPoint.resolvedStyle;
                var localPosition = new Vector2(
                    resolvedStyle.left,
                    resolvedStyle.top
                );

                if (selectionRect.Contains(localPosition))
                {
                    controlPoint.IsActive = true;
                }
            }
            
            onControlPointSelectionChanged?.Invoke();
        }
        
        public void SelectControlPoint(ControlPoint controlPoint, bool keepOtherSelection = false)
        {
            if (!ControlPoints.Contains(controlPoint)) return;

            if (!keepOtherSelection)
            {
                DoUnselectAllControlPoints();
            }

            controlPoint.IsActive = true;
            onControlPointSelectionChanged?.Invoke();
        }
        
        public void UnselectControlPoint(ControlPoint controlPoint)
        {
            if (!ControlPoints.Contains(controlPoint)) return;

            controlPoint.IsActive = false;
            onControlPointSelectionChanged?.Invoke();
        }

        public void UnselectAllControlPoints()
        {
            DoUnselectAllControlPoints();
            onControlPointSelectionChanged?.Invoke();
        }
        
        private void DoUnselectAllControlPoints()
        {
            foreach (var cp in ControlPoints)
            {
                cp.IsActive = false;
            }
        }
                
        public void UpdateView()
        {
            foreach(var controlPoint in ControlPoints)
            {
                controlPoint.UpdateView();
            }
        }
                
        private ControlPoint GetControlPoint(int index)
        {
            if (index < 0 || index >= ControlPoints.Count) return null;
            return ControlPoints[index];
        }
        
        private void ResetControlPoints()
        {
            if (Curve == null) return;
            
            foreach (var cp in ControlPoints)
            {
                cp.RemoveFromHierarchy();
            }
            ControlPoints.Clear();
            
            for (var i = 0; i < Curve.keys.Length; i++)
            {
                var controlPoint = CreateControlPoint(i);
                controlPoint.IsKeyBroken = Curve.GetKeyBroken(i);
                controlPoint.InTangentMode = Curve.GetInTangentMode(i);
                controlPoint.OutTangentMode = Curve.GetOutTangentMode(i);
            }
        }
        
        private ControlPoint CreateControlPoint(int index)
        {
            var controlPoint = _getNewControlPoint(this);
            _parent.Add(controlPoint);
            
            ControlPoints.Insert(index, controlPoint);
            
            return controlPoint;
        }

        public void SetPreWrapMode(WrapMode mode)
        {
            if (Curve == null) return;
            Curve.preWrapMode = mode;
            onCurveChanged?.Invoke();
        }
        
        public void SetPostWrapMode(WrapMode mode)
        {
            if (Curve == null) return;
            Curve.postWrapMode = mode;
            onCurveChanged?.Invoke();
        }
        
        /// <summary>
        /// ControlPointのKeyframeの時間が変更されたらAnimationCurve上のIndexが変わる可能性がある
        /// Indexが変わるなら追随する
        /// </summary>
        public void UpdateKeyframes(IEnumerable<(ControlPoint, Keyframe)> controlPointAndNewKeyframes, bool notifyOnChanged = true)
        {
            if (controlPointAndNewKeyframes == null || Curve == null || ControlPoints.Count == 0)
            {
                return;
            }
            
            using var _ = ListPool<(ControlPoint controlPoint, Keyframe keyframe)>.Get(out var list);
            list.AddRange(controlPointAndNewKeyframes);
            
            foreach (var (controlPoint, keyframe) in list)
            {
                var index = ControlPoints.IndexOf(controlPoint);
                if (index < 0 || index >= Curve.keys.Length)
                {
                    continue;
                }

                // 既存のKeyframeと同じtimeなら削除して上書き
                // MoveKey()ではtime変更なしの更新になるので自前で上書きを実装
                // > AnimationCurve does not support two keys with the same time. If key.time is the same as another keyframe, key is reinserted at the time of the keyframe at index. This cancels the move operation in the time dimension and keeps the modification in the value dimension.
                // https://docs.unity3d.com/ScriptReference/AnimationCurve.MoveKey.html
                var existingKeyIndex = Array.FindIndex(Curve.keys, k => Mathf.Approximately(k.time, keyframe.time));
                if (existingKeyIndex >= 0 && existingKeyIndex != index)
                {
                    var existingKeyControlPoint = GetControlPoint(existingKeyIndex);
                    DoRemoveControlPoint(existingKeyControlPoint);
                    
                    // indexが変わっている可能性があるので再取得
                    index = ControlPoints.IndexOf(controlPoint);
                }

                var newIndex = Curve.MoveKey(index, keyframe);
                if (newIndex != index)
                {
                    // Indexが変わったのでControlPointsの順番も入れ替える
                    var temp = ControlPoints[index];
                    ControlPoints.RemoveAt(index);
                    ControlPoints.Insert(newIndex, temp);
                }
            }

            if (notifyOnChanged)
            {
                OnCurveChanged();
                onControlPointSelectionChanged?.Invoke();
            }
        }
  
        public void OnCurveChanged()
        {
            ApplyTangentModeAndKeyBrokenToKeyframes();
            onCurveChanged?.Invoke();
        }

        
        /// <summary>
        /// ControlPointのTangentModeに応じてKeyframeのTangentを計算する
        /// TangentModeがLinearなら隣接するKeyframeとの傾きを計算して設定する必要があるため、ControlPoint内ではなくて
        /// ControlPointHolderでまとめて行う
        /// </summary>
        private void ApplyTangentModeAndKeyBrokenToKeyframes()
        {
            var curve = Curve;
            if (curve == null || ControlPoints.Count == 0) return;
            
            for (var i = 0; i < ControlPoints.Count; i++)
            {
                var controlPoint = ControlPoints[i];
                if (controlPoint == null) continue;

                var keyframe = curve[i];
                var current = keyframe.GetPosition();
                if (i > 0 && controlPoint.InTangentMode == TangentMode.Linear)
                {
                    // Set the tangent to the slope between the previous key and this key
                    var prev = curve[i - 1].GetPosition();
                    keyframe.inTangent = (current.y - prev.y) / (current.x - prev.x);
                }
                else if (controlPoint.InTangentMode == TangentMode.Constant)
                {
                    keyframe.inTangent = float.PositiveInfinity;
                }

                if (i < ControlPoints.Count - 1 && controlPoint.OutTangentMode == TangentMode.Linear)
                {
                    // Set the tangent to the slope between this key and the next key
                    var next = curve[i + 1].GetPosition();
                    keyframe.outTangent = (next.y - current.y) / (next.x - current.x);
                }
                else if (controlPoint.OutTangentMode == TangentMode.Constant)
                {
                    keyframe.outTangent = float.PositiveInfinity;
                }

                Curve.MoveKey(i, keyframe);
                
#if UNITY_EDITOR
                curve.SetKeyBroken(i, controlPoint.IsKeyBroken);
                curve.SetTangentMode(i, controlPoint.InTangentMode, controlPoint.OutTangentMode);
#endif
            }
        }
    }
}