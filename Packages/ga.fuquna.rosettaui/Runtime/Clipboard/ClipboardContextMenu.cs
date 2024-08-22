using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static class ClipboardUtility
    {
        public static Func<IEnumerable<MenuItem>> GenerateContextMenuItemsFunc<T>(Func<T> getter, Action<T> setter)
        {
            return () =>
            {
                var success = Clipboard.TryGet(out T value);

                return new[]
                {
                    new MenuItem("Copy", () => Clipboard.Set(getter())),
                    new MenuItem("Paste", () => setter(value)){ isEnable = success }
                };
            };
        }
    }
}
