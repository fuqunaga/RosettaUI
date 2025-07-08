using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientEditorPreset : SwatchBase<Gradient>
    {
        public new const string UssClassName = "rosettaui-gradient-editor-preset";
        public new const string TileClassName = UssClassName + "__tile";
        public const string TileCoreClassName = TileClassName + "-core";
        public const string TileCheckerboardClassName = TileClassName + "-checkerboard";
        public const string TileGradientClassName = TileClassName + "-gradient";

        private readonly VisualElement _gradientElement;
        
        private readonly Gradient _gradient = new();
        
        
        public override Gradient Value 
        {
            get => GradientHelper.Clone(_gradient);
            set
            {
                GradientHelper.Copy(value, _gradient);
                GradientVisualElementHelper.UpdateGradientPreviewToBackgroundImage(_gradient, _gradientElement);
            } 
        }

        public GradientEditorPreset()
        {
            var tileElement = new VisualElement();
            tileElement.AddToClassList(TileClassName);

            var coreElement = new VisualElement();
            coreElement.AddToClassList(TileCoreClassName);
            
            var checkerboardElement = new Checkerboard(CheckerboardTheme.Dark);
            checkerboardElement.AddToClassList(TileCheckerboardClassName);
            
            _gradientElement = new VisualElement();
            _gradientElement.AddToClassList(TileGradientClassName);
            
            checkerboardElement.Add(_gradientElement);
            coreElement.Add(checkerboardElement);
            
            tileElement.Add(coreElement);
            
            SetTileElement(tileElement);
        }
    }
}