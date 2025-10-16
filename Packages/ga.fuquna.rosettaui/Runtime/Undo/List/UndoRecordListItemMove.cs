namespace RosettaUI.Undo
{
    public class UndoRecordListItemMove : UndoRecordElementBase<UndoRecordListItemMove, ListViewItemContainerElement>
    {
        public static void Record(ListViewItemContainerElement listElement, int fromIndex, int toIndex)
        {
            if (!UndoHistory.CanAdd) return;
            
            var record = GetPooled();
            record.Initialize(listElement, fromIndex, toIndex);
            UndoHistory.Add(record);
        }
        
        
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
            Element.GetListEditor().MoveItem(_toIndex, _fromIndex);
        }

        public override void Redo()
        {
            Element.GetListEditor().MoveItem(_fromIndex, _toIndex);
        }
    }
}