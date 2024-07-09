using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Reactive;
using UnityEngine.Pool;

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
            _tabs = tabs?.Where(t => t != null).ToList() ?? new();

            using var pool = ListPool<Element>.Get(out var list);
            foreach (var tab in _tabs)
            {
                list.Add(tab.header);
                list.Add(tab.content);
            }
            
            SetElements(list);
            
            // RosettaUI.UIToolkit.Tabsでもコンテンツの表示/非表示の切り替えは行われるが
            // RosettaUIのElementには伝わらないため表示されてもElement.Update()が呼ばれない
            // Elementにも伝えるため同期されるcurrentTabIndexで監視しておく
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