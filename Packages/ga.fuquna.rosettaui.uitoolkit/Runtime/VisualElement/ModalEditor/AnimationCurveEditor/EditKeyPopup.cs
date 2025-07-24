using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// ControlPointの編集用ポップアップ 
    /// </summary>
    public class EditKeyPopup : EventBlocker
    {
        public static string VisualTreeAssetName { get; set; } = "RosettaUI_AnimationCurve_EditKeyPopup";

        private VisualElement _editKeyPopupRoot;
        private FloatField _timeField;
        private FloatField _valueField;

        private Action<Keyframe> _onSubmit;
        
        public EditKeyPopup(Action<Keyframe> onSubmit)
        {
            _onSubmit = onSubmit;
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
                    _onSubmit?.Invoke(new Keyframe(_timeField.value, _valueField.value));
                    Hide();
                });
            }, TrickleDown.TrickleDown);
        }

        public void Show(ControlPoint controlPoint)
        {
            InitializeIfNeeded();
            
            var keyframe = controlPoint.Keyframe;
            _timeField.value = keyframe.time;
            _valueField.value = keyframe.value;
            
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