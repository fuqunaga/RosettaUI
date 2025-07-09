using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientField : PreviewBaseField<Gradient, GradientField.GradientInput>
    {
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
            base.SetValueWithoutNotify(newValue);
            UpdateGradientTexture();
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
            private static Texture2D _checkerBoardTexture;

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