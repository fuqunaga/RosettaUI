using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

#if !UNITY_2022_1_OR_NEWER
using RosettaUI.UIToolkit.UnityInternalAccess;
#endif

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// choicesの全要素が収まるようにwidthを広げるPopupField
    ///
    /// BasePopupField.PopupTextElementを継承してDoMeasure()をoverrideするのがよさそうだが、
    /// 継承したクラスをBasePopupField.m_TextElementにセットする方法がなさそうなので
    /// styleでminWidthを指定する
    /// </summary>
    public class PopupFieldCustom<T> : PopupField<T>
    {
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
    }
}