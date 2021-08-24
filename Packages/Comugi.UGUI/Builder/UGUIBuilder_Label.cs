using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Comugi.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_Label(Element element, Element containerElement = null)
        {
            var label = (LabelElement)element;
            var initialValue = label.GetInitialValue();


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
                if (hierarchyElement.IsPrefix())
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
            textUI.text = stringElement.GetInitialValue();
            textUI.fontSize = settings.fontSize;
            textUI.color = settings.theme.labelColor;
            if (!stringElement.IsConst)
            {
                stringElement.RegisterSetValueToView((s) => textUI.text = s);
            }

            RegisterSetInteractable(stringElement, (interactable) =>
            {
                var theme = settings.theme;

                var alpha = interactable ? theme.labelColor.a : theme.textAlphaOnDisable;
                textUI.CrossFadeAlpha(alpha, 0f, true);
            });

        }
    }
}