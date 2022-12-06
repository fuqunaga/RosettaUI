using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class VisualElementExtension
    {
        public static void SetValueWithoutNotifyIfNotEqual<T>(this INotifyValueChanged<T> field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field.value, value))
            {
                field.SetValueWithoutNotify(value);
            }
        }
        
        /// <summary>
        /// ve.schedule.Execute() だと先に描画処理が走るようなのでより早く行うコールバック
        /// </summary>
        public static void ScheduleToUseResolvedLayoutBeforeRendering(this VisualElement ve, Action action)
        {
            ve.RegisterCallback<GeometryChangedEvent>(Callback);

            void Callback(GeometryChangedEvent _)
            {
                ve.UnregisterCallback<GeometryChangedEvent>(Callback);
                action();
            }
        }
    }
}