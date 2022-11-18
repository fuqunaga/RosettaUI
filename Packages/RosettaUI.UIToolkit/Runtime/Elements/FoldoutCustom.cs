using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // ChangeVisibleEventを呼ぶFoldout
    public class FoldoutCustom : Foldout
    {
        public FoldoutCustom()
        {
            RegisterCallback<ChangeEvent<bool>>(_ =>
            {
                using var changeVisibleEvent = ChangeVisibleEvent.GetPooled();
                changeVisibleEvent.target = contentContainer;
                SendEvent(changeVisibleEvent);
            });
        }
    }
}