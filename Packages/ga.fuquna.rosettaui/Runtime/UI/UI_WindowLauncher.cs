using System;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace RosettaUI
{
    public static partial class UI
    {
        public static WindowLauncherElement WindowLauncher(WindowElement window) => WindowLauncher(null, window);

        public static WindowLauncherElement WindowLauncher(LabelElement title, WindowElement window)
        {
            var label = title ?? window.Header?.FirstLabel()?.Clone();
            return new WindowLauncherElement(label, window);
        }

        public static WindowLauncherElement WindowLauncher<T>(bool supportMultiple = false, bool includeInactive = false, bool pageEnableInWindow = true)
            where T : Object
        {
            return WindowLauncher<T>(null, supportMultiple, includeInactive, pageEnableInWindow);
        }
        
        public static WindowLauncherElement WindowLauncher<T>(LabelElement title, bool supportMultiple = false, bool includeInactive = false, bool pageEnableInWindow = true)
            where T : Object
        {
            return WindowLauncher(title, supportMultiple, includeInactive, pageEnableInWindow, typeof(T));
        }

        public static WindowLauncherElement WindowLauncher(params Type[] types) => WindowLauncher(null, types);
        public static WindowLauncherElement WindowLauncher(bool supportMultiple, bool includeInactive, params Type[] types) 
            => WindowLauncher(null, supportMultiple, includeInactive, true, types);

        public static WindowLauncherElement WindowLauncher(LabelElement title, params Type[] types)
            => WindowLauncher(title, false, false, true, types);
        
        public static WindowLauncherElement WindowLauncher(LabelElement title, bool supportMultiple, bool includeInactive, bool pageEnableInWindow, params Type[] types)
        {
            Assert.IsTrue(types.Any());

            var elements = types.Select(t => FieldIfObjectFound(t, supportMultiple, includeInactive)).ToList();
            title ??= types.First().ToString().Split('.').LastOrDefault();
            var window = pageEnableInWindow
                ? Window(title, Page(elements))
                : Window(title, elements);
            
            var launcher = WindowLauncher(window);
            launcher.UpdateWhileDisabled = true;
            launcher.onUpdate += _ =>
            {
                // window.Enable == false のときは各elementsが更新されないのでCheckAndRebuild()で内容の有無だけ更新する
                if (!window.Enable)
                {
                    foreach(var e in elements) e.CheckAndRebuild();
                }
                
                var windowHasContents = elements.Any(dynamicElement => dynamicElement.Contents.Any());
                launcher.Enable = windowHasContents;
                
                if (!windowHasContents && window.IsOpen)
                {
                    window.Close();
                }
            };
            launcher.onDetachView += (_,_) => window.DetachView();

            return launcher;
        }
    }
}