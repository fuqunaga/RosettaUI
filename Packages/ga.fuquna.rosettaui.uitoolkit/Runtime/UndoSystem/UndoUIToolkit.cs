using RosettaUI.UIToolkit.Builder;
using RosettaUI.UndoSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UndoSystem
{
    /// <summary>
    /// Undo helper class for BaseField
    /// </summary>
    public static class UndoUIToolkit
    {
        public static void RecordBaseField<TValue>(string label, BaseField<TValue> field, TValue oldValue, TValue newValue)
        {
            // 対応するFieldBaseElementがあればそちらでUndoを記録する
            var element = UIToolkitBuilder.Instance.GetElement(field);
            if (element is FieldBaseElement<TValue> fieldElement)
            {
                Undo.RecordFieldBaseElement(fieldElement, oldValue, newValue);
                return;
            }

            // 無い場合は独自でUndoを記録する
            Undo.RecordValueChange(label, oldValue, newValue, value => field.value = value);
        }
    }
}