using System.Linq;

namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するFieldElement
    /// ラベル付きのReadOnlyValueElement
    /// </summary>
    public abstract class ReadOnlyFieldElement<T> : ReadOnlyValueElement<T>
    {
        public LabelElement Label => Children.FirstOrDefault() as LabelElement;

        protected ReadOnlyFieldElement(LabelElement label, IGetter<T> getter) : base(getter)
        {
            if (label != null)
            {
                label.SetLabelTypeToPrefixIfAuto();
                AddChild(label);
            }
        }
    }
}