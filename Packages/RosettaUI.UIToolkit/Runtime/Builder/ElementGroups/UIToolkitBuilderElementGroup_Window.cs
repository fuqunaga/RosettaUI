using System.Linq;
using RosettaUI.Reactive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Window(Element element, VisualElement visualElement)
        {
            if (element is not WindowElement windowElement || visualElement is not Window window) return false;

            var titleBarLeft = window.TitleBarContainerLeft.Children().FirstOrDefault();
            var successBind = titleBarLeft != null && Bind(windowElement.Header, titleBarLeft);
            if (!successBind)
            {
                window.TitleBarContainerLeft.Add(Build(windowElement.Header));
            }
            
            window.Closable = windowElement.Closable;

            window.onShow += OnShow;
            window.onHide += OnHide;
            
            var openDisposable = windowElement.IsOpenRx.SubscribeAndCallOnce(isOpen =>
            {
                if (isOpen)
                {
                    window.BringToFront();
                    window.Show();
                }
                else
                {
                    window.Hide();
                }
            });

            var positionDisposable = windowElement.positionRx.SubscribeAndCallOnce(posNullable =>
            {
                if (posNullable is { } pos)
                {
                    window.Position = pos;
                }
            });

            windowElement.GetViewBridge().onUnsubscribe += () =>
            {
                window.onShow -= OnShow;
                window.onHide -= OnHide;

                openDisposable.Dispose();
                positionDisposable.Dispose();
            };
            
            return Bind_ElementGroupContents(windowElement, window);


            void OnShow() => windowElement.Enable = true;
            void OnHide() => windowElement.Enable = false;
        }
    }
}