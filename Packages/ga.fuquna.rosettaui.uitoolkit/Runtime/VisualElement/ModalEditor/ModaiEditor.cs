using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// ColorPicker,GradientEditor, AnimationCurveEditorなど他の操作をさせないパラメータエディター
    /// </summary>
    public class ModalEditor<TValue> : VisualElement
    {
        protected readonly ModalWindow window;
        public event Action<TValue> onEditorValueChanged;

        protected ModalEditor()
        {
            window = new ModalWindow();
            window.RegisterCallback<NavigationSubmitEvent>(_ => window?.Hide());
        }

        protected void Show(Vector2 position, VisualElement target, Action<TValue> onValueChanged, Action onCancel = null)
        {
            onEditorValueChanged += onValueChanged;
            
            window.Add(this);

            window.onCancel += onCancel;
            window.RegisterCallbackOnce<DetachFromPanelEvent>(_ =>
            {
                window.onCancel -= onCancel;
                onEditorValueChanged -= onValueChanged;
                target?.Focus();
            });

            window.Show(position, target);

            // 画面からはみ出さないように補正する
            window.RegisterCallbackOnce<GeometryChangedEvent>(_ =>
            {
                VisualElementExtension.CheckOutOfScreen(position, window);
            });
        }

        protected void NotifyEditorValueChanged(TValue value)
        {
            onEditorValueChanged?.Invoke(value);
        }
    }
}