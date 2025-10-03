using System;
using System.Collections.Generic;

namespace RosettaUI.UndoSystem
{
    public static class UndoHistory
    {
        private static readonly Stack<IUndoRecord> UndoStack = new();
        private static readonly Stack<IUndoRecord> RedoStack = new();

        private static bool _isProcessing;
        
        public static event Action onRecordChanged; 
        
        public static IEnumerable<IUndoRecord> UndoRecords => UndoStack;
        public static IEnumerable<IUndoRecord> RedoRecords => RedoStack;
        
        
        public static bool CanUndo => UndoStack.Count > 0;
        public static bool CanRedo => RedoStack.Count > 0;
        
        
        public static void Add(IUndoRecord record)
        {
            if (_isProcessing) return;
            if (record == null) return;
            
            RemoveTopExpiredRecords(UndoStack);
            if (UndoStack.TryPeek(out var previousRecord)
                && previousRecord.CanMerge(record))
            {
                previousRecord.Merge(record);
            }
            else
            {
                UndoStack.Push(record);
            }

            ClearStack(RedoStack);
            
            NotifyRecordChanged();
        }

        public static bool Undo()
        {
            var changed = RemoveTopExpiredRecords(UndoStack);
            var hasRecord = UndoStack.TryPop(out var record);
            if (hasRecord)
            {
                _isProcessing = true;
                record.Undo();
                _isProcessing = false;
                RedoStack.Push(record);
                changed = true;
            }

            if (changed)
            {
                NotifyRecordChanged();
            }

            return hasRecord;
        }
        
        public static bool Redo()
        {
            var changed = RemoveTopExpiredRecords(RedoStack);
            var hasRecord = RedoStack.TryPop(out var record);
            if (hasRecord)
            {
                _isProcessing = true;
                record.Redo();
                _isProcessing = false;

                UndoStack.Push(record);
                changed = true;
            }

            if (changed)
            {
                NotifyRecordChanged();
            }

            return hasRecord;
        }
        
        private static void NotifyRecordChanged()
        {
            onRecordChanged?.Invoke();
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