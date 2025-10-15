using System;
using RosettaUI.Utilities;

namespace RosettaUI.Undo
{
    /// <summary>
    /// FieldBaseElementのValueを記録し復元するUndo機能向けのレコード
    /// UndoRecordListItemRemoveで利用される
    /// </summary>
    public class FieldBaseElementRestoreRecord<TValue> : ObjectPoolItem<FieldBaseElementRestoreRecord<TValue>>, IElementRestoreRecord
    {
        public static IElementRestoreRecord Create(Element element)
        {
            if (element is not FieldBaseElement<TValue> fieldBaseElement)
            {
                throw new ArgumentException($"element must be FieldBaseElement<{typeof(TValue).Name}>");
            }
            
            var record = GetPooled();
            record.Initialize(fieldBaseElement);
            return record;
        }
        
        
        private TValue _value;

        private void Initialize(FieldBaseElement<TValue>  fieldBaseElement)
        {
            _value = UndoHelper.Clone(fieldBaseElement.Value);
        }

        public bool TryRestore(IUndoRestoreElement element)
        {
            if (element is not FieldBaseElement<TValue> fieldBaseElement)
            {
                return false;
            }
            
            fieldBaseElement.GetViewBridge().SetValueFromView(UndoHelper.Clone(_value));
            return true;
        }
    }
}