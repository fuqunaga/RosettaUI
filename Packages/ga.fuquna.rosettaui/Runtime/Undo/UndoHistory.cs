using System.Collections.Generic;

namespace RosettaUI.UndoSystem
{
    public static class UndoHistory
    {
        private static readonly Stack<IUndoRecord> UndoStack = new();
        private static readonly Stack<IUndoRecord> RedoStack = new();

        private static bool _isProcessing;
        
        
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
        
        private static void RemoveTopExpiredRecords(Stack<IUndoRecord> stack)
        {
            while (stack.TryPeek(out var record) && record.IsExpired)
            {
                stack.Pop().Dispose();
            }
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