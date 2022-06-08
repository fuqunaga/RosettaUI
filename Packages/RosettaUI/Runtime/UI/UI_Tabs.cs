using System;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static TabsElement Tabs(params (string, Element)[] tabs) => Tabs(tabs.AsEnumerable());

        public static TabsElement Tabs(IEnumerable<(string, Element)> tabs)
        {
            return Tabs(tabs.Select(tab => Tab.Create(tab.Item1, tab.Item2)));
        }

        // Func<Element> call is delayed until displayed 
        public static TabsElement Tabs(params (string, Func<Element>)[] tabs)
        {
            return Tabs(tabs.Select(tab => Tab.Create(tab.Item1, tab.Item2)));
        }

        public static TabsElement Tabs(IEnumerable<Tab> tabs) => new(tabs);
    }
}