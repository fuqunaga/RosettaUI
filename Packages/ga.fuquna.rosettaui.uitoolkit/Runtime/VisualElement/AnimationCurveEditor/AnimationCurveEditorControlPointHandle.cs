using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveEditorControlPointHandle : VisualElement
    {
        private VisualElement _lineElement;
        private VisualElement _handleElement;
        
        public AnimationCurveEditorControlPointHandle(float angle)
        {
            AddToClassList("rosettaui-animation-curve-editor__control-point-handle");
            InitUI();
            SetAngle(angle);
        }

        private void InitUI()
        {
            _lineElement = new VisualElement();
            _lineElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__line");
            _lineElement.style.position = Position.Absolute;
            _lineElement.style.left = 5;
            _lineElement.style.top = 4;
            _lineElement.style.backgroundColor = new StyleColor(Color.white);
            _lineElement.style.borderTopWidth = 0.5f;
            _lineElement.style.borderBottomWidth = 0.5f;
            _lineElement.style.borderLeftWidth = 0.5f;
            _lineElement.style.borderRightWidth = 0.5f;
            _lineElement.style.borderTopColor = new StyleColor(Color.black);
            _lineElement.style.borderBottomColor = new StyleColor(Color.black);
            _lineElement.style.borderLeftColor = new StyleColor(Color.black);
            _lineElement.style.borderRightColor = new StyleColor(Color.black);
            _lineElement.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(0, Length.Percent(50)));
            _lineElement.style.opacity = 0.5f;
            _lineElement.style.width = 50;
            _lineElement.style.height = 2;
            // _lineElement.style.zIndex = 1;
            Add(_lineElement);
            
            float handleSize = 8;
            _handleElement = new VisualElement();
            _handleElement.AddToClassList("rosettaui-animation-curve-editor__control-point-handle__handle");
            _handleElement.style.position = Position.Absolute;
            _handleElement.style.right = -handleSize * 0.5f;
            _handleElement.style.top = -handleSize * 0.5f;
            _handleElement.style.width = handleSize;
            _handleElement.style.height = handleSize;
            _handleElement.style.borderTopLeftRadius = handleSize * 0.5f;
            _handleElement.style.borderTopRightRadius = handleSize * 0.5f;
            _handleElement.style.borderBottomLeftRadius = handleSize * 0.5f;
            _handleElement.style.borderBottomRightRadius = handleSize * 0.5f;
            _handleElement.style.backgroundColor = new StyleColor(Color.gray);
            _lineElement.Add(_handleElement);
            
        }
        
        public void SetAngle(float angle)
        {
            _lineElement.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(-angle)));
        }
    }
}