using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // Windowのサイズ再計算のリクエスト
    // 動的にエレメントが可視不可視が変更された際など
    public class RequestResizeWindowEvent : EventBase<RequestResizeWindowEvent>
    {
        public static void Send(VisualElement visualElement)
        {
            using var requestResizeWindowEvent = GetPooled();
            requestResizeWindowEvent.target = visualElement;
            visualElement.SendEvent(requestResizeWindowEvent);
        }
        
        protected override void Init()
        {
            base.Init();
            tricklesDown = true;
            bubbles = true;
        }
    }
}