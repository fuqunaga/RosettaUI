using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Tabs : VisualElement
    {
        private static readonly string UssClassName = "rosettaui-tabs";
        private static readonly string UssClassNameTitle = UssClassName + "__title";
        private static readonly string UssClassNameTitleActive = UssClassNameTitle + "--active";
        private static readonly string UssClassNameTitleInactive = UssClassNameTitle + "--inactive";
        private static readonly string UssClassNameTitleContainer = UssClassName + "__title-container";
        private static readonly string UssClassNameContentContainer = UssClassName + "__content-container";

        private readonly List<(VisualElement, VisualElement)> _tabs = new();
        private int _currentTabIndex;
        private readonly VisualElement _contentContainer = new();

        public VisualElement TitleContainer { get; } = new();
        public override VisualElement contentContainer => _contentContainer;

        public event Action<int> onCurrentTabIndexChanged;

        public int CurrentTabIndex
        {
            get => _currentTabIndex;
            set
            {
                if (_currentTabIndex != value)
                {
                    _currentTabIndex = value;
                    UpdateTabActive();
                    onCurrentTabIndexChanged?.Invoke(_currentTabIndex);
                }
            }
        }

        public Tabs()
        {
            AddToClassList(UssClassName);

            TitleContainer.AddToClassList(UssClassNameTitleContainer);
            hierarchy.Add(TitleContainer);

            _contentContainer.AddToClassList(UssClassNameContentContainer);
            hierarchy.Add(_contentContainer);
        }

        public void AddTab(VisualElement title, VisualElement content)
        {
            var index = _tabs.Count;
            var titleVe = new Button(() => CurrentTabIndex = index);
            titleVe.ClearClassList();
            titleVe.AddToClassList(UssClassNameTitle);
            titleVe.Add(title);
            
            TitleContainer.Add(titleVe);
            contentContainer.Add(content);

            _tabs.Add((titleVe, content));
            UpdateTabActive();
        }
        
        private void UpdateTabActive()
        {
            for (var i = 0; i < _tabs.Count; ++i)
            {
                var (title, content) = _tabs[i];

                if (i == _currentTabIndex)
                {
                    title.RemoveFromClassList(UssClassNameTitleInactive);
                    title.AddToClassList(UssClassNameTitleActive);
                    content.style.display = DisplayStyle.Flex;
                }
                else
                {
                    title.RemoveFromClassList(UssClassNameTitleActive);
                    title.AddToClassList(UssClassNameTitleInactive);

                    content.style.display = DisplayStyle.None;
                }
            }
        }
    }
}