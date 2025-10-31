using RosettaUI.Builder;
using RosettaUI.UIToolkit.UndoSystem;
using RosettaUI.UndoSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// AnimationCurve field
    /// </summary>
    /// <details>
    /// SetValueWithoutNotify()で値の比較をGetHashCode()で行おうとしたが、次のケースで同じハッシュ値が返る
    /// 
    ///  var curveA = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 2f));
    ///  var curveB = new AnimationCurve(new Keyframe(0f, 0f, 2f, 2f), new Keyframe(1f, 1f, 0f, 0f));
    ///
    /// また、WrapModeがGetHashCode()では考慮されない
    /// 結局AnimationCurve同士を比較する必要がある→最後にSetされた値をコピーして保持しておく必要がある
    /// </details>
    public class AnimationCurveField : ImagePreviewFieldBase<AnimationCurve, AnimationCurveField.AnimationCurveInput>
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public new const string ussClassName = "rosettaui-animation-curve-field";
     
        public AnimationCurveField() : this(null)
        {
        }
        
        public AnimationCurveField(string label) : base(label, new AnimationCurveInput())
        {
        }

        protected override void ShowEditor(Vector3 position)
        {
            var initialValue = UndoHelper.Clone(value);
            AnimationCurveEditor.AnimationCurveEditor.Show(position, this, value,
                onCurveChanged: curve => value = curve,
                onHide: _ =>
                {
                    if (!initialValue.Equals(value))
                    {
                        UndoUIToolkit.RecordBaseField(nameof(AnimationCurveField), this, initialValue, value);
                    }
                }
            );
        }
        
        protected override void Copy(AnimationCurve source, AnimationCurve destination)
            => AnimationCurveHelper.Copy(source, destination);

        protected override void UpdatePreviewToBackgroundImage(AnimationCurve currentValue, VisualElement element)
            => AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(currentValue, element);
        

        public class AnimationCurveInput : VisualElement, IPreviewElement
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once MemberCanBePrivate.Global
            public const string ussFieldInput = ussClassName + "__input";

            public VisualElement Preview { get;  }

            public AnimationCurveInput()
            {
                AddToClassList(ussFieldInput);
                
                Preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };

                Add(Preview);
            }
        }
    }
}