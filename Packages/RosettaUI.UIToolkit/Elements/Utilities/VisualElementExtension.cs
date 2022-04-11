using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class VisualElementExtension
    {
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