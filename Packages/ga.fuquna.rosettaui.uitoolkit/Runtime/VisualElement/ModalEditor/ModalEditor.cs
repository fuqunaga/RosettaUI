using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// ColorPicker,GradientEditor, AnimationCurveEditorなど他の操作をさせないパラメータエディター
    /// </summary>
    public abstract class ModalEditor<TValue> : VisualElement
    {
        protected readonly ModalWindow window;
        public event Action<TValue> onEditorValueChanged;

        protected ModalEditor(string visualTreeAssetPath = "", bool resizable = false)
        {
            if (!string.IsNullOrEmpty(visualTreeAssetPath))
            {
                var visualTree = Resources.Load<VisualTreeAsset>(visualTreeAssetPath);
                if (visualTree != null)
                {
                    visualTree.CloneTree(this);
                }
                else
                {
                    Debug.LogError($"Visual tree asset not found: {visualTreeAssetPath}");
                }
            }

            window = new ModalWindow(resizable);
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
                VisualElementExtensions.ClampPositionToScreen(position, window);
            });
        }

        protected virtual void NotifyEditorValueChanged()
        {
            onEditorValueChanged?.Invoke(CopiedValue);
        }


        // TValueがクラスの場合、同じTValueインスタンスを外部で変更されるとまずい場合がある
        // SetInitialValue()とGetValueForNotification()の実装を強制してTValueをCloneすべきかどうか
        protected abstract TValue CopiedValue { get; set; }
    }
}