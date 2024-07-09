using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class ElementBindingExtension
    {
        // Subscribe element -> field
        public static void SubscribeValueOnUpdateCallOnce<T>(this ReadOnlyValueElement<T> element, INotifyValueChanged<T> field)
        {
            // string以外の参照型は比較しても更新されたかわからないのでSetValueWithoutNotify()で常にfieldに値をセットする
            // 値に応じて何かを更新するかどうかはfield側の実装に任せる
            Action<T> action =  typeof(T).IsValueType switch 
            {
                true => field.SetValueWithoutNotifyIfNotEqual,
                false when typeof(T) == typeof(string) => field.SetValueWithoutNotifyIfNotEqual,
                false => field.SetValueWithoutNotify
            };
            
            element.GetViewBridge().SubscribeValueOnUpdateCallOnce(action);
        }
        
        // Subscribe field -> element
        private static void Subscribe<TValue>(this INotifyValueChanged<TValue> field, FieldBaseElement<TValue> element)
        {
            field.Subscribe(element, v => v);
        }
        
        // Subscribe field -> element
        private static void Subscribe<TFieldValue, TElementValue>(
            this INotifyValueChanged<TFieldValue> field,
            FieldBaseElement<TElementValue> element,
            Func<TFieldValue, TElementValue> fieldValueToElementValue)
        {
            var viewBridge = element.GetViewBridge();
            
            field.RegisterValueChangedCallback(OnValueChanged);
            viewBridge.onUnsubscribe += () => field?.UnregisterValueChangedCallback(OnValueChanged);
            
            // TFieldValueがstringのとき
            // ラベルの更新とBaseFieldの値の更新の区別がつかないのでラベルのイベントは止めておく
            if (field is BaseField<TFieldValue> baseField)
            {
                baseField.labelElement.RegisterValueChangedCallback(e => e.StopPropagation());
            }

            return;

            void OnValueChanged(ChangeEvent<TFieldValue> evt)
            {
                viewBridge.SetValueFromView(fieldValueToElementValue(evt.newValue));
            }
        }

        // Subscribe element <-> field
        public static void Bind<T>(this FieldBaseElement<T> element, INotifyValueChanged<T> field)
        {
            element.SubscribeValueOnUpdateCallOnce(field);
            field.Subscribe(element);
        }

        // Subscribe element <-(change value)-> field
        public static void Bind<TElementValue, TFieldValue>(
            this FieldBaseElement<TElementValue> element,
            BaseField<TFieldValue> field,
            Func<TElementValue, TFieldValue> elementValueToFieldValue,
            Func<TFieldValue, TElementValue> fieldValueToElementValue
        )
        {
            var viewBridge = element.GetViewBridge();
            
            viewBridge.SubscribeValueOnUpdateCallOnce(v => field.SetValueWithoutNotifyIfNotEqual(elementValueToFieldValue(v)));
            field.Subscribe(element, fieldValueToElementValue);
        }
    }
}