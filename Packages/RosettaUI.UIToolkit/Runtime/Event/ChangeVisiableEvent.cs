using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // 動的にエレメントが可視不可視が変更されたイベント
    // Windowのサイズ再計算トリガーとして使う
    public class ChangeVisibleEvent : EventBase<ChangeVisibleEvent>
    {
        protected override void Init()
        {
            base.Init();
            tricklesDown = true;
            bubbles = true;
        }
    }
}