using System.Linq;
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

            var vePairs = tabsElement.Tabs.Select(tab =>
                (header: BuildInternal(tab.header),
                    content: BuildInternal(tab.content)
                )
            );

            tabs.AddTabs(vePairs);
            
            tabsElement.currentTabIndex.SubscribeAndCallOnce(i => tabs.CurrentTabIndex = i);
            tabs.onCurrentTabIndexChanged += i => tabsElement.CurrentTabIndex = i;
            
            return tabs;
        }
    }
}