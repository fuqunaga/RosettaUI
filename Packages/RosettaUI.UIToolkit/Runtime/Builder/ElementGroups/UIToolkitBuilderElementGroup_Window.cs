using System.Linq;
using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Window(Element element, VisualElement visualElement)
        {
            if (element is not WindowElement windowElement || visualElement is not Window window) return false;

            var titleBarLeft = window.TitleBarContainerLeft.Children().FirstOrDefault();
            var bound = titleBarLeft != null && Bind(windowElement.Header, titleBarLeft);
            if (!bound)
            {
                window.TitleBarContainerLeft.Add(Build(windowElement.Header));
            }

            window.CloseButton.clicked += OnCloseButtonClicked;
            
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
                window.CloseButton.clicked -= OnCloseButtonClicked;
                openDisposable.Dispose();
                positionDisposable.Dispose();
            };
            
            return Bind_ElementGroupContents(windowElement, window);

            void OnCloseButtonClicked() =>  windowElement.Enable = !windowElement.Enable;
        }
    }
}