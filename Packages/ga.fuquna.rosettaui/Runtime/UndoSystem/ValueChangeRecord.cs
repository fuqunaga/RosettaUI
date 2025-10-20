namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// 値の変更を記録するUndoRecord向けのユーティリティクラス
    /// </summary>
    public class ValueChangeRecord<TValue>
    {
        public TValue BeforeRaw { get; set; }
        public TValue AfterRaw { get; set; }
        
        public TValue Before => UndoHelper.Clone(BeforeRaw);
        public TValue After => UndoHelper.Clone(AfterRaw);
        
        public void Initialize(TValue before, TValue after)
        {
            BeforeRaw = UndoHelper.Clone(before);
            AfterRaw = UndoHelper.Clone(after);
        }

        public void Clear()
        {
            BeforeRaw = default;
            AfterRaw = default;
        }
        
        public override string ToString() => $"{BeforeRaw} -> {AfterRaw}";
    }
}