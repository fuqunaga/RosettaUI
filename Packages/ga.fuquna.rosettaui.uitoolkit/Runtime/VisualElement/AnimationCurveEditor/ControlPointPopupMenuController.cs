using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ControlPointPopupMenuController
    {
        private readonly Action _onPointRemoved;
        private readonly Action<PointMode> _onPointModeChanged;
        private readonly Action<TangentMode?, TangentMode?> _onTangentModeChanged;
        private readonly Action<WeightedMode> _onWeightedModeChanged;

        private readonly Dictionary<PointMode, MenuItem> _pointModeMenuItems = new Dictionary<PointMode, MenuItem>();
        private readonly Dictionary<TangentMode, MenuItem> _inTangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();
        private readonly Dictionary<TangentMode, MenuItem> _outTangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();
        private readonly Dictionary<TangentMode, MenuItem> _bothTangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();
        private readonly MenuItem _inWeightedModeMenuItem;
        private readonly MenuItem _outWeightedModeMenuItem;
        private readonly MenuItem _bothWeightedModeMenuItem;


        public ControlPointPopupMenuController(Action onPointRemoved, Action<PointMode> onPointModeChanged, Action<TangentMode?, TangentMode?> onTangentModeChanged, Action<WeightedMode> onWeightedModeChanged)
        {
            _onPointRemoved = onPointRemoved;
            _onPointModeChanged = onPointModeChanged;
            _onTangentModeChanged = onTangentModeChanged;
            _onWeightedModeChanged = onWeightedModeChanged;

            _pointModeMenuItems[PointMode.Smooth] = new MenuItem("Smooth", () => _onPointModeChanged(PointMode.Smooth));
            _pointModeMenuItems[PointMode.Flat] = new MenuItem("Flat", () => _onPointModeChanged(PointMode.Flat));
            _pointModeMenuItems[PointMode.Broken] = new MenuItem("Broken", () => _onPointModeChanged(PointMode.Broken));
            
            _inTangentModeMenuItems[TangentMode.Free] = new MenuItem("Free", () => _onTangentModeChanged(TangentMode.Free, null));
            _inTangentModeMenuItems[TangentMode.Linear] = new MenuItem("Linear", () => _onTangentModeChanged(TangentMode.Linear, null));
            _inTangentModeMenuItems[TangentMode.Constant] = new MenuItem("Constant", () => _onTangentModeChanged(TangentMode.Constant, null));
            _inWeightedModeMenuItem = new MenuItem("Weighted", () => _onWeightedModeChanged(WeightedMode.In));
            
            _outTangentModeMenuItems[TangentMode.Free] = new MenuItem("Free", () => _onTangentModeChanged(null, TangentMode.Free));
            _outTangentModeMenuItems[TangentMode.Linear] = new MenuItem("Linear", () => _onTangentModeChanged(null, TangentMode.Linear));
            _outTangentModeMenuItems[TangentMode.Constant] = new MenuItem("Constant", () => _onTangentModeChanged(null, TangentMode.Constant));
            _outWeightedModeMenuItem = new MenuItem("Weighted", () => _onWeightedModeChanged(WeightedMode.Out));
            
            _bothTangentModeMenuItems[TangentMode.Free] = new MenuItem("Free", () => _onTangentModeChanged(TangentMode.Free, TangentMode.Free));
            _bothTangentModeMenuItems[TangentMode.Linear] = new MenuItem("Linear", () => _onTangentModeChanged(TangentMode.Linear, TangentMode.Linear));
            _bothTangentModeMenuItems[TangentMode.Constant] = new MenuItem("Constant", () => _onTangentModeChanged(TangentMode.Constant, TangentMode.Constant));
            _bothWeightedModeMenuItem = new MenuItem("Weighted", () => _onWeightedModeChanged(WeightedMode.Both));
        }

        public void Show(Vector2 position, VisualElement targetElement)
        {
            PopupMenuUtility.Show(
                new IMenuItem[]
                {
                    new MenuItem("Delete Key", () => _onPointRemoved()),
                    new MenuItemSeparator(),
                    _pointModeMenuItems[PointMode.Smooth],
                    _pointModeMenuItems[PointMode.Flat],
                    _pointModeMenuItems[PointMode.Broken],
                    new MenuItemSeparator(),
                    new MenuItem("Left Tangent", () => PopupMenuUtility.Show(
                        new IMenuItem[]
                        {
                            new MenuItem("Left Tangent", null) { isEnable = false },
                            new MenuItemSeparator(),
                            _inTangentModeMenuItems[TangentMode.Free],
                            _inTangentModeMenuItems[TangentMode.Linear],
                            _inTangentModeMenuItems[TangentMode.Constant],
                            new MenuItemSeparator(),
                            _inWeightedModeMenuItem,
                            new MenuItemSeparator(),
                            new MenuItem("Back", () => Show(position, targetElement))
                        },
                        position,
                        targetElement)
                    ),
                    new MenuItem("Right Tangent", () => PopupMenuUtility.Show(
                        new IMenuItem[]
                        {
                            new MenuItem("Right Tangent", null) { isEnable = false },
                            new MenuItemSeparator(),
                            _outTangentModeMenuItems[TangentMode.Free],
                            _outTangentModeMenuItems[TangentMode.Linear],
                            _outTangentModeMenuItems[TangentMode.Constant],
                            new MenuItemSeparator(),
                            _outWeightedModeMenuItem,
                            new MenuItemSeparator(),
                            new MenuItem("Back", () => Show(position, targetElement))
                        },
                        position,
                        targetElement)
                    ),
                    new MenuItem("Both Tangent", () => PopupMenuUtility.Show(
                        new IMenuItem[]
                        {
                            new MenuItem("Both Tangent", null) { isEnable = false },
                            new MenuItemSeparator(),
                            _bothTangentModeMenuItems[TangentMode.Free],
                            _bothTangentModeMenuItems[TangentMode.Linear],
                            _bothTangentModeMenuItems[TangentMode.Constant],
                            new MenuItemSeparator(),
                            _bothWeightedModeMenuItem,
                            new MenuItemSeparator(),
                            new MenuItem("Back", () => Show(position, targetElement))
                        },
                        position,
                        targetElement)
                    )
                },
                position,
                targetElement
            );
        }
        
        public void SetPointMode(PointMode pointMode)
        {
            foreach (var item in _pointModeMenuItems.Values)
            {
                item.isChecked = false;
            }

            _pointModeMenuItems[pointMode].isChecked = true;
        }
        
        public void SetTangentMode(TangentMode inTangentMode, TangentMode outTangentMode)
        {
            foreach (var item in _inTangentModeMenuItems.Values)
            {
                item.isChecked = false;
            }
            foreach (var item in _outTangentModeMenuItems.Values)
            {
                item.isChecked = false;
            }
            foreach (var item in _bothTangentModeMenuItems.Values)
            {
                item.isChecked = false;
            }

            if (inTangentMode == outTangentMode)
            {
                _bothTangentModeMenuItems[inTangentMode].isChecked = true;
            }

            _inTangentModeMenuItems[inTangentMode].isChecked = true;
            _outTangentModeMenuItems[outTangentMode].isChecked = true;
        }
        
        public void SetWeightedMode(WeightedMode weightedMode)
        {
            _inWeightedModeMenuItem.isChecked = weightedMode is WeightedMode.In or WeightedMode.Both;
            _outWeightedModeMenuItem.isChecked = weightedMode is WeightedMode.Out or WeightedMode.Both;
            _bothWeightedModeMenuItem.isChecked = weightedMode == WeightedMode.Both;
        }
    }
}