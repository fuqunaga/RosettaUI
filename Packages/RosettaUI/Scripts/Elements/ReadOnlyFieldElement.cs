namespace RosettaUI
{
    /// <summary>
    /// 値を持ち外部と同期するFieldElement
    /// ラベル付きのReadOnlyValueElement
    /// </summary>
    public abstract class ReadOnlyFieldElement<T> : ReadOnlyValueElement<T>
    {
        public LabelElement label { get; protected set; }

        public ReadOnlyFieldElement(LabelElement label, IGetter<T> getter) : base(getter)
        {
            if (label != null)
            {
                this.label = label;
                label.Parent = this;
            }
        }

        protected override void UpdateInternal()
        {
            label?.Update();
            base.UpdateInternal();
        }
    }
}