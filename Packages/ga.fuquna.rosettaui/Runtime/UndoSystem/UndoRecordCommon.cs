using System;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// データを保存してUndo/Redoを行う汎用的なUndoRecord
    /// </summary>
    public class UndoRecordCommon<TData> : UndoRecordFlexBase, IUndoRecord
    {
        private TData _data;
        private Action<TData> _undoAction;
        private Action<TData> _redoAction;
        
        public string Name { get; private set; }
        
        public void Initialize(string name, TData data, Action<TData> undoAction, Action<TData> redoAction)
        {
            Name = name;
            _data = data;
            _undoAction = undoAction;
            _redoAction = redoAction;
        }
        
        public void Undo() => _undoAction?.Invoke(_data);

        public void Redo() => _redoAction?.Invoke(_data);
        
        public override void Dispose()
        {
            base.Dispose();
            if (_data is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _data = default;
            _undoAction = null;
            _redoAction = null;
        }
    }
    
    public static partial class Undo
    {
        public static void RecordCommon<TData>(string label, TData data, Action<TData> undoAction, Action<TData> redoAction)
        {
            using (UndoRecorder<UndoRecordCommon<TData>>.Get(out var record))
            {
                record.Initialize(label, data, undoAction, redoAction);
            }
        }
    }
}