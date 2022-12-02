using RosettaUI.Reactive;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_IntField(Element element)
        {
            return Build_InputField((IntFieldElement)element, resource.inputField, TMP_InputField.ContentType.IntegerNumber, TryParseInt);
        }
        static GameObject Build_FloatField(Element element)
        {
            return Build_InputField((FloatFieldElement)element, resource.inputField, TMP_InputField.ContentType.DecimalNumber, TryParseFloat);
        }

        static GameObject Build_StringField(Element element)
        {
            return Build_InputField((TextFieldElement)element, resource.inputField, TMP_InputField.ContentType.Standard, str => (true, str));
        }

        static GameObject Build_BoolField(Element element)
        {
            var go = Instantiate(element, resource.toggle);

            var boolField = element as ToggleElement;

            var toggle = go.GetComponentInChildren<Toggle>();
            toggle.colors = settings.theme.fieldColors;

            toggle.onValueChanged.AddListener(boolField.GetViewBridge().SetValueFromView);

            if (!boolField.IsConst)
            {
                boolField.GetViewBridge().SubscribeValueOnUpdate((v) => toggle.isOn = v);
            }

            element.interactableRx.Subscribe( (interactable) =>
            {
                toggle.interactable = interactable;

                // Hiding the check mark is done by alpha, so when IsOn==false, it is not touched.
                if (toggle.isOn)
                {
                    SetTextColorWithInteractable(toggle.graphic, toggle.colors.fadeDuration, interactable);
                }
            });

            return BuildField_AddLabelIfHas(go, boolField);
        }



        #region Utility Function

        static (bool, int) TryParseInt(string str)
        {
            var success = int.TryParse(str, out var ret);
            return (success, ret);
        }

        static (bool, float) TryParseFloat(string str)
        {
            var success = float.TryParse(str, out var ret);
            return (success, ret);
        }

        static GameObject Build_InputField<T>(FieldBaseElement<T> field, GameObject prefab, TMP_InputField.ContentType contentType, Func<string, (bool, T)> tryParse)
        {
            return Build_InputField(field, prefab, contentType, tryParse, out var _);
        }


        static GameObject Build_InputField<T>(FieldBaseElement<T> field, GameObject prefab, TMP_InputField.ContentType contentType, Func<string, (bool, T)> tryParse, out TMP_InputField inputFieldUI)
        {
            var go = Instantiate(field, prefab);

            inputFieldUI = go.GetComponentInChildren<TMP_InputField>();
            inputFieldUI.contentType = contentType;
            inputFieldUI.text = field.Value?.ToString();
            inputFieldUI.pointSize = settings.fontSize;

            inputFieldUI.colors = settings.theme.fieldColors;
            inputFieldUI.selectionColor = settings.theme.selectionColor;
            inputFieldUI.textComponent.color = settings.theme.textColor;

            inputFieldUI.onValueChanged.AddListener((str) =>
            {
                var (success, v) = tryParse(str);
                if (success)
                {
                    field.GetViewBridge().SetValueFromView(v);
                }
            }
            );

            var capturedInputFieldUI = inputFieldUI; // capture UI for lambda

            if (!field.IsConst)
            {
                field.GetViewBridge().SubscribeValueOnUpdate((v) =>
                {
                    var (success, viewValue) = tryParse(capturedInputFieldUI.text);
                    var isDifferent = !success || !(v?.Equals(viewValue) ?? (viewValue == null));

                    if (isDifferent)
                    {
                        capturedInputFieldUI.text = v?.ToString() ?? "";
                    }
                });
            }

            SubscribeInteractable(field, capturedInputFieldUI, capturedInputFieldUI.textComponent);


            return BuildField_AddLabelIfHas(go, field);
        }


        static GameObject BuildField_AddLabelIfHas<T>(GameObject go, FieldBaseElement<T> field)
        {
            var label = field.Label;
            if (label != null)
            {
                var parent = Build_RowAt((string)label, field);

                var labelGo = Build_Label(label, field);
                labelGo.transform.SetParent(parent.transform);

                go.transform.SetParent(parent.transform);

                return parent;
            }
            return go;
        }

        #endregion
    }
}