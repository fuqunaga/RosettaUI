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
        
        public AxisLabelController(VisualElement parent)
        {
            _verticalAxisLabelContainer = parent.Q("vertical-axis-label-container");
            _horizontalAxisLabelContainer = parent.Q("horizontal-axis-label-container");
            for (int i = 0; i < 10; i++)
            {
                var verticalLabel = new Label();
                _verticalAxisLabels.Add(verticalLabel);
                _verticalAxisLabelContainer.Add(verticalLabel);
                
                var horizontalLabel = new Label();
                _horizontalAxisLabels.Add(horizontalLabel);
                _horizontalAxisLabelContainer.Add(horizontalLabel);
            }
        }
        
        public void UpdateAxisLabel(in Rect viewRectInGraph)
        {
            float horizontalUnit = GetUnit(viewRectInGraph.width);
            float verticalUnit = GetUnit(viewRectInGraph.height);
            float horizontalStart = Mathf.Ceil(viewRectInGraph.min.x / horizontalUnit) * horizontalUnit;
            float verticalStart = Mathf.Ceil(viewRectInGraph.min.y / verticalUnit) * verticalUnit;
            string horizontalFormat = horizontalUnit < 1f ? $"F{Mathf.CeilToInt(-Mathf.Log10(horizontalUnit))}" : "F0";
            string verticalFormat = verticalUnit < 1f ? $"F{Mathf.CeilToInt(-Mathf.Log10(verticalUnit))}" : "F0";
            
            for (int i = 0; i < 10; i++)
            {
                float x = horizontalStart + horizontalUnit * i;
                float y = verticalStart + verticalUnit * i;
                _horizontalAxisLabels[i].text = x.ToString(horizontalFormat);
                _horizontalAxisLabels[i].style.right = new StyleLength(Length.Percent(100f - (x - viewRectInGraph.min.x) / viewRectInGraph.width * 100f));
                _verticalAxisLabels[i].text = y.ToString(verticalFormat);
                _verticalAxisLabels[i].style.bottom = new StyleLength(Length.Percent((y - viewRectInGraph.min.y) / viewRectInGraph.height * 100f));
            }
        }

        private static float GetUnit(float size)
        {
            int order = Mathf.CeilToInt(Mathf.Log10(size));
            float u1 = Mathf.Pow(10f, order - 1);
            float u2 = u1 * 0.5f;
            return size / u1 > 5f ? u1 : u2;
        }
    }
}