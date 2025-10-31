using System;

namespace RosettaUI.UndoSystem
{
    public interface IObjectRestoreRecord
    {
        object RestoreObject();
    }
    
    
    public interface IObjectRestoreRecord<out TObject> : IObjectRestoreRecord
    {
        TObject Restore();
        object IObjectRestoreRecord.RestoreObject() => Restore();
    }

    
    public class ObjectRestoreRecord<TObject> : IObjectRestoreRecord<TObject>
    {
        private readonly Func<TObject> _restoreFunc;

        public ObjectRestoreRecord(Func<TObject> restoreFunc)
        {
            _restoreFunc = restoreFunc;
        }
        
        public TObject Restore() => _restoreFunc != null
            ? _restoreFunc()
            : default;
    }
}