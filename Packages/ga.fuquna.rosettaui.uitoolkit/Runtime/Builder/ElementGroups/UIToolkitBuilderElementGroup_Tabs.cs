using RosettaUI.Reactive;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Tabs(Element element, VisualElement visualElement)
        {
            if (element is not TabsElement tabsElement || visualElement is not Tabs tabs) return false;

            ReplaceTabs();
            
            var disposable = tabsElement.currentTabIndex.SubscribeAndCallOnce(i => tabs.CurrentTabIndex = i);
            tabs.onCurrentTabIndexChanged += OnTabIndexChanged;

            tabsElement.GetViewBridge().onUnsubscribe += () =>
            {
                disposable.Dispose();
                tabs.onCurrentTabIndexChanged -= OnTabIndexChanged;
            };

            return true;
            

            void ReplaceTabs()
            {
                var tabElements = tabsElement.Tabs;
            
                using var pool0 = ListPool<(VisualElement, VisualElement)>.Get(out var veList);
                veList.AddRange(tabs.TabPairs);
            
                using var pool1 = ListPool<(VisualElement, VisualElement)>.Get(out var veListNext);
            
                for(var i=0; i< tabElements.Count; ++i)
                {
                    var tab = tabElements[i];

                    var needHeaderBuild = true;
                    var needContentBuild = true;
                    var hasVe = (i < veList.Count);
                    if ( hasVe )
                    {
                        var (headerVeCurrent, contentVeCurrent) = veList[i];
                        needHeaderBuild = !Bind(tab.header, headerVeCurrent);
                        needContentBuild = !Bind(tab.content, contentVeCurrent);
                    }

                    var headerVe = needHeaderBuild ? Build(tab.header) : null;
                    var contentVe = needContentBuild ? Build(tab.content) : null;

                    veListNext.Add((headerVe, contentVe));
                }

                tabs.ReplaceTabs(veListNext);
            }

            void OnTabIndexChanged(int i) => tabsElement.CurrentTabIndex = i;
        }
    }
}