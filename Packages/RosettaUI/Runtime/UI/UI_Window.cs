using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace RosettaUI
{
    public static partial class UI
    {
        #region Window

        public static WindowElement Window(params Element[] elements) => Window(null, Indent(elements));

        public static WindowElement Window(LabelElement title, params Element[] elements) => new(title, elements.AsEnumerable());

        public static WindowElement Window(LabelElement title, IEnumerable<Element> elements) => new(title, new[]{Indent(elements)});

        #endregion
        
        
        #region Window Launcher

        public static WindowLauncherElement WindowLauncher(WindowElement window)
        {
            return WindowLauncher(null, window);
        }

        public static WindowLauncherElement WindowLauncher(LabelElement title, WindowElement window)
        {
            var label = title ?? window.bar?.FirstLabel()?.Clone();
            return new WindowLauncherElement(label, window);
        }

        public static WindowLauncherElement WindowLauncher<T>(LabelElement title = null)
            where T : Object
        {
            return WindowLauncher(title, typeof(T));
        }

        public static WindowLauncherElement WindowLauncher(params Type[] types) => WindowLauncher(null, types);

        public static WindowLauncherElement WindowLauncher(LabelElement title, params Type[] types)
        {
            Assert.IsTrue(types.Any());

            var elements = types.Select(FieldIfObjectFound).ToList();
            title ??= types.First().ToString().Split('.').LastOrDefault();
            var window = Window(title, elements);
            window.UpdateWhenDisabled = true;
            
            var launcher = WindowLauncher(window);
            launcher.UpdateWhenDisabled = true;
            launcher.onUpdate += _ =>
            {
                var windowHasContents = elements.Any(dynamicElement => dynamicElement.Contents.Any());
                launcher.Enable = windowHasContents;
            };
            launcher.onDestroy += _ => window.Destroy();

            return launcher;
        }

        #endregion


    }
}