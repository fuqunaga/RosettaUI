namespace RosettaUI.UndoSystem
{
    public abstract class UndoRecordElementBase<TElement> : IUndoRecord
        where TElement : Element
    {
        protected readonly ElementHierarchyPath hierarchyPath = new();
        
        protected TElement Element => hierarchyPath.TargetElement as TElement;
        
        protected void Initialize(TElement targetElement) => hierarchyPath.Initialize(targetElement);
        
        
        public abstract string Name { get; }
        public bool IsAvailable => hierarchyPath.TryGetExistingElement(out _);
        
        public abstract void Undo();
        public abstract void Redo();

        public virtual bool CanMerge(IUndoRecord newer) => false;
        public virtual void Merge(IUndoRecord newer)
        {
        }
        
        public virtual void Dispose()
        {
            hierarchyPath.Clear();
        }
    }
}