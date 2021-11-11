using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RosettaUI.Reactive;
using RosettaUI.Builder;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_Label(Element element, Element containerElement = null)
        {
            var label = (LabelElement)element;
            var initialValue = label.Value;


            // ラベルが変化するものは固定サイズにしておく。
            // 毎フレームtextが変わって自動レイアウトが再計算されると重いので、波及しないように固定のRectTransformで包む
            var isFixedSize = !label.IsConst;

            var go = Instantiate(initialValue, isFixedSize ? resource.fixedSizeLabel : resource.label);
            var trans = go.transform;


            var textUI = trans.GetComponentInChildren<TMP_Text>();
            SetupTextUIWithStringReadOnlyValueElement(label, textUI);

            var layoutElement = trans.GetComponentInChildren<LayoutElement>();

            if ( !isFixedSize)
            {
                var hierarchyElement = containerElement ?? label;
                //if (hierarchyElement.IsCompositeFieldLabel())
                if ( hierarchyElement.IsLeftMost())
                {
                    layoutElement.preferredWidth -= hierarchyElement.GetIndent() * settings.paddingIndent;
                    go.name = $"{label.GetIndent()}";
                }
                else
                {
                    layoutElement.minWidth = -1f;
                    layoutElement.preferredWidth = -1f;
                }
            }
            return go;
        }


        static void SetupTextUIWithStringReadOnlyValueElement(ReadOnlyValueElement<string> stringElement, TMP_Text textUI)
        {
            textUI.text = stringElement.Value;
            textUI.fontSize = settings.fontSize;
            textUI.color = settings.theme.labelColor;
            if (!stringElement.IsConst)
            {
                stringElement.SubscribeValueOnUpdate(s => textUI.text = s);
            }

            stringElement.interactableRx.Subscribe((interactable) =>
            {
                var theme = settings.theme;

                var alpha = interactable ? theme.labelColor.a : theme.textAlphaOnDisable;
                textUI.CrossFadeAlpha(alpha, 0f, true);
            });

        }
    }
}