using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class PointerDrag
    {
        public static void RegisterCallback(VisualElement target, EventCallback<PointerMoveEvent> onPointerMoveOnPanel)
        {
            VisualElement root; 
            
            target.RegisterCallback<PointerDownEvent>((evt) =>
            {
                root = target.panel.visualTree;
                root.RegisterCallback<PointerMoveEvent>(onPointerMoveOnPanel);
                root.RegisterCallback<PointerUpEvent>(OnPointerUpOnPanel);

                evt.StopPropagation();
            });

            void OnPointerUpOnPanel(PointerUpEvent evt)
            {
                root.UnregisterCallback<PointerMoveEvent>(onPointerMoveOnPanel);
                root.UnregisterCallback<PointerUpEvent>(OnPointerUpOnPanel);
                
                evt.StopPropagation();
            }
        }
    }
}