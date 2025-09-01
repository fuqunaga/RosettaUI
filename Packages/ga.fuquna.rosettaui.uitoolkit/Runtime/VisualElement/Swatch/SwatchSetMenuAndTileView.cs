using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Swatch;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{  
    public enum TileLayout
    {
        Grid,
        List
    }

    public static class SwatchSet
    {
        public const string UssClassName = "rosettaui-swatchset";
        public const string TileContainerClassName = UssClassName + "__tile-container";
        public const string TileContainerGridClassName = TileContainerClassName + "--grid";
        public const string TileContainerListClassName = TileContainerClassName + "--list";
    }
    
    public class SwatchSetMenuAndTileView<TValue, TSwatch>
        where TSwatch : SwatchBase<TValue>, new()
    {
        private const string PersistantKeyLayout = "Layout";
        
        private readonly VisualElement _menuButton;
        private readonly ScrollView _tileScrollView;
        private readonly SwatchBase<TValue> _currentSwatch;
        private readonly Action<TValue> _applyValueFunc; 
        private readonly Func<IEnumerable<IMenuItem>> _createAdditionalMenuItems;
        
        private TileLayout _tileLayout;
        
        public SwatchPersistentService<TValue> PersistentService { get; }
        
        private bool IsSwatchDisplayLabel => Layout == TileLayout.List;
        private IEnumerable<SwatchBase<TValue>> SwatchesWithCurrent => _tileScrollView.Children().Select(v => v as SwatchBase<TValue>);
        private IEnumerable<SwatchBase<TValue>> Swatches => SwatchesWithCurrent.Where(s => s != _currentSwatch);

        
        public TileLayout Layout
        {
            get => _tileLayout;
            
            set
            {
                _tileLayout = value;
                _tileScrollView.mode = value == TileLayout.Grid ? ScrollViewMode.Horizontal : ScrollViewMode.Vertical;
                _tileScrollView.contentContainer.EnableInClassList(SwatchSet.TileContainerGridClassName, value == TileLayout.Grid);
                _tileScrollView.contentContainer.EnableInClassList(SwatchSet.TileContainerListClassName, value == TileLayout.List);
                
                UpdateSwatchesEnableText();
                
                PersistentService.Set(PersistantKeyLayout, (int)value);
            }
        }

        public SwatchSetMenuAndTileView(VisualElement menuParent, VisualElement tileScrollViewParent,
            Action<TValue> applyValueFunc, string dataKeyPrefix,
            Func<IEnumerable<IMenuItem>> createAdditionalMenuItems = null) : this(menuParent, tileScrollViewParent, applyValueFunc, new SwatchPersistentService<TValue>(dataKeyPrefix), createAdditionalMenuItems)
        {
        }

        public SwatchSetMenuAndTileView(VisualElement menuParent, VisualElement tileScrollViewParent,
            Action<TValue> applyValueFunc, SwatchPersistentService<TValue> persistentService,
            Func<IEnumerable<IMenuItem>> createAdditionalMenuItems = null)
        {
            _applyValueFunc = applyValueFunc;
            _createAdditionalMenuItems = createAdditionalMenuItems;
            PersistentService = persistentService;
            
            _menuButton = CreateMenuButton();
            SetMenuButtonVisible(false);

            menuParent.Add(_menuButton);
            
            _tileScrollView = new ScrollView()
            {
                mode = ScrollViewMode.Vertical,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            
            _tileScrollView.contentContainer.AddToClassList(SwatchSet.TileContainerClassName);
            
            
            _currentSwatch = new TSwatch { IsCurrent = true };
            _currentSwatch.RegisterCallback<PointerDownEvent>(OnCurrentSwatchPointerDown);
            
            _tileScrollView.Add(_currentSwatch);
            tileScrollViewParent.Add(_tileScrollView);
            
            LoadSwatches();
            LoadStatus();
        }
        
        
        public void LoadStatus()
        {
            Layout = (TileLayout)PersistentService.Get<int>(PersistantKeyLayout);
        }
   
        
        public void SetValue(TValue currentValue) => _currentSwatch.Value = currentValue;

        public void SetMenuButtonVisible(bool v)
        {
            _menuButton.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ResetView()
        {
            _tileScrollView.Clear();
            LoadSwatches();
        }


        private VisualElement CreateMenuButton()
        {
            return new MoreVertMenuButton()
            {
                ButtonIndex = 0,
                CreateMenuItems = CreateMenuItems
            };

            IEnumerable<IMenuItem> CreateMenuItems()
            {
                var menuItems = new[]
                {
                    CreateLayoutMenuItem(TileLayout.Grid),
                    CreateLayoutMenuItem(TileLayout.List)
                };
                
                return menuItems.Concat(_createAdditionalMenuItems?.Invoke() ?? Array.Empty<IMenuItem>());
            }
            
            MenuItem CreateLayoutMenuItem(TileLayout tileLayout)
            {
                return new MenuItem(tileLayout.ToString(), () => Layout = tileLayout)
                {
                    isChecked = Layout == tileLayout
                };
            }
        }
        
        private void OnCurrentSwatchPointerDown(PointerDownEvent evt)
        {
            var newSwatch = AddSwatch(_currentSwatch.Value);
            SaveSwatches();
            
            if (IsSwatchDisplayLabel)
            {
                newSwatch.StartRename(SaveSwatches);
            }

            evt.StopPropagation();
        }
        
        private void OnSwatchPointerDown(PointerDownEvent evt)
        {
            if (evt.currentTarget is not TSwatch swatch) return;

            switch (evt.button)
            {
                case 0:
                    _applyValueFunc(swatch.Value);
                    evt.StopPropagation();
                    break;
                case 1:
                    PopupMenuUtility.Show(
                        CreateMenuItems(),
                        evt.position, 
                        swatch);
                
                    evt.StopPropagationAndFocusControllerIgnoreEvent();
                    break;
            }

            return;

            IEnumerable<IMenuItem> CreateMenuItems()
            {
                yield return new MenuItem("Replace", () => ReplaceSwatchValueToCurrent(swatch));
                yield return new MenuItem("Delete", () => DeleteSwatch(swatch));
                if (Layout == TileLayout.List)
                {
                    yield return new MenuItem("Rename", () => swatch.StartRename(SaveSwatches));
                }
                
                // "Move To First"が途中までしか表示されないのでスペースとダミー文字で文字数を増やす @Unity6000.0.2f1
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
        

        private TSwatch AddSwatch(TValue swatchValue, string swatchName = null)
        {
            var swatch = new TSwatch()
            {
                Label = swatchName,
                Value = swatchValue
            };
            
            swatch.RegisterCallback<PointerDownEvent>(OnSwatchPointerDown);
            swatch.EnableText = IsSwatchDisplayLabel;
            
            // 最後はCurrentSwatchなので、その一つ前に追加
            var prevLastIndex = Mathf.Max(0, _tileScrollView.childCount - 1);
            _tileScrollView.Insert(prevLastIndex, swatch);

            return swatch;
        }

        private void DeleteSwatch(TSwatch swatch)
        {
            _tileScrollView.Remove(swatch);
            SaveSwatches();
        }

        private void ReplaceSwatchValueToCurrent(TSwatch swatch)
        {
            swatch.Value = _currentSwatch.Value;
            SaveSwatches();
        }

        private void MoveToFirstSwatch(TSwatch swatch)
        {
            _tileScrollView.Remove(swatch);
            _tileScrollView.Insert(0, swatch);
            SaveSwatches();
        }
        
        private void SaveSwatches()
        {
            PersistentService.SaveSwatches(Swatches.Select(s => new NameAndValue<TValue>
            {
                name = s.Label,
                value = s.Value
            }));
        }
        
        private void LoadSwatches()
        {
            var nameAndValues = PersistentService.LoadSwatches();
            if (nameAndValues == null)
            {
                return;
            }
            
            foreach (var nameAndValue in nameAndValues)
            {
                AddSwatch(nameAndValue.value, nameAndValue.name);
            }
        }
    }
}
