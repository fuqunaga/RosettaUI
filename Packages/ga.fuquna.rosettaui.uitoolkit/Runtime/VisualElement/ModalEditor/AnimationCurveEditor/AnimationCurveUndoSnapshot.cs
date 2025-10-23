using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Utilities;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// AnimationCurveとIsBroken、TangentModeのスナップショットを取るクラス
    /// IsBrokenとTangentModeはランタイムではAnimationCurveに含まれないので別途管理する
    /// </summary>
    public class AnimationCurveUndoSnapshot : ObjectPoolItem<AnimationCurveUndoSnapshot>
    {
        public struct ExtraData : IEquatable<ExtraData>
        {
            public bool isBroken;
            public TangentMode inTangentMode;
            public TangentMode outTangentMode;

            public bool Equals(ExtraData other)
            {
                return isBroken == other.isBroken && inTangentMode == other.inTangentMode && outTangentMode == other.outTangentMode;
            }

            public override bool Equals(object obj)
            {
                return obj is ExtraData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(isBroken, (int)inTangentMode, (int)outTangentMode);
            }
        }

        public readonly AnimationCurve curve = new();
        public readonly List<ExtraData> extraDataList = new();
        
        public void Initialize(AnimationCurve currentCurve, IEnumerable<ControlPoint> controlPoints)
        {
            AnimationCurveHelper.Copy(currentCurve, curve);
            
            extraDataList.Clear();
            extraDataList.AddRange(controlPoints.Select(cp => new ExtraData()
            {
                isBroken = cp.IsKeyBroken,
                inTangentMode = cp.InTangentMode,
                outTangentMode = cp.OutTangentMode
            }));
        }

        public bool Equals(AnimationCurveUndoSnapshot other)
        {
            if (other == null) return false;
            if (!curve.Equals(other.curve)) return false;
            
            return extraDataList.SequenceEqual(other.extraDataList);
        }
    }
}