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
    public class CurvePointContainer : IEnumerable<(Keyframe key, ControlPoint point)>
    {
        public AnimationCurve Curve => _curve;
        public List<ControlPoint> ControlPoints => _controlPoints;
        public bool IsEmpty => _curve == null || _curve.keys.Length == 0;
        public int Count => _curve?.keys.Length ?? 0;
        public (Keyframe key, ControlPoint point) this [int index] 
        {
            get {
                if (_curve == null || index < 0 || index >= _curve.keys.Length) return (default, default);
                return (_curve.keys[index], _controlPoints[index]);
            }
        }
        
        private AnimationCurve _curve;
        private readonly List<ControlPoint> _controlPoints = new();
        
        private readonly VisualElement _parent;
        private readonly Func<ControlPoint> _getNewControlPoint;
        
        public CurvePointContainer(VisualElement parent, Func<ControlPoint> getNewControlPoint)
        {
            _parent = parent;
            _getNewControlPoint = getNewControlPoint;
        }
        
        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
            ResetControlPoints();
        }

        public int AddKey(Keyframe key)
        {
            if (IsEmpty) return -1;
            int idx = _curve.AddKey(key);
            if (idx < 0) return idx;
            
            // Add control point
            var controlPoint = _getNewControlPoint();
            _parent.Add(controlPoint);
            controlPoint.SetKeyframe(_curve, idx);
            controlPoint.SetActive(true);
            _controlPoints.Insert(idx, controlPoint);
            
            return idx;
        }
        
        public int MoveKey(int index, Keyframe key)
        {
            if (IsEmpty || index < 0) return index;

            for (var i = 0; i < _curve.keys.Length; i++)
            {
                if (i == index) continue;
                if (_curve.keys[i].time == key.time) return index;
            }
            
            int newIdx = _curve.MoveKey(index, key);
            if (newIdx != index)
            {
                // Swap control points
                var temp = _controlPoints[index];
                _controlPoints.RemoveAt(index);
                _controlPoints.Insert(newIdx, temp);
                index = newIdx;
            }
            
            return index;
        }
        
        public void RemoveKey(int index)
        {
            if (IsEmpty || index < 0 || index >= _curve.keys.Length || _curve.keys.Length <= 1) return;
            _curve.RemoveKey(index);
            var controlPoint = _controlPoints[index];
            _controlPoints.RemoveAt(index);
            _parent.Remove(controlPoint);
        }
        
        public void UpdateControlPoints()
        {
            if (IsEmpty) return;
            AnimationCurveEditorUtility.ApplyTangentMode(this);
            for (var i = 0; i < _curve.keys.Length; i++)
            {
                _controlPoints[i].SetKeyframe(_curve, i);
            }
        }
        
        public void SelectControlPoint(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) return;
            for (var i = 0; i < _controlPoints.Count; i++)
            {
                _controlPoints[i].SetActive(i == index);
            }
        }
        
        public void UnselectAllControlPoints()
        {
            foreach (var cp in _controlPoints)
            {
                cp.SetActive(false);
            }
        }
        
        public IEnumerator<(Keyframe key, ControlPoint point)> GetEnumerator()
        {
            return _curve.keys.Select((t, i) => (t, _controlPoints[i])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        private void ResetControlPoints()
        {
            if (_curve == null) return;
            foreach (var cp in _controlPoints)
            {
                _parent.Remove(cp);
            }
            _controlPoints.Clear();
            for (var i = 0; i < _curve.keys.Length; i++)
            {
                var key = _curve.keys[i];
                var controlPoint = _getNewControlPoint();
                _controlPoints.Add(controlPoint);
                _parent.Insert(0, controlPoint);
                controlPoint.SetKeyframe(_curve, i);
                controlPoint.SetPointMode(key.GetPointMode());
                controlPoint.SetTangentMode(_curve.GetInTangentMode(i), _curve.GetOutTangentMode(i));
                controlPoint.SetWeightedMode(key.weightedMode);
            }
        }
        
    }
}