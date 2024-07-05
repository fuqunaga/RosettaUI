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

        private readonly ColorPickerSwatch _currentSwatch;
        private readonly Clickable _currentSwatchClickableManipulator;
        
        public ColorPickerSwatchSet()
        {
            AddToClassList(GridUssClassName);
            
            text = "Swatches";
            
            _currentSwatchClickableManipulator = new Clickable(OnCurrentSwatchClicked);
            
            _currentSwatch = new ColorPickerSwatch { IsCurrent = true };
            _currentSwatch.AddManipulator(_currentSwatchClickableManipulator);
            
            Add(_currentSwatch);
            
            LoadSwatches();
        }
        
        public void SetColor(Color color) => _currentSwatch.Color = color;

        private void OnCurrentSwatchClicked()
        {
            AddSwatch(_currentSwatch.Color);
            SaveSwatches();
        }

        private void AddSwatch(Color color)
        {
            var swatch = new ColorPickerSwatch { Color = color };
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