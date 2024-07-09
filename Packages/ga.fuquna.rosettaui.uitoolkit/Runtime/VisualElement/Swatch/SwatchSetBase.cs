using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public abstract class SwatchSetBase<TValue, TSwatch> : Foldout
        where TSwatch : SwatchBase<TValue>, new()
    {
        public enum TileLayout
        {
            Grid,
            List
        }
        
        [Serializable]
        public struct NameAndValue
        {
            public string name;
            public TValue value;
        }
        
        public const string UssClassName = "rosettaui-swatchset";
        public const string TileScrollViewUssClassName = UssClassName + "__tile-scroll-view";
        
        private readonly VisualElement _swatchSetMenu;
        private readonly ScrollView _tileScrollView;
        private readonly SwatchBase<TValue> _currentSwatch;
        private readonly Action<TValue> _applyValueFunc; 

        private TileLayout _tileLayout;
        
        protected abstract string DataKeyLayout { get; }
        protected abstract string DataKeyIsOpen { get; }
        protected abstract string DataKeySwatches { get; }
        
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
                
                PersistentData.Set(DataKeyLayout, (int)value);
            }
        }

        
        public SwatchSetBase(string label, Action<TValue> applyValueFunc)
        {
            _applyValueFunc = applyValueFunc;
            text = label;
            
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
            
            
            _currentSwatch = new TSwatch { IsCurrent = true };
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
            var isOpen = PersistentData.Get<bool>(DataKeyIsOpen);
            value = isOpen;
            
            Layout = (TileLayout)PersistentData.Get<int>(DataKeyLayout);
        }
   
        
        public void SetValue(TValue currentValue) => _currentSwatch.Value = currentValue;

        private void SetMenuVisible(bool v)
        {
            _swatchSetMenu.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        } 
        
        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            var isOpen = evt.newValue;
            SetMenuVisible(isOpen);
            
            PersistentData.Set(DataKeyIsOpen, isOpen);
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
            if (evt.currentTarget is not TSwatch swatch) return;

            switch (evt.button)
            {
                case 0:
                    _applyValueFunc(swatch.Value);
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
                yield return new MenuItem("Replace", () => ReplaceSwatchValueToCurrent(swatch));
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
        

        private SwatchBase<TValue> AddSwatch(TValue swatchValue, string swatchName = null)
        {
            var swatch = new TSwatch
            {
                Label = swatchName,
                Value = swatchValue,
            };
            swatch.RegisterCallback<PointerDownEvent>(OnSwatchPointerDown);
            swatch.EnableText = IsSwatchDisplayLabel;
            _tileScrollView.Insert(_tileScrollView.childCount - 1, swatch);

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
            using var _ = ListPool<NameAndValue>.Get(out var nameAndValues);
            nameAndValues.AddRange(Swatches.Select(s => new NameAndValue { name = s.Label, value = s.Value }));
            
            PersistentData.Set(DataKeySwatches, nameAndValues);
        }
        
        private void LoadSwatches()
        {
            var nameAndValues = PersistentData.Get<List<NameAndValue>>(DataKeySwatches);
            if (nameAndValues == null) return;
            
            foreach (var nameAndValue in nameAndValues)
            {
                AddSwatch(nameAndValue.value, nameAndValue.name);
            }
        }
    }
}
