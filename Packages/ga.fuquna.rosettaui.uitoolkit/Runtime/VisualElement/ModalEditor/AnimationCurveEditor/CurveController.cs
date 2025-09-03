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
        public event Action onControlPointChanged;
        
        private readonly VisualElement _parent;
        private readonly Func<CurveController, ControlPoint> _getNewControlPoint;

        public AnimationCurve Curve { get; private set; } = new();
        public IEnumerable<ControlPoint> SelectedControlPoints => ControlPoints.Where(t => t.IsActive);


        private List<ControlPoint> ControlPoints { get; } = new();

        public CurveController(VisualElement parent, Func<CurveController, ControlPoint> getNewControlPoint)
        {
            _parent = parent;
            _getNewControlPoint = getNewControlPoint;
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
                controlPoint.SetInTangentMode(inTangentMode, false);
            }
            if (GetControlPointRight(controlPoint)?.InTangentMode is {} outTangentMode)
            {
                controlPoint.SetOutTangentMode(outTangentMode, false);
            }
            
            OnCurveChanged();
            onControlPointChanged?.Invoke();
            
            return index;
        }
        
        public void RemoveControlPoint(ControlPoint controlPoint)
        {
            if (controlPoint == null) return;
            
            DoRemoveControlPoint(controlPoint);
            OnCurveChanged();
            onControlPointChanged?.Invoke();
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
            onControlPointChanged?.Invoke();
        }
        
        private void DoRemoveControlPoint(ControlPoint controlPoint)
        {
            controlPoint.RemoveFromHierarchy();
            
            var index = ControlPoints.IndexOf(controlPoint);
            Curve.RemoveKey(index);
            ControlPoints.RemoveAt(index);
        }
        
        public void ShowEditKeyPopupOfSelectedControlPoint()
        {
            SelectedControlPoints.FirstOrDefault()?.ShowEditKeyPopup();
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
            
            onControlPointChanged?.Invoke();
        }
        
        public void SelectControlPoint(ControlPoint controlPoint, bool preserveOtherSelection = false)
        {
            if (!ControlPoints.Contains(controlPoint)) return;

            if (!preserveOtherSelection)
            {
                DoUnselectAllControlPoints();
            }

            controlPoint.IsActive = true;
            onControlPointChanged?.Invoke();
        }

        public void UnselectAllControlPoints()
        {
            DoUnselectAllControlPoints();
            onControlPointChanged?.Invoke();
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
                controlPoint.SetKeyBroken(Curve.GetKeyBroken(i), false);
                controlPoint.SetInTangentMode(Curve.GetInTangentMode(i), false);
                controlPoint.SetOutTangentMode(Curve.GetOutTangentMode(i), false);
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
        public void UpdateKeyframe(ControlPoint controlPoint, Keyframe keyframe)
        {
            if (controlPoint == null || Curve == null || ControlPoints.Count == 0)
            {
                return;
            }

            var index = ControlPoints.IndexOf(controlPoint);
            if (index < 0 || index >= Curve.keys.Length)
            {
                return;
            }

            var newIndex = Curve.MoveKey(index, keyframe);
            if (newIndex != index)
            {
                // Indexが変わったのでControlPointsの順番も入れ替える
                var temp = ControlPoints[index];
                ControlPoints.RemoveAt(index);
                ControlPoints.Insert(newIndex, temp);
            }

            OnCurveChanged();
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