using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement) element;
            var window = new Window();
            window.TitleBarContainerLeft.Add(Build(windowElement.Header));
            window.CloseButton.clicked += () => windowElement.Enable = !windowElement.Enable;

            windowElement.IsOpenRx.SubscribeAndCallOnce(isOpen =>
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

            windowElement.positionRx.SubscribeAndCallOnce(posNullable =>
            {
                if (posNullable is { } pos)
                {
                    window.Position = pos;
                }
            });


            // Focusable.ExecuteDefaultEvent() 内の this.focusController?.SwitchFocusOnEvent(evt) で
            // NavigationMoveEvent 方向にフォーカスを移動しようとする
            // キー入力をしている場合などにフォーカスが移ってしまうのは避けたいのでWindow単位で抑制しておく
            // UnityデフォルトでもTextFieldは抑制できているが、IntegerField.inputFieldでは出来ていないなど挙動に一貫性がない
            window.RegisterCallback<NavigationMoveEvent>(evt => evt.PreventDefault());

            Build_ElementGroupContents(window.contentContainer, element);

            window.AddBoxShadow();

            return window;
        }
    }
}