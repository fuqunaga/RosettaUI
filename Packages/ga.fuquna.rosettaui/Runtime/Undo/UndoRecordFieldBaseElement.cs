using System;

namespace RosettaUI.Undo
{
   public class UndoRecordFieldBaseElement<TValue> : UndoRecordElementBase<UndoRecordFieldBaseElement<TValue>, FieldBaseElement<TValue>>
    {
        public static void Record(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            var record = GetPooled();
            record.Initialize(field, before, after);
            UndoHistory.Add(record);
        }
        
        
        private TValue _before;
        private TValue _after;
        
        
        public override string Name => $"{(string)Element.Label} ({typeof(TValue).Name}: [{_before}] -> [{_after}])";
        

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
            if (hierarchyPath.TryGetExistingElement(out var element) && element is FieldBaseElement<TValue> fieldElement)
            {
                fieldElement.GetViewBridge().SetValueFromView(UndoHelper.Clone(_before));
            }
        }

        public override void Redo()
        {
            if (hierarchyPath.TryGetExistingElement(out var element) && element is FieldBaseElement<TValue> fieldElement)
            {
                fieldElement.GetViewBridge().SetValueFromView(UndoHelper.Clone(_after));
            }
        }

        public override bool CanMerge(IUndoRecord newer)
        {
            return (newer is UndoRecordFieldBaseElement<TValue> r)
                   && Element == r.Element
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