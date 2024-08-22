using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    // RequestResizeWindowEventを呼ぶFoldout
    public class FoldoutCustom : Foldout
    {
        public const string UssClassName = "rosettaui-foldout-custom";
        public const string HeaderContentUssClassName = UssClassName + "__header-content";
        
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
                _headerContent.AddToClassList(HeaderContentUssClassName);
                
                //　フィールドの値決定のエンターキーにToggleが反応してしまうので抑制
                _headerContent?.RegisterCallback<NavigationSubmitEvent>(evt =>
                {
                    evt.StopPropagation();
                }, TrickleDown.TrickleDown); // Unity2022だとTrickleDownが必要。2023では不要
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
            
            Toggle.RegisterCallback<ChangeEvent<bool>>(_ =>
            {
                using var requestResizeWindowEvent = RequestResizeWindowEvent.GetPooled();
                requestResizeWindowEvent.target = contentContainer;
                SendEvent(requestResizeWindowEvent);
            });
        }
    }
}