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
        public enum TileLayout
        {
            Grid,
            List
        }
        
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

        private readonly VisualElement _swatchSetMenu;

        private readonly Action<Color> _setColorPickerColor; 
        private readonly ColorPickerSwatch _currentSwatch;

        
        private bool IsSwatchDisplayLabel => Layout == TileLayout.List;
        
        private IEnumerable<ColorPickerSwatch> SwatchesWithCurrent => Children().Select(v => v as ColorPickerSwatch);
        private IEnumerable<ColorPickerSwatch> Swatches => SwatchesWithCurrent.Where(s => s != _currentSwatch);
        

        public TileLayout Layout
        {
            get => ClassListContains(GridUssClassName) ? TileLayout.Grid : TileLayout.List;
            
            set
            {
                var isGrid = value == TileLayout.Grid;
                EnableInClassList(GridUssClassName, isGrid);
                EnableInClassList(ListUssClassName, !isGrid);
                
                UpdateSwatchesEnableText();
            }
        }

        public ColorPickerSwatchSet(Action<Color> setColorPickerColor)
        {
            _setColorPickerColor = setColorPickerColor;
            text = "Swatches";
            
            AddToClassList(UssClassName);
            AddToClassList(ListUssClassName);

            _swatchSetMenu = CreateSwatchSetMenu();
            var toggle = this.Q<Toggle>();
            toggle.Add(_swatchSetMenu);

            value = false;
            SetMenuVisible(false);
            
            this.RegisterValueChangedCallback(evt => SetMenuVisible(evt.newValue));
            
            _currentSwatch = new ColorPickerSwatch { IsCurrent = true };
            _currentSwatch.RegisterCallback<PointerDownEvent>(OnCurrentSwatchPointerDown);
            
            Add(_currentSwatch);
            
            LoadSwatches();
            
            UpdateSwatchesEnableText();
        }

        private VisualElement CreateSwatchSetMenu()
        {
            return new MoreVertMenuButton()
            {
                ButtonIndex = 0,
                CreateMenuItems = () => new[]
                {
                    CreateTileLayoutItem(TileLayout.Grid),
                    CreateTileLayoutItem(TileLayout.List)
                }
            };
            
            MenuItem CreateTileLayoutItem(TileLayout tileLayout)
            {
                return new MenuItem(tileLayout.ToString(), () => Layout = tileLayout)
                {
                    isChecked = Layout == tileLayout
                };
            }
        }
   
        public void SetColor(Color color) => _currentSwatch.Color = color;

        private void SetMenuVisible(bool v) => _swatchSetMenu.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        
    
        private void OnCurrentSwatchPointerDown(PointerDownEvent evt)
        {
            var newSwatch = AddSwatch(_currentSwatch.Color);
            SaveSwatches();
            
            if ( IsSwatchDisplayLabel )
            {
                newSwatch.StartRename(SaveSwatches);
            }
            
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
                    PopupMenu.Show(
                        CreateMenuItems(),
                        evt.position, 
                        swatch);
                
                    evt.StopPropagationAndFocusControllerIgnoreEvent();
                    break;
            }

            return;

            IEnumerable<MenuItem> CreateMenuItems()
            {
                yield return new MenuItem("Replace", () => ReplaceSwatchColorToCurrent(swatch));
                yield return new MenuItem("Delete", () => DeleteSwatch(swatch));
                if (Layout == TileLayout.List)
                {
                    yield return new MenuItem("Rename", () => swatch.StartRename(SaveSwatches));
                }
                yield return new MenuItem("Move To First", () => MoveToFirstSwatch(swatch));
            }
        }
        
        
        private void UpdateSwatchesEnableText()
        {
            var isSwatchLabelVisible = IsSwatchDisplayLabel;
            
            foreach (var swatch in SwatchesWithCurrent)
            {
                swatch.EnableText = isSwatchLabelVisible;
            }
        }
        

        private ColorPickerSwatch AddSwatch(Color color, string swatchName = null)
        {
            var swatch = new ColorPickerSwatch
            {
                Label = swatchName,
                Color = color,
            };
            swatch.RegisterCallback<PointerDownEvent>(OnSwatchPointerDown);
            swatch.EnableText = IsSwatchDisplayLabel;
            Insert(childCount - 1, swatch);

            return swatch;
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
            using var _ = ListPool<NameAndColor>.Get(out var nameAndColors);
            nameAndColors.AddRange(Swatches.Select(s => new NameAndColor { name = s.Label, color = s.Color }));
            
            PersistantData.Set(ColorPickerSwatchesKey, nameAndColors);
        }
        
        private void LoadSwatches()
        {
            if (PersistantData.TryGet<List<NameAndColor>>(ColorPickerSwatchesKey, out var nameAndColors) 
                && nameAndColors != null)
            {
                foreach (var nameAndColor in nameAndColors)
                {
                    AddSwatch(nameAndColor.color, nameAndColor.name);
                }
            }
        }
    }
}