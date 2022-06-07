using System.Collections.Generic;
using System.Linq;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public class TabsElement : ElementGroup
    {
        public class Tab
        {
            public Element header;
            public Element content;
        }
        
        public readonly ReactiveProperty<int> currentTabIndex = new();
        private readonly List<Tab> _tabs;
        
        public int CurrentTabIndex {
            get => currentTabIndex.Value;
            set => currentTabIndex.Value = value;
        }

        public IReadOnlyList<Tab> Tabs => _tabs;

        public TabsElement(IEnumerable<Tab> tabs)
        {
            _tabs = tabs?.ToList() ?? new();
            SetElements(_tabs.SelectMany(t => new[] {t.header, t.content}));
        }
    }
}