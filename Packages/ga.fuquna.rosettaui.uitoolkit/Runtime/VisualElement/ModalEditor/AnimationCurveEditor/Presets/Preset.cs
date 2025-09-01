using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class Preset : SwatchBase<AnimationCurve>
    {
        public new const string UssClassName = "rosettaui-animation-curve-editor-preset";
        public new const string TileClassName = UssClassName + "__tile";
        
        private readonly VisualElement _curveElement;
        private readonly AnimationCurve _curve = new();
        
        public override AnimationCurve Value 
        {
            get => AnimationCurveHelper.Clone(_curve);
            set
            {
                AnimationCurveHelper.Copy(value, _curve);
                UpdatePreviewImage();
            } 
        }

        public Preset()
        {
            _curveElement = new VisualElement();
            _curveElement.AddToClassList(TileClassName);
            
            SetTileElement(_curveElement);
            
            RegisterCallback<GeometryChangedEvent>(_ => UpdatePreviewImage());
        }

        private void UpdatePreviewImage()
        {
            AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(_curve, _curveElement, Color.white);
        }
    }
}