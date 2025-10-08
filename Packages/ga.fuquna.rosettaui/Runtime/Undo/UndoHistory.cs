using System.Collections.Generic;

namespace RosettaUI.UndoSystem
{
    public static class UndoHistory
    {
        private static readonly Stack<IUndoRecord> UndoStack = new();
        private static readonly Stack<IUndoRecord> RedoStack = new();

        private static bool _isProcessing;
        private static bool _canTryMargeNextRecord = true;
        
        public static IEnumerable<IUndoRecord> UndoRecords => UndoStack;
        public static IEnumerable<IUndoRecord> RedoRecords => RedoStack;
        
        public static bool CanUndo => UndoStack.Count > 0;
        public static bool CanRedo => RedoStack.Count > 0;

        // Undo/Redo処理中はAddできない
        public static bool CanAdd => !_isProcessing;
        
        /// <summary>
        /// UndoStackにrecordを追加する。
        /// </summary>
        public static bool Add(IUndoRecord record, bool disposeRecordIfNotStacked = true)
        {
            if (record == null) return false;
            if (_isProcessing)
            {
                if (disposeRecordIfNotStacked) record.Dispose();
                return false;
            }
            

            RemoveTopExpiredRecords(UndoStack);
            
            // マージ可能条件
            // - RedoStackが空
            // - _canTryMargeNextRecord==true
            //   - 同じElementでも一度フォーカスが外れたらマージしないようにしたいので外部から_canTryMargeNextRecordで制御できるようにする
            if (RedoStack.Count <= 0
                && _canTryMargeNextRecord
                && UndoStack.TryPeek(out var previousRecord)
                && previousRecord.CanMerge(record))
            {
                previousRecord.Merge(record);
                if (disposeRecordIfNotStacked) record.Dispose();
            }
            else
            {
                UndoStack.Push(record);
                _canTryMargeNextRecord = true;
            }

            ClearStack(RedoStack);
            return true;
        }
        
        public static void Clear()
        {
            ClearStack(UndoStack);
            ClearStack(RedoStack);
        }
        
        public static void FixLastUndoRecord()
        {
            _canTryMargeNextRecord = false;
        }

        public static bool Undo()
        {
            RemoveTopExpiredRecords(UndoStack);
            if (!UndoStack.TryPop(out var record)) return false;
            
            _isProcessing = true;
            record.Undo();
            _isProcessing = false;
            
            RedoStack.Push(record);

            return true;
        }
        
        public static bool Redo()
        {
            RemoveTopExpiredRecords(RedoStack);
            if (!RedoStack.TryPop(out var record)) return false;
            
            _isProcessing = true;
            record.Redo();
            _isProcessing = false;

            UndoStack.Push(record);

            return true;
        }
        
        private static bool RemoveTopExpiredRecords(Stack<IUndoRecord> stack)
        {
            var removed = false;
            while (stack.TryPeek(out var record) && record.IsExpired)
            {
                stack.Pop().Dispose();
                removed = true;
            }

            return removed;
        }

        private static void ClearStack(Stack<IUndoRecord> stack)
        {
            while (stack.TryPop(out var record))
            {
                record.Dispose();
            }
        }
    }
}