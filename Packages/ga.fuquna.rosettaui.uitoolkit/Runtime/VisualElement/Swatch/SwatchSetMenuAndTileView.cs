using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Swatch;
using RosettaUI.UndoSystem;
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
            
            tileScrollViewParent.Add(_tileScrollView);

            LoadStatus();
            LoadSwatches();
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
                newSwatch.StartRename(() =>
                {
                    SaveSwatches();
                    RecordUndoAdd(newSwatch);
                });
            }
            else
            {
                RecordUndoAdd(newSwatch);
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
                yield return new MenuItem("Replace", () => ReplaceSwatchValueToCurrentUndoable(swatch));
                yield return new MenuItem("Delete", () => DeleteSwatchUndoable(swatch));
                if (Layout == TileLayout.List)
                {
                    yield return new MenuItem("Rename", () =>
                    {
                        var oldName = swatch.Label;
                        swatch.StartRename(() =>
                        {
                            SaveSwatches();
                            RecordUndoNameChange(swatch, oldName);
                        });
                    });
                }
                
                // "Move To First"が途中までしか表示されないのでスペースとダミー文字で文字数を増やす @Unity6000.0.2f1
                yield return new MenuItem("Move To First", () => MoveToFirstSwatchUndoable(swatch));
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
            var index = Mathf.Max(0, _tileScrollView.childCount - 1);
            return InsertSwatch(index, swatchValue, swatchName);
        }
        
        private TSwatch InsertSwatch(int index, TValue swatchValue, string swatchName = null)
        {
            var swatch = new TSwatch()
            {
                Label = swatchName,
                Value = swatchValue
            };
            
            swatch.RegisterCallback<PointerDownEvent>(OnSwatchPointerDown);
            swatch.EnableText = IsSwatchDisplayLabel;
            
            _tileScrollView.Insert(index, swatch);

            return swatch;
        }

        private void DeleteSwatch(TSwatch swatch)
        {
            _tileScrollView.Remove(swatch);
            SaveSwatches();
        }

        private void DeleteSwatchUndoable(TSwatch swatch)
        {
            RecordUndoDelete(swatch);
            DeleteSwatch(swatch);
        }
        

        private void ReplaceSwatchValueToCurrentUndoable(TSwatch swatch)
        {
            var oldValue = swatch.Value;
            swatch.Value = _currentSwatch.Value;
            SaveSwatches();

            RecordUndoValueChange(swatch, oldValue);
        }

        private void MoveToFirstSwatchUndoable(TSwatch swatch)
        {
            var oldIndex = _tileScrollView.IndexOf(swatch);
            MoveSwatch(oldIndex, 0);
            
            RecordUndoMoveToFirst(oldIndex);
        }
        
        private void MoveSwatch(int oldIndex, int newIndex)
        {
            var swatch = _tileScrollView.ElementAt(oldIndex);
            _tileScrollView.Remove(swatch);
            _tileScrollView.Insert(newIndex, swatch);
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
        
        public void LoadSwatches()
        {
            _tileScrollView.Clear();
            // AddSwatch()は_tileScrollViewにすでに_currentScrollViewが追加されていることを前提としているので
            // Clear()直後に追加しておく
            _tileScrollView.Add(_currentSwatch);
            
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
        
        
        #region Undo

        private void RecordUndoValueChange(TSwatch swatch, TValue before)
        {
            var index = _tileScrollView.IndexOf(swatch);
            var after = swatch.Value;
            
            Undo.RecordValueChange("Swatch Value Change", before, after, v =>
            {
                if (_tileScrollView.ElementAt(index) is not TSwatch s) return;
                s.Value = v;
                SaveSwatches();
            });
        }
        
        private void RecordUndoNameChange(TSwatch swatch, string before)
        {
            var index = _tileScrollView.IndexOf(swatch);
            var after = swatch.Label;
            
            Undo.RecordValueChange("Swatch Name Change", before, after, v =>
            {
                if (_tileScrollView.ElementAt(index) is not TSwatch s) return;
                s.Label = v;
                SaveSwatches();
            });
        }
        
        private void RecordUndoAdd(TSwatch swatch)
        {
            Undo.RecordAction("Swatch Add", (UndoHelper.Clone(swatch.Value), swatch.Label), 
                undoAction: _ => DeleteSwatch(GetTargetSwatch()),
                redoAction: data => 
                {
                    var (value, name) = data;
                    AddSwatch(UndoHelper.Clone(value), name);
                }
            );
            return;

            TSwatch GetTargetSwatch()
            {
                // 最後はCurrentSwatchなので、追加されたものは一つ前のはず
                var prevLastIndex = Mathf.Max(0, _tileScrollView.childCount - 2);
                return _tileScrollView.ElementAt(prevLastIndex) as TSwatch;
            }
        }
        
        private void RecordUndoDelete(TSwatch swatch)
        {
            var index = _tileScrollView.IndexOf(swatch);
            
            Undo.RecordAction("Swatch Delete", (index, UndoHelper.Clone(swatch.Value), swatch.Label), 
                undoAction: data => 
                {
                    var (i, value, name) = data;
                    InsertSwatch(i, UndoHelper.Clone(value), name);
                },
                redoAction: data =>
                {
                    var (i, _, _) = data;
                    var s = _tileScrollView.ElementAt(i) as TSwatch;
                    DeleteSwatch(s);
                });
        }
        
        private void RecordUndoMoveToFirst(int oldIndex)
        {
            Undo.RecordAction("Swatch Move To First", oldIndex,
                undoAction: i => MoveSwatch(0, i),
                redoAction: i => MoveSwatch(i, 0)
            );
        }
        
        #endregion
    }
}
