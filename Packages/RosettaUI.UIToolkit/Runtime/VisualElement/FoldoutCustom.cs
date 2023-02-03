using RosettaUI.UIToolkit.UnityInternalAccess;
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
            
            // disable 中でもクリック可能
            UIToolkitUtility.SetAcceptClicksIfDisabled(Toggle);
            
            // Foldout 直下の Toggle は marginLeft が default.uss で書き換わるので上書きしておく
            // セレクタ例： .unity-foldout--depth-1 > .unity-fold__toggle
            Toggle.style.marginLeft = 0;
            
            RegisterCallback<ChangeEvent<bool>>(_ =>
            {
                using var requestResizeWindowEvent = RequestResizeWindowEvent.GetPooled();
                requestResizeWindowEvent.target = contentContainer;
                SendEvent(requestResizeWindowEvent);
            });
        }
    }
}