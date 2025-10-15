namespace RosettaUI.Undo
{
    public class UndoRecordListItemMove : UndoRecordElementBase<UndoRecordListItemMove, ListViewItemContainerElement>
    {
        private int _fromIndex;
        private int _toIndex;
        
        
        public override string Name => $"List Item Move ({_fromIndex} -> {_toIndex})";
        
        private void Initialize(ListViewItemContainerElement listElement, int fromIndex, int toIndex)
        {
            base.Initialize(listElement);
            _fromIndex = fromIndex;
            _toIndex = toIndex;
        }
        
        public override void Undo()
        {
            throw new System.NotImplementedException();
        }

        public override void Redo()
        {
            throw new System.NotImplementedException();
        }
    }
}