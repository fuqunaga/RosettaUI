using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Comugi.UGUI.Builder
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

            button.onClick.AddListener(() => buttonElement.onClick?.Invoke());

            var textUI = go.GetComponentInChildren<TMP_Text>();
            if (textUI != null)
            {
                SetupTextUIWithStringReadOnlyValueElement(buttonElement, textUI);
            }

            RegisterSetInteractable(buttonElement, button, textUI);

            return go;
        }
    }
}