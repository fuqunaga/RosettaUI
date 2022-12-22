using System.Linq;
using TMPro;
using UnityEngine;
using RosettaUI.Reactive;


namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_Dropdown(Element element)
        {
            var go = Instantiate(element, resource.dropdown);

            var dropdownElement = element as DropdownElement;

            var dropdown = go.GetComponentInChildren<TMP_Dropdown>();
            dropdown.colors = settings.theme.dropdownColors;
            dropdown.captionText.color = settings.theme.textColor;
            dropdown.captionText.fontSize = settings.fontSize;

            // font==nullだとAwake()→LoadDeafault()でfontSizeを書き換えられてしまうので、
            // fontをセットしておく
            var itemText = dropdown.itemText;
            if (itemText.font == null)
            {
                itemText.font = TMP_Settings.defaultFontAsset;
            }
            itemText.fontSize = settings.fontSize;


            dropdown.options = dropdownElement.options?.Select(str => new TMP_Dropdown.OptionData(str)).ToList();

            dropdown.onValueChanged.AddListener(dropdownElement.GetViewBridge().SetValueFromView);
            
            if (!dropdownElement.IsConst)
            {
                dropdownElement.GetViewBridge().SubscribeValueOnUpdate(v => dropdown.value = v);
            }
            
            SubscribeInteractable(element, dropdown, dropdown.captionText);

            return BuildField_AddLabelIfHas(go, dropdownElement);
        }
    }
}