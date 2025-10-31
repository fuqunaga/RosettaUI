using System;

namespace RosettaUI.UndoSystem
{
   public class UndoRecordFieldBaseElement<TValue> : UndoRecordElementBase<FieldBaseElement<TValue>>
    {
        private readonly ValueChangeRecord<TValue> _valueChangeRecord = new();
        
        public override string Name => $"{(string)Element.Label} ({typeof(TValue).Name}: {_valueChangeRecord})";
        

        public void Initialize(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            base.Initialize(field);
            _valueChangeRecord.Initialize(before, after);
        }
        
        public override void Dispose()
        {
            _valueChangeRecord.Clear();
            base.Dispose();
        }
        
        public override void Undo()
        {
            if (hierarchyPath.TryGetExistingElement(out var element) && element is FieldBaseElement<TValue> fieldElement)
            {
                fieldElement.GetViewBridge().SetValueFromView(_valueChangeRecord.Before);
            }
        }

        public override void Redo()
        {
            if (hierarchyPath.TryGetExistingElement(out var element) && element is FieldBaseElement<TValue> fieldElement)
            {
                fieldElement.GetViewBridge().SetValueFromView(_valueChangeRecord.After);
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

            _valueChangeRecord.AfterRaw = r._valueChangeRecord.AfterRaw;
        }
    }
   
   
    public static partial class Undo
    {
        public static void RecordFieldBaseElement<TValue>(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            using (UndoRecorder<UndoRecordFieldBaseElement<TValue>>.Get(out var record))
            {
                record.Initialize(field, before, after);
            }
        }
    }
}