using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Tabs(Element element)
        {
            var tabsElement = (TabsElement) element;
            var tabs = new Tabs();

            foreach (var tab in tabsElement.Tabs)
            {
                var header = BuildInternal(tab.header);
                var content = BuildInternal(tab.content);

                tabs.AddTab(header, content);
            }

            tabsElement.currentTabIndex.SubscribeAndCallOnce(i => tabs.CurrentTabIndex = i);
            tabs.onCurrentTabIndexChanged += i => tabsElement.CurrentTabIndex = i;
            
            return tabs;
        }
    }
}