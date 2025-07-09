using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientField : PreviewBaseField<Gradient, GradientField.GradientInput>
    {
        private Gradient _lastAppliedGradient;
        
        public GradientField() : this(null)
        {
        }
        
        public GradientField(string label) : base(label, new GradientInput())
        {
        }

        protected override void ShowEditor(Vector3 position)
        {
            GradientEditor.Show(position, this, rawValue, gradient => value = gradient);
        }

        public override void SetValueWithoutNotify(Gradient newValue)
        {
            if (GradientHelper.EqualsByValue(_lastAppliedGradient, newValue))
            {
                return;
            }
            
            base.SetValueWithoutNotify(newValue);
            UpdateGradientTexture();

            _lastAppliedGradient ??= new Gradient();
            GradientHelper.Copy(newValue, _lastAppliedGradient);
        }

        private void UpdateGradientTexture()
        {
            var preview = inputField.preview;

            if (value == null)
            {
                preview.style.backgroundImage = StyleKeyword.Undefined;
            }
            else
            {
                GradientVisualElementHelper.UpdateGradientPreviewToBackgroundImage(value, preview);
            }
        }

        
        public class GradientInput : VisualElement
        {
            public readonly VisualElement checkerBoard;
            public readonly VisualElement preview;
            
            public GradientInput()
            {
                checkerBoard = new Checkerboard(CheckerboardTheme.Dark);

                preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };
                
                checkerBoard.Add(preview);
                Add(checkerBoard);
            }
        }
    }
}