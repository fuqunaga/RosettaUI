using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Reactive;

namespace RosettaUI
{
    public class TabsElement : ElementGroup
    {
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

            currentTabIndex.SubscribeAndCallOnce(newIndex =>
            {
                for (var i = 0; i < _tabs.Count; ++i)
                {
                    var tab = _tabs[i];
                    tab.content.Enable = (i == newIndex);
                }
            });
        }
    }
    
    public class Tab
    {
        public Element header;
        public Element content;

        public static Tab Create(string name, Element content) => new()
        {
            header = UI.Label(name),
            content = content
        };

        public static Tab Create(string name, Func<Element> build) => Create(name, UI.Lazy(build));
    }
}