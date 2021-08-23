using System;

namespace Comugi
{
    /// <summary>
    /// 値を持ち外部と同期するFieldElement
    /// ラベル付きのReadOnlyValueElement
    /// </summary>
    public abstract class ReadOnlyField<T> : ReadOnlyValueElement<T>
    {
        public Label label { get; protected set; }

        public ReadOnlyField(Label label, IGetter<T> getter) : base(getter)
        {
            this.label = label;
        }

        protected override void UpdateInternal()
        {
            label?.Update();
            base.UpdateInternal();
        }
    }
}