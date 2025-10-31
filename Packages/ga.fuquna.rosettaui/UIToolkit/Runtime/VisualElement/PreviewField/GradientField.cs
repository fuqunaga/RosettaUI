using RosettaUI.Builder;
using RosettaUI.UIToolkit.UndoSystem;
using RosettaUI.UndoSystem;
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
            var initialValue = UndoHelper.Clone(rawValue);
            GradientEditor.Show(position, this, rawValue,
                onGradientChanged: gradient => value = gradient,
                onHide: _ =>
                {
                    if (!GradientHelper.EqualsWithoutAlloc(initialValue, rawValue))
                    {
                        UndoUIToolkit.RecordBaseField(nameof(GradientField), this, initialValue, rawValue);
                    }
                }
            );
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