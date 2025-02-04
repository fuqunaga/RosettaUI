using System;
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
        private Func<AnimationCurve> _curveGetter;
        // private Action<float> _onUpdateVerticalPosition;
        // private Action<float> _onUpdateHorizontalPosition;

        public ScrollerController(VisualElement parent, string verticalElementName, string horizontalElementName, 
            Func<AnimationCurve> curveGetter)
        {
            _verticalScroller = parent.Q<Scroller>(verticalElementName);
            _horizontalScroller = parent.Q<Scroller>(horizontalElementName);
            _verticalDragger = _verticalScroller.Q("unity-dragger");
            _horizontalDragger = _horizontalScroller.Q("unity-dragger");
            _curveGetter = curveGetter;
            // _onUpdateVerticalPosition = onUpdateVerticalPosition;
            // _onUpdateHorizontalPosition = onUpdateHorizontalPosition;
            // _verticalScroller.valueChanged += OnVerticalScrollerValueChanged;
            // _horizontalScroller.valueChanged += OnHorizontalScrollerValueChanged;
        }
        
        public void UpdateScroller(Rect viewRectInGraph)
        {
            var curve = _curveGetter();
            var curveRange = curve.ComputeCurveRange();
            _verticalScroller.lowValue = curveRange.yRange.x;
            _verticalScroller.highValue = curveRange.yRange.y;
            _verticalScroller.value = viewRectInGraph.center.y;
            _horizontalScroller.lowValue = curveRange.xRange.x;
            _horizontalScroller.highValue = curveRange.xRange.y;
            _horizontalScroller.value = viewRectInGraph.center.x;
            
            // adjust dragger size
            _horizontalDragger.style.width = new StyleLength(Length.Percent(Mathf.Clamp01(viewRectInGraph.width / (curveRange.xRange.y - curveRange.xRange.x)) * 100f));
            _verticalDragger.style.height = new StyleLength(Length.Percent(Mathf.Clamp01(viewRectInGraph.height / (curveRange.yRange.y - curveRange.yRange.x)) * 100f));
        }
    }
}