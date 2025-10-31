using System;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// Undo/Redoを行う汎用的なUndoRecord
    /// </summary>
    public class UndoRecordAction: UndoRecordFlexBase, IUndoRecord
    {
        private Action _undoAction;
        private Action _redoAction;
        
        public string Name { get; private set; }
        
        public void Initialize(string name, Action undoAction, Action redoAction)
        {
            Name = name;
            _undoAction = undoAction;
            _redoAction = redoAction;
        }
        
        public void Undo() => _undoAction?.Invoke();

        public void Redo() => _redoAction?.Invoke();
        
        public override void Dispose()
        {
            base.Dispose();
            _undoAction = null;
            _redoAction = null;
        }
    }
    
    /// <summary>
    /// データを保存してUndo/Redoを行う汎用的なUndoRecord
    /// </summary>
    public class UndoRecordAction<TData> : UndoRecordFlexBase, IUndoRecord
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
        public static UndoRecordAction RecordAction(string label, Action undoAction, Action redoAction)
        {
            using var _ = UndoRecorder<UndoRecordAction>.Get(out var record);
            record.Initialize(label, undoAction, redoAction);
            return record;
        }
        
        public static UndoRecordAction<TData> RecordAction<TData>(string label, TData data, Action<TData> undoAction, Action<TData> redoAction)
        {
            using var _ = UndoRecorder<UndoRecordAction<TData>>.Get(out var record);
            record.Initialize(label, data, undoAction, redoAction);
            return record;
        }
    }
}