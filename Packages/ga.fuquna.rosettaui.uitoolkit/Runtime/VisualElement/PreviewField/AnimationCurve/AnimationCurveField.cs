using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveField : PreviewBaseFieldOfClass<AnimationCurve, AnimationCurveField.AnimationCurveInput>
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once ConvertToConstant.Global
        public new static readonly string ussClassName = "rosettaui-animation-curve-field";
        
        public AnimationCurveField() : this(null)
        {
        }

        
        public AnimationCurveField(string label) : base(label, new AnimationCurveInput())
        {
        }

        protected override AnimationCurve Clone(AnimationCurve curve) => AnimationCurveHelper.Clone(curve); 
        

        protected override void ShowEditor(Vector3 position)
        {
            AnimationCurveEditor.AnimationCurveEditor.Show(position, this, rawValue, curve => value = curve);
        }

        public override void SetValueWithoutNotify(AnimationCurve newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateAnimationCurveTexture();
        }

        
        private void UpdateAnimationCurveTexture()
        {
            var preview = inputField.preview;
            
            if (rawValue == null)
            {
                preview.style.backgroundImage = StyleKeyword.Undefined;
            }
            else
            {
                AnimationCurveVisualElementHelper.UpdateGradientPreviewToBackgroundImage(rawValue, preview);
            }
        }


        public class AnimationCurveInput : VisualElement
        {
            // ReSharper disable once InconsistentNaming
            public static readonly string ussFieldInput = ussClassName + "__input";
            
            public readonly VisualElement preview;

            public AnimationCurveInput()
            {
                AddToClassList(ussFieldInput);
                
                preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };

                Add(preview);
            }
        }
    }
}