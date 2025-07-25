using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Manage AnimationCurve and ControlPoints
    /// </summary>
    public class ControlPointHolder : IEnumerable<(Keyframe key, ControlPoint point)>
    {
        public AnimationCurve Curve { get; private set; }

        public List<ControlPoint> ControlPoints { get; } = new();

        public bool IsEmpty => Curve == null || Curve.keys.Length == 0;
        public int Count => Curve?.keys.Length ?? 0;
        
        public (Keyframe key, ControlPoint point) this [int index] 
        {
            get {
                if (Curve == null || index < 0 || index >= Curve.keys.Length) return (default, default);
                return (Curve.keys[index], ControlPoints[index]);
            }
        }
        
        public ControlPoint SelectedControlPoint => ControlPoints.FirstOrDefault(t => t.IsActive);

        private readonly VisualElement _parent;
        private readonly Func<ControlPoint> _getNewControlPoint;
        
        public ControlPointHolder(VisualElement parent, Func<ControlPoint> getNewControlPoint)
        {
            _parent = parent;
            _getNewControlPoint = getNewControlPoint;
        }
        
        public void SetCurve(AnimationCurve curve)
        {
            Curve = curve;
            ResetControlPoints();
        }

        public int AddKey(Keyframe key)
        {
            if (IsEmpty) return -1;
            var idx = Curve.AddKey(key);
            if (idx < 0) return idx;
            
            // Add control point
            var controlPoint = _getNewControlPoint();
            _parent.Add(controlPoint);
            controlPoint.SetKeyframe(Curve, idx);
            controlPoint.IsActive = true;
            ControlPoints.Insert(idx, controlPoint);
            
            return idx;
        }
        
        public int MoveKey(int index, Keyframe key)
        {
            if (IsEmpty || index < 0) return index;

            for (var i = 0; i < Curve.keys.Length; i++)
            {
                if (i == index) continue;
                if (Curve.keys[i].time == key.time) return index;
            }
            
            var newIdx = Curve.MoveKey(index, key);
            if (newIdx != index)
            {
                // Swap control points
                var temp = ControlPoints[index];
                ControlPoints.RemoveAt(index);
                ControlPoints.Insert(newIdx, temp);
                index = newIdx;
            }
            
            return index;
        }
        
        public void RemoveKey(int index)
        {
            if (IsEmpty || index < 0 || index >= Curve.keys.Length || Curve.keys.Length <= 1) return;
            Curve.RemoveKey(index);
            var controlPoint = ControlPoints[index];
            ControlPoints.RemoveAt(index);
            _parent.Remove(controlPoint);
        }
        
        public void UpdateControlPoints()
        {
            if (IsEmpty) return;
            AnimationCurveEditorUtility.ApplyTangentMode(this);
            for (var i = 0; i < Curve.keys.Length; i++)
            {
                ControlPoints[i].SetKeyframe(Curve, i);
            }
        }
        
        public void SelectControlPoint(int index)
        {
            if (index < 0 || index >= ControlPoints.Count) return;
            for (var i = 0; i < ControlPoints.Count; i++)
            {
                ControlPoints[i].IsActive = (i == index);
            }
        }
        
        public void UnselectAllControlPoints()
        {
            foreach (var cp in ControlPoints)
            {
                cp.IsActive = false;
            }
        }
        
        public IEnumerator<(Keyframe key, ControlPoint point)> GetEnumerator()
        {
            return Curve.keys.Select((t, i) => (t, ControlPoints[i])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        private void ResetControlPoints()
        {
            if (Curve == null) return;
            foreach (var cp in ControlPoints)
            {
                _parent.Remove(cp);
            }
            ControlPoints.Clear();
            for (var i = 0; i < Curve.keys.Length; i++)
            {
                var key = Curve.keys[i];
                var controlPoint = _getNewControlPoint();
                ControlPoints.Add(controlPoint);
                _parent.Insert(0, controlPoint);
                controlPoint.SetKeyframe(Curve, i);
                controlPoint.SetPointMode(key.GetPointMode());
                controlPoint.SetTangentMode(Curve.GetInTangentMode(i), Curve.GetOutTangentMode(i));
                controlPoint.SetWeightedMode(key.weightedMode);
            }
        }
        
    }
}