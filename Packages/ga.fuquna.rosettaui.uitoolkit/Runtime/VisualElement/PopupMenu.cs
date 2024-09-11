using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// マウスクリックでポップアップメニューを出す空のVisualElement
    /// 見た目などの設定は子供に別のVisualElementを追加する
    /// </summary>
    public class PopupMenu : VisualElement
    {
        public const string UssClassName = "rosettaui-popup-menu";
        
        private readonly PopupMenuManipulator _manipulator;
        
        public Func<IEnumerable<MenuItem>> CreateMenuItems { get; set; }

        public MouseButton MouseButton
        {
            set => _manipulator.Button = value;
        }
        
        public PopupMenu()
        {
            AddToClassList(UssClassName);

            _manipulator = new PopupMenuManipulator(() => CreateMenuItems?.Invoke());
            this.AddManipulator(_manipulator);
        }
    }
}