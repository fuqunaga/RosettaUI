using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class ScrollerController
    {
        private readonly Scroller _verticalScroller;
        private readonly Scroller _horizontalScroller;
        private readonly VisualElement _verticalDragger;
        private readonly VisualElement _horizontalDragger;
        private readonly Action<float> _onUpdateVerticalPosition;
        private readonly Action<float> _onUpdateHorizontalPosition;
        
        private const string VerticalScrollerName = "vertical-scroller";
        private const string HorizontalScrollerName = "horizontal-scroller";
        private const string DraggerName = "unity-dragger";
        
        public ScrollerController(VisualElement parent, Action<float> onUpdateVerticalPosition, Action<float> onUpdateHorizontalPosition)
        {
            _verticalScroller = parent.Q<Scroller>(VerticalScrollerName);
            _horizontalScroller = parent.Q<Scroller>(HorizontalScrollerName);
            _verticalDragger = _verticalScroller.Q(DraggerName);
            _horizontalDragger = _horizontalScroller.Q(DraggerName);
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