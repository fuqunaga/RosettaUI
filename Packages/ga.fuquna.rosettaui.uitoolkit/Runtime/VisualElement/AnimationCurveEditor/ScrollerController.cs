using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ScrollerController
    {
        private Scroller _verticalScroller;
        private Scroller _horizontalScroller;
        private VisualElement _verticalDragger;
        private VisualElement _horizontalDragger;
        private Action<float> _onUpdateVerticalPosition;
        private Action<float> _onUpdateHorizontalPosition;
        
        private float _yValueMax;
        
        public ScrollerController(VisualElement parent, Action<float> onUpdateVerticalPosition, Action<float> onUpdateHorizontalPosition)
        {
            // Scrollers
            _verticalScroller = parent.Q<Scroller>("vertical-scroller");
            _horizontalScroller = parent.Q<Scroller>("horizontal-scroller");
            _verticalDragger = _verticalScroller.Q("unity-dragger");
            _horizontalDragger = _horizontalScroller.Q("unity-dragger");
            _onUpdateVerticalPosition = onUpdateVerticalPosition;
            _onUpdateHorizontalPosition = onUpdateHorizontalPosition;
            _verticalScroller.valueChanged += OnVerticalScrollerValueChanged;
            _horizontalScroller.valueChanged += OnHorizontalScrollerValueChanged;
        }

        public void UpdateScroller(in Rect viewRectInGraph, in (Vector2 xRange, Vector2 yRange) curveRange)
        {
            _yValueMax = curveRange.yRange.y;
            _verticalScroller.lowValue = curveRange.yRange.x + viewRectInGraph.height * 0.5f;
            _verticalScroller.highValue = curveRange.yRange.y - viewRectInGraph.height * 0.5f;
            _verticalScroller.value = _yValueMax - viewRectInGraph.center.y;
            _horizontalScroller.lowValue = curveRange.xRange.x + viewRectInGraph.width * 0.5f;
            _horizontalScroller.highValue = curveRange.xRange.y - viewRectInGraph.width * 0.5f;
            _horizontalScroller.value = viewRectInGraph.center.x;
            
            // adjust dragger size
            _horizontalDragger.style.width = new StyleLength(Length.Percent(Mathf.Clamp01(viewRectInGraph.width / (curveRange.xRange.y - curveRange.xRange.x)) * 100f));
            _verticalDragger.style.height = new StyleLength(Length.Percent(Mathf.Clamp01(viewRectInGraph.height / (curveRange.yRange.y - curveRange.yRange.x)) * 100f));
        }

        private void OnVerticalScrollerValueChanged(float value)
        {
            _onUpdateVerticalPosition?.Invoke(_yValueMax - value);
        }
        
        private void OnHorizontalScrollerValueChanged(float value)
        {
            _onUpdateHorizontalPosition?.Invoke(value);
        }

        
    }
}