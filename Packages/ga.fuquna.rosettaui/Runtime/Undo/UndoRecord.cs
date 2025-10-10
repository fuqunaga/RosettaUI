using System;
using RosettaUI.Utilities;

namespace RosettaUI.UndoSystem
{
    public interface IUndoRecord : IDisposable
    {
        string Name { get; }
        bool IsExpired { get; }
        
        void Undo();
        void Redo();
        
        bool CanMerge(IUndoRecord newer);
        void Merge(IUndoRecord newer);
    }
    
    
    public abstract class ElementUndoRecord<TUndoRecord> : ObjectPoolItem<TUndoRecord>, IUndoRecord
        where TUndoRecord : ElementUndoRecord<TUndoRecord>, new()
    {
        protected Element element;
        
        protected void Initialize(Element targetElement) => element = targetElement;
        
        public override void Dispose()
        {
            element = null;
            base.Dispose();
        }
        
        public abstract string Name { get; }
        public bool IsExpired => !element.EnableInHierarchy();
        public abstract void Undo();
        public abstract void Redo();

        public virtual bool CanMerge(IUndoRecord newer) => false;

        public virtual void Merge(IUndoRecord newer)
        {
        }
    }

    
    public class UndoRecordFieldBaseElement<TValue> : ElementUndoRecord<UndoRecordFieldBaseElement<TValue>>
    {
        public static void Record(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            var record = GetPooled();
            record.Initialize(field, before, after);
            UndoHistory.Add(record);
        }
        
        
        private TValue _before;
        private TValue _after;
        
        private FieldBaseElement<TValue> Element => (FieldBaseElement<TValue>)element;

        public override string Name => Element.Label;
        

        private void Initialize(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            base.Initialize(field);
            _before = UndoHelper.Clone(before);
            _after = UndoHelper.Clone(after);
        }
        
        public override void Dispose()
        {
            _before = default;
            _after = default;
            base.Dispose();
        }
        
        public override void Undo()
        {
            Element.GetViewBridge().SetValueFromView(UndoHelper.Clone(_before));
        }

        public override void Redo()
        {
            Element.GetViewBridge().SetValueFromView(UndoHelper.Clone(_after));
        }

        public override bool CanMerge(IUndoRecord newer)
        {
            return (newer is UndoRecordFieldBaseElement<TValue> r)
                   && element == r.element
                   && typeof(TValue) != typeof(bool);
        } 
        
        public override void Merge(IUndoRecord newer)
        {
            if (newer is not UndoRecordFieldBaseElement<TValue> r)
            {
                throw new InvalidOperationException($"Cannot merge {GetType()} with {newer.GetType()}");
            }

            _after = r._after;
        }
    }
}