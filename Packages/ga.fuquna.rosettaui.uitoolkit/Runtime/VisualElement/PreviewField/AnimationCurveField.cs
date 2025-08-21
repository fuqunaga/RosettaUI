using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveField : PreviewBaseField<AnimationCurve, AnimationCurveField.AnimationCurveInput>
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public new const string ussClassName = "rosettaui-animation-curve-field";
     
        private int _lastAppliedHashCode = 0;
        
        public AnimationCurveField() : this(null)
        {
        }

        
        public AnimationCurveField(string label) : base(label, new AnimationCurveInput())
        {
            RegisterCallback<GeometryChangedEvent>(_ => UpdateAnimationCurveTexture());
        }

        protected override void ShowEditor(Vector3 position)
        {
            AnimationCurveEditor.AnimationCurveEditor.Show(position, this, value, curve => value = curve);
        }

        public override void SetValueWithoutNotify(AnimationCurve newValue)
        {
            var newHashCode = newValue?.GetHashCode() ?? 0;
            if (_lastAppliedHashCode == newHashCode)
            {
                return;
            }
            
            base.SetValueWithoutNotify(newValue);
            UpdateAnimationCurveTexture();
            
            _lastAppliedHashCode = newHashCode;
        }

        
        private void UpdateAnimationCurveTexture()
        {
            var preview = inputField.preview;
            
            if (value == null)
            {
                preview.style.backgroundImage = StyleKeyword.Undefined;
            }
            else
            {
                AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(value, preview);
            }
        }


        public class AnimationCurveInput : VisualElement
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once MemberCanBePrivate.Global
            public const string ussFieldInput = ussClassName + "__input";

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