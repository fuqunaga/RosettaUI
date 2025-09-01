using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Swatch;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class PresetsPopup : EventBlocker
    {
        private const string USSClassName = "rosettaui-animation-curve-editor__presets-popup";
        
        public const string KeyPrefix = "RosettaUI-AnimationCurvePresetSet";
        
        private readonly SwatchSetMenuAndTileView<AnimationCurve, Preset> _swatchSetMenuAndTileView;
        private readonly VisualElement _root;
        
        public AnimationCurveSwatchPersistentService PersistentService { get; } = new(KeyPrefix);
        
        public PresetsPopup(Action<AnimationCurve> applyValueFunc)
        {
            _root = new VisualElement();
            _root.AddToClassList(USSClassName);
            _root.AddBoxShadow();
            
            var row = new VisualElement(){style = { flexDirection = FlexDirection.Row }};
            var label = new Label("Presets");
            row.Add(label);
            _root.Add(row);
            
            _swatchSetMenuAndTileView = new SwatchSetMenuAndTileView<AnimationCurve, Preset>(row, _root, applyValueFunc, PersistentService, AddMenuItems);
            _swatchSetMenuAndTileView.SetMenuButtonVisible(true);
            
            Add(_root);

            
            Hide();
            
            RegisterCallback<PointerDownEvent>(e =>
            {
                Hide();
                e.StopPropagation();
            });
            
            return;

            IEnumerable<IMenuItem> AddMenuItems()
            {
                yield return new MenuItemSeparator();
                yield return new MenuItem("Add Factory Presets", AddFactoryPreset);
            }
        }

        public void Show(Vector2 leftBottom, AnimationCurve currentCurve)
        {
            _swatchSetMenuAndTileView.SetValue(currentCurve);
            
            var st = _root.style;
            st.left = leftBottom.x;
            st.bottom = leftBottom.y;
            st.position = Position.Absolute;
            
            style.display = DisplayStyle.Flex;
        }
        
        public void Hide()
        {
            style.display = DisplayStyle.None;
        }

        private void AddFactoryPreset()
        {
            PersistentService.AddFactoryPreset();
            _swatchSetMenuAndTileView.ResetView();
        }
    }
}