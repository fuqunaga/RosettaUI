using NUnit.Framework;
using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Fold(Element element, VisualElement visualElement)
        {
            if (element is not FoldElement foldElement || visualElement is not FoldoutCustom fold) return false;

            var headerElement = foldElement.Header;
            if (headerElement == null)
            {
                fold.HeaderContent = null;
            }
            else
            {
                var success = Bind(headerElement, fold.HeaderContent);
                if (!success)
                {
                    fold.HeaderContent = Build(headerElement);
                }
            }

            // Indentがあるなら１レベルキャンセル
            ApplyMinusIndentIfPossible(fold, foldElement);

            // 初回のElementの値セットではNotifyしない
            // NotifyするとFoldoutCustomでChangeVisibleEventを飛ばしWindowサイズの再計算を促すが
            // ListViewItemContainer内でBindされたときに飛ばすと、
            // ListViewItemContainerスクロール中にWindowサイズが変わってしまい見栄えがよくない
            // スクロール中はWindowサイズを替えたくない→ChangeVisibleEventは飛ばさない→Notifyしない
            fold.SetValueWithoutNotify(foldElement.IsOpen);
            
            // 処理負荷軽減のため初回OpenまではBind（≒Build）しない
            var firstOpen = true;
            BindContentsIfNeedWhenFirstOpen();
            
            // 通常時の値の相互通知
            var openDisposable = foldElement.IsOpenRx.Subscribe(isOpen => fold.value = isOpen);
            fold.RegisterValueChangedCallback(OnFoldValueChanged);
            
            foldElement.GetViewBridge().onUnsubscribe += () =>
            {
                openDisposable.Dispose();
                fold.UnregisterValueChangedCallback(OnFoldValueChanged);
            };

            return true;


            void OnFoldValueChanged(ChangeEvent<bool> evt)
            {
                if (evt.target != fold) return;

                foldElement.IsOpen = evt.newValue;
                BindContentsIfNeedWhenFirstOpen();
            }
            
            void BindContentsIfNeedWhenFirstOpen()
            {
                // ReSharper disable once AccessToModifiedClosure
                if (!firstOpen || !foldElement.IsOpen) return;

                firstOpen = false;
                var success = Bind_ElementGroupContents(foldElement, fold);
                Assert.IsTrue(success);
            }
        }
    }
}