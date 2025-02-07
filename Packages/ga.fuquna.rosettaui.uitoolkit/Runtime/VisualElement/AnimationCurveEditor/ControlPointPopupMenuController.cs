using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ControlPointPopupMenuController
    {
        private Action _onPointRemoved;
        private Action<PointMode> _onPointModeChanged;
        private Action<TangentMode?, TangentMode?> _onTangentModeChanged;

        private Dictionary<PointMode, MenuItem> _pointModeMenuItems = new Dictionary<PointMode, MenuItem>();
        private Dictionary<TangentMode, MenuItem> _inTangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();
        private Dictionary<TangentMode, MenuItem> _outTangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();
        private Dictionary<TangentMode, MenuItem> _bothTangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();


        public ControlPointPopupMenuController(Action onPointRemoved, Action<PointMode> onPointModeChanged, Action<TangentMode?, TangentMode?> onTangentModeChanged)
        {
            _onPointRemoved = onPointRemoved;
            _onPointModeChanged = onPointModeChanged;
            _onTangentModeChanged = onTangentModeChanged;

            _pointModeMenuItems[PointMode.Smooth] = new MenuItem("Smooth", () => _onPointModeChanged(PointMode.Smooth));
            _pointModeMenuItems[PointMode.Flat] = new MenuItem("Flat", () => _onPointModeChanged(PointMode.Flat));
            _pointModeMenuItems[PointMode.Broken] = new MenuItem("Broken", () => _onPointModeChanged(PointMode.Broken));
            
            _inTangentModeMenuItems[TangentMode.Free] = new MenuItem("Free", () => _onTangentModeChanged(TangentMode.Free, null));
            _inTangentModeMenuItems[TangentMode.Linear] = new MenuItem("Linear", () => _onTangentModeChanged(TangentMode.Linear, null));
            _inTangentModeMenuItems[TangentMode.Constant] = new MenuItem("Constant", () => _onTangentModeChanged(TangentMode.Constant, null));
            _inTangentModeMenuItems[TangentMode.Weighted] = new MenuItem("Weighted", () => _onTangentModeChanged(TangentMode.Weighted, null));
            
            _outTangentModeMenuItems[TangentMode.Free] = new MenuItem("Free", () => _onTangentModeChanged(null, TangentMode.Free));
            _outTangentModeMenuItems[TangentMode.Linear] = new MenuItem("Linear", () => _onTangentModeChanged(null, TangentMode.Linear));
            _outTangentModeMenuItems[TangentMode.Constant] = new MenuItem("Constant", () => _onTangentModeChanged(null, TangentMode.Constant));
            _outTangentModeMenuItems[TangentMode.Weighted] = new MenuItem("Weighted", () => _onTangentModeChanged(null, TangentMode.Weighted));
            
            _bothTangentModeMenuItems[TangentMode.Free] = new MenuItem("Free", () => _onTangentModeChanged(TangentMode.Free, TangentMode.Free));
            _bothTangentModeMenuItems[TangentMode.Linear] = new MenuItem("Linear", () => _onTangentModeChanged(TangentMode.Linear, TangentMode.Linear));
            _bothTangentModeMenuItems[TangentMode.Constant] = new MenuItem("Constant", () => _onTangentModeChanged(TangentMode.Constant, TangentMode.Constant));
            _bothTangentModeMenuItems[TangentMode.Weighted] = new MenuItem("Weighted", () => _onTangentModeChanged(TangentMode.Weighted, TangentMode.Weighted));
        }

        public void Show(Vector2 position, VisualElement targetElement)
        {
            PopupMenuUtility.Show(
                new[]
                {
                    new MenuItem("Delete Key", () => _onPointRemoved()),
                    MenuItem.Separator,
                    _pointModeMenuItems[PointMode.Smooth],
                    _pointModeMenuItems[PointMode.Flat],
                    _pointModeMenuItems[PointMode.Broken],
                    MenuItem.Separator,
                    new MenuItem("Left Tangent", () => PopupMenuUtility.Show(
                        new[]
                        {
                            new MenuItem("Left Tangent", null) { isEnable = false },
                            MenuItem.Separator,
                            _inTangentModeMenuItems[TangentMode.Free],
                            _inTangentModeMenuItems[TangentMode.Linear],
                            _inTangentModeMenuItems[TangentMode.Constant],
                            _inTangentModeMenuItems[TangentMode.Weighted],
                            MenuItem.Separator,
                            new MenuItem("Back", () => Show(position, targetElement))
                        },
                        position,
                        targetElement)
                    ),
                    new MenuItem("Right Tangent", () => PopupMenuUtility.Show(
                        new[]
                        {
                            new MenuItem("Right Tangent", null) { isEnable = false },
                            MenuItem.Separator,
                            _outTangentModeMenuItems[TangentMode.Free],
                            _outTangentModeMenuItems[TangentMode.Linear],
                            _outTangentModeMenuItems[TangentMode.Constant],
                            _outTangentModeMenuItems[TangentMode.Weighted],
                            MenuItem.Separator,
                            new MenuItem("Back", () => Show(position, targetElement))
                        },
                        position,
                        targetElement)
                    ),
                    new MenuItem("Both Tangent", () => PopupMenuUtility.Show(
                        new[]
                        {
                            new MenuItem("Both Tangent", null) { isEnable = false },
                            MenuItem.Separator,
                            _bothTangentModeMenuItems[TangentMode.Free],
                            _bothTangentModeMenuItems[TangentMode.Linear],
                            _bothTangentModeMenuItems[TangentMode.Constant],
                            _bothTangentModeMenuItems[TangentMode.Weighted],
                            MenuItem.Separator,
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
    }
}