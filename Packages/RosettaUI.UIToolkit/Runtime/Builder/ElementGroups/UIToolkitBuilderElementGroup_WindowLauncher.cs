using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_WindowLauncher(Element element, VisualElement visualElement)
        {
            if (element is not WindowLauncherElement windowLauncherElement 
                || visualElement is not WindowLauncher windowLauncher) return false;

            Bind_Field(windowLauncherElement, windowLauncher, false);
            Bind_ExistingLabel(windowLauncherElement.Label, null, str => windowLauncher.text = str);
            ApplyMinusIndentIfPossible(windowLauncher, element);
            
            var windowElement = windowLauncherElement.window;
            var window = (Window)(GetUIObj(windowElement) ?? Build(windowElement));
            
            windowLauncher.RegisterCallback<PointerUpEvent>(OnPointUpEvent);
            windowLauncherElement.GetViewBridge().onUnsubscribe += () =>
            {
                windowLauncher.UnregisterCallback<PointerUpEvent>(OnPointUpEvent);
            };
            
            return true;
            
            // ほかのWindowにかぶらない位置を計算する
            // 一度ドラッグしたWindowはその位置を覚えてこの処理の対象にはならない
            void OnPointUpEvent(PointerUpEvent evt)
            {
                // Toggleの値が変わるのはこのイベントの後
                // これから閉じるならリターン
                if (windowElement.Enable) return;
                
                // positionが指定されていたらそちらを優先
                if (windowElement.positionRx.Value.HasValue)
                {
                    window.Show(windowLauncher);
                    return;
                }
                
                var screenRect = GetUIObj(windowLauncherElement)?.panel.visualTree.layout;
                Assert.IsTrue(screenRect.HasValue);
                
                var parentWindowElement = windowLauncherElement.Parents().OfType<WindowElement>().FirstOrDefault();
                
                var pos = WindowLayout.CalcOpenWindowPosition(
                    evt.originalMousePosition,
                    screenRect.Value,
                    windowElement,
                    parentWindowElement,
                    getWindowRect: we => GetUIObj(we).layout,
                    isWindowMoved: we => GetUIObj(we) is Window { IsMoved: true }
                );

                window.Show(pos, windowLauncher);
            }
        }
    }
}