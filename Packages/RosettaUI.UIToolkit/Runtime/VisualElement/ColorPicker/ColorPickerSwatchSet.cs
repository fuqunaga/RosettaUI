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
        [Serializable]
        public struct NameAndColor
        {
            public string name;
            public Color color;
        }
        
        public const string UssClassName = "rosettaui-colorpicker-swatchset";
        public const string GridUssClassName = UssClassName + "-grid";
        public const string ListUssClassName = UssClassName + "-list";

        public const string ColorPickerSwatchesKey = "ColorPickerSwatches";

        private readonly Action<Color> _setColorPickerColor; 
        private readonly ColorPickerSwatch _currentSwatch;

        public ColorPickerSwatchSet(Action<Color> setColorPickerColor)
        {
            _setColorPickerColor = setColorPickerColor;
            
            AddToClassList(GridUssClassName);
            
            text = "Swatches";
            
            _currentSwatch = new ColorPickerSwatch { IsCurrent = true };
            _currentSwatch.RegisterCallback<PointerDownEvent>(OnCurrentSwatchPointerDown);
            
            Add(_currentSwatch);
            
            LoadSwatches();
        }
        
        public void SetColor(Color color) => _currentSwatch.Color = color;

        
        private void OnCurrentSwatchPointerDown(PointerDownEvent evt)
        {
            AddSwatch(_currentSwatch.Color);
            SaveSwatches();
            
            evt.StopPropagation();
        }
        
        private void OnSwatchPointerDown(PointerDownEvent evt)
        {
            if (evt.currentTarget is not ColorPickerSwatch swatch) return;

            switch (evt.button)
            {
                case 0:
                    _setColorPickerColor(swatch.Color);
                    evt.StopPropagation();
                    break;
                case 1:
                    PopupMenu.Show(new[]
                        {
                            new MenuItem("Replace", () => ReplaceSwatchColorToCurrent(swatch)),
                            new MenuItem("Delete", () => DeleteSwatch(swatch)),
                            new MenuItem("Move To First", () => MoveToFirstSwatch(swatch)),
                        }, 
                        evt.position, 
                        swatch);
                
                    evt.StopPropagationAndFocusControllerIgnoreEvent();
                    break;
            }
        }

        private void AddSwatch(Color color)
        {
            var swatch = new ColorPickerSwatch { Color = color };
            swatch.RegisterCallback<PointerDownEvent>(OnSwatchPointerDown);
            Insert(childCount - 1, swatch);
        }

        private void DeleteSwatch(ColorPickerSwatch swatch)
        {
            Remove(swatch);
            SaveSwatches();
        }

        private void ReplaceSwatchColorToCurrent(ColorPickerSwatch swatch)
        {
            swatch.Color = _currentSwatch.Color;
            SaveSwatches();
        }

        private void MoveToFirstSwatch(ColorPickerSwatch swatch)
        {
            Remove(swatch);
            Insert(0, swatch);
            SaveSwatches();
        }
        

        private void SaveSwatches()
        {
            var swatches = Children().Select(v => v as ColorPickerSwatch).Where(s => s != _currentSwatch);

            using var _ = ListPool<NameAndColor>.Get(out var nameAndColors);
            nameAndColors.AddRange(swatches.Select(s => new NameAndColor { name = null, color = s.Color }));
            
            PersistantData.Set(ColorPickerSwatchesKey, nameAndColors);
        }
        
        private void LoadSwatches()
        {
            if (PersistantData.TryGet<List<NameAndColor>>(ColorPickerSwatchesKey, out var nameAndColors) 
                && nameAndColors != null)
            {
                foreach (var nameAndColor in nameAndColors)
                {
                    AddSwatch(nameAndColor.color);
                }
            }
        }
    }
}