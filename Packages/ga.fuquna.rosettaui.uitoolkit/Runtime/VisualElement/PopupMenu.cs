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
            this.AddManipulator(new PopupMenuManipulator(() => CreateMenuItems?.Invoke()));
        }
    }
}