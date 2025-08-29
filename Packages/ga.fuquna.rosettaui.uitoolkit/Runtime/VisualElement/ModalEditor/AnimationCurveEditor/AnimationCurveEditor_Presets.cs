using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// Preset UI for AnimationCurveEditor
    /// </summary>
    public partial class AnimationCurveEditor
    {
        private const string PresetsContainerClassName = USSClassName + "__presets-container";
        private const string PopupButtonClassName = USSClassName + "__presets-popup-button";
        
        private PresetsPreview _presetsPreview;
        private PresetsPopup _presetsPopup;
        
        private SwatchPersistentService<AnimationCurve> PersistentService => _presetsPopup.PersistentService;
        
        private void InitPresetsUI()
        {
            _presetsPopup = new PresetsPopup(SetCurveFromPreset);
            Add(_presetsPopup);
            
            var scrollView = this.Q<ScrollView>("presets-scroll-view");
            var container = new VisualElement();
            container.AddToClassList(PresetsContainerClassName);
            scrollView.Add(container);
            
            var popupButton = new VisualElement();
            popupButton.AddToClassList(PopupButtonClassName);
            popupButton.RegisterCallback<PointerDownEvent>(OnPopupButtonPointerDown);
            
            container.Add(popupButton);
            

            _presetsPreview = new PresetsPreview(PersistentService, SetCurveFromPreset);
            container.Add(_presetsPreview);
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            return;

            
            void OnPopupButtonPointerDown(PointerDownEvent evt)
            {
                var rect = popupButton.layout;
                var popupLeftBottom = new Vector2(rect.xMin, rect.yMax);
                _presetsPopup.Show(popupLeftBottom);
                
                evt.StopPropagation();
            }
            
            
            void SetCurveFromPreset(AnimationCurve curve)
            {
                if (curve.Equals(_curveController.Curve))
                {
                    return;
                }
                
                _curveController.SetCurve(curve);
                UnselectAllControlPoint();
                UpdateView();
                NotifyEditorValueChanged();
            }
        }

        // 表示時にプリセットがなかったらデフォルトプリセットを保存しておく
        // 空の状態で保存されているケースとは異なるので注意
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            var persistentService = PersistentService;
            if (!persistentService.HasSwatches)
            {
                persistentService.SaveSwatches(AnimationCurveHelper.FactoryPresets.Select(curve => new Preset { Value = curve }));
            }
        }
    }
}