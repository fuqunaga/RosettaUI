using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static VisualElement Build_Label(Element element)
        {
            var labelElement = (LabelElement) element;
            var label = new Label(labelElement.Value);
            SetupLabelStyle(label, labelElement);
            label.ListenValue(labelElement);
            
            return label;
        }


        private static void SetupLabelStyle(TextElement label, LabelElement labelElement)
        {
            if (labelElement.IsLeftMost())
            {
                label.style.minWidth = Mathf.Max(0f,
                    LayoutSettings.LabelWidth - labelElement.GetIndent() * LayoutSettings.IndentSize);

                // Foldout直下のラベルはmarginRight、paddingRightがUnityDefaultCommon*.uss で書き換わるので上書きしておく
                // セレクタ例： .unity-foldout--depth-1 > .unity-base-field > .unity-base-field__label
                label.style.marginRight = LayoutSettings.LabelMarginRight;
                label.style.paddingRight = LayoutSettings.LabelPaddingRight;
            }
        }
        
        
        static void SetupLabelCallback<T, TElementValue>(BaseField<T> field, ReadOnlyFieldElement<TElementValue> fieldBaseElement)
        {
            var labelElement = fieldBaseElement.label;
            if (labelElement != null)
            {
                SetupLabelStyle(field.labelElement, labelElement);
                field.ListenLabel(labelElement);
            }
        }
    }
}