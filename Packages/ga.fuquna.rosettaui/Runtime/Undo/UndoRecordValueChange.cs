using System;

namespace RosettaUI.Undo
{
    /// <summary>
    /// 汎用的な値の変更を記録するUndoRecord
    /// </summary>
    public class UndoRecordValueChange<TValue> : IUndoRecord, IDisposable
    {
        private readonly ValueChangeRecord<TValue> _valueChangeRecord = new();

        private string _label;
        private Action<TValue> _applyValue;
        
        public Func<bool> IsAvailableFunc { get; set; }
        public Func<IUndoRecord, bool> CanMargeFunc { get; set; }
        public Action<IUndoRecord> MergeAction { get; set; }
        
        public string Name => $"{_label} ({_valueChangeRecord})";
        
        public bool IsAvailable => IsAvailableFunc?.Invoke() ?? true;
        
        
        public void Initialize(string label, TValue before, TValue after, Action<TValue> applyValue)
        {
            _label = label;
            _applyValue = applyValue;
            _valueChangeRecord.Initialize(before, after);
        }
        
        public void Undo() => _applyValue(_valueChangeRecord.Before);

        public void Redo() => _applyValue(_valueChangeRecord.After);

        public bool CanMerge(IUndoRecord newer)
        {
            if (CanMargeFunc != null)
            {
                return CanMargeFunc(newer);
            }

            // デフォルトは同じラベルならマージ可能とする
            return newer is UndoRecordValueChange<TValue> newRecord && _label == newRecord._label;
        }

        public void Merge(IUndoRecord newer)
        {
            if (MergeAction != null)
            {
                MergeAction(newer);
                return;
            }
            
            if (newer is UndoRecordValueChange<TValue> newerRecord)
            {
                _valueChangeRecord.AfterRaw = newerRecord._valueChangeRecord.AfterRaw;
            }
        }

        public void Dispose()
        {
            _valueChangeRecord.Clear();
            _label = null;
            _applyValue = null;
            
            IsAvailableFunc = null;
            CanMargeFunc = null;
            MergeAction = null;
        }
    }
}