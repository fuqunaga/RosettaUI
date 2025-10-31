using System;
using RosettaUI.Swatch;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// AnimationCurveEditor下部のプリセット表示部分
    /// </summary>
    public class PresetsPreview : VisualElement
    {
        private static readonly string USSClassName = $"{AnimationCurveEditor.USSClassName}__presets-preview";
        
        private readonly SwatchPersistentService<AnimationCurve> _persistentService;
        private readonly Action<AnimationCurve> _applyValueFunc;
        
        public PresetsPreview(SwatchPersistentService<AnimationCurve> persistentService, Action<AnimationCurve> applyValueFunc)
        {
            _persistentService = persistentService;
            _applyValueFunc = applyValueFunc;
            
            AddToClassList(USSClassName);
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ResetView();
            _persistentService.onSaveSwatches += ResetView;
        }


        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            _persistentService.onSaveSwatches -= ResetView;
        }

        private void ResetView()
        {
            // remove all children
            Clear();

            var nameAndValues = _persistentService.LoadSwatches() ?? Array.Empty<NameAndValue<AnimationCurve>>();
            
            foreach (var nameAndValue in nameAndValues)
            {
                var preset = new Preset()
                {
                    Value = nameAndValue.value,
                    Label = nameAndValue.name
                };
                
                preset.RegisterCallback<PointerDownEvent>(_ => _applyValueFunc?.Invoke(preset.Value));
                
                Add(preset);
            }
        }
    }
}