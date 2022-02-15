using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class PointerDrag
    {
        public static void RegisterCallback(VisualElement target, EventCallback<PointerMoveEvent> onPointerMoveOnPanel, Func<PointerDownEvent, bool> checkPointerIsValid = null)
        {
            VisualElement root; 
            
            target.RegisterCallback<PointerDownEvent>((evt) =>
            {
                if (checkPointerIsValid?.Invoke(evt) ?? true)
                {
                    root = target.panel.visualTree;
                    root.RegisterCallback<PointerMoveEvent>(onPointerMoveOnPanel);
                    root.RegisterCallback<PointerUpEvent>(OnPointerUpOnPanel);

                    evt.StopPropagation();
                }
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