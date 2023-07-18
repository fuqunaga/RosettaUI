using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace RosettaUI.UIToolkit
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

        /// <summary>
        /// 画面からはみ出さないように補正する
        /// </summary>
        /// <param name="position">表示しようとしている座標</param>
        /// <param name="target">対象</param>
        public static bool CheckOutOfScreen(Vector2 position, VisualElement target)
        {
            var pos = position;
            var root = target.panel.visualTree.Q<TemplateContainer>()
                       ?? target.panel.visualTree.Query(null, RosettaUIRootUIToolkit.USSRootClassName).First();
            
            bool isChanged = false;
            
            if(root.worldBound.x > pos.x)
            {
                pos.x = root.worldBound.x;
                isChanged = true;
            }
            if (root.worldBound.width <= pos.x + target.resolvedStyle.width)
            {
                pos.x = root.worldBound.width - target.resolvedStyle.width;
                isChanged = true;
            }

            if(root.worldBound.y > pos.y)
            {
                pos.y = root.worldBound.y;
                isChanged = true;
            }
            if (root.worldBound.height <= pos.y + target.resolvedStyle.height)
            {
                pos.y = root.worldBound.height - target.resolvedStyle.height;
                isChanged = true;
            }

            if (isChanged)
            {
                target.style.left = pos.x;
                target.style.top = pos.y;
            }

            return isChanged;
        }
    }
}