using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// ControlPointの編集用ポップアップ 
    /// </summary>
    public class ControlPointsEditPositionPopup : EventBlocker
    {
        public static string VisualTreeAssetName { get; set; } = "RosettaUI_AnimationCurve_ControlPointEditPositionPopup";

        private VisualElement _editKeyPopupRoot;
        private FloatField _timeField;
        private FloatField _valueField;

        private SelectedControlPointsEditor _controlPointsEditor;
        
        public ControlPointsEditPositionPopup()
        {
            Hide();
        }
        
        private void InitializeIfNeeded()
        {
            if (_timeField != null && _valueField != null)
            {
                return; // Already initialized
            }
            
            var visualTree = Resources.Load<VisualTreeAsset>(VisualTreeAssetName);
            if (visualTree != null)
            {
                visualTree.CloneTree(this);
            }
            else
            {
                Debug.LogError($"Visual tree asset not found: {VisualTreeAssetName}");
            }
            
            _editKeyPopupRoot = this.Q<VisualElement>("EditKeyPopupRoot");
            _timeField = this.Q<FloatField>("TimeField");
            _valueField = this.Q<FloatField>("ValueField");

            RegisterCallback<PointerDownEvent>(_ => Hide());
            
            // NavigationイベントはFloatFieldが止めてしまうので、TrickleDownで受け取る
            RegisterCallback<NavigationCancelEvent>(_ => Hide(), TrickleDown.TrickleDown);
            RegisterCallback<NavigationSubmitEvent>(_ =>
            {
                // FloatFieldのイベント後にSubmitを実行
                schedule.Execute(() =>
                {
                    _controlPointsEditor?.UpdateKeyframePosition(
                        _timeField.showMixedValue ? null : _timeField.value,
                        _valueField.showMixedValue ? null : _valueField.value
                    );
                    Hide();
                });
            }, TrickleDown.TrickleDown);
        }
        
        public void Show(Vector2 popupPosition, SelectedControlPointsEditor selectedControlPointsEditor)
        {
            if (selectedControlPointsEditor.IsEmpty)
            {
                return;
            }
            
            InitializeIfNeeded();
            
            _controlPointsEditor = selectedControlPointsEditor;

            using var _ = ListPool<Vector2>.Get(out var keyframePositions);
            keyframePositions.AddRange(
                _controlPointsEditor.ControlPoints.Select(cp => cp.KeyframePosition)
            );
            
            var hasMultipleValueX = keyframePositions.Select(p => p.x).Distinct().Skip(1).Any();
            var hasMultipleValueY = keyframePositions.Select(p => p.y).Distinct().Skip(1).Any();

            _timeField.showMixedValue = hasMultipleValueX;
            _valueField.showMixedValue = hasMultipleValueY;
            if (!hasMultipleValueX)
            {
                _timeField.value = keyframePositions[0].x;
            }
            if (!hasMultipleValueY)
            {
                _valueField.value = keyframePositions[0].y;
            }
            

            _editKeyPopupRoot.style.left = popupPosition.x;
            _editKeyPopupRoot.style.top = popupPosition.y;
            
            style.display = DisplayStyle.Flex;
            
            _editKeyPopupRoot.RegisterCallbackOnce<GeometryChangedEvent>(_ =>
            {
                _editKeyPopupRoot.ClampElementToParent();
            });
            
            schedule.Execute(() =>
            {
                _valueField.Focus();
            });
        }
        
        public void Hide()
        {
            _controlPointsEditor = null;
            style.display = DisplayStyle.None;
        }
    }
}