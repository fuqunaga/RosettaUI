using System;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// 汎用的な値の変更を記録するUndoRecord
    /// </summary>
    public class UndoRecordValueChange<TValue> : UndoRecordFlexBase, IUndoRecord
    {
        private readonly ValueChangeRecord<TValue> _valueChangeRecord = new();
        private string _label;
        private Action<TValue> _applyValue;
        
        
        public string Name => $"{_label} ({_valueChangeRecord})";
        
        
        public void Initialize(string label, TValue before, TValue after, Action<TValue> applyValue)
        {
            _label = label;
            _applyValue = applyValue;
            _valueChangeRecord.Initialize(before, after);
        }
        
        public void Undo() => _applyValue(_valueChangeRecord.Before);

        public void Redo() => _applyValue(_valueChangeRecord.After);

        protected override bool CanMargeDefault(IUndoRecord newer)
        {
            // デフォルトは同じラベルならマージ可能とする
            return (newer is UndoRecordValueChange<TValue> newRecord) && _label == newRecord._label;

        }
        
        protected override void MargeDefault(IUndoRecord newer)
        {
            if (newer is UndoRecordValueChange<TValue> newerRecord)
            {
                _valueChangeRecord.AfterRaw = newerRecord._valueChangeRecord.AfterRaw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _valueChangeRecord.Clear();
            _applyValue = null;
        }
    }
    
    
    public static partial class Undo
    {
        public static UndoRecordValueChange<TValue> RecordValueChange<TValue>(string label, TValue before, TValue after, Action<TValue> applyValue)
        {
            using var _ = UndoRecorder<UndoRecordValueChange<TValue>>.Get(out var record);

            record.Initialize(label, before, after, applyValue);
            return record;
        }
    }
}