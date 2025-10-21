using System;
using RosettaUI.UndoSystem;
using UnityEngine;
using UnityEngine.Assertions;
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

        /// <summary>
        /// persistentSizeKeyでウィンドウサイズを保存・復元するコンストラクタ
        /// </summary>
        protected ModalEditor(string persistentSizeKey, Vector2 defaultSize, string visualTreeAssetPath = "") : this(visualTreeAssetPath,
            true)
        {
            Assert.IsFalse(string.IsNullOrEmpty(persistentSizeKey), $"{nameof(persistentSizeKey)} is null or empty");
            
            if (!PersistentData.TryGet<Vector2>(persistentSizeKey, out var size))
            {
                size = defaultSize;
            }
            
            window.SetSize(size);
            window.onHide += (_) => PersistentData.Set(persistentSizeKey, window.GetSize());
        }
        
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
            window.Add(this);
        }

        protected void Show(Vector2 position, VisualElement target, Action<TValue> onValueChanged, Action<bool> onHide = null)
        {
            onEditorValueChanged += onValueChanged;
            
            UndoHistory.PushHistoryStack(nameof(ModalEditor<TValue>));
            window.onHide += OnHide;
            
            window.RegisterCallbackOnce<DetachFromPanelEvent>(_ =>
            {
                window.onHide -= OnHide;
                onEditorValueChanged -= onValueChanged;
                target?.Focus();
            });

            window.Show(position, target);

            // 画面からはみ出さないように補正する
            window.RegisterCallbackOnce<GeometryChangedEvent>(_ =>
            {
                VisualElementExtensions.ClampPositionToScreen(position, window);
            });

            return;
            
            void OnHide(bool isCancelled)
            {
                // onHideでModalEditorの結果をUndoに残すケースを考慮してonHideの前にPopする
                UndoHistory.PopHistoryStack();
                onHide?.Invoke(isCancelled);
            }
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