using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace RosettaUI.Test
{
    /// <summary>
    /// PopupMenuが複数設定されているときのマージやセパレーター追加の挙動をテストする
    /// </summary>
    [RequireComponent(typeof(RosettaUIRoot))]
    public class TestPopupMenu : MonoBehaviour
    {
        private void Start()
        {
            var root = GetComponent<RosettaUIRoot>();
            root.Build(CreateElement());
        }

        private Element CreateElement()
        {
            return UI.Window(
                UI.Box(
                    UI.Tabs(
                        ("Check marge menu items", CreateCheckSameItemNameOverrideElement)
                    )
                )
            );
        }

        /// <summary>
        /// IElementCreatorの参照元が変化してもUIが正しく更新されるか確認する
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
        private static Element CreateCheckSameItemNameOverrideElement()
        {
            var sameItemName = false;
            var subMenu = false;
            
            return UI.Column(
                UI.HelpBox(
                        "- When multiple popup menus overlap, they are automatically separated by separators.\n" +
                        "- When items have the same name, they will be overwritten by items from child menus."
                    ,
                    HelpBoxType.Info),
                UI.Toggle("Same Item Name", () => sameItemName),
                UI.Toggle("Sub Menu", () => subMenu),
                UI.DynamicElementOnStatusChanged(
                    () => (sameItemName, subMenu),
                    _ =>
                    {
                        var red = CreateItem3PopupMenuElement(null, "Red").SetBackgroundColor(new Color(0.5f, 0.2f, 0.2f));
                        var green = CreateItem3PopupMenuElement(red, "Green").SetBackgroundColor(new Color(0.2f, 0.5f, 0.2f));
                        var blue = CreateItem3PopupMenuElement(green, "Blue").SetBackgroundColor(new Color(0.2f, 0.2f, 0.5f));

                        return blue;
                    }
                )
            );
            
            
            Element CreateItem3PopupMenuElement(Element target, string prefix)
            {
                var itemLabel = sameItemName ? "Item" : $"{prefix}-Item";
                if (subMenu)
                {
                    itemLabel = $"Sub/{itemLabel}";
                }
                
                return UI.Popup(
                    UI.Column(
                        UI.Label($"{prefix}"),
                        UI.Label($"- {itemLabel}0\n- {itemLabel}1\n- {itemLabel}2"),
                        UI.Row(
                            UI.Space().SetWidth(10f),
                            target,
                            UI.Space().SetWidth(10f)
                        ),
                        UI.Space().SetHeight(10f)
                    ),
                    () => new[]
                    {
                        new MenuItem($"{itemLabel}0", () => Debug.Log($"{prefix} - Item0")),
                        new MenuItem($"{itemLabel}1", () => Debug.Log($"{prefix} - Item1")),
                        new MenuItem($"{itemLabel}2", () => Debug.Log($"{prefix} - Item2")),
                    }
                );
            }
        }
    }
}