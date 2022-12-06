using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // ChangeVisibleEventを呼ぶFoldout
    public class FoldoutCustom : Foldout
    {
        public Toggle Toggle { get; }
        
        private VisualElement _headerContent;

        public VisualElement HeaderContent
        {
            get => _headerContent;
            set
            {
                if (_headerContent != value)
                {
                    _headerContent?.RemoveFromHierarchy();
                }

                Toggle.Add(value);
                _headerContent = value;
            }
        }

        public FoldoutCustom()
        {
            Toggle = this.Q<Toggle>();
            
            RegisterCallback<ChangeEvent<bool>>(_ =>
            {
                using var changeVisibleEvent = ChangeVisibleEvent.GetPooled();
                changeVisibleEvent.target = contentContainer;
                SendEvent(changeVisibleEvent);
            });
        }
    }
}