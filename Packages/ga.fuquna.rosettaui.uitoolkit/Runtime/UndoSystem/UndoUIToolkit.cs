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
        public static void RecordBaseField<TValue>(BaseField<TValue> field, TValue oldValue, TValue newValue)
        {
            var element = UIToolkitBuilder.Instance.GetElement(field);
            if (element is not FieldBaseElement<TValue> fieldElement)
            {
                Debug.LogWarning($"UndoBaseField.Record: element is not FieldBaseElement<{typeof(TValue)}>.");
                return;
            }

            Undo.RecordFieldBaseElement(fieldElement, oldValue, newValue);
        }
    }
}