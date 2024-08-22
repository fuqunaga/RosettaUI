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
        public const string UssClassName = "rosettaui-popup-menu";
        
        public Func<IEnumerable<MenuItem>> CreateMenuItems { get; set; }
        
        public PopupMenu()
        {
            AddToClassList(UssClassName);
            
            this.AddManipulator(new PopupMenuManipulator(() => CreateMenuItems?.Invoke()));
        }
    }
}