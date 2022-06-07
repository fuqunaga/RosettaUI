using System.Linq;

namespace RosettaUI
{
    public static partial class UI
    {
        public static TabsElement Tabs(params (string, Element)[] tabs)
        {
            return new TabsElement(tabs.Select(pair =>
            {
                var (title, content) = pair;

                return new TabsElement.Tab()
                {
                    header = Label(title),
                    content = content
                };
            }));
        }
    }
}