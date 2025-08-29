using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class PresetsPopup : EventBlocker
    {
        private readonly AnimationCurveEditorPresetSet _presetSet;
        
        public SwatchPersistentService<AnimationCurve> PersistentService => _presetSet.PersistentService;
        
        public PresetsPopup(Action<AnimationCurve> applyValueFunc)
        {
            _presetSet = new AnimationCurveEditorPresetSet(applyValueFunc)
            {
                value = true
            };
            
            Add(_presetSet);
            
            Hide();
        }

        public void Show(Vector2 leftBottom)
        {
            var st = _presetSet.style;
            st.left = leftBottom.x;
            st.bottom = leftBottom.y;
            st.position = Position.Absolute;
            
            style.display = DisplayStyle.Flex;
        }
        
        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
    }
}