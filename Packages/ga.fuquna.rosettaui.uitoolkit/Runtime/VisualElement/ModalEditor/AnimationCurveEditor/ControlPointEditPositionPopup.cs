using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// ControlPointの編集用ポップアップ 
    /// </summary>
    public class ControlPointEditPositionPopup : EventBlocker
    {
        public static string VisualTreeAssetName { get; set; } = "RosettaUI_AnimationCurve_ControlPointEditPositionPopup";

        private ControlPoint _controlPoint;
        
        private VisualElement _editKeyPopupRoot;
        private FloatField _timeField;
        private FloatField _valueField;

        public ControlPointEditPositionPopup()
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
                    if (_controlPoint != null)
                    {
                        _controlPoint.KeyframePosition = new Vector2(_timeField.value, _valueField.value);
                    }
                    Hide();
                });
            }, TrickleDown.TrickleDown);
        }

        public void Show(ControlPoint controlPoint)
        {
            InitializeIfNeeded();
            
            _controlPoint = controlPoint;
            
            var position = _controlPoint.KeyframePosition;
            _timeField.value = position.x;
            _valueField.value = position.y;
            
            _editKeyPopupRoot.style.left = controlPoint.style.left;
            _editKeyPopupRoot.style.top = controlPoint.style.top;
            
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
            style.display = DisplayStyle.None;
        }
    }
}