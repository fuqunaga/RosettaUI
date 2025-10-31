using System;
using UnityEngine.Pool;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// UndoRecordをプーリングして管理するための構造体
    /// </summary>
    /// <code>
    /// using (UndoRecorder&lt;TUndoRecord&gt;.Get(out var record))
    /// {
    ///     // recordの初期化処理
    ///     record.Initialize(...);
    /// }
    /// // Dispose時にUndoHistoryにrecordが追加される
    /// </code>
    public readonly struct UndoRecorder<TUndoRecord> : IDisposable
        where TUndoRecord : class, IUndoRecord, new()
    {
        private static readonly ObjectPool<TUndoRecord> Pool = new(() => new TUndoRecord());
        
        public static UndoRecorder<TUndoRecord> Get(out TUndoRecord record)
        {
            record = Pool.Get();
            return new UndoRecorder<TUndoRecord>(record);
        }
        
        private static void OnDispose(IUndoRecord record)
        {
            if (record is TUndoRecord typedRecord)
            {
                Pool.Release(typedRecord);
            }
        }
        
        
        private readonly TUndoRecord _record;
        
        private UndoRecorder(TUndoRecord record)
        {
            _record = record;
        }

        public void Dispose()
        {
            UndoHistory.Add(_record, onDispose: OnDispose);
        }
    }
}