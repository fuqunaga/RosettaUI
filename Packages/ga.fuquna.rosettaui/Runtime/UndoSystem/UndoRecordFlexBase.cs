using System;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// デリゲートで挙動をカスタマイズできる汎用的なUndoRecordの基底クラス
    /// </summary>
    public abstract class UndoRecordFlexBase : IDisposable
    {
        public virtual bool IsAvailable => IsAvailableFunc?.Invoke() ?? true;

        public Func<bool> IsAvailableFunc { get; set; }
        public Func<IUndoRecord, bool> CanMargeFunc { get; set; }
        public Action<IUndoRecord> MergeAction { get; set; }

        
        public bool CanMerge(IUndoRecord newer)
        {
            return CanMargeFunc?.Invoke(newer) ?? CanMargeDefault(newer);
        }
        
        public void Merge(IUndoRecord newer)
        {
            if (MergeAction != null)
            {
                MergeAction(newer);
                return;
            }
            
            MargeDefault(newer);
        }

        protected virtual bool CanMargeDefault(IUndoRecord newer) => false;

        protected virtual void MargeDefault(IUndoRecord newer)
        {
        }


        public virtual void Dispose()
        {
            IsAvailableFunc = null;
            CanMargeFunc = null;
            MergeAction = null;
        }
    }
}