using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
#if !UNITY_2022_1_OR_NEWER
using RosettaUI.UIToolkit.UnityInternalAccess;
#endif

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// PopupFieldの拡張
    /// 
    /// 1. choicesの全要素が収まるようにwidthを広げるPopupField
    /// BasePopupField.PopupTextElementを継承してDoMeasure()をoverrideするのがよさそうだが、
    /// 継承したクラスをBasePopupField.m_TextElementにセットする方法がなさそうなので
    /// styleでminWidthを指定する
    ///
    /// 2. ドロップダウンメニューをanchoredをfalse（指定矩形の幅を使用しない）で呼ぶ
    /// BasePopupFieldが行っているanchored==trueで呼ぶとvisualInputの幅でメニューが表示されるが、
    /// これが十分な幅ではないためメニューないにスクロールバーが出てしまう
    /// 外側Windowが小さいサイズにされた場合はさらにvisualInputが小さくなってしまう
    /// </summary>
    public class PopupFieldCustom<T> : PopupField<T>
    {
        #region choicesの全要素が収まるようにwidthを広げる
        
        public override List<T> choices
        {
            get => base.choices;
            set
            {
                if (base.choices == value || (base.choices?.SequenceEqual(value) ?? false)) return;

                base.choices = value;
                AdjustMinWidthWithChoices();
            }
        }

        private void AdjustMinWidthWithChoices()
        {
            if (choices == null || !choices.Any()) return;

            if (textElement.panel == null)
            {
                textElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
            else
            {
                DoAdjustMinWidthWithChoices();
            }

            void OnGeometryChanged(GeometryChangedEvent _)
            {
                textElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                DoAdjustMinWidthWithChoices();
            }
        }
        
        
        private void DoAdjustMinWidthWithChoices()
        {
            var maxWidth = choices
                .Select(GetValueToDisplay)
                .Select(str => textElement.MeasureTextSize(str, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined).x)
                .Max();

            textElement.style.minWidth = maxWidth;
        }
        
        private string GetValueToDisplay(T str)
        {
            return formatListItemCallback?.Invoke(str) ??
                   (choices.Contains(str) ? str.ToString() : string.Empty);
        }
        
        #endregion

        
        #region ドロップダウンメニューをanchorをfalse（指定矩形の幅を使用しない）で呼ぶ

        private static readonly FieldInfo CreateMenuCallbackFieldInfo = typeof(BasePopupField<T, T>).GetField("createMenuCallback", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public PopupFieldCustom()
        {
            CreateMenuCallbackFieldInfo.SetValue(this, (Func<GenericDropdownMenu>)GenericDropdownMenuIgnoreAnchoredBuilder.CreateGenericDropdownMenuIgnoreAnchored);
        }
        
        #endregion
    }
}