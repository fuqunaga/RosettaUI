using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// AddしたVisualElementの中にフォーカスをループして留めるマニュピレーター
    /// </summary>
    public class FocusTrapManipulator : Manipulator
    {
        private VisualElementFocusRing _focusRing;
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<FocusOutEvent>(OnFocusOut);
        }
        
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            var direction = evt.direction;

            if (direction != VisualElementFocusChangeDirection.right &&
                direction != VisualElementFocusChangeDirection.left)
            {
                return;
            }
            
            // 自身の外へのフォーカスのみ対応
            if (evt.relatedTarget is VisualElement nextFocusTarget && IsChild(nextFocusTarget))
            {
                return;
            }
                
            _focusRing ??= new VisualElementFocusRing(target);

            // GetNextFocusable()でcurrentFocusableをnullにすると、最初or最後の要素が返る実装を当て込んでいる
            var focusTarget = _focusRing.GetNextFocusable(null, evt.direction);
            if (focusTarget == null)
            {
                return;
            }
                
            target.schedule.Execute(()=> focusTarget.Focus());
            evt.StopPropagation();

            return;

            bool IsChild(VisualElement child)
            {
                while (child != null)
                {
                    if (child == target) return true;
                    child = child.parent;
                }

                return false;
            }
        }
   }
}