namespace RosettaUI.UndoSystem
{
    public static class UndoRecordFlexBaseExtensions
    {
        public static UndoRecordFlexBase AvailableWhileElementEnabled(this UndoRecordFlexBase record, Element element)
        {
            record.IsAvailableFunc += element.EnableInHierarchy;
            return record;
        }
        
        public static UndoRecordFlexBase DisableMarge(this UndoRecordFlexBase record)
        {
            record.CanMargeFunc += _ => false;
            return record;
        }
    }
}