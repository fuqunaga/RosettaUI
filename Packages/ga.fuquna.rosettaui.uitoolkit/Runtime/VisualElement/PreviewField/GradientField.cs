using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientField : ImagePreviewFieldBase<Gradient, GradientField.GradientInput>
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


        protected override bool Equals(Gradient a, Gradient b)
            => GradientHelper.EqualsWithoutAlloc(a, b);

        protected override void Copy(Gradient source, Gradient destination)
            => GradientHelper.Copy(source, destination);

        protected override void UpdatePreviewToBackgroundImage(Gradient currentValue, VisualElement element)
            => GradientVisualElementHelper.UpdatePreviewToBackgroundImage(currentValue, element);

        public class GradientInput : VisualElement, IPreviewElement
        {
            public readonly VisualElement checkerBoard;
            public VisualElement Preview { get;  }
            
            public GradientInput()
            {
                checkerBoard = new Checkerboard(CheckerboardTheme.Dark);

                Preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };
                
                checkerBoard.Add(Preview);
                Add(checkerBoard);
            }
        }
    }
}