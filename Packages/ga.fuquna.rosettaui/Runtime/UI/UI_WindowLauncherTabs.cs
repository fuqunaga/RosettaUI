using System;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element WindowLauncherTabs(LabelElement label, params Type[] types)
        {
            return WindowLauncherTabs(label, types.Select(type => (type.ToString().Split(".").Last(), new[] { type })));
        }
        
        public static Element WindowLauncherTabs(LabelElement label, bool supportMultiple, bool includeInactive,params Type[] types)
        {
            return WindowLauncherTabs(label, supportMultiple, includeInactive, types.Select(type => (type.ToString().Split(".").Last(), new[] { type })));
        }

        public static Element WindowLauncherTabs(LabelElement label, IEnumerable<(string label, Type[] types)> tabs)
        {
            return WindowLauncherTabs(label, false, false, tabs);
        }
        
        public static Element WindowLauncherTabs(LabelElement label, bool supportMultiple, bool includeInactive, IEnumerable<(string label, Type[] types)> tabs)
        {
            return WindowLauncher(Window(label, Box(
                Tabs(
                    tabs.Select(pair => Tab.Create(
                            pair.label,
                            () => Page(pair.types.Select(type => FieldIfObjectFound(type, supportMultiple, includeInactive)))
                        )
                    )
                )
            )));
        }
    }
}