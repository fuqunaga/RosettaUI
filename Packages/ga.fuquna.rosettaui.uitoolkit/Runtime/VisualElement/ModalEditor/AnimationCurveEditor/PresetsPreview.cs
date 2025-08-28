using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class PresetsPreview : VisualElement
    {
        private static readonly string USSClassName = $"{AnimationCurveEditor.USSClassName}__presets-preview";
        
        public PresetsPreview(SwatchPersistentService<AnimationCurve> persistentService)
        {
            AddToClassList(USSClassName);
            Reset();

            persistentService.onSaveSwatches += Reset;
            return;

            void Reset() => ResetPresets(persistentService.LoadSwatches() ?? Array.Empty<SwatchPersistentService<AnimationCurve>.NameAndValue>());
        }
        
        private void ResetPresets(IEnumerable<SwatchPersistentService<AnimationCurve>.NameAndValue> nameAndValues)
        {
            // remove all children
            Clear();
            
            foreach (var nameAndValue in nameAndValues)
            {
                var preset = new Preset()
                {
                    Value = nameAndValue.value,
                    Label = nameAndValue.name
                };
                
                Add(preset);
            }
        }
    }
}