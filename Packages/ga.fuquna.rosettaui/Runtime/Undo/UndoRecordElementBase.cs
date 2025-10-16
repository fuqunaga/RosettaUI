using RosettaUI.Utilities;

namespace RosettaUI.Undo
{
    public abstract class UndoRecordElementBase<TUndoRecord, TElement> : ObjectPoolItem<TUndoRecord>, IUndoRecord
        where TUndoRecord : UndoRecordElementBase<TUndoRecord, TElement>, new()
        where TElement : Element
    {
        protected readonly ElementHierarchyPath hierarchyPath = new();
        
        protected TElement Element => hierarchyPath.TargetElement as TElement;
        
        protected void Initialize(TElement targetElement) => hierarchyPath.Initialize(targetElement);
        
        
        public abstract string Name { get; }
        public bool IsExpired => !hierarchyPath.TryGetExistingElement(out _);
        
        public abstract void Undo();
        public abstract void Redo();

        public virtual bool CanMerge(IUndoRecord newer) => false;
        public virtual void Merge(IUndoRecord newer)
        {
        }
        
        public override void Dispose()
        {
            hierarchyPath.Clear();
            base.Dispose();
        }
    }
}