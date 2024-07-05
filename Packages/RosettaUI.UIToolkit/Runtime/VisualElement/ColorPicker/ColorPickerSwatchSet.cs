using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPickerSwatchSet : Foldout
    {
        public const string UssClassName = "rosettaui-colorpicker-swatchset";
        public const string GridUssClassName = UssClassName + "-grid";
        public const string ListUssClassName = UssClassName + "-list";
        
        public readonly string colorPickerSwatchesKey = "ColorPickerSwatches";
        
        private Action<Color> _setColorPickerColor; 
        private readonly ColorPickerSwatch _currentSwatch;

        public ColorPickerSwatchSet(Action<Color> setColorPickerColor)
        {
            _setColorPickerColor = setColorPickerColor;
            
            AddToClassList(GridUssClassName);
            
            text = "Swatches";
            
            _currentSwatch = new ColorPickerSwatch { IsCurrent = true };
            _currentSwatch.RegisterCallback<ClickEvent>(OnCurrentSwatchClicked);
            
            Add(_currentSwatch);
            
            LoadSwatches();
        }
        
        public void SetColor(Color color) => _currentSwatch.Color = color;

        
        private void OnCurrentSwatchClicked(ClickEvent evt)
        {
            AddSwatch(_currentSwatch.Color);
            SaveSwatches();
            
            evt.StopPropagation();
        }
        
        private void OnSwatchClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not ColorPickerSwatch swatch) return;

            _setColorPickerColor(swatch.Color);
            
            evt.StopPropagation();
        }

        private void AddSwatch(Color color)
        {
            var swatch = new ColorPickerSwatch { Color = color };
            swatch.RegisterCallback<ClickEvent>(OnSwatchClicked);
            Insert(childCount - 1, swatch);
        }

        private void SaveSwatches()
        {
            var swatches = Children().Select(v => v as ColorPickerSwatch).Where(s => s != _currentSwatch);

            using var _ = ListPool<Color>.Get(out var colors);
            colors.AddRange(swatches.Select(s => s.Color));
            
            PersistantData.Set(colorPickerSwatchesKey, colors);
        }
        
        private void LoadSwatches()
        {
            if (!PersistantData.TryGet<List<Color>>(colorPickerSwatchesKey, out var colors))
            {
                return;
            }
            
            foreach (var color in colors)
            {
                AddSwatch(color);
            }
        }
    }
}