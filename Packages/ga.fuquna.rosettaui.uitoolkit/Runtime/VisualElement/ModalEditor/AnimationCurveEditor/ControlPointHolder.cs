using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Manage AnimationCurve and ControlPoints
    /// </summary>
    public class ControlPointHolder : IEnumerable<(Keyframe key, ControlPoint point)>
    {
        public event Action onCurveChanged;
        
        public AnimationCurve Curve { get; private set; }

        public List<ControlPoint> ControlPoints { get; } = new();

        public bool IsEmpty => Curve == null || Curve.keys.Length == 0;
        public int Count => Curve?.keys.Length ?? 0;

        
        private Keyframe[] _keyframesCache;

        private Keyframe[] Keyframes
        {
            get
            {
                if (_keyframesCache == null)
                {
                    if (Curve == null || Curve.keys.Length == 0) return Array.Empty<Keyframe>();
                    _keyframesCache = Curve.keys;
                }
                
                return _keyframesCache;
            }
        }
        
        public (Keyframe key, ControlPoint point) this [int index] 
        {
            get {
                if (Curve == null || index < 0 || index >= Curve.keys.Length) return (default, default);
                return (Curve.keys[index], ControlPoints[index]);
            }
        }
        
        public ControlPoint SelectedControlPoint => ControlPoints.FirstOrDefault(t => t.IsActive);

        private readonly VisualElement _parent;
        private readonly Func<ControlPointHolder, ControlPoint> _getNewControlPoint;
        
        public ControlPointHolder(VisualElement parent, Func<ControlPointHolder, ControlPoint> getNewControlPoint)
        {
            _parent = parent;
            _getNewControlPoint = getNewControlPoint;
        }
        
        public void SetCurve(AnimationCurve curve)
        {
            // 外部での変更を受けないようにClone
            Curve = AnimationCurveHelper.Clone(curve);
            
            RemoveKeyframeCache();
            ResetControlPoints();
        }
        
        public Keyframe GetKeyframe(ControlPoint controlPoint)
        {
            if (controlPoint == null) return default;
            var index = ControlPoints.IndexOf(controlPoint);
            if (index < 0 || index >= Keyframes.Length) return default;
            return Keyframes[index];
        }
        
        private ControlPoint GetControlPoint(int index)
        {
            if (index < 0 || index >= ControlPoints.Count) return null;
            return ControlPoints[index];
        }
        
        public ControlPoint GetControlPointLeft(ControlPoint controlPoint)
        {
            if (controlPoint == null) return null;
            var index = ControlPoints.IndexOf(controlPoint);
            if (index <= 0) return null;
            return GetControlPoint(index - 1);
        }
        
        public ControlPoint GetControlPointRight(ControlPoint controlPoint)
        {
            if (controlPoint == null) return null;
            var index = ControlPoints.IndexOf(controlPoint);
            if (index < 0 || index >= ControlPoints.Count - 1) return null;
            return GetControlPoint(index + 1);
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
            var inTangentMode = GetControlPointLeft(controlPoint)?.OutTangentMode;
            var outTangentMode = GetControlPointLeft(controlPoint)?.InTangentMode;
            controlPoint.SetTangentMode(inTangentMode, outTangentMode);
            
            OnCurveChanged();
            
            return index;
        }

        
        public void RemoveSelectedControlPoint()
        {
            if (ControlPoints.Count <= 1) return;
            
            var controlPoint = SelectedControlPoint;
            if (controlPoint == null) return;
            
            controlPoint.RemoveFromHierarchy();
            
            var index = ControlPoints.IndexOf(controlPoint);
            Curve.RemoveKey(index);
            ControlPoints.RemoveAt(index);
            
            OnCurveChanged();
        }

        
        public void SelectControlPoint(ControlPoint controlPoint)
        {
            if ( !ControlPoints.Contains(controlPoint) ) return;
            
            UnselectAllControlPoints();
            controlPoint.IsActive = true;
        }
        
        public void UnselectAllControlPoints()
        {
            foreach (var cp in ControlPoints)
            {
                cp.IsActive = false;
            }
        }
        
                
        public void UpdateView()
        {
            if (IsEmpty) return;
            AnimationCurveEditorUtility.ApplyTangentMode(this);
            foreach(var controlPoint in ControlPoints)
            {
                controlPoint.UpdateView();
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

        private void RemoveKeyframeCache()
        {
            _keyframesCache = null;
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
                var key = Curve.keys[i];
                var controlPoint = CreateControlPoint(i);
                controlPoint.SetPointMode(key.GetPointMode());
                controlPoint.SetTangentMode(Curve.GetInTangentMode(i), Curve.GetOutTangentMode(i));
            }
        }
        
        private ControlPoint CreateControlPoint(int index)
        {
            var controlPoint = _getNewControlPoint(this);
            _parent.Add(controlPoint);
            
            ControlPoints.Insert(index, controlPoint);
            
            return controlPoint;
        }

        /// <summary>
        /// ControlPointのKeyframeの時間が変更されたらAnimationCurve上のIndexが変わる可能性がある
        /// Indexが変わるなら追随する
        /// </summary>
        public void UpdateKeyframe(ControlPoint controlPoint, in Keyframe keyframe)
        {
            if (controlPoint == null || Curve == null || ControlPoints.Count == 0)
            {
                return;
            }

            var index = ControlPoints.IndexOf(controlPoint);

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
        
        public void UpdateKeyframePosition(ControlPoint controlPoint, Vector2 keyframePosition)
        {
            if (controlPoint == null || Curve == null || ControlPoints.Count == 0)
            {
                return;
            }

            var keyframe = controlPoint.Keyframe;
            keyframe.time = keyframePosition.x;
            keyframe.value = keyframePosition.y;
            
            UpdateKeyframe(controlPoint, keyframe);
        }
        
        private void OnCurveChanged()
        {
            RemoveKeyframeCache();
            onCurveChanged?.Invoke();
        }
    }
}