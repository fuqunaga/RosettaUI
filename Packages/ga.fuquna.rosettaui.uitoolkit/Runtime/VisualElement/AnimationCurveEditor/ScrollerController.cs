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
        
        public ScrollerController(VisualElement parent, Action<float> onUpdateVerticalPosition, Action<float> onUpdateHorizontalPosition)
        {
            _verticalScroller = parent.Q<Scroller>("vertical-scroller");
            _horizontalScroller = parent.Q<Scroller>("horizontal-scroller");
            _verticalDragger = _verticalScroller.Q("unity-dragger");
            _horizontalDragger = _horizontalScroller.Q("unity-dragger");
            _onUpdateVerticalPosition = onUpdateVerticalPosition;
            _onUpdateHorizontalPosition = onUpdateHorizontalPosition;
            _verticalScroller.valueChanged += OnVerticalScrollerValueChanged;
            _horizontalScroller.valueChanged += OnHorizontalScrollerValueChanged;
        }

        public void UpdateScroller(in Rect viewRectInGraph, in Rect curveRect)
        {
            _horizontalScroller.lowValue = Mathf.Min(curveRect.xMin + viewRectInGraph.width * 0.5f, viewRectInGraph.center.x);
            _horizontalScroller.highValue = Mathf.Max(curveRect.xMax - viewRectInGraph.width * 0.5f, viewRectInGraph.center.x);
            _horizontalScroller.value = viewRectInGraph.center.x;
            _verticalScroller.lowValue = Mathf.Max(curveRect.yMax - viewRectInGraph.height * 0.5f, viewRectInGraph.center.y);
            _verticalScroller.highValue = Mathf.Min(curveRect.yMin + viewRectInGraph.height * 0.5f, viewRectInGraph.center.y);
            _verticalScroller.value = viewRectInGraph.center.y;
            
            _horizontalDragger.style.width = new StyleLength(Length.Percent(Mathf.Clamp01(viewRectInGraph.width / curveRect.width) * 100f));
            _verticalDragger.style.height = new StyleLength(Length.Percent(Mathf.Clamp01(viewRectInGraph.height / curveRect.height) * 100f));
        }

        private void OnVerticalScrollerValueChanged(float value)
        {
            _onUpdateVerticalPosition?.Invoke(value);
        }
        
        private void OnHorizontalScrollerValueChanged(float value)
        {
            _onUpdateHorizontalPosition?.Invoke(value);
        }
    }
}