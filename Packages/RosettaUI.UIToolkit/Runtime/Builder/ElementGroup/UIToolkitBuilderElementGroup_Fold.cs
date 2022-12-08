using RosettaUI.Reactive;
using RosettaUI.UIToolkit.UnityInternalAccess;
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

            var openDisposable = foldElement.IsOpenRx.SubscribeAndCallOnce(isOpen => fold.value = isOpen);
            fold.RegisterValueChangedCallback(OnFoldValueChanged);
            
            foldElement.GetViewBridge().onUnsubscribe += () =>
            {
                openDisposable.Dispose();
                fold.UnregisterValueChangedCallback(OnFoldValueChanged);
            };

            return Bind_ElementGroupContents(foldElement, fold);
            
            void OnFoldValueChanged(ChangeEvent<bool> evt)
            {
                if (evt.target == fold)
                {
                    foldElement.IsOpen = evt.newValue;
                }
            }
        }
    }
}