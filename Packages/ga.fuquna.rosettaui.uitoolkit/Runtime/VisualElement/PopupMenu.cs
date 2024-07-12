using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// 右クリックでポップアップメニューを出す空のVisualElement
    /// 見た目などの設定は子供に別のVisualElementを追加する
    /// </summary>
    public class PopupMenu : VisualElement
    {
        public Func<IEnumerable<MenuItem>> CreateMenuItems { get; set; }
        
        
        public PopupMenu()
        {
#if true
            this.AddManipulator(new PopupMenuManipulator(() => CreateMenuItems?.Invoke()));
#else
            RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != ButtonIndex) return;
                
                var menuItems = CreateMenuItems?.Invoke();
                if (menuItems == null) return;
                
                PopupMenuUtility.Show(menuItems, evt.position, this);

                evt.StopPropagationAndFocusControllerIgnoreEvent();
            });
#endif
        }
    }
}