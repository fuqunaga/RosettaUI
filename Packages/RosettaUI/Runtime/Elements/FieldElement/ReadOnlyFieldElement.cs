namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するFieldElement
    /// ラベル付きのReadOnlyValueElement
    /// </summary>
    public abstract class ReadOnlyFieldElement<T> : ReadOnlyValueElement<T>, IFieldElement
    {
        public LabelElement Label { get; protected set; }

        public ReadOnlyFieldElement(LabelElement label, IGetter<T> getter) : base(getter)
        {
            if (label != null)
            {
                Label = label;
                label.SetParent(this);
            }
        }

        protected override void UpdateInternal()
        {
            Label?.Update();
            base.UpdateInternal();
        }
    }
}