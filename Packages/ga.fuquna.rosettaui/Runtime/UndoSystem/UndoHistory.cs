using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI.UndoSystem
{
    public static class UndoHistory
    {
        /// <summary>
        /// IUndoRecordを保持し、Dispose時に指定のActionを呼び出す
        /// IUndoRecordにObjectPoolの仕組みは入れたくないが実質ObjectPoolを利用したい
        /// UndoRecorderと連携してObjectPool管理するためのもの
        /// </summary>
        private readonly struct RecordHolder
        {
            public IUndoRecord Record { get; }
            private readonly Action<IUndoRecord> _onDispose;

            public bool IsValid => Record != null;

            public RecordHolder(IUndoRecord record, Action<IUndoRecord> onDispose)
            {
                Record = record;
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                Record.Dispose();
                _onDispose?.Invoke(Record);
            }
        }

        /// <summary>
        /// Undo/Redo用のスタック自体をスタックする
        /// 主にModalEditorで一時的にUndo/Redo履歴過去と切り離して管理する場合に利用する 
        /// </summary>
        private static readonly Stack<(string name, Stack<RecordHolder> undoStack, Stack<RecordHolder> redoStack)> HistoryStacks = new();
        
        private static bool _isProcessing;
        private static bool _canTryMargeNextRecord = true;


        public static IEnumerable<string> HistoryStackNames => HistoryStacks.Select(stack => stack.name);
        public static IEnumerable<IUndoRecord> UndoRecords => UndoStack.Select(holder => holder.Record);
        public static IEnumerable<IUndoRecord> RedoRecords => RedoStack.Select(holder => holder.Record);
        
        public static bool CanUndo => UndoStack.Count > 0;
        public static bool CanRedo => RedoStack.Count > 0;

        // Undo/Redo処理中はAddできない
        public static bool CanAdd => !_isProcessing;

        
        private static Stack<RecordHolder> UndoStack => HistoryStacks.Peek().undoStack;
        private static Stack<RecordHolder> RedoStack => HistoryStacks.Peek().redoStack;

        
        static UndoHistory()
        {
            // 最初の履歴スタックを作成
            PushHistoryStack("Default");
        }
        
        public static void PushHistoryStack(string name)
        {
            Assert.IsFalse(_isProcessing, "Cannot push history stack during undo/redo processing");
            FixLastUndoRecord();
            
            HistoryStacks.Push((name, new Stack<RecordHolder>(), new Stack<RecordHolder>()));
        }
        
        public static void PopHistoryStack()
        {
            Assert.IsFalse(_isProcessing, "Cannot pop history stack during undo/redo processing");
            FixLastUndoRecord();
            
            if (HistoryStacks.Count <= 1)
            {
                return;
            }
            
            var (_, undoStack, redoStack) = HistoryStacks.Pop();
            ClearStack(undoStack);
            ClearStack(redoStack);
        }
        
        
        /// <summary>
        /// UndoStackにrecordを追加する。
        /// </summary>
        public static void Add(IUndoRecord record, Action<IUndoRecord> onDispose = null, bool disposeRecordIfNotStacked = true)
        {
            if (record == null) return;
            
            var holder = new RecordHolder(record, onDispose);
            if (_isProcessing)
            {
                if (disposeRecordIfNotStacked) holder.Dispose();
                return;
            }
            

            RemoveTopUnavailableRecords(UndoStack);
            
            // マージ可能条件
            // - RedoStackが空
            // - _canTryMargeNextRecord==true
            //   - 同じElementでも一度フォーカスが外れたらマージしないようにしたいので外部から_canTryMargeNextRecordで制御できるようにする
            if (RedoStack.Count <= 0
                && _canTryMargeNextRecord
                && UndoStack.TryPeek(out var previous)
                && previous.Record.CanMerge(record))
            {
                previous.Record.Merge(record);
                if (disposeRecordIfNotStacked) record.Dispose();
            }
            else
            {
                UndoStack.Push(holder);
                _canTryMargeNextRecord = true;
            }

            ClearStack(RedoStack);
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
            if (!TryPopAndRemoveUnavailableRecords(UndoStack, out var holder)) return false;
            
            _isProcessing = true;
            holder.Record.Undo();
            _isProcessing = false;
            
            RedoStack.Push(holder);

            return true;
        }
        
        public static bool Redo()
        {
            if (!TryPopAndRemoveUnavailableRecords(RedoStack, out var holder)) return false;
            
            _isProcessing = true;
            holder.Record.Redo();
            _isProcessing = false;

            UndoStack.Push(holder);

            return true;
        }
        
        private static bool TryPopAndRemoveUnavailableRecords(Stack<RecordHolder> stack, out RecordHolder holder)
        {
            RemoveTopUnavailableRecords(stack);

            if (stack.TryPop(out holder))
            {
                return true;
            }

            holder = default;
            return false;
        }
        
        private static bool RemoveTopUnavailableRecords(Stack<RecordHolder> stack)
        {
            var removed = false;

            while (stack.TryPeek(out var holder) && !holder.Record.IsAvailable)
            {
                stack.Pop().Dispose();
                removed = true;
            }

            return removed;
        }

        private static void ClearStack(Stack<RecordHolder> stack)
        {
            while (stack.TryPop(out var record))
            {
                record.Dispose();
            }
        }
    }
}