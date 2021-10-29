using RosettaUI.Builder;
using RosettaUI.Reactive;
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
            SetupLabelCallback(label, labelElement);

            return label;
        }


        private static void SetupLabelCallback(Label label, LabelElement labelElement)
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

            if (!labelElement.IsConst)
            {
                labelElement.valueRx.Subscribe(text => label.text = text);
            }
        }

    }
}