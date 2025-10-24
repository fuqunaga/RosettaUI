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

        private CurveController _curveController;
        private VisualElement _focusBackElement;
        
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

            RegisterCallback<PointerDownEvent>(HideByEvent);
            RegisterCallback<KeyDownEvent>(evt =>
            {
                switch (evt.keyCode)
                {
                    case KeyCode.Escape:
                        HideByEvent(evt);
                        break;
                    
                    case KeyCode.Return or KeyCode.KeypadEnter:
                        _curveController?.CommandForSelection.SetKeyframePosition(
                            _timeField.showMixedValue ? null : _timeField.value,
                            _valueField.showMixedValue ? null : _valueField.value
                        );
                        HideByEvent(evt);
                        break;
                }
            }, TrickleDown.TrickleDown);

            this.AddManipulator(new FocusTrapManipulator());

            return;
            
            void HideByEvent(EventBase evt)
            {
                Hide();
                evt.StopPropagation();
            }
        }
        
        public void Show(Vector2 localPosition, CurveController curveController, VisualElement focusBackElement)
        {
            if (!curveController.SelectedControlPoints.Any())
            {
                return;
            }
            
            InitializeIfNeeded();
            
            _curveController = curveController;
            _focusBackElement = focusBackElement;

            using var _ = ListPool<Vector2>.Get(out var keyframePositions);
            keyframePositions.AddRange(
                _curveController.SelectedControlPoints.Select(cp => cp.KeyframePosition)
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
            

            _editKeyPopupRoot.style.left = localPosition.x;
            _editKeyPopupRoot.style.top = localPosition.y;
            
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
            _focusBackElement?.Focus();
            
            _curveController = null;
            _focusBackElement = null;
            style.display = DisplayStyle.None;
        }
    }
}