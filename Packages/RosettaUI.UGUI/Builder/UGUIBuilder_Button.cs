using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_Button(Element element)
        {
            var go = Instantiate(element, resource.button);

            var buttonElement = element as ButtonElement;
            var button = go.GetComponentInChildren<Button>();
            button.colors = settings.theme.buttonColors;
            button.GetComponentInChildren<TMP_Text>().color = settings.theme.textColor;

            button.onClick.AddListener(() => buttonElement.OnClick?.Invoke());

            var textUI = go.GetComponentInChildren<TMP_Text>();
            if (textUI != null)
            {
                SetupTextUIWithStringReadOnlyValueElement(buttonElement, textUI);
            }

            SubscribeInteractable(buttonElement, button, textUI);

            return go;
        }
    }
}