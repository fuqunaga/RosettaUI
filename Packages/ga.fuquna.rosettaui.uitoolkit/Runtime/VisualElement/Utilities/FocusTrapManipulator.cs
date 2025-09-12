using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// AddしたVisualElementの中にフォーカスを留めるマニュピレーター
    /// </summary>
    public class FocusTrapManipulator : Manipulator
    {
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }
        
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Tab) return;
            
            using var _ = ListPool<Focusable>.Get(out var focusableList);
            focusableList.AddRange(
                target.Query<VisualElement>()
                    .Where(ve => ve.canGrabFocus && (ve.parent == null || !ve.parent.canGrabFocus))
                    .Build()
            );
            
            var count = focusableList.Count;
            if (count == 0)
            {
                return;
            }

            var focused = target.panel.focusController.focusedElement;
            
            var currentIndex = focusableList.IndexOf(focused);
            
            // target外にフォーカスがある場合は無視
            if (focused != null && currentIndex < 0)
            {
                return;
            }
            
            var index = (focused == null)
                ? (evt.shiftKey ? focusableList.Count - 1 : 0)
                : (evt.shiftKey ? currentIndex - 1 : currentIndex + 1);

            index  = (index + count) % count;
            
            focusableList[index].Focus();
            evt.StopPropagation();
        }
    }
}