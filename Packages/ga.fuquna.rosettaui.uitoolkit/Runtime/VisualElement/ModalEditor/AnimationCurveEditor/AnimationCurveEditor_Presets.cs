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
        
        private AnimationCurveEditorPresetSet _presetSet;
        private PresetsPreview _presetsPreview;
        
        private void InitPresetsUI()
        {
            var scrollView = this.Q<ScrollView>("presets-scroll-view");
            var container = new VisualElement();
            container.AddToClassList(PresetsContainerClassName);
            scrollView.Add(container);
            
            var popupButton = new VisualElement();
            popupButton.AddToClassList(PopupButtonClassName);
            popupButton.RegisterCallback<PointerDownEvent>(evt => Debug.Log("PopupButton clicked"));
            
            container.Add(popupButton);
            

            
            _presetSet = new AnimationCurveEditorPresetSet(curve =>
            {
                CopiedValue = curve;
                NotifyEditorValueChanged();
            });
            
            PlayerPrefs.DeleteKey("RosettaUI-AnimationCurvePresetSet-Swatches");
            
            var persistentService = _presetSet.PersistentService;
            if (!persistentService.HasSwatches)
            {
                persistentService.SaveSwatches(new[]
                {
                    new SwatchPersistentService<AnimationCurve>.NameAndValue()
                    {
                        value = AnimationCurve.Constant(0f, 1f, 0f)
                    },
                    new SwatchPersistentService<AnimationCurve>.NameAndValue()
                    {
                        value = AnimationCurve.Linear(0f, 0f, 1f, 1f)
                    },
                });
                // Add default presets if there are no saved presets
                // foreach (var preset in DefaultPresets)
                // {
                //     _presetSet.AddSwatch(preset);
                // }
            }
            
            _presetsPreview = new PresetsPreview(persistentService);
            container.Add(_presetsPreview);
            // scrollView.Add(_presetSet);
        }
    }
}