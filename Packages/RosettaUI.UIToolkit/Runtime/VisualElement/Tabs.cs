using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class Tabs : VisualElement
    {
        private static readonly string UssClassName = "rosettaui-tabs";
        private static readonly string UssClassNameTitle = UssClassName + "__title";
        private static readonly string UssClassNameTitleSpacer = UssClassName + "__title-spacer";
        private static readonly string UssClassNameTitleActive = UssClassNameTitle + "--active";
        private static readonly string UssClassNameTitleInactive = UssClassNameTitle + "--inactive";
        private static readonly string UssClassNameTitleContainer = UssClassName + "__title-container";
        private static readonly string UssClassNameContentContainer = UssClassName + "__content-container";

        private readonly List<(VisualElement headerButton, VisualElement content)> _tabs = new();
        private int _currentTabIndex;
        private readonly VisualElement _contentContainer = new();

        public IEnumerable<(VisualElement header, VisualElement content)> TabPairs =>
            _tabs.Select(pair => (pair.headerButton.Children().First(), pair.content));
        public VisualElement TitleContainer { get; } = new();
        public override VisualElement contentContainer => _contentContainer;

        public event Action<int> onCurrentTabIndexChanged;

        public int CurrentTabIndex
        {
            get => _currentTabIndex;
            set
            {
                if (_currentTabIndex == value) return;

                _currentTabIndex = Mathf.Min(Mathf.Max(value, 0), _tabs.Count - 1);
                UpdateTabActive();
                onCurrentTabIndexChanged?.Invoke(_currentTabIndex);
                
                using var changeVisibleEvent = RequestResizeWindowEvent.GetPooled();
                changeVisibleEvent.target = this;
                SendEvent(changeVisibleEvent);
            }
        }

        public Tabs()
        {
            AddToClassList(UssClassName);

            // アクティブなtitleとcontentContainerは親のbackgroundカラーにしたい
            // titleContainerは親のbackgroundカラーより暗くしたい
            // titleContainerでbackgroundカラーを指定するとアクティブなtitleを親のbackgroundカラーにできない
            // したがって、titleContainer内の右端にtitleSpacerを入れておき、
            // 非アクティブなtitleとtitleSpacerを暗くして、擬似的にtitleContainerが暗くなっているように見せる
            var titleSpacer = new VisualElement();
            titleSpacer.AddToClassList(UssClassNameTitleSpacer);
            
            TitleContainer.AddToClassList(UssClassNameTitleContainer);
            TitleContainer.Add(titleSpacer);
            hierarchy.Add(TitleContainer);

            _contentContainer.AddToClassList(UssClassNameContentContainer);
            hierarchy.Add(_contentContainer);
        }
        
        public void AddTabs(IEnumerable<(VisualElement header, VisualElement content)> tabs)
        {
            foreach (var (header, content) in tabs)
            {
                DoAddTab(header, content);
            }

            UpdateTabActive();
        }

        public void AddTab(VisualElement header, VisualElement content)
        {
            DoAddTab(header, content);
            UpdateTabActive();
        }
        
        private void DoAddTab(VisualElement header, VisualElement content)
        {
            var index = _tabs.Count;
            var titleVe = new Button(() => CurrentTabIndex = index)
            {
                tabIndex = -1
            };
            titleVe.ClearClassList();
            titleVe.AddToClassList(UssClassNameTitle);
            titleVe.Add(header);
            
            TitleContainer.Insert(index, titleVe);
            contentContainer.Add(content);

            _tabs.Add((titleVe, content));
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

                    if (content != null)
                    {
                        content.style.display = DisplayStyle.Flex;
                    }
                }
                else
                {
                    title.RemoveFromClassList(UssClassNameTitleActive);
                    title.AddToClassList(UssClassNameTitleInactive);

                    if (content != null)
                    {
                        content.style.display = DisplayStyle.None;
                    }
                }
            }
        }

        public void ReplaceTabs(List<(VisualElement header, VisualElement content)> nextTabs)
        {
            var nextCount = nextTabs.Count;
            
            // 削除するタブ
            for (var i = _tabs.Count - 1; i >= nextCount; --i)
            {
                var tab = _tabs[i];
                tab.headerButton.RemoveFromHierarchy();
                tab.content.RemoveFromHierarchy();
                
                _tabs.RemoveAt(i);
            }
            
            var currentCount = _tabs.Count;
            
            // currentにもnextに存在するタブ
            for (var i = 0; i < currentCount; ++i)
            {
                var (headerButton, content) = _tabs[i];
                var header = headerButton.Children().First();

                var (headerNext, contentNext) = nextTabs[i];

                if (headerNext != null && header != headerNext)
                {
                    header.RemoveFromHierarchy();
                    headerButton.Add(headerNext);
                }

                if (contentNext != null && content != contentNext)
                {
                    var index = IndexOf(content);
                    content.RemoveFromHierarchy();
                    Insert(index, contentNext);
                    _tabs[i] = (headerButton, content);
                }
            }

            // 追加するタブ
            for (var i = currentCount; i < nextCount; ++i)
            {
                var (headerNext, contentNext) = nextTabs[i];
                DoAddTab(headerNext, contentNext);
            }
            
            UpdateTabActive();
        }
    }
}