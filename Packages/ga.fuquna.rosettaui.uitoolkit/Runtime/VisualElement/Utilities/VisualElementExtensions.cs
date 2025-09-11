using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace RosettaUI.UIToolkit
{
    public static class VisualElementExtensions
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
            return;

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
        public static bool ClampPositionToScreen(Vector2 position, VisualElement target)
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
        
        /// <summary>
        /// elementがparentの範囲内に収まるように位置を調整する
        /// </summary>
        /// <param name="element"></param>
        /// <param name="parent"></param>
        /// <returns>位置を調整したかどうか</returns>
        public static bool ClampElementToParent(this VisualElement element, VisualElement parent = null)
        {
            parent ??= element.parent;
            if (element == null || parent == null)
            {
                return false;
            }
            
            var elementRect = element.worldBound;
            var rootRect = parent.worldBound;

            var xMove = 0f;
            var yMove = 0f;
            
            if (elementRect.xMin < rootRect.xMin)
            {
                xMove = rootRect.xMin - elementRect.xMin;
            }
            else if (elementRect.xMax > rootRect.xMax)
            {
                xMove = rootRect.xMax - elementRect.xMax;
            }
            

            if (elementRect.yMin < rootRect.yMin)
            {
                yMove = rootRect.yMin - elementRect.yMin;
            }
            else if (elementRect.yMax > rootRect.yMax)
            {
                yMove = rootRect.yMax - elementRect.yMax;
            }

            if (xMove == 0f && yMove == 0f)
            {
                return false;
            }

            var resolvedStyle = element.resolvedStyle;
            if (xMove != 0f)
            {
                element.style.left = resolvedStyle.left + xMove;
            }

            if (yMove != 0f)
            {
                element.style.top = resolvedStyle.top + yMove;
            }
            
            return true;
        }

        
#if UNITY_6000_0_OR_NEWER
        
        private static float GetScaledPixelsPerPoint(this VisualElement ve) => ve.scaledPixelsPerPoint;
        
#else

        private delegate float ScaledPixelsPerPointPropertyGetFunc(VisualElement ve);
        private static ScaledPixelsPerPointPropertyGetFunc _getScaledPixelsPerPointPropertyGetFunc;
        private static float GetScaledPixelsPerPoint(this VisualElement ve)
        {
            if (_getScaledPixelsPerPointPropertyGetFunc == null)
            {
                var type = typeof(VisualElement);
                var property = type.GetProperty("scaledPixelsPerPoint", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (property != null)
                {
                    _getScaledPixelsPerPointPropertyGetFunc = (ScaledPixelsPerPointPropertyGetFunc)Delegate.CreateDelegate(typeof(ScaledPixelsPerPointPropertyGetFunc), property.GetGetMethod(true));
                }
            }

            return _getScaledPixelsPerPointPropertyGetFunc?.Invoke(ve) ?? 1f;
        }
        
#endif
        
        /// <summary>
        /// UIToolkitのWidthはPanelのスケールなどがかかっているのでスクリーン上のピクセル数は計算で求める
        /// </summary>
        public static float CalcWidthPixelOnScreen(this VisualElement ve)
        {
            if ( ve.panel == null )
            {
                return 0f;
            }
            
            return ve.resolvedStyle.width * ve.GetScaledPixelsPerPoint();
        }
        /// <summary>
        /// UIToolkitのHeightはPanelのスケールなどがかかっているのでスクリーン上のピクセル数は計算で求める
        /// </summary>
        public static float CalcHeightPixelOnScreen(this VisualElement ve)
        {
            if ( ve.panel == null )
            {
                return 0f;
            }
            
            return ve.resolvedStyle.height * ve.GetScaledPixelsPerPoint();
        }
        
        /// <summary>
        /// 親要素からの相対位置を取得する
        /// </summary>
        public static Vector2 GetLocalPosition(this VisualElement ve)
        {
            // ve.layout.positionは親のパディングやボーダーを含めた「レイアウト結果」としての位置なので使用しない
            var style = ve.resolvedStyle;
            return new Vector2(style.left, style.top);
        }
        
        public static bool IsShown(this VisualElement ve)
        {
            return ve.style.display != DisplayStyle.None;
        }
        
        public static void Show(this VisualElement ve)
        {
            ve.style.display = DisplayStyle.Flex;
        }
        
        public static void Hide(this VisualElement ve)
        {
            ve.style.display = DisplayStyle.None;
        }

        public static void SetShow(this VisualElement ve, bool flag)
        {
            if (flag)
            {
                ve.Show();
            }
            else
            {
                ve.Hide();
            }
            
        }
    }
}