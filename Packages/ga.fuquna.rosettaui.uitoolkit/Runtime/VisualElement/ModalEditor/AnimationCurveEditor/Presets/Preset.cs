using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class Preset : SwatchBase<AnimationCurve>
    {
        public new const string UssClassName = "rosettaui-animation-curve-editor-preset";
        public new const string TileClassName = UssClassName + "__tile";
        public const string TileCoreClassName = TileClassName + "-core";
        public const string TileCurveClassName = TileClassName + "-curve";
        
        
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
            var tileElement = new VisualElement();
            tileElement.AddToClassList(TileClassName);
            
            var coreElement = new VisualElement();
            coreElement.AddToClassList(TileCoreClassName);
            
            _curveElement = new VisualElement();
            _curveElement.AddToClassList(TileCurveClassName);
            
            coreElement.Add(_curveElement);
            tileElement.Add(coreElement);
            
            SetTileElement(tileElement, "New");
            
            RegisterCallback<GeometryChangedEvent>(_ => UpdatePreviewImage());
        }

        private void UpdatePreviewImage()
        {
            AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(_curve, _curveElement, Color.white);
        }
    }
}