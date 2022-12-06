using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class ElementBindingExtension
    {
        // Subscribe element -> field
        public static void SubscribeValueOnUpdateCallOnce<T>(this ReadOnlyValueElement<T> element, INotifyValueChanged<T> field)
        {
            element.GetViewBridge().SubscribeValueOnUpdateCallOnce(field.SetValueWithoutNotifyIfNotEqual);
        }
        
        // Subscribe field -> element
        public static void Subscribe<TValue>(this INotifyValueChanged<TValue> field, FieldBaseElement<TValue> element)
        {
            var viewBridge = element.GetViewBridge();
            
            field.RegisterValueChangedCallback(OnValueChanged);
            viewBridge.onUnsubscribe += () => field?.UnregisterValueChangedCallback(OnValueChanged);

            void OnValueChanged(ChangeEvent<TValue> evt)
            {
                viewBridge.SetValueFromView(evt.newValue);
            }
        }

        // Subscribe element <-> field
        public static void Bind<T>(this FieldBaseElement<T> element, INotifyValueChanged<T> field)
        {
            element.SubscribeValueOnUpdateCallOnce(field);
            field.Subscribe(element);
        }

        public static void Bind<TFieldValue, TElementValue>(
            this INotifyValueChanged<TFieldValue> field,
            FieldBaseElement<TElementValue> element,
            Func<TElementValue, TFieldValue> elementValueToFieldValue,
            Func<TFieldValue, TElementValue> fieldValueToElementValue
        )
        {
            var viewBridge = element.GetViewBridge();
            
            viewBridge.SubscribeValueOnUpdateCallOnce(v => field.SetValueWithoutNotifyIfNotEqual(elementValueToFieldValue(v)));
            field.RegisterValueChangedCallback(evt => viewBridge.SetValueFromView(fieldValueToElementValue(evt.newValue)));
        }
    }
}