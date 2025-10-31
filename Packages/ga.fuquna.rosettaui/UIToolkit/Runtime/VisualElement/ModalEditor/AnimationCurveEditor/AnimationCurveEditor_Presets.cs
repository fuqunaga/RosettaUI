using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Swatch;
using RosettaUI.UndoSystem;
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
        
        private PresetsPopup _presetsPopup;
        private PresetsPreview _presetsPreview;
        
        
        private void InitPresetsUI()
        {
            _presetsPopup = new PresetsPopup(SetCurveFromPreset);
            _presetsPopup.RegisterCallback<AttachToPanelEvent>(_ => _presetsPopup.Hide());
            Add(_presetsPopup);
            
            var scrollView = this.Q<ScrollView>("presets-scroll-view");
            var container = new VisualElement();
            container.AddToClassList(PresetsContainerClassName);
            scrollView.Add(container);
            
            var popupButton = new VisualElement();
            popupButton.AddToClassList(PopupButtonClassName);
            popupButton.RegisterCallback<PointerDownEvent>(OnPopupButtonPointerDown);
            
            container.Add(popupButton);
            

            _presetsPreview = new PresetsPreview(_presetsPopup.PersistentService, SetCurveFromPreset);
            container.Add(_presetsPreview);
            
            return;

            
            void OnPopupButtonPointerDown(PointerDownEvent evt)
            {
                var rect = popupButton.layout;
                var popupLeftBottom = new Vector2(rect.xMin, rect.yMax);
                
                _presetsPopup.Show(popupLeftBottom, _curveController.Curve);
                
                evt.StopPropagation();
            }
            
                    
            void SetCurveFromPreset(AnimationCurve curve)
            {   
                if (curve.Equals(_curveController.Curve))
                {
                    return;
                }

                _curveController.Command.SetCurve(curve);
                
                UpdateView();
                NotifyEditorValueChanged();
            }
        }
    }
}