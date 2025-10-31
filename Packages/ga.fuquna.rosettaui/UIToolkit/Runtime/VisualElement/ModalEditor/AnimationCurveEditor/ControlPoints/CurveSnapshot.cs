using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Keyframeのスナップショットを取り、復元するクラス
    ///
    /// ControlPointsドラッグ中に既存のKeyframeと同じtimeになると既存のものが上書きされるが、
    /// ドラッグが続き移動した場合は既存Keyframeを復元するために使用する
    ///
    /// 選択中のControlPoint同士の上書きも発生する
    /// - SelectedControlPointsRectのハンドルでの拡大縮小時
    /// </summary>
    public class CurveSnapshot
    {
        private readonly CurveController _curveController;
        private readonly List<(ControlPoint, Keyframe)> _controlPointAndKeyframes = new();
        
        public CurveSnapshot(CurveController curveController)
        {
            _curveController = curveController;
        }
        
        public void TakeSnapshot()
        {
            _controlPointAndKeyframes.Clear();
            _controlPointAndKeyframes.AddRange(
                _curveController.ControlPoints.Select(cp => (cp, cp.Keyframe))
            );
        }

        public void ApplySnapshotWithoutSelection()
        {
            for (var i = 0; i < _controlPointAndKeyframes.Count; i++)
            {
                var (controlPoint, keyframe) = _controlPointAndKeyframes[i];
                
                // IsActiveなControlPointは選択中なので無視
                // ただしIsActiveでもparent==nullの場合は削除されてるので復元対象
                if (controlPoint.parent != null && controlPoint.IsActive) continue;
                

                var newControlPoint = _curveController.AddKeyframe(keyframe);
                if (newControlPoint == null) continue;

                // Replace control point in snapshot to keep reference
                newControlPoint.IsActive = controlPoint.IsActive;
                _controlPointAndKeyframes[i] = (newControlPoint, keyframe);
            }
        }
    }
}