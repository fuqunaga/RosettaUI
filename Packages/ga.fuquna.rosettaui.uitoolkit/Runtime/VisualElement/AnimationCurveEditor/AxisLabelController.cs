using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class AxisLabelController
    {
        private VisualElement _verticalAxisLabelContainer;
        private VisualElement _horizontalAxisLabelContainer;
        private List<Label> _verticalAxisLabels = new List<Label>();
        private List<Label> _horizontalAxisLabels = new List<Label>();
        
        private const string VerticalAxisLabelContainerName = "vertical-axis-label-container";  
        private const string HorizontalAxisLabelContainerName = "horizontal-axis-label-container";
        
        public AxisLabelController(VisualElement parent)
        {
            _verticalAxisLabelContainer = parent.Q(VerticalAxisLabelContainerName);
            _horizontalAxisLabelContainer = parent.Q(HorizontalAxisLabelContainerName);
            for (int i = 0; i < 10; i++)
            {
                var verticalLabel = new Label { pickingMode = PickingMode.Ignore };
                _verticalAxisLabels.Add(verticalLabel);
                _verticalAxisLabelContainer.Add(verticalLabel);
                
                var horizontalLabel = new Label { pickingMode = PickingMode.Ignore };
                _horizontalAxisLabels.Add(horizontalLabel);
                _horizontalAxisLabelContainer.Add(horizontalLabel);
            }
        }
        
        public void UpdateAxisLabel(in Rect viewRectInGraph)
        {
            var gridViewport = new GridViewport(viewRectInGraph.width, viewRectInGraph.height);
            float xTick = gridViewport.XTick;
            float yTick = gridViewport.YTick;
            float xUnit = viewRectInGraph.width / xTick > 5f ? xTick : xTick * 0.5f;
            float yUnit = viewRectInGraph.height / yTick > 5f ? yTick : yTick * 0.5f;
                
            float horizontalStart = Mathf.Ceil(viewRectInGraph.min.x / xUnit) * xUnit;
            float verticalStart = Mathf.Ceil(viewRectInGraph.min.y / yUnit) * yUnit;
            string horizontalFormat = xUnit < 1f ? $"F{Mathf.CeilToInt(-Mathf.Log10(xUnit))}" : "F0";
            string verticalFormat = yUnit < 1f ? $"F{Mathf.CeilToInt(-Mathf.Log10(yUnit))}" : "F0";
            
            for (int i = 0; i < 10; i++)
            {
                float x = horizontalStart + xUnit * i;
                float y = verticalStart + yUnit * i;
                _horizontalAxisLabels[i].text = x.ToString(horizontalFormat);
                _horizontalAxisLabels[i].style.right = new StyleLength(Length.Percent(100f - (x - viewRectInGraph.min.x) / viewRectInGraph.width * 100f));
                _verticalAxisLabels[i].text = y.ToString(verticalFormat);
                _verticalAxisLabels[i].style.bottom = new StyleLength(Length.Percent((y - viewRectInGraph.min.y) / viewRectInGraph.height * 100f));
            }
        }
    }
}