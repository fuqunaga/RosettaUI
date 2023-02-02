using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // Windowのサイズ再計算のリクエスト
    // 動的にエレメントが可視不可視が変更された際など
    public class RequestResizeWindowEvent : EventBase<RequestResizeWindowEvent>
    {
        protected override void Init()
        {
            base.Init();
            tricklesDown = true;
            bubbles = true;
        }
    }
}