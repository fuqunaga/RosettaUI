using System;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Reactive;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        static void ApplyMinusIndentIfPossible(VisualElement ve, Element element)
        {
            // Indentがあるなら１レベルキャンセル
            if (element.CanMinusIndent())
            {
                ve.style.marginLeft = -LayoutSettings.IndentSize;
            }
        }

        static void ApplyIndent(VisualElement ve, int indentLevel = 1)
        {
            ve.style.marginLeft = LayoutSettings.IndentSize * indentLevel;
        }

        private static void SetupOpenCloseBaseElement(Foldout fold, OpenCloseBaseElement foldElement)
        {
            var toggle = fold.Q<Toggle>();
            toggle.Add(Build(foldElement.Header));

            // disable 中でもクリック可能
            UIToolkitUtility.SetAcceptClicksIfDisabled(toggle);

            // Foldout 直下の Toggle は marginLeft が default.uss で書き換わるので上書きしておく
            // セレクタ例： .unity-foldout--depth-1 > .unity-fold__toggle
            toggle.style.marginLeft = 0;

            // Indentがあるなら１レベルキャンセル
            ApplyMinusIndentIfPossible(fold, foldElement);

            foldElement.IsOpenRx.SubscribeAndCallOnce(isOpen => fold.value = isOpen);
            fold.RegisterValueChangedCallback(evt =>
            {
                if (evt.target == fold)
                {
                    foldElement.IsOpen = evt.newValue;
                }
            });
        }

        private VisualElement Build_ElementGroupContents(VisualElement container, Element element,
            Action<VisualElement, int> setupContentsVe = null)
        {
            var group = (ElementGroup) element;

            container.name = group.DisplayName;

            var i = 0;
            foreach (var ve in Build_ElementGroupContents(group))
            {
                setupContentsVe?.Invoke(ve, i);
                container.Add(ve);
                i++;
            }

            return container;
        }

        // ElementGroupのBind
        // visualElementの子供にBindできない場合はBuildで生成する
        private bool Bind_ElementGroupContents(ElementGroup elementGroup, VisualElement visualElement,
            Action<Element, VisualElement, int> bindChild = null)
        {
            var contentCount = elementGroup.Contents.Count();
            var visualElementCount = visualElement.childCount;

            for (var i = contentCount; i < visualElementCount; ++i)
            {
                visualElement.RemoveAt(i);
            }

            using var pool = ListPool<VisualElement>.Get(out var veList);
            veList.AddRange(visualElement.Children());

            foreach (var (e,idx) in elementGroup.Contents.Select((e,idx) => (e,idx)))
            {
                if (idx < veList.Count)
                {
                    var ve = veList[idx];
                    var success = Bind(e, ve);
                    if (success) continue;

                    visualElement.RemoveAt(idx);
                }

                var newVe = Build(e);
                Assert.IsNotNull(newVe);
                
                visualElement.Insert(idx, newVe);
            }

            return true;
        }
    }
}