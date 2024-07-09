#if false
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class SwatchSetBase<TValue> : Foldout
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
        public const string TileScrollViewUssClassName = UssClassName + "__tile-scroll-view";

        public const string ColorPickerSwatchSetLayoutKey = "ColorPickerSwatchSetLayout";
        public const string ColorPickerSwatchSetIsOpenKey = "ColorPickerSwatchSetIsOpen";
        public const string ColorPickerSwatchesKey = "ColorPickerSwatches";

        private readonly VisualElement _swatchSetMenu;
        private readonly ScrollView _tileScrollView;
        private readonly SwatchBase<TValue> _currentSwatch;
        private readonly Action<TValue> _applyValueFunc; 

        private TileLayout _tileLayout;
        
        private bool IsSwatchDisplayLabel => Layout == TileLayout.List;
        private IEnumerable<SwatchBase<TValue>> SwatchesWithCurrent => _tileScrollView.Children().Select(v => v as SwatchBase<TValue>);
        private IEnumerable<SwatchBase<TValue>> Swatches => SwatchesWithCurrent.Where(s => s != _currentSwatch);
        
        public TileLayout Layout
        {
            get => _tileLayout;
            
            set
            {
                _tileLayout = value;
                _tileScrollView.mode = _tileLayout == TileLayout.Grid ? ScrollViewMode.Horizontal : ScrollViewMode.Vertical;
                
                UpdateSwatchesEnableText();
                
                PersistentData.Set(ColorPickerSwatchSetLayoutKey, (int)value);
            }
        }

        
        public SwatchSetBase(string label, Action<TValue> applyValueFunc)
        {
            _applyValueFunc = applyValueFunc;
            text = "label";
            
            AddToClassList(UssClassName);

            _swatchSetMenu = CreateSwatchSetMenu();
            var toggle = this.Q<Toggle>();
            toggle.Add(_swatchSetMenu);

            value = false;
            this.RegisterValueChangedCallback(OnValueChanged);


            _tileScrollView = new ScrollView()
            {
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            
            _tileScrollView.AddToClassList(TileScrollViewUssClassName);
            
            // TODO:
            // _currentSwatch = new ColorPickerSwatch { IsCurrent = true };
            _currentSwatch.RegisterCallback<PointerDownEvent>(OnCurrentSwatchPointerDown);
            
            _tileScrollView.Add(_currentSwatch);
            Add(_tileScrollView);

            
            LoadSwatches();
            
            // すぐにLoadStatusしてSwatchSetが
            // ・Foldがオープン
            // ・TileLayoutがList
            // ・Swatchの名前が長い
            // 場合、自動レイアウトでColorPickerのWindow自体が広がってしまうので
            // しばらくはデフォルトの閉じてる状態をキープしてからLoadStatusする
            schedule.Execute(LoadStatus).ExecuteLater(32);
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
        
        
        private void LoadStatus()
        {
            var isOpen = PersistentData.Get<bool>(ColorPickerSwatchSetIsOpenKey);
            value = isOpen;
            
            Layout = (TileLayout)PersistentData.Get<int>(ColorPickerSwatchSetLayoutKey);
        }
   

        //TODO:
        // public void SetColor( color) => _currentSwatch.Color = color;

        private void SetMenuVisible(bool v)
        {
            _swatchSetMenu.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        } 
        
        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            var isOpen = evt.newValue;
            SetMenuVisible(isOpen);
            
            PersistentData.Set(ColorPickerSwatchSetIsOpenKey, isOpen);
        }


    
        private void OnCurrentSwatchPointerDown(PointerDownEvent evt)
        {
            var newSwatch = AddSwatch(_currentSwatch.Value);
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
                    // TODO:
                    // _applyValueFunc(swatch.Color);
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
        

        private SwatchBase<TValue> AddSwatch(TValue color, string swatchName = null)
        {
            var swatch = new ColorPickerSwatch
            {
                Label = swatchName,
                Color = color,
            };
            swatch.RegisterCallback<PointerDownEvent>(OnSwatchPointerDown);
            swatch.EnableText = IsSwatchDisplayLabel;
            _tileScrollView.Insert(_tileScrollView.childCount - 1, swatch);

            return swatch;
        }

        private void DeleteSwatch(ColorPickerSwatch swatch)
        {
            _tileScrollView.Remove(swatch);
            SaveSwatches();
        }

        private void ReplaceSwatchColorToCurrent(ColorPickerSwatch swatch)
        {
            swatch.Color = _currentSwatch.Color;
            SaveSwatches();
        }

        private void MoveToFirstSwatch(ColorPickerSwatch swatch)
        {
            _tileScrollView.Remove(swatch);
            _tileScrollView.Insert(0, swatch);
            SaveSwatches();
        }
        
        private void SaveSwatches()
        {
            using var _ = ListPool<NameAndColor>.Get(out var nameAndColors);
            nameAndColors.AddRange(Swatches.Select(s => new NameAndColor { name = s.Label, color = s.Color }));
            
            PersistentData.Set(ColorPickerSwatchesKey, nameAndColors);
        }
        
        private void LoadSwatches()
        {
            var nameAndColors = PersistentData.Get<List<NameAndColor>>(ColorPickerSwatchesKey);
            if (nameAndColors == null) return;
            
            foreach (var nameAndColor in nameAndColors)
            {
                AddSwatch(nameAndColor.color, nameAndColor.name);
            }
        }
    }
}

#endif